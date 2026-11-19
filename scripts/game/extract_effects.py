"""Effects read from the game files (offline, read only): the prefabs of Effect/Battle/<Skill|Unit|Scenes> in
resources.assets (catalogue: ResourceManager of globalgamemanagers) -> docs/Effects/<kind>/index.json, "asset" of each
effect (kept next to the recorded measures and the classification fields of collect_effects.py):

- castingTimeMs / durationMs from the ParticleSystem settings (exact, unlike the samples of the recording): the first
  start delay, and the end of the longest system (delay + length + max lifetime); -1 when a system loops or its particles
  live "forever" (lifetime >= 1000 s); loop: true then;
- systems: every ParticleSystem (path in the prefab, length, loop, delay, lifetime / size / speed as [min, max],
  maxParticles, shape type and radius);
- scripts: the MonoBehaviour classes of the prefab (EffectCmdCtrl, UnitCmdCtrl...: what drives it);
- objects: the number of objects of the prefab.
The ParticleSystem settings cannot be read in the game (their MainModule accessors are stripped from it).

usage: python scripts/game/extract_effects.py [--kind Skill,Unit,Scenes] [--game <game folder>] [--limit N]
"""
import argparse, json, os, struct, sys, time
import UnityPy

sys.path.insert(0, os.path.dirname(__file__))
from collect_effects import locked, dump_aligned  # noqa: E402

KINDS = {"Skill": "effect/battle/skill/", "Unit": "effect/battle/unit/", "Scenes": "effect/battle/scenes/"}
FOREVER = 1000.0   # a lifetime of 1000 s or more: the particles do not die (the effect is removed by its skill)
SHAPES = {0: "sphere", 1: "sphere-shell", 2: "hemisphere", 3: "hemisphere-shell", 4: "cone", 5: "box", 6: "mesh",
          7: "cone-shell", 8: "cone-volume", 9: "cone-volume-shell", 10: "circle", 11: "circle-edge", 12: "edge",
          13: "mesh-renderer", 14: "skinned-mesh", 15: "box-shell", 16: "box-edge", 17: "donut", 18: "rectangle", 19: "sprite",
          20: "sprite-renderer"}


def find_game(arg):
    for p in [arg, os.path.join(os.environ.get("LOCALAPPDATA", ""), "ggbh-game-link"), r"C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒"]:
        if p and os.path.exists(os.path.join(p, "guigubahuang_Data", "resources.assets")):
            return os.path.join(p, "guigubahuang_Data")
    sys.exit("game folder not found (--game)")


def curve(c):
    """[min, max] of a MinMaxCurve (constant, curve or two constants / curves: the scalars)."""
    if not isinstance(c, dict):
        return [c, c]
    state, s, mn = c.get("minMaxState", 0), c.get("scalar", 0.0), c.get("minScalar", 0.0)
    if state == 3:  # two constants
        return [round(min(s, mn), 3), round(max(s, mn), 3)]
    return [round(s, 3), round(s, 3)]


class Reader:
    def __init__(self, data):
        t = time.time()
        self.env = UnityPy.load(os.path.join(data, "globalgamemanagers"), os.path.join(data, "globalgamemanagers.assets"),
                                os.path.join(data, "resources.assets"))
        self.files = {os.path.basename(k).lower(): f for k, f in self.env.files.items()}
        self.res = self.files["resources.assets"]
        rm = next(o for o in self.files["globalgamemanagers"].objects.values() if o.type.name == "ResourceManager").read_typetree()
        self.catalogue = {c[0]: c[1]["m_PathID"] for c in rm["m_Container"]}
        self.scripts = {}
        print(f"game files read in {time.time() - t:.0f} s; {len(self.catalogue)} resources")

    def script_name(self, o):
        """Class of a MonoBehaviour: its m_Script pointer at a fixed offset (after m_GameObject and m_Enabled)."""
        try:
            raw = o.get_raw_data()
            fid, pid = struct.unpack_from("<iq", raw, 16)
            key = (fid, pid)
            if key not in self.scripts:
                f = self.res if fid == 0 else self.files.get(os.path.basename(self.res.externals[fid - 1].path).lower())
                sc = f.objects[pid].read_typetree() if f else {}
                self.scripts[key] = sc.get("m_ClassName", "?")
            return self.scripts[key]
        except Exception:
            return "?"

    def effect(self, path_id):
        objs = self.res.objects
        systems, scripts, count = [], set(), 0

        def walk(go_id, path):
            nonlocal count
            go = objs[go_id].read_typetree()
            count += 1
            here = f"{path}/{go['m_Name']}" if path else go["m_Name"]
            transform = None
            for c in go["m_Component"]:
                o = objs.get(c["component"]["m_PathID"])
                if o is None:
                    continue
                t = o.type.name
                if t in ("Transform", "RectTransform"):
                    transform = o.read_typetree()
                elif t == "ParticleSystem":
                    try:
                        ps = o.read_typetree()
                        init = ps.get("InitialModule", {})
                        shape = ps.get("ShapeModule", {})
                        radius = shape.get("radius")
                        systems.append({
                            "path": here, "length": round(ps.get("lengthInSec", 0.0), 3), "loop": bool(ps.get("looping")),
                            "delay": curve(ps.get("startDelay"))[1], "lifetime": curve(init.get("startLifetime")),
                            "size": curve(init.get("startSize")), "speed": curve(init.get("startSpeed")),
                            "maxParticles": init.get("maxNumParticles"),
                            "shape": SHAPES.get(shape.get("type"), shape.get("type")) if shape.get("enabled", True) else None,
                            "radius": round((radius.get("value") if isinstance(radius, dict) else radius) or 0.0, 3),
                        })
                    except Exception as e:
                        systems.append({"path": here, "error": repr(e)[:120]})
                elif t == "MonoBehaviour":
                    scripts.add(self.script_name(o))
            for ch in (transform or {}).get("m_Children", []):
                child = objs.get(ch["m_PathID"])
                if child is not None:
                    walk(child.read_typetree()["m_GameObject"]["m_PathID"], here)

        walk(path_id, "")
        ok = [s for s in systems if "error" not in s]
        loop = any(s["loop"] or s["lifetime"][1] >= FOREVER for s in ok)
        cast = int(min((s["delay"] for s in ok), default=0) * 1000) if ok else None
        end = max((s["delay"] + s["length"] + s["lifetime"][1] for s in ok), default=0)
        return {
            "castingTimeMs": cast,
            "durationMs": (-1 if loop else int(end * 1000) - (cast or 0)) if ok else None,
            "loop": loop if ok else None,
            "systems": systems, "scripts": sorted(scripts), "objects": count,
        }


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--kind", default="Skill,Unit,Scenes")
    ap.add_argument("--game")
    ap.add_argument("--limit", type=int, default=0)
    a = ap.parse_args()
    repo = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
    reader = Reader(find_game(a.game))
    for kind in [k.strip() for k in a.kind.split(",") if k.strip()]:
        prefix = KINDS[kind]
        names = sorted((k for k in reader.catalogue if k.startswith(prefix) and "/" not in k[len(prefix):]), key=str.lower)
        if a.limit:
            names = names[:a.limit]
        t = time.time()
        assets = {}
        for key in names:
            name = key[len(prefix):]
            try:
                assets[name.lower()] = reader.effect(reader.catalogue[key])
            except Exception as e:
                assets[name.lower()] = {"error": repr(e)[:160]}
        out = os.path.join(repo, "docs", "Effects", kind)
        os.makedirs(out, exist_ok=True)
        idx_file = os.path.join(out, "index.json")
        with locked(idx_file):
            index = json.load(open(idx_file, encoding="utf-8-sig")) if os.path.exists(idx_file) else []
            by_name = {e["name"].lower(): e for e in index}
            for i, key in enumerate(names):
                name = key[len(prefix):]
                e = by_name.get(name.lower())
                if e is None:
                    # not recorded yet: an entry with its asset only (collect_effects.py adds the measures later)
                    e = {"index": i, "name": name, "path": "Effect\\Battle\\" + kind + "\\" + name}
                    index.append(e)
                    by_name[name.lower()] = e
                e["asset"] = assets[name.lower()]
            index.sort(key=lambda x: x.get("index", 0))
            with open(idx_file, "w", encoding="utf-8", newline="\n") as f:
                dump_aligned(index, f)  # LAST: the long fields at the end
        loops = sum(1 for v in assets.values() if v.get("loop"))
        errors = sum(1 for v in assets.values() if "error" in v)
        print(f"{kind}: {len(names)} effects in {time.time() - t:.0f} s ({loops} looping, {errors} errors) -> {idx_file}")


if __name__ == "__main__":
    main()
