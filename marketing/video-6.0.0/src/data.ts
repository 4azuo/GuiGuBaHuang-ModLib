import generated from "./promo.generated.json";
import timing from "./chapters.json";

export type Slide = { file: string; caption?: string; name?: string };
/** A chapter of a video: a card (title over its first picture, blurred), then its scenes: one picture each, or the
 *  pictures of one feature (cards coming to the front in turn). */
export type Part = { title: string; subtitle: string; glyph: string; number: number; scenes: Slide[][] };
/** A video (written by scripts/prepare-assets.mjs): intro and outro in beats, the chapters. */
export type Plan = { intro: number; outro: number; parts: Part[] };

export const plans = generated as { long: Plan; short: Plan };

export const FPS = timing.fps;
export const BPM = timing.music.bpm;
export const CREDIT = timing.music.credit;
/** The tagline of the intro (its last word in gold) and the line above TAOIST in the end card. */
export const TAGLINE: [string, string] = ((timing as any).tagline ?? ["Your cultivation journey,", "ascended."]) as [string, string];
export const ENDLINE: string = (timing as any).endline ?? "A LIVING CULTIVATION WORLD";
/** Frames of a beat (16.07 at 30 fps and 112 BPM): a scene starts on the nearest frame of its beat. */
export const BEAT = (FPS * 60) / BPM;
/** The frame of a beat of the video. */
export const frameOf = (beats: number) => Math.round(beats * BEAT);
export const CHAPTER = timing.chapterBeats;
export const SLIDE = timing.slideBeats;

/** Beats of a video: intro + (chapter card + pictures) per part + outro; no overlap, so the music stays on the beat. */
export const beatsOf = (plan: Plan) =>
  plan.intro + plan.parts.reduce((s, p) => s + CHAPTER + p.scenes.flat().length * SLIDE, 0) + plan.outro;

/** Number of features shown (pictures), for the outro. */
export const featureCount = (plan: Plan) => plan.parts.reduce((s, p) => s + p.scenes.flat().length, 0);

/** Every picture of a video, in order (backgrounds of the intro and the outro). */
export const picturesOf = (plan: Plan) => plan.parts.flatMap((p) => p.scenes.flat().map((s) => s.file));

const NUMERALS = ["一", "二", "三", "四", "五", "六", "七", "八", "九", "十"];
/** "第一卷" (volume one, as in a cultivation novel). */
export const volumeOf = (n: number) => `第${NUMERALS[n - 1] ?? n}卷`;
const ROMAN = ["I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"];
export const romanOf = (n: number) => ROMAN[n - 1] ?? `${n}`;
