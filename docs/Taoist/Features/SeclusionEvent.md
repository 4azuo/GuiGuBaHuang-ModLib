# SeclusionEvent

`3161035078/ModProject/ModCode/ModMain/Mod/SeclusionEvent.cs` - cache key `ModConst.SECLUSION_EVENT` - group: World

Seclusion / enlightenment: years pass in a few minutes through the time skip of [TimeSkipEvent](TimeSkipEvent.md) (the world keeps running: NPCs grow, marry, die; inflation; town wars...). Some are offered (the player accepts or declines), some are forced. A summary drama ends each one.

## Saved data

| Property | Meaning |
|---|---|
| `LastMonth` | kind -> total month it was last offered (cooldown) |
| `LastForcedMonth` | total month of the last forced one |
| `Count` | seclusions started |
| `Current` | the running one (`Running`: Kind, Months, StartMonth, StartExp, StartLogIndex, Friends, StartPrice, Roll) |

## Switches (S&M)

`NoSeclusion` (none), `NoForcedSeclusion`, `SeclusionRate` (default 0.5 % a month).

## Kinds

| Kind | Type | Years | Trigger | Reward (for the months actually passed) |
|---|---|---|---|---|
| Spirit vein | offered | 3-8 | `SeclusionRate` %, 85 % of offers split 50/50 with Old tree | exp `YearExp x years` |
| Ancient inheritance | offered | 10-30 | 15 % of the offers | exp `YearExp x years x 1.5`, Talent `+2 + years / 10`, a pill / herb of grade + 1 |
| Old tree | offered | 5 | see spirit vein | a random basis (by `Roll`) `+2..4` |
| Heart demon | forced | 2-5 | `SeclusionRate x 0.3` %, grade >= 3 | 50 %: Talent +3, Luck +1; else lose 20 % of the exp gained in the phase |
| Time rift | forced | 10-20 | `SeclusionRate x 0.1` % | exp `YearExp x years`, an item of grade + 1 |
| Garden | manual | 1-20 | Granny Shi's garden (storyline E, `StoryEvent`) | exp `YearExp x years x 0.75` |

`YearExp` = a tenth of the exp of the current phase, at least 50.

## Flow

| Hook | What happens |
|---|---|
| `OnMonthly` (in game, not in battle) | Nothing if disabled, already secluded or skipping. **Forced** (not `NoForcedSeclusion`, 180 months since the last): heart demon, then time rift rolls. **Offered**: `SeclusionRate` roll, pick the kind, skip if offered less than 120 months ago. |
| `OnLoadGame` | a save made during a seclusion: `End()` 3 s later (rewards of the months passed). |

`Offer(kind)`: drama with the length and the lifespan left (`secl_{kind}`), red warning `secl_warn_life` if the seclusion is as long as the lifespan; options accept (`secl_o1`) / decline (log `log_secl_declined`).

`Forced(kind)`: `ForcedMonths = min(years x 12, life left - 12)`; below 12 months it does not happen (a forced seclusion never ends the player's life). A drama, then it starts.

`Start(kind, months)`: records `Current` (start month, exp, log index, friends, price of 10,000 stones, a roll), log `log_secl_start`, `TimeSkipEvent.SkipTime(months, End)`.

`End()`: if interrupted (a drama / battle during the skip), the text `secl_interrupted` and the rewards of the months passed only; then the summary drama `secl_summary`: years, exp gained, price change %, friends who died, births in the family (log entries `log_family_birth`), age, lifespan left. Log `log_secl_end`.

## While secluded

`IsSecluded` (`Current != null`) is read by other features: no encounters, no loan penalty ([BankLoanEvent](BankLoanEvent.md)), no bounty hunters after the player ([WantedEvent](WantedEvent.md)).

## Tests

TX-08.
