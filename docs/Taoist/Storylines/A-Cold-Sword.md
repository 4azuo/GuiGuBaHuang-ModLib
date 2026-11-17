# Line A — "Cold Sword" (Yue Han `{A}`, master `{M}`, sect `{S}`)

Dialogue of the line (approved 2026-11-16). Design: `../06-Storylines-Draft.md`; conventions: `00-Conventions.md`; code: `3161035078/ModProject/ModCode/ModMain/Story/`.

Voice of `{A}`: few words, short sentences, cold; warmer with intimacy; may call `{p}` by name.

## A0. Meeting (player grade ≥ 2, outside a town; 0.3 % a month)
1. **The hunt.**
   - *A woman in torn sect robes stumbles out of the reeds, a broken sword in her hand. Three disciples of {S} follow, blades drawn.*
   - **{A}:** "Stay out of this. It is not your fight."
   - **Disciple:** "Traitor {A} stole the sect's sword manual. Hand her over, and the sect will remember your kindness."
   - ▸ Fight the disciples ▸ "She's yours." ▸ Walk away
2. **After the fight** (won).
   - **{A}:** "…Thank you. I will not forget a debt."
   - *She tears a strip of blue silk from her sleeve and presses it into your hand.* "If you hear my name again, do not believe what they say."
   - → Story item "Blue Silk"; intimacy +100.
3. **The wanted notice** (1-3 months later, nearest town).
   - *A notice on the town gate: "Wanted — {A}, thief of {S}." Someone has drawn her face badly.*
   - ▸ Tear it down (the town master dislikes you, -20) ▸ Leave it
   - → A1.

- **Handed over** → "Traitor": *As they drag her away, her eyes find yours — no anger, only a cold, patient promise.*
- **Walked away** → she is met again 3 years later (once).

## A1. Old wound (time limit 1 year)
1. *Night. Someone taps on your window: {A}, pale, a hand pressed to her side.* "The wound will not close. Their sword was poisoned."
2. Needs an antidote pill, or a herb of her grade or higher.
   - ▸ Give her a pill ▸ Give her a herb ▸ "I have nothing." (search the market)
3. **First truth.**
   - **{A}:** "The manual was never stolen. It was sold. I saw who received it — a woman in red, with a lotus on her fan."
   - → A2.
- **Late** → she heals alone, intimacy -100, then A2.

## A2. The letter (time limit 3 years)
1. **The plan.**
   - **{A}:** "My master kept every letter. One went astray — the courier was robbed on the road to {T}. Find it."
2. **The search.** The letter is either:
   - sold as "old letter" by an NPC at the market of `{T}`;
   - or carried by a robber near `{T}` (battle).
3. **The seal.**
   - *The letter bears {M}'s seal — and a red lotus pressed into the wax. {A} reads it twice, then folds it with steady hands.*
   - **{A}:** "One proof. They will say I forged it."
   - ▸ "Then we find a second." ▸ "Prove it was not forged." (one more year; you learn of Lady Honglian)
   - → A3.
- **Late → "Broken Sword":**
  - *Word comes from {S}: {A} was caught and thrown into the sect prison.*
  - ▸ Break her out (battle at the sect; won → A3) ▸ Do nothing (she escapes 5 years later and joins the demonic path: Fallen)

## A3. The witness (time limit 2 years)
1. **Old Lu.**
   - **{A}:** "Old Lu kept the sect's treasury. He saw the exchange. He fled the sect a month later — afraid."
2. **The killers.**
   - *The door is already broken. Two masked killers stand over Old Lu.*
   - → Battle: protect him.
3. **Testimony.**
   - **Old Lu:** "I will speak before the elders — if you keep me alive until then."
   - → A4.
- **He dies (lost battle or late) → steal the treasury ledger:**
  - *The sect treasury is guarded day and night. The ledger is the only proof left.*
  - Sneak in: battle with the sect guards; risk of a bounty from `{S}`. Got it → A4.

## A4. Sharpening (5-10 years after A3; seclusions count)
1. **Sparring.**
   - **{A}:** "Proof is not enough against an elder. I must be strong enough to stand before him. Spar with me, {p}."
   - → Sparring battle (nobody dies); each win: intimacy +50, exp for her.
2. **A new blade.**
   - **{A}:** "My sword broke the day I left. I need a blade that will not."
   - ▸ Give a sword (an artifact of her grade or higher) ▸ Pay a smith (spirit stones) ▸ "Your will is the blade."
   - → A gifted sword: she wields it in the final battle; intimacy +100.
3. **Breakthrough.**
   - *Thunder rolls over the mountain. When the clouds part, {A} stands in the rain, her eyes clear for the first time.* "I am ready."
   - → She gains a grade.

## A5. Return to the sect (time limit 2 years)
1. **At the gate.**
   - **{A}:** "I will go to {S} and take back my name — with this sword, or with my life."
   - ▸ "Then we go together." ▸ "Let it go. Revenge will eat you alive." ▸ (secretly) Warn {M}
2. **Before the elders** (together).
   - **{M}:** "A thief and her accomplice. Elders, you see the shamelessness."
   - ▸ Show the letter and the witness (or the ledger) ▸ Challenge {M} to a duel
   - → Full proof: the elders stand aside, battle with `{M}` only. Missing proof: battle with the sect guardians and `{M}`.
3. **Mercy.**
   - **{A}:** "He is on his knees. Tell me, {p} — what is mercy worth?"
   - ▸ "Let the sect judge him." ▸ "Your choice, not mine."
   - → Ending by intimacy.

## Endings A
| Ending | Line | Condition / effect |
|---|---|---|
| Companion | "When I had nothing, you stood beside me. Walk the rest of the road with me, {p}." | Won, intimacy ≥ 500: dao companion proposal. |
| Sect Master | "I will build a sect where no disciple is betrayed by a master. Its first elder should be you." | Won, intimacy < 500: her sect; exchange discount, disciples help in battle. |
| Peace | "The sword is quieter now. So am I." | "Let it go" with intimacy ≥ 300: a sword a year. |
| Fallen | "Mercy was a lie they taught me. Lady Honglian taught me the truth." | "Let it go" with intimacy < 300, or abandoned in "Broken Sword": bounty on the player, boss of the final battle. |
| Sacrifice | *She steps in front of the blow meant for you.* "…Your turn… to live." | The player is about to die in the A5 battle: her sword (signature weapon). |
| Traitor | "You sold me once. I will collect the debt." | Handed over / warned `{M}`: she hunts the player every 3-5 years. |
