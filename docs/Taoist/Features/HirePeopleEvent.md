# HirePeopleEvent

`3161035078/ModProject/ModCode/ModMain/Mod/HirePeopleEvent.cs` - cache key `ModConst.HIRE_PEOPLE_EVENT` - group: World and NPCs

Teams: the player and NPCs can hire NPCs. Members follow their master and are paid every month (friends serve for free); unpaid members leave. NPC friends team up by themselves. Teams also matter in battles ([BattleEvent](BattleEvent.md): members join and may betray; whole teams join or stalk together).

## Saved data

| Property | Meaning |
|---|---|
| `TeamData` | `Dictionary<masterId, List<unitId>>`: the members of each team (the master first) |

Membership is also marked by the luck `TEAM_LUCK_ID` 420041121 on every member and master (`IsHired` = has this luck). Static UI flags: `isShowHirePeopleUI`, `isShowManageTeamUI`.

## Switch

`mod_configs.json` > `BattleConfigs.DifferenceRighteous` (80, team-up rule 3).

## Constants

| Name | Value |
|---|---|
| `FRIEND_INTIM` | 180 (a friend: intimacy toward the master >= 180) |
| `FRIEND_JOIN_RATE` | 10 % a month |
| `MONTHLY_PAYMENT_RATIO` | 10 (monthly pay = hiring price / 10) |

## Prices

`k` = current team size + 1 (1 when no team).

```
hiring price = (2^grade x 1000 x k + AbilityPoint x grade) x (2 if hero)
monthly pay  = hiring price / 10                      (0 for friends)
reputation   = 2^k x 100 + 2^grade x 10               (required)
```

## UI

| UI | What is added |
|---|---|
| Bounty board (`TownBounty`) | "Hire people" (`team420041131`) -> NPC search window with the hirable units within 4 (not hired, not town guardians, grade < player grade + 2) |
| NPC info of a hired unit | monthly pay (`other500020043`, own team, not a friend), team name (`ui_team_info`), "Dismiss" (own team or manage window) with confirmation |
| NPC info of a free unit (not a town guardian) | price and monthly pay (`ui_team_hire_cost`) and reputation (`ui_reputations`); for a friend: "free" (`team420041132`). Button "Hire" (`team420041133`): refused if the player is a member of another team (`team420041129`), lacks money (non-friends) or reputation; confirmation; pays (non-friends) |
| Player info (in a team) | "Disband team" (`team420041126`, master only), "Manage team" (`team420041127`: NPC search with the members, team name, total monthly pay), team name |
| Sky tip of the team luck | replaced by the team name |

The search windows set the flags; closing `NPCSearch` resets them.

## Monthly (`OnMonthlyForEachWUnit`)

1. **Team up** (NPC not hired): for every unit it likes (intimacy >= 180), chance `ChanceOver(10 %, MonthsPassed)`, if: (1) the other is a friend, or (2) intimacy >= 30 % of 180 and same sect, or (3) `BattleEvent.IsFriendlyUnit` and same alignment with stand values apart < 80. The other becomes its master: the player gets a drama (`420041123`, option `420041124` = accept); an NPC accepts at once.
2. **Payment** (unit is a master): dead master -> team disbanded. For each member: gone -> removed; dead -> removed and luck removed; not a friend -> pay `monthly pay x MonthsPassed`; not enough money -> dismissed and +50 hate; paid -> the member is moved to the master. Team empty -> removed.

## API

`Hire(master, member)` (no duplicates, BUG-06), `Dismiss`, `DismissTeam`, `IsHired`, `IsTeamMaster`, `IsTeam`, `IsFriend(a, b, r)`, `GetTeamData`, `GetTeamDetailData` (master, members; the unit alone when not in a team), `GetTeamMember`, `GetTeamMaster`, `GetTeamInfoStr` (master's name), `GetTotalMonthlyPayment`.

## Related

- [BattleEvent](BattleEvent.md): members join battles (5 %/s), betray (0.2 %/s), grow closer by fighting together.
- [BattleRewardEventV2](BattleRewardEventV2.md): members may take loot; 10 % quit after a lost battle.

## Tests

TG-09, TM-10.
