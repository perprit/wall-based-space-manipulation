import json
import random
import math

PRETTY_PRINT = False
DISABLED = False

if DISABLED:
    print("disabled now")
    exit()

"""
- ID
: 0 ~ 11

- INTERACTION_METHOD, counter-balanced
: CONST_N (0), DIST_N (1), ADAPT_N (2), CONST_W (3), DIST_W (4), ADAPT_W (5)

- Z DISTANCE, randomized
(S (0): 1 ~ 3, M (1): 4 ~ 6, F (2): 7 ~ 9)
: S2M, S2F, M2F, M2S, F2S, F2M

- XY POSITION, randomized (0 < dist(x,y) < 2)
(D1 (0): 0.0 ~ 0.5, D2 (1): 0.5 ~ 1.0, D3 (2): 1.0 ~ 1.5, D4 (3): 1.5 ~ 2.0)
: D1, D2, D3, D4


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

def z_gen(index):
    if index == 0: 
        # S
        return "%.3f" % random.uniform(1, 3)
    elif index == 1:
        # M
        return "%.3f" % random.uniform(4, 6)
    elif index == 2:
        # F
        return "%.3f" % random.uniform(7, 9)

def euc_dist(a, b):
    return math.sqrt((a[0]-b[0])*(a[0]-b[0])+(a[1]-b[1])*(a[1]-b[1]))

def xy_gen(index):
    start_xy = [random.uniform(-1, 1), random.uniform(-1, 1)]
    target_xy = [random.uniform(-1, 1), random.uniform(-1, 1)]

    while not (0.5 * index < euc_dist(start_xy, target_xy) and euc_dist(start_xy, target_xy) < 0.5 * (index+1)):
        start_xy = [random.uniform(-1, 1), random.uniform(-1, 1)]
        target_xy = [random.uniform(-1, 1), random.uniform(-1, 1)]
    
    print(euc_dist(start_xy, target_xy), 0.5 * (index+1), xypos_trans(index))
    return (["%.3f" % xy for xy in start_xy], ["%.3f" % xy for xy in target_xy])

zdist_trans_dic = ["S", "M", "F"]
def zdist_trans(l):
    return zdist_trans_dic[l[0]] + "2" + zdist_trans_dic[l[1]]


xypos_trans_dic = ["D1", "D2", "D3", "D4"]
def xypos_trans(i):
    return xypos_trans_dic[i]
    

method_trans_dic = [
    "CONST_N", "DIST_N", "ADAPT_N", "CONST_W", "DIST_W", "ADAPT_W"
]

method_sequence = [
    [4,3,2,5,1,0],
    [0,4,3,2,5,1],
    [1,0,4,3,2,5],
    [5,2,0,1,3,4],
    [3,1,5,0,4,2],
    [2,5,1,4,0,3],
    [4,3,2,5,1,0],
    [0,4,3,2,5,1],
    [1,0,4,3,2,5],
    [5,2,0,1,3,4],
    [3,1,5,0,4,2],
    [2,5,1,4,0,3],
]

zdist_list = [
    [0, 1], [0, 2], [1, 2], [1, 0], [2, 0], [2, 1]
]

# xypos_list = [
#     [0, 0], [0, 1], [1, 0], [1, 1]
# ]

xypos_list = [
    0, 1, 2, 3
]

sequence = {}

for id in range(0, 12):
    sequence[id] = []
    for method in method_sequence[id]:
        trials = []
        for zdist in zdist_list:
            for xypos in xypos_list:
                start, target = xy_gen(xypos)
                start.append(z_gen(zdist[0]))
                target.append(z_gen(zdist[1]))
                
                trials.append({
                    "z_type" : zdist_trans(zdist),
                    "xy_type" : xypos_trans(xypos),
                    "start" : start,
                    "target" : target
                })
        
        random.shuffle(trials)
        sequence[id].append({'method': method_trans_dic[method], 'trials': trials})

with open("sequence.json", "w") as json_file:
    if PRETTY_PRINT:
        json.dump(sequence, json_file, sort_keys=True, indent=4, separators=(',', ': '))
    else:
        json.dump(sequence, json_file)