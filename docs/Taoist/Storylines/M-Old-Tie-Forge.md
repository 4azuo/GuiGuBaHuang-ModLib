# Line M — "Old Tie's Forge" (old smith `{F}`, town `{T}`; Artisan Dao)

Dialogue of the line (6.0, 2026-11-20, coded: `ModMain/Story/Lines/LineM_OldTieForge.cs`, texts in `story_localText.json`). Conventions: `00-Conventions.md`.

Voice of `{F}`: gruff, proud of his trade, speaks in images of fire and steel; warm under it.

## M0. The burned hand
- `M0_1`: The smithy of {T} smells of burned flesh. An old smith wraps his right hand in a wet rag.<br>{F}: "The furnace spat at me. Forty years, and it spat at me."<br>{F}: "The guild's exam is in three days. If nobody of my forge passes it, they take my license. Can you swing a hammer?"
  - ▸ Take the exam for him
  - ▸ Give him a healing pill
  - ▸ Not my business
- `M0_exam`: Three days of sparks. The guild masters frown, then nod.<br>{F}: "Ha! Crooked, but it holds an edge. You have the hands for it."
- `M0_2`: The pill closes the burn overnight. He flexes his fingers, amazed.<br>{F}: "I owe you, cultivator. A smith pays his debts in steel."

## M1. The ore of the vein (6 months later)
- `M1_1`: {F}: "One more blade before I die. Not a pretty one - a true one. For that I need ore from the {0}, the rich one past the town."<br>{F}: "Bring me ore, or the money to buy it ({1:#,##0} [icon:zm_lingshi]). Or go down yourself - something guards the vein."
  - ▸ Give him ore
  - ▸ Pay for the ore ({1:#,##0} [icon:zm_lingshi])
  - ▸ Go down into the vein
- `M1_2`: The ore glows red in his furnace. He works it with his left hand while you hold the tongs.<br>{F}: "Watch the color, not the hammer. The steel tells you when."

## M2. The apprentice or the secret (1 year)
- `M2_1`: A boy sweeps the smithy floor and steals glances at the anvil.<br>{F}: "My grandson's friend. Clumsy, stubborn. Like I was."<br>{F}: "My secrets should not die with me. Teach him with me - or take them yourself and go far."
  - ▸ Teach the boy together
  - ▸ Learn the secrets yourself

## M3. The guild (time limit 2 years; late: the forge burns)
- `M3_1`: Men in guild aprons stand at the door of the smithy, torches in hand.<br>A guild master: "An old fool with a cripple's hand, and a stranger at his anvil. Close it, or we close it."<br>{F}: "Over my bones."
  - ▸ Drive them off
  - ▸ Buy the guild's peace ({0:#,##0} [icon:zm_lingshi])

## M4. The last blade
- `M4_1`: The last fire of the forge. {F} is thin now, his beard white as ash.<br>{F}: "One blade left in these hands. Let it be what it will - or bring me ore of grade {0} and I'll make it a masterwork."
  - ▸ Let the forge decide
  - ▸ Bring ore of grade {0} for a masterwork
- `M4_burned`: The smithy is ashes. {F} sits on the cold anvil, his burned hand on his knee.<br>{F}: "The fire takes back what it gave. Take what I taught you, and keep it burning somewhere else."

## Endings
- `endM_Masterwork`: The ore sings in the fire. For three days and nights he does not sleep.<br>{F}: "There. That is the blade I was born to make." He smiles, and puts down the hammer for good.
- `endM_Heir`: The boy holds the tongs now, and his strokes are true.<br>{F}: "See? Clumsy and stubborn. A smith." He puts a blade in your hands. "His first. My last. Keep it."
- `endM_Secret`: You carry his secrets away: the color of the steel, the breath of the bellows, the patience of an old man's hammer.

## Effects of the endings

- **Masterwork** (ore of the player's grade at M4): an artifact of grade +1.
- **Heir** (the apprentice taught): an artifact; every year the apprentice sends one.
- **Secret** (secrets learned, or the forge burned): Forge exp.

Every step gives Forge exp (the craft learned if not yet); the apprentice gives merit.
