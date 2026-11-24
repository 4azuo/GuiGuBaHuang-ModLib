"""Assessment of the recorded effects (GuiGuBaHuang-ModLib/docs/Effects/<kind>): picture analysis of every webp + the labels of the AI
reviewer kept (label_effects.py). Every field added here starts with "ai-" (assessed, not measured).

1. analyse (automatic): the effect separated from the ground (the first frame is the ground, before the effect):
   - ai-colors: main colours of the effect (hue / brightness of its pixels at its biggest frame);
   - ai-motion: static, expanding, shrinking, moving-<direction>, falling, rising, pulsing (area and centre over time);
   - ai-coverage: share of the screen the effect covers at its biggest (0..1);
   - a crop of its biggest frame -> contact sheets (GuiGuBaHuang-ModLib/docs/Effects/<kind>/review/sheet-NN.png, not in git) to review.
2. labels of the AI reviewer (ai-element, ai-usage, ai-shape, ai-size, ai-tags, ai-note, ai-skill-ideas): written in
   index.json by label_effects.py; this script only updates its own fields (ai-colors, ai-motion, ai-coverage).

usage: python scripts/game/assess_effects.py [--kind Skill] [--sheets] [--per-sheet 30]
"""
import argparse, colorsys, json, math, os, sys
from PIL import Image, ImageChops, ImageDraw, ImageFont

sys.path.insert(0, os.path.dirname(__file__))
import _modrepo  # noqa: E402
from collect_effects import locked, dump_aligned  # noqa: E402

W, H = 240, 135   # analysis size
DIRS = ["right", "up-right", "up", "up-left", "left", "down-left", "down", "down-right"]


def colour_name(r, g, b):
    h, l, s = colorsys.rgb_to_hls(r / 255, g / 255, b / 255)
    if l > 0.85 or (s < 0.18 and l > 0.6):
        return "white"
    if l < 0.15:
        return "black"
    if s < 0.18:
        return "grey"
    d = h * 360
    if d < 15 or d >= 340:
        return "red"
    if d < 40:
        return "orange" if l > 0.35 else "brown"
    if d < 65:
        return "yellow"
    if d < 160:
        return "green"
    if d < 195:
        return "cyan"
    if d < 255:
        return "blue"
    if d < 290:
        return "purple"
    return "pink"


def analyse(path):
    im = Image.open(path)
    frames = []
    for i in range(im.n_frames):
        im.seek(i)
        frames.append(im.convert("RGB"))
    ground = frames[0].resize((W, H))
    areas, centres, boxes = [], [], []
    for f in frames:
        small = f.resize((W, H))
        diff = ImageChops.difference(small, ground).convert("L").point(lambda v: 255 if v > 28 else 0)
        box = diff.getbbox()
        px = diff.load()
        n = sx = sy = 0
        for y in range(0, H, 2):
            for x in range(0, W, 2):
                if px[x, y]:
                    n += 1; sx += x; sy += y
        areas.append(n / ((W // 2) * (H // 2)))
        centres.append((sx / n, sy / n) if n else None)
        boxes.append(box)
    peak = max(range(len(frames)), key=lambda i: areas[i])
    shown = [i for i, a in enumerate(areas) if a > 0.002]
    # colours at the biggest frame (pixels of the effect)
    big = frames[peak].resize((W, H))
    mask = ImageChops.difference(big, ground).convert("L")
    counts = {}
    bp, mp = big.load(), mask.load()
    for y in range(0, H, 2):
        for x in range(0, W, 2):
            if mp[x, y] > 28:
                c = colour_name(*bp[x, y])
                counts[c] = counts.get(c, 0) + 1
    total = sum(counts.values()) or 1
    colours = [c for c, k in sorted(counts.items(), key=lambda kv: -kv[1]) if k / total >= 0.12 and c not in ("grey",)][:3]
    # motion
    motion = "static"
    if len(shown) >= 2:
        a0, a1, amax = areas[shown[0]], areas[shown[-1]], max(areas)
        c0 = next((centres[i] for i in shown if centres[i]), None)
        c1 = next((centres[i] for i in reversed(shown) if centres[i]), None)
        move = math.dist(c0, c1) / W if c0 and c1 else 0
        if move > 0.12:
            dx, dy = c1[0] - c0[0], c0[1] - c1[1]
            motion = "moving-" + DIRS[int(round(math.degrees(math.atan2(dy, dx)) / 45)) % 8]
            if motion == "moving-down":
                motion = "falling"
            elif motion == "moving-up":
                motion = "rising"
        elif amax > 3 * max(a0, 1e-4) and areas.index(amax) > shown[0] + 1:
            motion = "expanding" if a1 > 0.5 * amax else "burst"
        elif a0 > 2 * max(a1, 1e-4):
            motion = "shrinking"
        else:
            swings = sum(1 for i in range(1, len(areas)) if abs(areas[i] - areas[i - 1]) > 0.3 * max(amax, 1e-4))
            motion = "pulsing" if swings >= 4 else "static"
    # crop of the biggest frame around the union of the boxes (full size)
    union = None
    for b in boxes:
        if b:
            union = b if union is None else (min(union[0], b[0]), min(union[1], b[1]), max(union[2], b[2]), max(union[3], b[3]))
    fw, fh = frames[peak].size
    sxr, syr = fw / W, fh / H
    if union:
        x0, y0, x1, y1 = union[0] * sxr, union[1] * syr, union[2] * sxr, union[3] * syr
        side = max(x1 - x0, y1 - y0) * 1.15 + 20
        cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
        crop = frames[peak].crop((int(cx - side / 2), int(cy - side / 2), int(cx + side / 2), int(cy + side / 2)))
    else:
        crop = frames[peak]
    return {"ai-colors": colours, "ai-motion": motion, "ai-coverage": round(max(areas), 3)}, crop


def sheet(tiles, out, cols=6, tile=220):
    rows = math.ceil(len(tiles) / cols)
    img = Image.new("RGB", (cols * tile, rows * (tile + 18)), (24, 24, 24))
    d = ImageDraw.Draw(img)
    try:
        font = ImageFont.truetype("arial.ttf", 12)
    except Exception:
        font = ImageFont.load_default()
    for i, (label, crop) in enumerate(tiles):
        x, y = (i % cols) * tile, (i // cols) * (tile + 18)
        c = crop.copy()
        c.thumbnail((tile - 4, tile - 4))
        img.paste(c, (x + (tile - c.width) // 2, y + 18 + (tile - c.height) // 2))
        d.text((x + 3, y + 2), label[:34], fill=(255, 220, 120), font=font)
    img.save(out)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--kind", default="Skill")
    ap.add_argument("--sheets", action="store_true")
    ap.add_argument("--per-sheet", type=int, default=30)
    a = ap.parse_args()
    base = os.path.join(_modrepo.EFFECTS, a.kind)
    idx_file = os.path.join(base, "index.json")
    index = json.load(open(idx_file, encoding="utf-8-sig"))
    todo = [e for e in index if e.get("file") and e.get("visible")]
    results, tiles = {}, []
    for n, e in enumerate(todo):
        try:
            r, crop = analyse(os.path.join(base, e["file"]))
            results[e["name"].lower()] = r
            tiles.append((f"{e['index']:04d} {e['name']}", crop))
        except Exception as ex:
            print("error", e["name"], ex)
        if (n + 1) % 100 == 0:
            print(f"{n + 1}/{len(todo)}")
    if a.sheets:
        rev = os.path.join(base, "review")
        os.makedirs(rev, exist_ok=True)
        for i in range(0, len(tiles), a.per_sheet):
            sheet(tiles[i:i + a.per_sheet], os.path.join(rev, f"sheet-{i // a.per_sheet:02d}.png"))
        print("sheets:", math.ceil(len(tiles) / a.per_sheet), "->", rev)
    auto = ("ai-colors", "ai-motion", "ai-coverage")
    with locked(idx_file):
        index = json.load(open(idx_file, encoding="utf-8-sig"))
        for e in index:
            k = e["name"].lower()
            if k in results:
                for f in auto:
                    e.pop(f, None)
                e.update(results[k])
        with open(idx_file, "w", encoding="utf-8", newline="\n") as f:
            dump_aligned(index, f)
    print(f"{a.kind}: {len(results)} analysed, {sum(1 for e in index if e.get('ai-element'))} labelled")


if __name__ == "__main__":
    main()
