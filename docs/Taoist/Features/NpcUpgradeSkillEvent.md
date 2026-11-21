# NpcUpgradeSkillEvent

`3161035078/ModProject/ModCode/ModMain/Mod/NpcUpgradeSkillEvent.cs` - cache key `ModConst.NPC_UPGRADE_SKILL_EVENT` - group: World and NPCs

NPC martial arts grow over time: level ups, higher-grade versions, mastery exp and prefix level ups.

## Saved data

None.

## Switch

`npc_upgrade_skill_configs.json` > `UpgradeRates` (`NpcUpgradeSkillConfig`): chance (%) per martial type: SkillLeft 7, SkillRight 7, Step 12, Ultimate 8, Ability 16 (each multiplied by Luck / 100). See [02-Configs](../02-Configs.md).

## Flow

| Hook | What happens |
|---|---|
| `OnMonthlyForEachWUnit` | NPCs only; runs `UpgradeMartial(wunit)` once per month passed (`MonthsPassed`). |

`UpgradeMartial(wunit)`:

1. Picks the martial type with `RandomUpgradingMartial(Random(0, 100), Luck / 100)`: the rates of `UpgradeRates` are laid end to end, each multiplied by `Luck / 100`; the roll falls in one range -> that type, else `None` (nothing this month).
2. For each equipped martial of that type:
   - **Upgrade roll** `Random(0,100) < Talent / 1000 + Luck / 100`:
     - level < 6 and martial grade <= unit grade -> level + 1;
     - else if martial grade < unit grade and `Random(0,100) < Luck / 10` -> adds the same martial (same base id) at grade + 1 with a random level 1..5.
   - **Mastery exp** (always): `exp += Talent(base) x GradeID x ratio`, ratio by type:

     | Type | Ratio |
     |---|---|
     | SkillLeft | 1.0 |
     | SkillRight | 0.9 |
     | Step | 0.8 |
     | Ultimate | 0.5 |
     | Ability | 1.2 |

   - **Prefixes**: for each unlocked prefix, chance `Talent / (10 + rateDec)` %; if below the max level, sets a new random level up to level + 1; `rateDec += 2` after each success (later prefixes are harder in the same month).

## Notes

- `Luck / 100f` is a float division: with an int division, NPCs with less than 100 luck never upgraded (found by autotest TG-44).
- The new higher-grade martial is added beside the old one; the NPC AI chooses what to equip.

## Tests

TM-34, TG-44.
