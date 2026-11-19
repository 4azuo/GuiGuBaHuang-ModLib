# Line G — "The Hundred-Year Wager" (Lan Qiu `{W}`; lifelong, ~100 years)

Dialogue of the line (approved 2026-11-19, coded: `ModMain/Story/Lines/LineG_*.cs`, texts in `story_localText.json`). Design: `../06-Storylines-Draft.md` section 4; conventions:
`00-Conventions.md`.

Voice of `{W}`: proud, quick to bet, never admits defeat; calls `{p}` "rival" for decades, then by name.
The line counts **wagers won** by each side (`wins_p`, `wins_w`) and **debts of life** (`saved_p`, `saved_w`).
An act opens at the later of its year (since G0) and its realm.

## G0. The wager (start; player grade 1-2; 1 % a month)
1. **A brawl at the gate.**
   - *Outside a sect gate, a young cultivator shoves through the queue and knocks your bundle into the mud.*
   - **{W}:** "Out of the way. Some of us are going to be immortals."
   - ▸ Shove back (small battle, 1 round) ▸ "After you, immortal." ▸ Say nothing, pick up your things
2. **The bet.**
   - *Whatever happened, {W} looks you over twice.*
   - **{W}:** "You. You've got the look of someone who thinks they're special. Let's make it interesting — whoever reaches **Golden Core** first gets the other's pouch. And writes the winner's name in their own journal."
   - ▸ "Deal." ▸ "I don't gamble." (he bets anyway: "Then you lose by default, rival.")
   - → Story item "Lan Qiu's Pouch Token"; the realm race starts.
3. **Parting.** **{W}:** "Don't die before I beat you. It would ruin everything."

## G1. First rematch (15 years / grade 3)
1. *A letter, folded into a paper crane: "Rival. Fifteen years. Mountain pass of {T}. Come and lose."*
2. **The duel** (real battle, his grade = yours).
   - Won → `wins_p` +1. **{W}:** "Luck. Pure luck. …Your footwork is better than last time."
   - Lost → `wins_w` +1. **{W}:** "Ha! Write it down. Write it down twice."
3. **Interrupted.** *Bandits who heard of two tired cultivators surround the pass.*
   - **{W}:** "Truce? Just for the length of this fight."
   - → Battle side by side. → G2 later.

## G2. A life owed (30 years / grade 4)
1. **The lair.** *A rumour: a beast lair full of spirit herbs. You both arrive the same night.* (whoever is weaker falls in)
2. **Into the dark.**
   - If **he** fell: *His voice from the pit: "Don't — don't you dare laugh."* ▸ Climb down and fight the beast (battle; `saved_p` +1) ▸ Throw a rope and leave ▸ Walk away
   - If **you** fell: *A rope, then {W} dropping down beside you, sword out.* "Don't look at me like that. I need you alive to lose to me." (`saved_w` +1)
3. **The fire after.**
   - **{W}:** "I had a brother. He cultivated faster than me. Died at Foundation, trying to keep up with someone. …That's not why I'm telling you. I'm not telling you anything."
   - Walked away → he survives alone, scarred: the line leans to "Eternal Rival".

## G3. His sect burns (45 years / grade 5; time limit 2 years)
1. *For the first time in forty-five years, a letter without an insult.* "Rival — they came at night. Red banners. I need help. Don't make me ask twice."
2. ▸ Fight for his sect (town / sect battle) ▸ Shelter him (he lives with you for a year; intimacy +200) ▸ Refuse
3. **After.**
   - Fought/sheltered: **{W}:** "…I owe you. Don't make a face. I hate owing."
   - Refused / late: *He goes alone. He survives, but loses a realm.* **{W}:** "I'll remember this. Every day."

## G4. Two old men (70 years / grade 6)
1. *Tea on a mountain peak. He has grey at his temples now.*
   - **{W}:** "Seventy years. {0} wins for me, {1} for you. Who's counting." (he is counting)
2. ▸ "One last duel." (battle) ▸ "Let's train a disciple together instead." ▸ "Let's just drink."
3. **{W}:** "You know what I hated most? That you never once looked worried. Not once in seventy years."

## G5. The last wager (100 years / grade 7, or when his life ≤ 5 years)
1. **The tribulation / the deathbed.**
   - Breaking through: *Thunder gathers over his cave.* **{W}:** "Last bet. If I survive this, I win everything."
     ▸ Stand in the lightning with him (lightning trial battle) ▸ Watch from afar
   - Dying: *He is thin now, smiling at the ceiling.* **{W}:** "Looks like you outlived me. That counts as a win. Don't let it go to your head."
2. **The pouch.** *He presses the old pouch from G0 into your hands — still full, a century of interest in it.*
3. → Ending by the counts (below).

## Endings G
- **Lifelong Friend** (`saved_p + saved_w` ≥ 2 and G3 helped): he joins your battles; his sect / disciples are friendly. *"Rival. Friend. Same word, really."*
- **Eternal Rival** (`wins_w` > `wins_p`, or walked away in G2): a duel every 10 years; a technique scroll for each one you win.
- **Surpassed** (he reached every wager's realm first): a letter and a breakthrough pill. *"I waited at the top. It's lonely up here. Hurry."*
- **The Grave** (he died before G5): his sword and his disciple, who calls you "uncle". *On the gravestone, in his hand: "Here lies the winner."*
