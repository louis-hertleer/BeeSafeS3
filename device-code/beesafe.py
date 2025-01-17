# This is a library to encapsulate the interaction between the device
# and server.
import requests
import logging
import json
from datetime import datetime

from enum import Enum

class MessageType(int, Enum):
    PING = 0
    PONG = 1
    DETECTION_EVENT = 2

class RegistrationDeviceError(Exception):
    pass

class NonExistantDeviceError(Exception):
    pass

class DeviceNotApprovedError(Exception):
    pass

class BeeSafeClient:
    """
    This class allows you to interact with the application via messages.
    """
    def __init__(self, url, latitude: float, longitude: float, direction: float, device_id:str =""):
        """
        :param str url: The URL of the application.
        :param str device_id: (optional) the ID of an already registered device.
        """
        self.url = url
        self.device_id = device_id
        if device_id == "":
            self.device_id = self.__register_device(latitude, longitude, direction)

    def __register_device(self, latitude: float, longitude: float, direction: float) -> str:
        data = {
            'latitude': latitude,
            'longitude': longitude,
            'direction': direction
        }

        r = requests.post(self.url + "/Device/Register", json=data)

        if r.status_code != 200:
            raise RegistrationDeviceError()

        try:
            id = r.json()["id"]
        except KeyError:
            raise Exception("Expected id in response, not found.")

        logging.info(f"Successfully registered device with id {id}.")

        return id

    def _check_status_code(self, status_code: int, generic_message: str):
        if status_code != 200:
            message = generic_message
            if status_code == 403:
                raise DeviceNotApprovedError()
            elif status_code == 401:
                raise NonExistantDeviceError()


    def send_detection_event(self, hornet_direction: float, timestamp: datetime):
        """
        Send a detection event message. When a hornet is detected, this
        function is what you use.

        :param float hornet_direction: The direction of the hornet detected.
        :param datetime timestamp: The time when the hornet was detected.
        """
        data = {
            'device': self.device_id,
            'message_type': MessageType.DETECTION_EVENT,
            'data': {
                'hornet_direction': hornet_direction,
                'timestamp': timestamp.timestamp()
            }
        }

        r = requests.post(self.url + "/Device/DetectionEvent", json=data)

        self._check_status_code(r.status_code, "Failed to send detection event.")

        logging.info(f"Successfully sent detection event.")

    def send_ping(self):
        """
        Send a ping to the server. You should do this regularly to let
        the server know that this device is still working as intended.
        """
        data = {
            'device': self.device_id,
            'message_type': MessageType.PING
        }

        r = requests.post(self.url + "/Device/Ping", json=data)

        try:
            self._check_status_code(r.status_code, "Failed to ping server.")
        # We can safely ignore that
        except DeviceNotApprovedError:
            return


        response = r.json()

        assert response["message_type"] == MessageType.PONG, \
            f"Ping response must have the message_type field be PONG."

        logging.info(f"Successfully pinged server")
