#!/usr/bin/env python3
"""
Extracts pictures of the game into GuiGuBaHuang-ModLib/docs/GameAssets (kept in git with LFS), so they are looked up there instead of
extracted again:
  - TextIcons/<name>.png: the icons of the game's TextMeshPro sprite asset "Emoj" ("[icon:NAME]" tokens of the
    localTexts, "<sprite name=NAME>"), cut from its atlas with the glyph table of the asset (read from the raw data:
    the game is Il2Cpp, its MonoBehaviours have no type tree);
  - DramaBackgrounds/<name>.webp: the drama backgrounds (the "backgroud" of the drama configs of SampleConfs, the
    names given to DramaHelper.OpenDrama2 / StoryLine.Tell), 960 px wide;
  - ItemIcons/<icon>.png: the icons of the items ("icon" of ItemProps / DLCItemProps; the sprite drawn in the bag),
    with items.json: icon -> item ids;
  - Buildings/<atlas>/<name>.png: the sprites of the atlases TownCommon (buildings of the towns), CommonMapGrid
    (buildings and places of the world map), VillageCommon, SchoolCommon (the Build* sprites: halls of the sects)
    (SpriteTool.GetSprite(atlas, name), EstateEvent);
  - SkillIcons/<name>.png: the sprites of the atlas CommonSkillIcon (the "icon" of BattleSkillAttack / BattleStepBase:
    the icons of the martial arts in battle, SpriteTool.GetSprite("CommonSkillIcon", icon)), with skills.json: icon ->
    the martial arts a player can learn that use it (id, kind, basis "basType" of the config, MP cost); tags.json
    (kept, written by hand after a review of the pictures: basis + what the picture shows) is shown in the gallery;
  - index.json (names, sizes) and index.html (gallery with a search box).
Files already extracted are kept (--force to extract again).

usage:
  python GuiGuBaHuang-ModLib/scripts/extract-game-assets.py [--icons] [--backgrounds] [--items] [--buildings] [--skills] [--force]      (none: all)
needs: pip install UnityPy Pillow
"""
import argparse
import glob
import json
import os
import re
import struct
import sys

import UnityPy

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), 'game'))
import _modrepo  # noqa: E402
ROOT = _modrepo.repo()   # the mod repository (shared script: GuiGuBaHuang-ModLib/scripts)
OUT = os.path.join(_modrepo.MODLIB, 'docs', 'GameAssets')
GAME_DATA = r'C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒\guigubahuang_Data'
SAMPLE_CONFS = os.path.join(_modrepo.MODLIB, '3385996759', 'SampleConfs')
BG_WIDTH = 960


def game_data():
    if os.path.isdir(GAME_DATA):
        return GAME_DATA
    # scripts/open-folder.ps1 game-data -Print
    import subprocess
    p = subprocess.run(['powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', os.path.join(ROOT, 'scripts', 'open-folder.ps1'), 'game-data', '-Print'],
                       capture_output=True, text=True)
    return p.stdout.strip().splitlines()[0]


def tmp_icons(env, out_dir, force):
    """The sprite asset "Emoj": character table (name -> glyph) and glyph table (glyph -> rect in the atlas)."""
    asset = None
    for o in env.objects:
        if o.type.name != 'MonoBehaviour':
            continue
        d = o.get_raw_data()
        if b'zm_lingshi' in d and b'gongji' in d:
            asset = d
            break
    if asset is None:
        raise SystemExit('TMP sprite asset not found')
    chars = {}
    for m in re.finditer(rb'([\x02-\x30])\x00\x00\x00([a-zA-Z][a-zA-Z0-9_]{1,40})', asset):
        n = m.group(1)[0]
        name = m.group(2)[:n]
        if len(m.group(2)) < n:
            continue
        _, _, glyph = struct.unpack_from('<iIi', asset, m.start() - 16)
        scale, = struct.unpack_from('<f', asset, m.start() - 4)
        if 0 <= glyph < 1000 and 0.1 < scale < 10:
            chars[name.decode()] = glyph
    start = next(s for s in range(0, len(asset) - 120, 4)
                 if struct.unpack_from('<III', asset, s)[0] == 0 and struct.unpack_from('<I', asset, s + 60)[0] == 1 and struct.unpack_from('<I', asset, s + 120)[0] == 2)
    glyphs = {}
    off = start
    while off + 60 <= len(asset) and struct.unpack_from('<I', asset, off)[0] == len(glyphs):
        glyphs[len(glyphs)] = struct.unpack_from('<4i', asset, off + 24)
        off += 60
    atlas = None
    for o in env.objects:
        if o.type.name == 'Texture2D':
            t = o.read()
            if t.m_Name == 'Emoj':
                atlas = t.image
                break
    os.makedirs(out_dir, exist_ok=True)
    H = atlas.height
    done = []
    for name, g in sorted(chars.items()):
        if g not in glyphs:
            continue
        x, y, w, h = glyphs[g]
        path = os.path.join(out_dir, f'{name}.png')
        if force or not os.path.exists(path):
            atlas.crop((x, H - y - h, x + w, H - y)).save(path)
        done.append({'name': name, 'file': f'TextIcons/{name}.png', 'w': w, 'h': h})
    print(f'text icons: {len(done)} -> {out_dir}')
    return done


def drama_backgrounds(env, out_dir, force):
    names = set()
    for f in glob.glob(os.path.join(SAMPLE_CONFS, '*Drama*.json')):
        names.update(m for m in re.findall(r'"backgroud":\s*"([^"]+)"', open(f, encoding='utf-8-sig').read()) if m not in ('0', '-1'))
    # the backgrounds named in the code of the mod (StoryLine, EncounterEvent...)
    for f in glob.glob(os.path.join(ROOT, _modrepo.project(), 'ModProject', 'ModCode', 'ModMain', '**', '*.cs'), recursive=True):
        names.update(re.findall(r'return "([a-z][a-z0-9_]+)";\s*//', open(f, encoding='utf-8-sig').read()))
    os.makedirs(out_dir, exist_ok=True)
    have = {os.path.splitext(f)[0] for f in os.listdir(out_dir)} if not force else set()
    want = names - have
    done = []
    if want:
        for o in env.objects:
            if o.type.name != 'Texture2D':
                continue
            t = o.read()
            if t.m_Name not in want:
                continue
            want.discard(t.m_Name)
            img = t.image.convert('RGB')
            if img.width > BG_WIDTH:
                img = img.resize((BG_WIDTH, round(img.height * BG_WIDTH / img.width)))
            img.save(os.path.join(out_dir, f'{t.m_Name}.webp'), 'WEBP', quality=78, method=6)
    from PIL import Image
    for f in sorted(os.listdir(out_dir)):
        if f.endswith('.webp'):
            with Image.open(os.path.join(out_dir, f)) as im:
                done.append({'name': f[:-5], 'file': f'DramaBackgrounds/{f}', 'w': im.width, 'h': im.height})
    print(f'drama backgrounds: {len(done)} -> {out_dir} (not found in the game: {len(want)})')
    return done


def item_icons(env, out_dir, force):
    """The sprites named by the "icon" of the item configs (same names as their Texture2D when the sprite is a whole
    texture; sprite.image crops the atlas otherwise)."""
    icons = {}
    for f in ['ItemProps.json', 'DLCItemProps.json']:
        p = os.path.join(SAMPLE_CONFS, f)
        if os.path.exists(p):
            for x in json.load(open(p, encoding='utf-8-sig')):
                if x.get('icon') not in (None, '', '0'):
                    icons.setdefault(x['icon'], []).append(int(x['id']))
    os.makedirs(out_dir, exist_ok=True)
    want = set(icons) if force else {n for n in icons if not os.path.exists(os.path.join(out_dir, f'{n}.png'))}
    if want:
        for o in env.objects:
            if o.type.name != 'Sprite':
                continue
            try:
                sp = o.read()
            except Exception:
                continue
            if sp.m_Name in want:
                want.discard(sp.m_Name)
                try:
                    sp.image.save(os.path.join(out_dir, f'{sp.m_Name}.png'))
                except Exception as e:
                    print('  sprite', sp.m_Name, e)
    json.dump({k: sorted(v) for k, v in sorted(icons.items())}, open(os.path.join(out_dir, 'items.json'), 'w', encoding='utf-8', newline='\n'), indent=0)
    done = [{'name': n, 'file': f'ItemIcons/{n}.png', 'items': icons[n][:5]} for n in sorted(icons) if os.path.exists(os.path.join(out_dir, f'{n}.png'))]
    print(f'item icons: {len(done)} of {len(icons)} -> {out_dir}')
    return done


# atlas -> prefix of the sprites kept (None: all)
BUILDING_ATLASES = {'towncommon': None, 'commonmapgrid': None, 'villagecommon': None, 'schoolcommon': 'Build'}


def buildings(out_dir, force):
    """The sprites of the atlases of the towns and of the world map (SpriteTool.GetSprite(atlas, name) in the mods):
    Buildings/<atlas>/<name>.png. The atlases are resources (ResourceManager of globalgamemanagers)."""
    data = game_data()
    env = UnityPy.load(os.path.join(data, 'globalgamemanagers'), os.path.join(data, 'resources.assets'))
    files = {os.path.basename(k).lower(): f for k, f in env.files.items()}
    rm = next(o for o in files['globalgamemanagers'].objects.values() if o.type.name == 'ResourceManager').read_typetree()
    res = files['resources.assets']
    cat = {c[0]: c[1]['m_PathID'] for c in rm['m_Container']}
    done = []
    for name, prefix in BUILDING_ATLASES.items():
        key = 'spriteatlas/' + name
        if key not in cat:
            print('  atlas not found:', name)
            continue
        d = os.path.join(out_dir, name)
        os.makedirs(d, exist_ok=True)
        for ptr in res.objects[cat[key]].read().m_PackedSprites:
            try:
                sp = ptr.read()
                if prefix and not sp.m_Name.startswith(prefix):
                    continue
                path = os.path.join(d, f'{sp.m_Name}.png')
                if force or not os.path.exists(path):
                    sp.image.save(path)
                from PIL import Image
                with Image.open(path) as im:
                    done.append({'name': f'{name}/{sp.m_Name}', 'file': f'Buildings/{name}/{sp.m_Name}.png', 'w': im.width, 'h': im.height})
            except Exception as e:
                print('  sprite', name, e)
    done.sort(key=lambda x: x['name'].lower())
    print(f'building sprites: {len(done)} -> {out_dir}')
    return done


# kind of a martial art: BattleSkillAttack type (1/2 attack, 3 spell, 4 ultimate), BattleStepBase: step
SKILL_KINDS = {'1': 'attack', '2': 'attack', '3': 'spell', '4': 'ultimate'}


def skill_icons(out_dir, force):
    """The sprites of the atlas CommonSkillIcon: SkillIcons/<name>.png, and skills.json (icon -> martial arts a player
    can learn that use it: those with a class, as SkillForgeEvent.GetBaseIds)."""
    uses = {}
    for f, step in [('BattleSkillAttack.json', False), ('DLCBattleSkillAttack.json', False), ('BattleStepBase.json', True)]:
        p = os.path.join(SAMPLE_CONFS, f)
        if not os.path.exists(p):
            continue
        for x in json.load(open(p, encoding='utf-8-sig')):
            icon = x.get('icon')
            if icon in (None, '', '0') or x.get('className') in (None, '', '0') or x.get('name') in (None, '', '0'):
                continue
            uses.setdefault(icon, []).append({'id': int(x['id']), 'kind': 'step' if step else SKILL_KINDS.get(x.get('type'), x.get('type')),
                                              'basis': x.get('basType') or '0', 'mp': x.get('mpCost')})
    data = game_data()
    env = UnityPy.load(os.path.join(data, 'globalgamemanagers'), os.path.join(data, 'resources.assets'))
    files = {os.path.basename(k).lower(): f for k, f in env.files.items()}
    rm = next(o for o in files['globalgamemanagers'].objects.values() if o.type.name == 'ResourceManager').read_typetree()
    res = files['resources.assets']
    cat = {c[0]: c[1]['m_PathID'] for c in rm['m_Container']}
    key = 'spriteatlas/commonskillicon'
    if key not in cat:
        print('  atlas not found: CommonSkillIcon')
        return []
    os.makedirs(out_dir, exist_ok=True)
    tags_path = os.path.join(out_dir, 'tags.json')
    tags = json.load(open(tags_path, encoding='utf-8')).get('tags', {}) if os.path.exists(tags_path) else {}
    done = []
    for ptr in res.objects[cat[key]].read().m_PackedSprites:
        try:
            sp = ptr.read()
            path = os.path.join(out_dir, f'{sp.m_Name}.png')
            if force or not os.path.exists(path):
                sp.image.save(path)
            u = uses.get(sp.m_Name, [])
            done.append({'name': sp.m_Name, 'file': f'SkillIcons/{sp.m_Name}.png',
                         'kinds': sorted({x['kind'] for x in u}), 'basis': sorted({x['basis'] for x in u if x['basis'] != '0'}), 'bases': len(u),
                         'tags': tags.get(sp.m_Name, [])})
        except Exception as e:
            print('  sprite', e)
    done.sort(key=lambda x: x['name'].lower())
    json.dump({k: v for k, v in sorted(uses.items())}, open(os.path.join(out_dir, 'skills.json'), 'w', encoding='utf-8', newline='\n'), indent=0)
    print(f'skill icons: {len(done)} ({sum(1 for x in done if x["bases"])} used by martial arts of the player) -> {out_dir}')
    return done


GALLERY = """<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Game Assets</title><style>
:root{--bg:#f6f7f9;--panel:#fff;--line:#e2e6eb;--text:#1d2330;--muted:#6b7380}
@media (prefers-color-scheme:dark){:root{--bg:#12151b;--panel:#1a1e26;--line:#2c3240;--text:#e4e8ef;--muted:#9aa3b2}}
body{margin:0;background:var(--bg);color:var(--text);font:13px "Segoe UI",system-ui,sans-serif}
header{position:sticky;top:0;background:var(--panel);border-bottom:1px solid var(--line);padding:10px 16px;display:flex;gap:12px;align-items:center;flex-wrap:wrap;z-index:2}
input{padding:5px 8px;border:1px solid var(--line);border-radius:6px;background:var(--panel);color:var(--text);min-width:240px}
.tabs button{border:1px solid var(--line);background:var(--bg);color:var(--text);padding:4px 12px;border-radius:999px;cursor:pointer}
.tabs button.on{background:#3b6fd8;color:#fff;border-color:#3b6fd8}
main{padding:12px 16px;display:grid;gap:10px}
main.icons{grid-template-columns:repeat(auto-fill,minmax(120px,1fr))}
main.items{grid-template-columns:repeat(auto-fill,minmax(120px,1fr))}
.items figure img{height:64px}
main.bgs{grid-template-columns:repeat(auto-fill,minmax(240px,1fr))}
main.builds{grid-template-columns:repeat(auto-fill,minmax(170px,1fr))}
main.skills{grid-template-columns:repeat(auto-fill,minmax(120px,1fr))}
.skills figure img{height:64px}
.builds figure img{height:130px;object-fit:contain;background:repeating-conic-gradient(#0001 0 25%,#0000 0 50%) 0 0/16px 16px}
figure{margin:0;background:var(--panel);border:1px solid var(--line);border-radius:8px;padding:8px;text-align:center;cursor:copy}
figure img{max-width:100%;image-rendering:auto}
.icons figure img{height:48px}
figcaption{font-family:ui-monospace,Consolas,monospace;font-size:12px;color:var(--muted);word-break:break-all;margin-top:4px}
.count{color:var(--muted);margin-left:auto}
.viewers{display:inline-flex;border:1px solid var(--line);border-radius:999px;overflow:hidden}
.viewers a{padding:4px 12px;color:var(--muted);text-decoration:none;white-space:nowrap}
.viewers a+a{border-left:1px solid var(--line)}
.viewers a:hover{color:var(--text);background:var(--bg)}
.viewers a.on{background:#3b6fd8;color:#fff;font-weight:600}
</style></head><body><header><b>Game Assets</b><span class="tabs"><button data-k="icons" class="on">Text icons</button><button data-k="bgs">Drama backgrounds</button><button data-k="items">Item icons</button><button data-k="builds">Buildings</button><button data-k="skills">Skill icons</button></span>
<input id="q" type="search" placeholder="Filter by name..."><span class="count" id="count"></span>
<nav class="viewers" aria-label="Viewers"><a href="../../../logs/test-viewer/index.html" title="Results of the autotest and the unit tests">Tests</a><a href="../../effect-viewer/index.html" title="Effects recorded by the galleries (TE-xx)">Effects</a><a href="../../../logs/promo-viewer/index.html" title="Steam Workshop pictures, promotion takes, marketing videos">Promo</a><a class="on" aria-current="page" href="index.html" title="Pictures extracted from the game: text icons, drama backgrounds, item icons, buildings, skill icons">Assets</a></nav></header><main id="main"></main>
<script>const DATA=__DATA__;let kind='icons';const L=()=>DATA[{icons:'icons',bgs:'backgrounds',items:'items',builds:'buildings',skills:'skills'}[kind]]||[];
const info=x=>x.kinds?` [${x.kinds.join(', ')}${x.tags&&x.tags.length?' · '+x.tags.join(', '):''}]`:'';
function show(){const q=document.getElementById('q').value.toLowerCase();const list=L().filter(x=>(x.name+info(x)).toLowerCase().includes(q));
const m=document.getElementById('main');m.className=kind;m.innerHTML=list.map(x=>`<figure title="click: copy the name" data-n="${x.name}"><img loading="lazy" src="${x.file}" alt=""><figcaption>${x.name}${x.items?' ('+x.items.join(', ')+')':''}${x.w&&kind==='builds'?' '+x.w+'x'+x.h:''}${kind==='skills'?info(x):''}</figcaption></figure>`).join('');
document.getElementById('count').textContent=list.length+' / '+L().length;}
function pick(k){kind=k;document.querySelectorAll('.tabs button').forEach(x=>x.classList.toggle('on',x.dataset.k===k));history.replaceState(null,'','#'+k);show();}
document.querySelectorAll('.tabs button').forEach(b=>b.onclick=()=>pick(b.dataset.k));
if(['icons','bgs','items','builds','skills'].includes(location.hash.slice(1)))kind=location.hash.slice(1),document.querySelectorAll('.tabs button').forEach(x=>x.classList.toggle('on',x.dataset.k===kind));
document.getElementById('q').oninput=show;document.getElementById('main').onclick=e=>{const f=e.target.closest('figure');if(f)navigator.clipboard&&navigator.clipboard.writeText(f.dataset.n);};show();</script></body></html>
"""


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--icons', action='store_true')
    ap.add_argument('--backgrounds', action='store_true')
    ap.add_argument('--items', action='store_true')
    ap.add_argument('--buildings', action='store_true')
    ap.add_argument('--skills', action='store_true')
    ap.add_argument('--force', action='store_true')
    a = ap.parse_args()
    both = not a.icons and not a.backgrounds and not a.items and not a.buildings and not a.skills
    env = UnityPy.load(os.path.join(game_data(), 'resources.assets'))
    index_path = os.path.join(OUT, 'index.json')
    index = json.load(open(index_path, encoding='utf-8')) if os.path.exists(index_path) else {'icons': [], 'backgrounds': []}
    if a.icons or both:
        index['icons'] = tmp_icons(env, os.path.join(OUT, 'TextIcons'), a.force)
    if a.backgrounds or both:
        index['backgrounds'] = drama_backgrounds(env, os.path.join(OUT, 'DramaBackgrounds'), a.force)
    if a.items or both:
        index['items'] = item_icons(env, os.path.join(OUT, 'ItemIcons'), a.force)
    if a.buildings or both:
        index['buildings'] = buildings(os.path.join(OUT, 'Buildings'), a.force)
    if a.skills or both:
        index['skills'] = skill_icons(os.path.join(OUT, 'SkillIcons'), a.force)
    os.makedirs(OUT, exist_ok=True)
    json.dump(index, open(index_path, 'w', encoding='utf-8', newline='\n'), ensure_ascii=False, indent=1)
    open(os.path.join(OUT, 'index.html'), 'w', encoding='utf-8', newline='\n').write(GALLERY.replace('__DATA__', json.dumps(index, ensure_ascii=False)))
    print('gallery:', os.path.join(OUT, 'index.html'))


if __name__ == '__main__':
    main()
