"""
Music of a marketing video: pieces of the song (public/song.wav, decoded from the mp3 by prepare-assets) put end to
end, every cut on a beat, so that the drops land on the chapter cards of the video.

    python scripts/cut-music.py <song.wav> <timeline.json> <out.wav>

timeline.json (written by scripts/prepare-assets.mjs): {"bpm": 112, "firstBeat": 1.367, "pieces": [[0, 16], [44, 32],
...]} - each piece is [first beat in the song, number of beats]; beat 0 of the song is at firstBeat seconds (a downbeat
of a phrase). The pieces join with a short crossfade just before the beat, so the attack of the beat stays whole.
"""
import json
import sys
import wave

import numpy as np

XFADE = 0.012     # seconds of crossfade at a cut
FADE_IN = 0.02
FADE_OUT = 1.5


def read_wav(path):
    with wave.open(path, "rb") as w:
        sr, ch, width = w.getframerate(), w.getnchannels(), w.getsampwidth()
        if width != 2:
            raise SystemExit(f"{path}: 16-bit PCM expected")
        data = np.frombuffer(w.readframes(w.getnframes()), "<i2").astype(np.float32) / 32768
    return sr, data.reshape(-1, ch)


def write_wav(path, sr, buf):
    data = (np.clip(buf, -1, 1) * 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(buf.shape[1])
        w.setsampwidth(2)
        w.setframerate(sr)
        w.writeframes(data.tobytes())


def render(sr, song, timeline):
    beat = 60.0 / timeline["bpm"]
    first = timeline["firstBeat"]
    total = sum(b for _, b in timeline["pieces"])
    n_out = int(round(total * beat * sr))
    out = np.zeros((n_out + sr, song.shape[1]), np.float32)
    xf = int(XFADE * sr)
    ramp = np.sin(np.linspace(0, np.pi / 2, xf))[:, None] ** 2
    at_beat = 0
    for i, (src, beats) in enumerate(timeline["pieces"]):
        dst = int(round(at_beat * beat * sr))
        a = int(round((first + src * beat) * sr))
        n = int(round(beats * beat * sr))
        # the piece starts xf samples early (fading in over the end of the previous one) and runs xf samples late
        lead = xf if i > 0 else 0
        piece = np.zeros((n + lead + xf, song.shape[1]), np.float32)
        s0 = a - lead
        lo, hi = max(s0, 0), min(s0 + len(piece), len(song))
        if hi > lo:
            piece[lo - s0:hi - s0] = song[lo:hi]
        if lead:
            piece[:xf] *= ramp
        piece[-xf:] *= ramp[::-1]
        out[dst - lead:dst - lead + len(piece)] += piece
        at_beat += beats
    out = out[:n_out]
    fi = int(FADE_IN * sr)
    out[:fi] *= np.linspace(0, 1, fi)[:, None]
    fo = min(int(FADE_OUT * sr), n_out)
    out[n_out - fo:] *= np.linspace(1, 0, fo)[:, None]
    return out


if __name__ == "__main__":
    sr, song = read_wav(sys.argv[1])
    timeline = json.load(open(sys.argv[2], encoding="utf-8"))
    write_wav(sys.argv[3], sr, render(sr, song, timeline))
    beats = sum(b for _, b in timeline["pieces"])
    cuts = " + ".join(f"{s}-{s + b}" for s, b in timeline["pieces"])
    print(f"music: {beats} beats at {timeline['bpm']} BPM ({beats * 60 / timeline['bpm']:.1f} s), song beats {cuts} -> {sys.argv[3]}")
