"""Labels by rule of the Unit effects (GuiGuBaHuang-ModLib/docs/Effects/Unit/index.json, fields "ai-*"): there is no
gallery of them to look at, so their attribute and use come from the words (pinyin) of their name and of their particle
systems. Only what the words say: an effect without such a word keeps no attribute. Labels of a reviewer ("user-*", or
"ai-*" without "ai-rule") are never changed.

usage: python GuiGuBaHuang-ModLib/scripts/game/label_unit_effects.py [--dry]
"""
import json, os, re, sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from collect_effects import locked, dump_aligned

BASE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "docs", "Effects", "Unit", "index.json")

# attribute -> words (in a name: strong; in a particle system: weak, several needed)
ATTR = {
    "fire": r"(?<!fu)(?<!shou)(?<!mei)huo(?!de|shou|fa|zhe|shen)|lieyan|yanhuo|ranshao|zhuque|fenghuang|fire|flame",
    "frost": r"bing(?!qi|fa)|shuang(?!bei|ling)|xuehua|hanqi|dongjie|ice|frost|snow",
    "thunder": r"lei(?!tai|xing)|shandian|dianliu|daidian|mabi|lightning|thunder",
    "wind": r"feng(?!yin|huang|shui|zi)|xuanfeng|jufeng|wind|tornado",
    "earth": r"shitou|yanshi|tudun|dilie|caidilie|rock|stone",
    "wood": r"teng(?!xun)|shuteng|huaban|yezi|zhongdu|duwu|leaf|vine|poison",
    "sword": r"jian(?!su|shao|bian|tou|ta|xiu|yan|hua)|sword",
    "blade": r"daoguang|daoqi|dadao|blade|slash",
    "spear": r"changqiang|qiangbao|spear",
    "fist": r"quanshu|quanji|fist",
    "palm": r"zhangfa|zhangyin|palm",
}
USE = [
    (r"hudun|huzhao|jingangzhao|gangti|shouhu|shield", ["shield", "aura"]),
    (r"huixie|huixue|zhiliao|fuhuo|heal", ["heal", "buff"]),
    (r"beiji|shouji|hit|chudi|jida", ["hit"]),
    (r"bao(?!wei|kai)|baozha|explo", ["explosion"]),
    (r"zhaohuan|summon|chuxian", ["summon"]),
    (r"tuowei|trail", ["trail"]),
    (r"chongci|fashe|tiaoyue", ["charge"]),
    (r"shifa|xuli|qianyao|ultimateshifa", ["cast"]),
    (r"bingdong|xuanyun|mabi|jiansu|zhongdu|liuxue|chanrao|shufu|ruodian|fengyin", ["debuff", "status-icon"]),
    (r"kuangbao|jiaqiang|jiasu|buff|zhenxin", ["buff", "aura"]),
    (r"yujing", ["decoration"]),
    (r"siwang|_die|xiaoshi", ["decoration"]),
]


def labels(e):
    name = e["name"].lower()
    systems = [s["path"].split("/", 1)[-1].lower() for s in (e.get("asset") or {}).get("systems", [])]
    score = {}
    for a, rx in ATTR.items():
        n = (3 if re.search(rx, name) else 0) + min(3, sum(1 for s in systems if re.search(rx, s.split("/")[-1])))
        if n >= 2:
            score[a] = n
    best = sorted(score, key=lambda a: -score[a])[:2]
    if len(best) == 2 and score[best[1]] * 2 <= score[best[0]]:
        best = best[:1]
    use = next((u for rx, u in USE if re.search(rx, name)), [])
    return best, use


def main():
    dry = "--dry" in sys.argv
    with locked(BASE):
        index = json.load(open(BASE, encoding="utf-8-sig"))
        n = u = 0
        for e in index:
            if any(k.startswith("user-") for k in e) or (e.get("ai-element") and not e.get("ai-rule")):
                continue
            el, use = labels(e)
            for k in ("ai-element", "ai-usage", "ai-rule", "ai-tags"):
                e.pop(k, None)
            if el:
                e["ai-element"] = el
                n += 1
            if use:
                e["ai-usage"] = use
                u += 1
            if el or use:
                e["ai-tags"] = sorted(set(el + use))
                e["ai-rule"] = True
        if not dry:
            with open(BASE, "w", encoding="utf-8", newline="\n") as f:
                dump_aligned(index, f)
    print(f"{len(index)} unit effects: {n} with an attribute, {u} with a use")
    if dry:
        for e in index[::23]:
            print(e["name"], labels(e))


if __name__ == "__main__":
    main()
