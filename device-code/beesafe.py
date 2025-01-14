# This is a library to encapsulate the interaction between the device
# and server.
import requests
import logging
import json

from enum import Enum

class MessageType(int, Enum):
    PING = 0
    PONG = 1
    DETECTION_EVENT = 2


class BeeSafeClient:
    def __init__(self, url, device_id=None):
        self.url = url
        self.device_id = device_id
        if device_id == None:
            self.device_id = self.__register_device()

    def __register_device(self):
        data = {
            'latitude': 51.5,
            'longitude': 5.05,
            'direction': 25
        }

        r = requests.post(self.url + "/Device/Register", json=data)

        if r.status_code != 200:
            raise Exception("Failed to register device.")

        try:
            id = r.json()["id"]
        except KeyError:
            raise Exception("Expected id in response, not found.")

        logging.info(f"Successfully registered device with id {id}.")

        return id

    # timestamp must be a Unix timestamp.
    def send_detection_event(self, hornet_direction, timestamp):
        data = {
            'device': self.device_id,
            'message_type': MessageType.DETECTION_EVENT,
            'data': {
                'hornet_direction': hornet_direction,
                'timestamp': timestamp
            }
        }

        r = requests.post(self.url + "/Device/DetectionEvent", json=data)

        if r.status_code != 200:
            raise Exception("Failed to send detection event.")

        logging.info(f"Successfully sent detection event.")

    def send_ping(self):
        data = {
            'device': self.device_id,
            'message_type': MessageType.PING
        }

        r = requests.post(self.url + "/Device/Ping", json=data)

        if r.status_code != 200:
            message = "Failed to ping server."
            if r.status_code == 403:
                message = "This device has not been approved yet."
            raise Exception(message)

        response = r.json()

        assert response["message_type"] == MessageType.PONG, \
            f"Ping response must have the message_type field be PONG."

        logging.info(f"Successfully pinged server")
