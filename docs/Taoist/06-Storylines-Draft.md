# NPC storylines — design (approved 2026-11-16, coded in `ModMain/Story/`; simplifications listed in `05-Backlog.md`)

The design of the NPC storylines: casting, rules and the acts of every line. The dialogue of every step is in
`Storylines/` (one file per line, same acts and names). Written in English; the other languages are translated from
the game texts (`Translate/ai_bridge.py`). Code: `3161035078/ModProject/ModCode/ModMain/Story/`.

## 0. Decisions (2026-11-16)

1. All 5 lines and the final battle are made.
2. Casting: wait for a fitting NPC; when none comes, a new NPC is created with a chance growing every year
   (2 % the first year, +2 % a year, at most 50 %).
3. Every act has a time limit; a failed or late act does not close the line, it opens a **fail branch**.
4. The love triangle of line D is kept.
5. The herb garden of line E is a **manual seclusion** place (1-20 years, spirit stones, exp a year, interrupted like
   a time skip), only with **intimacy ≥ 300 with Granny Shi** (her intimacy when she died, once she is dead).
   The garden is a **fixed point of the world map** (where Granny Shi meets the player, outside towns; no map button):
   the quest "Granny Shi's garden" leads there, and the garden window opens when the player stands on it (2026-11-17).
6. Every line has 5-6 acts of at least 3 steps each.
7. Tone, dialogue, fail branches, the "Peace" ending (A), the secret "Kneel" ending (final battle) and the numbers
   (chances 0.3-1 %, time limits, rewards) are approved.
8. Still open (not storyline): "coma instead of exp loss" when the player loses a battle.

## 1. Casting

- A role has **casting rules**:
  - sex;
  - age as a share of the lifespan (cultivators live long: "young" / "old" is relative);
  - looks (`beauty`, the game's beauty tiers `RoleBeauty`);
  - character: inner trait (`inTrait` 1-7) and outer traits (`outTrait` 8-19), the game's trait tables;
  - alignment (`standUp` / `standDown`);
  - grade compared with the player;
  - for some roles, a sect or a town.
- When a line opens, the role goes to the best-scoring living NPC that fits, has no other role, and is not the
  player's companion or descendant. None fits: wait, with the yearly creation chance above (the new NPC is set to
  the sex, age, traits and grade of the role; looks come from the game, rerolled a few times for the roles that need
  beauty).
- The name of the character is the name of the NPC in the world (`{A}`, `{H}`... in the dialogue); the names below
  are working names.
- Cast NPCs live as any NPC (age, break through, marry, may die). A death mid-line goes to the matching ending
  ("Sacrifice", "The Heir"...) or closes the line, with a Taoist log entry.
- Pacing: an act opens when the previous one is done and its conditions are met (checked monthly); it may come
  during a time skip / seclusion (its drama interrupts the skip).
- Rewards stay modest (no power creep): relations (allies, companions, enemies), small perks (discounts, yearly
  goods), one signature item per ending; no big exp.
- S&M page 3: "No NPC storylines" (and one toggle per line if wanted).
- Every choice and ending goes to the Taoist log; Taoist Settings gets a "Stories" page to read them again.

## 2. Characters

| # | Working name | Sex | Casting rules | Voice |
|---|---|---|---|---|
| 1 | **Yue Han** (exiled swordswoman) `{A}` | F | 15-35 % of life lived; looks ≥ "Pretty" (beauty 250); inner Upstanding / Selfless; outer Vengeful or Faithful; righteous; sword is her best weapon basis; same grade as the player or 1 above; in a sect | Few words, short sentences, cold; warmer with intimacy |
| 2 | **Old Huang** (indebted merchant) `{H}` | M | ≥ 60 % of life lived; inner Middle Way / Selfish; outer Family-Oriented or Glory Hound; rich; lives in a town with a market | Talkative, whiny, flattering, comic; sincere in danger |
| 3 | **Su Xiaoman** (apprentice alchemist) `{X}` | F | ≤ 15 % of life lived; inner Kind; outer Carefree; alchemy is her best art; grade below the player | Quick, jokes, causes trouble, calls herself "little sister" |
| 4 | **Iron Ox** (body cultivator) `{N}` | M | 15-40 % of life lived; plain looks; inner Upstanding; outer Loyal to friends; fist / palm is his best basis | Honest, blunt, big eater, naive comedy; loyal to brothers |
| 5 | **Lady Honglian** (demonic elder, the mastermind) `{R}` | F | 40-70 % of life lived; looks ≥ "Beautiful" (beauty 450); inner Evil / Wicked; outer Power-hungry; demonic; 1-2 grades above the player | Elegant, soft, mocking; threatens without raising her voice |
| 6 | **Murong Ge** (wandering zither poet) `{C}` | M | 15-30 % of life lived; looks ≥ "Pretty" (beauty 250); outer Romantic; inner Middle Way | Lyrical, recites poems, flirts; light humor, hidden grief |
| 7 | **Granny Shi** (hermit herbalist) `{O}` | F | ≥ 75 % of life lived; inner Kind / Selfless; outer Traditional; no sect | Gentle, slow, speaks in images; calls the player "child" |
| 8 | **A Li** (orphan girl) `{Y}` | F | Created when line E opens: a child record (6-10 years), an NPC at 16 | Shy as a child, firm when grown; shaped by how the player raises her |

Other roles: `{M}` Yue Han's master (an elder of her sect), `{S}` her sect, Old Lu (a sect treasurer who fled),
`{G}` the grandmaster alchemist, `{L}` the woman Murong Ge courts, `{T}` / `{T2}` towns.

A Li as a child: no NPC, no portrait (the game has adult portraits only). Her lines are narration over a
background, or told by Granny Shi. At 16 she becomes an NPC (new, female, game looks) with a portrait.

## 3. Lines (acts; every act = at least 3 steps, see 07)

### Line A — "Cold Sword" (Yue Han, her master `{M}`)
| Act | Content | Time limit / fail branch |
|---|---|---|
| A0 Meeting | Sect disciples hunt a wounded swordswoman: save her (fight), hand her over, or walk away; a blue silk token; a wanted notice | Hand over → "Traitor" ending; walk away → meet again in 3 years (once) |
| A1 Old wound | She comes back poisoned: give a pill / herb; first truth (a woman in red bought the manual) | 1 year; late → she heals alone, intimacy -100 |
| A2 The letter | Find the lost letter of `{M}` (market of `{T}` or a robber); red lotus seal | 3 years; late → **"Broken Sword"**: she is jailed; rescue her (battle) or she escapes to the demonic path in 5 years |
| A3 The witness | Old Lu saw the deal; save him from killers (battle); his testimony | 2 years; he dies → steal the sect treasury ledger (battle, risk of bounty) |
| A4 Sharpening | 5-10 years later: sparring, a new sword (gift), her breakthrough | — |
| A5 Return to the sect | Before the elders: evidence or duel; battle with `{M}`; mercy question | 2 years |

Endings: **Companion** (won, intimacy ≥ 500), **Sect Master** (won, < 500), **Peace** (let go, ≥ 300),
**Fallen** (let go < 300, or abandoned in "Broken Sword"; boss of the final battle), **Sacrifice** (player about to
die in A5; her sword), **Traitor** (she hunts the player every 3-5 years).

### Line B — "Old Huang's Debt" (Old Huang, town `{T}`)
| Act | Content | Time limit / fail branch |
|---|---|---|
| B0 Bargain | A cheap item; masked men claim it (return / fight / run to him) | Return → line paused 2 years |
| B1 Confession | Find him hiding; his Red Lotus loan; vouch, lend, or refuse | 2 years |
| B2 Last caravan | Escort his caravan to `{T2}` (ambush battle); share of the profit | 1 year; late / robbed → **"Vanished Debtor"**: debt doubled, find him in a far town |
| B3 Red ledger | A Red Lotus envoy; half the merchants owe her; steal the ledger (battle), pay, or stall | 2 years |
| B4 Siege | Red banners over `{T}`; his secret savings; defend (town war), tribute, or stay out | Next town war, at most 3 years |
| B5 Aftermath | A drink, a gift, the ending | — |

Endings: **Merchant Prince** (defended: market discount, yearly goods), **The Heir** (he dies of old age: his
listings and a good buy order), **Fallen Town** (lost / stayed out: demonic town, tax ×2, hostile guards),
**Betrayed** (vouched and he fled: the player owes; bounty on him), **Benefactor** (lent and repaid: he clears a
bounty on the player once).

### Line C — "Exploding Furnace" (Su Xiaoman, grandmaster `{G}`; comic)
| Act | Content | Time limit / fail branch |
|---|---|---|
| C0 Test subject | A furnace explodes; test her pill (lavender hair for 3 months); a real pill as an apology | Refused twice → line closed |
| C1 Moonfrost Lotus | A valley map; a giant snake (battle); Granny Shi appears (opens line E) | 2 years; late → **"Failed Exam"**: she goes alone and comes back hurt; nurse her (+1 year) or "Last Explosion" |
| C2 The exam | Furnace fee; three fire-control choices (2 of 3 right = pass) | Fail → again next spring |
| C3 Stolen recipe | `{G}` stole her recipe; two apprentices' favors as proof; expose, duel, or let go | 1 year; let go → "Last Explosion" |
| C4 Grand Gathering | `{G}`'s challenge; a saboteur at night (battle); the contest | — |

Endings: **Master Alchemist** (her pills -30 %), **Companion** (intimacy ≥ 300: joins the team without pay),
**Last Explosion** (a teahouse; food that restores mood now and then).

### Line D — "Sworn Brothers" (Iron Ox, Murong Ge, `{L}`; comic; love triangle kept)
| Act | Content | Time limit / fail branch |
|---|---|---|
| D0 Eleven bowls | Ox can't pay the inn; pay for him or fight him; the poet's verse | — |
| D1 Beast hunt | Hunt with both (allies); campfire talk; swear brotherhood (intimacy ≥ 200 with both) | 1 year |
| D2 Forty-two poems | A gift for `{L}`; help, sabotage, or court her yourself (triangle) | 1 year |
| D3 Murong's blood debt | His family burned by the Red Lotus; track a killer to a ruined shrine (battle) | 2 years |
| D4 Two battle lines | A town war with Ox on the other side: talk, fight, or switch sides | When such a town war comes |

Endings: **Three Sworn** (both help in battles; they put a bounty on the player's killer), **Murong as Companion**
(female player or same-sex option; Romantic → Faithful), **A Brother Lost** (Ox dies: "Ox Fist" technique),
**Rivals** (the player won `{L}`: Ge fights on the enemy side of town wars).

### Line E — "Granny's Herb Garden" (Granny Shi, A Li; earnest and warm)
| Act | Content | Time limit / fail branch |
|---|---|---|
| E0 Mountain garden | Keep her garden; monkeys stealing peaches (fun battle); a lesson a year (+1 basis, at most 5) | — |
| E1 Child in the ashes | A Li, from a village burned under red banners; adopt, send to a sect, or leave with Granny | 1 year |
| E2 A Li grows up | Moments at 8, 12 and 15 (visit within a year each) shape her: alchemist / sword / free | All 3 missed → "Lost Child" |
| E3 Granny falls ill | ≤ 3 years of life left: a longevity herb (encounter + battle, or market); she accepts (+10 years) or declines (last lesson, +3 basis) | — |
| E4 Last wish | She dies under the pear tree; a seed; keep the garden (manual seclusion with intimacy ≥ 300) or give it to A Li | — |

Endings: **Successor** (A Li becomes alchemist or swordswoman; preferred heir when continuing as a descendant),
**Lost Child** (A Li grows up under Lady Honglian), **Garden of Generations** (descendants get the first lesson).

### Final battle — "Red Lotus" (Lady Honglian)
- **Opens:** line A or B reached act 4, and at least 2 other lines ended.
- **Time limit:** 5 years. Late → **"Spreading Demon Realm"**: 2 more fallen towns a year, harder battle.

| Act | Content |
|---|---|
| F1 Demon realm | Towns raise red banners; her letter; gather allies or go alone |
| F2 Allies | A short drama with every ally of a good ending (A, H, N, C, Y, X); enemies of bad endings join her |
| F3 Lotus Pavilion | Guardians (battle); her offer (fight, or kneel: secret ending); the last battle |
| F4 Aftermath | Her towns are freed, her debt ledger burned, "Red Lotus Mirror" (unique item); farewells |

Secret ending **Kneel**: the whole world puts bounties on the player, who rules her towns.

### Links between lines
- Lady Honglian is behind A (the sold manual), B (the loans and the siege), D (Murong's family) and E (A Li's
  village).
- C leads to E through Granny Shi; `{L}` of D may be Yue Han (if line A has no Companion ending).
- Bad endings feed the final battle: Fallen Yue Han, Lost A Li, Rival Murong fight for her.
