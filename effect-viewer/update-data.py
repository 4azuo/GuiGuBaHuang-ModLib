"""Data of the effect viewer (effect-viewer/index.html): docs/Effects/<kind>/index.json -> effect-viewer/data/effects.js.
Run after a gallery, an extraction or an assessment of the effects (scripts/game/ of the main repository GuiGuBaHuang-Taoist).

usage: python effect-viewer/update-data.py [--open]
"""
import json, os, sys, time

repo = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
kinds = {}
for kind in ("Skill", "Unit", "Scenes"):
    p = os.path.join(repo, "docs", "Effects", kind, "index.json")
    if os.path.exists(p):
        kinds[kind] = json.load(open(p, encoding="utf-8-sig"))
out = os.path.join(repo, "effect-viewer", "data")
os.makedirs(out, exist_ok=True)
data = {"generated": time.strftime("%Y-%m-%d %H:%M"), "kinds": kinds}
with open(os.path.join(out, "effects.js"), "w", encoding="utf-8", newline="\n") as f:
    f.write("window.EFFECT_DATA = " + json.dumps(data, ensure_ascii=False, separators=(",", ":")) + ";\n")
print(", ".join(f"{k}: {len(v)}" for k, v in kinds.items()), "->", os.path.join(out, "effects.js"))
if "--open" in sys.argv:
    os.startfile(os.path.join(repo, "effect-viewer", "index.html"))
