import cv2
import os

# Video file path
video_path = "C:/Users/aykaq/Downloads/Hornet_detection_20-01/videos hornets/GP047420v1 - Trim.MP4"

# Output directory for frames
output_dir = "frames"
os.makedirs(output_dir, exist_ok=True)

# Load video
cap = cv2.VideoCapture(video_path)
fps = cap.get(cv2.CAP_PROP_FPS)  # Get frames per second
frame_time = 1000 / fps  # Time per frame in milliseconds

# Set extraction interval (e.g., 1 ms or 5 ms)
interval_ms = 5  # Change to 1 for every millisecond

# Process frames
frame_count = 0
while cap.isOpened():
    current_time = frame_count * frame_time  # Current time in ms
    if current_time % interval_ms == 0:
        ret, frame = cap.read()
        if not ret:
            break
        frame_filename = os.path.join(output_dir, f"frame_{frame_count}.jpg")
        cv2.imwrite(frame_filename, frame)
        
        # Print progress in the terminal
        print(f"Processed frame {frame_count} at {current_time:.2f} ms")

    frame_count += 1

cap.release()
cv2.destroyAllWindows()

print(f"\nFrames saved in '{output_dir}'")
