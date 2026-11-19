# NpcAutoCraftArtifact

`3161035078/ModProject/ModCode/ModMain/Mod/NpcAutoCraftArtifact.cs` - cache key `ModConst.NPC_AUTO_CRAFT_ARTIFACT` - group: World and NPCs

NPCs now and then craft an artifact suited to their grade, with random grade, level and attributes.

## Saved data

None (`ArtifactConfs` is a static runtime cache).

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame` | Caches every item config that is an artifact (`IsArtifact() != null`) into `ArtifactConfs`. |
| `OnMonthlyForEachWUnit` | NPCs only. Artifacts whose `initGrade <= unit grade`; on a successful roll: draws the grade, drops the kinds already owned at that grade, adds a random remaining one and randomizes it. |

## Formulas

```
chance (%) = ChanceOver(RefineWeapon / 1000 + Luck / 100, MonthsPassed)
```

`ChanceOver(p, n)` = chance of at least one success in `n` tries (time skip).

Crafted artifact (`WUnitHelper.LastAddedItems`):

| Field | Value |
|---|---|
| grade | biased random from `max(grade - 2, 1)` to `grade`: 80 % to stop at each step, so the lowest value is the most frequent |
| level | biased random 1..6 with stop chance `LevelStopRate = clamp(80 - RefineWeapon / 100, 0, 80)` %: each step stops with that chance, else goes one level higher. RefineWeapon 0: level 1 80 %, level 2+ 20 %; 1,000 (stop 70 %): level 2+ 30 %; 4,000 (stop 40 %): level 2+ 60 %, level 6 about 8 % (0.6^5); 8,000+: always level 6 |
| 5 attribute rates | `Random(70 + b, 130 + b)` % each (`SetAttrrRate`), `b = RefineWeapon / 50` (`AttrBonus`: +1 % every 50 forge points; e.g. 200 -> 74-134 %, 1,000 -> 90-150 %) |

## Notes

- The artifact is added to the bag; the NPC's AI equips it by itself.
- No duplicates: the grade is drawn first, then the candidates exclude every artifact kind (item id) the NPC already owns at that grade, equipped or in the bag (`OwnedArtifactKeys`, key `"{propsId}_{grade}"`). If none is left, nothing is crafted that month. The same kind at another grade is allowed. Before 2026-11-19 there was no limit at all (e.g. RefineWeapon 200, Luck 50: about 8 artifacts in 100 years, duplicates included).

## Tests

TM-34.
