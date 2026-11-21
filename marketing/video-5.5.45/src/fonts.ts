import { continueRender, delayRender, staticFile } from "remotion";

/** Brush characters (Ma Shan Zheng, simplified Chinese only) and the titles (Cinzel), both from Google Fonts (OFL),
 *  downloaded to public/fonts by scripts/prepare-assets.mjs. */
export const BRUSH = "'Ma Shan Zheng', 'KaiTi', 'SimSun', serif";
export const DISPLAY = "'Cinzel', Georgia, serif";
export const SERIF = "Georgia, 'Times New Roman', serif";
export const SANS = "'Segoe UI', 'Helvetica Neue', Arial, sans-serif";

const handle = delayRender("Loading the fonts");
Promise.all([
  new FontFace("Ma Shan Zheng", `url(${staticFile("fonts/MaShanZheng-Regular.ttf")})`).load(),
  new FontFace("Cinzel", `url(${staticFile("fonts/Cinzel.ttf")})`, { weight: "400 900" }).load(),
])
  .then((faces) => faces.forEach((f) => document.fonts.add(f)))
  .catch((e) => console.error("Fonts not loaded (run npm run prepare-assets)", e))
  .finally(() => continueRender(handle));
