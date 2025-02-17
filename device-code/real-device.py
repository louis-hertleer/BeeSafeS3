#!/usr/bin/env python3
#
# This script emulates a real Raspberry Pi that would connect to our
# server.

import os
import beesafe
from random import uniform
from time import sleep
from threading import Thread
import cv2
import numpy as np
from ultralytics import YOLO
from scipy.spatial.distance import euclidean
import RPi.GPIO as GPIO
import time

# Constants
URL = "https://beesafe-app-container.gentlewater-59ffe662.uksouth.azurecontainerapps.io"
ID_FILE = './data/id'

# GPIO setup
RELAY_PIN = 17   # GPIO pin connected to the relay
BUTTON_PIN = 18  # GPIO pin connected to the button
GPIO.setmode(GPIO.BCM)  # Use BCM pin numbering
GPIO.setup(RELAY_PIN, GPIO.OUT)  # Set relay pin as output
GPIO.setup(BUTTON_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)  # Set button pin as input with pull-up resistor
GPIO.output(RELAY_PIN, GPIO.LOW)  # Ensure relay starts OFF

# Global state variables
relay_enabled = True  # Controls whether the relay can be activated
hornet_colors = {}
next_hornet_id = 0

# Load YOLO ONNX model
model = YOLO("/home/on8ei/BeeSafe/Final_11/weights/best.pt")

# Initialize Beesafe client
if os.path.isfile(ID_FILE):
    with open(ID_FILE) as f:
        device_id = f.read().strip()
        print(f"Using device id {device_id}")
        client = beesafe.BeeSafeClient(URL, 0, 0, 0, device_id=device_id)
else:
    client = beesafe.BeeSafeClient(URL, 51.163000 + uniform(-0.01, 0.01), 4.989118 + uniform(-0.02, 0.02), 25)
    with open(ID_FILE, "w") as f:
        f.write(client.device_id)

# Define the function to send ping to the server
def ping_server():
    while True:
        client.send_ping()
        print("Sent ping to server.")
        sleep(10)

# Define the function to process hornet detection
def hornet_detection():
    global next_hornet_id
    # Open video capture
    video_path = "/home/on8ei/BeeSafe/GP047419 4m40 GOED - Trim (2).MP4"
    cap = cv2.VideoCapture(video_path)

    # Get video properties
    frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    frame_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    fps = int(cap.get(cv2.CAP_PROP_FPS))

    # Define output video writer
    output_path = "/home/on8ei/BeeSafe/hornet-177-02 captured.mp4"
    fourcc = cv2.VideoWriter_fourcc(*'mp4v')
    out = cv2.VideoWriter(output_path, fourcc, fps, (frame_width, frame_height))

    def extract_dominant_color(image, box):
        x1, y1, x2, y2 = map(int, box)
        if x2 <= x1 or y2 <= y1:  
            return (60, 255)  
        hornet_crop = image[y1:y2, x1:x2]
        if hornet_crop.size == 0:
            return (60, 255)
        hsv_crop = cv2.cvtColor(hornet_crop, cv2.COLOR_BGR2HSV)
        hist = cv2.calcHist([hsv_crop], [0, 1], None, [180, 256], [0, 180, 0, 256])
        h, s = np.unravel_index(np.argmax(hist), hist.shape)
        return (h, s)

    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break  

        results = model.predict(frame, imgsz=640, conf=0.5)
        
        if not results or len(results[0].boxes) == 0:
            print("⚠️ No detections in this frame.")
            if not relay_enabled:
                GPIO.output(RELAY_PIN, GPIO.LOW)
            out.write(frame)
            cv2.imshow("Hornet Detection", frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
            continue  

        for result in results:
            boxes = result.boxes.cpu().numpy()

            for box in boxes:
                x1, y1, x2, y2 = box.xyxy[0].tolist()
                confidence = float(box.conf[0])  # Extract confidence score
                detected_color = extract_dominant_color(frame, (x1, y1, x2, y2))

                matched_id = None
                for hornet_id, stored_color in hornet_colors.items():
                    if euclidean(detected_color, stored_color) < 20:
                        matched_id = hornet_id
                        break

                if matched_id is None:
                    matched_id = next_hornet_id
                    hornet_colors[matched_id] = detected_color
                    next_hornet_id += 1

                print(f"✅ Detected Hornet {matched_id} | Color: {detected_color}")

                box_color = (int(detected_color[0] * 1.4), int(detected_color[1] * 1.4), 255)
                cv2.rectangle(frame, (int(x1), int(y1)), (int(x2), int(y2)), box_color, 2)
                label = f"Hornet {matched_id}: {confidence:.2f}"
                cv2.putText(frame, label, (int(x1), int(y1) - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, box_color, 2)

                if relay_enabled:
                    GPIO.output(RELAY_PIN, GPIO.HIGH)
                    time.sleep(0.5)
                    GPIO.output(RELAY_PIN, GPIO.LOW)
                else:
                    GPIO.output(RELAY_PIN, GPIO.HIGH)

        out.write(frame)
        cv2.imshow("Hornet Detection", frame)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    cap.release()
    out.release()
    cv2.destroyAllWindows()

# Define button callback for relay control
def button_callback(channel):
    global relay_enabled
    relay_enabled = not relay_enabled
    if not relay_enabled:
        GPIO.output(RELAY_PIN, GPIO.LOW)  
    print(f"Button pressed! Relay {'ENABLED' if relay_enabled else 'DISABLED'}.")
    time.sleep(0.2)

# Set up event detection for the button
GPIO.add_event_detect(BUTTON_PIN, GPIO.FALLING, callback=button_callback, bouncetime=300)

def main():
    ping_thread = Thread(target=ping_server)
    hornet_thread = Thread(target=hornet_detection)

    print('fgfg')

    ping_thread.start()
    hornet_thread.start()

    ping_thread.join()
    hornet_thread.join()

if __name__ == '__main__':
    main()
