# DamageInfo + DamageInfoEvent

`3161035078/ModProject/ModCode/ModMain/Mod/DamageInfo.cs` - `DamageInfo` is a static class; `DamageInfoEvent` (cache key `ModConst.DAMAGE_INFO_EVENT`) only stores the recorded foes - group: Battle

The damage calculation window: shows how the [BattleModifyEvent](BattleModifyEvent.md) pipeline treats the player's hits and the hits the player takes. Page 1: the player's attack / defense numbers, the target, example hits without randomness, the last hit dealt / taken step by step. Page 2: the last 3 monsters and 3 cultivators fought, as cards with their picture and the damage both ways.

## Opened by

- the "Damage Calc" button of the battle screen (under the damage multiplier, added by BattleModifyEvent);
- the hotkey "Damage calculation" (Taoist Settings > Other hotkeys, default F9 = `DEFAULT_KEY` 290; [AnimaWeaponEvent](AnimaWeaponEvent.md)), battle only;
- a button of the player info, stats tab, under the combat numbers ([UIEvent](UIEvent.md)), to work it out before a battle.

`Toggle()` opens / closes. In a battle, opening **pauses** the game (`PauseGame`) when it was running; `Close()` or `Watch()` (window closed by Esc / X, checked every frame) resumes it.

## Saved data (`DamageInfoEvent`)

| Property | Meaning |
|---|---|
| `Foes` | `List<FoeRecord>`: recent foes with their numbers at that time (newest first, 3 per kind) |

`FoeRecord`: Human, Name, Grade, BaseId (monster model) / UnitId (portrait), Atk, Def, Hp/HpMax, Mp/MpMax, Sp/SpMax, Dp/DpMax, the 12 bases, Evade, BlockMax, BlockDmg, BlockRatio, CritChance, CritDamage, MinDmg, SkillBonus, Year, Month. Identity: cultivator by unit id, monster by model + grade (the same foe replaces its old record).

## Recording hits (runtime)

| Call (from BattleModifyEvent) | What it does |
|---|---|
| `Reset()` (battle screen opens) | forgets the last hits and the record timers |
| `BeginHit()` (each hit) | only hits dealt or taken by the player: a new `Hit` (attacker, target) becomes `LastDealt` or `LastTaken`; `LastFoe` = the other unit; `RecordFoe` (at most every 2 s per unit) |
| `Step(name, value)` (each step) | adds the value after a pipeline step |

## Numbers of a side

`Side` is built `FromWorld` (a cultivator: battle numbers when in battle, else world numbers), `FromCtrl` (a battle unit) or `FromRecord` (a recorded foe). The formulas match BattleModifyEvent: `EvadeChance`, `BlockMax`, `BlockChance(basis)`, `BlockDmg`, `BlockRatio`, `CritChance`, `CritDamage`, `MinDmgPerGrade`, `SkillBonus` (left skill expertise x SkillDamage adjustment), `TimeMultiplier` (1 outside battle). `MainBasis` = the highest basis.

## Example hit (`Example(attacker, defender, crit)`)

Base = attacker Atk, through the pipeline without random parts (no evasion, no block chance, critical only when asked; MP / SP / DP shields as they are now):

`base > time > skill (cultivator) > basis (main basis) > mp > sp (grade 4+) > dp (grade 8+) > crit (if asked) > basisdef > mpblock > spblock (4+) > dpblock (8+) > min`

`Steps(list)` renders "label value > label value ...", skipping steps that changed nothing (labels `dmginfo_step_*`). `HitsToKill(dmg, hp) = ceil(hp / dmg)`.

## Page 1 (`StatsPage`)

- **Attack** column: Atk, time multiplier, skill (name, bonus), main basis (value, bonus `Atk x basis / 1000`), MP / SP / DP bonus, critical chance and multiplier `1 + CritDamage`, instant kill %, min damage (per grade x target grade), steal HP / MP / SP.
- **Defense** column: Def, evasion (chance, max, step bonus), block (chance vs the target's main basis, max, amount), basis defense, MP block (tries, amount, ratio), SP / DP block, HP.
- **Target**: in a battle the last foe still alive (`dmginfo_target`), else the player against themselves (`dmginfo_target_self`); its Atk / Def / HP / evasion / block; example dealt, critical and taken.
- **Last hits**: `LastDealt`, `LastTaken` step by step; a note (battle paused / see page 2).

## Page 2 (`FoesPage`)

Monsters on top, cultivators below, 3 cards each (`dmginfo_foes_none` when empty). A card: picture 130 x 190 (cultivator: portrait via `PortraitHelper`, UV `HUMAN_UV`; monster: battle model via `battleUnitBase.CreateModel`, UV `MONSTER_UV`, offset `MonsterOffsetY` -2.3), name, grade and date, Atk / Def / HP / evasion, damage dealt (normal, critical, hits to kill) and taken (hits to fall), computed from the recorded numbers against the player's numbers now.

## Tests

TB-17, TG-61, TR-02.
