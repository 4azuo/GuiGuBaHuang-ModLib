/* Effect viewer: docs/Effects/<kind>/index.json (data/effects.js from effect-viewer/update-data.py). */
(function () {
  "use strict";
  const DATA = window.EFFECT_DATA;
  const $ = (id) => document.getElementById(id);
  const esc = (s) => String(s == null ? "" : s).replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]));
  const arr = (v) => (Array.isArray(v) ? v : v == null || v === "" ? [] : [v]);
  const store = { get: (k, d) => { try { return localStorage.getItem("ev." + k) ?? d; } catch { return d; } },
                  set: (k, v) => { try { localStorage.setItem("ev." + k, v); } catch { /* no storage */ } } };

  if (!DATA || !DATA.kinds || !Object.keys(DATA.kinds).length) { $("empty").classList.remove("hidden"); return; }
  $("generated").textContent = "Data of " + DATA.generated;

  const kinds = Object.keys(DATA.kinds);
  let kind = kinds.includes(store.get("kind")) ? store.get("kind") : kinds[0];
  let selected = null;
  const shown = (e) => !!e.file && e.visible !== false;
  const loops = (e) => (e.asset && e.asset.loop != null ? e.asset.loop : e.loop);
  const ELEMENTS = ["blade", "spear", "sword", "fist", "palm", "finger", "fire", "frost", "thunder", "wind", "earth", "wood",
    "water", "light", "dark", "poison", "blood", "spirit", "none"];
  const USAGES = ["projectile", "beam", "chain", "slash", "thrust", "hit", "explosion", "aoe", "field", "rain", "buff", "debuff",
    "shield", "aura", "heal", "summon", "trail", "cast", "charge", "status-icon", "teleport", "decoration"];
  const SIZES = ["small", "medium", "large", "huge"];
  const canSave = !!DATA.server;
  // an assessment: a user-* field with a value (the aligned index gives every effect the columns, null)
  const userLabelled = (e) => Object.keys(e).some((k) => k.startsWith("user-") && k !== "user-updated" && arr(e[k]).length);
  const duration = (e) => { const d = e.asset && e.asset.durationMs != null ? e.asset.durationMs : e.durationMs; return d == null ? null : d; };

  function tabs() {
    $("tabs").innerHTML = kinds.map((k) => `<button data-k="${k}" class="${k === kind ? "active" : ""}">${k} <small>${DATA.kinds[k].filter(shown).length}/${DATA.kinds[k].length}</small></button>`).join("");
    $("tabs").querySelectorAll("button").forEach((b) => b.onclick = () => { kind = b.dataset.k; store.set("kind", kind); selected = null; closeDetail(); tabs(); options(); render(); });
  }

  function options() {
    const list = DATA.kinds[kind];
    const fill = (id, label, values) => {
      const cur = $(id).value;
      const counts = {};
      values.forEach((v) => counts[v] = (counts[v] || 0) + 1);
      $(id).innerHTML = `<option value="">${label}: all</option>` + Object.keys(counts).sort().map((v) => `<option value="${esc(v)}">${esc(v)} (${counts[v]})</option>`).join("");
      if (counts[cur]) $(id).value = cur;
    };
    fill("fElement", "Element", list.flatMap((e) => arr(e["ai-element"])));
    fill("fUsage", "Usage", list.flatMap((e) => arr(e["ai-usage"])));
    fill("fMotion", "Motion", list.flatMap((e) => arr(e["ai-motion"])));
    fill("fColor", "Colour", list.flatMap((e) => arr(e["ai-colors"])));
  }

  function matches(e) {
    const q = $("q").value.trim().toLowerCase();
    const st = $("fState").value;
    if (st === "shown" && !shown(e)) return false;
    if (st === "hidden" && shown(e)) return false;
    if (st === "labelled" && !e["ai-element"]) return false;
    if (st === "unlabelled" && (e["ai-element"] || !shown(e))) return false;
    if (st === "userlabelled" && !userLabelled(e)) return false;
    if (st === "usernot" && (userLabelled(e) || !shown(e))) return false;
    const f = (id, key) => !$(id).value || arr(e[key]).includes($(id).value);
    if (!f("fElement", "ai-element") || !f("fUsage", "ai-usage") || !f("fMotion", "ai-motion") || !f("fColor", "ai-colors")) return false;
    if ($("fLoop").value !== "" && String(loops(e) ? 1 : 0) !== $("fLoop").value) return false;
    if (!q) return true;
    const hay = [e.name, e.path, e["ai-note"], arr(e["ai-tags"]).join(" "), arr(e["ai-skill-ideas"]).join(" "), arr(e["ai-element"]).join(" "),
      arr(e["ai-usage"]).join(" "), e["ai-shape"], e.asset ? arr(e.asset.scripts).join(" ") : "",
      e["user-note"], arr(e["user-tags"]).join(" "), arr(e["user-skill-ideas"]).join(" ")].join(" ").toLowerCase();
    return q.split(/\s+/).every((w) => hay.includes(w));
  }

  function sorted(list) {
    const k = $("sort").value;
    const by = {
      index: (a, b) => (a.index ?? 0) - (b.index ?? 0),
      name: (a, b) => a.name.localeCompare(b.name),
      duration: (a, b) => (duration(b) ?? -2) - (duration(a) ?? -2),
      size: (a, b) => ((b.width || 0) * (b.height || 0)) - ((a.width || 0) * (a.height || 0)),
      coverage: (a, b) => (b["ai-coverage"] || 0) - (a["ai-coverage"] || 0),
    }[k];
    return list.slice().sort(by);
  }

  const img = (e) => `../docs/Effects/${kind}/${encodeURIComponent(e.file)}`;

  function card(e) {
    const el = arr(e["ai-element"]).map((x) => `<span class="badge el">${esc(x)}</span>`).join("");
    const us = arr(e["ai-usage"]).map((x) => `<span class="badge us">${esc(x)}</span>`).join("");
    const lp = (loops(e) ? `<span class="badge loop">loop</span>` : "") + (userLabelled(e) ? `<span class="badge user" title="Assessed by you">&#9733; you</span>` : "");
    const thumb = shown(e) ? `<img loading="lazy" src="${img(e)}" alt="">` : `<span>${e.visible === false ? "not visible" : "no recording"}</span>`;
    return `<div class="card${selected === e.name ? " sel" : ""}" data-n="${esc(e.name)}"><div class="thumb">${thumb}</div>
      <div class="meta"><div class="name" title="${esc(e.name)}"><span class="idx">${String(e.index ?? "").padStart(4, "0")}</span>${esc(e.name)}</div>
      <div class="badges">${el}${us}${lp}</div></div></div>`;
  }

  function render() {
    const list = sorted(DATA.kinds[kind].filter(matches));
    $("count").textContent = `${list.length} effect(s)`;
    $("grid").innerHTML = list.map(card).join("");
    $("grid").querySelectorAll(".card").forEach((c) => c.onclick = () => openDetail(c.dataset.n));
  }

  function kv(rows) {
    return `<div class="kv">${rows.filter((r) => r[1] != null && r[1] !== "" && !(Array.isArray(r[1]) && !r[1].length))
      .map(([k, v, ai]) => `<div>${esc(k)}</div><div class="${ai ? "ai" : ""}">${esc(Array.isArray(v) ? v.join(", ") : v)}</div>`).join("")}</div>`;
  }

  function openDetail(name) {
    const e = DATA.kinds[kind].find((x) => x.name === name);
    if (!e) return;
    selected = name;
    document.querySelectorAll(".card.sel").forEach((c) => c.classList.remove("sel"));
    const c = document.querySelector(`.card[data-n="${CSS.escape(name)}"]`);
    if (c) c.classList.add("sel");
    const a = e.asset || {};
    const sys = arr(a.systems).map((s) => `<tr><td class="mono">${esc(s.path)}</td><td>${esc(s.length)}</td><td>${s.loop ? "yes" : ""}</td><td>${esc(s.delay)}</td>
      <td>${esc(arr(s.lifetime).join("-"))}</td><td>${esc(arr(s.size).join("-"))}</td><td>${esc(arr(s.speed).join("-"))}</td><td>${esc(s.maxParticles)}</td><td>${esc(s.shape)} ${esc(s.radius)}</td></tr>`).join("");
    $("detailBody").innerHTML = `
      <h2>${esc(e.name)}</h2>
      <div class="mono">${esc(e.path)}<button class="copy" id="copyPath">copy</button></div>
      ${shown(e) ? `<img class="big" src="${img(e)}" alt=""><div class="frames" id="frames"></div>` : ""}
      <h3>AI assessment</h3>
      ${e["ai-element"] ? "" : `<div class="sub">Not labelled by the AI reviewer yet: only the picture analysis below (motion, colours, coverage).</div>`}
      ${kv([["element", e["ai-element"], 1], ["usage", e["ai-usage"], 1], ["shape", e["ai-shape"], 1], ["size", e["ai-size"], 1],
            ["motion", e["ai-motion"], 1], ["colours", e["ai-colors"], 1], ["screen coverage", e["ai-coverage"], 1], ["tags", e["ai-tags"], 1], ["note", e["ai-note"], 1]])}
      ${arr(e["ai-skill-ideas"]).length ? `<div class="kv"><div>skill ideas</div><div class="ai"><ul class="ideas">${arr(e["ai-skill-ideas"]).map((x) => `<li>${esc(x)}</li>`).join("")}</ul></div></div>` : ""}
      <h3>Your assessment ${e["user-updated"] ? `<span class="sub">(saved ${esc(e["user-updated"])})</span>` : ""}</h3>
      ${userForm(e)}
      <h3>Recorded (in game)</h3>
      ${kv([["index", e.index], ["visible", e.visible], ["casting time (ms)", e.castingTimeMs], ["duration (ms)", e.durationMs], ["loop", e.loop],
            ["direction", e.direction], ["size (world)", e.width != null ? `${e.width} x ${e.height}` : null], ["camera / zoom", e.camera != null ? `${e.camera} / ${e.zoom}` : null]])}
      <h3>Asset (game files)</h3>
      ${kv([["casting time (ms)", a.castingTimeMs], ["duration (ms)", a.durationMs === -1 ? "-1 (until removed)" : a.durationMs], ["loop", a.loop],
            ["objects", a.objects], ["scripts", a.scripts]])}
      ${sys ? `<table class="sys"><tr><th>system</th><th>length</th><th>loop</th><th>delay</th><th>lifetime</th><th>size</th><th>speed</th><th>max</th><th>shape</th></tr>${sys}</table>` : ""}`;
    $("copyPath").onclick = () => navigator.clipboard && navigator.clipboard.writeText(e.path);
    bindUserForm(e);
    showFrames(e);
    $("detail").classList.remove("hidden");
  }

  // ------------------------------------------------------------- 12 frames of the effect (ImageDecoder: Chrome / Edge)
  // the frames where the effect shows (compared with the first frame: the ground before it), 12 consecutive ones from the first (a strip to scroll)
  async function showFrames(e) {
    const box = $("frames");
    if (!box) return;
    if (!("ImageDecoder" in window)) { box.innerHTML = `<div class="sub">Frames: needs Chrome or Edge (ImageDecoder).</div>`; return; }
    box.dataset.n = e.name;
    box.innerHTML = `<div class="sub">decoding the frames...</div>`;
    const stale = () => box.dataset.n !== e.name || !document.body.contains(box);
    try {
      const res = await fetch(img(e));
      const dec = new ImageDecoder({ data: await res.arrayBuffer(), type: "image/webp" });
      await dec.tracks.ready;
      const n = dec.tracks.selectedTrack.frameCount;
      const W = 160, H = 90;
      const cv = document.createElement("canvas");
      cv.width = W; cv.height = H;
      const cx = cv.getContext("2d", { willReadFrequently: true });
      // the ground (no effect): the first or the last frame, the plainer one (effects add light and colour)
      const grab = async (i) => { const { image } = await dec.decode({ frameIndex: i }); cx.drawImage(image, 0, 0, W, H);
        const t = image.timestamp; image.close(); return { px: cx.getImageData(0, 0, W, H).data, t }; };
      const energy = (px) => { let e = 0; for (let k = 0; k < px.length; k += 4) {
        const mx = Math.max(px[k], px[k + 1], px[k + 2]), mn = Math.min(px[k], px[k + 1], px[k + 2]); e += (mx - mn) * mx; } return e; };
      const g0 = (await grab(0)).px.slice(), gl = (await grab(n - 1)).px.slice();
      const ground = energy(g0) <= energy(gl) ? g0 : gl;
      const info = [];
      for (let i = 0; i < n; i++) {
        const { image } = await dec.decode({ frameIndex: i });
        cx.drawImage(image, 0, 0, W, H);
        const px = cx.getImageData(0, 0, W, H).data;
        let changed = 0;
        for (let k = 0; k < px.length; k += 4) {
          if (Math.abs(px[k] - ground[k]) + Math.abs(px[k + 1] - ground[k + 1]) + Math.abs(px[k + 2] - ground[k + 2]) > 72) changed++;
        }
        info.push({ t: Math.round(image.timestamp / 1000), changed });
        image.close();
        if (stale()) return;
      }
      // shown: a change of at least a tenth of the biggest one (small far sparks / map edge noise left out)
      const limit = Math.max(12, Math.max(...info.map((x) => x.changed)) * 0.1);
      const shownIdx = info.map((x, i) => (x.changed >= limit ? i : -1)).filter((i) => i >= 0);
      const first = shownIdx.length ? shownIdx[0] : 0, last = shownIdx.length ? shownIdx[shownIdx.length - 1] : n - 1;
      // consecutive frames, one recorded frame apart (a whole multiple of it for effects longer than 12 frames)
      const step = Math.max(1, Math.round((last - first + 1) / 12));
      const start = Math.max(0, Math.min(first, n - 1 - step * 11));
      const picks = [];
      for (let i = start; i < n && picks.length < 12; i += step) picks.push(i);
      box.innerHTML = "";
      for (const i of picks) {
        const { image } = await dec.decode({ frameIndex: i });
        if (stale()) { image.close(); return; }
        const c = document.createElement("canvas");
        c.width = 320; c.height = 180;
        c.getContext("2d").drawImage(image, 0, 0, 320, 180);
        image.close();
        const cell = document.createElement("div");
        cell.className = "frame";
        cell.appendChild(c);
        const lab = document.createElement("span");
        lab.textContent = `${info[i].t - info[first].t} ms`;
        cell.appendChild(lab);
        box.appendChild(cell);
      }
      dec.close();
    } catch (err) {
      box.innerHTML = `<div class="sub">Frames: not read (${esc(err.message)})${canSave ? "" : " - open the viewer with the VS Code task \"Test: Open Effect Viewer\""}.</div>`;
    }
  }

  // ------------------------------------------------------------- the tester's own assessment (user-* fields)
  function checks(name, values, chosen) {
    return `<div class="checks">${values.map((v) => `<label><input type="checkbox" name="${name}" value="${v}"${chosen.includes(v) ? " checked" : ""}>${v}</label>`).join("")}</div>`;
  }

  function userForm(e) {
    const rating = e["user-rating"] || 0;
    return `<form class="uform" id="uform">
      ${canSave ? "" : `<div class="sub warn">Read only: open the viewer with the VS Code task "Test: Open Effect Viewer" of the mod repository (python GuiGuBaHuang-ModLib/scripts/effect-viewer-server.py --open) to save.</div>`}
      <div class="row"><span>rating</span><span class="stars" id="uStars">${[1, 2, 3, 4, 5].map((i) => `<button type="button" data-v="${i}" class="${i <= rating ? "on" : ""}">&#9733;</button>`).join("")}</span></div>
      <div class="row"><span>element</span>${checks("uElement", ELEMENTS, arr(e["user-element"]))}</div>
      <div class="row"><span>usage</span>${checks("uUsage", USAGES, arr(e["user-usage"]))}</div>
      <div class="row"><span>shape</span><input id="uShape" value="${esc(e["user-shape"] || "")}" placeholder="${esc(e["ai-shape"] || "circle, line, arc...")}"></div>
      <div class="row"><span>size</span><select id="uSize"><option value=""></option>${SIZES.map((x) => `<option${e["user-size"] === x ? " selected" : ""}>${x}</option>`).join("")}</select></div>
      <div class="row"><span>tags</span><input id="uTags" value="${esc(arr(e["user-tags"]).join(", "))}" placeholder="comma separated"></div>
      <div class="row"><span>note</span><textarea id="uNote" rows="2">${esc(e["user-note"] || "")}</textarea></div>
      <div class="row"><span>skill ideas</span><textarea id="uIdeas" rows="3" placeholder="one a line">${esc(arr(e["user-skill-ideas"]).join("\n"))}</textarea></div>
      <div class="acts"><button type="button" id="uCopyAi">Copy AI</button><button type="button" id="uClear">Clear</button>
        <button type="submit" class="primary"${canSave ? "" : " disabled"}>Save</button><span class="sub" id="uMsg"></span></div>
    </form>`;
  }

  function bindUserForm(e) {
    const form = $("uform");
    let rating = e["user-rating"] || 0;
    const stars = () => form.querySelectorAll("#uStars button").forEach((x) => x.classList.toggle("on", +x.dataset.v <= rating));
    form.querySelectorAll("#uStars button").forEach((b) => b.onclick = () => { const v = +b.dataset.v; rating = rating === v ? 0 : v; stars(); });
    const setChecks = (name, values) => form.querySelectorAll(`input[name=${name}]`).forEach((c) => c.checked = values.includes(c.value));
    $("uCopyAi").onclick = () => {
      setChecks("uElement", arr(e["ai-element"]));
      setChecks("uUsage", arr(e["ai-usage"]));
      $("uShape").value = e["ai-shape"] || "";
      $("uSize").value = e["ai-size"] || "";
      $("uTags").value = arr(e["ai-tags"]).join(", ");
      $("uNote").value = e["ai-note"] || "";
      $("uIdeas").value = arr(e["ai-skill-ideas"]).join("\n");
    };
    $("uClear").onclick = () => {
      setChecks("uElement", []); setChecks("uUsage", []);
      ["uShape", "uSize", "uTags", "uNote", "uIdeas"].forEach((id) => $(id).value = "");
      rating = 0; stars();
    };
    form.onsubmit = async (ev) => {
      ev.preventDefault();
      if (!canSave) return;
      const picked = (name) => [...form.querySelectorAll(`input[name=${name}]:checked`)].map((c) => c.value);
      const fields = {
        "user-rating": rating || null, "user-element": picked("uElement"), "user-usage": picked("uUsage"),
        "user-shape": $("uShape").value.trim(), "user-size": $("uSize").value,
        "user-tags": $("uTags").value.split(",").map((x) => x.trim()).filter(Boolean),
        "user-note": $("uNote").value.trim(), "user-skill-ideas": $("uIdeas").value.split("\n").map((x) => x.trim()).filter(Boolean),
      };
      $("uMsg").textContent = "saving...";
      try {
        const r = await fetch("/api/user-label", { method: "POST", headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ kind, name: e.name, fields }) });
        const res = await r.json();
        if (!res.ok) throw new Error(res.error);
        Object.keys(e).filter((k) => k.startsWith("user-")).forEach((k) => delete e[k]);
        Object.assign(e, res.fields);
        render();
        openDetail(e.name);
        $("uMsg").textContent = "saved";
      } catch (err) { $("uMsg").textContent = "not saved: " + err.message; }
    };
  }

  function closeDetail() { $("detail").classList.add("hidden"); }
  $("close").onclick = closeDetail;
  document.addEventListener("keydown", (ev) => { if (ev.key === "Escape") closeDetail(); });
  ["q", "fElement", "fUsage", "fMotion", "fColor", "fLoop", "fState", "sort"].forEach((id) => {
    const el = $(id);
    const saved = store.get(id, null);
    if (saved != null && id !== "q") el.value = saved;
    el.addEventListener(id === "q" ? "input" : "change", () => { store.set(id, el.value); render(); });
  });

  tabs();
  options();
  ["fElement", "fUsage", "fMotion", "fColor"].forEach((id) => { const s = store.get(id, null); if (s) $(id).value = s; });
  render();
})();
