# BattleEvent

`3161035078/ModProject/ModCode/ModMain/Mod/BattleEvent.cs` - cache key `ModConst.BATTLE_EVENT` - group: Battle

Battles are not closed rooms: cultivators near the player may join during the fight (enemies, friends, stalkers, sect members, town guards), team members join the player and may betray. Members who fought beside the player grow closer.

## Saved data

`AvgGrade` (private set, rebuilt every battle). Runtime only (`[JsonIgnore]`): `AroundUnits` (candidates), `TeamMembers` (not joined yet), `JointTeamMembers` (joined battle units).

## Switch

`mod_configs.json` > `BattleConfigs` (rates in % **per second**):

| Key | Value |
|---|---|
| `JoinRange` | 6 (map cells around the player) |
| `RandomNpcJoinRate` | 0.10 (enemy roll and friend roll, separate) |
| `SectNpcJoinRate` / `TownGuardNpcJoinRate` | 1.00 / 3.00 |
| `TeamMemberJoinRate` | 5.00 per member |
| `TeamMemberBetrayRate` | 0.20 per joined member |
| `StalkerJoinRate` | 1.00 |
| `FriendlyIntim` / `EnemyIntim` | 160 / -160 |
| `DifferenceRighteous` | 80 |

## Which battles

`IsJoinableBattle()` is false for: town wars and monster waves ([MapBuildBattleEvent](MapBuildBattleEvent.md)), the sneaky-catch dungeon, the realm dungeons 3901-3911, sect position 4601, Mind Demon 5016850, arena (24), challenge, spar, trial of lightning, bamboo challenge, sect events 120-125, the new game battle 15, the three-legged crow battle. The battle must also be a real battle (`isRealBattle`).

## Flow

| Hook | What happens |
|---|---|
| `OnBattleStart` | `AvgGrade = player grade - 1`. Joinable: `AroundUnits` = units within 6 that pass `CondJoinBattle` and are friendly, enemy, a town guardian, a member of the battle's sect, or a stalker; `TeamMembers` = the player's living team members ([HirePeopleEvent](HirePeopleEvent.md)). |
| `OnTimeUpdate1000ms` (battle started, player alive and visible) | The rolls below. |
| `OnBattleEndHandler` | Every joined member alive: `TeamBond` (+20 intimacy both ways on a win, +10 on a loss; log `log_team_friend` when it becomes a friend). Every team member who is a friend: 20 % (`ARGUE_RATE`) to gain 5 hate. |

`CondJoinBattle`: alive, grade >= `AvgGrade`, HP > 70 %, MP > 50 %, SP > 30 %.

Each join brings the unit's **whole team**, placed at one of 4 points 16 away from the room center, and raises `AvgGrade = (AvgGrade + grade) / 2 + 0.1` (later joiners must be stronger).

### Rolls each second

| Roll | Who | Side | Drama |
|---|---|---|---|
| `StalkerJoinRate` | first stalker around; its team leaves `Stalkers` | enemy | by reason: `battleevent480110600` (drop), `601` (market, stones), `602` (market, items); options fight / pay |
| `RandomNpcJoinRate` | first enemy (`IsEnemyUnit >= 0`) | enemy | type 2 (alignment): `480110501` righteous / `480110502` evil; else `480110100` |
| `RandomNpcJoinRate` | first friend (`IsFriendlyUnit >= 0`) | player | `480110200` |
| `SectNpcJoinRate` (battle on a sect: `ModBattleEvent.School`) | sect member | player if friendly / same sect, else enemy | `480110300` |
| `TownGuardNpcJoinRate` (battle in a town: `ModBattleEvent.Town`) | town guardian | same rule | `480110400` |
| `TeamMemberJoinRate` per member | team member | player | - |
| `TeamMemberBetrayRate` per joined member | if `IsBetrayUnit >= 0` | switches to enemy, dismissed from the team, +200 hate both ways | type 2: righteous / evil drama, else `480110500` |

Drama texts use `ModTextHelper.RandomLS` (random variants of a key).

### Relations

| Function | Result |
|---|---|
| `IsFriendlyUnit` | 1: inner trait Selfless, or intimacy >= 160, or sect intimacy >= 160 either way; 2: same sect; -1 otherwise |
| `IsEnemyUnit` | 1: intimacy <= -160 (or sect intimacy); 0: inner trait Wicked / Selfish / Evil; 2: opposite alignment with stand value apart > 80; -1 |
| `IsBetrayUnit` | 0: hater; 1: inner Selfish / Evil or outer Power-hungry; 2: opposite alignment > 80; -1: loyal |

### Paying off a stalker

`StalkerFee = max(CalculateInflationary(500 x grade), player money / 10)`. Option 2 of the drama: enough money -> the money goes to the stalker, its joined units are removed from the battle (no kill rewards), log `log_stalker_paid`, drama `battleevent480110612`; not enough -> `battleevent480110613` and the fight goes on.

## Notes

- `ModBattleEvent.School` / `Town` are the sect / town at the player's map position when the battle starts. Until 2026-11-19 these two rolls also required `DungeonHelper.IsArena()` (dungeon 24), which `IsJoinableBattle()` excludes, so they never happened.
- `NPCJoin` skips units already in the battle.

## Tests

TB-07.
