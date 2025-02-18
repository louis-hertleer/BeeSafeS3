<img width="180px" align="right" style="float: right;" src="https://github.com/user-attachments/assets/20672e1f-54ca-4860-8527-f84ca6efdf0e">

# BeeSafeS3
#### Why and for who was this project created ?
This solution was made to fullfill the needs of a local beekeeperunion to combat Asian Hornets.
They pose a threat to the local eco-system by killing bees. 
The application in combination with IoT devices should locate nests and activate a trap if hornet is detected.
With this we aim to protect our local bee population

#### What does the application do ?
The application can be used in combination with an IoT device to combat asian hornets.
It will track hornet activity and try to calculate a nest location with direction and time.
If the IoT device is attached to an electric harp, it will trigger it if an hornet is detected in the FOV.
The electric harp will attempt to kill the hornet if it flies through the trap.

## Architecture
Our solution consists of two parts: the web application, which is the user interface for the end users,
and the device code which runs on the Raspberry Pi.

### Device code
This component of the solution is written in Python. It runs the AI model, takes images from the video feed and
constantly sends messages to the web application.

### Web application
This component of the solution is written in C#, with ASP.NET. This collects the data from the devices,
performing the predictions of the hornet nests from that data, and allows the user of our solution to easily
view and manage that information in an easy to use web interface.

### Data Model

<img width="70%" style="float:center;" src="https://github.com/user-attachments/assets/7b7e186c-ee51-4407-9e6b-eda15702a260">



## Setup
In this section, we will go over how to set up our solution.

### Device code
The code of this component is located in the `device-code` folder in this repository.

You will need a Raspberry Pi 4 as a device, with any UVC compatible webcam (generally any USB webcam/camera will
work) as the camera. You will also need an `ssh` connection with the Raspberry Pi.

#### Steps
> [!NOTE]  
> Commands that are run with the regular user are prefixed with `$` should be run with the regular user,
> commands prefixed with `#` are to be run as root, such as with `sudo`.

Clone this GitHub repository as follows, and enter into the directory:
```
$ git clone --depth 1 https://github.com/louis-hertleer/BeeSafeS3
$ cd BeeSafeS3
```
Take a look at `device-code/real-device.py`. Modify the URL variable to your production web application URL
if it is not correct.

Install the required dependencies with the command below:
```
# pip install numpy scipy ultralytics opencv-python
```
This will install the dependencies system-wide, as the systemd unit (see below) will run as an unprivileged user.
If `pip` complains about breaking system packages, override that behavior with the `--break-system-packages` flag.

The systemd unit expects the application to be located in `/opt/beesafe`, so create a folder called "beesafe" in
the `/opt` directory:
```
# mkdir /opt/beesafe
```

Copy the contents of the `device-code` into the newly created folder, and go to it:
```
# cp ./device-code/* /opt/beesafe
$ cd /opt/beesafe
```
Enable the systemd unit, so that it runs on startup:
```
# systemctl enable ./beesafe.service
```

Optionally, you can start it immediately:
```
# systemctl start beesafe.service
```

The device should appear in the "Pending Devices" page. If not, double check whether the URL in `real-device.py` is correct,
and that there is an outgoing internet connection.

### Web application
> [!NOTE]  
> This web application requires Microsoft SQL Server. Using a different database management system requires modification of
> the code.

The code of this component is located in the `BeeSafeWeb` folder in this repository.

In the `BeeSafeWeb`, there is a Dockerfile which will allow for easy building of the web application. An example command to
build the Dockerfile (from the root of the repository):
```
$ docker build -t beesafe:latest ./BeeSafeWeb
```

To run the application (exposing container port 80 to host port 8080)
```
$ docker run -p 8080:80 beesafe:latest
```

#### Connecting a database
Connecting a database to the application is required, as it will crash on startup without one. You can pass a connection
string to the web application as an environment variable:
```
$ docker run -p 8080:80 -e "ConnectionStrings:DefaultConnection"="Server=yourdbhost;User ID=yourusername;Password=yourpassword" beesafe:latest
```
> [!WARNING]
> This specific command may not work on PowerShell for some reason, due to how it handles special characters.

## BeeSafe Application
The applications is accessible through a public link given in the pipeline or in our case, hosted on a fixed domain name: <a href="https://beesafe.space/">BeeSafe Application</a>
Once you register an account, you can add your own IoT devices. 
If the device is active, it will register the data and start showing predictions.
The application is writtin in .NET

<br>
<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/69d36043-2965-4e62-a2db-bae02db5a066">


The application will ask you to create a first user to get started.
Once created, the application is available:


<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/3b25e9da-77ce-42ed-8789-73fd737932f1">


If enough data is collected, the application will make a prediction on where the nest could be. This information is accessible to everyone on the homepage:


<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/1a2ee4dc-64bf-4a90-9dae-aba3c6e77060">


To see your own registered traps, you will need to have an account and register devices:


<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/b233da88-b94e-4b0f-a22c-7c88fa12cd81">

U can manually add hornet detections on the hornet detection page where u enter the direction amount of hornets and minutes it takes for the hornet to come back

<img width="75%" style="float:center;" src="https://github.com/user-attachments/assets/cb20495c-040f-4b16-9605-0a81daa48cae">

<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/b7f28f83-fc52-47f9-af6a-c4e6433baf7d">

On the pending devices page u can see devices that were turned on but have not yet been added to the application. U can eather reject the device or set it's location and add it to the application.

<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/791fa9de-e0cc-4ff1-a512-a92248ef2502">

<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/0eba4046-dd9c-4181-8ecf-ed45337d7051">

The device locations, direction it's facing and name can also be cahanged on the edit device page using the same form.

On the Hornet Calculation page we added the parameters used for calculating the hornets nest location. these parameters can be changed in here to fine tune the predictions.

<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/6e1dc25f-e2e6-4be4-be9e-73207205e8e1">

We also provided a way for users to request an account, on the register page u can request an account. An admin then has to approve the account before ur able to login to the application. An account can also be rejected u won't be able to login if thats the case.

<img width="35%" style="float:center;" src="https://github.com/user-attachments/assets/b05066c8-5cba-46d6-8c36-940dc1a55878">

<img width="75%" style="float:center;" src="https://github.com/user-attachments/assets/150dd234-4083-4341-a2a6-91927a647286">



# Nest Calculation Explanation

The nest calculation component of BeeSafeS3 is responsible for processing detection events and predicting the likely location of Asian Hornet nests. This process involves several steps and uses configurable dynamic settings to adjust the behavior of the calculations. Below is an in-depth explanation of how these calculations work.

## Overview

The calculation service processes detection events received from IoT devices. Each detection event contains data such as:
- **Device Location:** The latitude and longitude where the event was recorded.
- **Detection Timing:** The time difference between the first and second detection, which represents the hornet’s flight time.
- **Hornet Direction:** The direction from which the hornet was detected.

Using these data points, the service estimates where a hornet nest might be located by predicting how far and in which direction a hornet may have traveled.

## Dynamic Settings

The service uses several adjustable settings to fine-tune the calculations. These settings are loaded dynamically (from a settings class) and include:

- **Hornet Speed (m/s):**  
  The average speed at which a hornet flies. This value affects the estimated travel distance during the detection period.

- **Correction Factor:**  
  A multiplier that adjusts the calculated travel distance. A higher correction factor increases the estimated distance and vice versa.

- **Geo Threshold (m):**  
  The maximum geographic distance within which two nest estimates are considered overlapping. This threshold is used when clustering individual estimates.

- **Direction Bucket Size (°):**  
  The angular range used to group nest estimates by their direction. Smaller bucket sizes lead to more precise directional grouping.

- **Direction Threshold (°):**  
  The maximum allowed difference in direction (within a bucket) for estimates to be merged. Estimates with a smaller difference in direction are more likely to be part of the same cluster.

- **Overlap Threshold:**  
  The ratio threshold that determines how much the accuracy circles (representing uncertainty) of two estimates must overlap to be merged into a single cluster.

*Note:* The **Reverse Bearing** flag is not dynamic—it is hardcoded. When enabled, it reverses the detected bearing by adding 180° to it.

## Calculation Process

### 1. Data Retrieval & Filtering
- **Retrieval:**  
  The service retrieves existing nest estimates and detection events from the database, including related information such as device details and known hornet data.
- **Filtering:**  
  Detection events are filtered to ensure they are valid. An event is considered valid if:
  - It is associated with a device.
  - It has a known hornet or is marked as a manual event.
  - The time difference between the first and second detection is positive and does not exceed 20 minutes.

### 2. Individual Nest Estimate Calculation
For each valid detection event:
- **Flight Time Calculation:**  
  The service calculates the flight time in seconds.
- **Distance Estimation:**  
  Using the dynamic hornet speed and correction factor, the estimated distance traveled by the hornet is computed.
- **Direction Processing:**  
  The detected hornet direction is converted to radians. If the reverse bearing flag is enabled, the bearing is adjusted by 180°.
- **Location Prediction:** 
  Spherical trigonometry is applied (using concepts similar to the haversine formula) to predict the new latitude and longitude where the hornet nest might be located.
- **Nest Estimate Creation:**  
  A nest estimate is created with the predicted coordinates, an accuracy value based on the estimated distance, and the final adjusted direction.

### 3. Clustering & Refinement
- **Grouping:**  
  Individual estimates are first grouped by device (or hornet). Within each group, they are further bucketed by their direction using the dynamic bucket size.
- **Merging Overlapping Estimates:**  
  Within each direction bucket, overlapping estimates (those that are both geographically close and similar in direction) are merged.  
  - The service uses weighted averages (weighted by the inverse square of the display accuracy) to merge overlapping estimates.
  - The overlap between estimates is quantified by comparing the intersection area of their accuracy circles to the area of the smaller circle. If this overlap ratio exceeds the dynamic overlap threshold, the estimates are merged.

### 4. Final Merging & Persistence
- **Post-Processing:**  
  After initial clustering, clusters that are fully contained within each other are merged further.
- **Output:**  
  The final merged nest estimates are then saved to the database and are available for further processing by the application (e.g., triggering traps).

## Conclusion

This dynamic and configurable approach enables the BeeSafeS3 system to adapt the nest calculation algorithm in real time. By adjusting these parameters, the accuracy of nest predictions can be fine-tuned based on real-world detection data, ultimately helping local beekeepers respond effectively to Asian Hornet threats.



## IoT (Raspberry Pi4)
The Physical device consists of a Raspberry Pi4 connected to a relay to trigger the harp.
It should be connected to Wifi or a mobile connection with a 4G adapter.
A webcam should be connected as well to be able to run the AI model.
Once put in a weather-proof case, it can be used outside
<br>
Below is a wiring diagram on how our prototype is wired:
<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/6fabbbda-4b46-408d-94d8-fa051c04c524">

## You Only Look Once (YOLO) - AI hornet detection
To be able to detect the hornets infront of the device, the solution uses a trained YOLO AI model.
The model uses YOLOv11. It runs on the Raspberry Pi and will send data to the application. 

1. **Data Collection & Labelling**
   
We began by capturing frames (taking screenshots) from the provided videos to create a dataset for training our object detection model. These frames were then labelled using [Label Studio](https://labelstud.io/guide/), an upen-soruce tool designed for labelling images and videos. 
During this process, we focused solely on labelling Asian Hornets in the frames. Each hornet was marked with a bounding box which indicates the location of the hornet within the image. A class name was also assigned to each labeled object, in this case, “Asian Hornet.” The class name is used to distinguish different objects in the dataset (in our case, it’s just one class—Asian Hornet).
After labeling, we exported both the images and the annotations (labels) using the YOLO format. This format includes the class name and the bounding box coordinates for each Asian Hornet. All images were stored in Google Drive, utilizing premium storage due to the large volume of data.

2. **Training with YOLOv11**
   
For model training, we used YOLOv11. Specifically, we trained the model using the YOLOv11n version, which is a lighter, faster variant optimized for smaller-scale datasets.
Training was conducted on Google Colab, a cloud-based platform that provides free access to GPUs, making it easier to train deep learning models.

To facilitate training, we created a data.yaml file, which defines the paths to our training and validation datasets and lists the class names. The dataset from Step 1 was split into two parts:
    - Training Set: The majority of the labeled images used to teach the model.
    - Validation Set: A smaller set of images used to test and validate the model's performance during training.
These datasets were organized into a folder named dataset, where the images and their respective labels were stored.

<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/26d52e43-34b6-420f-9c52-136d22ffb909">

3. **Video Processing & Prediction**
We upload the YOLO model (best.pt) that we trained to detect hornets and process video frames from the provided file. Each frame is passed through the YOLOv11 model, which is pre-trained on a dataset of Asian hornet images, to perform object detection.

Frame Extraction:

The system reads the input video feed frame by frame. When using a live camera feed, the system captures video frames in real-time; when using a pre-recorded video file, the frames are extracted sequentially from the file.

Prediction Output:

For each detected hornet, the YOLO model provides a confidence score, which indicates how certain the model is that the object within the bounding box is indeed an Asian hornet. A bounding box is a rectangular box around an object (in this case, the Asian Hornet) that helps us locate it in each frame.

4. Once we had the model trained and were able to detect Asian Hornets in the videos, the next step was to track these hornets as they appeared across frames in real-time. For this, we used the SORT (Simple Online and Realtime Tracking) algorithm.
 
SORT is a simple, efficient method that helps track multiple objects across frames in a video by using the bounding box coordinates that our YOLO model predicted. The track ID is assigned to each object (hornet) and is used to keep track of its movement throughout the video.
 
**Saving Data to the Database**
Once a hornet is detected and tracked, we save the details to the database. 


5. **Exporting the Model**
The model is then exported to a format that can be used for inference on raspberry pi. The chosen format in our case is ONNX (Open Neural Network Exchange), which is a popular format for exporting models to run on various hardware platforms, including edge devices like Raspberry Pi.
```
model.export(format="onnx")
```
It will be saved as best.onnx for deployment on Raspberry Pi. The exported ONNX model can be loaded and run on a Raspberry Pi or any device that supports ONNX runtime. After exporting the model, it can be used to make predictions in the deployment environment by running inference on live video streams or pre-recorded videos like we did locally.

Once you have access to the Raspberry Pi, you need to copy all the necessary files (including the trained YOLO model and Python scripts). 

Do not forget to install the ONNX Runtime on the Raspberry Pi:
```
pip install onnxruntime
```

## Pipeline
The application is being build and provisioned automatically through GitHub Actions
There is comments in the yaml files to explain what each part does.
### Deploying
By building the application into a docker image it will be hosted on Azure.
By adding Azure credentials in the repository secrets, you can make it run on your own azure account.

### SAST
Every pull request being made to main, CodeQL will check the code for vulnerabilities like exposed secrets.
After the sast scan is done, all alerts will show up in the security tab. 

### DAST
After a push made on main, an Owasp scan will run on the hosted application to further test vulnerabilities.
The results of the scan will be published in the issue tab in the repository.
<br>

## Azure Cloud Setup
![BeeSafe Azure Setup](https://github.com/user-attachments/assets/d0382a02-2e80-43f9-a913-5936a761d35e)
