import torch
from ultralytics import YOLO
import cv2
import logging
import pyodbc
import uuid
from datetime import datetime
import numpy as np
from sort import Sort
import time

# Configure logging
logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")

# Database connection settings
DB_CONNECTION_STRING = (
    "Driver={ODBC Driver 17 for SQL Server};"
    "Server=tcp:dbbeesafe.database.windows.net,1433;"
    "Database=beesafe;"
    "Uid=xxxx;"  
    "Pwd=xxxx;"  
    "Encrypt=yes;"
    "TrustServerCertificate=no;"
    "Connection Timeout=30;"
)

# Device ID (Only 1 device in use for now)
DEVICE_ID = "4b9a002b-3725-4b74-b034-43e98bb52520"
#KNOWN_HORNET_ID = "958659d4-bc71-4432-8ba7-fce7a47b0f94"


# Dictionary to track the last time a detection was saved for each hornet
last_saved_time = {}  # Format: {track_id: last_saved_timestamp}
SAVE_DELAY = 3  # Minimum delay in seconds between database inserts per hornet ID

# Load YOLO model
model = YOLO("best.pt")

# Initialize SORT tracker
tracker = Sort(max_age=40, min_hits=3, iou_threshold=0.25)

# Open video file
video_path = "test_video.mp4"
cap = cv2.VideoCapture(video_path)
frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))

# Dictionary to track hornet first detections
first_detections = {}


def get_known_hornet_id():
    """Fetch an existing KnownHornetId from the KnownHornet table."""
    try:
        conn = pyodbc.connect(DB_CONNECTION_STRING)
        cursor = conn.cursor()

        # Fetch an existing KnownHornetId (modify this query if needed)
        cursor.execute("SELECT TOP 1 Id FROM dbo.KnownHornet ORDER BY NEWID();")
        row = cursor.fetchone()

        if row:
            known_hornet_id = row[0]
            logging.info(f"Retrieved KnownHornetId: {known_hornet_id}")
            return known_hornet_id  # Return a valid ID
        else:
            logging.error("No KnownHornetId found in the database.")
            return None
    except Exception as e:
        logging.error(f"Database error while fetching KnownHornetId: {e}")
        return None
    finally:
        if 'conn' in locals():
            conn.close()



def save_to_database(timestamp, first_detection, second_detection, direction, hornet_count, track_id):
    """Save detection event to database with a valid KnownHornetId."""

    global last_saved_time

    try:
        known_hornet_id = get_known_hornet_id()  # Always fetch from KnownHornet table

        if not known_hornet_id:
            logging.error("No valid KnownHornetId found. Skipping database insert.")
            return  # Stop execution if no valid ID


         # Check if enough time has passed since the last save
        last_time = last_saved_time.get(track_id, 0)
        current_time = time.time()

        if current_time - last_time < SAVE_DELAY:
            logging.info(f"Skipping database insert for Hornet ID {track_id}, delay not met.")
            return  # Skip this insert

        conn = pyodbc.connect(DB_CONNECTION_STRING)
        cursor = conn.cursor()

        detection_id = str(uuid.uuid4())

        query = '''
        INSERT INTO dbo.DetectionEvent (
            Id, Timestamp, HornetDirection, FirstDetection, SecondDetection, 
            IsManual, HornetCount, DeviceId, KnownHornetId
        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        '''

        cursor.execute(query, (
            detection_id, timestamp, direction, first_detection, second_detection,
            False, hornet_count, DEVICE_ID, known_hornet_id
        ))
        conn.commit()

        # Update last saved time
        last_saved_time[track_id] = current_time

        logging.info(f"Detection saved: ID={detection_id}, HornetCount={hornet_count}, Direction={direction:.2f}")
    except Exception as e:
        logging.error(f"Database error: {e}")
        if 'conn' in locals():
            conn.rollback()
    finally:
        if 'conn' in locals():
            conn.close()



# Process video
while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break
    
    timestamp = datetime.now()
    results = model(frame)
    detections = []
    
    for r in results:
        for box in r.boxes:
            x1, y1, x2, y2 = box.xyxy[0].tolist()
            conf = box.conf[0].item()
            if conf > 0.25:
                detections.append([x1, y1, x2, y2, conf])
    
    detections = np.array(detections)
    if detections.shape[0] == 0:
        detections = np.empty((0, 5))
    
    # Update SORT tracker
    tracked_objects = tracker.update(detections)
    hornet_count = len(tracked_objects)
    
    for obj in tracked_objects:
        x1, y1, x2, y2, track_id = map(int, obj)
        x_center = (x1 + x2) / 2
        normalized_x = x_center / frame_width
        hornet_direction = float(normalized_x * 180.0)
        
        # Check if hornet was detected before
        if track_id not in first_detections:
            first_detections[track_id] = timestamp
            second_detection_time = None
            logging.info(f"First detection for Hornet ID {track_id}")
        else:
            second_detection_time = timestamp
            logging.info(f"Second detection for Hornet ID {track_id}")
        
        # Save to database
        save_to_database(timestamp, first_detections[track_id], second_detection_time, hornet_direction, hornet_count)
    
cap.release()
cv2.destroyAllWindows()
logging.info("Video processing completed.")
