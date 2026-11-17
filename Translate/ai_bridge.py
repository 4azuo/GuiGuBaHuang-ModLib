#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
AI translation bridge: hand the missing locale texts to an AI (Claude...) with as few tokens as possible.

1. export: finds the entries of the main *localText.json files missing in the locale folders (de, es, fr, ja,
   la, ru, th, vi), applies the translations already in the cache (Translate/.cache/en-<lang>.json, also filled
   by run.py), and writes what is left:
     - <out>.json: {"langs": [...], "texts": {"t1": "English text", ...}} - each English text once, even when
       it is used by several keys / files / languages (this is the only file the AI reads);
     - <out>.refs.json: where each text goes (project, file, id, key, languages).
2. Give <out>.json to the AI with the rules printed by export; it writes one file per language:
     {"t1": "translation", ...}
3. import: checks every translation (placeholders {0} {1:#,##0}, <color> tags, line breaks, nothing empty),
   appends the valid ones to the locale files (created when missing, style of the file kept), and stores them in
   the cache (never asked again). Invalid ones are listed and left missing.

usage:
  python ai_bridge.py export --projects 3161035078 3385996759 [--langs all|de,fr] [--out ai_todo]
  python ai_bridge.py import --lang de --file de.json [--refs ai_todo.refs.json]
  python ai_bridge.py status --projects 3161035078 3385996759
  python ai_bridge.py seed-cache --projects 3161035078 3385996759   (cache <- translations already in the locale files)
"""

import argparse
import glob
import json
import os
import re
import sys

from consts import DEFAULT_TARGET_LANGUAGES, DIR_PATTERNS, LOCALE_COMBINED_KEY, SKIP_TRANSLATION_TEXTS, TRANSLATION_CONFIG

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
PH = re.compile(r'\{\d+(?::[^}]*)?\}|\{[A-Za-z][A-Za-z0-9]*\}')  # {0}, {1:#,##0}, story name tokens {p} {A} {T2}...
TAG = re.compile(r'</?color[^>]*>')

RULES = """Translate the English texts of a cultivation (xianxia) game into {langs}.
Input: {file} ({count} texts). For each language write <lang>.json = {{"t1": "translation", ...}} with every id.
Rules: keep {{0}}, {{1:#,##0}}..., the name tokens {{p}} {{A}} {{T}}... and <color=...>...</color> exactly; keep the same number of line breaks (\\n);
natural game style (dialogue sounds spoken, options short); no notes, only the JSON."""


def load_json(path):
    raw = open(path, 'rb').read().decode('utf-8-sig')
    return json.loads(re.sub(r',(\s*[\]}])', r'\1', raw))


def modconf(project):
    p = project if os.path.isdir(project) else os.path.join(ROOT, project)
    return os.path.join(p, DIR_PATTERNS['modconf_path'])


def english(row):
    for k, v in row.items():
        if k == 'en' or (k != LOCALE_COMBINED_KEY and re.match(r'.*\ben\b.*', k) and '|' in k):
            return v
    return row.get('en')


def missing_entries(projects, langs):
    """[(project, file, id, key, english, [langs missing])]"""
    out = []
    for project in projects:
        base = modconf(project)
        for f in sorted(glob.glob(os.path.join(base, DIR_PATTERNS['localtext_pattern']))):
            name = os.path.basename(f)
            rows = [r for r in load_json(f) if isinstance(r, dict) and r.get('key')]
            have = {}
            for l in langs:
                p = os.path.join(base, l, name)
                have[l] = {r.get('key') for r in load_json(p)} if os.path.exists(p) else set()
            for r in rows:
                en = english(r)
                if not en or not str(en).strip():
                    continue
                miss = [l for l in langs if r['key'] not in have[l]]
                if miss:
                    out.append((project, name, str(r.get('id')), r['key'], en, miss))
    return out


def cache_path(lang):
    return os.path.join(HERE, TRANSLATION_CONFIG['cache_dir'], f"{TRANSLATION_CONFIG['source_language']}-{lang}.json")


def load_cache(lang):
    try:
        return json.load(open(cache_path(lang), encoding='utf-8'))
    except Exception:
        return {}


def save_cache(lang, data):
    os.makedirs(os.path.dirname(cache_path(lang)), exist_ok=True)
    json.dump(data, open(cache_path(lang), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)


def problems(en, tr):
    out = []
    if not isinstance(tr, str) or (not tr.strip() and en.strip()):
        return ['empty']
    if sorted(PH.findall(en)) != sorted(PH.findall(tr)):
        out.append(f'placeholders {PH.findall(en)} vs {PH.findall(tr)}')
    if TAG.findall(en) != TAG.findall(tr):
        out.append('color tags')
    if en.count('\n') != tr.count('\n'):
        out.append(f'line breaks {en.count(chr(10))} vs {tr.count(chr(10))}')
    return out


def append_locale(path, entries):
    """Appends {id, key, en|ch|tc|kr} entries to a locale file, keeping its style (BOM, line ends, tab indent)."""
    if os.path.exists(path):
        raw = open(path, 'rb').read()
        bom = raw.startswith(b'\xef\xbb\xbf')
        s = raw.decode('utf-8-sig')
        nl = '\r\n' if '\r\n' in s else '\n'
        body = s.rstrip()
        if not body.endswith(']'):
            raise ValueError(f'not a JSON array: {path}')
        body = body[:-1].rstrip()
        items = [('\t' + json.dumps(e, ensure_ascii=False, indent='\t').replace('\n', nl + '\t')) for e in entries]
        s = body + (',' if body.endswith('}') else '') + nl + (',' + nl).join(items) + nl + ']' + (nl if s.endswith(('\n', '\r\n')) else '')
        open(path, 'wb').write((b'\xef\xbb\xbf' if bom else b'') + s.encode('utf-8'))
    else:
        os.makedirs(os.path.dirname(path), exist_ok=True)
        open(path, 'wb').write(json.dumps(entries, ensure_ascii=False, indent='\t').encode('utf-8'))


def write_entries(per_file, lang):
    for (project, name), entries in per_file.items():
        append_locale(os.path.join(modconf(project), lang, name), entries)


def cmd_export(args):
    langs = DEFAULT_TARGET_LANGUAGES if args.langs == 'all' else args.langs.split(',')
    rows = missing_entries(args.projects, langs)
    # translations already cached: applied at once
    caches = {l: load_cache(l) for l in langs}
    applied = {l: {} for l in langs}
    texts, refs, ids = {}, {}, {}
    for project, name, rid, key, en, miss in rows:
        left = []
        for l in miss:
            tr = en if en.strip() in SKIP_TRANSLATION_TEXTS else caches[l].get(en)
            if tr is not None and not problems(en, tr):
                applied[l].setdefault((project, name), []).append({'id': rid, 'key': key, LOCALE_COMBINED_KEY: tr})
            else:
                left.append(l)
        if not left:
            continue
        if en not in ids:
            ids[en] = f"t{len(ids) + 1}"
            texts[ids[en]] = en
        refs.setdefault(ids[en], []).append({'project': project, 'file': name, 'id': rid, 'key': key, 'langs': left})
    n_cache = 0
    for l in langs:
        if applied[l]:
            write_entries(applied[l], l)
            n_cache += sum(len(v) for v in applied[l].values())
    need = sorted({l for r in refs.values() for x in r for l in x['langs']})
    out = args.out
    json.dump({'langs': need, 'texts': texts}, open(out + '.json', 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
    json.dump(refs, open(out + '.refs.json', 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
    chars = sum(len(t) for t in texts.values())
    print(f"missing entries: {sum(len(r[5]) for r in rows)} (texts x languages); applied from the cache: {n_cache}")
    print(f"for the AI: {len(texts)} unique texts ({chars} characters) x {len(need)} languages -> {out}.json")
    if texts:
        print()
        print(RULES.format(langs=', '.join(need), file=out + '.json', count=len(texts)))


def cmd_import(args):
    refs = json.load(open(args.refs, encoding='utf-8'))
    src = json.load(open(args.out + '.json', encoding='utf-8'))['texts'] if os.path.exists(args.out + '.json') else {}
    tr = load_json(args.file)
    cache = load_cache(args.lang)
    per_file, bad, done = {}, [], 0
    for tid, places in refs.items():
        places = [p for p in places if args.lang in p['langs']]
        if not places:
            continue
        en = src.get(tid)
        t = tr.get(tid)
        if t is None:
            bad.append(f"{tid}: missing"); continue
        p = problems(en, t) if en is not None else []
        if p:
            bad.append(f"{tid}: {', '.join(p)}"); continue
        if en is not None:
            cache[en] = t
        for x in places:
            per_file.setdefault((x['project'], x['file']), []).append({'id': x['id'], 'key': x['key'], LOCALE_COMBINED_KEY: t})
            done += 1
    if not args.check:
        write_entries(per_file, args.lang)
        save_cache(args.lang, cache)
    print(f"{args.lang}: {done} entries {'valid' if args.check else 'written'} in {len(per_file)} files; problems: {len(bad)}")
    for b in bad:
        print('  ' + b)


def cmd_seed(args):
    """Fills the cache with the translations already in the locale files (English text of the main file -> translation)."""
    langs = DEFAULT_TARGET_LANGUAGES if args.langs == 'all' else args.langs.split(',')
    for l in langs:
        cache = load_cache(l)
        before = len(cache)
        for project in args.projects:
            base = modconf(project)
            for f in glob.glob(os.path.join(base, DIR_PATTERNS['localtext_pattern'])):
                p = os.path.join(base, l, os.path.basename(f))
                if not os.path.exists(p):
                    continue
                tr = {r.get('key'): r.get(LOCALE_COMBINED_KEY) for r in load_json(p) if isinstance(r, dict)}
                for r in load_json(f):
                    if not isinstance(r, dict): continue
                    en, t = english(r), tr.get(r.get('key'))
                    if en and isinstance(t, str) and t.strip() and en not in cache and not problems(en, t):
                        cache[en] = t
        save_cache(l, cache)
        print(f"{l}: {len(cache)} cached ({len(cache) - before} added)")


def cmd_status(args):
    langs = DEFAULT_TARGET_LANGUAGES if args.langs == 'all' else args.langs.split(',')
    rows = missing_entries(args.projects, langs)
    per = {l: 0 for l in langs}
    for r in rows:
        for l in r[5]:
            per[l] += 1
    print('missing per language:', per)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest='cmd', required=True)
    e = sub.add_parser('export'); e.add_argument('--projects', nargs='+', required=True); e.add_argument('--langs', default='all'); e.add_argument('--out', default='ai_todo')
    i = sub.add_parser('import'); i.add_argument('--lang', required=True); i.add_argument('--file', required=True)
    i.add_argument('--out', default='ai_todo', help='prefix of the export (source texts for the checks)'); i.add_argument('--refs', default=None); i.add_argument('--check', action='store_true', help='only validate')
    s = sub.add_parser('status'); s.add_argument('--projects', nargs='+', required=True); s.add_argument('--langs', default='all')
    c = sub.add_parser('seed-cache'); c.add_argument('--projects', nargs='+', required=True); c.add_argument('--langs', default='all')
    args = ap.parse_args()
    if args.cmd == 'import' and not args.refs:
        args.refs = args.out + '.refs.json'
    {'export': cmd_export, 'import': cmd_import, 'status': cmd_status, 'seed-cache': cmd_seed}[args.cmd](args)


if __name__ == '__main__':
    main()
