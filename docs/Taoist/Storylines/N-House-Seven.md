# Line N — "The House on the Old Street" (neighbour `{N}`, town `{T}` of the player's house; estates)

Dialogue of the line (6.0, 2026-11-20, coded: `ModMain/Story/Lines/LineN_HouseSeven.cs`, texts in `story_localText.json`). Conventions: `00-Conventions.md`.

Voice of `{N}`: a mortal widow, plain and warm, a little afraid of cultivators.

## N0. The cellar
- `N0_1`: Your house in {T}. A loose board under the bed, and under it, stone steps and a door sealed with a talisman gone grey with age.<br>A cultivator lived here long before you.
  - ▸ Break the seal
  - ▸ Seal it again
- `N0_2`: Behind the seal, a stone puppet lies in pieces among pill bottles and notebooks.<br>Your neighbour knocks at the door, pale.<br>{N}: "That noise... the old master's cellar? Grandmother said never to open it."
- `N0_sealed`: You press a new talisman over the old one. The board goes back.<br>Your neighbour waves from her doorstep.<br>{N}: "Good evening, immortal! The street is glad to have a cultivator again."

## N1. The bandits of the street (6 months)
- `N1_1`: {N}: "They come every month now - men with knives, a demonic cultivator with them. They take the rice, the cloth, the girls' bracelets."<br>{N}: "We are only mortals. You live on our street..."
  - ▸ Wait for them and fight
  - ▸ Hire guards for the street ({0:#,##0} [icon:zm_lingshi])
  - ▸ Mortals' troubles are not mine

## N2. The notes of the cellar (1 year)
- `N2_1`: The old master's notes, written in a tidy hand: he bought the whole street once, pulled the houses down for a manor, and grew rich on the treasure under it. The mortals cursed him until he died.<br>One page marks a second hoard - under the houses next to yours.
  - ▸ Buy them out and dig
  - ▸ Burn the page

## N3. The attack (time limit 1 year)
- `N3_1`: Smoke over the wall of {T}. Raiders at the gate, and the old street right behind it.<br>{N}: "They say the wall won't hold. Where will we go?"
  - ▸ Hold the gate
  - ▸ Pay for the wall ({0:#,##0} [icon:zm_lingshi] to the town)
  - ▸ Leave the town to its fate

## N4. The verdict of the street

## Endings
- `endN_Manor`: The neighbours' houses come down one by one; under them, a hoard of the old master. The street is quiet now - nobody lives on it but you.
- `endN_Home`: Lanterns on every door of the old street. {N} brings a bowl of dumplings.<br>{N}: "The children call it the immortal's street now. You'll always have a seat at our table."
- `endN_Empty`: Doors shut when you walk by. One by one, the families of the old street sell and move away.

## Effects of the endings

- **Manor** (the street bought out in N2): an artifact of grade +1; the neighbours leave.
- **Home** (2 of: the street protected, the bandits beaten, the gate held / the wall paid; not absent): merit, a herb every year.
- **Empty**: nothing; the street empties.

Opens when the player owns a house (EstateEvent). The wall money goes to the town budget.
