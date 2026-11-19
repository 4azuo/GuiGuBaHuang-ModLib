"""Labels of the AI reviewer -> docs/Effects/<kind>/index.json (fields "ai-*").

Families of effects named by a pattern get their labels by rule (attack_family); the others are written by the reviewer
(per sheet) with set_labels. Field names without the "ai-" prefix here; it is added when written.

ai-element: the 12 attributes of the game (blade, spear, sword, fist, palm, finger, fire, frost, thunder, wind, earth,
wood) + extended (water, light, dark, poison, blood, spirit, none).
ai-usage: projectile, beam, chain, slash, thrust, hit, explosion, aoe, field, rain, buff, debuff, shield, aura, heal,
summon, trail, cast, charge, status-icon, teleport, decoration.
"""
import json, os, re, sys

BASE = os.path.join(os.path.dirname(__file__), "..", "..", "docs", "Effects")

WEAPONS = {  # pinyin -> (element, usage, shape, what)
    "dao": ("blade", ["slash", "hit"], "arc", "crescent blade slash"),
    "jian": ("sword", ["thrust", "hit"], "sword", "flying sword thrust"),
    "qiang": ("spear", ["thrust", "hit"], "spike", "spear thrust streak"),
    "quan": ("fist", ["hit"], "fist", "fist punch"),
    "zhang": ("palm", ["hit", "projectile"], "palm", "palm strike"),
    "zhi": ("finger", ["projectile", "hit"], "line", "finger beam strike"),
}
ATTRS = {"bing": "frost", "feng": "wind", "huo": "fire", "lei": "thunder", "mu": "wood", "tu": "earth"}


def attack_family(name):
    """AttackDao / AttackJianHuo...: the basic attacks of a weapon, with an attribute."""
    m = re.match(r"^attack(dao|jian|qiang|quan|zhang|zhi)(bing|feng|huo|lei|mu|tu)?$", name.lower())
    if not m:
        return None
    el, usage, shape, what = WEAPONS[m.group(1)]
    attr = ATTRS.get(m.group(2) or "")
    elements = [el] + ([attr] if attr else [])
    return {
        "element": elements, "usage": usage, "shape": shape, "size": "small",
        "tags": ["basic-attack", "weapon"] + ([attr] if attr else []),
        "note": f"Basic {el} attack: {what}" + (f", {attr} variant" if attr else ""),
        "skill-ideas": [f"{el} weapon basic attack" + (f" ({attr})" if attr else ""), "hit effect of a melee skill"],
    }


def set_labels(kind, entries):
    """Labels {name: {field: value}} written into docs/Effects/<kind>/index.json (fields prefixed with "ai-")."""
    sys.path.insert(0, os.path.dirname(__file__))
    from collect_effects import locked, dump_aligned
    p = os.path.join(BASE, kind, "index.json")
    want = {k.lower(): v for k, v in entries.items()}
    with locked(p):
        index = json.load(open(p, encoding="utf-8-sig"))
        for e in index:
            v = want.get(e["name"].lower())
            if v:
                e.update({f if f.startswith("ai-") else "ai-" + f: x for f, x in v.items()})
        with open(p, "w", encoding="utf-8", newline="\n") as f:
            dump_aligned(index, f)
    return sum(1 for e in index if e.get("ai-element"))


if __name__ == "__main__":
    kind = sys.argv[1] if len(sys.argv) > 1 else "Skill"
    index = json.load(open(os.path.join(BASE, kind, "index.json"), encoding="utf-8-sig"))
    fam = {e["name"]: attack_family(e["name"]) for e in index if e.get("visible") and e.get("file")}
    print(set_labels(kind, {k: v for k, v in fam.items() if v}), "labels")
