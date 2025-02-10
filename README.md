<img width="180px" align="right" style="float: right;" src="https://github.com/user-attachments/assets/20672e1f-54ca-4860-8527-f84ca6efdf0e">

# BeeSafeS3
This repository will serve as version control and CI/CD for project 4.0

The application can be used in combination with an IoT device to combat asian hornets.
It will track hornet activity and try to calculate a nest location with direction and time.
If the IoT device is attached to an electric harp, it will trigger it if an hornet is detected in the FOV.
The electric harp will attempt to kill the hornet if it flies through the trap.

## BeeSafe Application
The applications is accessible through a public link given in the pipeline
Once you register an account, you can add your own IoT devices. 
If the device is active, it will register the data and start showing predictions.
<br>
The application is writtin in .NET
<br>

## IoT (Raspberry Pi4)
The Physical device consists of a Raspberry Pi4 connected to a relay to trigger the harp.
It should be connected to Wifi or a mobile connection with a 4G adapter.
A webcam should be connected as well to be able to run the AI model.
Once put in a weather-proof case, it can be used outside

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
