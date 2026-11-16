/* Test results viewer: autotest + unit test runs from data/runs.js (scripts/update-test-viewer.ps1). */
(function () {
  'use strict';

  const DATA = window.TEST_DATA || null;

  // PowerShell 5.1 writes 1-element arrays as plain values: make every list field an array.
  const arr = v => Array.isArray(v) ? v : v == null ? [] : [v];
  if (DATA) {
    ['autotest', 'unittest', 'definitions', 'status', 'modLogs'].forEach(k => DATA[k] = arr(DATA[k]));
    DATA.modLogs.forEach(l => l.lines = arr(l.lines));
    DATA.autotest.forEach(r => {
      r.log = arr(r.log); r.logErrors = arr(r.logErrors); r.scenarios = arr(r.scenarios);
      r.logs = arr(r.logs); r.logs.forEach(l => l.lines = arr(l.lines));
      r.scenarios.forEach(s => { s.checks = arr(s.checks); s.screenshots = arr(s.screenshots); });
    });
    DATA.unittest.forEach(r => { r.tests = arr(r.tests); r.tests.forEach(t => t.categories = arr(t.categories)); });
    DATA.definitions.forEach(d => { d.steps = arr(d.steps); d.expected = arr(d.expected); d.replaces = arr(d.replaces); });
  }
  const $ = (sel, root) => (root || document).querySelector(sel);
  const esc = s => String(s == null ? '' : s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  const OKNG = { PASS: 'OK', FAIL: 'NG', ERROR: 'NG', SKIP: 'SKIP', RUNNING: 'NG', INFO: 'INFO' };
  const label = s => OKNG[s] || s || '-';
  const badge = (s, text) => `<span class="badge b-${esc(String(s || 'none').replace(/\s/g, ''))}">${esc(text || label(s))}</span>`;
  const fmtTime = t => t ? t.replace('T', ' ').slice(0, 16) : '';
  const runTitle = r => `${fmtTime(r.time) || r.id}  -  ${r.id}`;

  const state = { tab: 'auto', run: {}, filter: 'all', query: '', open: new Set() };

  // scenario definitions (3161035078/tests/scenarios.json) by id; run ids may carry a suffix ("TB-01#2")
  const DEFS = {};
  if (DATA) DATA.definitions.forEach(d => DEFS[d.id] = d);
  const defOf = id => DEFS[String(id || '').split('#')[0]] || null;

  /** "What is tested" block: description, preconditions, steps, expected. */
  function renderWhat(d) {
    if (!d) return '';
    return `<div class="what">
      ${d.description ? `<p>${esc(d.description)}</p>` : ''}
      <div class="manual">
        <div><h3>Preconditions</h3>${esc(d.preconditions || '-')}${d.manualReason ? `<h3 style="margin-top:8px">Why manual</h3>${esc(d.manualReason)}` : ''}${d.replaces.length ? `<h3 style="margin-top:8px">Replaces manual</h3>${esc(d.replaces.join(', '))}` : ''}</div>
        <div><h3>Steps</h3><ol>${d.steps.map(x => `<li>${esc(x)}</li>`).join('')}</ol></div>
        <div><h3>Expected</h3><ul>${d.expected.map(x => `<li>${esc(x)}</li>`).join('')}</ul></div>
      </div></div>`;
  }
  const descLine = d => d && d.description ? `<div class="sdesc" title="${esc(d.description)}">${esc(d.description)}</div>` : '';

  // ------------------------------------------------------------------ hash (deep links)
  function readHash() {
    const p = new URLSearchParams(location.hash.slice(1));
    if (p.get('tab')) state.tab = p.get('tab');
    if (p.get('run')) state.run[state.tab] = p.get('run');
    if (p.get('sc')) state.open.add(p.get('sc'));
    if (p.get('q')) state.query = p.get('q');
    if (p.get('logs')) state.logsOpen = true;
    if (p.get('sc') && p.get('img') != null) state.openImg = { sc: p.get('sc'), i: +p.get('img') };
  }
  function writeHash() {
    const p = new URLSearchParams();
    p.set('tab', state.tab);
    if (state.run[state.tab]) p.set('run', state.run[state.tab]);
    history.replaceState(null, '', '#' + p.toString());
  }

  // ------------------------------------------------------------------ runs
  const runsOf = tab => !DATA ? [] : tab === 'unit' ? DATA.unittest || [] : DATA.autotest || [];
  // "Latest of every scenario": a virtual run made of the last result of each scenario id over all runs
  const LATEST = '__latest__';
  let latestRun = null;
  function buildLatestRun() {
    const runs = DATA.autotest || [];
    if (!runs.length) return null;
    const last = {};
    runs.forEach(r => (r.scenarios || []).forEach(s => {
      last[s.id] = Object.assign({}, s, { _run: r.id, _time: r.time || r.startedAt });
    }));
    const order = {};
    (DATA.definitions || []).forEach((d, i) => order[d.id] = i);
    const scenarios = Object.values(last).sort((a, b) => ((order[a.id.split('#')[0]] ?? 999) - (order[b.id.split('#')[0]] ?? 999)) || a.id.localeCompare(b.id));
    const latest = runs[runs.length - 1];
    return { id: LATEST, virtual: true, time: latest.time, scenarios, log: [], logs: [], logErrors: [],
      taoistVersion: latest.taoistVersion, modLibVersion: latest.modLibVersion, player: latest.player,
      startedAt: runs[0].startedAt || runs[0].time, updatedAt: latest.updatedAt };
  }
  function currentRun(tab) {
    const runs = runsOf(tab);
    if (!runs.length) return null;
    if (tab !== 'unit' && (state.run[tab] === LATEST || state.run[tab] == null)) return latestRun || (latestRun = buildLatestRun());
    return runs.find(r => r.id === state.run[tab]) || runs[runs.length - 1];
  }
  /** Run a scenario result comes from (its own run in the "latest of every scenario" mode). */
  const sourceRun = s => s && s._run ? (DATA.autotest || []).find(r => r.id === s._run) : currentRun('auto');
  function runCounts(r) {
    const c = { OK: 0, NG: 0, SKIP: 0 };
    (r.scenarios || []).forEach(s => { c[s.status === 'PASS' ? 'OK' : s.status === 'SKIP' ? 'SKIP' : 'NG']++; });
    return c;
  }
  function runColor(tab, r) {
    if (tab === 'unit') return r.counts.failed ? 'var(--ng)' : 'var(--ok)';
    const c = runCounts(r);
    if (r.status === 'ERROR' || r.status === 'RUNNING') return 'var(--warn)';
    return c.NG ? 'var(--ng)' : 'var(--ok)';
  }

  function renderRunPicker() {
    const pick = $('#runpick');
    const tab = state.tab === 'unit' ? 'unit' : 'auto';
    pick.style.visibility = state.tab === 'unit' || state.tab === 'auto' ? 'visible' : 'hidden';
    const runs = runsOf(tab);
    const cur = currentRun(tab);
    const sel = $('#runSelect');
    const latestOpt = tab === 'unit' ? '' : (() => {
      const lr = latestRun || (latestRun = buildLatestRun());
      const c = lr ? runCounts(lr) : { OK: 0, NG: 0 };
      return `<option value="${LATEST}" ${state.run.auto === LATEST || state.run.auto == null ? 'selected' : ''}>Latest result of every scenario - OK ${c.OK} NG ${c.NG}</option>`;
    })();
    sel.innerHTML = latestOpt + runs.slice().reverse().map((r, i) => {
      const extra = tab === 'unit' ? `${r.counts.passed}/${r.counts.total} passed` : (() => { const c = runCounts(r); return `OK ${c.OK} NG ${c.NG}`; })();
      return `<option value="${esc(r.id)}" ${cur && r.id === cur.id ? 'selected' : ''}>${esc(fmtTime(r.time) || r.id)}${i === 0 ? ' (latest)' : ''} - ${esc(extra)}</option>`;
    }).join('');
    $('#history').innerHTML = runs.slice(-40).map(r =>
      `<span data-run="${esc(r.id)}" class="${cur && r.id === cur.id ? 'sel' : ''}" style="background:${runColor(tab, r)}" title="${esc(runTitle(r))}"></span>`).join('');
  }

  // ------------------------------------------------------------------ autotest view
  function scenarioMatches(s) {
    const st = label(s.status);
    if (state.filter !== 'all' && st !== state.filter) return false;
    if (!state.query) return true;
    const q = state.query.toLowerCase();
    return [s.id, s.feature, s.title, (defOf(s.id) || {}).description].some(x => String(x || '').toLowerCase().includes(q)) ||
      (s.checks || []).some(c => String(c.message || '').toLowerCase().includes(q));
  }

  function renderAuto() {
    const r = currentRun('auto');
    if (!r) return '<div class="empty">No autotest run yet. Run <code>scripts\\game\\run-autotest.ps1</code>.</div>';
    const c = runCounts(r);
    const total = (r.scenarios || []).length || 1;
    const shots = (r.scenarios || []).reduce((n, s) => n + (s.screenshots || []).length, 0);
    const checks = (r.scenarios || []).reduce((n, s) => n + (s.checks || []).filter(x => x.id).length, 0);
    const defs = (DATA.definitions || []);
    const executed = new Set((r.scenarios || []).map(s => s.id.split('#')[0]));
    const notRun = defs.filter(d => d.kind === 'auto' && !executed.has(d.id));
    const rows = (r.scenarios || []).filter(scenarioMatches).map(renderScenarioRow).join('');
    return `
      <div class="card">
        <div class="summary">
          <div class="stat ok"><b>${c.OK}</b><small>OK</small></div>
          <div class="stat ng"><b>${c.NG}</b><small>NG</small></div>
          <div class="stat skip"><b>${c.SKIP}</b><small>Skip</small></div>
          <div class="stat warn"><b>${notRun.length}</b><small>Not run</small></div>
          <div class="stat"><b>${checks}</b><small>Checks</small></div>
          <div class="stat"><b>${shots}</b><small>Screenshots</small></div>
          <div class="bar">
            <div class="track">
              <i style="width:${c.OK / total * 100}%;background:var(--ok)"></i>
              <i style="width:${c.NG / total * 100}%;background:var(--ng)"></i>
              <i style="width:${c.SKIP / total * 100}%;background:var(--skip)"></i>
            </div>
            <small>${r.virtual ? 'Latest result of every scenario over all runs (the run of each result is shown on its row)' : `Runner ${badge(r.status === 'DONE' ? 'OK' : 'NG', r.status)} &nbsp; exit ${esc(r.exitCode)}`}</small>
          </div>
          <dl class="meta">
            <dt>Run</dt><dd>${r.virtual ? 'latest of every scenario' : `<code>${esc(r.id)}</code>`}</dd>
            <dt>Taoist / ModLib</dt><dd>${esc(r.taoistVersion)} / ${esc(r.modLibVersion)}</dd>
            <dt>Player</dt><dd>${esc(r.player || '-')}</dd>
            <dt>Time</dt><dd>${esc(fmtTime(r.startedAt))} &rarr; ${esc((r.updatedAt || '').slice(11, 16))}</dd>
          </dl>
        </div>
      </div>
      <div class="card">
        <div class="card-head">
          <h2>Scenarios</h2>
          <div class="toolbar">
            ${['all', 'NG', 'OK', 'SKIP'].map(f => `<button class="chip ${state.filter === f ? 'on' : ''}" data-filter="${f}">${f === 'all' ? 'All' : f}</button>`).join('')}
            <input class="search" id="search" placeholder="Search id, feature, description, check..." value="${esc(state.query)}">
          </div>
        </div>
        <table class="rows">
          <thead><tr><th></th><th>Id</th><th>Result</th><th>Feature / scenario</th><th class="num">Checks</th><th class="cam">Evidence</th></tr></thead>
          <tbody>${rows || '<tr><td colspan="6" class="noshot">No scenario matches.</td></tr>'}</tbody>
        </table>
      </div>
      ${notRun.length ? `<details class="card box"><summary>Not run (${notRun.length})</summary><table class="rows"><tbody>${notRun.map(d =>
        `<tr><td class="id">${esc(d.id)}</td><td>${badge('NOTRUN', 'NOT RUN')}</td><td><div>${esc(d.title)}</div><div class="feat">${esc(d.feature)}</div>${descLine(d)}</td><td class="feat">${esc(d.preconditions)}</td></tr>`).join('')}</tbody></table></details>` : ''}
      ${(r.logErrors || []).length ? `<details class="card box"><summary>Errors in game logs (${r.logErrors.length})</summary><pre class="log">${esc(r.logErrors.join('\n'))}</pre></details>` : ''}
      ${r.virtual ? '' : renderLogsCard(r)}`;
  }

  function renderScenarioRow(s) {
    const checks = (s.checks || []).filter(c => c.id);
    const ok = checks.filter(c => c.status === 'PASS').length;
    const open = state.open.has(s.id);
    const shots = (s.screenshots || []).length;
    return `
      <tr class="row ${open ? 'open' : ''}" data-sc="${esc(s.id)}">
        <td><span class="caret">&#9656;</span></td>
        <td class="id">${esc(s.id)}</td>
        <td>${badge(s.status)}</td>
        <td><div>${esc(s.title)}</div><div class="feat">${esc(s.feature)} <span class="kind">${esc(s.kind)}</span>${s._run ? ` <button class="kind runlink" data-gorun="${esc(s._run)}" title="Open this run">run ${esc(fmtTime(s._time) || s._run)}</button>` : ''}</div>${open ? '' : descLine(defOf(s.id))}</td>
        <td class="num">${ok}/${checks.length}</td>
        <td class="cam">${shots ? `&#128247; ${shots}` : '-'}</td>
      </tr>
      ${open ? `<tr class="detail"><td colspan="6">${renderScenarioDetail(s)}</td></tr>` : ''}`;
  }

  // ------------------------------------------------------------------ logs
  const TS = /^\[(\d\d:\d\d:\d\d\.\d{3})/;
  const isErr = l => /\u3010Error\u3011|Exception|\berror\b|\[ERROR\]/i.test(l);

  /** [start, end] (HH:mm:ss.fff strings) of a scenario, from the runner log ("<time> [id] start" ... "<time> [id] PASS"). */
  function scenarioWindow(r, id) {
    let from = null, to = null;
    (r.log || []).forEach(l => {
      const m = /^(\d\d:\d\d:\d\d\.\d{3}) \[([^\]]+)\] (start|PASS|FAIL|ERROR|SKIP)/.exec(l);
      if (!m || m[2] !== id) return;
      if (m[3] === 'start') from = m[1]; else to = m[1];
    });
    return from ? [from, to || '99'] : null;
  }

  /** ModLib/Taoist log lines written while the scenario ran (continuation lines follow their timestamped line). */
  function scenarioLog(r, id) {
    const w = scenarioWindow(r, id);
    if (!w) return [];
    const out = [];
    (r.logs || []).filter(l => l.name.startsWith('mod-')).forEach(l => {
      let inside = false;
      l.lines.forEach(line => {
        const m = TS.exec(line);
        if (m) inside = m[1] >= w[0] && m[1] <= w[1];
        if (inside) out.push(line);
      });
    });
    return out;
  }

  function logHtml(lines, query, errorsOnly) {
    const q = (query || '').toLowerCase();
    const shown = [];
    lines.forEach((l, i) => {
      if (errorsOnly && !isErr(l)) return;
      if (q && !l.toLowerCase().includes(q)) return;
      shown.push(`<div class="ll ${isErr(l) ? 'lerr' : ''}${/\[AUTOTEST\]/.test(l) ? ' lat' : ''}"><i>${i + 1}</i>${esc(l)}</div>`);
    });
    return shown.length ? shown.join('') : '<div class="noshot" style="padding:8px">No line.</div>';
  }

  function renderLogsCard(r) {
    const sources = [{ name: 'runner', lines: r.log || [] }].concat(r.logs || []);
    if (!state.logSrc || !sources.some(x => x.name === state.logSrc)) state.logSrc = (sources.find(x => x.name.startsWith('mod-')) || sources[0]).name;
    const src = sources.find(x => x.name === state.logSrc);
    const errs = sources.reduce((n, x) => n + x.lines.filter(isErr).length, 0);
    return `<details class="card box" id="logsCard" ${state.logsOpen ? 'open' : ''}>
      <summary>Logs <span class="feat">(${sources.length} files${errs ? `, <span style="color:var(--ng)">${errs} error lines</span>` : ''})</span></summary>
      <div class="logbar">
        ${sources.map(x => `<button class="chip ${x.name === state.logSrc ? 'on' : ''}" data-logsrc="${esc(x.name)}" title="${x.lines.length} lines">${esc(x.name.replace(/^mod-/, 'mod: ').replace(/-\d{8}$/, ''))}
          ${x.lines.some(isErr) ? '<b style="color:var(--ng)">&#9679;</b>' : ''}</button>`).join('')}
        <label class="chk"><input type="checkbox" id="logErrOnly" ${state.logErrOnly ? 'checked' : ''}> errors only</label>
        <input class="search" id="logSearch" placeholder="Filter lines..." value="${esc(state.logQuery || '')}">
      </div>
      <div class="logview" id="logView">${state.logsOpen ? logHtml(src.lines, state.logQuery, state.logErrOnly) : ''}</div>
    </details>`;
  }

  function renderScenarioDetail(s) {
    const r = currentRun('auto');
    const checkLi = c => `<li><span class="cid">${esc(c.id || '')}</span><span>${badge(c.status)}</span>
      <span><span class="cstep">${esc(c.step)}:</span> ${esc(c.message)}</span></li>`;
    // one card per screenshot group (step), with the checks of that step under the image
    const groups = [];
    (s.screenshots || []).forEach((sh, i) => {
      let g = groups.find(x => x.no === sh.stepNo);
      if (!g) { g = { no: sh.stepNo, step: sh.step, items: [] }; groups.push(g); }
      g.items.push({ sh, i });
    });
    groups.sort((a, b) => a.no - b.no);
    const used = new Set();
    const cards = groups.map(g => {
      const sc = g.no === 0 ? [] : (s.checks || []).filter(c => c.step === g.step);
      sc.forEach(c => used.add(c));
      const st = !sc.filter(c => c.id).length ? '' : sc.some(c => c.status === 'FAIL') ? badge('FAIL') : badge('PASS');
      return `<div class="shot-card">
        <div class="step-head"><span class="step-no">${g.no === 0 ? 'before' : 'step ' + g.no}</span><b>${esc(g.no === 0 ? 'state before the scenario' : g.step)}</b><span class="spacer"></span>${st}</div>
        ${g.items.map(({ sh, i }) => `<div class="shot" data-sc="${esc(s.id)}" data-i="${i}" title="Click to enlarge">
          <img loading="lazy" src="${esc(sh.file)}" alt="">${sh.kind === 'extra' ? `<span>${esc(sh.step)}</span>` : ''}</div>`).join('')}
        ${sc.length ? `<ul class="checks small">${sc.map(checkLi).join('')}</ul>` : ''}
      </div>`;
    }).join('');
    const rest = (s.checks || []).filter(c => !used.has(c));
    const src = sourceRun(s);
    const log = src ? scenarioLog(src, s.id) : [];
    const logErr = log.filter(isErr).length;
    return `<div class="detail-box">
      ${renderWhat(defOf(s.id))}
      ${rest.length ? `<ul class="checks cols">${rest.map(checkLi).join('')}</ul>` : ''}
      ${s.error ? `<div class="err">${esc(s.error)}</div>` : ''}
      ${cards ? `<div class="shot-grid">${cards}</div>` : '<div class="noshot">No screenshot.</div>'}
      <details class="sclog"><summary>Mod log during the scenario <span class="feat">(${log.length} lines${logErr ? `, <span style="color:var(--ng)">${logErr} errors</span>` : ''})</span></summary>
        <div class="logview">${logHtml(log)}</div></details>
    </div>`;
  }

  // ------------------------------------------------------------------ unit test view
  function unitTestMatches(t) {
    const q = state.query.toLowerCase();
    return (state.filter === 'all' || (state.filter === 'NG' ? t.outcome === 'Failed' : state.filter === 'OK' ? t.outcome === 'Passed' : !['Passed', 'Failed'].includes(t.outcome))) &&
      (!q || (t.name + ' ' + t.className + ' ' + (t.description || '') + ' ' + (t.output || '')).toLowerCase().includes(q));
  }

  function renderUnit() {
    const r = currentRun('unit');
    if (!r) return '<div class="empty">No unit test run yet. Run <code>scripts\\run-unittests.ps1</code>.</div>';
    const tests = r.tests.filter(unitTestMatches);
    const byClass = {};
    tests.forEach(t => (byClass[t.className] = byClass[t.className] || []).push(t));
    const total = r.counts.total || 1;
    const body = Object.keys(byClass).sort().map(cls => `
      <tr><td colspan="5" class="feat" style="background:var(--panel-2);font-weight:600">${esc(cls)}</td></tr>
      ${byClass[cls].map(t => {
        const key = 'u:' + cls + '.' + t.name, open = state.open.has(key), canOpen = !!(t.message || t.output || t.description);
        return `<tr class="row ${open ? 'open' : ''}" data-sc="${esc(key)}">
          <td>${canOpen ? '<span class="caret">&#9656;</span>' : ''}</td>
          <td>${badge(t.outcome, t.outcome === 'Passed' ? 'OK' : t.outcome === 'Failed' ? 'NG' : 'SKIP')}</td>
          <td><div class="id">${esc(t.name)}</div>${t.description ? `<div class="udesc">${esc(t.description)}</div>` : ''}</td>
          <td>${(t.categories || []).map(c => `<span class="kind">${esc(c)}</span>`).join(' ')}</td>
          <td class="num">${esc((t.duration || '').replace(/^00:00:0?/, '').slice(0, 6))}s</td></tr>
          ${open && canOpen ? `<tr class="detail"><td colspan="5"><div class="udetail">
            ${t.description ? `<div><h3>What is tested</h3>${esc(t.description)}</div>` : ''}
            ${t.output ? `<div><h3>Values (test output)</h3><pre class="uout">${esc(t.output.trim())}</pre></div>` : ''}
            ${t.message ? `<div><h3>Failure</h3><div class="err">${esc(t.message)}\n\n${esc(t.stack || '')}</div></div>` : ''}</div></td></tr>` : ''}`;
      }).join('')}`).join('');
    return `
      <div class="card"><div class="summary">
        <div class="stat ok"><b>${r.counts.passed}</b><small>OK</small></div>
        <div class="stat ng"><b>${r.counts.failed}</b><small>NG</small></div>
        <div class="stat skip"><b>${r.counts.skipped}</b><small>Skip</small></div>
        <div class="stat"><b>${r.counts.total}</b><small>Total</small></div>
        <div class="bar"><div class="track">
          <i style="width:${r.counts.passed / total * 100}%;background:var(--ok)"></i>
          <i style="width:${r.counts.failed / total * 100}%;background:var(--ng)"></i></div>
          <small><code>${esc(r.file)}</code></small></div>
      </div></div>
      <div class="card">
        <div class="card-head"><h2>Tests</h2>
          <div class="toolbar">${['all', 'NG', 'OK', 'SKIP'].map(f => `<button class="chip ${state.filter === f ? 'on' : ''}" data-filter="${f}">${f === 'all' ? 'All' : f}</button>`).join('')}
          <input class="search" id="search" placeholder="Search test, description, values..." value="${esc(state.query)}"></div>
          </div>
        <table class="rows"><thead><tr><th></th><th>Result</th><th>Test</th><th>Category</th><th class="num">Time</th></tr></thead>
        <tbody>${body || '<tr><td colspan="5" class="noshot">No test matches.</td></tr>'}</tbody></table>
      </div>`;
  }

  // ------------------------------------------------------------------ mod folder logs
  function renderModLogs() {
    const files = DATA.modLogs || [];
    if (!files.length) return `<div class="empty">No log in <code>${esc(DATA.modLogDir || '%USERPROFILE%\\AppData\\LocalLow\\guigugame\\guigubahuang\\mod')}</code>.</div>`;
    if (!state.mlFile || !files.some(f => f.name === state.mlFile)) state.mlFile = files[0].name;
    const cur = files.find(f => f.name === state.mlFile);
    const errs = cur.lines.filter(isErr).length;
    const kb = n => n > 1048576 ? (n / 1048576).toFixed(1) + ' MB' : Math.max(1, Math.round(n / 1024)) + ' KB';
    return `
      <div class="card"><div class="card-head"><h2>Mod folder logs</h2><span class="feat"><code>${esc(DATA.modLogDir || '')}</code> &middot; newest ${files.length} files, last 6000 lines each &middot; data generated ${esc(fmtTime(DATA.generatedAt))}</span></div>
        <div class="logbar modlog-files">${files.map(f => `<button class="chip ${f.name === state.mlFile ? 'on' : ''}" data-mlfile="${esc(f.name)}" title="${esc(fmtTime(f.time))} - ${f.lines.length} lines">${esc(f.name)}<small>${esc(fmtTime(f.time).slice(5))} &middot; ${kb(f.size)}</small>
          ${f.lines.some(isErr) ? '<b style="color:var(--ng)">&#9679;</b>' : ''}</button>`).join('')}</div>
        <div class="logbar">
          <span class="feat">${cur.lines.length} lines${errs ? `, <span style="color:var(--ng)">${errs} error lines</span>` : ''}</span>
          <label class="chk"><input type="checkbox" id="mlErrOnly" ${state.mlErrOnly ? 'checked' : ''}> errors only</label>
          <input class="search" id="mlSearch" placeholder="Filter lines..." value="${esc(state.mlQuery || '')}">
        </div>
        <div class="logview" id="mlView">${logHtml(cur.lines, state.mlQuery, state.mlErrOnly)}</div>
      </div>`;
  }

  // ------------------------------------------------------------------ status view
  function statusMap() {
    const status = {};
    (DATA.status || []).forEach(s => status[s.id] = s);
    return status;
  }
  /** Definitions shown in the status tab (filter + search). */
  function statusDefs() {
    const status = statusMap(), q = state.query.toLowerCase();
    return (DATA.definitions || []).filter(d => {
      const st = (status[d.id] || {}).result || '-';
      if (state.filter !== 'all' && !(state.filter === '-' ? st === '-' : st === state.filter)) return false;
      return !q || [d.id, d.feature, d.title, d.description].some(x => String(x || '').toLowerCase().includes(q));
    });
  }

  function renderStatus() {
    const defs = DATA.definitions || [];
    const status = statusMap();
    const runs = (DATA.autotest || []).slice(-12);
    const histOf = id => runs.map(r => {
      const sc = (r.scenarios || []).filter(s => s.id.split('#')[0] === id);
      if (!sc.length) return `<i title="${esc(r.id)}: not run"></i>`;
      const st = sc.some(s => ['FAIL', 'ERROR', 'RUNNING'].includes(s.status)) ? 'NG' : sc.some(s => s.status === 'PASS') ? 'OK' : 'SKIP';
      return `<i class="c-${st}" title="${esc(r.id)}: ${st}"></i>`;
    }).join('');
    const list = statusDefs();
    const counts = { OK: 0, NG: 0, SKIP: 0, '-': 0 };
    defs.forEach(d => { const st = (status[d.id] || {}).result || '-'; counts[st in counts ? st : '-']++; });
    const body = list.map(d => {
      const s = status[d.id] || {}, st = s.result || '-', key = 's:' + d.id, open = state.open.has(key);
      const ng = Object.keys(s.checks || {}).filter(k => s.checks[k] === 'NG');
      return `<tr class="row ${open ? 'open' : ''}" data-sc="${esc(key)}">
        <td><span class="caret">&#9656;</span></td><td class="id">${esc(d.id)}</td>
        <td>${badge(st === '-' ? 'none' : st.replace(' ', ''), st === '-' ? 'not tested' : st)}</td>
        <td><div>${esc(d.title)}</div><div class="feat">${esc(d.feature)} <span class="kind">${esc(d.kind)}</span></div>${open ? '' : descLine(d)}</td>
        <td>${d.kind === 'manual' ? '<span class="feat">manual</span>' : `<span class="cells">${histOf(d.id)}</span>`}</td>
        <td class="feat">${esc(fmtTime(s.lastRun))}${s.tester ? ' &middot; ' + esc(s.tester) : ''}${ng.length ? `<div style="color:var(--ng)">NG: ${esc(ng.join(', '))}</div>` : ''}${s.note ? `<div>${esc(s.note)}</div>` : ''}</td></tr>
        ${open ? `<tr class="detail"><td colspan="6"><div class="detail-box">${renderWhat(d)}</div></td></tr>` : ''}`;
    }).join('');
    return `
      <div class="card"><div class="summary">
        <div class="stat ok"><b>${counts.OK}</b><small>OK</small></div>
        <div class="stat ng"><b>${counts.NG}</b><small>NG</small></div>
        <div class="stat skip"><b>${counts.SKIP}</b><small>Skip</small></div>
        <div class="stat"><b>${counts['-']}</b><small>Not tested</small></div>
        <div class="bar"><small>Latest result of every scenario id (<code>3161035078/tests/scenario-status.json</code>).
          Result by hand: <code>scripts\\game\\set-scenario-status.ps1 -Id TG-45 -Result OK</code>. History: last ${runs.length} autotest runs.</small></div>
      </div></div>
      <div class="card"><div class="card-head"><h2>Scenarios (${list.length})</h2>
        <div class="toolbar">${[['all', 'All'], ['NG', 'NG'], ['OK', 'OK'], ['SKIP', 'SKIP'], ['-', 'Not tested']].map(([f, t]) => `<button class="chip ${state.filter === f ? 'on' : ''}" data-filter="${f}">${t}</button>`).join('')}
        <input class="search" id="search" placeholder="Search..." value="${esc(state.query)}"></div></div>
        <table class="rows"><thead><tr><th></th><th>Id</th><th>Latest</th><th>Scenario</th><th>History</th><th>Last run</th></tr></thead><tbody>${body}</tbody></table>
      </div>`;
  }

  // ------------------------------------------------------------------ render
  function render() {
    document.querySelectorAll('#tabs button').forEach(b => b.classList.toggle('active', b.dataset.tab === state.tab));
    if (!DATA) { $('#empty').hidden = false; $('#view').innerHTML = ''; return; }
    $('#generated').textContent = 'Data generated ' + fmtTime(DATA.generatedAt);
    renderRunPicker();
    $('#hdrActs').style.visibility = state.tab === 'modlog' ? 'hidden' : 'visible';
    $('#view').innerHTML = state.tab === 'unit' ? renderUnit() : state.tab === 'status' ? renderStatus() : state.tab === 'modlog' ? renderModLogs() : renderAuto();
    writeHash();
  }

  function keepSearchFocus(fn) {
    const el = $('#search'), pos = el ? el.selectionStart : null, had = document.activeElement === el;
    fn();
    if (had) { const n = $('#search'); n.focus(); n.setSelectionRange(pos, pos); }
  }

  // ------------------------------------------------------------------ events
  document.addEventListener('click', e => {
    const t = e.target;
    const tabBtn = t.closest('#tabs button');
    if (tabBtn) { state.tab = tabBtn.dataset.tab; state.filter = 'all'; state.query = ''; render(); return; }
    const go = t.closest('[data-gorun]');
    if (go) { e.stopPropagation(); state.run.auto = go.dataset.gorun; state.open.clear(); render(); window.scrollTo(0, 0); return; }
    const hist = t.closest('#history span');
    if (hist) { state.run[state.tab] = hist.dataset.run; state.open.clear(); render(); return; }
    const chip = t.closest('[data-filter]');
    if (chip) { state.filter = chip.dataset.filter; render(); return; }
    const act = t.closest('[data-act]');
    if (act) {
      if (act.dataset.act === 'expandAll') {
        if (state.tab === 'auto') { const r = currentRun('auto'); if (r) r.scenarios.filter(scenarioMatches).forEach(s => state.open.add(s.id)); }
        else if (state.tab === 'unit') { const u = currentRun('unit'); if (u) u.tests.filter(t => unitTestMatches(t) && (t.message || t.output || t.description)).forEach(x => state.open.add('u:' + x.className + '.' + x.name)); }
        else if (state.tab === 'status') statusDefs().forEach(d => state.open.add('s:' + d.id));
      }
      else state.open.clear();
      const y = window.scrollY; render(); window.scrollTo(0, y); return;
    }
    const thumb = t.closest('.shot');
    if (thumb) { openModal(thumb.dataset.sc, +thumb.dataset.i); return; }
    const mf = t.closest('[data-mlfile]');
    if (mf) { state.mlFile = mf.dataset.mlfile; render(); return; }
    const ls = t.closest('[data-logsrc]');
    if (ls) { e.preventDefault(); state.logSrc = ls.dataset.logsrc; render(); return; }
    if (t.closest('.logbar') || t.closest('details') && !t.closest('tr.row')) return;
    const row = t.closest('tr.row');
    if (row) { const k = row.dataset.sc; state.open.has(k) ? state.open.delete(k) : state.open.add(k); render(); }
  });
  $('#runSelect').addEventListener('change', e => { state.run[state.tab] = e.target.value; state.open.clear(); render(); });
  document.addEventListener('input', e => {
    if (e.target.id === 'search') { state.query = e.target.value; keepSearchFocus(render); }
    if (e.target.id === 'mlSearch' || e.target.id === 'mlErrOnly') {
      state.mlQuery = $('#mlSearch').value; state.mlErrOnly = $('#mlErrOnly').checked;
      const f = (DATA.modLogs || []).find(x => x.name === state.mlFile);
      $('#mlView').innerHTML = logHtml(f ? f.lines : [], state.mlQuery, state.mlErrOnly);
      return;
    }
    if (e.target.id === 'logSearch' || e.target.id === 'logErrOnly') {
      state.logQuery = $('#logSearch').value; state.logErrOnly = $('#logErrOnly').checked;
      const r = currentRun('auto');
      const src = [{ name: 'runner', lines: r.log || [] }].concat(r.logs || []).find(x => x.name === state.logSrc);
      $('#logView').innerHTML = logHtml(src ? src.lines : [], state.logQuery, state.logErrOnly);
    }
  });
  document.addEventListener('toggle', e => {
    if (e.target.id === 'logsCard') {
      const was = state.logsOpen; state.logsOpen = e.target.open;
      if (state.logsOpen && !was) { const y = window.scrollY; render(); window.scrollTo(0, y); }
    }
  }, true);

  // ------------------------------------------------------------------ image modal (zoom, pan, prev/next)
  const modal = { list: [], i: 0, scale: 1, x: 0, y: 0, fit: 1, drag: null, sc: null };
  const img = $('#modalImg'), stage = $('#stage');

  function openModal(scId, index) {
    const r = currentRun('auto');
    const s = r && r.scenarios.find(x => x.id === scId);
    if (!s) return;
    modal.list = s.screenshots || [];
    modal.sc = s;
    $('#modal').hidden = false;
    document.body.style.overflow = 'hidden';
    $('#strip').innerHTML = modal.list.map((sh, i) => `<img src="${esc(sh.file)}" data-i="${i}" title="${esc(sh.kind)}: ${esc(sh.step)}">`).join('');
    show(index);
  }
  function closeModal() { $('#modal').hidden = true; document.body.style.overflow = ''; img.removeAttribute('src'); }
  function show(i) {
    if (!modal.list.length) return;
    modal.i = Math.max(0, Math.min(modal.list.length - 1, i));
    const sh = modal.list[modal.i];
    img.onload = () => fitImage();
    img.src = sh.file;
    $('#openRaw').href = sh.file;
    const what = sh.stepNo === 0 ? 'before the scenario' : `step ${sh.stepNo} ${sh.kind === 'after' ? '(after)' : '(' + sh.kind + ')'}: ${sh.step}`;
    $('#modalCaption').innerHTML = `<b>${esc(modal.sc.id)}</b> ${esc(modal.sc.title)} &nbsp;&middot;&nbsp; ${esc(what)} &nbsp;&middot;&nbsp; ${modal.i + 1}/${modal.list.length}`;
    $('#prevImg').disabled = modal.i === 0;
    $('#nextImg').disabled = modal.i === modal.list.length - 1;
    document.querySelectorAll('#strip img').forEach(x => x.classList.toggle('cur', +x.dataset.i === modal.i));
    const cur = $(`#strip img[data-i="${modal.i}"]`);
    if (cur) cur.scrollIntoView({ block: 'nearest', inline: 'center' });
  }
  function apply() {
    img.style.transform = `translate(${modal.x}px, ${modal.y}px) scale(${modal.scale})`;
    $('#zoomLevel').textContent = Math.round(modal.scale * 100) + '%';
  }
  function fitImage() {
    const w = stage.clientWidth, h = stage.clientHeight, iw = img.naturalWidth || 1, ih = img.naturalHeight || 1;
    modal.fit = Math.min(w / iw, h / ih, 1) * 0.98;
    modal.scale = modal.fit;
    modal.x = (w - iw * modal.scale) / 2;
    modal.y = (h - ih * modal.scale) / 2;
    apply();
  }
  function zoomAt(factor, cx, cy) {
    const ns = Math.max(0.1, Math.min(8, modal.scale * factor));
    if (cx == null) { cx = stage.clientWidth / 2; cy = stage.clientHeight / 2; }
    modal.x = cx - (cx - modal.x) * (ns / modal.scale);
    modal.y = cy - (cy - modal.y) * (ns / modal.scale);
    modal.scale = ns;
    apply();
  }
  function actualSize() {
    const r = stage.getBoundingClientRect();
    zoomAt(1 / modal.scale, r.width / 2, r.height / 2);
  }

  stage.addEventListener('wheel', e => {
    e.preventDefault();
    const r = stage.getBoundingClientRect();
    zoomAt(e.deltaY < 0 ? 1.15 : 1 / 1.15, e.clientX - r.left, e.clientY - r.top);
  }, { passive: false });
  stage.addEventListener('mousedown', e => {
    if (e.target.closest('.nav')) return;
    modal.drag = { x: e.clientX - modal.x, y: e.clientY - modal.y };
    stage.classList.add('dragging');
  });
  window.addEventListener('mousemove', e => {
    if (!modal.drag) return;
    modal.x = e.clientX - modal.drag.x; modal.y = e.clientY - modal.drag.y; apply();
  });
  window.addEventListener('mouseup', () => { modal.drag = null; stage.classList.remove('dragging'); });
  stage.addEventListener('dblclick', e => {
    if (e.target.closest('.nav')) return;
    if (Math.abs(modal.scale - modal.fit) < 0.01) { const r = stage.getBoundingClientRect(); zoomAt(2 / modal.scale, e.clientX - r.left, e.clientY - r.top); }
    else fitImage();
  });
  $('#zoomIn').onclick = () => zoomAt(1.25);
  $('#zoomOut').onclick = () => zoomAt(1 / 1.25);
  $('#zoomFit').onclick = fitImage;
  $('#zoom100').onclick = actualSize;
  $('#prevImg').onclick = () => show(modal.i - 1);
  $('#nextImg').onclick = () => show(modal.i + 1);
  $('#modalClose').onclick = closeModal;
  $('#strip').addEventListener('click', e => { const t = e.target.closest('img'); if (t) show(+t.dataset.i); });
  window.addEventListener('resize', () => { if (!$('#modal').hidden) fitImage(); });
  document.addEventListener('keydown', e => {
    if ($('#modal').hidden) return;
    if (e.key === 'Escape') closeModal();
    else if (e.key === 'ArrowLeft') show(modal.i - 1);
    else if (e.key === 'ArrowRight') show(modal.i + 1);
    else if (e.key === '+' || e.key === '=') zoomAt(1.25);
    else if (e.key === '-') zoomAt(1 / 1.25);
    else if (e.key === '0') fitImage();
    else if (e.key === '1') actualSize();
  });

  // ------------------------------------------------------------------ start
  readHash();
  render();
  if (state.openImg) openModal(state.openImg.sc, state.openImg.i);
})();
