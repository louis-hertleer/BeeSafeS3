#!/usr/bin/env python3
import os
from dotenv import load_dotenv
import beesafe
from random import uniform
from time import sleep
from threading import Thread
import torch
from ultralytics import YOLO
import cv2
import logging
import pyodbc
import uuid
from datetime import datetime
import numpy as np
from sort import Sort  

# Configure logging
logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")

# Load environment variables from .env file
load_dotenv()

# Now you can access your environment variables like this:
DB_CONNECTION_STRING = os.getenv('DB_CONNECTION_STRING')
URL = os.getenv('URL')
DEVICE_ID = os.getenv('DEVICE_ID')
ENVIRONMENT = os.getenv('ENVIRONMENT')

# Production URL
URL = os.getenv("URL")
ID_FILE = './data/id'

# Device ID (Only 1 device in use for now)
DEVICE_ID = os.getenv("DEVICE_ID")

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

# Initialize the BeeSafeClient (with device ID if available)
if os.path.isfile(ID_FILE):
    with open(ID_FILE) as f:
        device_id = f.read().strip()
        print(f"Using device id {device_id}")
        # pass dummy values, they are not used if device_id is set
        client = beesafe.BeeSafeClient(URL, 0, 0, 0, device_id=device_id)
else:
    client = beesafe.BeeSafeClient(URL, 51.163000 + uniform(-0.01, 0.01), 4.989118 + uniform(-0.02, 0.02), 25)
    # write the id to a file, so we can pick it up later
    with open(ID_FILE, "w") as f:
        f.write(client.device_id)

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


def save_to_database(timestamp, first_detection, second_detection, direction, hornet_count):
    """Save detection event to database with a valid KnownHornetId."""
    try:
        known_hornet_id = get_known_hornet_id()  # Always fetch from KnownHornet table

        if not known_hornet_id:
            logging.error("No valid KnownHornetId found. Skipping database insert.")
            return  # Stop execution if no valid ID

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

        logging.info(f"Detection saved: ID={detection_id}, HornetCount={hornet_count}, Direction={direction:.2f}")
    except Exception as e:
        logging.error(f"Database error: {e}")
        if 'conn' in locals():
            conn.rollback()
    finally:
        if 'conn' in locals():
            conn.close()

def track_and_save_detections(detections, frame, timestamp):
    """Update tracker with new detections and save to database."""
    # Update SORT tracker with detections
    tracked_objects = tracker.update(detections)
    hornet_count = len(tracked_objects)

    for obj in tracked_objects:
        x1, y1, x2, y2, track_id = map(int, obj)  # Get tracking data

        # Calculate hornet's direction based on x-coordinate (normalized)
        x_center = (x1 + x2) / 2
        normalized_x = x_center / frame_width
        hornet_direction = float(normalized_x * 180.0)

        # Handle first detection and second detection logic
        if track_id not in first_detections:
            first_detections[track_id] = timestamp
            second_detection_time = None
            logging.info(f"First detection for Hornet ID {track_id}")
        else:
            second_detection_time = timestamp
            logging.info(f"Second detection for Hornet ID {track_id}")
        
        # Save to database
        save_to_database(timestamp, first_detections[track_id], second_detection_time, hornet_direction, hornet_count)

def ping_server():
    while True:
        # Ping the server
        client.send_ping()
        print("Sent ping to server.")
        sleep(10)

def main():
    ping_thread = Thread(target=ping_server)
    ping_thread.start()
    ping_thread.join()

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

        # Track and save detections
        track_and_save_detections(detections, frame, timestamp)
    
    cap.release()
    cv2.destroyAllWindows()
    logging.info("Video processing completed.")

if __name__ == '__main__':
    main()
