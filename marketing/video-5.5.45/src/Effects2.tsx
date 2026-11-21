import { useId } from "react";
import { AbsoluteFill } from "remotion";
import { Vapour } from "./Effects";

/** Pseudo-random 0..1 (deterministic: the same frame renders the same). */
const rand = (i: number, seed: number) => {
  const v = Math.sin(i * 127.1 + seed * 311.7) * 43758.5453;
  return v - Math.floor(v);
};
const clamp01 = (v: number) => Math.max(0, Math.min(1, v));
/** 0 -> 1 between a and b. */
export const span = (x: number, a: number, b: number) => clamp01((x - a) / (b - a));
/** Fade in over [a, b], out over [c, d]. */
export const inOut = (x: number, a: number, b: number, c: number, d: number) => Math.min(span(x, a, b), 1 - span(x, c, d));

/**
 * Mask (CSS mask-image) showing only the inside of a ragged circle (fractal noise edge) of radius r at (cx, cy):
 * ink spreading, a crack opening, a portal.
 */
export const blobMask = (cx: number, cy: number, r: number, seed = 3, rough = 160, freq = 0.01) => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="1920" height="1080"><defs><filter id="b" x="-30%" y="-30%" width="160%" height="160%">` +
    `<feTurbulence type="fractalNoise" baseFrequency="${freq}" numOctaves="4" seed="${seed}" result="n"/>` +
    `<feDisplacementMap in="SourceGraphic" in2="n" scale="${rough}" xChannelSelector="R" yChannelSelector="G"/><feGaussianBlur stdDeviation="4"/></filter></defs>` +
    `<circle cx="${cx}" cy="${cy}" r="${Math.max(0, r)}" fill="white" filter="url(#b)"/></svg>`;
  const url = `url("data:image/svg+xml;utf8,${encodeURIComponent(svg)}")`;
  return { WebkitMaskImage: url, maskImage: url, WebkitMaskSize: "100% 100%", maskSize: "100% 100%" } as React.CSSProperties;
};

/** A ring of colour on the ragged edge of a blobMask (same seed / rough / freq): ink, light. */
export const BlobRim: React.FC<{ cx: number; cy: number; r: number; color: string; width: number; seed?: number; rough?: number; freq?: number; blur?: number; opacity?: number }> =
  ({ cx, cy, r, color, width, seed = 3, rough = 160, freq = 0.01, blur = 8, opacity = 1 }) => {
    const id = useId().replace(/:/g, "");
    if (r <= 0) return null;
    return (
      <svg width={1920} height={1080} style={{ position: "absolute", left: 0, top: 0, overflow: "visible", opacity }}>
        <defs>
          <filter id={`r${id}`} x="-30%" y="-30%" width="160%" height="160%">
            <feTurbulence type="fractalNoise" baseFrequency={freq} numOctaves={4} seed={seed} result="n" />
            <feDisplacementMap in="SourceGraphic" in2="n" scale={rough} xChannelSelector="R" yChannelSelector="G" />
            <feGaussianBlur stdDeviation={blur} />
          </filter>
        </defs>
        <circle cx={cx} cy={cy} r={r} fill="none" stroke={color} strokeWidth={width} filter={`url(#r${id})`} />
      </svg>
    );
  };

/** A jagged lightning bolt from (x0, y0) to (x1, y1) with side branches; glowing violet-white. */
export const Bolt: React.FC<{ x0: number; y0: number; x1: number; y1: number; seed?: number; opacity?: number }> = ({ x0, y0, x1, y1, seed = 1, opacity = 1 }) => {
  const id = useId().replace(/:/g, "");
  const path = (ax: number, ay: number, bx: number, by: number, n: number, jit: number, sd: number) => {
    const pts: string[] = [];
    for (let i = 0; i <= n; i++) {
      const t = i / n;
      const j = i === 0 || i === n ? 0 : (rand(i, sd) - 0.5) * jit;
      pts.push(`${ax + (bx - ax) * t + j},${ay + (by - ay) * t + j * 0.3}`);
    }
    return pts.join(" ");
  };
  const main = path(x0, y0, x1, y1, 14, 120, seed);
  const branches = [0.3, 0.5, 0.68].map((t, k) => {
    const bx = x0 + (x1 - x0) * t, by = y0 + (y1 - y0) * t;
    const dir = rand(k, seed + 4) > 0.5 ? 1 : -1;
    return path(bx, by, bx + dir * (140 + rand(k, seed + 5) * 160), by + 120 + rand(k, seed + 6) * 120, 6, 50, seed + 10 + k);
  });
  return (
    <svg width={1920} height={1080} style={{ position: "absolute", left: 0, top: 0, overflow: "visible", opacity }}>
      <defs><filter id={`g${id}`} x="-20%" y="-20%" width="140%" height="140%"><feGaussianBlur stdDeviation={10} /></filter></defs>
      <g filter={`url(#g${id})`} stroke="#b48cff" fill="none" strokeLinejoin="round" opacity={0.9}>
        <polyline points={main} strokeWidth={26} />
        {branches.map((b, i) => <polyline key={i} points={b} strokeWidth={12} />)}
      </g>
      <g stroke="#ffffff" fill="none" strokeLinejoin="round">
        <polyline points={main} strokeWidth={5} />
        {branches.map((b, i) => <polyline key={i} points={b} strokeWidth={2.5} />)}
      </g>
    </svg>
  );
};

/** Dark storm clouds over the top of the screen, the sky darkened (o: 0 -> 1). */
export const StormSky: React.FC<{ o: number; t: number }> = ({ o, t }) => (
  <AbsoluteFill style={{ opacity: o, pointerEvents: "none" }}>
    <AbsoluteFill style={{ background: "linear-gradient(180deg, rgba(14,10,28,0.85) 0%, rgba(20,14,34,0.55) 45%, rgba(20,14,34,0.15) 100%)" }} />
    {[0, 1, 2].map((i) => (
      <Vapour key={i} width={1500} height={520} seed={30 + i * 5} t={t} density={1.5} scale={1.1} tint="#8a7fa8" light={0.6}
        style={{ position: "absolute", left: -300 + i * 650 + Math.sin(t / 30 + i) * 40, top: -200 + (i % 2) * 60 }} />
    ))}
  </AbsoluteFill>
);

/** A formation (阵法): gold rings turning opposite ways, the eight trigrams, runes, a hexagram; size = diameter. */
export const Formation: React.FC<{ size: number; t: number; glow?: number }> = ({ size, t, glow = 1 }) => {
  const r = size / 2;
  const trigrams = ["☰", "☱", "☲", "☳", "☴", "☵", "☶", "☷"];
  const runes = "乾兑离震巽坎艮坤".split("");
  return (
    <svg width={size} height={size} viewBox={`${-r} ${-r} ${size} ${size}`} style={{ overflow: "visible",
      filter: `drop-shadow(0 0 ${18 * glow}px rgba(255,210,120,0.9)) drop-shadow(0 0 ${40 * glow}px rgba(232,181,74,0.6))` }}>
      <g fill="none" stroke="#f2c766">
        <circle r={r * 0.98} strokeWidth={r * 0.012} />
        <circle r={r * 0.9} strokeWidth={r * 0.006} />
        <circle r={r * 0.62} strokeWidth={r * 0.01} />
        <circle r={r * 0.55} strokeWidth={r * 0.005} strokeDasharray={`${r * 0.03} ${r * 0.02}`} transform={`rotate(${-t * 1.5})`} />
        <circle r={r * 0.3} strokeWidth={r * 0.008} />
      </g>
      <g transform={`rotate(${t * 0.8})`} fill="#ffe3a0" fontSize={r * 0.11} textAnchor="middle" dominantBaseline="central" style={{ fontFamily: "'Segoe UI Symbol', 'Segoe UI', sans-serif" }}>
        {trigrams.map((g, i) => {
          const a = (i / 8) * Math.PI * 2;
          return <text key={i} x={Math.sin(a) * r * 0.76} y={-Math.cos(a) * r * 0.76} transform={`rotate(${(a * 180) / Math.PI} ${Math.sin(a) * r * 0.76} ${-Math.cos(a) * r * 0.76})`}>{g}</text>;
        })}
      </g>
      <g transform={`rotate(${-t * 1.2})`} fill="#f2c766" fontSize={r * 0.08} textAnchor="middle" dominantBaseline="central" style={{ fontFamily: "'Ma Shan Zheng', 'KaiTi', serif" }}>
        {runes.map((c, i) => {
          const a = (i / 8 + 1 / 16) * Math.PI * 2;
          return <text key={i} x={Math.sin(a) * r * 0.45} y={-Math.cos(a) * r * 0.45}>{c}</text>;
        })}
      </g>
      <g fill="none" stroke="#f2c766" strokeWidth={r * 0.007} transform={`rotate(${t * 2})`}>
        <polygon points={[0, 1, 2].map((i) => `${Math.sin((i / 3) * Math.PI * 2) * r * 0.3},${-Math.cos((i / 3) * Math.PI * 2) * r * 0.3}`).join(" ")} />
        <polygon points={[0, 1, 2].map((i) => `${Math.sin((i / 3 + 1 / 6) * Math.PI * 2) * r * 0.3},${-Math.cos((i / 3 + 1 / 6) * Math.PI * 2) * r * 0.3}`).join(" ")} />
      </g>
    </svg>
  );
};

/** The taiji (yin-yang) of diameter size, turned by angle, with a gold rim. */
export const Taiji: React.FC<{ size: number; angle: number; glow?: number }> = ({ size, angle, glow = 1 }) => (
  <svg width={size} height={size} viewBox="-1.02 -1.02 2.04 2.04" style={{ overflow: "visible", transform: `rotate(${angle}deg)`,
    filter: `drop-shadow(0 0 ${30 * glow}px rgba(232,181,74,0.7))` }}>
    <circle r={1} fill="#f3ead6" />
    <path d="M0,-1 A1,1 0 0,1 0,1 A0.5,0.5 0 0,1 0,0 A0.5,0.5 0 0,0 0,-1 Z" fill="#14100c" />
    <circle cy={-0.5} r={0.13} fill="#14100c" />
    <circle cy={0.5} r={0.13} fill="#f3ead6" />
    <circle r={1} fill="none" stroke="#e8b54a" strokeWidth={0.02} />
  </svg>
);

/** A red-crowned crane flying right, wings flapping (t: frames, phase). */
export const Crane: React.FC<{ size: number; t: number; phase?: number }> = ({ size, t, phase = 0 }) => {
  const f = Math.sin(t / 3.4 + phase);
  // wing tips: up to 70 above the back on the up-stroke, 40 below on the down-stroke
  const ty = -15 - 55 * f;
  const wing = (dx: number, dy: number, near: boolean) => {
    const tipX = -34 + dx, tipY = ty + dy;
    return (
      <g>
        <path d={`M${4 + dx} ${-4 + dy} Q${-6 + dx} ${(tipY - 4) * 0.5} ${tipX} ${tipY} Q${-8 + dx} ${tipY * 0.35 + 2} ${-20 + dx} ${-2 + dy} Z`}
          fill={near ? "#ffffff" : "#e6e1d6"} stroke="#cfc8ba" strokeWidth={0.6} />
        {/* the black flight feathers at the end of the wing */}
        <path d={`M${tipX + 10} ${tipY * 0.82 + dy * 0.2} Q${tipX + 2} ${tipY * 0.95} ${tipX} ${tipY} Q${tipX - 2} ${tipY * 0.8} ${tipX + 4} ${tipY * 0.62}`}
          fill="#1b1b1b" />
      </g>
    );
  };
  return (
    <svg width={size} height={size * 0.8} viewBox="-75 -80 150 120" style={{ overflow: "visible", filter: "drop-shadow(0 4px 6px rgba(0,0,0,0.35))" }}>
      {wing(6, -2, false)}
      <path d="M-22 4 L-62 10 M-22 6 L-61 13" stroke="#2a2a2a" strokeWidth={1.8} />
      <ellipse cx={0} cy={0} rx={26} ry={9} fill="#fbf8f2" />
      <path d="M-26 -1 Q-36 2 -44 7 Q-32 7 -22 3 Z" fill="#1b1b1b" />
      <path d="M20 -3 Q36 -8 52 -12" stroke="#fbf8f2" strokeWidth={5} fill="none" strokeLinecap="round" />
      <path d="M38 -9 Q45 -11 52 -12" stroke="#1b1b1b" strokeWidth={5} fill="none" strokeLinecap="round" />
      <circle cx={54} cy={-12} r={4.2} fill="#fbf8f2" />
      <circle cx={55} cy={-15.5} r={2.1} fill="#d42a2a" />
      <path d="M57 -12 L71 -10" stroke="#c9b27a" strokeWidth={2.2} />
      {wing(-4, 2, true)}
    </svg>
  );
};

/** A sky lantern (孔明灯), glowing warm. */
export const Lantern: React.FC<{ size: number; flicker?: number }> = ({ size, flicker = 1 }) => (
  <div style={{ width: size, height: size * 1.35, borderRadius: "42% 42% 30% 30% / 50% 50% 22% 22%",
    background: `radial-gradient(ellipse at 50% 75%, rgba(255,240,180,${0.95 * flicker}) 0%, rgba(255,170,60,0.95) 45%, rgba(190,70,20,0.95) 100%)`,
    boxShadow: `0 0 ${size * 0.8}px ${size * 0.25}px rgba(255,160,60,${0.45 * flicker})`, position: "relative" }}>
    <div style={{ position: "absolute", left: "18%", right: "18%", bottom: -size * 0.06, height: size * 0.08, borderRadius: 4, background: "#5a2a10" }} />
  </div>
);

/** A bronze alchemy cauldron (丹炉) of width w: three legs, two ears, a lid lifted by lid (px), fire glowing below. */
export const Furnace: React.FC<{ w: number; lid: number; fire: number }> = ({ w, lid, fire }) => {
  const id = useId().replace(/:/g, "");
  return (
    <svg width={w} height={w * 1.05} viewBox="-100 -110 200 210" style={{ overflow: "visible", filter: "drop-shadow(0 10px 20px rgba(0,0,0,0.6))" }}>
      <defs>
        <linearGradient id={`z${id}`} x1="0" y1="0" x2="1" y2="0">
          <stop offset="0%" stopColor="#3a2812" /><stop offset="35%" stopColor="#a27a3a" /><stop offset="55%" stopColor="#c99a50" /><stop offset="100%" stopColor="#3a2812" />
        </linearGradient>
        <radialGradient id={`f${id}`}><stop offset="0%" stopColor="#fff2b0" /><stop offset="45%" stopColor="#ff9a2e" /><stop offset="100%" stopColor="rgba(255,90,20,0)" /></radialGradient>
      </defs>
      <ellipse cx={0} cy={86} rx={90 * fire} ry={30 * fire} fill={`url(#f${id})`} opacity={fire} />
      {/* legs */}
      <path d="M-50 40 L-62 96 L-48 96 L-36 46 Z M50 40 L62 96 L48 96 L36 46 Z M-6 50 L-6 98 L6 98 L6 50 Z" fill={`url(#z${id})`} />
      {/* body */}
      <path d="M-80 -30 Q-86 50 0 62 Q86 50 80 -30 Z" fill={`url(#z${id})`} stroke="#2a1c0c" strokeWidth={2} />
      <path d="M-70 -6 Q0 8 70 -6" stroke="#e8b54a" strokeWidth={2} fill="none" opacity={0.7} />
      <text x={0} y={30} textAnchor="middle" fontSize={34} fill="#e8b54a" opacity={0.85} style={{ fontFamily: "'Ma Shan Zheng', 'KaiTi', serif" }}>丹</text>
      {/* ears */}
      <path d="M-74 -30 L-92 -66 L-78 -66 L-62 -30 Z M74 -30 L92 -66 L78 -66 L62 -30 Z" fill={`url(#z${id})`} />
      {/* rim, lid */}
      <rect x={-86} y={-38} width={172} height={12} rx={5} fill="#5a4220" />
      <g transform={`translate(0 ${-lid})`}>
        <path d="M-70 -40 Q0 -86 70 -40 Z" fill={`url(#z${id})`} stroke="#2a1c0c" strokeWidth={2} />
        <circle cx={0} cy={-74} r={9} fill="#c99a50" />
      </g>
    </svg>
  );
};

/** A huge palm of golden light (the Buddha's palm), fingers up. */
export const Palm: React.FC<{ size: number }> = ({ size }) => (
  <svg width={size} height={size * 1.25} viewBox="-60 -90 120 150" style={{ overflow: "visible",
    filter: "drop-shadow(0 0 25px rgba(255,210,120,0.95)) drop-shadow(0 0 60px rgba(232,181,74,0.7))" }}>
    <defs>
      <linearGradient id="palmg" x1="0" y1="0" x2="0" y2="1">
        <stop offset="0%" stopColor="#fff6d6" /><stop offset="60%" stopColor="#f2c766" /><stop offset="100%" stopColor="#c8902e" />
      </linearGradient>
    </defs>
    <g fill="url(#palmg)" opacity={0.92}>
      <rect x={-34} y={-20} width={68} height={70} rx={24} />
      <rect x={-33} y={-70} width={15} height={60} rx={7.5} />
      <rect x={-15} y={-84} width={15} height={74} rx={7.5} />
      <rect x={3} y={-80} width={15} height={70} rx={7.5} />
      <rect x={20} y={-64} width={14} height={56} rx={7} />
      <rect x={-56} y={-6} width={14} height={46} rx={7} transform="rotate(-38 -49 17)" />
    </g>
    <g fill="none" stroke="#fff6d6" strokeWidth={1.2} opacity={0.6}>
      <path d="M-24 10 Q0 22 26 6" /><path d="M-26 24 Q-2 30 24 22" />
    </g>
  </svg>
);

/** A chain of gold links from (x0, y0) to (x1, y1), drawn in up to p (0 -> 1). */
export const Chain: React.FC<{ x0: number; y0: number; x1: number; y1: number; p: number }> = ({ x0, y0, x1, y1, p }) => {
  const len = Math.hypot(x1 - x0, y1 - y0);
  const n = Math.floor(len / 34);
  const a = (Math.atan2(y1 - y0, x1 - x0) * 180) / Math.PI;
  const shown = Math.floor(n * p);
  return (
    <svg width={1920} height={1080} style={{ position: "absolute", left: 0, top: 0, overflow: "visible", filter: "drop-shadow(0 0 8px rgba(232,181,74,0.8)) drop-shadow(0 4px 6px rgba(0,0,0,0.6))" }}>
      {Array.from({ length: shown }, (_, i) => {
        const t = (i + 0.5) / n;
        const cx = x0 + (x1 - x0) * t, cy = y0 + (y1 - y0) * t;
        return i % 2 === 0
          ? <ellipse key={i} cx={cx} cy={cy} rx={22} ry={11} transform={`rotate(${a} ${cx} ${cy})`} fill="none" stroke="#f2c766" strokeWidth={6} />
          : <rect key={i} x={cx - 22} y={cy - 3} width={44} height={6} rx={3} transform={`rotate(${a} ${cx} ${cy})`} fill="#c8902e" />;
      })}
    </svg>
  );
};

/** Cracks from (cx, cy), jagged, glowing red-gold, grown to p. */
export const Cracks: React.FC<{ cx: number; cy: number; p: number; seed?: number }> = ({ cx, cy, p, seed = 2 }) => (
  <svg width={1920} height={1080} style={{ position: "absolute", left: 0, top: 0, overflow: "visible", filter: "drop-shadow(0 0 10px rgba(255,120,60,0.9))" }}>
    {Array.from({ length: 9 }, (_, k) => {
      const a = (k / 9) * Math.PI * 2 + rand(k, seed) * 0.5;
      const L = (500 + rand(k, seed + 1) * 600) * p;
      const pts = [`${cx},${cy}`];
      for (let i = 1; i <= 8; i++) {
        const d = (L * i) / 8;
        const j = (rand(i + k * 10, seed + 2) - 0.5) * 60;
        pts.push(`${cx + Math.cos(a) * d - Math.sin(a) * j},${cy + Math.sin(a) * d + Math.cos(a) * j}`);
      }
      return <polyline key={k} points={pts.join(" ")} stroke="#ffd0a0" strokeWidth={3.5} fill="none" strokeLinejoin="round" />;
    })}
  </svg>
);

/** Snow falling (flakes drifting down); t frames, o opacity. */
export const Snow: React.FC<{ t: number; o: number; count?: number }> = ({ t, o, count = 90 }) => (
  <AbsoluteFill style={{ pointerEvents: "none", opacity: o }}>
    {Array.from({ length: count }, (_, i) => {
      const sp = 1.5 + rand(i, 41) * 3;
      const y = ((rand(i, 42) * 1200 + t * sp) % 1200) - 60;
      const x = rand(i, 43) * 1920 + Math.sin(t / 18 + i) * 30;
      const s = 3 + rand(i, 44) * 6;
      return <div key={i} style={{ position: "absolute", left: x, top: y, width: s, height: s, borderRadius: s, background: "#ffffff",
        opacity: 0.6 + rand(i, 45) * 0.4, boxShadow: `0 0 ${s * 2}px rgba(255,255,255,0.8)` }} />;
    })}
  </AbsoluteFill>
);

/** Frost spreading over the picture from its corners (p 0 -> 1): pale icy vapour and a blue tint. */
export const Frost: React.FC<{ p: number; t: number }> = ({ p, t }) => {
  const r = p * 1400;
  const corners = [[0, 0], [1920, 0], [0, 1080], [1920, 1080]];
  const mask = corners.map(([x, y]) => `radial-gradient(circle at ${x}px ${y}px, #000 ${r * 0.6}px, transparent ${r}px)`).join(", ");
  return (
    <AbsoluteFill style={{ pointerEvents: "none", WebkitMaskImage: mask, maskImage: mask }}>
      <AbsoluteFill style={{ background: "rgba(170,215,240,0.35)", mixBlendMode: "screen" }} />
      <Vapour width={2200} height={1300} seed={51} t={t} density={1.3} scale={0.35} tint="#e6f6ff" light={1.2} opacity={0.75}
        style={{ position: "absolute", left: -140, top: -110 }} />
    </AbsoluteFill>
  );
};

/** Triangles covering the screen (a jittered grid, each cell cut in two): the shards of a shattered picture. */
export const SHARDS: { pts: [number, number][]; cx: number; cy: number }[] = (() => {
  const cols = 3, rows = 2;
  const P = (i: number, j: number): [number, number] => {
    const edge = i === 0 || j === 0 || i === cols || j === rows;
    return [(i / cols) * 1920 + (edge ? 0 : (rand(i * 7 + j, 61) - 0.5) * 300), (j / rows) * 1080 + (edge ? 0 : (rand(i * 7 + j, 62) - 0.5) * 220)];
  };
  const out: { pts: [number, number][]; cx: number; cy: number }[] = [];
  for (let i = 0; i < cols; i++) for (let j = 0; j < rows; j++) {
    const a = P(i, j), b = P(i + 1, j), c = P(i + 1, j + 1), d = P(i, j + 1);
    for (const tri of (i + j) % 2 ? [[a, b, c], [a, c, d]] : [[a, b, d], [b, c, d]]) {
      const pts = tri as [number, number][];
      out.push({ pts: pts.map(([x, y]) => [x + (x - 960) * 0.01, y + (y - 540) * 0.01] as [number, number]), cx: pts.reduce((s, q) => s + q[0], 0) / 3, cy: pts.reduce((s, q) => s + q[1], 0) / 3 });
    }
  }
  return out;
})();
