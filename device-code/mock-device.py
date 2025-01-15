#!/usr/bin/env python3
#
# This script emulates a real Raspberry Pi that would connect to our
# server.

import logging
import sys
import beesafe

from datetime import datetime
from time import sleep
from random import randrange

def usage():
    print(f"usage: {sys.argv[0]} URL")
    sys.exit(1)

URL = "http://localhost:5089"

def main():
    logging.basicConfig(level=logging.INFO)

    client = beesafe.BeeSafeClient(URL)
    input("Please press any key to continue when the device has been approved.")
    for i in range(0, 5):
        # Ping the server
        client.send_ping()
        sleep(2)
    for i in range(0, 3):
        # Send a detection event
        client.send_detection_event(5 + randrange(-3,3), int(datetime.now().timestamp()))
        sleep(1)

if __name__ == '__main__':
    main()
