import cv2
import numpy as np
from ultralytics import YOLO
from scipy.spatial.distance import euclidean

# Load YOLO ONNX model
model = YOLO("/home/on8ei/BeeSafe/best.onnx")  # Load ONNX model

# Open camera (use 0, 1, or 2 depending on your camera index)
cap = cv2.VideoCapture(0)  # Change to 1 or 2 if needed

# Set camera resolution (Optional)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1280)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 720)

# Storage for hornet colors & re-identification
hornet_colors = {}
next_hornet_id = 0  # ID counter for new hornets

def extract_dominant_color(image, box):
    """Extracts dominant color from the detected hornet region."""
    x1, y1, x2, y2 = map(int, box)

    # Ensure the bounding box is valid
    if x2 <= x1 or y2 <= y1:
        return None  

    hornet_crop = image[y1:y2, x1:x2]

    # If crop is empty, return a default color
    if hornet_crop.size == 0:
        return None  

    # Convert to HSV color space
    hsv_crop = cv2.cvtColor(hornet_crop, cv2.COLOR_BGR2HSV)

    # Compute **mean HSV** (instead of just histogram max)
    mean_color = np.mean(hsv_crop, axis=(0, 1))

    return (int(mean_color[0]), int(mean_color[1]))  # Keep Hue & Saturation only

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        print("⚠️ Camera not detected. Check connection.")
        break  

    # Run YOLO inference
    results = model.predict(frame, imgsz=640, conf=0.1)

    for result in results:
        boxes = result.boxes.cpu().numpy()

        for box in boxes:
            x1, y1, x2, y2 = box.xyxy[0].tolist()
            confidence = float(box.conf[0])
            class_id = int(box.cls[0])

            # Extract dominant color of the detected hornet
            detected_color = extract_dominant_color(frame, (x1, y1, x2, y2))

            if detected_color:
                matched_id = None

                # **Re-identify hornets based on color similarity**
                for hornet_id, stored_color in hornet_colors.items():
                    if euclidean(detected_color, stored_color) < 30:  # Looser threshold
                        matched_id = hornet_id
                        break

                # If no match, assign a new ID
                if matched_id is None:
                    matched_id = next_hornet_id
                    hornet_colors[matched_id] = detected_color
                    next_hornet_id += 1

                print(f"✅ Detected Hornet {matched_id} | Color: {detected_color}")

                # Convert HSV to BGR for display
                box_color = cv2.cvtColor(
                    np.uint8([[[detected_color[0], detected_color[1], 255]]]),
                    cv2.COLOR_HSV2BGR)[0][0]
                box_color = tuple(int(c) for c in box_color)

                # Draw bounding box & label
                cv2.rectangle(frame, (int(x1), int(y1)), (int(x2), int(y2)), box_color, 2)
                label = f"Hornet {matched_id}: {confidence:.2f}"
                cv2.putText(frame, label, (int(x1), int(y1) - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, box_color, 2)

                # Save frame when a hornet is detected
                frame_name = f"/home/on8ei/BeeSafe/detections/hornet_{matched_id}.jpg"
                cv2.imwrite(frame_name, frame)

      # **Save frame only if a hornet was detected**

    # **Show detections while running**
    cv2.imshow("Hornet Detection Live", frame)

    # Press 'q' to exit
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release resources
cap.release()
cv2.destroyAllWindows()

print("✅ Live detection stopped. All detected hornet images saved.")
