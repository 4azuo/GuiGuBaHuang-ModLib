# Taoist - Configuration

## mod_configs.json (`ModProject/ModConf/mod_configs.json` -> `Object/ModStts.cs`)

Read once in `ModMain.OnLoadClass` (`ModMain.ModObj.ModSettings`). Static balance data; players normally do not change it.

| Section | Used by | Content |
|---|---|---|
| `BaseConfigs` | ModMain | Ratios applied to base configs: level exp, artifact durability/SP/HP/Atk/Def, skill mastery exp, skill MP cost / CD / requirements per grade, power-up pill effects |

The settings of the features are in their own files (below).

## Feature config files (`JsonConfig<T>`)

One file per feature in `ModProject/ModConf/`, read once in `ModMain.OnLoadClass` (`<Class>.Load(modId)`, then `<Class>.Conf`). The defaults are the property initializers of the class and the file holds the same values (unit test `FeatureConfigs_MatchDefaults`: change both). A value left out, a broken file or a wrong value (chance outside 0-100...): the default / a fixed value (`Fix`), so players can edit the files without breaking the mod. Base class: `Object/JsonConfig.cs`. Keep game (Il2Cpp/Unity) types out of these classes where possible, so the unit tests can read them outside the game (a method returning a game type goes to the event, e.g. `UnitTypeEvent.RandomUnitType`, `CustomRefine.RandomRefineLevel`); the classes keyed by game enums (`MonstType`, `MartialType`) are only checked by their keys.

| File | Class (`Mod/<Event>.Config.cs`) | Used by | Content |
|---|---|---|---|
| `estate_configs.json` | `EstateConfig` (own loader, `EstateEvent.Config.cs`) | EstateEvent, SpiritNoteEvent | plots, houses, palace, rooms, map, notes |
| `battle_configs.json` | `BattleConfig` | BattleEvent, HirePeopleEvent | join range and rates, intimacy thresholds, betray rate, righteousness difference |
| `market_item_configs.json` | `MarketItemConfig` | RealMarketEvent3 | join/deal/cancel rates, hidden/stalked rates, range |
| `map_build_battle_configs.json` | `MapBuildBattleConfig` | MapBuildBattleEvent | `EscapeChance` |
| `inflationary_configs.json` | `InflationaryConfig` | InflationaryEvent | `InflationaryRate` |
| `bank_account_configs.json` | `BankAccountConfig` | BankAccountEvent | `OpenFee` per area (an area left out: its default) |
| `custom_refine_configs.json` | `CustomRefineConfig` | CustomRefine | `LevelRates` (adjustment quality chances) |
| `contribution_exchange_configs.json` | `ContributionExchangeConfig` | ContributionExchangeEvent | `ExchangeRatio` |
| `mission_declare_configs.json` | `MissionDeclareConfig` | CommissionTask | fee, degree cost, `CostTime`, `SuccessRate` per item level (6 values) |
| `battle_manashield_configs.json` | `BattleManashieldConfig` | BattleManashieldEvent | `ManaShieldRate1/2` |
| `unit_type_configs.json` | `UnitTypeConfig` | UnitTypeEvent | chances of each role (`UnitTypes`) |
| `real_trial_configs.json` | `RealTrialConfig` | RealTrialEvent | power-up per game level / grade |
| `real_storage_configs.json` | `RealStorageConfig` | RealStorageEvent | `FeeRate` |
| `real_market_configs.json` | `RealMarketConfig` | RealMarketEvent/2 | min/max sell & buy rates, town master/guardian discounts, `Events` (HappyNewYear, HappyMidYear windows) |
| `monst_stronger_configs.json` | `MonstStrongerConfig` (MonstType) | MonstStrongerEvent, BattleModifyEvent | `GrowRate`, `KillGrowRate`, `AreaBonus`, `PlayerAffectSummon`, `PlayerAtk2Hp`, `RebirthAffect`, Atk/Def/MHp ratios |
| `npc_upgrade_skill_configs.json` | `NpcUpgradeSkillConfig` (MartialType) | NpcUpgradeSkillEvent | `UpgradeRates` per martial type |
| `expert_configs.json` | `ExpertConfig` (MartialType) | ExpertEvent, UnitModifyHelper | `SkillDmgRatios`, `SkillExpRatios`, artifact exp rates |
| `battle_reward_configs.json` | `BattleRewardConfig` | BattleRewardEventV2 | `ExpPerDmgDealt`, `ExpPerDmgRecv` |

## S&M configs (difficulty)

Edited on the title screen (`SMGlobalConfigsEvent`, saved in `mod\nE7UL2\globals\SMGlobalEvent.json`), copied into each **new** save (`SMLocalConfigsEvent`, `saves\<playerId>\SMLocalEvent.json`). Changing the global configs does not affect existing saves.

* Rates (`Add...Rate`, applied with `Calculate(base, rate)` = base x (1 + rate)): monster Atk/Def/HP/basis, special monsters, tax, inflation, building cost, bank account cost/fee, refine cost, sect exchange, NPC growth, level-up exp, item value, market NPC join/attack.
* Switches: HideSaveButton, HideReloadButton, HideBattleMap, NoRebirth, Onelife, OnlyPortalAtCityAndSect, NoExpFromBattles, SectNoExchange, BossHasShield, NoGrowupFromBattles, AllowUpgradeNaturally, EnableTrainer, AllowTownBuildupOverTime(+_IncludeFirstTown), LostItemWhenDie, MonsterPropertiesDependOnPlayerProperties, AutoSaveAfterLostInBattle, NoMarketItem, NoStalker, NoTaxing, NoQiCultivation, NoBodyCultivation, NoMerit, NoArtisan, NoDaoHeart, NoSpiritSense, LostLifespanWhenDie, LostMoneyWhenDie, OnlyActiveOnCurOrNearArea, DestroyBuildingAfterTownWar, AllowRepairGodWeapon, AllFunctionsApplyToNearestUnits.
* Values: PriorityDestinyLevel, NPCAmount, GrowUpSpeed (months between growth ticks), StartupPlayerRole, MaxSellersEachTown, MaxItemsOnSeller, MaxBuyersOnSellingItem.

## _EventOrderIndex.json

`{ "<CacheId>": <OrderIndex> }` - execution order of events (see [00-Overview](00-Overview.md#event-order-_eventorderindexjson)).

## Game config overrides and texts

`<group>_<configName>.json` files add or replace rows of base game configs (`roleCreateFeature`, `roleEffect`, `dramaDialogue`, `taskBase`, `dungeonBase`...). Check ids with ModLib's title screen tool (duplicated ids). Formats: [../ModConf/00-SampleConfs-List.md](../ModConf/00-SampleConfs-List.md). Texts: `*_localText.json` (+ translated folders), see [../04-LocalText.md](../04-LocalText.md).
