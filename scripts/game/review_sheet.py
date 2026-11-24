"""Facts of the effects of one review strip (GuiGuBaHuang-ModLib/docs/Effects/<kind>/review/strip-NNN.png: index N*12 .. N*12+11), for the AI reviewer: the measures,
the picture analysis and the asset summary of each tile, one line each.

usage: python scripts/game/review_sheet.py <strip number> [--kind Skill] [--per-sheet 12]
"""
import argparse, json, os

ap = argparse.ArgumentParser()
ap.add_argument("sheet", type=int)
ap.add_argument("--kind", default="Skill")
ap.add_argument("--per-sheet", type=int, default=12)
a = ap.parse_args()
base = os.path.join(os.path.dirname(__file__), "..", "..", "docs", "Effects", a.kind)
index = json.load(open(os.path.join(base, "index.json"), encoding="utf-8-sig"))
todo = [e for e in index if e.get("file") and e.get("visible")]
for e in [e for e in todo if a.sheet * a.per_sheet <= e['index'] < (a.sheet + 1) * a.per_sheet]:
    s = e.get("asset") or {}
    scripts = [x.replace("Ctrl", "").replace("Effect", "") for x in s.get("scripts", []) if x not in ("EffectAutoSortOrderCtrl", "PlaySoundsCtrl", "LinkObjCacheCtrl", "EffectSortOrderDataCtrl")]
    shapes = sorted({p.get("shape") for p in s.get("systems", []) if p.get("shape")})
    print(f"{e['index']:04d} {e['name']:<30} {e.get('ai-motion','?'):<16} {','.join(e.get('ai-colors', [])):<20} "
          f"cov {e.get('ai-coverage')} {e.get('width')}x{e.get('height')} dir {e.get('direction')} "
          f"dur {s.get('durationMs')} ps {len(s.get('systems', []))} {','.join(shapes)} | {','.join(scripts)}")
