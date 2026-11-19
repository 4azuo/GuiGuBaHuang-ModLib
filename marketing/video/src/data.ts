import generated from "./promo.generated.json";
import timing from "./chapters.json";

/** A promo picture of SteamImg with its caption ("promo" of 3161035078/tests/scenarios.json). */
export type PromoPicture = { name: string; file: string; caption: string; scenario: string };

export const promo: PromoPicture[] = generated.promo;
/** The other pictures of SteamImg (1.jpg...): used when no promo picture exists. */
export const extra: string[] = generated.extra;

export const FPS = timing.fps;
export const BPM = timing.bpm;
/** Frames of a beat (12 at 30 fps and 150 BPM): every cut is on a beat of the music. */
export const BEAT = Math.round((FPS * 60) / BPM);
export const INTRO = timing.introBeats * BEAT;
export const CHAPTER = timing.chapterBeats * BEAT;
export const SLIDE = timing.slideBeats * BEAT;
export const OUTRO = timing.outroBeats * BEAT;

export type Slide = { file: string; caption?: string; name?: string };
/** A part of a video: a chapter card, then its pictures. */
export type Part = { title: string; subtitle: string; number: number; slides: Slide[] };

/** The chapters of a video with their pictures present in SteamImg (empty chapters skipped). */
export const partsFor = (short: boolean): Part[] => {
  if (promo.length === 0) return [{ title: "TAOIST", subtitle: "", number: 1, slides: extra.map((file) => ({ file })) }];
  const used = new Set<string>();
  const parts = timing.chapters
    .map((c) => {
      const names = short ? c.short : c.pictures;
      names.forEach((n) => used.add(n));
      const slides = names
        .map((n) => promo.find((p) => p.name === n))
        .filter((p): p is PromoPicture => !!p)
        .map((p) => ({ file: p.file, caption: p.caption, name: p.name }));
      return { title: c.title, subtitle: c.subtitle, slides };
    })
    .filter((p) => p.slides.length > 0);
  // the long video also shows the pictures of no chapter, in the last one
  if (!short && parts.length > 0) {
    const rest = promo.filter((p) => !timing.chapters.some((c) => c.pictures.includes(p.name)));
    parts[parts.length - 1].slides.push(...rest.map((p) => ({ file: p.file, caption: p.caption, name: p.name })));
  }
  return parts.map((p, i) => ({ ...p, number: i + 1 }));
};

/** Frames of a video: intro + (chapter card + pictures) per part + outro; no overlap, so the music stays on the beat. */
export const durationOf = (parts: Part[]) =>
  INTRO + parts.reduce((s, p) => s + CHAPTER + p.slides.length * SLIDE, 0) + OUTRO;

/** Number of features shown (pictures), for the outro. */
export const featureCount = (parts: Part[]) => parts.reduce((s, p) => s + p.slides.length, 0);
