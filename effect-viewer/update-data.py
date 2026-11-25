"""Data of the effect viewer (effect-viewer/index.html): docs/Effects/<kind>/index.json -> effect-viewer/data/effects.js,
with the skill icons of the game and their tags (docs/GameAssets/SkillIcons/tags.json, skills.json: extract-game-assets.py
--skills): the detail of an effect shows the icons of the martial arts that suit its elements.
Run after a gallery, an extraction or an assessment of the effects (scripts/game/: the galleries TE-xx run from a mod repository).

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
icons = {}
sk = os.path.join(repo, "docs", "GameAssets", "SkillIcons")
if os.path.exists(os.path.join(sk, "tags.json")):
    tags = json.load(open(os.path.join(sk, "tags.json"), encoding="utf-8")).get("tags", {})
    uses = json.load(open(os.path.join(sk, "skills.json"), encoding="utf-8")) if os.path.exists(os.path.join(sk, "skills.json")) else {}
    icons = {n: {"tags": t, "kinds": sorted({u["kind"] for u in uses.get(n, [])})} for n, t in tags.items()}
data = {"generated": time.strftime("%Y-%m-%d %H:%M"), "kinds": kinds, "skillIcons": icons}
with open(os.path.join(out, "effects.js"), "w", encoding="utf-8", newline="\n") as f:
    f.write("window.EFFECT_DATA = " + json.dumps(data, ensure_ascii=False, separators=(",", ":")) + ";\n")
print(", ".join(f"{k}: {len(v)}" for k, v in kinds.items()), f"skill icons: {len(icons)}", "->", os.path.join(out, "effects.js"))
if "--open" in sys.argv:
    os.startfile(os.path.join(repo, "effect-viewer", "index.html"))
