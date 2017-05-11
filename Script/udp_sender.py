import sys
import socket
import json

ID = 1

if len(sys.argv) == 2:
    ID = sys.argv[1]
else:
    print("Please specify ID and Trial number.")
    exit()

HOLOLENS_IP = "192.168.0.192"
HOLOLENS_PORT = 3003

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

message = ID
data = message.encode(encoding='UTF-8')
sock.sendto(data, (HOLOLENS_IP, HOLOLENS_PORT))