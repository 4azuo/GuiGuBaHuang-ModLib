"""Elements of the recorded effects (docs/Effects/<Kind>/index.json, "ai-element") limited to the 12 attributes of the game
(weapons: blade, spear, sword, fist, palm, finger; roots: fire, frost, thunder, wind, earth, wood). The elements read by AI
from the pictures had more (water, light, dark, spirit, blood, poison, none): an effect keeps its game attributes (water is
frost); one without any gets one from its name (pinyin), note and tags, else from its colors, else wind. "element-source"
says how ("ai", "keyword", "color", "default"); "ai-element-raw" keeps what the AI read.

usage: python scripts/classify-effect-elements.py [--dry]   (prints the counts and the effects to review: color / default)
"""
import json, os, re, sys, collections

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, ".."))
GAME = ["blade", "spear", "sword", "fist", "palm", "finger", "fire", "frost", "thunder", "wind", "earth", "wood"]
ALIAS = {"water": "frost", "ice": "frost", "poison": "wood"}

# whole segments of the name (pinyin; "zhangsan", "zhizhu" are not palm, finger: segments only) -> attribute
NAME_SEGMENTS = {"jian": "sword", "qiang": "spear", "huo": "fire", "bing": "frost", "lei": "thunder", "feng": "wind"}
# words of the note, tags and ideas (English) -> attribute (first match in this order)
KEYWORDS = [
    ("sword", r"\bswords?\b|\bsword-|\bflying-sword"),
    ("spear", r"\bspears?\b|\blance|\bthrust|\bpike"),
    ("fist", r"\bfists?\b|\bpunch"),
    ("palm", r"\bpalms?\b"),
    ("finger", r"\bfingers?\b|\bneedles?\b|\barrows?\b|\bbeams?\b|\brays?\b|\bpierc"),
    ("blade", r"\bblades?\b|\bslash|\bclaws?\b|\bcut\b|\bscratch|\bcrescent|\bdagger"),
    ("fire", r"\bflames?\b|\bfire|\bburn|\bembers?\b|\bmagma|\blava|\binferno|\bexplosion"),
    ("frost", r"\bice\b|\bicy\b|\bfrost|\bsnow|\bwater|\bbubbles?\b|\bwaves?\b|\bdroplets?\b|\brain\b"),
    ("thunder", r"\blightning|\bthunder|\belectric|\bsparks?\b|\bbolts?\b|\bplasma"),
    ("wind", r"\bwind|\btornado|\bwhirl|\bgust|\bfeathers?\b|\bcyclone|\bvortex|\bsmoke|\bclouds?\b"),
    ("earth", r"\brocks?\b|\bstones?\b|\bearth|\bsand\b|\bdust\b|\bcracks?\b|\bboulders?\b|\btentacles?\b|\bmud\b|\bmountain"),
    ("wood", r"\bleaf|\bleaves|\bpetals?\b|\bflowers?\b|\bvines?\b|\bplants?\b|\bwood|\bpoison|\btoxic|\bthorns?\b|\bseeds?\b|\bbloom|\blotus|\bweb\b"),
]
COLORS = [("fire", {"red", "orange"}), ("frost", {"blue", "cyan"}), ("thunder", {"purple", "violet", "magenta", "pink", "yellow", "gold"}),
          ("wood", {"green"}), ("earth", {"brown"}), ("wind", {"white", "grey", "gray", "black"})]


def classify(x):
    raw = x.get("ai-element-raw", x.get("ai-element")) or []
    kept = []
    for e in raw:
        e = ALIAS.get(e, e)
        if e in GAME and e not in kept:
            kept.append(e)
    if kept:
        return kept, "ai", raw
    for seg in re.split(r"[_\d\s]+", (x.get("name") or "").lower()):
        if seg in NAME_SEGMENTS:
            return [NAME_SEGMENTS[seg]], "keyword", raw
    text = " ".join([x.get("ai-note") or "", " ".join(x.get("ai-tags") or []), " ".join(x.get("ai-skill-ideas") or [])]).lower()
    text = re.sub(r"[_\-]", " ", text)
    for el, rx in KEYWORDS:
        if re.search(rx, text):
            return [el], "keyword", raw
    # the first color named by the note (its main color), else the colors read from the picture
    words = re.findall(r"[a-z]+", (x.get("ai-note") or "").lower()) + [c.lower() for c in (x.get("ai-colors") or [])]
    for w in words:
        for el, cs in COLORS:
            if w in cs:
                return [el], "color", raw
    return ["wind"], "default", raw


def main(dry):
    sys.path.insert(0, os.path.join(HERE, "game"))
    from collect_effects import locked, dump_aligned   # the aligned format of the index (one object a line)
    counts, review = collections.Counter(), []
    for kind in ["Skill", "Unit", "Scenes"]:
        f = os.path.join(ROOT, "docs", "Effects", kind, "index.json")
        if not os.path.exists(f):
            continue
        with locked(f):
            items = json.load(open(f, encoding="utf-8-sig"))
            changed = 0
            for x in items:
                if not (x.get("ai-element") or x.get("ai-element-raw")):
                    continue
                els, src, raw = classify(x)
                counts[(kind, src)] += 1
                if src != "ai":
                    review.append((kind, x.get("name"), raw, els[0], src, (x.get("ai-note") or "")[:70]))
                if x.get("ai-element") != els or x.get("ai-element-source") != src:
                    x["ai-element-raw"] = raw
                    x["ai-element"] = els
                    x["ai-element-source"] = src
                    changed += 1
            if not dry and changed:
                with open(f, "w", encoding="utf-8", newline="\n") as out:
                    dump_aligned(items, out)
        print(f"{kind}: {changed} changed")
    print(dict(counts))
    # the effects whose attribute was guessed (keyword, color, default): to check by eye in the effect viewer
    lines = ["# Effects whose game attribute was guessed", "",
             "`scripts/classify-effect-elements.py`: the AI read no attribute of the game (only water, light, dark, spirit, blood, poison,",
             "none); the attribute below comes from the name / note / tags (keyword), the main color (color) or none (default: wind).",
             "Check them in the effect viewer; a fix goes in `ai-element` of `docs/Effects/<Kind>/index.json`.", "",
             "| Kind | Effect | AI read | Attribute | From | Note |", "|---|---|---|---|---|---|"]
    lines += ["| %s | %s | %s | %s | %s | %s |" % (k, n, ", ".join(raw), el, src, note.replace("|", "/")) for k, n, raw, el, src, note in review]
    if not dry:
        open(os.path.join(ROOT, "docs", "Effects", "elements-review.md"), "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
    print(len(review), "to review")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main("--dry" in sys.argv)
