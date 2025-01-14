#!/usr/bin/env python3
#
# This script emulates a real Raspberry Pi that would connect to our
# server.

import logging
import sys
import beesafe

from time import sleep

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

if __name__ == '__main__':
    main()
