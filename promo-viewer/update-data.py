"""Data of the promo viewer of a mod repository (shared by the mods using ModLib) -> <repo>/logs/promo-viewer/index.html
(from the template index.html of this folder: loads app.js/style.css of ModLib) and <repo>/logs/promo-viewer/data/promo.js
(git-ignored with logs/):
- the pictures of <modProject>/ModProject/SteamImg (promo-<version>/promo_<scenario>_<name>.jpg with the captions of
  scenarios.json, the other pictures);
- the promo entries of <modProject>/tests/scenarios.json (taken or not);
- the promotion screenshots ("promo-<name>") of every autotest run still in logs/autotest, to compare the takes;
- the rendered videos of marketing/<video>/out (the newest folder, or "promoVideoDir" of scripts/settings.json).
<repo> = --repo, default: the folder holding this ModLib submodule. The mod is named in <repo>/scripts/settings.json:
"modProject" (project id; default "releaseAllVersionProject") and "modName".
Run by collect-promo.ps1 after a promotion run (scripts/game/run-promo.ps1); can be run alone.

usage: python GuiGuBaHuang-ModLib/promo-viewer/update-data.py [--open] [--repo <mod repository>]
"""
import html, json, os, re, sys, time

here = os.path.dirname(os.path.abspath(__file__))
modlib = os.path.dirname(here)
repo = os.path.dirname(modlib)
if "--repo" in sys.argv:
    repo = os.path.abspath(sys.argv[sys.argv.index("--repo") + 1])


def load(p, default):
    try:
        return json.load(open(p, encoding="utf-8-sig"))
    except (OSError, ValueError):
        return default


def stamp(p):
    return time.strftime("%Y-%m-%d %H:%M", time.localtime(os.path.getmtime(p)))


settings = load(os.path.join(repo, "scripts", "settings.json"), {})
project = settings.get("modProject") or settings.get("releaseAllVersionProject") or ""
mod_name = settings.get("modName") or "Mod"
if not project:
    print(f"warning: no modProject in {os.path.join(repo, 'scripts', 'settings.json')}")
steam = os.path.join(repo, project, "ModProject", "SteamImg")

defs = load(os.path.join(repo, project, "tests", "scenarios.json"), {"scenarios": []})["scenarios"] if project else []
entries = []
for d in defs:
    for p in d.get("promo", []):
        # a video clip ("clip": true, logs/promo-clips/...mp4): not a picture of SteamImg
        if p.get("clip"):
            continue
        # promo-<version>/promo_<scenario>_<name>.jpg (collect-promo.ps1): one folder per version of the mod
        ver = d.get("promoVersion") or "other"
        entries.append({"name": p["name"], "caption": p.get("caption", ""), "scenario": d["id"], "step": p["step"],
                        "nth": p.get("nth", 1), "version": ver,
                        "file": f"promo-{ver}/promo_{d['id']}_{p['name']}.jpg"})
known = {e["file"] for e in entries}

pictures = []
if project and os.path.isdir(steam):
    for top, _dirs, names in os.walk(steam):
        for n in names:
            if not re.search(r"\.(png|jpe?g)$", n, re.I):
                continue
            full = os.path.join(top, n)
            f = os.path.relpath(full, steam).replace(os.sep, "/")
            m = re.match(r"promo_(?:([A-Z]{2}-\d+)_)?(.+)\.(?:png|jpe?g)$", n, re.I)
            vm = re.match(r"promo-([^/]+)/", f)
            pictures.append({"file": f, "name": m.group(2) if m else None, "scenario": m.group(1) if m else None,
                             "version": vm.group(1) if vm else None,
                             "size": os.path.getsize(full), "time": stamp(full), "stale": bool(m) and f not in known})
    pictures.sort(key=lambda x: x["file"])

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

# marketing videos: "promoVideoDir" (relative to <repo>) or the newest marketing/<video>/out
video_dir = settings.get("promoVideoDir")
if not video_dir:
    mk = os.path.join(repo, "marketing")
    outs = sorted(d for d in (os.listdir(mk) if os.path.isdir(mk) else []) if os.path.isdir(os.path.join(mk, d, "out")))
    video_dir = f"marketing/{outs[-1]}/out" if outs else ""
videos = []
out = os.path.join(repo, video_dir) if video_dir else ""
if out and os.path.isdir(out):
    for f in sorted(os.listdir(out)):
        if f.lower().endswith((".mp4", ".webm", ".png")):
            full = os.path.join(out, f)
            videos.append({"file": f, "size": os.path.getsize(full), "time": stamp(full)})

viewer = os.path.join(repo, "logs", "promo-viewer")   # next to the runs (logs/autotest)
dst = os.path.join(viewer, "data")
os.makedirs(dst, exist_ok=True)
# paths for the page in <repo>/logs/promo-viewer (with /: file:// and a local server rooted at <repo>)
rel = lambda p: os.path.relpath(p, viewer).replace(os.sep, "/")
data = {"generated": time.strftime("%Y-%m-%d %H:%M"), "modName": mod_name, "modProject": project,
        "steamDir": rel(steam) + "/", "runsDir": rel(auto) + "/",
        "videoDir": (video_dir.strip("/") + "/") if video_dir else "", "videoUrl": (rel(out) + "/") if out else "", "entries": entries, "pictures": pictures,
        "runs": runs, "videos": videos}
with open(os.path.join(dst, "promo.js"), "w", encoding="utf-8", newline="\n") as fh:
    fh.write("window.PROMO_DATA = " + json.dumps(data, ensure_ascii=False, separators=(",", ":")) + ";\n")
page = open(os.path.join(here, "index.html"), encoding="utf-8").read()
page = page.replace("{{MODLIB}}", rel(modlib)).replace("{{MOD_NAME}}", html.escape(mod_name))
with open(os.path.join(viewer, "index.html"), "w", encoding="utf-8", newline="\n") as fh:
    fh.write(page)
print(f"promo viewer: {len(pictures)} picture(s), {len(entries)} promo entries, {len(runs)} run(s) with takes, "
      f"{len(videos)} video file(s) -> {os.path.join(dst, 'promo.js')}")
if "--open" in sys.argv:
    os.startfile(os.path.join(viewer, "index.html"))
