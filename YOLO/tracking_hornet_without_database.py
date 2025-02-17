import cv2  # OpenCV library for handling video frames and drawing
import numpy as np  # NumPy for array operations
from ultralytics import YOLO  # Importing the YOLO object detection model from Ultralytics
from sort import Sort  # Importing the SORT tracker for tracking objects across frames

# Load the YOLOv11 model with pre-trained weights
model = YOLO("best.pt")

# Initialize the SORT tracker with parameters:
# - max_age: Number of frames an object can be missing before being removed
# - min_hits: Minimum detections before an object is considered valid
# - iou_threshold: IoU threshold for associating detections with existing tracks
tracker = Sort(max_age=50, min_hits=3, iou_threshold=0.50)

# Load video from file
cap = cv2.VideoCapture("test_video.mp4")

# Get video properties such as width, height, and FPS
frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
frame_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
fps = int(cap.get(cv2.CAP_PROP_FPS))

# Define a video writer to save the output with bounding boxes
out = cv2.VideoWriter(
    "output_out_of_frame_tracking_maxage50.avi",  # Output file name
    cv2.VideoWriter_fourcc(*'XVID'),  # Codec used for output video
    fps,  # Frames per second
    (frame_width, frame_height)  # Video resolution
)

# Dictionary to store objects that go out of the frame
out_of_frame_objects = {}

# Initialize a frame counter
frame_count = 0  

# Process each frame until the video ends
while cap.isOpened():
    ret, frame = cap.read()  # Read the next frame
    if not ret:  # If no frame is read, exit the loop
        break

    frame_count += 1  # Increment frame count

    # Run the YOLO model on the current frame to detect objects
    results = model(frame)

    detections = []  # List to store detected bounding boxes

    # Extract detected objects and their confidence scores
    for r in results:
        for box in r.boxes:
            x1, y1, x2, y2 = box.xyxy[0].tolist()  # Extract bounding box coordinates
            conf = box.conf[0].item()  # Get confidence score

            if conf > 0.25:  # Only keep detections above the confidence threshold
                detections.append([x1, y1, x2, y2, conf])

    # Convert detections list to a NumPy array
    detections = np.array(detections)

    # Ensure detections are properly formatted for SORT tracker
    if detections.shape[0] == 0:  # If no objects detected, create an empty array
        detections = np.empty((0, 5))

    # Update the SORT tracker with new detections
    tracked_objects = tracker.update(detections)

    # Check for objects that have moved out of the frame and save their last position
    for obj in tracked_objects:
        x1, y1, x2, y2, track_id = map(int, obj)  # Extract object details

        # If the object is out of bounds (beyond the frame), save its last known position
        if x1 < 0 or x2 > frame_width or y1 < 0 or y2 > frame_height:
            out_of_frame_objects[track_id] = (x1, y1, x2, y2)

    # Draw bounding boxes and track IDs on the frame
    for obj in tracked_objects:
        x1, y1, x2, y2, track_id = map(int, obj)  # Extract bounding box and ID

        # Draw a rectangle around the detected object
        cv2.rectangle(frame, (x1, y1), (x2, y2), (0, 255, 0), 4)  # Green box

        # Display the track ID above the object
        cv2.putText(frame, f"ID {track_id}", (x1, y1 - 10),
                    cv2.FONT_HERSHEY_SIMPLEX, 1.0, (0, 255, 0), 3)

        # If an object was previously detected as out of frame, check if it has returned
        if track_id in out_of_frame_objects:
            last_position = out_of_frame_objects[track_id]  # Get last known position
            
            # Compare the new position with the last known position
            if (abs(x1 - last_position[0]) < 100 and abs(y1 - last_position[1]) < 100):
                # If the position difference is small, assume it's the same object
                out_of_frame_objects.pop(track_id)  # Remove from out-of-frame list
                print(f"Object {track_id} reappeared!")  # Print a message

    # Write the processed frame to the output video file
    out.write(frame)

    # Display the frame with tracking (if needed, can be commented out)
    # cv2.imshow("Tracking", frame)
    #if cv2.waitKey(1) & 0xFF == ord('q'):  # Press 'q' to stop processing
        #break

# Release video capture and writer resources
cap.release()
out.release()
cv2.destroyAllWindows()  # Close all OpenCV windows
