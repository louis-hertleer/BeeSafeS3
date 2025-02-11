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


<img width="65%" style="float:center;" src="https://github.com/user-attachments/assets/e077132a-4cdf-41ed-8545-2e403413072a">




## IoT (Raspberry Pi4)
The Physical device consists of a Raspberry Pi4 connected to a relay to trigger the harp.
It should be connected to Wifi or a mobile connection with a 4G adapter.
A webcam should be connected as well to be able to run the AI model.
Once put in a weather-proof case, it can be used outside

## YOLO (AI hornet detection)
To be able to detect the hornets infront of the device, the solution uses a trained YOLO AI model.
The model uses YOLO V11 with SAHI. It runs on the Raspberry Pi and will send data to the application

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
