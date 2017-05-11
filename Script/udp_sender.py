import socket
import json

HOLOLENS_IP = "192.168.0.192"
HOLOLENS_PORT = 3003

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

message = "what the fucn"
data = message.encode(encoding='UTF-8')
sock.sendto(data, (HOLOLENS_IP, HOLOLENS_PORT))