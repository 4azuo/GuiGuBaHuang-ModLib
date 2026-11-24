"""Effect galleries (TE-01/02/03) of an autotest run -> GuiGuBaHuang-ModLib/docs/Effects (in git).

For each effect recorded by the run (<run>/effects/<TE-id>/<NNNN>_<name>/f###.jpg + frames.txt):
- GuiGuBaHuang-ModLib/docs/Effects/<Skill|Unit|Scenes>/<NNNN>_<name>.webp: animated, the game area (no window title bar), 960x540, the real
  timing of the frames;
- GuiGuBaHuang-ModLib/docs/Effects/<kind>/index.json: one entry per effect with the measures of the game (<run>/effects-meta.jsonl:
  castingTimeMs, durationMs, loop, direction, width, height, zoom, camera, visible) and the classification fields
  (element, shape, range, color, motion, tags, note), kept from the former index.
The fresh effect lists of the game (<run>/_Def_Res_Efx_Battle_*.txt) replace ModConf's (added / removed printed).
The frames are deleted once the webp is made (--keep-frames to keep them).
Runs of several game slots may collect at the same time: the index and the lists are updated under a lock.

usage: python scripts/game/collect_effects.py <run folder> [--width 960] [--quality 60] [--keep-frames]
"""
import argparse, contextlib, json, os, shutil, sys, time
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import _modrepo  # noqa: E402

KINDS = {"TE-01": "Skill", "TE-02": "Unit", "TE-03": "Scenes"}
LISTS = {"Skill": "_Def_Res_Efx_Battle_Skill.txt", "Unit": "_Def_Res_Efx_Battle_Unit.txt", "Scenes": "_Def_Res_Efx_Battle_Scenes.txt"}
MEASURES = ["visible", "castingTimeMs", "durationMs", "loop", "direction", "width", "height", "zoom", "camera"]
# kept as they are when the measures are written again: the AI assessment (assess_effects.py, label_effects.py), the
# tester's own (user-*: effect viewer) and
# the asset read from the game files (extract_effects.py)
KEPT = ("ai-", "user-", "asset")
BORDER = 8


@contextlib.contextmanager
def locked(path, timeout=600):
    """Exclusive lock through a lock file next to `path`."""
    lock = path + ".lock"
    start = time.time()
    while True:
        try:
            fd = os.open(lock, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
            break
        except FileExistsError:
            if time.time() - start > timeout:   # left by a killed run
                os.remove(lock)
                start = time.time()
                continue
            time.sleep(0.5)
    try:
        yield
    finally:
        os.close(fd)
        os.remove(lock)


# long values at the end of a line (not padded)
LAST = ("ai-note", "ai-skill-ideas", "user-note", "user-skill-ideas", "asset")


def dump_aligned(entries, f, last=LAST):
    """
    A JSON array with one object a line, the key/value pairs in columns: every object gets every key (null when
    missing) in the same order (the keys of `last` at the end, not padded: long nested values).
    """
    keys = []
    for e in entries:
        for k in e:
            if k not in keys and k not in last:
                keys.append(k)
    keys += [k for k in last if any(k in e for e in entries)]
    cells = [[f'"{k}": ' + json.dumps(e.get(k), ensure_ascii=False, separators=(", ", ": ")) for k in keys] for e in entries]
    widths = [max((len(row[i]) for row in cells), default=0) for i in range(len(keys))]
    f.write("[\n")
    for n, row in enumerate(cells):
        parts = [cell + "," + " " * (widths[i] - len(cell)) if i < len(row) - 1 else cell for i, cell in enumerate(row)]
        f.write("  {" + " ".join(parts) + "}" + (",\n" if n < len(cells) - 1 else "\n"))
    f.write("]\n")


def game_area(im):
    """The game area of a window capture: without the title bar and the borders (16:9 at the bottom)."""
    w = im.width - 2 * BORDER
    h = int(w * 9 / 16)
    top = im.height - BORDER - h
    if w <= 0 or top < 0:
        return im
    return im.crop((BORDER, top, BORDER + w, top + h))


def make_webp(frames_dir, out, width, quality):
    files = sorted(f for f in os.listdir(frames_dir) if f.endswith(".jpg"))
    if not files:
        return False
    times = []
    tf = os.path.join(frames_dir, "frames.txt")
    if os.path.exists(tf):
        times = [int(x) for x in open(tf).read().split() if x.strip()]
    imgs = []
    for f in files:
        im = game_area(Image.open(os.path.join(frames_dir, f)).convert("RGB"))
        imgs.append(im.resize((width, int(width * im.height / im.width)), Image.LANCZOS))
    if len(times) == len(imgs):
        durs = [max(40, times[i + 1] - times[i]) if i + 1 < len(times) else 100 for i in range(len(imgs))]
    else:
        durs = [100] * len(imgs)
    imgs[0].save(out, save_all=True, append_images=imgs[1:], duration=durs, loop=0, quality=quality, method=4)
    return True


def write_index(idx_file, items, made_files):
    """index.json updated with the measures of `items` (the ai-* fields and the asset kept)."""
    index = {}
    if os.path.exists(idx_file):
        for e in json.load(open(idx_file, encoding="utf-8-sig")):
            index[e["name"].lower()] = e   # extract_effects.py writes the names of the game's catalogue (lower case)
    for m in items:
        old = index.get(m["name"].lower(), {})
        e = {"index": m["index"], "name": m["name"], "path": m["path"], "file": made_files.get(m["name"], old.get("file"))}
        for k in MEASURES:
            e[k] = m.get(k)
        e.update({k: v for k, v in old.items() if k.startswith(KEPT)})
        index[m["name"].lower()] = e
    lst = sorted(index.values(), key=lambda x: x["index"])
    with open(idx_file, "w", encoding="utf-8", newline="\n") as f:
        dump_aligned(lst, f, last=LAST)


def update_lists(repo, run):
    """The fresh lists of the game (<run>/_Def_Res_Efx_Battle_*.txt) -> ModConf of ModLib (3385996759)."""
    conf = os.path.join(_modrepo.MODLIB, "3385996759", "ModProject", "ModConf")   # the lists of the galleries (ModLib)
    for name in LISTS.values():
        fresh = os.path.join(run, name)
        if not os.path.exists(fresh):
            continue
        new = [x.strip() for x in open(fresh, encoding="utf-8-sig") if x.strip()]
        target = os.path.join(conf, name)
        with locked(target):
            old = [x.strip() for x in open(target, encoding="utf-8-sig") if x.strip()] if os.path.exists(target) else []
            if new == old:
                continue
            added = sorted(set(new) - set(old))
            removed = sorted(set(old) - set(new))
            with open(target, "w", encoding="utf-8", newline="\r\n") as f:
                f.write("\n".join(new) + "\n")
        print(f"{name}: {len(new)} effects in the game ({len(added)} added, {len(removed)} removed since the former list)")
        for x in added[:20]:
            print("  +", x)
        for x in removed[:20]:
            print("  -", x)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("run")
    ap.add_argument("--width", type=int, default=960)
    ap.add_argument("--quality", type=int, default=60)
    ap.add_argument("--keep-frames", action="store_true")
    a = ap.parse_args()
    repo = _modrepo.repo()
    run = os.path.abspath(a.run)
    metas = []
    mf = os.path.join(run, "effects-meta.jsonl")
    if os.path.exists(mf):
        metas = [json.loads(l) for l in open(mf, encoding="utf-8") if l.strip()]
    if not metas:
        print("No effect measures in", run)
    by_kind = {}
    for m in metas:
        by_kind.setdefault(KINDS.get(m["id"], "Other"), []).append(m)
    made = 0
    for kind, items in by_kind.items():
        out = os.path.join(_modrepo.EFFECTS, kind)
        os.makedirs(out, exist_ok=True)
        # the pictures first (slow, no lock), then the index (locked read-modify-write)
        made_files = {}
        for m in items:
            tag = "%04d_%s" % (m["index"], m["name"])
            frames = os.path.join(run, "effects", m["id"], tag)
            if os.path.isdir(frames) and make_webp(frames, os.path.join(out, tag + ".webp"), a.width, a.quality):
                made_files[m["name"]] = tag + ".webp"
                made += 1
                if not a.keep_frames:
                    shutil.rmtree(frames, ignore_errors=True)
        idx_file = os.path.join(out, "index.json")
        with locked(idx_file):
            write_index(idx_file, items, made_files)
        print(f"{kind}: {len(items)} effect(s) -> {out}")
    print("webp made:", made)
    update_lists(repo, run)


if __name__ == "__main__":
    sys.exit(main())
