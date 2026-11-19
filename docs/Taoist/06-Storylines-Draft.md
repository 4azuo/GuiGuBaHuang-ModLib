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

## 4. New lines G-L (approved 2026-11-19; coded in `ModMain/Story/Lines/LineG..L_*.cs`, tests TS-07..TS-12)

Lines A-F all open early and end within 5-15 years, so nothing waits for the high realms. Six new lines, in three
pairs: two **lifelong lines** open at the start and last ~100 years (acts decades apart), two open at a
**middle realm**, two at a **high realm** near the end of the game. Same rules as section 1 (casting, time limits
with fail branches, modest rewards, Taoist log, S&M "No NPC storylines"). Realms (grade): 1 Qi Refining,
2 Foundation, 3 Qi Condensation, 4 Golden Core, 5 Origin Spirit, 6 Nascent Soul, 7 Soul Formation,
8 Enlightenment, 9 Reborn, 10 Transcendent.

Pacing of the lifelong lines: an act opens at the **later** of its year (years since the line opened) and its
realm, so a slow player meets the acts later, never skips one; a time skip / seclusion goes through them like any
act. When a cast NPC dies of old age before the end, the line jumps to its "grave" ending.

### Characters

| # | Working name | Sex | Casting rules | Voice |
|---|---|---|---|---|
| 9 | **Lan Qiu** (the lifelong rival) `{W}` | any (opposite of `{V}`'s choice: none) | ≤ 10 % of life lived; same grade as the player; outer Glory Hound or Vengeful; best basis = the player's best basis | Proud, quick to bet, never admits defeat; softens over a century |
| 10 | **Mei Niang** (innkeeper) `{K}` | F | mortal-like: grade 1, ≤ 15 % of life lived; inner Kind; outer Family-Oriented; lives in the player's first town | Warm, practical, scolds the player like family |
| — | her descendants `{K2}` `{K3}` | — | children records, NPCs at 16 (as A Li, line E) | Each generation its own voice |
| 11 | **Elder Gu** (tomb seeker) `{Q}` | M | ≥ 60 % of life lived; grade = player's; outer Power-hungry; any sect | Polite, scholarly, cold underneath |
| 12 | **Xiao Ye** (the cursed boy) `{J}` | M | created: grade 1, 12-14 years (a child record until 16, then an NPC) | Silent, then fierce; mirrors how he is raised |
| 13 | **The Thunder Remnant** `{U}` | M | no NPC: a voice / background (the soul of an immortal who failed his ascension 30,000 years ago) | Ancient, weary, half-mad, grandiose |
| 14 | **The Heart Demon** `{Z}` | = player | a copy of the player (looks, grade, arts) created for the last act only | The player's own words, twisted |

### Line G — "The Hundred-Year Wager" (Lan Qiu; lifelong, ~100 years)
Opens: player grade 1-2, the first months / years of a new game (1 % a month).

| Act | When (year / realm, the later) | Content | Fail branch |
|---|---|---|---|
| G0 The wager | start | A brawl at a sect gate; Lan Qiu bets who reaches Golden Core first (stakes: a spirit stone pouch, a name in the other's log) | Refuse the bet → he bets anyway |
| G1 First rematch | 15 y / grade 3 | Duel (real battle, his grade = yours); winner keeps the pouch; a shared enemy interrupts | — |
| G2 A life owed | 30 y / grade 4 | One of you falls into a beast lair (whoever is weaker): save him (battle) or be saved by him | Ignore → he survives, scarred, "Eternal Rival" path |
| G3 His sect burns | 45 y / grade 5 | His sect / town is attacked; he asks for help for the first time: fight, shelter him, or refuse | 2 years; refuse → he swears revenge on the attackers alone, grade -1 |
| G4 Two old men | 70 y / grade 6 | Tea on a mountain; the score of 70 years (wins / losses counted by the line); last duel or a shared disciple | — |
| G5 The last wager | 100 y / grade 7, or when his life is near its end | His tribulation / his death bed: help him through (battle in the lightning trial), or let him go; the pouch returned | — |

Endings: **Lifelong Friend** (more help than duels: he joins the player's battles, his sect is friendly),
**Eternal Rival** (more duels won by him: a duel every 10 years, a technique scroll for each win of the player),
**Surpassed** (he reached a realm before the player at every wager: he leaves a letter and a pill to break through),
**The Grave** (he died before G5: his sword and his disciple, who calls the player "uncle").

### Line H — "The Roadside Inn" (Mei Niang and her line; lifelong, ~100 years, warm)
Opens: the player's first town with an inn (`TownPub`), the first 5 years (1 % each visit).

| Act | When (year / realm) | Content | Fail branch |
|---|---|---|---|
| H0 A leaking roof | start | A young widow opens an inn with a loan; lend her spirit stones, chase off the loan shark (battle), or eat and leave | Leave → she opens it anyway, smaller |
| H1 The wedding | 10 y | She marries a caravan guard; a gift, a toast (drama with both) | — |
| H2 The fire | 25 y | Bandits burn the inn: hunt them (battle), pay the rebuilding, or comfort and move on | 2 years; nothing done → the inn is gone ("Ashes") |
| H3 The son who wants to fly | 40 y | Her son `{K2}` (now 16) wants to cultivate: take him as a disciple, send him to a sect, or talk him out of it | — |
| H4 Mei Niang grows old | 60 y, or when her life ≤ 5 years | She asks the player to keep coming; last meal; she dies (drama of farewell) | — |
| H5 The hundredth year | 100 y | The grandchild `{K3}` runs the inn (or `{K2}`'s sect hall); a feast with the people of the player's life (allies of A-G still alive) | — |

Endings: **Inn of Ages** (the inn stays: resting there restores mood / HP, news of the world every year, a yearly
meal), **Disciple Lineage** (`{K2}` became the player's disciple: preferred heir like A Li), **Ashes** (a grave and a
memory in the log).

### Line I — "The Sealed Tomb" (Elder Gu; opens at Golden Core, grade ≥ 4)
Opens: grade 4+, outside a town (0.5 % a month).

| Act | Content | Time limit / fail branch |
|---|---|---|
| I0 A map in a corpse | A dead cultivator holds half a map; Elder Gu holds the other half: partner or rival | — |
| I1 The team | Gather 2 companions (hired team members count) or go with Gu alone | 1 year |
| I2 The outer halls | Puppet guardians (battle), a riddle of three doors (choices), a poisoned treasure | 2 years; late → another sect opens it first: "Looted Tomb" |
| I3 The knife in the back | Gu betrays the team at the inner gate (battle with Gu and his men) | — |
| I4 The tomb owner | The soul of the owner: take his legacy, free him, or seal him again | — |

Endings: **Heir of the Tomb** (+5 basis of the player's best weapon, Gu's sect hostile), **Freed Soul** (a yearly
counsel: a pill or a rare herb of the player's grade), **Sealer** (the righteous sects' gratitude: contribution and
reputation), **Looted Tomb** (only the outer treasure; Gu's bounty on the player).

### Line J — "The Cursed Disciple" (Xiao Ye; opens at Origin Spirit, grade ≥ 5)
Opens: grade 5+, in a town (0.5 % a month).

| Act | Content | Time limit / fail branch |
|---|---|---|
| J0 The execution ground | A boy with a demon mark about to be executed by a sect: save him (battle), buy him, or watch | Watch → line closed |
| J1 First lessons | Teach him (3 moments over 5 years, choices shape him: mercy / strength / freedom) | Missed moments → he runs away (J3 earlier) |
| J2 The mark wakes | The curse flares in a town: suppress it (pill / battle with him), or let it burn out | 1 year |
| J3 The hunters | The sect that marked him comes back with elders (battle); the truth: the mark is the sect master's own sin | 2 years |
| J4 The heart of the boy | His breakthrough: the demon offers him power; the player's lessons decide | — |
| J5 The student surpasses | Final duel with him (real battle) or side by side against the sect master | — |

Endings: **Righteous Disciple** (joins the team for free, preferred heir option), **Demon Disciple** (he leaves to
the demonic path: a recurring enemy, boss of a town war), **Sealed** (the player seals the mark and his
cultivation: he becomes a mortal innkeeper - links to line H if open).

### Line K — "The Thunder Remnant" (opens at Soul Formation, grade ≥ 7)
Opens: grade 7+, during a breakthrough attempt or a lightning trial (`RealTrialEvent`), once.

| Act | Content | Time limit / fail branch |
|---|---|---|
| K0 A voice in the thunder | During the tribulation a voice speaks; after it, a scar of lightning on the player's arm | — |
| K1 The broken platform | Find his ascension platform (a far map point); puppets of lightning (battle) | 3 years |
| K2 His descendants | A sect that worships him wants the player's scar (battle or bargain) | 2 years |
| K3 The truth | He failed because he betrayed his dao companion; he wants the player's body for a second try, or only a witness | — |
| K4 The second tribulation | The trial of lightning with his remnant: guide (he lends power), fight him inside it, or refuse him | — |

Endings: **Guided** (the next breakthrough is easier: tribulation damage -30 %, once), **Devourer Defeated** (his
remnant destroyed: a lightning technique bonus, +thunder resistance), **Second Chance** (the player lets him be
reborn as an NPC: a powerful friend - or, by a hidden choice, an enemy).

### Line L — "The Heart Demon" (opens at Enlightenment, grade ≥ 8; the last line)
Opens: grade 8+, at the first seclusion / time skip of 5+ years at that realm (or 1 % a month).

| Act | Content | Time limit / fail branch |
|---|---|---|
| L0 Dreams | Dreams of the people of the player's lines (endings read from the story log: Yue Han, Old Huang, Lan Qiu, Mei Niang...) | — |
| L1 The mirror lake | The demon shows the choices the player regrets (3 past choices from the log); answer each | — |
| L2 The ghosts | Fight the shadows of the enemies of bad endings (Traitor Yue Han, Demon Disciple, Lady Honglian if Kneel) | — |
| L3 The copy | `{Z}`, a copy of the player, fights the player (real battle, same stats) | Lost → retry after 1 year, exp loss |
| L4 Clear heart | Accept, deny or embrace the demon | — |

Endings: **Clear Heart** (a permanent bonus to breakthrough and a log epilogue of every line), **Scarred Heart**
(the demon returns every 20 years), **Embraced** (the player's alignment turns demonic; huge power, the righteous
world hostile).

### Links (G-L)
- G and H give the long thread of a whole life; L reads every ending of A-K for its dreams and ghosts.
- J may end in H (Sealed Xiao Ye becomes an innkeeper); K uses the trial of lightning (`RealTrialEvent`,
  dungeon 110081) like TB-08; G5 can use it too.
- Open questions: the realm gates (4 / 5 / 7 / 8) and the ~100-year span of G and H; whether the cast of G is the
  same sex as the player or any; the rewards of the high lines (Guided, Clear Heart) are the only non-modest ones.

## 5. Links between the lines (approved 2026-11-19; coded)

The lines can change each other: every line keeps its outcome as a flag of the save (`StoryEvent`: line id -> ending
key, e.g. `G:rival_friend`, `H:inn_burned`), and an act of another line checks the flag in its conditions (an option
shown or not, another NPC, another reward). Ideas:
- G (rival) -> K (thunder remnant): a rival who became a friend in G comes to help in K's battle; an enemy rival
  joins the remnant instead.
- H (roadside inn) -> J (cursed disciple): the inn of H is where the cursed disciple hides; an inn kept alive gives
  a peaceful option in J, a burned inn only the fight.
- I (sealed tomb) -> L (heart demon): what was taken from the tomb in I comes back as the heart demon's temptation.
- A-F: their endings (already logged) can open a short epilogue line in G-L (a cameo, a letter, a gift).
A flag is only read, never required: every line can still be played alone.

Coded (2026-11-19): `StoryLine.EndingOf(line)`; G -> K (`K4_friend` / `K4_rival`: Lan Qiu helps, or fights for the
remnant), H -> J (`LineH_RoadsideInn.InnStands`: hide Xiao Ye at the inn in J2, his hunters find him there in J3,
the "Sealed" ending makes him an innkeeper there), I -> L (`L1_tomb`: the tomb legacy as the first regret),
A-K -> L (`LineL_HeartDemon.Dreams`: the endings of every other line in the dreams and the clear-heart ending),
A-E, G -> H5 (the guests of the hundredth-year feast). Acts of the long lines open at the later of their year and
realm (`StoryLine.Gate`, tests: `StoryEvent.NoGates`).
