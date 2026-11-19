# NPC storylines — dialogue (approved 2026-11-16)

The dialogue of every step of the lines designed in `../06-Storylines-Draft.md` (same acts, names, time limits and
endings; the design rules are there). English only; once approved, the texts go to the other languages
(ch / tc / kr and the 8 locale languages, `Translate/ai_bridge.py`).

Conventions:
- `{p}` is the player's name; `{A}`, `{H}`... are the names of the cast NPCs (see 06, section 2).
- `▸` an option, `→` a result, *italics* narration.
- Every numbered step is one drama (sometimes followed by a battle); steps of an act may be months apart.
- An act opens when the previous one is done (checked monthly); a time limit (06) sends a late act to its fail branch.

Files: `A-Cold-Sword.md`, `B-Old-Huang-Debt.md`, `C-Exploding-Furnace.md`, `D-Sworn-Brothers.md`, `E-Herb-Garden.md`, `F-Red-Lotus.md`;
lines G-L (approved 2026-11-19, coded, tests TS-07..TS-12): `G-Hundred-Year-Wager.md`, `H-Roadside-Inn.md`,
`I-Sealed-Tomb.md`, `J-Cursed-Disciple.md`, `K-Thunder-Remnant.md`, `L-Heart-Demon.md`.

## Backgrounds

A narrated step (`Tell`) shows the drama background of its text key from `StoryLine.BACKGROUNDS` (a cave, a tomb,
bandits, a storm, a lake, a grave...; `xinjianhualuo1` for the keys not listed). Pick new ones in
`docs/GameAssets/index.html` (tab "Drama backgrounds").
