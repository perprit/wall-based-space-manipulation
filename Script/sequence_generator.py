import json
import random

PRETTY_PRINT = False
DISABLED = True

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

- XY POSITION, randomized
(I (0): (-0.75 ~ 0.75)x(-0.75 ~ 0.75), O (1): (-1.5 ~ -0.75 || 0.75 ~ 1.5)x(-1.5 ~ -0.75 || 0.75 ~ 1.5)))
: I2I, I2O, O2I, O2O


OUTPUT

{
    <id>:
    [
        {
            <method>: ~~,
            <trials>:
            [
                {
                    "xy_type": ~~,
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

def xy_gen(index):
    if index == 0:
        # In
        return ["%.3f" % random.uniform(-0.75, 0.75), "%.3f" % random.uniform(-0.75, 0.75)]
    elif index == 1:
        # Out
        ret = []
        for _ in range(2):
            if random.randint(0, 1) == 0:
                ret.append("%.3f" % random.uniform(-1.5, -0.75))
            else:
                ret.append("%.3f" % random.uniform(0.75, 1.5))
        return ret

zdist_trans_dic = ["S", "M", "F"]
def zdist_trans(l):
    return zdist_trans_dic[l[0]] + "2" + zdist_trans_dic[l[1]]


xypos_trans_dic = ["I", "O"]
def xypos_trans(l):
    return xypos_trans_dic[l[0]] + "2" + xypos_trans_dic[l[1]]
    

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

xypos_list = [
    [0, 0], [0, 1], [1, 0], [1, 1]
]

sequence = {}

for id in range(0, 12):
    sequence[id] = []
    for method in method_sequence[id]:
        trials = []
        for zdist in zdist_list:
            for xypos in xypos_list:
                start = xy_gen(xypos[0])
                start.append(z_gen(zdist[0]))
                target = xy_gen(xypos[1])
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