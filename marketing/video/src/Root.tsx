import { Composition } from "remotion";
import { Promo, PromoProps } from "./Promo";
import { FPS, durationOf, partsFor } from "./data";

export const Root: React.FC = () => {
  const long = partsFor(false);
  const short = partsFor(true);
  return (
    <>
      {/* every promo picture by chapter (Steam Workshop / Ko-fi page) */}
      <Composition
        id="TaoistPromo"
        component={Promo}
        durationInFrames={durationOf(long)}
        fps={FPS}
        width={1920}
        height={1080}
        defaultProps={{ parts: long, music: "music-long.wav" } satisfies PromoProps}
      />
      {/* ~30 s cut for social posts */}
      <Composition
        id="TaoistPromoShort"
        component={Promo}
        durationInFrames={durationOf(short)}
        fps={FPS}
        width={1920}
        height={1080}
        defaultProps={{ parts: short, music: "music-short.wav" } satisfies PromoProps}
      />
    </>
  );
};
