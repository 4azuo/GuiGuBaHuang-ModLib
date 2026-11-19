# WantedEvent

`3161035078/ModProject/ModCode/ModMain/Mod/WantedEvent.cs` - cache key `ModConst.WANTED_EVENT` - group: Economy

Bounties (wanted notices) paid from a town budget. Town masters (the player, or NPC masters on units they hate) post them; bounty hunters go after the targets every month; the player can hunt too and can be hunted.

## Saved data

| Property | Meaning |
|---|---|
| `Bounties` | `List<Bounty>`: Id, TownId, TargetId, PosterId, Reward, PostedMonth, Attempts |

Runtime: `HuntBattleBountyId` (the ambush battle running), `IsOpenWantedList`.

## Switch

`NoBounty` (S&M): everything off.

## Constants

| Name | Value |
|---|---|
| `EXPIRE_MONTHS` | 36 |
| `HUNT_RATE` | 15 % |
| `NPC_POST_RATE` | 20 % a year per NPC town master |
| `CLEAR_NAME_RATE` | 1.5 |
| `NPC_POST_HATE` | 200 (intimacy <= -200) |

`MinReward(target) = CalculateInflationary(1,000 x grade^2)`.

## Posting (`Post(town, poster, target, reward)`)

Refused (LocalText key): `wanted_err_off`, `wanted_err_target` (no / dead target, or the poster itself), `wanted_err_already` (same town, same target), `wanted_err_min`, `wanted_err_budget`. Otherwise the reward leaves the town budget at once, log `log_wanted_post`; a bounty on the player opens the drama `wanted_on_player`.

- **The player** (master of at least one town): NPC info -> "Post a bounty" (`wanted_post`) -> window: target, town (the player's first town), budget and minimum, reward slider (min .. budget), rule text -> post.
- **NPC masters** (`OnYearly`): 20 % a year each, on the unit they hate most (intimacy <= -200, the player included), reward `max(min, budget / 10)`.

## Monthly (`OnMonthly`)

For each bounty:

1. 36 months old -> refunded to the town (`log_wanted_expired`).
2. Target dead / gone -> refunded (`log_wanted_dead`).
3. Target is the secluded player -> skipped.
4. Else a hunt with chance `ChanceOver(15 % x min(3, reward / min reward), MonthsPassed)`.

`FindHunter`: a living NPC, not the target, not the poster, not in the target's team; the weakest with grade >= target grade (random among equals), else the strongest.

`Hunt`:

- **NPC target**: win chance `100 x hunterPower / (hunterPower + targetPower)` (`UnitPowerCalHelper.CalWUnitBattlePower`). Win: the target dies, the hunter gets the reward (`Claim`, `log_wanted_claimed`; message `wanted_done` if the player posted it). Loss: the target hates the hunter +100, `log_wanted_failed`.
- **The player** (`Ambush`, not in battle / time skip): the hunter is moved to the player, log `log_wanted_ambush`, drama `wanted_ambush` with options: fight, or pay `ceil(reward x 1.5)` to the town to clear the name (refund to the town, `log_wanted_cleared`). Not enough money -> fight. The fight is a sneaky-catch dungeon battle; lost -> the hunter claims the reward.

## Player as hunter

`OnBattleUnitDie`: the player kills a world unit with bounties -> every bounty on it is paid to the player (`log_wanted_player_claimed`).

Bounty board (`OnOpenUIEnd(TownBounty)`): button "Wanted list" (`wanted_list`) -> the game's NPC search window with the portraits of the living targets (`wanted_list_desc` / `wanted_none`). Clicking one opens its NPC info with only the first tab and the bounties on it (`wanted_detail`: reward, town, poster, months left); the player's own bounties have a cancel button (refund, `log_wanted_cancel`).

## Related

- [FamilyEvent](FamilyEvent.md): relatives who are town masters post a bounty on the player who kills a family member.
- [SeclusionEvent](SeclusionEvent.md): no hunters during a seclusion.

## Tests

TG-59.
