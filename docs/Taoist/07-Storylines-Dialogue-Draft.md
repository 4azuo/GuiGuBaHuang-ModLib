# NPC storylines — dialogue draft v2 (to approve, not coded yet)

The dialogue of every step of the lines designed in `06-Storylines-Draft.md` (same acts, names, time limits and
endings; the design rules are there). English only; once approved, the texts go to the other languages
(ch / tc / kr and the 8 locale languages, `Translate/ai_bridge.py`).

Conventions:
- `{p}` is the player's name; `{A}`, `{H}`... are the names of the cast NPCs (see 06, section 2).
- `▸` an option, `→` a result, *italics* narration.
- Every numbered step is one drama (sometimes followed by a battle); steps of an act may be months apart.
- An act opens when the previous one is done (checked monthly); a time limit (06) sends a late act to its fail branch.

---

## Line A — "Cold Sword" (Yue Han `{A}`, master `{M}`, sect `{S}`)

Voice of `{A}`: few words, short sentences, cold; warmer with intimacy; may call `{p}` by name.

### A0. Meeting (player grade ≥ 2, outside a town; 0.3 % a month)
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

### A1. Old wound (time limit 1 year)
1. *Night. Someone taps on your window: {A}, pale, a hand pressed to her side.* "The wound will not close. Their sword was poisoned."
2. Needs an antidote pill, or a herb of her grade or higher.
   - ▸ Give her a pill ▸ Give her a herb ▸ "I have nothing." (search the market)
3. **First truth.**
   - **{A}:** "The manual was never stolen. It was sold. I saw who received it — a woman in red, with a lotus on her fan."
   - → A2.
- **Late** → she heals alone, intimacy -100, then A2.

### A2. The letter (time limit 3 years)
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

### A3. The witness (time limit 2 years)
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

### A4. Sharpening (5-10 years after A3; seclusions count)
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

### A5. Return to the sect (time limit 2 years)
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

### Endings A
| Ending | Line | Condition / effect |
|---|---|---|
| Companion | "When I had nothing, you stood beside me. Walk the rest of the road with me, {p}." | Won, intimacy ≥ 500: dao companion proposal. |
| Sect Master | "I will build a sect where no disciple is betrayed by a master. Its first elder should be you." | Won, intimacy < 500: her sect; exchange discount, disciples help in battle. |
| Peace | "The sword is quieter now. So am I." | "Let it go" with intimacy ≥ 300: a sword a year. |
| Fallen | "Mercy was a lie they taught me. Lady Honglian taught me the truth." | "Let it go" with intimacy < 300, or abandoned in "Broken Sword": bounty on the player, boss of the final battle. |
| Sacrifice | *She steps in front of the blow meant for you.* "…Your turn… to live." | The player is about to die in the A5 battle: her sword (signature weapon). |
| Traitor | "You sold me once. I will collect the debt." | Handed over / warned `{M}`: she hunts the player every 3-5 years. |

---

## Line B — "Old Huang's Debt" (Old Huang `{H}`, town `{T}`)

Voice of `{H}`: talkative, whiny, flattering, comic; sincere in danger.

### B0. Bargain (player opens the market of `{T}`; 1 % each time)
1. **The pitch.**
   - **{H}:** "This [{item}] — worth three times the price, I swear on my grandmother's tomb. For you? Half."
   - ▸ Buy it ▸ "Your grandmother must be very busy."
2. **The claim** (next day).
   - *Two masked men block your path.* "That [{item}] belongs to the Red Lotus. Return it, and keep your hands."
   - ▸ Return it ▸ Fight ▸ Run to {H}
3. **The old man trembles.**
   - **{H}:** "They came? Already? Oh heavens, oh ancestors, oh my aching knees—"
   - → B1 (returned: the line pauses 2 years first).

### B1. Confession (time limit 2 years)
1. **Hiding.** He hides in a warehouse of `{T}` or a nearby town; the player finds him.
2. **His story.**
   - **{H}:** "When my caravan sank, I borrowed from the Red Lotus. Ten years of interest… I sold their goods to pay them back. Badly, as you saw."
3. **His request.**
   - **{H}:** "Vouch for me at the bank? Just a signature! What could go wrong?"
   - ▸ Vouch for him (if he does not repay, the debt is yours) ▸ Lend him spirit stones yourself ▸ "Every 'what could go wrong' ends in a funeral."
   - → B2.

### B2. Last caravan (time limit 1 year)
1. **The plan.**
   - **{H}:** "One last caravan to {T2}. Silk, herbs, a little spirit jade. If it arrives, I pay everyone. If not… I become a very sincere monk."
2. **Escort.** The caravan travels to `{T2}`; an ambush on the way: bandits, or Red Lotus killers if the item was not returned in B0.
3. **The share.**
   - **{H}:** "Your share. Don't count it in front of me, I'll cry."
   - → The player gets 10 % of the profit; part of the debt is paid; → B3.
- **Late or robbed → "Vanished Debtor":**
  - *His shop is empty. A note: "Gone to see my cousin. Far away. Very far."*
  - The debt doubles. Find him in a far town: he pays in goods, → B3.

### B3. Red ledger (time limit 2 years)
1. **The envoy.**
   - *A man in red silk walks into {H}'s shop.* "The Lady sends her regards. Interest is due."
2. **The secret.**
   - **{H}:** "I saw his ledger. Not just me — half the merchants of this region owe her. She is buying the towns, one debt at a time."
   - ▸ Steal the ledger (battle with the envoy's guards) ▸ Pay the interest ▸ Stall him
3. **Consequences.**
   - Ledger stolen: the indebted merchants become allies (market discounts) and the ledger is used in the final battle.
   - Interest paid: 2 quiet years.
   - Stalled: → B4 at once.

### B4. Siege (next town war, at most 3 years)
1. **Red banners.**
   - *Red banners rise outside {T}. The Red Lotus demands the town as payment for {H}'s debt.*
2. **Preparations.**
   - **{H}:** "I have savings. Secret savings. Very secret. Take them — hire guards, buy arrows, anything!"
   - ▸ Hire guards (town defense budget) ▸ Negotiate a tribute ▸ Stay out of it
3. **The defense** (town war).
   - Won: Merchant Prince.
   - Lost or stayed out: Fallen Town.
   - Tribute: the town loses 30 % of its budget a year for 5 years; the siege may come back.

### B5. Aftermath (after B4, or when `{H}` dies of old age)
1. **A drink.**
   - **{H}:** "You know, {p}, in forty years of trade you are my only customer who never haggled. Fool."
2. **A gift.** A precious item of the player's grade.
3. **The ending**, from the earlier choices.

### Endings B
| Ending | Line | Effect |
|---|---|---|
| Merchant Prince | "From today, your coin is no good in my shops. Well — half good." | Market discount, yearly goods. |
| The Heir | *A letter in shaky handwriting:* "To my only honest customer…" | He dies of old age: his listings and a good buy order. |
| Fallen Town | *The banners stay red.* | Demonic town: tax ×2, hostile guards. |
| Betrayed | *A note on an empty desk:* "Sorry, friend. A man must live." | Vouched and he fled: the player owes; bounty on him. |
| Benefactor | "You trusted a fool. The fool will not forget." | Lent and repaid: once, he pays off a bounty on the player. |

---

## Line C — "Exploding Furnace" (Su Xiaoman `{X}`, grandmaster `{G}`; comic)

Voice of `{X}`: quick, jokes, trouble; calls herself "little sister".

### C0. Test subject (player enters `{X}`'s town; 1 % each time)
1. **BOOM.**
   - *BOOM. Purple smoke rolls down the street. A girl with singed eyebrows waves at you through the haze.*
   - **{X}:** "You look healthy. Want to test a pill? Ninety percent safe! Eighty. Probably."
   - ▸ "…Fine." ▸ Back away slowly (she insists once more a month later; refused twice → line closed)
2. **Side effect.**
   - *Your hair turns lavender for three months. {X} takes notes, delighted.*
   - → Temporary trait "Lavender Hair", mood +10.
3. **Apology.**
   - **{X}:** "Sorry sorry sorry! Here, a real pill. Works every time. Mostly."
   - → A pill of the player's grade; → C1.

### C1. Moonfrost Lotus (time limit 2 years)
1. **The exam.**
   - **{X}:** "The Alchemy Guild exam is next spring. I need a Moonfrost Lotus. One valley. One very large snake. Hehe."
2. **The map.** Buy the valley map from a herb seller, or trade a herb for it.
3. **The valley.** Battle with the giant snake; the lotus. Granny Shi is there (opens line E).
   - **Granny Shi:** "Young ones, always in a hurry. Take the lotus — and leave the roots."
- **Late → "Failed Exam":**
  - *{X} went alone. She comes back limping, empty-handed, and does not joke for a week.*
  - ▸ Nurse her (pills; C2 one year later) ▸ Leave her be (Last Explosion)

### C2. The exam (next spring)
1. **The fee.**
   - **Examiner:** "Furnace fee: {0} spirit stones." *{X} looks at you with enormous eyes.*
   - ▸ Pay ▸ "You have savings."
2. **Fire control.** Three questions; 2 of 3 right answers pass.
   - **{X}:** "Big fire or small fire? Quick, quick, it's bubbling!"
3. **Result.**
   - Pass: "I PASSED! Did you see? Did everyone see?!" → C3.
   - Fail: "…Next year. I'll do it next year." → the exam again next spring.

### C3. Stolen recipe (time limit 1 year)
1. **The theft.**
   - **{X}** *(no jokes for once)*: "Grandmaster {G} presented my recipe to the Guild. My recipe. He laughed when I said so."
2. **Proof.** Two Guild apprentices will speak; each wants a favor (an item, or beating the bully who torments them).
3. **Confrontation.**
   - ▸ Expose him before the Guild (both apprentices, or 500 reputation) ▸ Duel him ▸ "Let it go. You'll make better ones."
   - → Exposed / duel won: C4. Let go: intimacy -100, Last Explosion.

### C4. Grand Gathering (the year after C3)
1. **The challenge.**
   - **{G}:** "Little thief, face me at the Grand Gathering. The loser leaves alchemy forever."
2. **Sabotage.** The night before, someone breaks into her workshop: battle with the saboteur (lost: she competes with her old furnace, lower chance).
3. **The contest.**
   - *Two furnaces, one hall, a thousand eyes. {X}'s pill rises from the flames, glowing like a small moon.*
   - → Won: Master Alchemist, or Companion (intimacy ≥ 300). Lost: Last Explosion.

### Endings C
| Ending | Line | Effect |
|---|---|---|
| Master Alchemist | "Master {X}'s pills — for you, at cost. For everyone else, double!" | Her pills -30 %. |
| Companion | "I'm coming with you. Someone has to test pills on your enemies." | Intimacy ≥ 300: joins the team without pay; comic dramas when she tests pills on teammates. |
| Last Explosion | *A small teahouse opens. The tea is excellent. Nothing explodes.* | Food that restores mood now and then. |

---

## Line D — "Sworn Brothers" (Iron Ox `{N}`, Murong Ge `{C}`, `{L}`; comic; love triangle)

### D0. Eleven bowls (an inn of a town with both of them)
1. **No money.**
   - **Innkeeper:** "Eleven bowls of noodles and no money?!"
   - **{N}:** "I can pay with a fight! A good one!"
   - **{C}** *(strumming)*: "Eleven bowls, one fool, no coin — a tragedy in three verses."
2. **Choice.**
   - ▸ Pay for him (Ox intimacy +150) ▸ "Fight me, then." (duel; win or lose, Ox intimacy +100: "Ha! My ribs! Again tomorrow?")
3. **Wine.**
   - **{C}:** "A fool, a hero and a poet walk into an inn. I'll write it down."
   - → D1.

### D1. Beast hunt (time limit 1 year)
1. **The invitation.**
   - **{N}:** "There's a beast eating the farmers' oxen. Three of us — easy! Ge, you can sing at it."
2. **The hunt.** Battle with both as allies against a beast of the player's grade.
3. **Campfire.**
   - **{N}:** "My sect wants me to fight in their wars. I just want to punch bandits."
   - **{C}:** "I write love poems because the other kind hurts too much."
   - ▸ "Then swear brotherhood with me." (intimacy ≥ 200 with both) ▸ "Get some sleep."

### D2. Forty-two poems (time limit 1 year)
1. **The request.**
   - **{C}:** "I have written forty-two poems for {L}. She has read none. I need a… strategist."
2. **The gift.** Choose a gift for `{L}`; one she likes (her hobbies) doubles the effect.
3. **The confession.**
   - ▸ Help him sincerely (Ge ↔ `{L}` intimacy +200; they may become companions) ▸ Sabotage (Ge intimacy -100) ▸ Court {L} yourself
   - → Courting: `{L}` chooses the higher intimacy. The player wins: Rivals (D4 still comes). Ge wins: he invites the player to the wedding.

### D3. Murong's blood debt (time limit 2 years)
1. **The secret.**
   - **{C}** *(no music tonight)*: "My family were merchants. They refused the Red Lotus's loan. One night, the house burned with them in it."
2. **The trail.** Track a Red Lotus killer to a ruined shrine: battle.
3. **Last words.**
   - **Killer** *(dying)*: "The Lady… will… burn you too."
   - ▸ "We'll end her together." (Ge joins the final battle) ▸ "Revenge will not bring them back."

### D4. Two battle lines (a town war with Ox on the other side)
1. **Across the field.**
   - **{N}:** "Brother… my sect ordered me. I can't run."
   - ▸ "Lower your fists and walk away with me." ▸ "Then fight me, and don't hold back." ▸ Switch sides (your home town hates you)
2. **The duel** (if he does not walk away): with Ox intimacy ≥ 300, he holds back.
3. **After the battle.**
   - Ox alive: "Your fists are heavier than my sect's orders." → Three Sworn if Ge's intimacy ≥ 300 too.
   - Ox dead: A Brother Lost.

### Endings D
| Ending | Line | Effect |
|---|---|---|
| Three Sworn | "Heaven above, earth below — three of us, one life!" | Both help in battles; they put a bounty on the player's killer. |
| Murong as Companion | "Forty-three poems. This one, you'll read." | Female player or same-sex option; his Romantic trait becomes Faithful. |
| A Brother Lost | *A slow song drifts over the grave.* "He always said the noodles here were the best." | "Ox Fist" technique. |
| Rivals | "You won her. Next time we meet, it's on the battlefield." | Ge fights on the enemy side of town wars. |

---

## Line E — "Granny's Herb Garden" (Granny Shi `{O}`, A Li `{Y}`; earnest and warm)

A Li as a child: narration over a background, or told by `{O}` (no portrait); an NPC with a portrait at 16.

### E0. Mountain garden (from line C, or in the mountains; 0.3 % a month)
1. **The request.**
   - **{O}:** "Child, these old knees cannot reach the high terraces anymore. Will you keep my garden for a while?"
   - ▸ "Of course, grandmother." ▸ "I have no time for gardens."
2. **First day.** Weeding, watering, chasing off the monkeys stealing peaches (a fun battle with spirit monkeys). `{O}` intimacy +100.
3. **First lesson.**
   - **{O}:** "A sword is like a root: it grows deep before it grows tall."
   - → +1 random basis. Then a lesson each yearly visit (at most 5; intimacy +50 each).
   - With intimacy ≥ 300 she lets the player sit in seclusion in her garden (see E4).

### E1. Child in the ashes (time limit 1 year)
1. *A thin girl hides behind {O}'s skirts, clutching a wooden spoon like a sword.*
2. **{O}:** "Her village burned. Red banners, the survivors said. I am too old to raise her, and too old to say no to her."
3. **Choice.**
   - ▸ Adopt her (an adopted child in the family system) ▸ Send her to a sect (a sect disciple at 16, later an ally) ▸ Leave her with you, grandmother (`{O}` dies sooner; A Li wanders → Lost Child if not found within 3 years)

### E2. A Li grows up (at 8, 12 and 15; visit within a year each)
1. **Age 8.**
   - *{Y} burned the rice again. She says it was a "fire technique".*
   - ▸ Teach her to cook (alchemist) ▸ Teach her a stance (sword)
2. **Age 12.**
   - *{Y} came home with a black eye.* "He called you a nobody. I corrected him."
   - ▸ Scold her ▸ Laugh ▸ Teach her when to fight
3. **Age 15.**
   - *{Y} asks what her parents were like. {O} looks at you.*
   - ▸ Tell the truth (red banners: she joins the final battle) ▸ "They loved you."

Her path adds up over the three moments: alchemist / sword / free. All three missed → Lost Child.

### E3. Granny falls ill (`{O}` has ≤ 3 years of life left)
1. **{Y}** *(narration)*: "Grandmother coughs at night now. She says it's the wind."
2. **The herb.** A longevity herb: an encounter outside town with a guardian beast (battle), or the market at a very high price.
3. **Her choice.**
   - **{O}:** "You brought this for me? …Child. A garden needs old trees to fall, so young ones get the light."
   - ▸ Insist (she lives 10 more years; intimacy +200) ▸ Respect her wish (last lesson: +3 basis)

### E4. Last wish (when `{O}` dies of old age)
1. *{O} sleeps under the old pear tree and does not wake. In her hand, a seed wrapped in silk.*
2. *A letter:* "To the one who kept my garden: plant it where you will rest."
   - ▸ Keep the garden ▸ Give it to {Y}
3. **The garden.**
   - Kept, with intimacy ≥ 300 (when she died): a **manual seclusion** place — 1-20 years, spirit stones, exp a year, interrupted like a time skip.
     - **{O}** *(her last words, remembered)*: "Sit under the pear tree as long as you need. The garden will keep time for you."
   - Kept, intimacy < 300: a keepsake only.
   - Given to A Li: Successor.

### Endings E
| Ending | Line | Effect |
|---|---|---|
| Successor | "You taught me to stand. Now let me carry the family." | A Li becomes alchemist or swordswoman; preferred heir when continuing as a descendant. |
| Lost Child | "Nobody came for me. She did." | A Li grows up under Lady Honglian; enemy in the final battle. |
| Garden of Generations | *The pear tree blooms for the third generation.* | Descendants get the first lesson. |

---

## Final battle — "Red Lotus" (Lady Honglian `{R}`)

Opens when line A or B reached act 4 and at least 2 other lines ended. Time limit 5 years; late → "Spreading Demon
Realm" (2 more fallen towns a year, harder battle).

### F1. Demon realm
1. *Three towns raise red banners in one season. Merchants whisper a name: the Lady.*
2. **{R}** *(letter)*: "Little thread-puller, you have been busy. Come to the Lotus Pavilion. Bring your friends — I so love company."
3. ▸ Gather allies (F2) ▸ Go alone now (F3, harder, more reward)

### F2. Allies (one short drama per ally of a good ending)
- **{A}:** "Finally."
- **{H}:** "I'll bring the arrows. And snacks."
- **{N}:** "Who do I punch?"
- **{C}:** "For my family."
- **{Y}:** "For my village."
- **{X}:** "I made explosive pills! On purpose this time!"

Enemies of bad endings (Fallen Yue Han, Lost A Li, Rival Murong) join her.

### F3. Lotus Pavilion
1. **The gate.** Battle with her guardians and the enemies from the lines.
2. **The Lady.**
   - **{R}:** "So many little threads, all tied back to me. Kneel, and I will let your friends keep their heads."
   - ▸ "My friends answer for themselves." ▸ Kneel (secret ending)
3. **The last battle** with `{R}`, the allies at the player's side.

### F4. Aftermath
1. *The red lotus withers. The towns she held open their gates.*
2. Rewards: "Red Lotus Mirror" (unique item), her towns freed, her debt ledger burned.
3. **Farewells:** one line from each ally, then back to their own endings.

**Kneel (secret ending):**
- **{R}:** "Wise child. From today, my towns are your towns — and the world's hatred is yours too."
- The whole world puts bounties on the player, who rules her towns.

---

## To approve
1. The steps and lines above (v2: 6 acts for A and B, 5 for C, D and E, 4 for the final battle; 3 steps each).
2. The new episodes:
   - **A:** the old wound, the witness Old Lu, the sharpening.
   - **B:** the last caravan, the red ledger.
   - **C:** the exam, the Grand Gathering.
   - **D:** the beast hunt, Murong's blood debt.
   - **E:** A Li growing up, Granny's illness.
3. **Links:** A Li's village and Murong's family were both destroyed by the Red Lotus; the red ledger of line B is used in the final battle.
