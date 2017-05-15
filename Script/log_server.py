import sys
import socket
import json

LOGSERVER_PORT = 3005

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

sock.bind(("", LOGSERVER_PORT))

print("listening from port: {}".format(LOGSERVER_PORT))
with open("log.tsv", "a") as log_file:
    while True:
        msg = sock.recvfrom(4096)
        log = msg[0].decode("utf-8")
        print(log)
        log_file.write(log)
        log_file.write("\n")
        log_file.flush()