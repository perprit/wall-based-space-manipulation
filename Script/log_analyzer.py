logs = {}

methods = ["CONST_N", "DIST_N", "ADAPT_N", "CONST_W", "DIST_W", "ADAPT_W"]

with open("log.tsv") as logfile:
    contents = logfile.readlines()
    for line in contents:
        line = line.strip()
        sp = line.split("\t")

        user_id = int(sp[0])
        method = sp[1]
        trial_num = int(sp[2])
        xy_type = sp[3]
        z_type = sp[4]
        time = float(sp[5])
        dist = float(sp[6])
        event_type = sp[7]

        obj = {
            "user_id": user_id,
            "method" : method,
            "trial_num": trial_num,
            "xy_type" : xy_type,
            "z_type": z_type,
            "time": time,
            "dist": dist,
            "event_type": event_type
        }

        dep = {
            "time": time,
            "dist": dist
        }

        if not user_id in logs:
            logs[user_id] = {}

        if not method in logs[user_id]:
            logs[user_id][method] = {}

        if not trial_num in logs[user_id][method]:
            logs[user_id][method][trial_num] = []

        logs[user_id][method][trial_num].append(obj)

    for method in methods:
        # TODO calculate from first interaction
        times = [l[-1]["time"]-l[1]["time"] for l in list(filter(lambda l: (l[-1]["z_type"] == "M2S"), [logs[0][method][i] for i in range(24)]))]
        dists = [l[-1]["dist"]-l[1]["dist"] for l in list(filter(lambda l: (l[-1]["z_type"] == "M2S"), [logs[0][method][i] for i in range(24)]))]
        # times = [logs[0][method][i][-1]["time"] for i in range(24)]
        # dists = [logs[0][method][i][-1]["dist"] for i in range(24)]
        
        print("{}, time: {} / dist: {}".format(method, sum(times)/len(times), sum(dists)/len(dists)))