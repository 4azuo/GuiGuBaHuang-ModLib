// Copies the pictures of 3161035078/ModProject/SteamImg to public/steam (served by Remotion) and writes
// src/promo.generated.json: the promo pictures present in SteamImg with their captions (the "promo" entries of
// 3161035078/tests/scenarios.json, in that order; pictures taken by scripts/game/run-promo.ps1) and the other
// pictures (1.jpg...).
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(here, "..");
const repo = path.resolve(root, "..", "..");
const steamImg = path.join(repo, "3161035078", "ModProject", "SteamImg");
const scenariosFile = path.join(repo, "3161035078", "tests", "scenarios.json");
const pub = path.join(root, "public", "steam");

if (!fs.existsSync(steamImg)) throw new Error(`No SteamImg folder: ${steamImg}`);
fs.rmSync(pub, { recursive: true, force: true });
fs.mkdirSync(pub, { recursive: true });

const files = fs.readdirSync(steamImg).filter((f) => /\.(png|jpe?g)$/i.test(f));
for (const f of files) fs.copyFileSync(path.join(steamImg, f), path.join(pub, f));

const scenarios = JSON.parse(fs.readFileSync(scenariosFile, "utf8").replace(/^﻿/, "")).scenarios ?? [];
const promo = scenarios
  .flatMap((s) => (s.promo ?? []).map((p) => ({ name: p.name, file: `promo_${p.name}.png`, caption: p.caption ?? "", scenario: s.id })))
  .filter((p) => files.includes(p.file));
const extra = files.filter((f) => !f.startsWith("promo_")).sort();

fs.writeFileSync(path.join(root, "src", "promo.generated.json"), JSON.stringify({ promo, extra }, null, 2));
console.log(`SteamImg: ${files.length} picture(s), ${promo.length} with a caption -> public/steam`);

// Music of each video, cut to its timeline (same chapters and beats as src/data.ts): scripts/make-music.py
import { execFileSync } from "node:child_process";
const timing = JSON.parse(fs.readFileSync(path.join(root, "src", "chapters.json"), "utf8"));
const timelineOf = (short) => {
  const segments = [["intro", timing.introBeats]];
  const names = new Set(promo.map((p) => p.name));
  const parts = timing.chapters.map((c) => (short ? c.short : c.pictures).filter((n) => names.has(n)));
  if (!short && parts.length > 0) {
    const rest = promo.filter((p) => !timing.chapters.some((c) => c.pictures.includes(p.name)));
    parts[parts.length - 1].push(...rest.map((p) => p.name));
  }
  for (const p of parts.filter((x) => x.length > 0)) {
    segments.push(["chapter", timing.chapterBeats]);
    segments.push(["slides", p.length * timing.slideBeats]);
  }
  if (promo.length === 0) segments.push(["chapter", timing.chapterBeats], ["slides", extra.length * timing.slideBeats]);
  segments.push(["outro", timing.outroBeats]);
  return { bpm: timing.bpm, segments };
};
for (const [name, short] of [["long", false], ["short", true]]) {
  const tl = path.join(root, "src", `timeline-${name}.generated.json`);
  fs.writeFileSync(tl, JSON.stringify(timelineOf(short), null, 2));
  const out = execFileSync("python", [path.join(here, "make-music.py"), tl, path.join(root, "public", `music-${name}.wav`)], { encoding: "utf8" });
  process.stdout.write(out);
}
