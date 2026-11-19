# ExpertEvent

`3161035078/ModProject/ModCode/ModMain/Mod/ExpertEvent.cs` - cache key `ModConst.EXPERT_EVENT` - group: Unit properties

Expertise: every martial art, step, ability and artifact (each item instance, by `soleID`) earns expertise exp when it is used. The expertise level gives bonuses (applied by [UnitModifyEvent](UnitModifyEvent.md) and [BattleModifyEvent](BattleModifyEvent.md), shown by [UIEvent](UIEvent.md)).

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `ExpertExps` | `Dictionary<soleID, int>` | Expertise exp of each item (all units) |

## Switch

`mod_configs.json` > `ExpertConfigs`:

| Key | SkillLeft | SkillRight | Step | Ability | Ultimate |
|---|---|---|---|---|---|
| `SkillExpRatios` (exp gain) | 0.02 | 0.8 | 0.1 | 1.0 | 0.6 |
| `SkillDmgRatios` (damage bonus factor) | 0.1 | 1.0 | 1.0 | 0.4 | 5.0 |

`AutoArtifactExpRate` 1.0 (monthly, NPCs), `BattleArtifactExpRate` 0.02 (per hit). `GrowUpSpeed` (S&M) for the monthly part.

## Where exp comes from

| Hook | Who | Item | Ratio |
|---|---|---|---|
| `OnMonthlyForEachWUnit` (x ticks of `GrowUpSpeed`) | NPCs | every equipped martial art of a configured type | `SkillExpRatios[type]` |
| same | player | **active abilities only** (the player earns the rest by playing) | `SkillExpRatios[Ability]` |
| same | NPCs | every equipped artifact | `AutoArtifactExpRate` |
| `OnBattleUnitHit` | any human attacker | every equipped artifact with durability > 0 | `BattleArtifactExpRate` |
| same | | the left skill that made the hit | `SkillExpRatios[SkillLeft]` |
| `OnBattleUnitUseSkill` | any human | the right skill / ultimate used | ratio of its type |
| `OnTimeUpdate200ms` (map) | player | the equipped step, each time the player's map cell changes | `SkillExpRatios[Step]` |

## Formulas

```
gain       = ratio x 100 x gradeLvl x Random(1, 2) x Talent / 100        (clamped to [0, int.MaxValue])
need(lvl)  = 2^(lvl - 1) x 1000 x itemGrade x (1 + 0.2 x itemLevel)      (capped at int.MaxValue)
level      = highest lvl with exp >= need(lvl)
```

Example: grade 3 item level 2 -> level 1 at 4,200 exp, level 2 at 8,400, level 5 at 67,200.

## Bonuses by expertise level (`UnitModifyHelper`)

`L` = expertise level, `G` / `l` = item grade / level, `base` = the unit's base value of that property.

| Item | Bonus |
|---|---|
| Artifact Atk | `artifact atk x L x (0.012G + 0.004l) + 24G + 6l` |
| Artifact Def | `artifact def x L x (0.008G + 0.002l) + 6G + l` |
| Ability Atk / Def / HP / MP / SP | `base x L x r + v`, r / v: Atk 0.005G+0.001l / 5G+2l; Def 0.003G+0.0006l / 4G+l; HP 0.010G+0.002l / 100G+20l; MP 0.010G+0.002l / 4G+l; SP 0.003G+0.0005l / 4G+l |
| Step speed | `L x (6G + 2l)` |
| Step evade (%) | `L x (0.10G + 0.03l)` (battle) |
| Skill damage | `(skill atk x L x (0.010G + 0.002l) + 24G + 6l) x SkillDmgRatios[type]` (battle) |
| Skill / step MP cost | `max(cost x L x r, least)`, r = 0.06G+0.01l (skill), 0.08G+0.01l (step) |

All bonuses are 0 at level 0.

## API

`AddExpertExp(wunit, soleId, ratio)`, `GetExpertExp(soleId)`, `GetExpertNeedExp(lvl, grade, level)`, `GetExpertLvl(soleId, grade, level)`.

## Notes

- Exp is stored by item instance: a new copy of the same skill starts at 0.
- Entries of items that no longer exist stay in the dictionary.

## Tests

TG-07, TG-52.
