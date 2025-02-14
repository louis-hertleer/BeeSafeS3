from ultralytics import YOLO
import cv2

# Load the YOLO ONNX model
model = YOLO("/home/on8ei/BeeSafe/best.onnx")  # Load ONNX model using YOLO

# Open the video
video_path = "/home/on8ei/BeeSafe/hornet 1 cropped.mp4"
cap = cv2.VideoCapture(video_path)

# Get video properties
frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
frame_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
fps = int(cap.get(cv2.CAP_PROP_FPS))

# Define output video writer
output_path = "/home/on8ei/BeeSafe/detected_video_123.mp4"
fourcc = cv2.VideoWriter_fourcc(*'mp4v')
out = cv2.VideoWriter(output_path, fourcc, fps, (frame_width, frame_height))

frame_count = 0  # Count processed frames

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break  # Stop when the video ends

    frame_count += 1
    print(f"Processing frame {frame_count}...")

    # Run YOLO inference
    results = model.predict(frame, imgsz=640, conf=0.3)  # Predict on frame

    for box in results[0].boxes:  # Loop through detected objects
        x1, y1, x2, y2 = box.xyxy[0].tolist()  # Get bounding box coordinates
        confidence = float(box.conf[0])  # Get confidence score
        class_id = int(box.cls[0])  # Get class ID
        print(f"Detection: x1={x1}, y1={y1}, x2={x2}, y2={y2}, confidence={confidence}, class_id={class_id}")

        # Draw bounding box
        cv2.rectangle(frame, (int(x1), int(y1)), (int(x2), int(y2)), (0, 255, 0), 2)

        # Display confidence score
        label = f"Hornet: {confidence:.2f}"
        cv2.putText(frame, label, (int(x1), int(y1) - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)

    # Write processed frame to output video
    out.write(frame)

    # Show output frame in SSH-friendly way
    if frame_count % 30 == 0:  # Save every 30th frame
        cv2.imwrite(f"/home/on8ei/BeeSafe/sample_frame_{frame_count}.jpg", frame)
        print(f"Saved preview frame: sample_frame_{frame_count}.jpg")

    # Press 'q' to exit video processing
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release resources
cap.release()
out.release()
cv2.destroyAllWindows()

print(f"✅ Detection complete. Video saved as {output_path}")
