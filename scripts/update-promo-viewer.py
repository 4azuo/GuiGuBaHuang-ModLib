"""Data of the promo viewer (promo-viewer/index.html) -> promo-viewer/data/promo.js:
- the pictures of 3161035078/ModProject/SteamImg (promo_*.jpg with the captions of scenarios.json, the other pictures);
- the promo entries of 3161035078/tests/scenarios.json (taken or not);
- the promotion screenshots ("promo-<name>") of every autotest run still in logs/autotest, to compare the takes;
- the rendered videos of marketing/video/out.
Run by collect-promo.ps1 after a promotion run (scripts/game/run-promo.ps1); can be run alone.

usage: python scripts/update-promo-viewer.py [--open]
"""
import json, os, re, sys, time

repo = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
steam = os.path.join(repo, "3161035078", "ModProject", "SteamImg")


def load(p, default):
    try:
        return json.load(open(p, encoding="utf-8-sig"))
    except (OSError, ValueError):
        return default


def stamp(p):
    return time.strftime("%Y-%m-%d %H:%M", time.localtime(os.path.getmtime(p)))


defs = load(os.path.join(repo, "3161035078", "tests", "scenarios.json"), {"scenarios": []})["scenarios"]
entries = []
for d in defs:
    for p in d.get("promo", []):
        entries.append({"name": p["name"], "caption": p.get("caption", ""), "scenario": d["id"], "step": p["step"],
                        "nth": p.get("nth", 1)})
known = {e["name"] for e in entries}

pictures = []
if os.path.isdir(steam):
    for f in sorted(os.listdir(steam)):
        if not re.search(r"\.(png|jpe?g)$", f, re.I):
            continue
        full = os.path.join(steam, f)
        m = re.match(r"promo_(.+)\.(?:png|jpe?g)$", f, re.I)
        pictures.append({"file": f, "name": m.group(1) if m else None, "size": os.path.getsize(full),
                         "time": stamp(full), "stale": bool(m) and m.group(1) not in known})

# takes of every run: screens/NNN_<id>_NN-promo-<name>.png
runs = []
auto = os.path.join(repo, "logs", "autotest")
if os.path.isdir(auto):
    for r in sorted(os.listdir(auto), reverse=True):
        screens = os.path.join(auto, r, "screens")
        if not re.match(r"^\d{8}-\d{6}", r) or not os.path.isdir(screens):
            continue
        takes = []
        for f in sorted(os.listdir(screens)):
            m = re.match(r"^\d+_([A-Z]{2}-\d+)_\d+-promo-(.+)\.png$", f)
            if m:
                takes.append({"name": m.group(2), "scenario": m.group(1), "file": f})
        if takes:
            runs.append({"run": r, "takes": takes})

videos = []
out = os.path.join(repo, "marketing", "video", "out")
if os.path.isdir(out):
    for f in sorted(os.listdir(out)):
        if f.lower().endswith((".mp4", ".webm", ".png")):
            full = os.path.join(out, f)
            videos.append({"file": f, "size": os.path.getsize(full), "time": stamp(full)})

data = {"generated": time.strftime("%Y-%m-%d %H:%M"), "entries": entries, "pictures": pictures, "runs": runs,
        "videos": videos}
dst = os.path.join(repo, "promo-viewer", "data")
os.makedirs(dst, exist_ok=True)
with open(os.path.join(dst, "promo.js"), "w", encoding="utf-8", newline="\n") as fh:
    fh.write("window.PROMO_DATA = " + json.dumps(data, ensure_ascii=False, separators=(",", ":")) + ";\n")
print(f"promo viewer: {len(pictures)} picture(s), {len(entries)} promo entries, {len(runs)} run(s) with takes, "
      f"{len(videos)} video file(s) -> {os.path.join(dst, 'promo.js')}")
if "--open" in sys.argv:
    os.startfile(os.path.join(repo, "promo-viewer", "index.html"))
