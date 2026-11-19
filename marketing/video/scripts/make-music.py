"""
Background music of the marketing video, synthesized (no sample, no licence issue): an epic xianxia track in
D minor pentatonic at the BPM of the timeline - taiko drums and rolls, a low string ostinato, an erhu / dizi-like lead
with vibrato and slides, guzheng glissandi into the drops, brass swells, gongs on the chapters - cut to the timeline
of a video so that its hits land on the cuts.

    python scripts/make-music.py <timeline.json> <out.wav>

timeline.json (written by scripts/prepare-assets.mjs): {"bpm": 120, "segments": [["intro", 8], ["chapter", 4],
["slides", 16], ..., ["outro", 12]]} - lengths in beats. Deterministic (fixed seed).
"""
import json
import sys
import wave

import numpy as np

SR = 44100
rng = np.random.default_rng(11)


def midi(n):
    return 440.0 * 2 ** ((n - 69) / 12)


PENTA = [62, 65, 67, 69, 72, 74, 77, 79]          # D F G A C D F G (two octaves of the lead)
CHORDS = [[38, 50, 57, 62, 65], [34, 46, 53, 58, 62], [36, 48, 55, 60, 64], [33, 45, 52, 57, 60]]   # Dm Bb C Am
# lead phrases (indexes in PENTA, None = rest), 8 eighths a bar; a new phrase every 2 bars
PHRASES = [
    [3, None, 4, 3, 2, None, 0, None, 1, 2, 3, None, 3, None, None, None],
    [5, None, 4, 3, 4, None, 5, 6, 7, None, 6, 5, 4, None, None, None],
    [3, 4, 5, None, 4, 3, 2, None, 3, None, 1, 0, 0, None, None, None],
    [7, None, 6, 5, 6, None, 4, None, 5, 4, 3, None, 3, None, None, None],
]


def env_exp(n, decay):
    return np.exp(-np.arange(n) / SR / decay)


def lowpass(x, width):
    if width <= 1:
        return x
    return np.convolve(x, np.ones(width) / width, mode="same")


def taiko(big=True):
    n = int(SR * (1.6 if big else 0.6))
    t = np.arange(n) / SR
    f0 = 52 if big else 95
    freq = f0 + (60 if big else 80) * np.exp(-t / 0.07)
    body = np.sin(2 * np.pi * np.cumsum(freq) / SR) * env_exp(n, 0.45 if big else 0.16)
    skin = lowpass(rng.standard_normal(n), 5) * env_exp(n, 0.04) * 0.7
    return (body + skin) * (1.2 if big else 0.7)


def rim():
    n = int(SR * 0.08)
    noise = rng.standard_normal(n)
    return (noise - lowpass(noise, 3)) * env_exp(n, 0.012) * 0.35


def gong(length=4.0):
    n = int(SR * length)
    t = np.arange(n) / SR
    out = np.zeros(n)
    for f, a in [(98, 1.0), (147.5, 0.7), (196.3, 0.5), (263, 0.4), (331, 0.3), (419, 0.25), (527, 0.2)]:
        wob = 1 + 0.004 * np.sin(2 * np.pi * 3.1 * t)
        out += a * np.sin(2 * np.pi * f * wob * t) * np.exp(-t / (length * 0.45))
    shimmer = (rng.standard_normal(n) - lowpass(rng.standard_normal(n), 4)) * np.exp(-t / 0.9) * 0.15
    return (out * 0.22 + shimmer) * np.minimum(1, t / 0.01)


def cymbal(length=2.5):
    n = int(SR * length)
    noise = rng.standard_normal(n)
    return (noise - lowpass(noise, 3)) * env_exp(n, 0.7) * 0.28


def string_note(note, length, vel=1.0):
    """Bowed low string: detuned saws, filtered, a fast bow attack."""
    n = int(SR * length)
    t = np.arange(n) / SR
    out = np.zeros(n)
    for det in (-0.08, 0.0, 0.07):
        f = midi(note) * 2 ** (det / 12)
        ph = (f * t) % 1.0
        out += 2 * ph - 1
    out = lowpass(out, 22)
    e = np.minimum(1, t / 0.02) * np.minimum(1, (length - t) / 0.03)
    return out * np.clip(e, 0, 1) * 0.11 * vel


def lead(notes, step):
    """Erhu / dizi-like lead: a sawtooth-ish voice with vibrato growing on long notes and slides between notes."""
    total = len(notes) * step
    n = int(SR * total)
    t = np.arange(n) / SR
    freq = np.zeros(n)
    amp = np.zeros(n)
    prev = None
    for i, note in enumerate(notes):
        a, b = int(i * step * SR), int((i + 1) * step * SR)
        if note is None:
            prev = None
            continue
        target = midi(note)
        seg = np.arange(b - a) / SR
        if prev is not None:
            glide = np.minimum(1, seg / 0.06)
            freq[a:b] = prev + (target - prev) * glide
        else:
            freq[a:b] = target * (1 - 0.03 * np.exp(-seg / 0.03))
        amp[a:b] = np.minimum(1, seg / 0.03)
        prev = target
    # hold the last pitch through the rests that follow a note (legato phrases)
    vib_depth = 0.006 + 0.012 * np.minimum(1, (t % (step * 2)) / (step * 2))
    f = freq * (1 + vib_depth * np.sin(2 * np.pi * 5.5 * t))
    phase = 2 * np.pi * np.cumsum(f) / SR
    tone = np.sin(phase) + 0.45 * np.sin(2 * phase) + 0.25 * np.sin(3 * phase) + 0.12 * np.sin(4 * phase) + 0.06 * np.sin(5 * phase)
    breath = lowpass(rng.standard_normal(n), 12) * 0.03
    amp = lowpass(amp, 400)
    return (tone + breath) * amp * 0.16


def guzheng_gliss(start_note, length, up=True):
    """A fast run over the pentatonic (guzheng glissando)."""
    notes = [n + o for o in (0, 12, 24) for n in (50, 53, 55, 57, 60)]
    if not up:
        notes = notes[::-1]
    n = int(SR * length)
    out = np.zeros(n + SR)
    step = length / len(notes)
    for i, note in enumerate(notes):
        m = int(SR * 0.6)
        tt = np.arange(m) / SR
        f = midi(note)
        s = sum(a * np.sin(2 * np.pi * h * f * tt) * np.exp(-tt / (0.5 / (1 + h * 0.8))) for h, a in [(1, 1), (2, 0.5), (3, 0.3), (4, 0.15)])
        i0 = int(i * step * SR)
        out[i0:i0 + m] += s * 0.2 * (0.6 + 0.4 * i / len(notes))
    return out


def brass(chord, length, crescendo=True):
    n = int(SR * length)
    t = np.arange(n) / SR
    out = np.zeros(n)
    for note in chord[1:4]:
        for det in (-0.05, 0.05):
            f = midi(note) * 2 ** (det / 12)
            ph = (f * t) % 1.0
            out += 2 * ph - 1
    out = lowpass(out, 14)
    e = (t / length) ** 1.5 if crescendo else np.minimum(1, t / 0.2) * np.exp(-t / length)
    return out * e * 0.06


def riser(length):
    n = int(SR * length)
    t = np.arange(n) / SR
    noise = rng.standard_normal(n)
    mix = (1 - t / length) * lowpass(noise, 40) + (t / length) * (noise - lowpass(noise, 3))
    return mix * (t / length) ** 2.2 * 0.3


def add(buf, sig, at, gain=1.0, pan=0.0):
    i = int(at * SR)
    if i >= buf.shape[1] or i < 0:
        return
    j = min(buf.shape[1], i + len(sig))
    s = sig[: j - i] * gain
    buf[0, i:j] += s * (1 - max(0.0, pan))
    buf[1, i:j] += s * (1 + min(0.0, pan))


def render(timeline):
    beat = 60.0 / timeline["bpm"]
    segments = timeline["segments"]
    total_beats = sum(b for _, b in segments)
    buf = np.zeros((2, int((total_beats * beat + 5) * SR)))
    BIG, SMALL, RIM, GONG, CYM = taiko(True), taiko(False), rim(), gong(), cymbal()
    pos = 0
    phrase_no = 0
    for kind, beats in segments:
        start = pos * beat
        if kind == "intro":
            # a gong, slow taiko building into a roll, a rising brass swell and a guzheng run into the first cut
            add(buf, GONG, start, 1.0)
            for b in range(beats):
                add(buf, BIG, start + b * beat, 0.5 + 0.5 * b / beats, pan=-0.15 if b % 2 else 0.15)
            for s in range(8):
                add(buf, SMALL, start + (beats - 2) * beat + s * beat / 4, 0.4 + 0.08 * s)
            add(buf, brass(CHORDS[0], beats * beat), start, 1.4)
            add(buf, guzheng_gliss(50, beat * 1.5), start + (beats - 1.5) * beat, 1.0, pan=0.3)
            add(buf, lead([69, None, 72, 74, None, None, 72, 69], beat / 2), start + beat * 2, 0.9)
        elif kind == "chapter":
            # a break: gong, taiko call and answer, a riser, a guzheng run into the drop
            add(buf, GONG, start, 0.8)
            for b, g in [(0, 1.0), (1, 0.6), (1.5, 0.6), (2, 0.9), (3, 0.5), (3.25, 0.6), (3.5, 0.7), (3.75, 0.8)]:
                add(buf, BIG if b in (0, 2) else SMALL, start + b * beat, g)
            add(buf, riser(beats * beat), start, 0.9)
            add(buf, brass(CHORDS[phrase_no % 4], beats * beat), start, 1.0)
            add(buf, guzheng_gliss(50, beat), start + (beats - 1) * beat, 1.0, pan=-0.3)
        elif kind in ("slides", "outro"):
            outro = kind == "outro"
            add(buf, CYM, start, 0.9)
            add(buf, BIG, start, 1.2)
            for b in range(beats):
                t0 = start + b * beat
                bar = (pos + b) // 4
                chord = CHORDS[bar % 4]
                if outro and b >= 4:
                    if b == 4:
                        add(buf, GONG, t0, 1.1)
                        add(buf, BIG, t0, 1.3)
                        add(buf, brass(chord, (beats - 4) * beat + 3, False), t0, 1.6)
                        add(buf, lead([62, None, None, None, None, None, None, None], beat), t0, 0.8)
                    continue
                # drums: big taiko on 1 and 3, small on the off-beats, rim clicks on the 16ths
                add(buf, BIG if b % 2 == 0 else SMALL, t0, 1.0 if b % 2 == 0 else 0.8)
                add(buf, SMALL, t0 + beat * 0.5, 0.45)
                if b % 4 == 3:
                    add(buf, SMALL, t0 + beat * 0.75, 0.5)
                for s in range(4):
                    add(buf, RIM, t0 + s * beat / 4, 0.8 if s % 2 else 0.5, pan=0.3 if s % 2 else -0.3)
                # low strings: an 8th-note ostinato on the root and the fifth
                for s in range(2):
                    note = chord[0] + (7 if s == 1 and b % 2 else 0)
                    add(buf, string_note(note, beat / 2 * 0.95), t0 + s * beat / 2, 1.0)
                    add(buf, string_note(note + 12, beat / 2 * 0.95), t0 + s * beat / 2, 0.5)
                # the lead: a phrase every 2 bars
                if (pos + b) % 8 == 0:
                    ph = PHRASES[phrase_no % len(PHRASES)]
                    phrase_no += 1
                    notes = [None if i is None else PENTA[i] for i in ph]
                    add(buf, lead(notes, beat / 2), t0, 1.0, pan=0.1)
                if b % 4 == 0:
                    add(buf, brass(chord, 4 * beat, False), t0, 0.7)
        pos += beats

    buf = np.tanh(buf * 1.2)
    peak = np.max(np.abs(buf)) or 1.0
    buf = buf / peak * 0.9
    n = buf.shape[1]
    fade_in = int(0.03 * SR)
    buf[:, :fade_in] *= np.linspace(0, 1, fade_in)[None, :]
    fade_out = int(3.0 * SR)
    buf[:, n - fade_out:] *= np.linspace(1, 0, fade_out)[None, :]
    return buf


def write_wav(path, buf):
    data = (np.clip(buf.T, -1, 1) * 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(2)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())


if __name__ == "__main__":
    timeline = json.load(open(sys.argv[1], encoding="utf-8"))
    write_wav(sys.argv[2], render(timeline))
    beats = sum(b for _, b in timeline["segments"])
    print(f"music: {beats} beats at {timeline['bpm']} BPM ({beats * 60 / timeline['bpm']:.1f} s) -> {sys.argv[2]}")
