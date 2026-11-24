"""Review strips of the recorded effects (GuiGuBaHuang-ModLib/docs/Effects/<kind>/review/strip-NNN.png, not in git) for the AI reviewer,
which can read one picture but not an animation: one row per effect, 12 frames from the moment it shows, one recorded
frame apart (a whole multiple of it for longer effects, to reach the moment it is gone), each with its time; 12 effects per strip, by index
(strip-000: 0000-0011).
The frames are whole (no crop): the camera is centred on the spot the effect starts from and zoomed to its size.

usage: python scripts/game/effect_strips.py [--kind Skill] [--per-strip 12] [--start 0]
"""
import argparse, json, math, os
from PIL import Image, ImageChops, ImageDraw, ImageFont

FRAMES, TW, TH, LABEL = 12, 160, 90, 16


def shown_frames(path):
    """The frames of a webp with their times (ms), and the first / last frame where the effect shows."""
    im = Image.open(path)
    frames, times, t = [], [], 0
    for i in range(im.n_frames):
        im.seek(i)
        frames.append(im.convert("RGB"))
        times.append(t)
        t += im.info.get("duration", 100) or 100
    # the ground (no effect): the first or the last frame, the plainer one (effects add light and colour) - an effect
    # may already show in the first frame and loop to the last one
    def energy(f):
        h = f.resize((160, 90)).convert("HSV")
        return sum(s * v for _, s, v in h.getdata())
    ground = min(frames[0], frames[-1], key=energy).resize((160, 90))
    changed = []
    for f in frames:
        d = ImageChops.difference(f.resize((160, 90)), ground).convert("L").point(lambda v: 255 if v > 24 else 0)
        changed.append(sum(d.histogram()[255:]))
    # shown: a change of at least a tenth of the biggest one (small far sparks / map edge noise left out)
    limit = max(12, max(changed) * 0.1)
    shown = [i for i, c in enumerate(changed) if c >= limit]
    first, last = (shown[0], shown[-1]) if shown else (0, len(frames) - 1)
    return frames, times, first, last


def pick(first, last, total, n=FRAMES):
    """n frames from the first one shown, one recorded frame apart (the fixed step, ~110 ms), a whole multiple of it
    (the nearest) for longer effects, so the row reaches about the moment the effect is gone; always n when the
    recording is long enough."""
    step = max(1, round((last - first + 1) / n))
    start = max(0, min(first, total - 1 - step * (n - 1)))
    return [i for i in range(start, total, step)][:n]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--kind", default="Skill")
    ap.add_argument("--per-strip", type=int, default=12)
    ap.add_argument("--start", type=int, default=0, help="first strip to make (resume)")
    a = ap.parse_args()
    base = os.path.join(os.path.dirname(__file__), "..", "..", "docs", "Effects", a.kind)
    index = json.load(open(os.path.join(base, "index.json"), encoding="utf-8-sig"))
    todo = [e for e in index if e.get("file") and e.get("visible")]
    out = os.path.join(base, "review")
    os.makedirs(out, exist_ok=True)
    try:
        font = ImageFont.truetype("arial.ttf", 13)
        small = ImageFont.truetype("arial.ttf", 11)
    except Exception:
        font = small = ImageFont.load_default()
    # strip N holds the effects of index N*12 .. N*12+11 (fixed: an effect is always in the same strip), the ones not
    # shown in game (no webp) left out
    count = math.ceil((max(e["index"] for e in index) + 1) / a.per_strip)
    for s in range(a.start, count):
        group = [e for e in todo if s * a.per_strip <= e["index"] < (s + 1) * a.per_strip]
        if not group:
            continue
        img = Image.new("RGB", (FRAMES * TW, len(group) * (TH + LABEL)), (24, 24, 24))
        d = ImageDraw.Draw(img)
        for r, e in enumerate(group):
            y = r * (TH + LABEL)
            frames, times, first, last = shown_frames(os.path.join(base, e["file"]))
            d.text((4, y + 2), f"{e['index']:04d} {e['name']}", fill=(255, 220, 120), font=font)
            for c, i in enumerate(pick(first, last, len(frames))):
                img.paste(frames[i].resize((TW - 2, TH)), (c * TW, y + LABEL))
                d.text((c * TW + 4, y + LABEL + 2), f"{times[i] - times[first]} ms", fill=(255, 255, 255), font=small)
        img.save(os.path.join(out, f"strip-{s:03d}.png"))
        if (s + 1) % 10 == 0:
            print(f"{s + 1}/{count}")
    print(f"{count} strips -> {out}")


if __name__ == "__main__":
    main()
