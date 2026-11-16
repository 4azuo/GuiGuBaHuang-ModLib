# Known issues

Issues found while documenting, unit testing and preparing the autotest (2026-11-15). **All fixed on 2026-11-16** (fix applied = the "Suggested fix" column, except as noted below).

Fix notes:
* BUG-01: instant kill only when a world unit hits a **monster** (`IsWorldUnitAttacking && !IsWorldUnitHit`): the feature now works for the first time (balance change).
* BUG-07/08/09: their reproducing tests became normal tests (`ObjectTests.PngEncode_Signature`, `ConfigTests.LocalTextKeys_UsedInCode_Exist`, `ConfigTests.EventOrderIndex_MatchesEvents`); the allow-lists in `KnownIssues` are empty.
* BUG-09/10: ChallengeQuestEvent 2025, UseSpiritStoneCulEvent 2085, RepairShinkiEvent 4030, UISettingScreenEvent 6025, TimeSkipEvent 7000 (still after every other event, as before); markets keep their actual order with distinct values: RealMarketEvent2 3050, RealMarketEvent3 3060, RealMarketEvent 3065.
* BUG-12: `SafeSumFloat(params double[])` added, `SafeSumInt(params double[])` kept as `[Obsolete]` (ModLib API).
* BUG-16: `CacheHelper.AddIfAccepted` resets the static `Instance` of a rejected object; `UnitModifyHelper` Qi formulas return 0 when `QiCulEvent.Instance` is null.
* BUG-17: UI samples loaded first, default `ModConfigs` (with a log line) when `mod_configs.json` is missing/invalid.

Also fixed while testing (2026-11-16, not in the table): lost items given to the killer (autotest TB-12), one-life death and the auto save after a lost battle now run back on the map instead of inside the battle (TZ-01, TB-15), `GodArtifactHelper.IsEnabled*` checked only the first artifact. Found by the full autotest run on a new character (2026-11-16):
  * `NpcUpgradeSkillEvent`: `luck / 100` (int division) was 0 below 100 luck, NPCs never upgraded their martial arts (TG-44).
  * `InflationaryEvent` currency reform: the player money was divided twice (the player is also in the NPC loop) -> 0 (TX-02).
  * `challenge_roleEffect.json`: effect ids 4444421xx while the lucks use 4444451xx -> `NullReferenceException` in `CreateAction` whenever a challenge luck was added (TB-07); unit test `ConfigTests.LuckEffects_Exist` guards every luck effect.
  * ModLib `UIHelper.MsgBox` Yes/No showed "Enable/Disable" buttons (game popup type 2 labels): now "Yes/No" (TG-46).
  * `RealMarketEvent3` VIP fee paid to the town of the market open now (the cover could hold an older market UI).
  * ModLib `RemoveStorageItem` removed items while enumerating the list; `SafeAddInt`/`SafeMulInt` turned the value into 0 on a NaN term (now unchanged).

Status: `Open` (to check) · `Confirmed` · `Fixed` · `Won't fix`. Severity: High = wrong behaviour players will see, Medium = feature partly broken, Low = robustness / cosmetic.
`Test` = unit test reproducing the issue (`tests/UnitTests/KnownIssues.cs`, run with `scripts\run-unittests.ps1 -IncludeKnownIssues`); when an issue is fixed, move its test to the normal tests and remove it from the allow-lists in `KnownIssues`.

| Id | Severity | Status | Where | Issue | Suggested fix | Test |
|---|---|---|---|---|---|---|
| BUG-01 | High | Fixed | `3161035078/.../Mod/BattleModifyEvent.cs:135` | Instant kill condition `IsWorldUnitHit && !IsWorldUnitHit` is always false: the `InstantKill` refine adjustment never triggers | `ModBattleEvent.IsWorldUnitAttacking && !ModBattleEvent.IsWorldUnitHit` (world unit attacks a monster) - confirm the intended targets | - (battle only) |
| BUG-02 | Medium | Fixed | `3161035078/.../Mod/BattleEvent.cs:124-127` | Stalker join: the stalkers are removed from `BattleAfterEvent.Stalkers` (line 124) before `Stalkers[wunitId]` is read (line 127) -> `KeyNotFoundException`, hidden by `[ErrorIgnore]`. The stalker team joins but the reason drama is never shown | Read `stalkReason` before removing the entries | - (battle only) |
| BUG-03 | Medium | Fixed | `3161035078/.../Mod/BattleAfterEvent.cs:76` | `Stalkers.Add(...)` without `ContainsKey`: throws when a member already stalks the player, the remaining members are skipped | `if (!Stalkers.ContainsKey(id)) Stalkers.Add(...)` (or `AddStalker`) | - |
| BUG-04 | Medium | Fixed | `3161035078/.../Mod/MapBuildBattleEvent.cs:151-152` | `if (master == null) return;` inside the town-war loop: the first town without master stops the war check of every following town for that month | `continue` | - |
| BUG-05 | Medium | Fixed | `3161035078/.../Mod/RepairShinkiEvent.cs:104-112` | `AddFishJade` checks that the player has the money but never deducts `cost` (Devil Demon / God Eye repairs do) | `g.world.playerUnit.AddUnitMoney(-cost);` before `GodArtifactHelper.AddFishJade(1)` | - |
| BUG-06 | Low | Fixed | `3161035078/.../Mod/HirePeopleEvent.cs:349` | `Hire` does not check that the member is already in the team (monthly "team up with friends" can add the same id twice -> double payment) | Return when `TeamData[masterId].Contains(memberId)` | - |
| BUG-07 | Medium | Fixed | `3385996759/.../Helper/PngEncodeHelper.cs:21` + `Write` (line ~91) | The PNG signature is written with `Encoding.ASCII`: byte `0x89` becomes `?` (0x3F), so every PNG produced by `EncodeRGBA` (texture export) is invalid | Write the signature as bytes: `ms.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8)` | `BUG07_PngEncode_Signature` |
| BUG-08 | Medium | Fixed | `3161035078/.../Mod/RealMarketEvent3.cs:405` | `GameTool.LS("500020093")`: the LocalText key is `other500020093`, the drama shows the raw key | `GameTool.LS("other500020093")` | `BUG08_LocalTextKey_500020093` |
| BUG-09 | Low | Fixed | `3161035078/ModProject/ModConf/_EventOrderIndex.json` | No order for `ChallengeQuestEvent`, `RepairShinkiEvent`, `TimeSkipEvent`, `UISettingScreenEvent`, `UseSpiritStoneCulEvent`: they get 9000+ automatically and run after the UI/battle groups | Add them to the matching group | `BUG09_AllEventsHaveOrder` |
| BUG-10 | Low | Fixed | `3161035078/ModProject/ModConf/_EventOrderIndex.json` | `RealMarketEvent` and `RealMarketEvent3` both have 3060 and `RealMarketEvent2` (3050) runs before `RealMarketEvent`: relative order undefined/unintended? | Give distinct, increasing values (3050/3055/3060) | - |
| BUG-11 | Low | Fixed | `3385996759/.../Converter/EnumObjectConverter.cs:65` | `WriteJson` calls `writer.WriteValueAsync(...)` without awaiting it inside a synchronous converter. Works with the in-memory writers used today, could write nothing/out of order with an async stream writer | `writer.WriteValue(...)` | - (round trip covered by `EnumObjectConverter_RoundTripKeepsSingleton`) |
| BUG-12 | Low | Fixed | `3385996759/.../Helper/ValueHelper.cs:183` | `SafeSumInt(params double[])` returns a `float` clamped to the float range (the name says int) | Rename (`SafeSumFloat`) or return `int` like the `long[]` overload | - |
| BUG-13 | Low | Fixed | `3161035078/.../Mod/SliceLifeQuestEvent.cs:168` | `task == null & taskInfo.ActiveCondition()` uses `&` (no short-circuit): harmless today, `&&` intended | `&&` | - |
| BUG-14 | Low | Fixed | `3161035078/.../Mod/MissionDeclareEvent.cs:219` | `OnMonthly` does not call `base.OnMonthly()` (all other events do) | Add `base.OnMonthly();` | - |
| BUG-15 | Low | Fixed | `3161035078/.../Mod/BankAccountEvent.cs:66` | `Cost` computes `round = OpenFee / 100` and divides by it: an `OpenFee` below 100 in `mod_configs.json` throws `DivideByZeroException` (current values are fine, guarded by `ModConfigs_SectionsMatchModStts`) | `Math.Max(1, OpenFee / 100)` | config guard only |
| BUG-16 | Medium | Fixed | `3385996759/.../Object/Base/CachableObject.cs` (`OnLoadClass`) + `Helper/CacheHelper.cs` (`LoadGlobalCaches`/`LoadGameCaches`) | The static `Instance` property is assigned in `OnLoadClass`, i.e. **before** `OnCacheHandler()` can reject the object. A feature switched off (e.g. `TrainerEvent` with EnableTrainer off, `QiCulEvent`, `UpgradeNaturallyEvent`, `RepairShinkiEvent`) keeps a non-null `Instance`: code testing `Instance != null` treats it as enabled (found by autotest TG-28: the trainer window opened although the trainer is disabled) | Assign `Instance` only after `OnCacheHandler()` returned true (or reset it to null when it returns false) | autotest TG-28 (scenario now checks the loaded events) |
| BUG-17 | Medium | Fixed | `3385996759/.../Mod/Base/ModMaster_CommonEvent.cs` (`OnInitMod`) | `OnInitMod` reads ModLib `ModConf/mod_configs.json` first and loads the UI sample components after: when the file is missing (it was missing in the Workshop copy of ModLib on this PC) the exception skips `UISampleHelper.LoadUISampples()` and **every** custom button of ModLib and Taoist fails (`The Object you want to instantiate is null`) | Load the samples first and fall back to default `ModConfigs` when the file is missing/invalid | found by autotest run 20261115-172232 |

(BUG-07 and BUG-08 were draft numbers merged into BUG-11/14.)

## Limitations (not bugs)

* `ModStts` (and every type using Il2Cpp enums such as `MartialType`, `MonstType`) cannot be loaded outside the game (`TypeLoadException: Method in an Enum`), so `mod_configs.json` is validated by structure in unit tests and by `TG-01` in the in-game autotest.
* Battle behaviour (BUG-01, BUG-02) can only be verified in game: manual scenarios TM-21/TM-27 and the battle watch scenarios (`-WatchBattles`).
