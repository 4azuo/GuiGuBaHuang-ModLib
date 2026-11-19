# SMScoreEvent + SMScoreRecordsEvent

`3161035078/ModProject/ModCode/ModMain/Mod/SMScoreEvent.cs` - `SMScoreEvent` (per save, cache key `ModConst.SM_SCORE_EVENT`) and `SMScoreRecordsEvent` (global, `ModConst.SM_SCORE_RECORDS_EVENT`) - group: Battle / S&M

What the S&M score **P** of a save does. P = total score of the S&M settings when the save was created (`SMGlobalConfigsEvent.Score`, copied with the configs into [SMLocalConfigsEvent](SMLocalConfigsEvent.md)): harder settings give more, easier ones less.

## Saved data

`SMScoreEvent` (per save):

| Property | Meaning |
|---|---|
| `StartBonusPending` | true on a new save until the start bonus is given |
| `BestGrade` | best grade reached by the player (for the records) |

`SMScoreRecordsEvent` (global): `Records` (max 50, best P first): UnitId, Name, P, BestGrade, Years, Generation, Updated.

## Effects

| # | Effect | Rule |
|---|---|---|
| 1 | Battle rewards | `RewardPercent = clamp(P / 2,000, -20, +50)` %; battle exp (`ScaleExp`) and the counts of every dropped item and spirit stones of a won battle (`ScaleLoot`, at least 1 each) x `1 + RewardPercent / 100`. Used by [BattleRewardEventV2](BattleRewardEventV2.md). |
| 2 | Title | tier by P: < 0 Leisurely Wanderer, 0 Mortal Path, 10k Seeker of the Dao, 30k Ascetic, 60k Asura, 100k Heaven Defier (`sm_title_0..5`, icons xinqing, xingyun, wuxing, daodian, zhandouxiaoguo, daoli); `TitleLine` = icon + title + P + reward %, shown in the player info ([UIEvent](UIEvent.md)). |
| 3 | Rare encounter | "Heavenly trial" possible from tier 3 (Ascetic, 30k P) ([EncounterEvent](EncounterEvent.md)). |
| 4 | Start bonus | new save, P > 0: `min(10, P / 10,000)` ability points and `min(20,000, P / 5)` spirit stones, once; log `log_score_start`. |
| 5 | Descendant | continuing as a descendant with P < 0 keeps `InheritShare` of the spirit stones: 1 at 0 P, linear down to 0.5 at -20,000 P ([FamilyEvent](FamilyEvent.md)). |

`Scale(count, m, roll)`: `floor(count x m)` + 1 when `roll < fraction` (exp uses roll 0.5, loot a random roll).

## Flow

| Hook | What happens |
|---|---|
| `OnLoadClass(isNew)` | new save: `StartBonusPending = true` |
| `OnLoadGame`, `OnMonthly` | `GiveStartBonus()` (once), `UpdateRecord()` |

`UpdateRecord`: `BestGrade = max(BestGrade, grade)`; upsert the player's record (P, best grade, game year, generation from FamilyEvent, date); saved globally at once when something changed; above 50 records the best 50 are kept.

## Records window (`OpenRecords`, Taoist Settings > Score records)

`UICustom1` `sm_records_title`: the current save's title line (if in game), description, then up to 18 records ranked by P then best grade (`sm_records_line`: rank, name, title, P, best realm, years, generation, date), or `sm_records_none`.

## Tests

TG-60.
