##THIS IS OPTIONAL, DOES NOT HAVE TO BE DONE. 

from ultralytics import YOLO  # Import the YOLO class from the Ultralytics library

# Load the best model from a previous training run stored in the 'best.pt' file. 
model = YOLO("runs/detect/train13/weights/best.pt")  #change the path

# Perform prediction (inference) on a video file
results = model.predict(
    source="GP047419 4m40 GOED - Trim.MP4",  # The input video file or image for prediction. Here, it's a video file.
    save=True,  # Whether to save the results (images or videos with predictions drawn). Set to True to save output.
    conf=0.50,  # Confidence threshold. Objects detected with a confidence lower than 0.25 will be ignored.
    device="0"  # Specifies the device used for prediction. Here, it’s set to CPU. You can use "cuda" for GPU acceleration.
)
