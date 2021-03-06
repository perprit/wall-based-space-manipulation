import sys
import socket
import json

HOLOLENS_IP = "192.168.0.192"
HOLOLENS_PORT = 3003

ID = 0
BLOCK = 0
MODE = "t"

# validate ID, BLOCK and TRIAL

if len(sys.argv) == 3:
    ID = int(sys.argv[1])
    BLOCK = int(sys.argv[2])
elif len(sys.argv) == 4:
    ID = int(sys.argv[1])
    BLOCK = int(sys.argv[2])
    MODE = sys.argv[3]
else:
    print("Usage: <script> <ID> <BLOCK>")
    exit()

if ID < 0 or ID > 11:
    print ("valid ID: 0 <= ID <= 11, current ID: {}".format(ID))
    exit()

if BLOCK < 0 or BLOCK > 5:
    print ("valid BLOCK: 0 <= BLOCK <= 5, current BLOCK: {}".format(BLOCK))
    exit()

if MODE != "t" and MODE != "p":
    print ("valid MODE: p or n, current MODE: {}".format(MODE))

# open sequence file
with open('sequence.json') as data_file:    
    sequence_all = json.load(data_file)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

s= sequence_all[str(ID)][BLOCK]
s["id"] = ID
s["mode"] = MODE

message = json.dumps(s)
print("ID: {} / BLOCK: {}, METHOD: {}".format(ID, BLOCK, s["method"]))
if MODE == "p":
    print("PRACTICE MODE: log is not recorded")
else:
    print("TEST MODE: logs are recorded")
# print(message)
data = message.encode(encoding='UTF-8')
sock.sendto(data, (HOLOLENS_IP, HOLOLENS_PORT))