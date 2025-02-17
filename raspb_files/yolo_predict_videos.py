import cv2
import numpy as np
from ultralytics import YOLO
from scipy.spatial.distance import euclidean

# Load YOLO ONNX model
model = YOLO("C:/Users/aykaq/Downloads/Hornet_detection_20-01/raspb_files/Final_11/weights/best.pt")

# Open the video file
video_path = "C:/Users/aykaq/Downloads/Hornet_detection_20-01/GP047419 4m40 GOED - Trim.MP4"
cap = cv2.VideoCapture(video_path)

# Get video properties
frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
frame_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
fps = int(cap.get(cv2.CAP_PROP_FPS))

# Define output video writer
output_path = "C:/Users/aykaq/Downloads/Hornet_detection_20-01/GP047419 14-02 - Trim.MP4"
fourcc = cv2.VideoWriter_fourcc(*'mp4v')
out = cv2.VideoWriter(output_path, fourcc, fps, (frame_width, frame_height))

# Store detected hornet colors for re-identification
hornet_colors = {}
next_hornet_id = 0  

def extract_dominant_color(image, box):
    """Extracts dominant color from detected hornet."""
    x1, y1, x2, y2 = map(int, box)
    
    if x2 <= x1 or y2 <= y1:  
        return (60, 255)  # Default to a stable greenish hue in HSV

    hornet_crop = image[y1:y2, x1:x2]

    if hornet_crop.size == 0:
        return (60, 255)  # Return a stable color if no valid pixels

    # Convert to HSV (Hue-Saturation-Value)
    hsv_crop = cv2.cvtColor(hornet_crop, cv2.COLOR_BGR2HSV)

    # Compute histogram to find dominant color
    hist = cv2.calcHist([hsv_crop], [0, 1], None, [180, 256], [0, 180, 0, 256])
    h, s = np.unravel_index(np.argmax(hist), hist.shape)

    return (h, s)  # Keep HSV values for consistency

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break  

    # Run YOLO inference
    results = model.predict(frame, imgsz=640, conf=0.5)
    
    if not results or len(results[0].boxes) == 0:  
        print("⚠️ No detections in this frame.")
        out.write(frame)  # Still write the frame even if no detections
        cv2.imshow("Hornet Detection", frame)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
        continue  

    for result in results:
        boxes = result.boxes.cpu().numpy()

        for box in boxes:
            x1, y1, x2, y2 = box.xyxy[0].tolist()
            confidence = float(box.conf[0])
            class_id = int(box.cls[0])

            # Extract dominant color
            detected_color = extract_dominant_color(frame, (x1, y1, x2, y2))

            # Match hornet by color
            matched_id = None

            for hornet_id, stored_color in hornet_colors.items():
                if euclidean(detected_color, stored_color) < 20:  
                    matched_id = hornet_id
                    break

            # Assign new ID if no match
            if matched_id is None:
                matched_id = next_hornet_id
                hornet_colors[matched_id] = detected_color
                next_hornet_id += 1

            print(f"✅ Detected Hornet {matched_id} | Color: {detected_color}")

            # Draw bounding box & label
            box_color = (int(detected_color[0] * 1.4), int(detected_color[1] * 1.4), 255)
            cv2.rectangle(frame, (int(x1), int(y1)), (int(x2), int(y2)), box_color, 2)
            label = f"Hornet {matched_id}: {confidence:.2f}"
            cv2.putText(frame, label, (int(x1), int(y1) - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, box_color, 2)

            # Save frame when a hornet is detected
            frame_name = f"/home/on8ei/BeeSafe/detections/hornetl_{matched_id}.jpg"
            cv2.imwrite(frame_name, frame)

    # Write processed frame
    out.write(frame)

    # Show detections
    cv2.imshow("Hornet Detection", frame)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release resources
cap.release()
out.release()
cv2.destroyAllWindows()

print(f"✅ Detection complete. Video saved as {output_path}")
