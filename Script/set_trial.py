import sys
import socket
import json

HOLOLENS_IP = "192.168.0.192"
HOLOLENS_PORT = 3003

ID = 0
BLOCK = 0
TRIAL = 0

# validate ID, BLOCK and TRIAL

if len(sys.argv) == 3:
    ID = int(sys.argv[1])
    BLOCK = int(sys.argv[2])
elif len(sys.argv) == 4:
    ID = int(sys.argv[1])
    BLOCK = int(sys.argv[2])
    TRIAL = int(sys.argv[3])
else:
    print("Usage: <script> <ID> <BLOCK> [<TRIAL>]")
    exit()

if ID < 0 or ID > 11:
    print ("valid ID: 0 <= ID <= 11, current ID: {}".format(ID))
    exit()

if BLOCK < 0 or BLOCK > 5:
    print ("valid BLOCK: 0 <= BLOCK <= 5, current BLOCK: {}".format(BLOCK))
    exit()

if TRIAL < 0 or TRIAL > 23:
    print ("valid TRIAL: 0 <= TRIAL <= 23, current TRIAL: {}".format(TRIAL))
    exit()


# open sequence file
with open('sequence.json') as data_file:    
    sequence_all = json.load(data_file)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

s= sequence_all[str(ID)][BLOCK]
message = json.dumps(s)
print("ID: {} / BLOCK: {} / METHOD: {}".format(ID, BLOCK, s["method"]))
print(message)
data = message.encode(encoding='UTF-8')
sock.sendto(data, (HOLOLENS_IP, HOLOLENS_PORT))