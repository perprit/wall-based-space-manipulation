import json
import random
import math

PRETTY_PRINT = True
DISABLED = False

if DISABLED:
    print("disabled now")
    exit()

"""
- ID
: 0 ~ 11

- INTERACTION_METHOD, counter-balanced
: WALL (0), NO_WALL(1)

- Z DISTANCE, randomized
(C (0): 3, F (1): 9)
: C2F, F2C

- XY POSITION
(SHORT (0): 1, FAR (1): 2)
: SHORT, FAR


OUTPUT

{
    <id>:
    [
        {
            <method>: ~~,
            <trials>:
            [
                {
                    "z_type": ~~,
                    "start": ~~,
                    "target": ~~
                },
                x 24
            ]
        },
        x 12
    ]
}
"""

def euc_dist(a, b):
    return math.sqrt((a[0]-b[0])*(a[0]-b[0])+(a[1]-b[1])*(a[1]-b[1]))

def z_gen(z_type):
    if z_type == "C2F":
        return ["{:.4f}".format(float(z)) for z in [3, 9]]
    elif z_type == "F2C":
        return ["{:.4f}".format(float(z)) for z in [9, 3]]

def xy_gen(xy_type):
    start_xy = [random.uniform(-1, 1), random.uniform(-1, 1)]
    target_xy = [random.uniform(-1, 1), random.uniform(-1, 1)]

    if xy_type == "SHORT":
        length = 1
    elif xy_type == "FAR":
        length = 2

    dist = euc_dist(start_xy, target_xy)

    while not (length - 0.01 < dist and dist < length + 0.01):
        start_xy = [random.uniform(-1, 1), random.uniform(-1, 1)]
        target_xy = [random.uniform(-1, 1), random.uniform(-1, 1)]
        dist = euc_dist(start_xy, target_xy)
    
    # print("{:7} {:8.3f} | {:8.3f}, {:8.3f} / {:8.3f}, {:8.3f}".format(xy_type, dist, start_xy[0], start_xy[1], target_xy[0], target_xy[1]))
    return (["%.3f" % xy for xy in start_xy], ["%.3f" % xy for xy in target_xy])

z_types = ["C2F", "F2C"]
xy_types = ["SHORT", "FAR"]
method_types = ["WALL", "NO_WALL"]
method_sequence = [
    [0, 1],
    [1, 0],
    [0, 1],
    [1, 0],
    [0, 1],
    [1, 0],
    [0, 1],
    [1, 0],
    [0, 1],
    [1, 0],
    [0, 1],
    [1, 0]
]

sequence = {}

for id in range(0, 8):
    sequence[id] = []
    for method in method_sequence[id]:

        trials = []
        for z_type in z_types:
            for xy_type in xy_types:
                start, target = xy_gen(xy_type)
                start.append(z_gen(z_type)[0])
                target.append(z_gen(z_type)[1])
                
                trials.append({
                    "z_type" : z_type,
                    "xy_type" : xy_type,
                    "start" : start,
                    "target" : target
                })
        
        random.shuffle(trials)
        sequence[id].append({'method': method_types[method], 'trials': trials})

with open("sequence.json", "w") as json_file:
    if PRETTY_PRINT:
        json.dump(sequence, json_file, sort_keys=True, indent=4, separators=(',', ': '))
    else:
        json.dump(sequence, json_file)