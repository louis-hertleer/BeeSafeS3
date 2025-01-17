#!/usr/bin/env python3
#
# This script emulates a real Raspberry Pi that would connect to our
# server.

import logging
import sys
import beesafe
from random import uniform

from time import sleep
from threading import Thread

def usage():
    print(f"usage: {sys.argv[0]} URL")
    sys.exit(1)

URL = "http://localhost:5089"

client = beesafe.BeeSafeClient(URL, 51.163000 + uniform(-0.01, 0.01), 4.989118 + uniform(-0.02, 0.02), 25)

def ping_server():
    logging.basicConfig(level=logging.INFO)
    while True:
        # Ping the server
        client.send_ping()
        sleep(10)

def main():
    logging.basicConfig(level=logging.INFO)

    input("Please press any key to continue when the device has been approved.")

    ping_thread = Thread(target=ping_server)

    ping_thread.start()
    ping_thread.join()

if __name__ == '__main__':
    main()
