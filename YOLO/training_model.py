from ultralytics import YOLO  # Importing the YOLO class from the Ultralytics library. Make sure you have installed ultralyrics! 


model = YOLO("yolo11n.pt")  # Using the pre-trained weights for YOLOv11n model on COCO dataset

# Start training the model on a custom dataset, here using the COCO8 example dataset
results = model.train(
    data="/content/drive/MyDrive/Colab Notebooks/Rae/dataset/data.yaml",  # Path to the YAML file that contains dataset configurations and class labels
    epochs=50,  # Number of epochs (iterations over the entire dataset). Here, we train for 50 epochs
    imgsz=640,  # The image size for input. Setting it to 640x640 for a balance between speed and accuracy

    # This is optional. we have this because we trained on Google Colab and if you dont declare the project and name of the project, the model will train but will not save the results (despite saying it did in the output)
    project="/content/drive/MyDrive/Colab Notebooks/Rae/",  # Project directory where results will be saved (including logs and weights)
    name="Final_",  # Name of the folder in the project directory where model files will be saved


    batch=16,  # Batch size: Number of images per batch during training. A smaller batch might speed up training, but larger can be more stable.
    device=0,  # Specifies the device (GPU 0) to use. 0 means first available GPU. You can use "cpu" if you dont have a GPU, it will just take longer to run. 
    
    # Augmentation parameters that help the model generalize better:
    hsv_h=0.015,  # Random hue shift. Helps the model be invariant to slight changes in color.
    hsv_s=0.7,    # Random saturation shift. Helps the model adapt to variations in lighting conditions.
    hsv_v=0.4,    # Random brightness shift. Simulates changes in brightness in different lighting.
    
    degrees=30,   # Random rotation within the given angle. Helps the model learn rotational invariance.
    translate=0.2,  # Random translation (shifting) of images within the specified percentage. Helps the model learn position invariance.
    scale=0.2,    # Random scaling of images. This ensures the model can recognize objects at various scales.
    shear=0.1,    # Random shear. Small distortions that simulate real-world scenarios.
    
    flipud=0.3,   # Vertical flip probability. Random flipping helps to increase variation and make the model more robust.
    fliplr=0.5,   # Horizontal flip probability. Horizontal flipping is quite common and allows the model to generalize better.
    
    mosaic=1.0,   # Mosaic augmentation. It randomly combines multiple images into a single image to provide a richer dataset.
    mixup=0.3,    # Mixup augmentation. Combines two images, which can improve model generalization by preventing overfitting.
    copy_paste=0.3  # Copy-paste augmentation. Helps in simulating occlusions or objects appearing in unusual parts of the image.
)
