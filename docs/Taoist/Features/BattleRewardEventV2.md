# BattleRewardEventV2

`3161035078/ModProject/ModCode/ModMain/Mod/BattleRewardEventV2.cs` - cache key `ModConst.BATTLE_REWARD_EVENT_V2` - group: Battle

What battles give and take: the player's properties grow by damage thresholds while fighting, exp from damage after a win, loot shared with the team, and the penalties of a defeat or a death (exp, items, money, lifespan, reputation, one-life). Angel / Evil role kills grow the killer.

## Saved data

| Property | Meaning |
|---|---|
| `TraningValues` | per training property `X`: `XRatio`, `XFactor`, `XLimit` (current threshold) |

Static (not saved): `PendingOneLifeDeath`, `PendingLossMonth`.

## Switches (S&M)

`NoGrowupFromBattles`, `NoExpFromBattles`, `GrowUpSpeed` (scales the threshold steps), `Onelife`, `AutoSaveAfterLostInBattle`, `LostLifespanWhenDie`, `LostItemWhenDie`, `LostMoneyWhenDie`.

`battle_reward_configs.json` (`BattleRewardConfig`): `ExpPerDmgDealt` 0.001, `ExpPerDmgRecv` 0.01.

## Training by damage (`OnBattleUnitHit`)

Real battles only, unless `NoGrowupFromBattles`. On every hit, repeatedly: the first property (in table order) whose unlock condition holds and whose damage counter is above its threshold gains **+1**, then `factor x= ratio`, `limit += factor`.

Damage counters come from `ModBattleEvent` (ModLib): **Global** = summed over all battles, **Local** = this battle.

| Name | Property | Counter | Ratio | Start step (x GrowUpSpeed) | First threshold | Unlock |
|---|---|---|---|---|---|---|
| Atk1 / Atk3 / Atk5 / Atk7 / Atk9 | Attack | global damage dealt | 1.07 / 1.08 / 1.09 / 1.10 / 1.11 | 50 / 100 / 200 / 400 / 800 | 100 / 200 / 800 / 3,200 / 12,800 | grade >= 1 / 3 / 5 / 7 / 9 |
| Def1 / Def4 / Def8 | Defense | global damage received | 1.13 / 1.15 / 1.20 | 100 / 1,000 / 10,000 | same | grade >= 1 / 4 / 8 |
| HpMax, 1000P+ ... 48000P+HpMax | Max HP | global damage received | 1.015 | 10 | 10 | physical bases sum >= 0 / 1k / 3k / 6k / 12k / 24k / 48k |
| MpMax, 1000M+ ... 48000M+MpMax | Max MP | global magic damage dealt + received | 1.2 | 200 | 200 | magic bases sum >= 0 / 1k ... 48k |
| Blade, Spear, Sword, Fist, Palm, Finger | that basis | **local** dealt + received of that type | 1.05 | 125 | 1,100 | - |
| Fire, Froze, Thunder, Wind, Earth, Wood | that basis | **local** dealt + received | 1.049 | 125 | 1,100 | - |
| GBlade ... GWood (12) | that basis | global dealt + received | 1.1 | 100,000 | 100,000 | - |

Thresholds are created at `OnLoadGame` for entries missing in the save (with the `GrowUpSpeed` of that save).

## Win (`OnBattleEndHandler`, `isWin`)

1. Loot x the S&M score multiplier (`SMScoreEvent.ScaleLoot`, [SMScoreEvent](SMScoreEvent.md)).
2. **Team takes loot**: for each dropped item, a random team member (not the player) takes it with chance `Luck / 10` %.
3. **Growth** (unless `NoGrowupFromBattles`): the basis of the damage type dealt most: +1 with chance `Talent / 10` %; same for the type received most.
4. **Exp** (unless `NoExpFromBattles`): `max(dealt x 0.001, 1) + max(received x 0.01, 1)` (this battle), `x Talent / 100`, then `SMScoreEvent.ScaleExp`.

## Defeat

Real battles, not the three-legged crow battle:

- Player dead:
  - exp cleared, **unless** the player has a body reconstruction item of the grade (grade 4: 1011721; 5-6: 1011722; 7: 1011723; 8: 1011723 / 1011725; 9-10: 1011724 / 1011726): one is used instead;
  - `Onelife`: Life = 0 and `PendingOneLifeDeath`;
  - `Onelife` or `AutoSaveAfterLostInBattle`: `PendingLossMonth`.
- Each team member: 10 % to quit (dismissed, +20 hate).

Back on the map (`OnOpenUIEnd(MapMain)`, 500 ms later): one-life -> the character dies (or an heir takes over: `FamilyEvent.OfferInherit`, [FamilyEvent](FamilyEvent.md)); else the loss month -> `TimeSkipEvent.SkipTime(1)` (one month passes and the game saves). Done on the map because the game does not kill a unit in a battle (autotest TZ-01).

## A world unit dies (`OnBattleUnitDie`)

Not in town wars / monster waves.

| Effect | Rule |
|---|---|
| NPC killer exp | `(dead max HP + killer max HP) x killer Talent / 100` |
| Lifespan (`LostLifespanWhenDie`) | dead loses `max(6 x gameLevel, maxHP / (3000 / gameLevel))` months; a world-unit killer gains half |
| Items (`LostItemWhenDie`) | each sellable item is lost unless `Random < Luck / 10 + ring lockScore / 100` %; the killer receives it (non-stackable items keep their data; player killer: removed from the drop list) |
| Player died to a world unit | the killer takes all of the battle's drops |
| Money (`LostMoneyWhenDie`) | the killer takes it all, the dead has 0 |
| Reputation | dead rep > killer rep: killer + dead rep / 10; dead rep > 2 x killer rep: dead - rep / 10 |
| Hate | the dead hates the killer +200 |
| Angel kills Evil (role lucks) | killer gains 20 % of the dead's max HP, MP, SP, Atk, Def and its own best basis |
| Evil kills anyone | killer gains 1 % of the dead's max HP, MP, SP, Atk, Def and all 12 bases |

The real-battle part (lifespan onwards) needs a real battle that is not the crow battle; NPC exp does not.

## Tests

TB-02, TM-29, TB-12, TZ-01.
