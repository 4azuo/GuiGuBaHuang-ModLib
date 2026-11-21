import { createContext, useContext } from "react";
import { AbsoluteFill, Audio, Easing, Img, interpolate, Sequence, staticFile, useCurrentFrame, useVideoConfig } from "remotion";
import { BEAT, CHAPTER, CREDIT, Part, Plan, SLIDE, Slide, featureCount, frameOf, picturesOf, romanOf, volumeOf, TAGLINE, ENDLINE } from "./data";
import { BRUSH, DISPLAY, SERIF } from "./fonts";
import { BurnRim, Burst, CloudBank, Drop, EdgeSparks, Gates, ImmortalOnCloud, Petals, Smoke, Talisman, Tree, Vapour, burnMask } from "./Effects";
import { BlobRim, Bolt, Chain, Crane, Cracks, Formation, Frost, Furnace, Lantern, Palm, SHARDS, Snow, StormSky, Taiji, blobMask, inOut, span } from "./Effects2";

export type PromoProps = { plan: Plan; music: string };

const PAPER = "#f3ead6";
const MUTED = "#cbbf9f";
const GOLD = "#e8b54a";
const SEAL = "#b8322a";

const clamp = { extrapolateLeft: "clamp", extrapolateRight: "clamp" } as const;
const outCubic = { easing: Easing.out(Easing.cubic), ...clamp };

/** Pseudo-random 0..1 (deterministic: the same frame renders the same). */
const rand = (i: number, seed: number) => {
  const v = Math.sin(i * 127.1 + seed * 311.7) * 43758.5453;
  return v - Math.floor(v);
};

/** Gold leaf text (gradient fill with a glow). */
const gold = (glow = 30): React.CSSProperties => ({
  background: "linear-gradient(180deg, #fff3cf 0%, #f2c766 45%, #c8902e 70%, #f7dc96 100%)",
  WebkitBackgroundClip: "text", backgroundClip: "text", color: "transparent",
  filter: `drop-shadow(0 0 ${glow}px rgba(232,181,74,0.45)) drop-shadow(0 4px 6px rgba(0,0,0,0.8))`,
});

/** Promo pictures, blurred and darkened (a new one every `every` frames, cross-fading): the background of the cards. */
const Backdrop: React.FC<{ files: string[]; every?: number; blur?: number; dim?: number }> = ({ files, every = 1e9, blur = 14, dim = 0.6 }) => {
  const frame = useCurrentFrame();
  const n = files.length;
  const i = Math.floor(frame / every);
  const t = (frame % every) / every;
  const fade = interpolate(t, [0.75, 1], [0, 1], clamp);
  const layer = (k: number, opacity: number) => (
    <Img key={k} src={staticFile(`steam/${files[k % n]}`)} style={{ position: "absolute", inset: 0, width: "100%", height: "100%",
      objectFit: "cover", opacity, filter: `blur(${blur}px) brightness(${dim}) saturate(1.05) sepia(0.12)`,
      transform: `scale(${1.2 + 0.06 * ((frame / 300) % 1)}) translateX(${(k % 2 ? 1 : -1) * frame * 0.15}px)` }} />
  );
  return (
    <AbsoluteFill style={{ backgroundColor: "#0b0906", overflow: "hidden" }}>
      {n > 0 && layer(i, 1)}
      {n > 1 && fade > 0 && layer(i + 1, fade)}
      <AbsoluteFill style={{ background: "radial-gradient(ellipse at 50% 45%, rgba(20,14,8,0) 40%, rgba(8,6,4,0.8) 100%)" }} />
    </AbsoluteFill>
  );
};

/** Drifting mist: bands of lit vapour moving slowly across. */
const Mist: React.FC<{ strength?: number }> = ({ strength = 1 }) => {
  const frame = useCurrentFrame();
  const bands = [
    { top: 74, h: 340, speed: 0.6, o: 0.42, seed: 2 },
    { top: 18, h: 280, speed: -0.4, o: 0.35, seed: 5 },
    { top: 92, h: 300, speed: 0.9, o: 0.45, seed: 8 },
  ];
  return (
    <AbsoluteFill style={{ pointerEvents: "none", overflow: "hidden" }}>
      {bands.map((b, i) => (
        <Vapour key={i} width={2600} height={b.h} seed={b.seed} t={frame} density={0.9} stretch={2.6} scale={1.1} opacity={b.o * strength}
          style={{ position: "absolute", top: `calc(${b.top}% - ${b.h / 2}px)`, left: -340 + ((frame * b.speed) % 400) - 200 }} />
      ))}
    </AbsoluteFill>
  );
};

/** The frame of the whole video (the frames of a Sequence start at 0). */
const VideoFrame = createContext(0);

/**
 * The mist lying low under the texts all through the video: two layers of lit vapour drifting in opposite ways at
 * their own pace, swelling and sinking a little (t: frames of the video, so it flows on from one scene to the next).
 */
const LowMist: React.FC<{ z?: number }> = ({ z = 240 }) => {
  const t = useContext(VideoFrame);
  return (
    <AbsoluteFill style={{ pointerEvents: "none", zIndex: z, overflow: "hidden" }}>
      <Vapour width={3000} height={380} seed={4} t={t} density={0.9} stretch={2.6} scale={1.1} opacity={0.5}
        style={{ position: "absolute", left: -540 + ((t * 0.9) % 400), top: 850 + Math.sin(t / 37) * 14 }} />
      <Vapour width={2800} height={300} seed={13} t={t} density={0.8} stretch={3} scale={0.9} opacity={0.38} tint="#f3e7d0"
        style={{ position: "absolute", left: -380 - ((t * 0.55) % 360), top: 930 + Math.sin(t / 29 + 1.3) * 18 }} />
    </AbsoluteFill>
  );
};

/** Motes of spiritual energy rising and swaying, flickering. */
const Qi: React.FC<{ count?: number }> = ({ count = 34 }) => {
  const frame = useCurrentFrame();
  return (
    <AbsoluteFill style={{ pointerEvents: "none" }}>
      {Array.from({ length: count }, (_, i) => {
        const speed = 0.35 + (i % 5) * 0.12;
        const y = 110 - ((frame * speed * 0.18 + i * 29) % 125);
        const x = ((i * 61.8) % 100) + Math.sin(frame / 40 + i) * 1.5;
        const s = 3 + (i % 4) * 2;
        const flicker = 0.35 + 0.35 * (0.5 + 0.5 * Math.sin(frame / 7 + i * 2.1));
        const c = i % 7 === 0 ? "#9fe3c4" : GOLD;
        return <div key={i} style={{ position: "absolute", left: `${x}%`, top: `${y}%`, width: s, height: s, borderRadius: s,
          background: c, opacity: flicker, boxShadow: `0 0 ${s * 4}px ${s}px ${c}66` }} />;
      })}
    </AbsoluteFill>
  );
};

/** A character written with the brush: revealed from the top like a stroke, from blur to sharp. */
const BrushGlyph: React.FC<{ char: string; size: number; delay?: number; duration?: number; opacity?: number; style?: React.CSSProperties }> =
  ({ char, size, delay = 0, duration = BEAT, opacity = 1, style }) => {
    const frame = useCurrentFrame();
    const p = interpolate(frame - delay, [0, duration], [0, 1], outCubic);
    const mask = `linear-gradient(165deg, #000 ${p * 130 - 25}%, transparent ${p * 130}%)`;
    return (
      <div style={{ fontFamily: BRUSH, fontSize: size, lineHeight: 1, opacity: opacity * Math.min(1, p * 3),
        transform: `scale(${1.08 - 0.08 * p})`, WebkitMaskImage: mask, maskImage: mask, ...style }}>
        <span style={gold(40)}>{char}</span>
      </div>
    );
  };

/** A red seal stamped on the paper (vertical text for several characters). */
const SealStamp: React.FC<{ text: string; size: number; delay?: number; style?: React.CSSProperties }> = ({ text, size, delay = 0, style }) => {
  const frame = useCurrentFrame();
  const s = interpolate(frame - delay, [0, BEAT * 0.8], [0, 1], { easing: Easing.out(Easing.quad), ...clamp });
  const vertical = text.length > 1;
  return (
    <div style={{ width: size, height: vertical ? size * (0.35 + 0.82 * text.length) : size, background: SEAL, borderRadius: size * 0.1,
      display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", color: "#f7e9d3",
      fontFamily: BRUSH, fontSize: size * (vertical ? 0.72 : 0.78), lineHeight: 1.12,
      boxShadow: `inset 0 0 0 ${size * 0.05}px ${SEAL}, inset 0 0 0 ${size * 0.08}px rgba(247,233,211,0.85), 0 4px 18px rgba(0,0,0,0.6)`,
      opacity: s, transform: `scale(${1.25 - 0.25 * s}) rotate(-6deg)`, ...style }}>
      {vertical ? text.split("").map((c, i) => <span key={i}>{c}</span>) : text}
    </div>
  );
};

/** Gold light at a cut (the first frames of a scene). */
const Flash: React.FC<{ strength?: number }> = ({ strength = 0.8 }) => {
  const frame = useCurrentFrame();
  const o = interpolate(frame, [0, BEAT], [strength * 0.35, 0], clamp);
  return <AbsoluteFill style={{ background: "radial-gradient(circle at 50% 50%, #fff6dc 0%, #f2c766 60%, #b07a25 100%)", opacity: o, pointerEvents: "none" }} />;
};

/** Every n-th picture, for the cross-fading backgrounds. */
const pick = (files: string[], count: number) => {
  const step = Math.max(1, Math.floor(files.length / count));
  return files.filter((_, i) => i % step === 0).slice(0, count);
};

/** Title: the game's name, the 道 written behind, TAOIST slams on the middle beat with its seal, then the tagline. */
const Intro: React.FC<{ beats: number; files: string[] }> = ({ beats, files }) => {
  const frame = useCurrentFrame();
  const u = (beats / 8) * BEAT;
  const pre = interpolate(frame, [0, u, u * 3.4, u * 4], [0, 1, 1, 0], clamp);
  const slam = interpolate(frame, [u * 4, u * 4 + BEAT * 2], [0, 1], { easing: Easing.inOut(Easing.quad), ...clamp });
  const tag = interpolate(frame, [u * 6, u * 6 + BEAT * 1.5], [0, 1], outCubic);
  return (
    <AbsoluteFill>
      <Backdrop files={pick(files, 4)} every={Math.round(u * 2)} dim={0.45} />
      <Mist />
      <LowMist z={0} />
      <AbsoluteFill style={{ justifyContent: "center", alignItems: "center" }}>
        <BrushGlyph char="道" size={860} delay={u} duration={u * 2.5} opacity={0.2} />
      </AbsoluteFill>
      <Qi />
      <AbsoluteFill style={{ justifyContent: "center", alignItems: "center", opacity: pre }}>
        <div style={{ textAlign: "center", transform: `scale(${1 + frame / 600})` }}>
          <div style={{ fontFamily: BRUSH, fontSize: 96, color: PAPER, letterSpacing: 24, textShadow: "0 4px 20px rgba(0,0,0,0.8)" }}>鬼谷八荒</div>
          <div style={{ fontFamily: DISPLAY, fontSize: 40, letterSpacing: 22, color: GOLD, marginTop: 10 }}>TALE OF IMMORTAL</div>
        </div>
      </AbsoluteFill>
      <AbsoluteFill style={{ justifyContent: "center", alignItems: "center", }}>
        <div style={{ display: "flex", alignItems: "center", gap: 40, opacity: slam, transform: `scale(${1.12 - 0.12 * slam})`, filter: `blur(${(1 - slam) * 10}px)` }}>
          <div style={{ fontFamily: DISPLAY, fontWeight: 900, fontSize: 230, letterSpacing: 24, ...gold(50) }}>TAOIST</div>
          <SealStamp text="道" size={130} delay={u * 5} />
        </div>
        <div style={{ position: "absolute", top: "68%", fontFamily: SERIF, fontStyle: "italic", fontSize: 54, color: PAPER, opacity: tag,
          transform: `translateY(${(1 - tag) * 30}px)`, textShadow: "0 3px 14px rgba(0,0,0,0.9)" }}>
          {TAGLINE[0]} <span style={{ color: GOLD, fontWeight: 700 }}>{TAGLINE[1]}</span>
        </div>
      </AbsoluteFill>
    </AbsoluteFill>
  );
};

/** A chapter: its first picture blurred behind, the brush character, the volume seal and the title. */
const ChapterCard: React.FC<{ part: Part }> = ({ part }) => {
  const frame = useCurrentFrame();
  const line = interpolate(frame, [BEAT * 0.2, BEAT * 0.9], [0, 1], outCubic);
  const vol = interpolate(frame, [BEAT * 0.4, BEAT * 0.9], [0, 1], outCubic);
  const title = interpolate(frame, [BEAT * 0.6, BEAT * 1.4], [0, 1], outCubic);
  const sub = interpolate(frame, [BEAT * 1.6, BEAT * 2.2], [0, 1], outCubic);
  const zoom = 1 + frame / (CHAPTER * BEAT * 30);
  return (
    <AbsoluteFill>
      <Backdrop files={[part.scenes[0][0].file]} blur={12} dim={0.6} />
      <AbsoluteFill style={{ background: "linear-gradient(90deg, rgba(8,6,4,0.8) 0%, rgba(8,6,4,0.55) 55%, rgba(8,6,4,0.2) 100%)" }} />
      <Mist strength={0.8} />
      <LowMist z={0} />
      <Qi count={22} />
      <AbsoluteFill style={{ flexDirection: "row", alignItems: "center", justifyContent: "center", gap: 70, transform: `scale(${zoom})` }}>
        <div style={{ position: "relative" }}>
          <BrushGlyph char={part.glyph} size={440} duration={BEAT * 0.9} />
          <SealStamp text={volumeOf(part.number)} size={78} delay={BEAT * 1.2} style={{ position: "absolute", right: -30, bottom: -10 }} />
        </div>
        <div style={{ width: 3, height: 380 * line, background: `linear-gradient(180deg, transparent, ${GOLD}, transparent)` }} />
        <div style={{ maxWidth: 1000 }}>
          <div style={{ fontFamily: DISPLAY, fontSize: 34, letterSpacing: 16, color: GOLD, opacity: vol }}>VOLUME {romanOf(part.number)}</div>
          <div style={{ fontFamily: DISPLAY, fontWeight: 700, fontSize: 104, lineHeight: 1.05, color: PAPER, marginTop: 14, opacity: title,
            letterSpacing: `${0.02 + (1 - title) * 0.25}em`, transform: `translateX(${(1 - title) * 40}px)`, textShadow: "0 6px 30px rgba(0,0,0,0.9)" }}>
            {part.title}
          </div>
          <div style={{ fontFamily: SERIF, fontStyle: "italic", fontSize: 42, color: MUTED, marginTop: 22, opacity: sub }}>{part.subtitle}</div>
        </div>
      </AbsoluteFill>
      <Flash strength={0.5} />
    </AbsoluteFill>
  );
};

/** Frame of the pictures: the front card fills most of the screen, the caption goes below it. */
const CARD = { left: 192, top: 34, width: 1536, height: 864 };

/** Frames of a picture coming to the front. */
const CARD_MOVE = BEAT * 1.25;

/**
 * How a scene of several pictures is shown: a stack of cards (the one shown leaving in one of 5 ways), a gallery, a
 * handscroll, an ascent through the heavens, a fan, a turning prism, a mosaic. The scenes take them in turn
 * (GROUP_ORDER), each way once in the long video.
 */
type Leave = "drift" | "rise" | "flip" | "shuffle" | "melt";
type Group = `stack-${Leave}` | "gallery" | "handscroll" | "ascend" | "fan" | "orbit" | "mosaic";
const GROUP_ORDER: Group[] = ["stack-flip", "gallery", "orbit", "fan", "handscroll", "stack-shuffle", "stack-rise", "stack-melt",
  "mosaic", "stack-drift", "ascend"];

/**
 * The card shown leaving the front of a stack of n cards (q: 0 -> 1): drifting away to the left on the wind, rising
 * and fading like qi, turned over like the page of a book, shuffled to the back of the stack, swelling and melting
 * into a haze.
 */
const cardLeave = (leave: Leave, q: number, n: number): React.CSSProperties => {
  switch (leave) {
    case "rise":
      return { transform: `translateY(${-760 * q}px) scale(${1 - 0.18 * q})`, opacity: 1 - q, filter: `blur(${8 * q}px) brightness(${1 + 0.4 * q})`, zIndex: 200 };
    case "flip":
      return { transformOrigin: "0% 50%", transform: `perspective(2200px) rotateY(${-100 * q}deg)`, filter: `brightness(${1 - 0.45 * q})`, zIndex: 200 };
    case "shuffle": {
      // down out of the stack, then up behind the others, to the back of the stack (on screen all the way)
      const back = n - 1;
      const x = 70 * back * q;
      const y = Math.sin(q * Math.PI) * 640;
      return { transform: `translate(${x}px, ${y}px) scale(${1 - 0.07 * back * q}) rotate(${Math.sin(q * Math.PI) * -4}deg)`,
        filter: `brightness(${1 - 0.22 * Math.min(back, 3) * q})`, opacity: q < 0.7 ? 1 : 1 - (q - 0.7) / 0.3, zIndex: q < 0.5 ? 200 : 1 };
    }
    case "melt":
      return { transformOrigin: "50% 50%", transform: `scale(${1 + 0.14 * q})`, opacity: 1 - q, filter: `blur(${12 * q}px) brightness(${1 + 0.3 * q})`, zIndex: 200 };
    default:
      return { transform: `translateX(${-2100 * q}px) rotate(${-4 * q}deg)`, zIndex: 200 };
  }
};

const FRAME_SHADOW = "0 0 0 1px rgba(232,181,74,0.55), 0 0 0 6px rgba(10,8,6,0.6), 0 18px 60px rgba(0,0,0,0.8)";

/** A picture in a gold-rimmed frame, whole (nothing cut off). */
const Card: React.FC<{ file: string; style: React.CSSProperties; children?: React.ReactNode }> = ({ file, style, children }) => (
  <div style={{ position: "absolute", borderRadius: 6, overflow: "hidden", backfaceVisibility: "hidden", boxShadow: FRAME_SHADOW, ...style }}>
    <Img src={staticFile(`steam/${file}`)} style={{ width: "100%", height: "100%", objectFit: "contain", backgroundColor: "black" }} />
    {children}
  </div>
);

/** Stack: the next cards wait behind, peeking out on the right; the one shown leaves in the way of the volume. */
const StackLayout: React.FC<{ slides: Slide[]; c: number; leave: Leave }> = ({ slides, c, leave }) => (
  <>
    {slides.map((slide, i) => {
      const d = i - c;
      if (d > 3.5 || d < -1) return null;
      const style: React.CSSProperties = d >= 0
        ? { transform: `translateX(${70 * d}px) scale(${1 - 0.07 * d})`, filter: d > 0.01 ? `brightness(${1 - 0.22 * Math.min(d, 3)})` : undefined, zIndex: 100 - Math.round(d * 10) }
        : cardLeave(leave, -d, slides.length);
      return <Card key={i} file={slide.file} style={{ ...CARD, transformOrigin: "100% 50%", ...style }} />;
    })}
  </>
);

/**
 * Gallery: each picture comes in big from the right, then shrinks to the left and takes its place in a column of the
 * pictures already shown.
 */
const GalleryLayout: React.FC<{ slides: Slide[]; c: number }> = ({ slides, c }) => {
  const big = { left: 548, top: 40, width: 1332, height: 749 };
  const n = slides.length;
  const th = Math.min(250, (big.height - 18 * (n - 2)) / Math.max(1, n - 1));
  const tw = (th * 16) / 9;
  return (
    <>
      {slides.map((slide, i) => {
        const d = i - c;
        if (d >= 1) return null;
        // d in (0, 1): coming in from the right; d in (-1, 0): going to its slot in the column; <= -1: in its slot
        const slot = { left: (548 - tw) / 2 - 10, top: big.top + i * (th + 18), width: tw, height: th };
        let box: { left: number; top: number; width: number; height: number };
        let extra: React.CSSProperties;
        if (d > 0) {
          box = { left: big.left + d * 1500, top: big.top - d * 40, width: big.width * (1 + 0.1 * d), height: big.height * (1 + 0.1 * d) };
          extra = { opacity: 1 - d * 0.4, zIndex: 150 };
        } else {
          const q = Math.min(1, -d);
          box = { left: big.left + (slot.left - big.left) * q, top: big.top + (slot.top - big.top) * q,
            width: big.width + (slot.width - big.width) * q, height: big.height + (slot.height - big.height) * q };
          extra = { zIndex: 100 - i, filter: q > 0.01 ? `brightness(${1 - 0.3 * q})` : undefined };
        }
        return <Card key={i} file={slide.file} style={{ ...box, ...extra }} />;
      })}
    </>
  );
};

/**
 * Handscroll: the pictures side by side on a long scroll between two rollers; the view pans from one to the next,
 * drawing back a little on the way. open (0 -> 1): the scroll unrolls from the middle when the scene comes in and
 * rolls up again (the two rollers meeting in the middle) when it leaves.
 */
const HandscrollLayout: React.FC<{ slides: Slide[]; c: number; open: number }> = ({ slides, c, open }) => {
  const step = CARD.width + 130;
  const moving = c - Math.floor(c);
  const zoom = 1 - 0.12 * Math.sin(moving * Math.PI);
  // half the width shown around the middle of the screen while rolled (the full scroll when open)
  const half = 40 + open * (CARD.width / 2 + 90);
  const rolling = open < 0.999;
  const roller = (left: number, turn: number) => (
    <div style={{ position: "absolute", left, top: CARD.top - 40, width: 44, height: CARD.height + 80, borderRadius: 22,
      background: `repeating-linear-gradient(180deg, rgba(0,0,0,0) 0 ${18 + (turn % 1) * 6}px, rgba(0,0,0,0.25) ${20 + (turn % 1) * 6}px 24px), linear-gradient(90deg, #2a1c10, #6b4a2a 40%, #3a2716 70%, #1a120a)`,
      boxShadow: "0 10px 30px rgba(0,0,0,0.8), inset 0 0 0 2px rgba(232,181,74,0.5)" }}>
      <div style={{ position: "absolute", left: -6, top: -18, width: 56, height: 22, borderRadius: 8, background: "linear-gradient(90deg, #8a6020, #f2c766, #8a6020)" }} />
      <div style={{ position: "absolute", left: -6, bottom: -18, width: 56, height: 22, borderRadius: 8, background: "linear-gradient(90deg, #8a6020, #f2c766, #8a6020)" }} />
    </div>
  );
  return (
    <AbsoluteFill style={{ transform: `scale(${zoom})`, transformOrigin: "50% 45%" }}>
      <AbsoluteFill style={rolling ? { clipPath: `inset(0 ${960 - half}px 0 ${960 - half}px)` } : {}}>
        <div style={{ position: "absolute", left: 0, top: 0, transform: `translateX(${-c * step}px)` }}>
          {/* the paper of the scroll behind the pictures */}
          <div style={{ position: "absolute", left: CARD.left - 70, top: CARD.top - 24, width: step * slides.length + 10, height: CARD.height + 48,
            background: "linear-gradient(180deg, #e9dcc0, #d8c6a0)", boxShadow: "0 20px 60px rgba(0,0,0,0.8)" }} />
          {slides.map((slide, i) => (
            <div key={i}>
              <Card file={slide.file} style={{ ...CARD, left: CARD.left + i * step, boxShadow: "0 0 0 2px rgba(120,80,30,0.8)" }} />
              {i > 0 && <div style={{ position: "absolute", left: CARD.left + i * step - 66, top: CARD.top + 40, width: 2, height: CARD.height - 80,
                background: `linear-gradient(180deg, transparent, ${GOLD}, transparent)` }} />}
            </div>
          ))}
          {!rolling && roller(CARD.left - 114, 0)}
          {!rolling && roller(CARD.left + step * (slides.length - 1) + CARD.width + 70, 0)}
        </div>
      </AbsoluteFill>
      {rolling && roller(960 - half - 44, half / 40)}
      {rolling && roller(960 + half, -half / 40)}
    </AbsoluteFill>
  );
};

const HEAVENS = ["第一重天", "第二重天", "第三重天", "第四重天", "第五重天", "第六重天", "第七重天", "第八重天", "第九重天"];

/**
 * Ascend: the pictures one above the other, each a heaven ("第一重天"...) above a layer of clouds; the view soars
 * straight up through the clouds (they rush by faster than the pictures) to the next heaven.
 */
const AscendLayout: React.FC<{ slides: Slide[]; c: number }> = ({ slides, c }) => {
  const gap = 1080;
  const moving = c - Math.floor(c);
  return (
    <>
      <div style={{ position: "absolute", left: 0, top: 0, transform: `translateY(${c * gap}px)` }}>
        {slides.map((slide, i) => (
          <Card key={i} file={slide.file} style={{ ...CARD, top: CARD.top - i * gap }}>
            <SealStamp text={HEAVENS[i] ?? `${i + 1}`} size={46} style={{ position: "absolute", right: 22, top: 18 }} />
          </Card>
        ))}
      </div>
      {/* the cloud layers between the heavens, passing faster (parallax) */}
      <div style={{ position: "absolute", left: 0, top: 0, width: 1920, height: 1080, transform: `translateY(${c * gap * 1.5}px)` }}>
        {slides.slice(1).map((_, i) => (
          <div key={i} style={{ position: "absolute", left: 0, top: 1330 - (i + 1) * gap * 1.5, width: 1920, height: 1 }}>
            <CloudBank x={960} y={0} vertical scale={1.2} seed={i + 3} />
          </div>
        ))}
      </div>
      {/* light of the heavens while soaring */}
      <AbsoluteFill style={{ pointerEvents: "none", opacity: Math.sin(moving * Math.PI) * 0.35,
        background: "radial-gradient(ellipse at 50% 30%, rgba(255,240,200,0.9), rgba(255,240,200,0) 70%)" }} />
    </>
  );
};

/**
 * Fan: the cards lie one on the other; when the next comes, they spread out like a fan of cards and close again with
 * the next one on top.
 */
const FanLayout: React.FC<{ slides: Slide[]; c: number }> = ({ slides, c }) => {
  const spread = Math.sin((c - Math.floor(c)) * Math.PI);
  return (
    <>
      {slides.map((slide, i) => {
        const d = i - c;
        const ad = Math.abs(d);
        return <Card key={i} file={slide.file} style={{ ...CARD, transformOrigin: "50% 190%", zIndex: 100 - Math.round(ad * 10),
          transform: `rotate(${d * 13 * spread}deg) scale(${1 - 0.05 * Math.min(ad, 2) * spread})`,
          filter: ad > 0.05 ? `brightness(${1 - 0.3 * Math.min(ad, 2)})` : undefined }} />;
      })}
    </>
  );
};

/** Orbit: the pictures on the faces of a prism turning in space, the next face coming round to the front. */
const OrbitLayout: React.FC<{ slides: Slide[]; c: number }> = ({ slides, c }) => {
  const n = slides.length;
  const theta = 360 / Math.max(n, 3);
  const w = 1300, h = (w * 9) / 16;
  const r = (w / 2) / Math.tan(Math.PI / Math.max(n, 3)) + 40;
  return (
    <AbsoluteFill style={{ perspective: 3200, perspectiveOrigin: "50% 40%" }}>
      <div style={{ position: "absolute", left: 960 - w / 2, top: 70, width: w, height: h, transformStyle: "preserve-3d",
        transform: `translateZ(${-r}px) rotateY(${-c * theta}deg)` }}>
        {slides.map((slide, i) => {
          const off = Math.abs(i - c);
          return <Card key={i} file={slide.file} style={{ left: 0, top: 0, width: w, height: h,
            transform: `rotateY(${i * theta}deg) translateZ(${r}px)`, filter: `brightness(${1 - 0.5 * Math.min(off, 1)})` }} />;
        })}
      </div>
    </AbsoluteFill>
  );
};

/** Mosaic: all the pictures small in a grid; the one shown rises out of its cell to full size, then goes back. */
const MosaicLayout: React.FC<{ slides: Slide[]; c: number }> = ({ slides, c }) => {
  const n = slides.length;
  const cols = n <= 4 ? 2 : 3;
  const rows = Math.ceil(n / cols);
  const cw = Math.min(720, (1700 - 40 * (cols - 1)) / cols), ch = (cw * 9) / 16;
  const x0 = 960 - (cols * cw + (cols - 1) * 40) / 2, y0 = 470 - (rows * ch + (rows - 1) * 34) / 2;
  return (
    <>
      {slides.map((slide, i) => {
        const cell = { left: x0 + (i % cols) * (cw + 40), top: y0 + Math.floor(i / cols) * (ch + 34), width: cw, height: ch };
        const q = Easing.inOut(Easing.quad)(Math.max(0, 1 - Math.abs(i - c)));
        const box = { left: cell.left + (CARD.left - cell.left) * q, top: cell.top + (CARD.top - cell.top) * q,
          width: cell.width + (CARD.width - cell.width) * q, height: cell.height + (CARD.height - cell.height) * q };
        return <Card key={i} file={slide.file} style={{ ...box, zIndex: Math.round(q * 100), filter: q < 0.99 ? `brightness(${0.55 + 0.45 * q})` : undefined }} />;
      })}
    </>
  );
};

/**
 * A scene: one picture, or the pictures of one feature shown in the way given (Group),
 * each SLIDE beats, over the one shown blurred. Below: the chapter, "2 / 3", the caption with a 新 seal.
 */
const FeatureScene: React.FC<{ slides: Slide[]; part: Part; group: Group; enterFrames: number; nextEnterFrames: number }> =
  ({ slides, part, group, enterFrames: inFrames, nextEnterFrames }) => {
    const frame = useCurrentFrame();
    const per = SLIDE * BEAT;
    const k = Math.min(slides.length - 1, Math.floor(frame / per));
    const local = frame - k * per;
    // position of the pictures (k when settled, from k-1 to k while the next one comes)
    const c = k > 0 && local < CARD_MOVE ? k - 1 + Easing.inOut(Easing.cubic)(local / CARD_MOVE) : k;
    // the caption comes in once the scene is in (the one of the previous scene is gone), leaves as the next comes in
    const end = slides.length * per;
    const leave = interpolate(frame, [end, end + nextEnterFrames * 0.35], [1, 0], clamp);
    const caption = k === 0
      ? interpolate(frame, [inFrames * 0.6, inFrames * 0.6 + BEAT], [0, 1], outCubic)
      : interpolate(local, [0, BEAT * 0.35, BEAT * 0.5, BEAT], [1, 0, 0, 1], clamp);
    const shown = k > 0 && local < BEAT * 0.5 ? k - 1 : k;
    const text = slides[shown].caption ?? "";
    // one line: smaller type for a long caption
    const size = Math.min(50, Math.floor(1500 / Math.max(1, text.length * 0.52)));
    const layout = slides.length === 1 ? <StackLayout slides={slides} c={c} leave="drift" />
      : group.startsWith("stack-") ? <StackLayout slides={slides} c={c} leave={group.slice(6) as Leave} />
      : group === "gallery" ? <GalleryLayout slides={slides} c={c} />
      : group === "handscroll" ? <HandscrollLayout slides={slides} c={c} open={Math.min(
        Easing.inOut(Easing.cubic)(interpolate(frame, [inFrames * 0.3, inFrames * 0.3 + BEAT * 1.2], [0, 1], clamp)),
        Easing.inOut(Easing.cubic)(interpolate(frame, [end - BEAT * 1.1, end + nextEnterFrames * 0.3], [1, 0], clamp)))} />
      : group === "fan" ? <FanLayout slides={slides} c={c} />
      : group === "orbit" ? <OrbitLayout slides={slides} c={c} />
      : group === "mosaic" ? <MosaicLayout slides={slides} c={c} />
      : <AscendLayout slides={slides} c={c} />;
    return (
      <AbsoluteFill style={{ backgroundColor: "black", overflow: "hidden" }}>
        <Backdrop files={slides.map((x) => x.file)} every={Math.round(per)} blur={28} dim={0.38} />
        {layout}
        {/* the mist lying low, under the caption */}
        <LowMist />
        {/* motes of qi drifting in front */}
        <AbsoluteFill style={{ opacity: 0.5, zIndex: 250, pointerEvents: "none" }}><Qi count={16} /></AbsoluteFill>
        <div style={{ position: "absolute", left: CARD.left, right: CARD.left, top: CARD.top + CARD.height + 22, height: 140, display: "flex",
          alignItems: "center", gap: 26, opacity: (k === 0 ? caption : 1) * leave, zIndex: 300 }}>
          <SealStamp text="新" size={70} delay={inFrames * 0.6} />
          <div style={{ minWidth: 0 }}>
            <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 4 }}>
              <span style={{ fontFamily: BRUSH, fontSize: 30, ...gold(8) }}>{part.glyph}</span>
              <span style={{ fontFamily: DISPLAY, fontSize: 20, letterSpacing: 6, color: GOLD }}>{part.title.toUpperCase()}</span>
              {slides.length > 1 && (
                <span style={{ fontFamily: DISPLAY, fontSize: 20, letterSpacing: 4, color: MUTED, marginLeft: 10 }}>{shown + 1} / {slides.length}</span>
              )}
            </div>
            <div style={{ fontFamily: SERIF, fontSize: size, fontWeight: 700, color: PAPER, whiteSpace: "nowrap", textShadow: "0 3px 12px rgba(0,0,0,0.9)",
              opacity: k === 0 ? 1 : caption, transform: `translateY(${(1 - caption) * 10}px)` }}>
              {text}
            </div>
          </div>
        </div>
      </AbsoluteFill>
    );
  };

/** End card: the pictures flying by behind, the feature count, TAOIST with its seal, where to get it, the music credit. */
const Outro: React.FC<{ beats: number; count: number; files: string[] }> = ({ beats, count, files }) => {
  const frame = useCurrentFrame();
  const u = (beats / 8) * BEAT;
  const a = interpolate(frame, [0, BEAT * 2], [0, 1], { easing: Easing.inOut(Easing.quad), ...clamp });
  const b = interpolate(frame, [u * 2, u * 2 + BEAT * 1.5], [0, 1], outCubic);
  const fade = interpolate(frame, [beats * BEAT - u * 1.5, beats * BEAT], [1, 0], clamp);
  return (
    <AbsoluteFill style={{ opacity: fade }}>
      <Backdrop files={files} every={Math.round(BEAT * 4)} dim={0.42} />
      <Mist />
      <LowMist z={0} />
      <AbsoluteFill style={{ justifyContent: "center", alignItems: "center" }}>
        <BrushGlyph char="道" size={860} duration={u * 2} opacity={0.16} />
      </AbsoluteFill>
      <Qi />
      <AbsoluteFill style={{ justifyContent: "center", alignItems: "center", textAlign: "center" }}>
        <div style={{ fontFamily: DISPLAY, fontSize: 52, fontWeight: 700, letterSpacing: 8, color: GOLD, opacity: a }}>{ENDLINE}</div>
        <div style={{ display: "flex", alignItems: "center", gap: 36, margin: "10px 0", transform: `scale(${1.1 - 0.1 * a})`, opacity: a }}>
          <div style={{ fontFamily: DISPLAY, fontWeight: 900, fontSize: 200, letterSpacing: 22, ...gold(50) }}>TAOIST</div>
          <SealStamp text="道" size={112} delay={u} />
        </div>
        <div style={{ fontFamily: SERIF, fontSize: 52, color: PAPER, opacity: b, textShadow: "0 3px 12px black" }}>Subscribe on the Steam Workshop</div>
        <div style={{ fontFamily: SERIF, fontStyle: "italic", fontSize: 32, marginTop: 14, opacity: b, color: MUTED }}>Requires the ModLib framework mod</div>
      </AbsoluteFill>
      <div style={{ position: "absolute", bottom: 36, width: "100%", textAlign: "center", fontFamily: SERIF, fontSize: 24, color: "#a99c80", opacity: b }}>
        {CREDIT}
      </div>
      <Flash strength={0.85} />
    </AbsoluteFill>
  );
};

/**
 * The ways a scene comes in, all soft, each with its particles: "focus" (from blur), "mist" (a fog bank from the left),
 * "qi" (spreading from the centre), "scroll" (opening from the middle like a hanging scroll), "ink" (a diagonal wash),
 * "dream" (out of a haze), "cloud" (a bank of auspicious clouds sweeps the old scene away), "rider" (an immortal on a
 * flying cloud, the new scene in its wake), "ascend" (soaring up through the clouds into the scene above), "talisman"
 * (a paper talisman flies in and burns, the new scene spreading from the fire), "sword" (a flying sword slashes across,
 * the scene opening along the cut), "moon" (a round moon gate of gold opening), "bamboo" (bamboo slips opening one
 * after the other), "pillar" (a pillar of heavenly light coming down and widening), "petals" (peach blossom drifting
 * across), "gates" (red temple gates closing, then opening on the new scene), "ripple" (a drop falls from a leaf into a lake, the rings opening the scene).
 * The scenes take them in turn (ENTER_ORDER): each about once or twice in the long video.
 */
type EnterKind = "focus" | "mist" | "qi" | "scroll" | "ink" | "dream" | "cloud" | "rider" | "ascend"
  | "talisman" | "sword" | "moon" | "bamboo" | "pillar" | "petals" | "gates" | "ripple"
  | "thunder" | "array" | "taiji" | "cranes" | "frost" | "inkdrop" | "palm" | "furnace" | "lanterns" | "seal";
/**
 * The transitions in order, one per scene of the long video (chosen for what comes: the yin-yang for a new
 * character, a formation for the rules, ink for the log, a tribulation for the techniques, the palm for the damage,
 * temple gates for the bank, lanterns for the market, frost for the seclusion, cranes for the team, a seal for the
 * bounties, the furnace for the storylines...).
 */
const ENTER_ORDER: EnterKind[] = ["mist", "taiji", "array", "cloud", "bamboo", "inkdrop",
  "qi", "moon", "sword", "thunder", "scroll", "ascend", "palm",
  "pillar", "gates", "lanterns", "ripple", "rider",
  "petals", "dream", "frost", "cranes", "seal",
  "ink", "talisman", "furnace"];

/** Frames of a transition. */
const ENTER_BEATS: Partial<Record<EnterKind, number>> = { thunder: 2.25, array: 2, taiji: 2, cranes: 2, frost: 2.25, inkdrop: 2,
  palm: 2, furnace: 2.5, lanterns: 2.25, seal: 2.5, cloud: 1.5, rider: 2, ascend: 2, talisman: 2, gates: 2, ripple: 2.75, petals: 1.75, sword: 1.5 };
const enterFrames = (kind: EnterKind) => Math.round(BEAT * (ENTER_BEATS[kind] ?? 1.25));
const TRANS = enterFrames("mist");

const masked = (mask: string): React.CSSProperties => ({ WebkitMaskImage: mask, maskImage: mask });

/** A scene coming in over the previous one during its first frames (the previous scene keeps playing under it). */
const Enter: React.FC<{ kind: EnterKind; children: React.ReactNode }> = ({ kind, children }) => {
  const frame = useCurrentFrame();
  const dur = enterFrames(kind);
  const p = interpolate(frame, [0, dur], [0, 1], { easing: Easing.inOut(Easing.sin), ...clamp });
  const lin = interpolate(frame, [0, dur], [0, 1], clamp);
  let style: React.CSSProperties = {};
  let over: React.ReactNode = null;
  if (lin < 1) {
    switch (kind) {
      case "focus": style = { filter: `blur(${(1 - p) * 18}px)`, opacity: p }; break;
      case "mist": {
        const x = p * 140 - 40;
        style = masked(`linear-gradient(90deg, #000 ${x}%, transparent ${x + 40}%)`);
        over = <>
          <Vapour width={900} height={1500} seed={6} t={frame} density={1.1} scale={1.2} opacity={Math.sin(p * Math.PI) * 0.9}
            style={{ position: "absolute", left: ((p * 140 - 20) / 100) * 1920 - 450, top: -210 }} />
          <EdgeSparks x={((p * 140 - 20) / 100) * 1920} t={frame} count={30} color="#fff3d6" />
        </>;
        break;
      }
      case "qi": {
        const r = p * 1500 - 300;
        style = masked(`radial-gradient(circle at 50% 50%, #000 ${r}px, transparent ${r + 300}px)`);
        over = <Burst x={960} y={540} p={lin} count={70} color="#9fe3c4" power={1100} />;
        break;
      }
      case "scroll": {
        const w = p * 62;
        style = { ...masked(`linear-gradient(90deg, transparent ${50 - w - 12}%, #000 ${50 - w}%, #000 ${50 + w}%, transparent ${50 + w + 12}%)`), opacity: Math.min(1, p * 3) };
        over = <>
          <EdgeSparks x={960 - (w / 100) * 1920} t={frame} count={22} dir={-1} />
          <EdgeSparks x={960 + (w / 100) * 1920} t={frame + 7} count={22} dir={1} />
        </>;
        break;
      }
      case "ink": {
        const x = p * 160 - 50;
        style = masked(`linear-gradient(125deg, #000 ${x}%, transparent ${x + 50}%)`);
        over = <EdgeSparks x={((x + 25) / 100) * 1920 * 0.9} angle={35} t={frame} count={30} color="#d8c7a4" spread={2200} />;
        break;
      }
      case "dream": {
        style = { opacity: p, filter: `blur(${(1 - p) * 12}px) brightness(${1.35 - 0.35 * p})`, transform: `scale(${1.05 - 0.05 * p})` };
        over = <AbsoluteFill style={{ opacity: Math.sin(p * Math.PI) }}><Qi count={60} /></AbsoluteFill>;
        break;
      }
      case "cloud": {
        // the bank crosses the screen; behind it (to its left) the new scene
        const x = -1150 + lin * 4220;
        const fade = Math.min(1, lin / 0.15, (1 - lin) / 0.15);
        style = masked(`linear-gradient(90deg, #000 ${x - 300}px, transparent ${x + 100}px)`);
        over = <div style={{ opacity: fade }}><CloudBank x={x} scale={1.3} t={frame} /><EdgeSparks x={x + 200} t={frame} count={30} dir={1} /></div>;
        break;
      }
      case "rider": {
        const x = -500 + lin * 2900;
        const y = 380 - Math.sin(lin * Math.PI) * 110 + Math.sin(frame / 6) * 6;
        style = masked(`linear-gradient(100deg, #000 ${x - 260}px, transparent ${x + 40}px)`);
        over = <>
          <EdgeSparks x={x - 300} y={y + 120} t={frame} count={40} spread={300} />
          <div style={{ position: "absolute", left: x - 470, top: y - 250 }}>
            <ImmortalOnCloud width={560} t={frame} />
          </div>
        </>;
        break;
      }
      case "ascend": {
        // the new scene comes down from above (the view soars up), its lower edge lost in the clouds rushing down
        const y = (1 - p) * -1080;
        style = { transform: `translateY(${y}px)`, ...masked("linear-gradient(180deg, #000 0%, #000 80%, transparent 100%)") };
        // the band rides on the soft edge, coming from above the screen and going on below it
        const band = -700 + p * 2500;
        const fade = Math.min(1, lin / 0.2, (1 - lin) / 0.2);
        over = <div style={{ opacity: fade }}><CloudBank x={960} y={band} vertical scale={1.6} t={frame} /><EdgeSparks x={960} y={band} angle={90} t={frame} count={40} spread={2000} /></div>;
        break;
      }
      case "thunder": {
        // the sky darkens under storm clouds, lightning strikes; the new scene cracks open from the strike
        const sky = inOut(lin, 0, 0.25, 0.75, 1);
        const bolt = span(lin, 0.4, 0.43) * (1 - span(lin, 0.5, 0.6));
        const open = Easing.out(Easing.quad)(span(lin, 0.43, 1));
        const sx = 1120, sy = 700;
        style = open > 0 ? blobMask(sx, sy, open * 1900, 9, 220, 0.008) : { opacity: 0 };
        over = <>
          <StormSky o={sky} t={frame} />
          {bolt > 0 && <Bolt x0={980} y0={-40} x1={sx} y1={sy} seed={3} opacity={bolt} />}
          <AbsoluteFill style={{ background: "radial-gradient(circle at 58% 60%, rgba(220,200,255,0.55), rgba(160,120,255,0) 60%)", opacity: bolt * 0.6 }} />
          {open > 0 && <BlobRim cx={sx} cy={sy} r={open * 1900} color="#c7a6ff" width={18} seed={9} rough={220} freq={0.008} blur={10} opacity={1 - open} />}
          {lin > 0.42 && <EdgeSparks x={sx} y={sy} t={frame} count={40} spread={500} color="#d9c6ff" dir={0} />}
        </>;
        break;
      }
      case "array": {
        // a formation appears and turns, then opens as a portal onto the new scene
        const grow = Easing.out(Easing.cubic)(span(lin, 0, 0.4));
        const open = Easing.inOut(Easing.cubic)(span(lin, 0.38, 1));
        const size = 260 + grow * 840 + open * 2600;
        const r = open * 1250;
        style = open > 0 ? masked(`radial-gradient(circle at 50% 50%, #000 ${r}px, transparent ${r + 120}px)`) : { opacity: 0 };
        over = <>
          <AbsoluteFill style={{ justifyContent: "center", alignItems: "center", opacity: inOut(lin, 0, 0.15, 0.7, 1) }}>
            <Formation size={size} t={frame * 1.5} glow={1 + open} />
          </AbsoluteFill>
          <EdgeSparks x={960} y={540} t={frame} count={40} spread={size * 0.6} color="#ffe39a" dir={0} />
        </>;
        break;
      }
      case "taiji": {
        // the yin-yang turns and swells over the whole screen, then fades away on the new scene
        const g = Easing.inOut(Easing.cubic)(span(lin, 0, 0.5));
        const size = 160 + g * 2300 + span(lin, 0.5, 1) * 1800;
        style = { opacity: lin < 0.5 ? 0 : 1 };
        over = <AbsoluteFill style={{ justifyContent: "center", alignItems: "center", opacity: 1 - Easing.in(Easing.quad)(span(lin, 0.52, 1)) }}>
          <Taiji size={size} angle={frame * 7} glow={1 + g} />
        </AbsoluteFill>;
        break;
      }
      case "cranes": {
        // a flight of cranes crosses the sky, the new scene coming in behind them
        const lead = -300 + lin * 2800;
        style = masked(`linear-gradient(100deg, #000 ${lead - 420}px, transparent ${lead - 80}px)`);
        const flock = [[0, 0], [-260, 130], [-260, -110], [-520, 240], [-520, -200], [-780, 60]];
        over = <>
          <Vapour width={2600} height={420} seed={22} t={frame} density={0.9} stretch={2.6} opacity={inOut(lin, 0, 0.2, 0.8, 1) * 0.6}
            style={{ position: "absolute", left: -340, top: 120 }} />
          {flock.map(([dx, dy], i) => (
            <div key={i} style={{ position: "absolute", left: lead + dx - 120, top: 300 + dy - Math.sin(lin * Math.PI) * 60 + Math.sin(frame / 9 + i) * 8 }}>
              <Vapour width={460} height={70} seed={40 + i} t={frame} density={0.8} stretch={4} opacity={0.6} style={{ position: "absolute", left: -380, top: 30 }} />
              <Crane size={i === 0 ? 380 : 330 - i * 14} t={frame} phase={i * 1.3} />
            </div>
          ))}
        </>;
        break;
      }
      case "inkdrop": {
        // a drop of ink falls, strikes the middle and spreads; the new scene is in the ink
        const fall = Easing.in(Easing.quad)(span(lin, 0, 0.22));
        const spread = Easing.out(Easing.cubic)(span(lin, 0.22, 1));
        const r = spread * 1800;
        style = spread > 0 ? blobMask(960, 540, r, 5, 260, 0.012) : { opacity: 0 };
        over = <>
          {spread === 0 && <div style={{ position: "absolute", left: 960 - 14, top: -60 + fall * 600, width: 28, height: 38,
            borderRadius: "50% 50% 50% 50% / 60% 60% 40% 40%", background: "radial-gradient(circle at 35% 35%, #5a5a5a 0 10%, #111 40%, #000 100%)" }} />}
          {spread > 0 && <BlobRim cx={960} cy={540} r={r} color="#0a0806" width={60} seed={5} rough={260} freq={0.012} blur={12} opacity={0.85 * (1 - spread)} />}
          {spread > 0 && <Burst x={960} y={540} p={Math.min(1, spread * 2)} count={30} color="#151210" power={420} />}
        </>;
        break;
      }
      case "palm": {
        // a palm of golden light thrusts in; its shockwave ripples space and opens on the new scene
        const push = Easing.out(Easing.cubic)(span(lin, 0, 0.4));
        const hit = span(lin, 0.38, 1);
        const px = 2300 - push * 1200, py = 540;
        const r = Easing.out(Easing.quad)(hit) * 1900;
        style = hit > 0 ? masked(`radial-gradient(circle at ${px}px ${py}px, #000 ${r}px, transparent ${r + 160}px)`) : { opacity: 0 };
        over = <>
          <div style={{ position: "absolute", left: px - 210, top: py - 262, transform: `scale(${(0.55 + push * 0.45) * (1 + hit * 0.5)})`,
            transformOrigin: "50% 50%", opacity: 1 - span(lin, 0.55, 0.9) }}><Palm size={420} /></div>
          {[0, 1, 2].map((i) => {
            const rr = Math.max(0, r - i * 160);
            return hit > 0 && <div key={i} style={{ position: "absolute", left: px - rr, top: py - rr, width: rr * 2, height: rr * 2, borderRadius: "50%",
              border: "6px solid rgba(255,225,150,0.75)", boxShadow: "0 0 40px 10px rgba(255,210,120,0.5)", opacity: (1 - hit) * (1 - i * 0.25) }} />;
          })}
          {hit > 0 && <Burst x={px} y={py} p={hit} count={60} color="#ffe39a" power={1400} />}
        </>;
        break;
      }
      case "furnace": {
        // the alchemy furnace rises, opens: fire and smoke billow over the screen; it sinks again on the new scene
        const rise = Easing.out(Easing.cubic)(span(lin, 0, 0.25)) * (1 - Easing.in(Easing.cubic)(span(lin, 0.72, 0.96)));
        const lid = Easing.out(Easing.back(2))(span(lin, 0.2, 0.34)) * 130;
        const fire = span(lin, 0.18, 0.3) * (1 - span(lin, 0.7, 0.9));
        const cover = inOut(lin, 0.28, 0.5, 0.62, 0.95);
        style = { opacity: span(lin, 0.47, 0.55) };
        const fy = 1250 - rise * 560;
        over = <>
          {[0, 1, 2, 3, 4].map((i) => {
            const up = span(lin, 0.25 + i * 0.03, 0.75);
            const w = 900 + up * 1700 + i * 200, h = 700 + up * 900;
            return <Vapour key={i} width={w} height={h} seed={60 + i * 3} t={frame} density={1.7} scale={0.9} tint={i % 2 ? "#d9cdbf" : "#cfc2b0"} light={0.9}
              opacity={cover * 0.97} style={{ position: "absolute", left: 960 - w / 2 + (i - 2) * 260, top: fy - 200 - h / 2 - up * 450 + (i % 2) * 120 }} />;
          })}
          <AbsoluteFill style={{ background: "radial-gradient(circle at 50% 70%, rgba(255,150,60,0.35), rgba(255,150,60,0) 55%)", opacity: fire }} />
          <div style={{ position: "absolute", left: 960 - 260, top: fy - 250 }}><Furnace w={520} lid={lid} fire={fire} /></div>
          {fire > 0 && <EdgeSparks x={960} y={fy - 320} angle={90} t={frame} count={40} spread={260} color="#ffb347" />}
        </>;
        break;
      }
      case "lanterns": {
        // night falls; sky lanterns drift up by the dozen, the new scene rising behind them
        const night = inOut(lin, 0, 0.22, 0.75, 1);
        const front = lin * 150 - 40;
        style = masked(`linear-gradient(0deg, #000 ${front}%, transparent ${front + 40}%)`);
        over = <>
          <AbsoluteFill style={{ background: "linear-gradient(180deg, rgba(8,10,30,0.75), rgba(20,16,40,0.45))", opacity: night }} />
          {Array.from({ length: 38 }, (_, i) => {
            const st = rand(i, 71) * 0.35;
            const q = span(lin, st, st + 0.62);
            if (q <= 0 || q >= 1) return null;
            const size = 36 + rand(i, 72) * 60;
            const x = rand(i, 73) * 1920 + Math.sin(frame / 15 + i) * 20;
            const y = 1160 - q * (1350 + size * 4);
            return <div key={i} style={{ position: "absolute", left: x, top: y, opacity: inOut(q, 0, 0.1, 0.8, 1), transform: `rotate(${Math.sin(frame / 20 + i) * 5}deg)` }}>
              <Lantern size={size} flicker={0.85 + 0.15 * Math.sin(frame / 3 + i * 2)} /></div>;
          })}
          <AbsoluteFill style={{ background: "radial-gradient(ellipse at 50% 100%, rgba(255,170,80,0.3), rgba(255,170,80,0) 60%)", opacity: night }} />
        </>;
        break;
      }
      case "seal": {
        // golden chains bind the picture, talismans at its corners, the seal 封 stamps down: it cracks open on the new scene
        const chains = Easing.inOut(Easing.quad)(span(lin, 0, 0.3));
        const tal = span(lin, 0.12, 0.32);
        const stamp = Easing.out(Easing.cubic)(span(lin, 0.34, 0.48));
        const crack = span(lin, 0.48, 0.62);
        const open = Easing.in(Easing.quad)(span(lin, 0.55, 1));
        const gone = 1 - span(lin, 0.62, 0.92);
        style = open > 0 ? blobMask(960, 540, open * 1900, 12, 200, 0.01) : { opacity: 0 };
        over = <>
          <div style={{ opacity: gone }}>
            <Chain x0={-60} y0={-40} x1={1980} y1={1120} p={chains} />
            <Chain x0={1980} y0={-40} x1={-60} y1={1120} p={chains} />
            {[[60, 60], [1740, 60], [60, 660], [1740, 660]].map(([x, y], i) => (
              <div key={i} style={{ position: "absolute", left: x, top: y, opacity: tal, transform: `scale(${0.6 + tal * 0.4}) rotate(${(i % 2 ? 1 : -1) * 8}deg)` }}>
                <Talisman height={340} /></div>
            ))}
            {stamp > 0 && <AbsoluteFill style={{ justifyContent: "center", alignItems: "center" }}>
              <SealStamp text="封" size={300} style={{ transform: `scale(${2.2 - 1.2 * stamp}) rotate(-6deg)`, opacity: stamp }} />
            </AbsoluteFill>}
            {crack > 0 && <Cracks cx={960} cy={540} p={crack} />}
          </div>
          {crack > 0 && <div style={{ position: "absolute", left: 960 - crack * 1000, top: 540 - crack * 1000, width: crack * 2000, height: crack * 2000, borderRadius: "50%",
            border: "8px solid rgba(220,60,40,0.8)", boxShadow: "0 0 50px 14px rgba(220,60,40,0.5)", opacity: 1 - crack }} />}
          {open > 0 && <Burst x={960} y={540} p={open} count={60} color="#ffcf9a" power={1500} />}
        </>;
        break;
      }
      case "frost":
      case "talisman":
      case "sword":
        // the scene going out is cut / burnt over this one (Exit)
        break;
      case "moon": {
        const r = p * 1250;
        style = masked(`radial-gradient(circle at 50% 50%, #000 ${r}px, transparent ${r + 6}px)`);
        over = <>
          <div style={{ position: "absolute", left: 960 - r - 14, top: 540 - r - 14, width: 2 * r + 28, height: 2 * r + 28, borderRadius: "50%",
            border: "14px solid #c8902e", boxShadow: "0 0 40px 10px rgba(232,181,74,0.6), inset 0 0 30px rgba(232,181,74,0.6)", opacity: 1 - lin * 0.4 }} />
          <Burst x={960} y={540} p={lin} count={50} color="#ffe39a" power={r + 80} />
        </>;
        break;
      }
      case "bamboo": {
        // 8 slips opening from their middle, one after the other from the left
        const N = 8;
        const stops: string[] = [];
        for (let i = 0; i < N; i++) {
          const t = Math.max(0, Math.min(1, p * 1.7 - (i / (N - 1)) * 0.7));
          const c = ((i + 0.5) / N) * 100, half = (t * 100) / N / 2 + (t >= 1 ? 0.5 : 0);
          stops.push(`transparent ${c - half}%`, `#000 ${c - half}%`, `#000 ${c + half}%`, `transparent ${c + half}%`);
        }
        style = masked(`linear-gradient(90deg, ${stops.join(", ")})`);
        over = <EdgeSparks x={p * 1920 * 1.2 - 100} t={frame} count={30} color="#cfe8a0" />;
        break;
      }
      case "pillar": {
        // a pillar of light comes down, then widens over the screen
        const down = Math.min(1, lin / 0.35);
        const wide = Math.max(0, (lin - 0.3) / 0.7);
        const w = 4 + Easing.in(Easing.quad)(wide) * 62;
        style = wide > 0 ? masked(`linear-gradient(90deg, transparent ${50 - w - 6}%, #000 ${50 - w}%, #000 ${50 + w}%, transparent ${50 + w + 6}%)`) : { opacity: 0 };
        over = <>
          <div style={{ position: "absolute", left: 960 - (w / 100) * 1920 - 60, top: 0, width: (w / 50) * 1920 + 120, height: 1080 * down,
            background: "linear-gradient(90deg, rgba(255,244,214,0), rgba(255,244,214,0.85) 30%, rgba(255,255,255,0.95) 50%, rgba(255,244,214,0.85) 70%, rgba(255,244,214,0))",
            opacity: wide < 0.6 ? 1 : 1 - (wide - 0.6) / 0.4, filter: "blur(8px)" }} />
          <EdgeSparks x={960} angle={0} t={frame} count={50} spread={1080 * down} color="#fffbe8" />
        </>;
        break;
      }
      case "petals": {
        style = masked(`linear-gradient(225deg, #000 ${p * 150 - 40}%, transparent ${p * 150 + 10}%)`);
        over = <>
          <div style={{ opacity: Math.min(1, Math.sin(lin * Math.PI) * 2.2) }}>
            <Tree x={1990} y={1140} size={1.6} lean={-0.5} seed={9} kind="blossom" sway={Math.sin(frame / 12) * 0.8} />
          </div>
          <Petals p={lin} t={frame} count={180} />
        </>;
        break;
      }
      case "gates": {
        // closed over the old scene, the new one behind them, then open
        const open = lin < 0.4 ? 1 - Easing.inOut(Easing.cubic)(lin / 0.4) : lin < 0.55 ? 0 : Easing.inOut(Easing.cubic)((lin - 0.55) / 0.45);
        style = { opacity: lin < 0.45 ? 0 : 1 };
        over = <><Gates open={open} />{lin > 0.55 && <EdgeSparks x={960} t={frame} count={40} spread={1080} dir={0} />}</>;
        break;
      }
      case "ripple": {
        // a drop gathers at the tip of a leaf on a bough of the tree, rests there half a beat, falls and strikes the
        // lake: rings spread, the new scene in them
        const cx = 1300, cy = 640, tipY = 300;
        const grow = span(lin, 0.05, 0.2);
        const fall = span(lin, 0.2 + (BEAT * 0.5) / dur, 0.48);
        const ring = span(lin, 0.48, 1);
        const r = Easing.out(Easing.quad)(ring) * 1700 - 200;
        style = ring > 0 ? masked(`radial-gradient(ellipse ${Math.max(1, r)}px ${Math.max(1, r * 0.8)}px at ${cx}px ${cy}px, #000 70%, transparent 100%)`) : { opacity: 0 };
        const dropY = tipY + 6 + Easing.in(Easing.quad)(fall) * (cy - tipY - 6);
        over = <AbsoluteFill>
          <div style={{ opacity: ring < 0.5 ? Math.min(1, lin * 6) : 1 - (ring - 0.5) * 2 }}>
            <Tree x={-40} y={1130} size={1.55} lean={0.42} seed={4} kind="leaf" sway={Math.sin(frame / 14) * 0.15} reach={{ x: cx, y: tipY }} />
          </div>
          {ring === 0 && grow > 0 && <Drop x={cx} y={dropY + 12 * grow} size={8 + 14 * grow} stretch={1 + fall * 0.5 + (fall === 0 ? Math.sin(frame / 3) * 0.04 : 0)} />}
          {ring > 0 && <Burst x={cx} y={cy} p={Math.min(1, ring * 2.5)} count={26} color="#d8ecff" power={160} />}
          {[0, 1, 2, 3, 4].map((i) => {
            const rr = Math.max(0, Easing.out(Easing.quad)(ring) * 1600 - i * 120);
            if (ring <= 0 || rr <= 0) return null;
            return <div key={i} style={{ position: "absolute", left: cx - rr, top: cy - rr * 0.4, width: 2 * rr, height: 0.8 * rr, borderRadius: "50%",
              border: `${3 - i * 0.4}px solid rgba(235,248,255,0.8)`, opacity: (1 - ring) * (1 - i * 0.15), boxShadow: "0 0 14px rgba(220,240,255,0.6)" }} />;
          })}
        </AbsoluteFill>;
        break;
      }
    }
  }
  // isolate: the z-index of the cards of a scene stays inside it (else the cards of the scene going out cover this one)
  return (
    <>
      <AbsoluteFill style={{ isolation: "isolate", ...style }}>{children}</AbsoluteFill>
      {over && <AbsoluteFill style={{ pointerEvents: "none", overflow: "hidden" }}>{over}</AbsoluteFill>}
    </>
  );
};

/**
 * A scene going out over the next one (which waits under it) when the next comes in by "sword" (a flying sword
 * slashes across: the scene splits along the cut, its two halves sliding apart and falling away like space
 * collapsing, shards along the cut) or by "talisman" (a talisman flies onto the picture and sets it alight: it burns
 * away from there, a ragged hole with a charred rim and a line of fire, embers and smoke); else nothing.
 */
const Exit: React.FC<{ kind: EnterKind; start: number; dur: number; children: React.ReactNode }> = ({ kind, start, dur, children }) => {
  const frame = useCurrentFrame();
  if ((kind !== "sword" && kind !== "talisman" && kind !== "frost" && kind !== "palm") || frame < start) return <>{children}</>;
  const q = Math.min(1, (frame - start) / dur);
  const t = frame - start;
  if (kind === "palm") {
    // the shockwave of the palm ripples the space of the scene going out
    const amount = Math.sin(span(q, 0.36, 1) * Math.PI) * 70;
    return (
      <AbsoluteFill>
        <svg width={0} height={0} style={{ position: "absolute" }}>
          <filter id="palm-ripple" x="-10%" y="-10%" width="120%" height="120%">
            <feTurbulence type="turbulence" baseFrequency={`${0.004 + q * 0.004} 0.02`} numOctaves={2} seed={4} result="n" />
            <feDisplacementMap in="SourceGraphic" in2="n" scale={amount} xChannelSelector="R" yChannelSelector="G" />
          </filter>
        </svg>
        <AbsoluteFill style={{ filter: amount > 0.5 ? "url(#palm-ripple)" : undefined }}>{children}</AbsoluteFill>
      </AbsoluteFill>
    );
  }
  if (kind === "frost") {
    // snow, frost creeping in from the corners, then the frozen picture shatters and its shards fall away
    const frost = Easing.out(Easing.quad)(span(q, 0, 0.45));
    const sh = span(q, 0.42, 1);
    const snow = inOut(q, 0, 0.12, 0.85, 1);
    return (
      <AbsoluteFill>
        {sh === 0 ? (
          <AbsoluteFill style={{ filter: `saturate(${1 - frost * 0.5}) brightness(${1 + frost * 0.12})` }}>
            {children}
            <Frost p={frost} t={frame} />
          </AbsoluteFill>
        ) : SHARDS.map((sd, i) => {
          const fall = Easing.in(Easing.quad)(sh);
          const dx = (sd.cx - 960) * 0.35 * sh, dy = fall * (900 + rand(i, 81) * 700) - sh * 60;
          const rot = (rand(i, 82) - 0.5) * 70 * sh;
          const pts = sd.pts.map(([x, y]) => `${x}px ${y}px`).join(", ");
          return (
            <AbsoluteFill key={i} style={{ transform: `translate(${dx}px, ${dy}px) rotate(${rot}deg)`, transformOrigin: `${sd.cx}px ${sd.cy}px`,
              opacity: 1 - span(sh, 0.55, 1) }}>
              <AbsoluteFill style={{ clipPath: `polygon(${pts})`, filter: "saturate(0.5) brightness(1.12)" }}>{children}</AbsoluteFill>
              <svg width={1920} height={1080} style={{ position: "absolute", left: 0, top: 0, overflow: "visible" }}>
                <polygon points={sd.pts.map(([x, y]) => `${x},${y}`).join(" ")} fill="rgba(200,235,255,0.18)" stroke="rgba(235,250,255,0.9)" strokeWidth={3} />
              </svg>
            </AbsoluteFill>
          );
        })}
        <Snow t={frame} o={snow} />
        {sh > 0 && <Burst x={960} y={540} p={sh} count={50} color="#e8f6ff" power={1200} />}
      </AbsoluteFill>
    );
  }
  if (kind === "sword") {
    // the cut: a line through the centre, 30 degrees down to the right
    const slash = Math.min(1, q / 0.3);
    const o = Easing.in(Easing.quad)(Math.max(0, (q - 0.22) / 0.78));
    const nx = 0.5, ny = -0.866;
    const sx = -1500 + Easing.out(Easing.cubic)(slash) * 3000;
    const upper = "polygon(-772px -460px, 2692px 1540px, 2692px -460px)";
    const lower = "polygon(-772px -460px, 2692px 1540px, -772px 1540px)";
    const half = (clip: string, sign: number) => (
      <AbsoluteFill style={{ clipPath: clip, transformOrigin: "50% 50%",
        transform: `translate(${sign * nx * 420 * o}px, ${sign * ny * 420 * o + 900 * o * o}px) rotate(${sign * 5 * o}deg) scale(${1 - 0.06 * o})`,
        filter: `brightness(${1 - 0.5 * o})`, opacity: 1 - Math.max(0, o - 0.6) / 0.4 }}>{children}</AbsoluteFill>
    );
    return (
      <AbsoluteFill>
        {o > 0 ? <>{half(upper, 1)}{half(lower, -1)}</> : children}
        {/* the cut glowing, then the sword light */}
        {q > 0.15 && <div style={{ position: "absolute", left: 960 - 1300, top: 537, width: 2600, height: 6, transform: "rotate(30deg)",
          background: "linear-gradient(90deg, transparent, #fff3d0 20%, #ffffff 50%, #fff3d0 80%, transparent)",
          boxShadow: "0 0 40px 12px rgba(255,220,150,0.85)", opacity: Math.max(0, 1 - o * 2.5) }} />}
        {slash < 1 && <div style={{ position: "absolute", left: 960 + sx * 0.866 - 550, top: 540 + sx * 0.5 - 3, width: 1100, height: 8,
          transform: "rotate(30deg)", background: "linear-gradient(90deg, rgba(255,240,200,0), #fff3d0 80%, #ffffff)",
          boxShadow: "0 0 30px 10px rgba(255,220,150,0.8)" }} />}
        {o > 0 && <EdgeSparks x={960} angle={60} t={t} count={70} spread={2600} color="#fff1c4" dir={1} />}
        {o > 0 && <Burst x={960} y={540} p={o} count={50} color="#e8dcc0" power={900} />}
      </AbsoluteFill>
    );
  }
  // talisman: flies in (first 35 %), then the picture burns from it
  const fly = Easing.out(Easing.cubic)(Math.min(1, q / 0.35));
  const burn = Math.max(0, (q - 0.32) / 0.68);
  const tx = -300 + fly * 1260, ty = 500;
  const r = Easing.in(Easing.quad)(burn) * 1700;
  return (
    <AbsoluteFill>
      <AbsoluteFill style={burn > 0 ? burnMask(tx, ty, r) : {}}>{children}</AbsoluteFill>
      {burn > 0 && <BurnRim cx={tx} cy={ty} r={r} />}
      {burn < 0.35 && <div style={{ position: "absolute", left: tx - 80, top: ty - 220, opacity: 1 - burn / 0.35,
        transform: `rotate(${(1 - fly) * -40}deg)`, filter: `brightness(${1 + burn * 6}) sepia(${burn * 2})` }}><Talisman height={440} /></div>}
      {burn > 0 && <Burst x={tx} y={ty} p={Math.min(1, burn * 1.4)} count={90} color="#ffb347" power={r + 200} />}
      {burn > 0 && <EdgeSparks x={tx} y={ty} angle={0} t={t} count={40} spread={Math.max(200, r * 1.6)} color="#ff9a2e" dir={0} />}
      {burn > 0 && <Smoke x={tx} y={ty} p={burn} t={frame} tint="#6e655c" size={1.3} />}
    </AbsoluteFill>
  );
};

/** The video: every scene starts on a beat and comes in over the previous one (Enter: the ways in turn); the music is
 *  cut to the same timeline (scripts/cut-music.py), its drops on chapter cards. */
export const Promo: React.FC<PromoProps> = ({ plan, music }) => {
  const { durationInFrames } = useVideoConfig();
  const videoFrame = useCurrentFrame();
  const files = picturesOf(plan);
  // the scenes in order: what comes in how (the scene before stays under it as long as the transition lasts)
  type Item = { beats: number; kind: EnterKind; node: (enter: number, next: number) => React.ReactNode };
  const items: Item[] = [];
  let groups = 0;
  for (const part of plan.parts) {
    items.push({ beats: CHAPTER, kind: ENTER_ORDER[items.length % ENTER_ORDER.length], node: () => <ChapterCard part={part} /> });
    part.scenes.forEach((slides) => {
      const kind = ENTER_ORDER[items.length % ENTER_ORDER.length];
      const group: Group = slides.length > 1 ? GROUP_ORDER[groups++ % GROUP_ORDER.length] : "stack-drift";
      items.push({ beats: slides.length * SLIDE, kind, node: (enter, next) =>
        <FeatureScene slides={slides} part={part} group={group} enterFrames={enter} nextEnterFrames={next} /> });
    });
  }
  const outroKind: EnterKind = "focus";
  let at = plan.intro;
  const scenes = items.map((it, i) => {
    const nextKind = i + 1 < items.length ? items[i + 1].kind : outroKind;
    const next = enterFrames(nextKind);
    const from = frameOf(at);
    const own = frameOf(at + it.beats) - from;
    const over = nextKind === "sword" || nextKind === "talisman" || nextKind === "frost";
    const seq = (
      <Sequence key={i} from={from} durationInFrames={own + next} style={over ? { zIndex: 1 } : undefined}>
        <Exit kind={nextKind} start={own} dur={next}>
          <Enter kind={it.kind}>{it.node(enterFrames(it.kind), next)}</Enter>
        </Exit>
      </Sequence>
    );
    at += it.beats;
    return seq;
  });
  const first = items.length > 0 ? enterFrames(items[0].kind) : TRANS;
  return (
    <VideoFrame.Provider value={videoFrame}>
    <AbsoluteFill style={{ backgroundColor: "black" }}>
      <Audio src={staticFile(music)} volume={(f) => interpolate(f, [0, 4, durationInFrames - 10, durationInFrames], [0, 1, 1, 0], clamp)} />
      <Sequence from={0} durationInFrames={frameOf(plan.intro) + first}><Intro beats={plan.intro} files={files} /></Sequence>
      {scenes}
      <Sequence from={frameOf(at)} durationInFrames={frameOf(at + plan.outro) - frameOf(at)}>
        <Enter kind={outroKind}><Outro beats={plan.outro} count={featureCount(plan)} files={files} /></Enter>
      </Sequence>
    </AbsoluteFill>
    </VideoFrame.Provider>
  );
};
