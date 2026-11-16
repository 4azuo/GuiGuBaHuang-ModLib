"""Generate docs/Taoist/03-Test-Scenarios.md from 3161035078/tests/scenarios.json.

usage: python scripts/game/export-scenarios-doc.py
"""
import json
import os
repo = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
d = json.load(open(repo + r'\3161035078\tests\scenarios.json', encoding='utf-8'))['scenarios']
files = {
    'TT-01': 'Title/TT01_UITitleScreenEvent.cs', 'TT-02': 'Title/TT02_SMGlobalConfigsEvent.cs',
}
import os, re
base = repo + r'\3161035078\ModProject\ModCode\ModMain\AutoTest\Scenarios'
for r, _, fs in os.walk(base):
    for f in fs:
        m = re.match(r'(T[TPGBXZ])(\d\d)_', f)
        if m:
            files[f'{m.group(1)}-{m.group(2)}'] = os.path.relpath(os.path.join(r, f), base).replace('\\', '/')


def cell(x):
    if isinstance(x, str):
        x = x.replace('<', '&lt;').replace('>', '&gt;')
    elif isinstance(x, list):
        x = [v.replace('<', '&lt;').replace('>', '&gt;') for v in x]
    if isinstance(x, list):
        return '<br>'.join(f'{i + 1}. {v}' for i, v in enumerate(x)) if x else ''
    return str(x).replace('|', '/')


out = ['# Taoist - Test scenarios', '',
       'Source of truth: `3161035078/tests/scenarios.json` (this page is generated from it: `python scripts/game/export-scenarios-doc.py`).',
       'Latest OK/NG result of every id: `3161035078/tests/scenario-status.json` (updated by `run-autotest.ps1`, manual results with `set-scenario-status.ps1`; view: `test-viewer/index.html`, tab Scenario status).', '',
       '## Ids', '',
       '| Prefix | Kind | Executed by |', '|---|---|---|',
       '| `TT-nn` | auto, title screen | `AutoTestEvent` before the save is loaded |',
       '| `TP-nn` | auto, pre-test | creates the new character used by the run (trainer on, CrueltyShura); skipped with an existing save |',
       '| `TG-nn` | auto, in game | `AutoTestEvent` after the save is loaded |',
       '| `TB-01/02` | watch, battle | `AutoTestEvent` while you play battles (`-WatchBattles`) |',
       '| `TB-03+` | auto, battle | `AutoTestEvent` starts the battle by code, hits/kills units, ends it and goes back to the map |',
       '| `TX-nn` | auto, time-changing | `AutoTestEvent` last (skip time, currency reform, rebirth); the save is restored after the run |',
       '| `TZ-nn` | auto, very last | ends the character (one-life); the save is restored after the run |',
       '',
       'Each automatic check also gets an id `<scenario id>-<nn>` (e.g. `TG-04-02`) in `results.json`, the report and the status file, so a NG can be traced to the exact assertion.',
       'Automatic scenario code: `3161035078/ModProject/ModCode/ModMain/AutoTest/Scenarios/<Group>/<Id>_<Feature>.cs` (one file per id).', '',
       '**Test data**: use a dedicated copy of a save (mid game, in or near a town, a few NPCs around, some items in storage). The scenarios call mod functions on it (most changes are reverted, NPC exp is not).', '']
for kind, title in (('auto', 'Automatic scenarios'), ('watch', 'Battle watch scenarios'), ('manual', 'Manual scenarios')):
    rows = [x for x in d if x['kind'] == kind]
    if not rows:
        continue  # no empty section (all former manual scenarios are automated)
    out += [f'## {title} ({len(rows)})', '']
    if kind == 'auto':
        out += ['Field `replaces`: former manual ids now covered by the scenario.', '']
    if kind == 'manual':
        out += ['| Id | Feature | Title | Preconditions | Steps | Expected | Why manual |', '|---|---|---|---|---|---|---|']
        for x in rows:
            out.append(f"| {x['id']} | {cell(x['feature'])} | {cell(x['title'])} | {cell(x['preconditions'])} | {cell(x['steps'])} | {cell(x['expected'])} | {cell(x.get('manualReason', ''))} |")
    else:
        out += ['| Id | Feature | Title | Preconditions | Expected | Code |', '|---|---|---|---|---|---|']
        for x in rows:
            code = files.get(x['id'], '?')
            out.append(f"| {x['id']} | {cell(x['feature'])} | {cell(x['title'])} | {cell(x['preconditions'])} | {cell(x['expected'])} | `{code}` |")
    out.append('')
open(repo + r'\docs\Taoist\03-Test-Scenarios.md', 'w', encoding='utf-8', newline='\n').write('\n'.join(out))
missing = [x['id'] for x in d if x['kind'] != 'manual' and x['id'] not in files]
extra = [k for k in files if k not in [x['id'] for x in d]]
print('missing code:', missing, 'code without json:', extra)
