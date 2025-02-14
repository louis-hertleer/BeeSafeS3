#!/usr/bin/env python3
#
# This script emulates a real Raspberry Pi that would connect to our
# server.

import os
import beesafe
from random import uniform

from time import sleep
from threading import Thread

URL = "https://beesafe-app-container.gentlewater-59ffe662.uksouth.azurecontainerapps.io"
ID_FILE = './data/id'

if os.path.isfile(ID_FILE):
    with open(ID_FILE) as f:
        device_id = f.read().strip()
        print(f"Using device id {device_id}")
        # pass dummy values, they are not used if device_id is set
        client = beesafe.BeeSafeClient(URL, 0, 0, 0, device_id=device_id)
else:
    client = beesafe.BeeSafeClient(URL, 51.163000 + uniform(-0.01, 0.01), 4.989118 + uniform(-0.02, 0.02), 25)
    # write the id to a file, so we can pick it up later
    with open(ID_FILE, "w") as f:
        f.write(client.device_id)


def ping_server():
    while True:
        # Ping the server
        client.send_ping()
        print("Sent ping to server.")
        sleep(10)

def main():
    ping_thread = Thread(target=ping_server)

    ping_thread.start()
    ping_thread.join()

if __name__ == '__main__':
    main()
