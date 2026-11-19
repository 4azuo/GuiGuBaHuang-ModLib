# TrainerEvent

`3161035078/ModProject/ModCode/ModMain/Mod/TrainerEvent.cs` - cache key `ModConst.TRAINER_EVENT` - group: Core

Cheat trainer for testing and casual play: a 4-page window opened by Ctrl+1 / Ctrl+2 / Ctrl+3 on the map.

## Saved data

None.

## Switch

`EnableTrainer` (S&M). Off: the event is not loaded (`OnCacheHandler`).

## Opening

`OnMonoUpdate`: Left Ctrl + 1 / 2 / 3 -> `OnHotkey(page 0 / 1 / 2)`: opens the trainer on that page, or switches the open one to it (a window closed by X / Esc is detected and rebuilt). Only on the map screen; elsewhere message `trainer044`. `UICustom1` with navigation buttons, fast close.

## Page 1: player

Left column (`trainer001`): recover (HP, MP, SP, mood, energy, health to max), max HP +-100,000, max MP +-10,000, max SP +-1,000, bases +-, Atk +-10,000, Def +-1,000.

Middle column: go to page 2, money +-1,000,000, mayor degree +-1,000, contribution +-, ability exp +-100,000, reputation +-10,000, "Become town master" (the player replaces the whole council of the town where they stand, master luck; message `townmaster420041113drama`; outside a town `other500020065`), divine artifact select (none / Pisces Pendant only / Devil Demon only / God Eye only, `GodArtifactHelper`).

Right column: pause / resume (`GameHelper.ChangeGameSpeed`), game speed x1 / x2 / x3, grade +-1 phase, add exp, lifespan +-100 years (1,200 months), travel speed +-1,000, view range +-.

Live readouts (`trainer038`, `039`, `040`): HP, MP, SP, money, mayor degree, contribution, Atk, Def, mood, stamina, health, lifespan, travel speed, view range, battle speed, ability points, luck, talent, exp, reputation, grade; the 12 bases; other numbers (`trainer049`-`054`).

Increases stop at `MAX_NUMBER`; decreases do not go under one step.

## Page 2: teleport

Buttons for every named building, grouped in columns by area (`TELE_AREA_COL`: which areas and buildings are shown); click -> the player moves to its first open point.

## Page 3: breakthrough items

A button per grade / phase / quality of `g.conf.roleGrade` that has breakthrough items (`itemShowA` / `itemShowB`): gives one of each (`trainer046`).

## Page 4: info

Number of units in the world (`trainer048`).

## Tests

TG-28, TG-51.
