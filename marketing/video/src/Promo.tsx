import { AbsoluteFill, Audio, Easing, Img, interpolate, Sequence, spring, staticFile, useCurrentFrame, useVideoConfig } from "remotion";
import { BEAT, CHAPTER, INTRO, OUTRO, Part, SLIDE, Slide, featureCount } from "./data";

export type PromoProps = { parts: Part[]; music: string };

const INK = "#14120f";
const PAPER = "#f3ead6";
const GOLD = "#e8b54a";
const SEAL = "#c0392b";
const SERIF = "Georgia, 'Times New Roman', serif";
const SANS = "'Segoe UI', 'Helvetica Neue', Arial, sans-serif";

const clamp = { extrapolateLeft: "clamp", extrapolateRight: "clamp" } as const;

/** 0..1 pulse at every beat (1 on the beat, falling fast): the picture "punches" with the kick. */
const beatPulse = (frame: number) => {
  const p = (frame % BEAT) / BEAT;
  return Math.exp(-p * 6);
};

/** Dark ink background with drifting gold sparks, shared by the cards. */
const Stage: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const frame = useCurrentFrame();
  const sparks = Array.from({ length: 28 }, (_, i) => {
    const x = (i * 137.5) % 100;
    const y = 100 - ((frame * (0.25 + (i % 5) * 0.08) + i * 23) % 120);
    const s = 3 + (i % 4) * 2;
    return <div key={i} style={{ position: "absolute", left: `${x}%`, top: `${y}%`, width: s, height: s, borderRadius: s,
      background: GOLD, opacity: 0.25 + (i % 3) * 0.15, boxShadow: `0 0 ${s * 3}px ${GOLD}` }} />;
  });
  return (
    <AbsoluteFill style={{ background: `radial-gradient(ellipse at 50% 40%, #3a2a1a 0%, #1a130c 55%, #080604 100%)`,
      justifyContent: "center", alignItems: "center", color: PAPER, fontFamily: SERIF, overflow: "hidden" }}>
      {sparks}
      {children}
    </AbsoluteFill>
  );
};

/** White flash at a cut (the first frames of a scene). */
const Flash: React.FC<{ strength?: number }> = ({ strength = 0.8 }) => {
  const frame = useCurrentFrame();
  const o = interpolate(frame, [0, 5], [strength, 0], clamp);
  return <AbsoluteFill style={{ background: "white", opacity: o, pointerEvents: "none" }} />;
};

const Intro: React.FC = () => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  // the title slams on beat 4, the tagline on beat 6
  const slam = spring({ frame: frame - BEAT * 4, fps, config: { damping: 9, stiffness: 220 } });
  const shake = frame >= BEAT * 4 && frame < BEAT * 4 + 8 ? Math.sin(frame * 3) * (BEAT * 4 + 8 - frame) * 1.5 : 0;
  const tag = interpolate(frame, [BEAT * 6, BEAT * 6 + 8], [0, 1], clamp);
  const pre = interpolate(frame, [0, BEAT, BEAT * 3.5, BEAT * 4], [0, 1, 1, 0], clamp);
  return (
    <Stage>
      <div style={{ position: "absolute", fontFamily: SANS, fontSize: 54, letterSpacing: 18, color: GOLD, opacity: pre,
        transform: `scale(${1 + frame / 400})` }}>
        TALE OF IMMORTAL
      </div>
      <div style={{ textAlign: "center", transform: `translateX(${shake}px)` }}>
        <div style={{ fontSize: 210, fontWeight: 700, letterSpacing: 30, opacity: slam,
          transform: `scale(${2.2 - 1.2 * slam})`, textShadow: `0 0 40px ${GOLD}` }}>
          TAOIST
        </div>
        <div style={{ fontFamily: SANS, fontSize: 52, marginTop: 10, opacity: tag, letterSpacing: 4,
          transform: `translateY(${(1 - tag) * 30}px)` }}>
          Your cultivation journey, <span style={{ color: GOLD, fontWeight: 700 }}>upgraded.</span>
        </div>
      </div>
      {frame >= BEAT * 4 && <Sequence from={BEAT * 4}><Flash strength={0.9} /></Sequence>}
    </Stage>
  );
};

const ChapterCard: React.FC<{ part: Part }> = ({ part }) => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const num = spring({ frame, fps, config: { damping: 12, stiffness: 200 } });
  const title = spring({ frame: frame - BEAT, fps, config: { damping: 14, stiffness: 180 } });
  const sub = interpolate(frame, [BEAT * 2, BEAT * 2 + 6], [0, 1], clamp);
  const zoom = 1 + frame / (CHAPTER * 6);
  return (
    <Stage>
      <div style={{ textAlign: "center", transform: `scale(${zoom})` }}>
        <div style={{ fontFamily: SANS, fontSize: 40, letterSpacing: 14, color: GOLD, opacity: num,
          transform: `translateY(${(1 - num) * -40}px)` }}>
          {`0${part.number}`.slice(-2)} — NEW
        </div>
        <div style={{ fontSize: 130, fontWeight: 700, marginTop: 10, opacity: title,
          transform: `scale(${1.6 - 0.6 * title})`, textShadow: "0 6px 30px rgba(0,0,0,0.8)" }}>
          {part.title}
        </div>
        <div style={{ fontFamily: SANS, fontSize: 42, marginTop: 24, opacity: sub }}>{part.subtitle}</div>
      </div>
      <Flash strength={0.6} />
    </Stage>
  );
};

/** One picture: punch-in on the cut, a pulse on every beat, a caption that slams in with a "NEW" badge. */
const PictureScene: React.FC<{ slide: Slide; index: number }> = ({ slide, index }) => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const enter = spring({ frame, fps, config: { damping: 16, stiffness: 260 } });
  const drift = interpolate(frame, [0, SLIDE], [0, 1], { easing: Easing.inOut(Easing.quad), ...clamp });
  const scale = 1.25 - 0.17 * enter + 0.04 * drift + 0.012 * beatPulse(frame);
  const dx = (index % 2 === 0 ? -1 : 1) * 24 * drift;
  const caption = spring({ frame: frame - 4, fps, config: { damping: 14, stiffness: 220 } });
  const badge = spring({ frame: frame - 8, fps, config: { damping: 8, stiffness: 260 } });
  return (
    <AbsoluteFill style={{ backgroundColor: "black", overflow: "hidden" }}>
      <Img src={staticFile(`steam/${slide.file}`)}
        style={{ width: "100%", height: "100%", objectFit: "cover", transform: `scale(${scale}) translateX(${dx}px)` }} />
      <AbsoluteFill style={{ background: "linear-gradient(0deg, rgba(0,0,0,0.75) 0%, rgba(0,0,0,0) 35%)" }} />
      {slide.caption && (
        <AbsoluteFill style={{ justifyContent: "flex-end", alignItems: "flex-start", padding: "0 0 70px 70px" }}>
          <div style={{ display: "flex", alignItems: "center", gap: 24, transform: `translateX(${(1 - caption) * -120}px)`, opacity: caption,
            background: "linear-gradient(90deg, rgba(12,10,8,0.88) 0%, rgba(12,10,8,0.8) 80%, rgba(12,10,8,0) 100%)", padding: "14px 120px 14px 18px", borderRadius: 8 }}>
            <div style={{ fontFamily: SANS, fontWeight: 800, fontSize: 30, color: INK, background: GOLD, padding: "6px 16px",
              borderRadius: 6, transform: `scale(${badge}) rotate(-4deg)` }}>NEW</div>
            <div style={{ fontFamily: SERIF, fontSize: 54, fontWeight: 700, color: PAPER, maxWidth: 1500,
              textShadow: "0 3px 12px rgba(0,0,0,0.9)", borderLeft: `8px solid ${SEAL}`, paddingLeft: 22 }}>
              {slide.caption}
            </div>
          </div>
        </AbsoluteFill>
      )}
      <Flash strength={0.35} />
    </AbsoluteFill>
  );
};

const Outro: React.FC<{ count: number }> = ({ count }) => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const a = spring({ frame, fps, config: { damping: 10, stiffness: 200 } });
  const n = Math.round(interpolate(frame, [0, BEAT * 3], [0, count], clamp));
  const b = interpolate(frame, [BEAT * 4, BEAT * 4 + 8], [0, 1], clamp);
  const fade = interpolate(frame, [OUTRO - BEAT * 2, OUTRO], [1, 0], clamp);
  return (
    <Stage>
      <div style={{ textAlign: "center", opacity: fade }}>
        <div style={{ fontFamily: SANS, fontSize: 64, color: GOLD, fontWeight: 800, opacity: a }}>
          {n}+ features in one mod
        </div>
        <div style={{ fontSize: 190, fontWeight: 700, letterSpacing: 26, transform: `scale(${1.5 - 0.5 * a})`, opacity: a,
          textShadow: `0 0 50px ${GOLD}` }}>TAOIST</div>
        <div style={{ fontFamily: SANS, fontSize: 52, marginTop: 20, opacity: b }}>Subscribe on the Steam Workshop</div>
        <div style={{ fontFamily: SANS, fontSize: 32, marginTop: 14, opacity: b, color: "#cbbf9f" }}>Requires the ModLib framework mod</div>
      </div>
      <Flash strength={0.9} />
    </Stage>
  );
};

/** Frames of a transition: the next scene comes in over the previous one, starting on the beat of the cut. */
const TRANS = 9;

/** The ways a scene comes in, in turn: iris, wipe, slide, punch zoom, ink diagonal, blur, spin, flash, blinds. */
const ENTERS = ["iris", "wipe", "slide", "punch", "ink", "blur", "spin", "flash", "blinds"] as const;
type EnterKind = (typeof ENTERS)[number];

/** A scene coming in over the previous one during its first TRANS frames (the previous scene keeps playing under it). */
const Enter: React.FC<{ kind: EnterKind; children: React.ReactNode }> = ({ kind, children }) => {
  const frame = useCurrentFrame();
  const p = interpolate(frame, [0, TRANS], [0, 1], { easing: Easing.out(Easing.cubic), ...clamp });
  let style: React.CSSProperties = {};
  switch (kind) {
    case "iris": style = { clipPath: `circle(${p * 80}% at 50% 50%)` }; break;
    case "wipe": style = { clipPath: `inset(0 ${100 - p * 100}% 0 0)` }; break;
    case "slide": style = { transform: `translateX(${(1 - p) * 100}%)` }; break;
    case "punch": style = { transform: `scale(${1.6 - 0.6 * p})`, opacity: p }; break;
    case "ink": {
      // a diagonal edge sweeping from the top left, like a brush stroke
      const x = -60 + p * 220;
      style = { clipPath: `polygon(0 0, ${x}% 0, ${x - 60}% 100%, 0 100%)` };
      break;
    }
    case "blur": style = { filter: `blur(${(1 - p) * 24}px)`, opacity: p }; break;
    case "spin": style = { transform: `rotate(${(1 - p) * -12}deg) scale(${1.3 - 0.3 * p})`, opacity: p }; break;
    case "flash": style = { opacity: p }; break;
    case "blinds": {
      // 6 vertical strips opening together (a repeating mask)
      const w = (p * 100) / 6;
      const mask = `repeating-linear-gradient(90deg, #000 0 ${w}%, transparent ${w}% ${100 / 6}%)`;
      style = p >= 1 ? {} : { WebkitMaskImage: mask, maskImage: mask };
      break;
    }
  }
  return (
    <AbsoluteFill style={style}>
      {children}
      {kind === "flash" && <AbsoluteFill style={{ background: "white", opacity: interpolate(frame, [0, 3, TRANS], [0, 0.9, 0], clamp) }} />}
    </AbsoluteFill>
  );
};

/** The video: every scene starts on a beat and comes in over the previous one (Enter); the music is cut to the same
 *  timeline (scripts/make-music.py). */
export const Promo: React.FC<PromoProps> = ({ parts, music }) => {
  const { durationInFrames } = useVideoConfig();
  let at = INTRO;
  let index = 0;
  const scenes: React.ReactNode[] = [];
  for (const part of parts) {
    scenes.push(
      <Sequence key={`c${part.number}`} from={at} durationInFrames={CHAPTER + TRANS}>
        <Enter kind="ink"><ChapterCard part={part} /></Enter>
      </Sequence>);
    at += CHAPTER;
    for (const s of part.slides) {
      const kind = ENTERS[index % ENTERS.length];
      scenes.push(
        <Sequence key={`s${index}`} from={at} durationInFrames={SLIDE + TRANS}>
          <Enter kind={kind}><PictureScene slide={s} index={index} /></Enter>
        </Sequence>);
      at += SLIDE;
      index++;
    }
  }
  return (
    <AbsoluteFill style={{ backgroundColor: "black" }}>
      <Audio src={staticFile(music)} volume={(f) => interpolate(f, [0, 6, durationInFrames - 45, durationInFrames], [0, 1, 1, 0], clamp)} />
      <Sequence from={0} durationInFrames={INTRO + TRANS}><Intro /></Sequence>
      {scenes}
      <Sequence from={at} durationInFrames={OUTRO}><Enter kind="iris"><Outro count={featureCount(parts)} /></Enter></Sequence>
    </AbsoluteFill>
  );
};
