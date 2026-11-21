# Line O — "Ashes of the Town" (child `{O}`, town `{T}`; merit, massacre)

Dialogue of the line (6.0, 2026-11-20, coded: `ModMain/Story/Lines/LineO_AshesOfTown.cs`, texts in `story_localText.json`). Conventions: `00-Conventions.md`.

Voice of `{O}`: a quiet child, then a determined young cultivator.

## O0. The child in the ashes (a ruin of a massacre; `O0_butcher` when the player did it, under karma)
- `O0_1`: {T}. Black beams, cold ash, the smell of an old fire. In the ruins of a house, a child sits by a broken doll and does not cry.
  - ▸ Take the child with you
  - ▸ Leave money with a farmer for the child
- `O0_butcher`: {T}. You know this place. You know whose sword made these ashes.<br>In the ruins of a house, a child sits by a broken doll and looks at you without fear - it does not know your face.

## O1. The question (2 years)
- `O1_1`: {O} is taller now, and quiet in a way children should not be.<br>{O}: "Every night I see the fire. Someone did it. Who? You're a cultivator - you know."
  - ▸ Tell the truth
  - ▸ Lie: "bandits, long dead"
  - ▸ "Hatred is a poor teacher"

## O2. The training (4 years)
- `O2_1`: {O} swings a wooden sword at dawn, again and again, until the hands bleed.<br>{O}: "Teach me. I'll be strong enough this time."
  - ▸ Spar with {O}
  - ▸ Teach patience instead

## O3. The reckoning / the new town (7 years)
- `O3_reckoning`: {O} stands on the road, a real sword now, the eyes of a grown cultivator.<br>{O}: "I found the old records of {T}. I know whose sword it was. I know whose bread I ate for years."<br>{O}: "Draw."
  - ▸ Lower your sword
  - ▸ Fight
- `O3_rebuild`: {O}: "I went back to {T}. Weeds in the streets, but the well is still sweet."<br>{O}: "Families would come back if there were walls and houses. It needs {0:#,##0} [icon:zm_lingshi]. Will you build it with me?"
  - ▸ Rebuild {T} ({0:#,##0} [icon:zm_lingshi])
  - ▸ Let the dead town rest
- `O3_lie`: {O}: "I never found those bandits. Maybe it doesn't matter now."<br>{O}: "I went back to {T}. Families would come back if there were walls and houses. It needs {0:#,##0} [icon:zm_lingshi]. Help me?"

## Endings
- `endO_Atonement`: Your sword falls in the dust. {O}'s blade stops a finger from your throat - and trembles, and is lowered.<br>{O}: "I can't. You raised me." The child of the ashes walks away. Something in your heart, long frozen, cracks open.
- `endO_Ashes`: {T} stays a ruin. Grass grows over it, and nobody remembers the child of the ashes.
- `endO_Rebuilt`: New walls of {T}, new roofs. Smoke from kitchens, not from fires.<br>{O}: "They asked me to be the town's guardian. I said yes - if you visit."

## Effects of the endings

- **Atonement** (the butcher who told the truth lowers the sword, or loses the duel): merit +2000, Dao heart +300.
- **Ashes** (the butcher kills the child, or the town left dead): Dao heart -300 after the duel.
- **Rebuilt** (pay the rebuilding): the money to the town budget, the ruin lifted (population back), merit +1000.

The truth: Dao heart +100; a lie: -100 (and the butcher is never found out).
