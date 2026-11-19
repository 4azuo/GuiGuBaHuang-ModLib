# FamilyEvent

`3161035078/ModProject/ModCode/ModMain/Mod/FamilyEvent.cs` - cache key `ModConst.FAMILY_EVENT` - group: World

Families: NPCs (and the player) marry and have children; a child becomes a grade-1 NPC at 16. A family tree on the game's kinship screen. Relatives who are town masters put a bounty on the player who kills a family member. When the player dies of old age (or of one-life), the player can continue as a grown-up child or grandchild.

The money side of the household is [FamilyFundEvent](FamilyFundEvent.md).

## Saved data

| Property | Meaning |
|---|---|
| `Children` | `List<Child>`: Id, FatherId, MotherId, FatherName, MotherName, BirthMonth, UnitId (null until grown, "-" when creation failed) |
| `Marriages` | marriages made by the mod |
| `InheritDeclinedMonth` | month the player last refused to pass on (-1 none) |
| `Generation` | generation of the player (1 at start) |

## Switches (S&M)

`NoFamily` (all off), `AllowSameSexCouple`, `ChildSurname` (0 father, 1 mother, 2 random), `ContinueAsDescendant`.

## Constants

| Name | Value |
|---|---|
| `ADULT_YEARS` | 16 |
| `MARRY_MIN_YEARS` / `MARRY_MIN_LIFE_LEFT_YEARS` | 18 / 20 |
| `MARRY_RATE` | 8 % a year at the peak |
| `BIRTH_RATE` | 6 % a year (young couple, grade 1) |
| `MAX_CHILDREN` | 4 per couple |
| `MAX_MARRIAGES_A_YEAR` / `MAX_BIRTHS_A_YEAR` | 30 / 20 |
| `MAX_PENDING` | 300 unborn records (no births above) |

## Formulas

```
MarryChance(age, life) = 0 if age < 18 or life - age < 20
                       = 8 x min(1, (age - 17) / 12) x clamp01(1.2 - 1.5 x age / life)          (% a year)

BirthChance(lived, avgGrade, born) = 0 if born >= 4 or lived >= 0.8
                       = 6 x clamp01(1.4 - 1.75 x lived) / (1 + 0.35 x (avgGrade - 1)) / (1 + born)
```

`lived` = the larger share of lifespan lived of the two parents.

## Flow

| Hook | What happens |
|---|---|
| `OnYearly` (x years passed) | `Marry()` then `Births()` |
| `OnMonthly` | children 16 years old -> `GrowUp`; not in battle: `CheckOldAge()` |
| `OnBattleUnitDie` | clan revenge (below) |

**Marry**: single NPCs in random order, each with `MarryChance`; `FindPartner`: same area, single, of age with 20+ years left, grade within 2, not of the same blood (the unit and its ancestors, 3 generations), opposite sex unless same-sex allowed; best by `intimacy + 50 if same alignment + Random(0, 30)`. `SetMarried`: the game's Married relation both ways; log `log_family_married` when the player or the player's family is involved.

**Births**: couples of opposite sex (NPCs and the player), each couple once: `BirthChance` roll -> `Birth`: a `Child` record; log `log_family_birth`; the player's child: drama `family_birth` (not during a skip / battle).

**GrowUp** (16 years later): `g.world.run.npc.AddUnit(1, 1)` (a grade-1 NPC); surname = first part of the father's / mother's recorded name (before a space, or the first character); age 16; Talent = parents' average +-10; alignment = parents' average; placed within 3 of a living parent; `LinkParents` (parent array, parents' children lists); the player's child: +200 intimacy toward the player and drama `family_grown`; log `log_family_grown`.

`IsFamilyOfPlayer(u)`: the player's companion or a grown child of the player. `Family(u)`: parents, spouse, children (game + mod records), unborn, siblings (game brothers + other children of the parents).

## Family tree (`OpenFamily(u)`)

Opened from the household accounts window ([FamilyFundEvent](FamilyFundEvent.md)). The game's `NPCInfoRelation` screen (it shows the relations set by the mod), plus a note bottom left with what only the mod knows: unborn children (`family_unborn`, years to adult) and siblings (`family_siblings`, `family_dead` for the dead).

## Clan revenge

The player kills a world unit (bounties on): every town whose NPC master is a parent, child, sibling or spouse of the dead posts a bounty on the player: `max(min reward, budget / 5)` ([WantedEvent](WantedEvent.md)); log `log_family_revenge`.

## Continue as a descendant

`CanInherit` = family on and `ContinueAsDescendant`. Heirs = living grown-up children and grandchildren of the player, strongest first (grade, then exp).

- **Old age** (`CheckOldAge`): 2 months of lifespan left, not skipping, not refused in the last 24 months -> `OfferInherit`.
- **One-life death**: [BattleRewardEventV2](BattleRewardEventV2.md) calls `OfferInherit(die)` before killing the player.

`OfferInherit`: drama `family_inherit` with up to 3 heirs (name, age, realm) and "No" (`family_inherit_no`: refusal recorded, `onDecline`). Then `AskSkills`: keep the old player's techniques or take the heir's. Then `Inherit(heir, keepSkills)`:

| Step | What happens |
|---|---|
| 0 | Bounties on the old player end (refunded, `log_wanted_heir`); bounties on the heir move to the player |
| 1 | Every writable base property of the heir (name, look, sex, age, lifespan, grade, exp, bases, talent, reputation...) is copied to the player unit, except unit links and acquired destinies (`addLuck`) |
| 2 | Heir's techniques (learned martial arts and slots) if chosen |
| 3 | Sect: the old player leaves; the player joins the heir's sect with the heir's post |
| 4 | Town master seats: the old player's end, the heir's go to the player |
| 5 | `InheritRelations`: the player takes the heir's parents, companion, children, siblings (+ the old player's other children as siblings), lovers, master, students; the old companion is single again; intimacies merged by `InheritIntim` (old friendships halved, enmities in full; an enmity on either side wins); every other unit's links to the heir point to the player |
| 6 | Child records: the old player becomes `ancestor{Generation}_{id}`; the heir's records become the player's |
| 7 | S&M score < 0: the player keeps `SMScoreEvent.InheritShare` of the spirit stones (`log_score_inherit`) |
| end | heir unit removed, model and portrait refreshed, `Generation + 1`, log `log_family_inherit`, drama `family_inherited`, then the game saves |

The player unit keeps its id, items, spirit stones, acquired destinies and Taoist loans ([BankLoanEvent](BankLoanEvent.md)).

## Tests

TX-08, TX-10.
