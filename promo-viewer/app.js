/* Promo viewer: SteamImg pictures, promo entries of scenarios.json, takes of the runs, marketing videos
   (data/promo.js from scripts/update-promo-viewer.py). */
(function () {
  "use strict";
  const DATA = window.PROMO_DATA;
  const $ = (id) => document.getElementById(id);
  const esc = (s) => String(s == null ? "" : s).replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]));
  const store = { get: (k, d) => { try { return localStorage.getItem("pv." + k) ?? d; } catch { return d; } },
                  set: (k, v) => { try { localStorage.setItem("pv." + k, v); } catch { /* no storage */ } } };
  const STEAM = "../3161035078/ModProject/SteamImg/";
  const RUNS = "../logs/autotest/";
  const VIDEO = "../marketing/video-6.0.0/out/";
  const kb = (n) => (n > 1048576 ? (n / 1048576).toFixed(1) + " MB" : Math.round(n / 1024) + " KB");

  if (!DATA) { $("empty").classList.remove("hidden"); return; }
  $("generated").textContent = "Data of " + DATA.generated;

  const pictureOf = (name) => DATA.pictures.find((p) => p.name === name);
  const takesOf = (name) => DATA.runs.flatMap((r) => r.takes.filter((t) => t.name === name).map((t) => ({ run: r.run, ...t })));

  // one list of cards: every promo entry (taken or not), the stale promo pictures, the other SteamImg pictures
  const cards = DATA.entries.map((e) => ({ ...e, picture: pictureOf(e.name), takes: takesOf(e.name) }));
  DATA.pictures.filter((p) => p.stale).forEach((p) => cards.push({ name: p.name, caption: "", scenario: "", picture: p, takes: [], stale: true }));
  DATA.pictures.filter((p) => !p.name).forEach((p) => cards.push({ name: p.file, caption: "", scenario: "", picture: p, takes: [], extra: true }));

  const TABS = [["pictures", "Pictures"], ["runs", "Takes by run"], ["videos", "Videos"]];
  let tab = TABS.some((t) => t[0] === store.get("tab")) ? store.get("tab") : "pictures";
  $("tabs").innerHTML = TABS.map(([k, l]) => `<button data-tab="${k}">${l}</button>`).join("");
  $("tabs").addEventListener("click", (e) => { const b = e.target.closest("button"); if (b) { tab = b.dataset.tab; store.set("tab", tab); render(); } });
  $("q").value = store.get("q", "");
  $("fState").value = store.get("state", "");
  $("q").addEventListener("input", () => { store.set("q", $("q").value); render(); });
  $("fState").addEventListener("change", () => { store.set("state", $("fState").value); render(); });

  // lightbox over a list of {src, cap}
  let lb = [], lbAt = 0;
  const showLb = () => { $("lbImg").src = lb[lbAt].src; $("lbCap").innerHTML = lb[lbAt].cap; };
  const openLb = (list, at) => { lb = list; lbAt = at; showLb(); $("lightbox").classList.remove("hidden"); };
  const step = (d) => { if (!lb.length) return; lbAt = (lbAt + d + lb.length) % lb.length; showLb(); };
  $("lbClose").onclick = () => $("lightbox").classList.add("hidden");
  $("lbPrev").onclick = () => step(-1);
  $("lbNext").onclick = () => step(1);
  document.addEventListener("keydown", (e) => {
    if ($("lightbox").classList.contains("hidden")) return;
    if (e.key === "Escape") $("lightbox").classList.add("hidden");
    if (e.key === "ArrowLeft") step(-1);
    if (e.key === "ArrowRight") step(1);
  });

  const matches = (c) => {
    const q = $("q").value.trim().toLowerCase();
    const st = $("fState").value;
    if (q && ![c.name, c.caption, c.scenario].join(" ").toLowerCase().includes(q)) return false;
    if (st === "taken" && !c.picture) return false;
    if (st === "missing" && (c.picture || c.extra || c.stale)) return false;
    if (st === "stale" && !c.stale) return false;
    return true;
  };

  function renderPictures(list) {
    const shown = list.filter((c) => c.picture);
    const box = shown.map((c) => ({ src: STEAM + c.picture.file, cap: `<b>${esc(c.caption || c.name)}</b> &middot; ${esc(c.picture.file)}` }));
    $("main").innerHTML = `<div class="grid">${list.map((c, i) => {
      const at = shown.indexOf(c);
      const badge = c.stale ? '<span class="badge bad">no promo entry</span>'
        : c.extra ? '<span class="badge">other picture</span>'
        : c.picture ? '<span class="badge ok">in SteamImg</span>' : '<span class="badge warn">not taken</span>';
      const takes = c.takes.length > 1 ? `<div class="takes">${c.takes.map((t, j) =>
        `<img loading="lazy" src="${RUNS}${esc(t.run)}/screens/${esc(t.file)}" data-card="${i}" data-take="${j}" title="${esc(t.run)}">`).join("")}</div>` : "";
      return `<article class="card">
        <div class="thumb${c.picture ? "" : " none"}" ${at >= 0 ? `data-at="${at}"` : ""}>${c.picture
          ? `<img loading="lazy" src="${STEAM}${esc(c.picture.file)}" alt="${esc(c.caption || c.name)}">` : "no picture yet"}</div>
        <div class="body">
          <div class="caption">${esc(c.caption || c.name)}</div>
          <div class="meta">${badge}${c.scenario ? `<span class="mono">${esc(c.scenario)} &middot; ${esc(c.step)}</span>` : ""}
            ${c.picture ? `<span>${esc(c.picture.time)} &middot; ${kb(c.picture.size)}</span>` : ""}
            ${c.takes.length ? `<span>${c.takes.length} take(s)</span>` : ""}</div>
          <div class="meta mono">${esc(c.picture ? c.picture.file : "promo_" + c.name + ".jpg")}</div>
          ${takes}
        </div></article>`;
    }).join("")}</div>`;
    $("main").querySelectorAll(".thumb[data-at]").forEach((el) => el.addEventListener("click", () => openLb(box, +el.dataset.at)));
    $("main").querySelectorAll(".takes img").forEach((el) => el.addEventListener("click", () => {
      const c = list[+el.dataset.card];
      openLb(c.takes.map((t) => ({ src: `${RUNS}${t.run}/screens/${t.file}`, cap: `<b>${esc(c.caption)}</b> &middot; run ${esc(t.run)}` })), +el.dataset.take);
    }));
    return list.length;
  }

  function renderRuns() {
    const q = $("q").value.trim().toLowerCase();
    let n = 0;
    $("main").innerHTML = DATA.runs.length ? DATA.runs.map((r) => {
      const takes = r.takes.filter((t) => !q || (t.name + " " + t.scenario).toLowerCase().includes(q));
      n += takes.length;
      if (!takes.length) return "";
      return `<section class="run"><h2>Run ${esc(r.run)} <span class="sub">${takes.length} take(s)</span></h2>
        <div class="grid">${takes.map((t, j) => {
          const e = DATA.entries.find((x) => x.name === t.name);
          return `<article class="card"><div class="thumb" data-run="${esc(r.run)}" data-at="${j}">
            <img loading="lazy" src="${RUNS}${esc(r.run)}/screens/${esc(t.file)}"></div>
            <div class="body"><div class="caption">${esc(e ? e.caption : t.name)}</div>
            <div class="meta mono">${esc(t.scenario)} &middot; ${esc(t.file)}</div></div></article>`;
        }).join("")}</div></section>`;
    }).join("") : '<div class="empty">No promotion run in logs/autotest yet (scripts/game/run-promo.ps1).</div>';
    $("main").querySelectorAll(".thumb[data-run]").forEach((el) => el.addEventListener("click", () => {
      const r = DATA.runs.find((x) => x.run === el.dataset.run);
      const takes = r.takes.filter((t) => !q || (t.name + " " + t.scenario).toLowerCase().includes(q));
      openLb(takes.map((t) => ({ src: `${RUNS}${r.run}/screens/${t.file}`, cap: `${esc(t.name)} &middot; run ${esc(r.run)}` })), +el.dataset.at);
    }));
    return n;
  }

  function renderVideos() {
    $("main").innerHTML = DATA.videos.length ? `<div class="videos">${DATA.videos.map((v) => `<article class="card">
      ${/\.png$/i.test(v.file) ? `<img src="${VIDEO}${esc(v.file)}">` : `<video controls preload="metadata" src="${VIDEO}${esc(v.file)}"></video>`}
      <div class="body"><div class="caption mono">${esc(v.file)}</div><div class="meta">${esc(v.time)} &middot; ${kb(v.size)}</div></div>
      </article>`).join("")}</div>`
      : '<div class="empty">No video yet: <code>cd marketing/video-6.0.0; npm run render</code> (VS Code task "Marketing: Render Video").</div>';
    return DATA.videos.length;
  }

  function render() {
    $("tabs").querySelectorAll("button").forEach((b) => b.classList.toggle("active", b.dataset.tab === tab));
    $("fState").classList.toggle("hidden", tab !== "pictures");
    const n = tab === "pictures" ? renderPictures(cards.filter(matches)) : tab === "runs" ? renderRuns() : renderVideos();
    $("count").textContent = n + (tab === "videos" ? " file(s)" : tab === "runs" ? " take(s)" : " picture(s)");
  }
  render();
})();
