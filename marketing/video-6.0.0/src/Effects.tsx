import { useId } from "react";
import { AbsoluteFill } from "remotion";

const GOLD = "#e8b54a";

/** Circles of an auspicious cloud (祥云), in a 0..300 x 0..130 box: [cx, cy, r]. */
const CLOUD_PUFFS: [number, number, number][] = [
  [60, 80, 32], [104, 58, 42], [158, 50, 48], [212, 64, 38], [250, 84, 26], [30, 94, 18], [130, 88, 32], [190, 90, 30],
];

/**
 * An auspicious cloud (祥云) painted softly: puffs shaded from warm white to parchment, a thin faded gold edge (drawn
 * under the fill, so only the outline shows), two spiral curls; flip mirrors it.
 */
export const Cloud: React.FC<{ width: number; opacity?: number; flip?: boolean; style?: React.CSSProperties }> =
  ({ width, opacity = 1, flip = false, style }) => {
    const id = useId().replace(/:/g, "");
    return (
      <svg width={width} height={(width * 130) / 300} viewBox="-10 0 320 130"
        style={{ overflow: "visible", opacity, transform: flip ? "scaleX(-1)" : undefined, filter: "drop-shadow(0 6px 14px rgba(0,0,0,0.25))", ...style }}>
        <defs>
          <radialGradient id={`c${id}`} cx="50%" cy="30%" r="75%">
            <stop offset="0%" stopColor="#fffaf0" />
            <stop offset="60%" stopColor="#f1e6cd" />
            <stop offset="100%" stopColor="#d6c197" />
          </radialGradient>
        </defs>
        <g fill="rgba(176,122,37,0.45)">{CLOUD_PUFFS.map(([x, y, r], i) => <circle key={i} cx={x} cy={y} r={r + 2} />)}</g>
        <rect x={18} y={92} width={250} height={32} rx={16} fill="rgba(176,122,37,0.45)" transform="translate(0 2)" />
        <g fill={`url(#c${id})`}>
          {CLOUD_PUFFS.map(([x, y, r], i) => <circle key={i} cx={x} cy={y} r={r} />)}
          <rect x={18} y={92} width={250} height={32} rx={16} />
        </g>
        <g fill="none" stroke={GOLD} strokeWidth={2} strokeLinecap="round" opacity={0.6}>
          <path d="M150 72 a13 13 0 1 1 13 -13 a21 21 0 1 1 -23 25" />
          <path d="M212 86 a8 8 0 1 0 -8 -8 a14 14 0 1 0 15 15" />
        </g>
      </svg>
    );
  };

/** A soft fog bank (blurred light). */
export const Fog: React.FC<{ width: number; height: number; opacity?: number; style?: React.CSSProperties }> = ({ width, height, opacity = 0.9, style }) => (
  <div style={{ width, height, borderRadius: "50%", opacity, filter: "blur(36px)",
    background: "radial-gradient(ellipse at 50% 50%, rgba(246,239,224,1) 0%, rgba(243,234,214,0.85) 40%, rgba(243,234,214,0) 70%)", ...style }} />
);

/**
 * An immortal standing on a flying cloud, facing right (the way it flies): a robed figure in ink with a gold rim, a
 * sword on the back, sleeve, hair and two sashes streaming behind; t (frames) makes them ripple.
 */
export const ImmortalOnCloud: React.FC<{ width: number; t: number }> = ({ width, t }) => {
  const w = Math.sin(t / 5) * 6;
  return (
    <div style={{ position: "relative", width, height: width * 0.95 }}>
      {/* trail of mist behind */}
      <Vapour width={width * 2.2} height={width * 0.42} seed={21} t={t} density={1.1} stretch={3} scale={0.7} opacity={0.9}
        style={{ position: "absolute", right: width * 0.3, top: width * 0.55 }} />
      <svg viewBox="-200 0 360 240" width={width} height={width * 0.66} style={{ position: "absolute", left: width * 0.08, top: 0, overflow: "visible",
        filter: "drop-shadow(0 0 12px rgba(232,181,74,0.6))" }}>
        <g fill="none" stroke={GOLD} strokeWidth={3} strokeLinecap="round">
          <path d={`M58 92 C 20 ${100 + w}, -20 ${80 - w}, -80 ${100 + w} S -170 ${96 - w}, -200 ${112 + w}`} />
          <path d={`M60 100 C 30 ${116 - w}, -10 ${104 + w}, -60 ${122 - w} S -130 ${118 + w}, -160 ${132 - w}`} opacity={0.7} />
        </g>
        <g fill="#16110c" stroke={GOLD} strokeWidth={1.6}>
          <path d="M44 30 L86 120" stroke={GOLD} strokeWidth={4} />
          <path d={`M50 62 Q18 ${68 + w} -14 ${92 + w} Q16 ${94 + w} 46 84 Z`} />
          <path d="M52 46 Q44 74 40 112 Q36 160 24 204 Q62 214 100 200 Q88 156 80 112 Q74 74 68 46 Z" />
          <path d="M66 60 Q86 80 92 104 Q80 104 70 90 Z" />
          <circle cx={60} cy={32} r={13} />
          <circle cx={58} cy={15} r={6} />
          <path d={`M50 28 Q34 ${38 + w / 2} 22 ${56 + w}`} fill="none" strokeWidth={4} stroke="#16110c" />
        </g>
      </svg>
      <Vapour width={width * 1.05} height={width * 0.42} seed={17} t={t} density={1.6} scale={0.55} tint="#fff1d6"
        style={{ position: "absolute", left: -width * 0.02, top: width * 0.5 }} />
    </div>
  );
};

/** Pseudo-random 0..1 (deterministic: the same frame renders the same). */
const rand = (i: number, seed: number) => {
  const v = Math.sin(i * 127.1 + seed * 311.7) * 43758.5453;
  return v - Math.floor(v);
};

/**
 * A bank of clouds: layers of lit fractal vapour (Vapour), dense in the middle and fraying at the edges; vertical = a
 * band across the screen at y (for soaring up through it), else a wall at x (sweeping sideways).
 */
export const CloudBank: React.FC<{ x: number; y?: number; scale?: number; vertical?: boolean; seed?: number; t?: number }> =
  ({ x, y = 540, scale = 1, vertical = false, seed = 1, t = 0 }) => {
    const layers = vertical
      ? [{ w: 3000, h: 820, dx: 0, dy: 0, d: 1.5, s: seed }, { w: 2200, h: 520, dx: -500, dy: -160, d: 1.2, s: seed + 4 }, { w: 2000, h: 480, dx: 600, dy: 170, d: 1.25, s: seed + 9 }]
      : [{ w: 1300, h: 1900, dx: 0, dy: 0, d: 1.55, s: seed }, { w: 900, h: 1200, dx: -380, dy: -260, d: 1.2, s: seed + 4 }, { w: 1000, h: 1300, dx: 320, dy: 300, d: 1.3, s: seed + 9 }];
    return (
      <AbsoluteFill style={{ pointerEvents: "none" }}>
        {layers.map((l, i) => {
          const w = l.w * scale, h = l.h * (vertical ? scale : 1);
          const cx = (vertical ? 960 : x) + l.dx * scale, cy = (vertical ? y : 540) + l.dy;
          return <Vapour key={i} width={w} height={h} seed={l.s} t={t} density={l.d} scale={1.3} stretch={vertical ? 1.4 : 0.8}
            style={{ position: "absolute", left: cx - w / 2, top: cy - h / 2 }} />;
        })}
      </AbsoluteFill>
    );
  };

/**
 * Sparks along a moving edge (a line through (x, y) at an angle, degrees from vertical): gold motes born on the edge,
 * drifting away from it and fading; t in frames since the start, deterministic.
 */
export const EdgeSparks: React.FC<{ x: number; y?: number; angle?: number; t: number; count?: number; color?: string; spread?: number; dir?: number }> =
  ({ x, y = 540, angle = 0, t, count = 46, color = "#ffd98a", spread = 1200, dir = -1 }) => {
    const a = (angle * Math.PI) / 180;
    return (
      <AbsoluteFill style={{ pointerEvents: "none" }}>
        {Array.from({ length: count }, (_, i) => {
          const life = 14 + rand(i, 5) * 16;
          const age = (t + rand(i, 9) * life) % life;
          const along = (rand(i, 2) - 0.5) * spread;
          const drift = age * (2 + rand(i, 4) * 5);
          const px = x + Math.sin(a) * along + dir * drift * Math.cos(a) + Math.sin(age / 3 + i) * 4;
          const py = y + Math.cos(a) * along - age * (0.6 + rand(i, 6) * 1.4);
          const s = 2 + rand(i, 8) * 5;
          const o = Math.sin((age / life) * Math.PI);
          return <div key={i} style={{ position: "absolute", left: px, top: py, width: s, height: s, borderRadius: s, background: color,
            opacity: o, boxShadow: `0 0 ${s * 3}px ${s}px ${color}88` }} />;
        })}
      </AbsoluteFill>
    );
  };

/** A burst of embers from a point (talisman burning, a seal stamped...): p 0 -> 1 over the burst. */
export const Burst: React.FC<{ x: number; y: number; p: number; count?: number; color?: string; power?: number }> =
  ({ x, y, p, count = 60, color = "#ffb347", power = 700 }) => (
    <AbsoluteFill style={{ pointerEvents: "none" }}>
      {Array.from({ length: count }, (_, i) => {
        const ang = rand(i, 11) * Math.PI * 2;
        const v = (0.3 + rand(i, 12) * 0.7) * power;
        const e = 1 - Math.pow(1 - p, 2);
        const px = x + Math.cos(ang) * v * e;
        const py = y + Math.sin(ang) * v * e - p * p * 120;
        const s = 2 + rand(i, 13) * 6;
        return <div key={i} style={{ position: "absolute", left: px, top: py, width: s, height: s, borderRadius: s, background: color,
          opacity: (1 - p) * (0.6 + rand(i, 14) * 0.4), boxShadow: `0 0 ${s * 3}px ${s}px ${color}77` }} />;
      })}
    </AbsoluteFill>
  );

/** A paper talisman (符): yellow paper, red border and brush characters. */
export const Talisman: React.FC<{ height: number; style?: React.CSSProperties }> = ({ height, style }) => (
  <div style={{ width: height * 0.36, height, background: "linear-gradient(180deg, #f2d27a, #e3b955)", border: `${height * 0.012}px solid #a3201a`,
    boxShadow: "0 10px 40px rgba(0,0,0,0.6), inset 0 0 0 6px #f2d27a, inset 0 0 0 8px #a3201a", display: "flex", flexDirection: "column",
    alignItems: "center", justifyContent: "center", color: "#a3201a", fontFamily: "'Ma Shan Zheng', 'KaiTi', serif", fontSize: height * 0.2, lineHeight: 1.05, ...style }}>
    <span>敕</span><span>令</span><span>开</span>
  </div>
);

/** Peach blossom petals falling and drifting across (p 0 -> 1). */
export const Petals: React.FC<{ p: number; t: number; count?: number }> = ({ p, t, count = 70 }) => (
  <AbsoluteFill style={{ pointerEvents: "none" }}>
    {Array.from({ length: count }, (_, i) => {
      const start = rand(i, 21) * 0.5;
      const q = Math.max(0, Math.min(1, (p - start) / 0.5));
      if (q <= 0 || q >= 1) return null;
      const x0 = 1920 + rand(i, 22) * 900 - 200;
      const y0 = -200 + rand(i, 23) * 900;
      const px = x0 - q * (1500 + rand(i, 24) * 900);
      const py = y0 + q * (500 + rand(i, 25) * 500) + Math.sin(t / 7 + i) * 20;
      const s = 18 + rand(i, 26) * 26;
      return (
        <svg key={i} width={s} height={s} viewBox="-2 -12 28 24" style={{ position: "absolute", left: px, top: py,
          transform: `rotate(${t * (2 + rand(i, 27) * 4) + i * 40}deg)`, opacity: Math.sin(q * Math.PI) * 0.95, overflow: "visible" }}>
          <path d="M0 0 C 6 -11, 20 -10, 24 -2 C 21 -1, 21 1, 24 2 C 20 10, 6 11, 0 0 Z" fill={rand(i, 28) > 0.5 ? "#f6b8c8" : "#f9d3dc"} stroke="#e48aa2" strokeWidth={0.8} />
        </svg>
      );
    })}
  </AbsoluteFill>
);

/** The two leaves of a red-lacquered gate with gold studs and ring handles; open 0 (closed) -> 1 (open). */
export const Gates: React.FC<{ open: number }> = ({ open }) => {
  const leaf = (left: boolean) => (
    <div style={{ position: "absolute", top: 0, width: 960, height: 1080, left: left ? -960 * open : 960 + 960 * open,
      background: "linear-gradient(90deg, #4a120d, #7a2016 50%, #4a120d)", boxShadow: "0 0 60px rgba(0,0,0,0.8), inset 0 0 0 14px #3a0e0a, inset 0 0 0 18px #c8902e" }}>
      <div style={{ position: "absolute", inset: 60, backgroundImage: "radial-gradient(circle, #f2c766 0 9px, #8a5a18 10px, transparent 12px)", backgroundSize: "120px 120px", opacity: 0.9 }} />
      <div style={{ position: "absolute", top: 470, [left ? "right" : "left"]: 40, width: 120, height: 120, borderRadius: "50%",
        border: "10px solid #e8b54a", boxShadow: "0 0 20px rgba(232,181,74,0.6)" } as React.CSSProperties} />
    </div>
  );
  return <AbsoluteFill style={{ pointerEvents: "none", overflow: "hidden" }}>{leaf(true)}{leaf(false)}</AbsoluteFill>;
};

/** A sprig with two leaves hanging from the top of the screen, the tip of the lower leaf at (tipX, tipY); sway in px. */
export const LeafSprig: React.FC<{ tipX: number; tipY: number; sway?: number }> = ({ tipX, tipY, sway = 0 }) => (
  <svg width={520} height={tipY + 40} viewBox={`0 0 520 ${tipY + 40}`} style={{ position: "absolute", left: tipX - 430, top: 0, overflow: "visible",
    filter: "drop-shadow(0 6px 10px rgba(0,0,0,0.5))", transform: `rotate(${sway * 0.3}deg)`, transformOrigin: "60% 0%" }}>
    <defs>
      <linearGradient id="leafg" x1="0" y1="0" x2="1" y2="1">
        <stop offset="0%" stopColor="#4f7a3a" />
        <stop offset="100%" stopColor="#2c4a22" />
      </linearGradient>
    </defs>
    {/* the branch from the top */}
    <path d={`M300 -10 Q290 ${tipY * 0.35} 330 ${tipY * 0.55}`} stroke="#3b2a1a" strokeWidth={7} fill="none" strokeLinecap="round" />
    {/* an upper leaf */}
    <path d={`M318 ${tipY * 0.42} Q420 ${tipY * 0.3} 470 ${tipY * 0.38} Q410 ${tipY * 0.55} 318 ${tipY * 0.42} Z`} fill="url(#leafg)" />
    {/* the lower leaf, its tip where the drop forms */}
    <path d={`M330 ${tipY * 0.55} Q470 ${tipY * 0.6} 430 ${tipY} Q360 ${tipY * 0.82} 330 ${tipY * 0.55} Z`} fill="url(#leafg)" />
    <path d={`M330 ${tipY * 0.55} Q400 ${tipY * 0.72} 430 ${tipY}`} stroke="#8fb37a" strokeWidth={2} fill="none" opacity={0.7} />
  </svg>
);

/** A drop of water: light refracting, a bright highlight. */
export const Drop: React.FC<{ x: number; y: number; size: number; stretch?: number }> = ({ x, y, size, stretch = 1 }) => (
  <div style={{ position: "absolute", left: x - size / 2, top: y - size * stretch, width: size, height: size * stretch * 1.25,
    borderRadius: "50% 50% 50% 50% / 60% 60% 40% 40%",
    background: "radial-gradient(circle at 35% 35%, rgba(255,255,255,0.95) 0 12%, rgba(200,230,255,0.55) 30%, rgba(120,170,210,0.35) 70%, rgba(255,255,255,0.6) 100%)",
    boxShadow: "0 0 10px rgba(200,230,255,0.7)" }} />
);

/**
 * A cloud (or smoke, fog) from fractal noise, lit from above: the noise of SVG feTurbulence shaped by a soft ellipse,
 * shaded by a light (feDiffuseLighting) so the tops glow and the undersides darken, edges fraying like real vapour.
 * t (frames) makes it billow slowly; tint colours the light; density raises the cover; stretch > 1 makes it streaky
 * (smoke, a fog bank lying flat).
 */
export const Vapour: React.FC<{ width: number; height: number; seed?: number; t?: number; tint?: string; density?: number;
  scale?: number; stretch?: number; opacity?: number; light?: number; style?: React.CSSProperties }> =
  ({ width, height, seed = 1, t = 0, tint = "#fff6e6", density = 1, scale = 1, stretch = 1, opacity = 1, light = 1, style }) => {
    const id = useId().replace(/:/g, "");
    const fx = (0.0065 / scale) * (1 + 0.04 * Math.sin(t / 45 + seed));
    const fy = (fx * stretch);
    // alpha = 2.6 * noise - offset: more density, more cover
    const off = 0.5 - 0.1 * density;
    return (
      <svg width={width} height={height} style={{ overflow: "visible", opacity, ...style }}>
        <defs>
          <radialGradient id={`s${id}`} cx="50%" cy="50%" r="50%">
            <stop offset="0%" stopColor="#fff" stopOpacity={1} />
            <stop offset="45%" stopColor="#fff" stopOpacity={0.8} />
            <stop offset="100%" stopColor="#fff" stopOpacity={0} />
          </radialGradient>
          <filter id={`f${id}`} x="-10%" y="-10%" width="120%" height="120%" colorInterpolationFilters="sRGB">
            <feTurbulence type="fractalNoise" baseFrequency={`${fx} ${fy}`} numOctaves={5} seed={seed} result="noise" />
            <feColorMatrix in="noise" type="matrix" result="vap"
              values={`0 0 0 0 1  0 0 0 0 1  0 0 0 0 1  ${1.55 * density} 0 0 0 -${off}`} />
            <feComposite in="vap" in2="SourceGraphic" operator="arithmetic" k1={1.15} k2={0} k3={0} k4={0} result="shaped" />
            <feGaussianBlur in="shaped" stdDeviation={1.2} result="soft" />
            <feDiffuseLighting in="soft" lightingColor={tint} surfaceScale={7} diffuseConstant={1.15 * light} result="lit">
              <feDistantLight azimuth={250} elevation={52} />
            </feDiffuseLighting>
            <feComposite in="lit" in2="soft" operator="in" />
          </filter>
        </defs>
        <ellipse cx={width / 2} cy={height / 2} rx={width / 2} ry={height / 2} fill={`url(#s${id})`} filter={`url(#f${id})`} />
      </svg>
    );
  };

/**
 * Smoke rising from a point (x, y): plumes of grey vapour drifting up, widening and thinning out; p 0 -> 1.
 */
export const Smoke: React.FC<{ x: number; y: number; p: number; t: number; tint?: string; size?: number }> = ({ x, y, p, t, tint = "#9a9186", size = 1 }) => (
  <AbsoluteFill style={{ pointerEvents: "none" }}>
    {[0, 1, 2, 3].map((i) => {
      const q = Math.max(0, Math.min(1, p * 1.3 - i * 0.1));
      if (q <= 0) return null;
      const w = (300 + q * 700) * size, h = (420 + q * 600) * size;
      return <Vapour key={i} width={w} height={h} seed={11 + i * 7} t={t} tint={tint} density={1.1 - q * 0.6} stretch={2.2} scale={0.9}
        opacity={Math.sin(Math.min(1, q * 1.2) * Math.PI) * 0.85}
        style={{ position: "absolute", left: x - w / 2 + (i - 1.5) * 90 * size + Math.sin(t / 20 + i) * 30, top: y - h * 0.7 - q * 500 * size }} />;
    })}
  </AbsoluteFill>
);

/**
 * A tree drawn branch by branch (recursive, deterministic by seed) from its foot at (x, y), leaning by lean (radians,
 * 0 = straight up): tapering branches of rough bark (noise displacement, shaded), twigs, and leaves ("leaf") or
 * cherry blossom ("blossom": washes of pink in depth, a darker one behind, flowers on the twigs and along the branches);
 * reach (screen point): a long bough from the trunk to there, ending in a big leaf whose tip is that point (a drop
 * hangs from it); sway (degrees) bends it in the wind around its foot.
 */
export const Tree: React.FC<{ x: number; y: number; size?: number; lean?: number; seed?: number; kind?: "leaf" | "blossom"; sway?: number; depth?: number;
  reach?: { x: number; y: number } }> =
  ({ x, y, size = 1, lean = 0.3, seed = 3, kind = "leaf", sway = 0, depth = 6, reach }) => {
    const isBlossom = kind === "blossom";
    const id = useId().replace(/:/g, "");
    const limbs: string[] = [];
    const tips: { x: number; y: number; k: number }[] = [];
    const along: { x: number; y: number; k: number }[] = [];
    let k = 0;
    /** A tapering limb from (bx, by) (width w0) to (ex, ey) (width w1) bent through (mx, my). */
    const limb = (bx: number, by: number, mx: number, my: number, ex: number, ey: number, w0: number, w1: number) => {
      const dx = ex - bx, dy = ey - by, l = Math.hypot(dx, dy) || 1;
      const nx = -dy / l, ny = dx / l;
      const wm = (w0 + w1) / 2;
      limbs.push(`M${bx + nx * w0 / 2} ${by + ny * w0 / 2} Q${mx + nx * wm / 2} ${my + ny * wm / 2} ${ex + nx * w1 / 2} ${ey + ny * w1 / 2}` +
        ` L${ex - nx * w1 / 2} ${ey - ny * w1 / 2} Q${mx - nx * wm / 2} ${my - ny * wm / 2} ${bx - nx * w0 / 2} ${by - ny * w0 / 2} Z`);
    };
    let trunkTop = { x: 0, y: 0 };
    const grow = (bx: number, by: number, a: number, len: number, w: number, level: number) => {
      const ex = bx + Math.sin(a) * len, ey = by - Math.cos(a) * len;
      const bend = (rand(k++, seed) - 0.5) * len * 0.3;
      const mx = (bx + ex) / 2 + Math.cos(a) * bend, my = (by + ey) / 2 + Math.sin(a) * bend;
      limb(bx, by, mx, my, ex, ey, w, w * 0.7);
      if (level === depth) trunkTop = { x: ex, y: ey };
      if (level <= 2) along.push({ x: (bx + ex) / 2, y: (by + ey) / 2, k: k++ });
      if (level === 0) {
        tips.push({ x: ex, y: ey, k: k++ });
        // twigs
        for (let i = 0; i < 2; i++) {
          const ta = a + (i ? 0.7 : -0.7) + (rand(k++, seed) - 0.5) * 0.3, tl = len * 0.45;
          limb(ex, ey, ex + Math.sin(ta) * tl * 0.5, ey - Math.cos(ta) * tl * 0.5, ex + Math.sin(ta) * tl, ey - Math.cos(ta) * tl, w * 0.5, w * 0.2);
          tips.push({ x: ex + Math.sin(ta) * tl, y: ey - Math.cos(ta) * tl, k: k++ });
        }
        return;
      }
      const n = level > depth - 2 ? 2 : 2 + (rand(k++, seed) > 0.55 ? 1 : 0);
      for (let i = 0; i < n; i++) {
        const spread = (i - (n - 1) / 2) * (0.5 + rand(k++, seed) * 0.35);
        grow(ex, ey, a + spread + (rand(k++, seed) - 0.5) * 0.25, len * (0.68 + rand(k++, seed) * 0.14), w * 0.66, level - 1);
      }
    };
    grow(0, 0, lean, 260 * size, 40 * size, depth);
    // the bough to the reach point (in the tree's own coordinates), its big leaf hanging with the tip there
    let bigLeaf: React.ReactNode = null;
    if (reach) {
      const rx = reach.x - x, ry = reach.y - y;
      const sx = trunkTop.x, sy = trunkTop.y;
      const lx = rx - 70 * size, ly = ry - 120 * size; // where the leaf stalk starts
      limb(sx, sy, (sx + lx) / 2, Math.min(sy, ly) - 140 * size, lx, ly, 20 * size, 6 * size);
      for (let i = 1; i <= 2; i++) {
        const t = i / 4;
        const px = sx + (lx - sx) * t, py = sy + (ly - sy) * t - Math.sin(t * Math.PI) * 120 * size;
        tips.push({ x: px, y: py - 20 * size, k: 900 + i });
      }
      const L = Math.hypot(rx - lx, ry - ly), ang = (Math.atan2(ry - ly, rx - lx) * 180) / Math.PI;
      bigLeaf = (
        <g transform={`translate(${lx} ${ly}) rotate(${ang})`}>
          <path d={`M0 0 Q${L * 0.45} ${-L * 0.32} ${L} 0 Q${L * 0.45} ${L * 0.32} 0 0 Z`} fill={`url(#l${id})`} stroke="#2c4a22" strokeWidth={1} />
          <path d={`M0 0 L${L} 0`} stroke="#9cc283" strokeWidth={1.6} />
          {[0.25, 0.45, 0.65].map((t) => <path key={t} d={`M${L * t} 0 L${L * (t + 0.12)} ${-L * 0.14} M${L * t} 0 L${L * (t + 0.12)} ${L * 0.14}`} stroke="#8fb37a" strokeWidth={1} opacity={0.7} />)}
        </g>
      );
    }
    const flower = (px: number, py: number, s: number, a: number, key: string, light: boolean) => (
      <g key={key} transform={`translate(${px} ${py}) rotate(${a})`} opacity={0.94}>
        {[0, 72, 144, 216, 288].map((r) => (
          <path key={r} transform={`rotate(${r})`} d={`M0 0 C ${-s * 0.55} ${-s * 0.35}, ${-s * 0.5} ${-s * 1.05}, ${-s * 0.12} ${-s}
            L0 ${-s * 0.86} L${s * 0.12} ${-s} C ${s * 0.5} ${-s * 1.05}, ${s * 0.55} ${-s * 0.35}, 0 0 Z`}
            fill={light ? "#fde9ef" : "#f7c6d4"} stroke="#e7a3b8" strokeWidth={0.5} />
        ))}
        <circle r={s * 0.2} fill="#c9476e" />
        {[0, 60, 120, 180, 240, 300].map((r) => <circle key={r} cx={Math.cos((r * Math.PI) / 180) * s * 0.36} cy={Math.sin((r * Math.PI) / 180) * s * 0.36} r={s * 0.055} fill="#a8304f" />)}
      </g>
    );
    const leafAt = (px: number, py: number, s: number, a: number, key: string, dark: boolean) => (
      <path key={key} transform={`translate(${px} ${py}) rotate(${a})`} d={`M${-s} 0 Q0 ${-s * 0.5} ${s} 0 Q0 ${s * 0.5} ${-s} 0 Z`}
        fill={dark ? "#3a6a2c" : "#5d8f3f"} stroke="#2c4a22" strokeWidth={0.6} opacity={0.96} />
    );
    return (
      <svg style={{ position: "absolute", left: x, top: y, overflow: "visible", transform: `rotate(${sway}deg)`, transformOrigin: "0 0",
        filter: "drop-shadow(0 8px 14px rgba(0,0,0,0.45))" }} width={1} height={1}>
        <defs>
          {/* bark: rough edges and shading */}
          <filter id={`k${id}`} x="-20%" y="-20%" width="140%" height="140%">
            <feTurbulence type="fractalNoise" baseFrequency="0.09 0.025" numOctaves={3} seed={seed} result="n" />
            <feDisplacementMap in="SourceGraphic" in2="n" scale={5} xChannelSelector="R" yChannelSelector="G" result="d" />
            <feDiffuseLighting in="n" surfaceScale={2.5} lightingColor="#c9a882" result="light"><feDistantLight azimuth={225} elevation={45} /></feDiffuseLighting>
            <feComposite in="light" in2="d" operator="in" result="lit" />
            <feBlend in="lit" in2="d" mode="multiply" />
          </filter>
          <linearGradient id={`t${id}`} x1="0" y1="0" x2="1" y2="0">
            <stop offset="0%" stopColor="#1f160e" /><stop offset="50%" stopColor="#4a3523" /><stop offset="100%" stopColor="#24190f" />
          </linearGradient>
          {/* watercolour: soft washes with ragged edges */}
          <filter id={`w${id}`} x="-50%" y="-50%" width="200%" height="200%">
            <feTurbulence type="fractalNoise" baseFrequency="0.03" numOctaves={3} seed={seed} result="n" />
            <feDisplacementMap in="SourceGraphic" in2="n" scale={44} xChannelSelector="R" yChannelSelector="G" result="d" />
            <feGaussianBlur in="d" stdDeviation={4} />
          </filter>
          <radialGradient id={`p${id}`}>
            <stop offset="0%" stopColor="#f4a9bf" stopOpacity={0.72} /><stop offset="60%" stopColor="#f7c4d3" stopOpacity={0.42} /><stop offset="100%" stopColor="#fbe0e8" stopOpacity={0} />
          </radialGradient>
          <radialGradient id={`q${id}`}>
            <stop offset="0%" stopColor="#c86b8a" stopOpacity={0.5} /><stop offset="100%" stopColor="#c86b8a" stopOpacity={0} />
          </radialGradient>
          <radialGradient id={`v${id}`}>
            <stop offset="0%" stopColor="#4f7f38" stopOpacity={0.28} /><stop offset="100%" stopColor="#4f7f38" stopOpacity={0} />
          </radialGradient>
          <linearGradient id={`l${id}`} x1="0" y1="0" x2="1" y2="1"><stop offset="0%" stopColor="#5d8f3f" /><stop offset="100%" stopColor="#2c4a22" /></linearGradient>
        </defs>
        {/* the far foliage: a darker wash behind the branches */}
        <g filter={`url(#w${id})`}>
          {tips.map((t, i) => <circle key={i} cx={t.x + 20 * size} cy={t.y - 15 * size} r={80 * size * (0.7 + rand(t.k, seed + 9) * 0.6)} fill={isBlossom ? `url(#q${id})` : `url(#v${id})`} />)}
        </g>
        <g filter={`url(#k${id})`} fill={`url(#t${id})`}>{limbs.map((d, i) => <path key={i} d={d} />)}</g>
        {bigLeaf}
        {isBlossom && <g filter={`url(#w${id})`}>
          {tips.map((t, i) => <circle key={i} cx={t.x} cy={t.y} r={64 * size * (0.7 + rand(t.k, seed + 10) * 0.6)} fill={`url(#p${id})`} />)}
        </g>}
        {tips.map((t, i) => (
          <g key={i}>
            {Array.from({ length: isBlossom ? 5 : 8 }, (_, j) => {
              const r = (isBlossom ? 40 : 42) * size * (0.3 + rand(t.k + j, seed + 5) * 0.9);
              const ang = rand(t.k + j, seed + 6) * Math.PI * 2;
              const px = t.x + Math.cos(ang) * r, py = t.y + Math.sin(ang) * r * 0.8;
              const s = (isBlossom ? 12 : 15) * size * (0.7 + rand(t.k + j, seed + 7) * 0.6);
              return isBlossom ? flower(px, py, s, ang * 57, `${i}-${j}`, rand(t.k + j, seed + 8) > 0.45)
                : leafAt(px, py, s, ang * 57, `${i}-${j}`, rand(t.k + j, seed + 8) > 0.5);
            })}
          </g>
        ))}
        {isBlossom && along.map((a, i) => flower(a.x + (rand(a.k, seed + 11) - 0.5) * 20, a.y + (rand(a.k, seed + 12) - 0.5) * 20, 9 * size, rand(a.k, seed) * 360, `a${i}`, true))}
      </svg>
    );
  };

/** The noise filter shared by a burn hole and its charred rim (same seed and scale: they match). */
const burnFilter = (id: string) => (
  <filter id={id} x="-20%" y="-20%" width="140%" height="140%">
    <feTurbulence type="fractalNoise" baseFrequency="0.012" numOctaves={4} seed={7} result="n" />
    <feDisplacementMap in="SourceGraphic" in2="n" scale={220} xChannelSelector="R" yChannelSelector="G" />
  </filter>
);

/** Mask (CSS mask-image) of a picture burning away from (cx, cy): opaque except a ragged hole of radius r. */
export const burnMask = (cx: number, cy: number, r: number) => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="1920" height="1080"><defs><filter id="b" x="-20%" y="-20%" width="140%" height="140%">` +
    `<feTurbulence type="fractalNoise" baseFrequency="0.012" numOctaves="4" seed="7" result="n"/><feDisplacementMap in="SourceGraphic" in2="n" scale="220" xChannelSelector="R" yChannelSelector="G"/></filter>` +
    `<mask id="m"><rect width="1920" height="1080" fill="white"/><circle cx="${cx}" cy="${cy}" r="${Math.max(0, r)}" fill="black" filter="url(#b)"/></mask></defs>` +
    `<rect width="1920" height="1080" fill="white" mask="url(#m)"/></svg>`;
  const url = `url("data:image/svg+xml;utf8,${encodeURIComponent(svg)}")`;
  return { WebkitMaskImage: url, maskImage: url, WebkitMaskSize: "100% 100%", maskSize: "100% 100%" } as React.CSSProperties;
};

/** The charred, glowing rim of the burn hole (drawn over the burning picture, matching burnMask). */
export const BurnRim: React.FC<{ cx: number; cy: number; r: number }> = ({ cx, cy, r }) => {
  const id = useId().replace(/:/g, "");
  if (r <= 0) return null;
  return (
    <svg width={1920} height={1080} style={{ position: "absolute", left: 0, top: 0, overflow: "visible" }}>
      <defs>{burnFilter(`b${id}`)}<filter id={`g${id}`}><feGaussianBlur stdDeviation={6} /></filter></defs>
      <g filter={`url(#b${id})`}>
        {/* scorched paper around the hole, a glowing line of fire on its edge */}
        <circle cx={cx} cy={cy} r={r + 36} fill="none" stroke="rgba(60,30,10,0.75)" strokeWidth={60} />
        <circle cx={cx} cy={cy} r={r + 10} fill="none" stroke="rgba(25,10,4,0.95)" strokeWidth={22} />
        <circle cx={cx} cy={cy} r={r + 2} fill="none" stroke="#ff9a2e" strokeWidth={6} filter={`url(#g${id})`} />
        <circle cx={cx} cy={cy} r={r + 2} fill="none" stroke="#ffd27a" strokeWidth={2} />
      </g>
    </svg>
  );
};
