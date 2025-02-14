import cv2
from ultralytics import YOLO

# Load YOLO ONNX model
model = YOLO("/home/on8ei/BeeSafe/best.onnx")

# Open the camera
cap = cv2.VideoCapture(0)

if not cap.isOpened():
    print("❌ Error: Could not open camera.")
    exit()

frame_count = 0


while True:
    ret, frame = cap.read()
    if not ret:
        print("❌ Error: Failed to capture frame.")
        break

    # Run YOLO inference
    results = model.predict(frame, imgsz=640, conf=0.5)  # Lower confidence to detect more

    hornet_detected = False  # Track if hornet is found in this frame

    print(f"📢 Raw Model Output: {results}")  # Debugging: Print detection output

    for result in results:
        for box in result.boxes:
            x1, y1, x2, y2 = box.xyxy[0].tolist()
            confidence = float(box.conf[0])
            class_id = int(box.cls[0])

            print(f"🟢 Detection: x1={x1}, y1={y1}, x2={x2}, y2={y2}, conf={confidence}, class={class_id}")

            if class_id == 0 and confidence > 0.1:  # Lowered confidence
                hornet_detected = True
                cv2.rectangle(frame, (int(x1), int(y1)), (int(x2), int(y2)), (0, 255, 0), 2)
                label = f"Hornet: {confidence:.2f}"
                cv2.putText(frame, label, (int(x1), int(y1) - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)

    if hornet_detected:
        frame_path = f"/home/on8ei/BeeSafe/detected_frame_{frame_count}.jpg"
        cv2.imwrite(frame_path, frame)

        print(f"✅ Hornet detected! Frame saved: {frame_path}")
        frame_count += 1

    # **REMOVE `cv2.imshow()` due to SSH issues**
    cv2.imshow("Hornet Detection", frame)  

    # Press 'q' to exit
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
print("📌 Camera stream stopped.")
