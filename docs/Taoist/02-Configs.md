# Taoist - Configuration

## mod_configs.json (`ModProject/ModConf/mod_configs.json` -> `Object/ModStts.cs`)

Read once in `ModMain.OnLoadClass` (`ModMain.ModObj.ModSettings`). Static balance data; players normally do not change it.

| Section | Used by | Content |
|---|---|---|
| `BaseConfigs` | ModMain | Ratios applied to base configs: level exp, artifact durability/SP/HP/Atk/Def, skill mastery exp, skill MP cost / CD / requirements per grade, power-up pill effects |
| `ContributionExchangeConfigs` | ContributionExchangeEvent | `ExchangeRatio` |
| `MissionDeclareConfigs` | CommissionTask | fee, degree cost, `CostTime`, `SuccessRate` per grade |
| `BattleManashieldConfigs` | BattleManashieldEvent | `ManaShieldRate1/2` |
| `UnitTypeConfigs` | UnitTypeEvent | chances of each role (`UnitTypes`) |
| `RealTrialConfigs` | RealTrialEvent | power-up per game level / grade |
| `RealStorageConfigs` | RealStorageEvent | `FeeRate` |
| `RealMarketConfigs` | RealMarketEvent/2 | min/max sell & buy rates, town master/guardian discounts, `Events` (HappyNewYear, HappyMidYear windows) |
| `MonstStrongerConfigs` | MonstStrongerEvent, BattleModifyEvent | `GrowRate`, `KillGrowRate`, `AreaBonus`, `PlayerAffectSummon`, `PlayerAtk2Hp`, `RebirthAffect`, Atk/Def/MHp ratios |
| `NpcUpgradeSkillConfigs` | NpcUpgradeSkillEvent | `UpgradeRates` per martial type |
| `ExpertConfigs` | ExpertEvent, UnitModifyHelper | `SkillDmgRatios`, `SkillExpRatios`, artifact exp rates |
| `BattleRewardConfigs` | BattleRewardEventV2 | `ExpPerDmgDealt`, `ExpPerDmgRecv` |
| `BankAccountConfigs` | BankAccountEvent | `OpenFee` per area |
| `InflationaryConfigs` | InflationaryEvent | `InflationaryRate` |
| `MapBuildBattleConfigs` | MapBuildBattleEvent | town war / wave settings |
| `MarketItemConfigs` | RealMarketEvent3 | join/deal/cancel rates, ranges |
| `BattleConfigs` | BattleEvent, HirePeopleEvent | join range and rates, intimacy thresholds, betray rate |
| `CustomRefineConfigs` | CustomRefine | `LevelRates` (adjustment quality chances) |

## S&M configs (difficulty)

Edited on the title screen (`SMGlobalConfigsEvent`, saved in `mod\nE7UL2\globals\SMGlobalEvent.json`), copied into each **new** save (`SMLocalConfigsEvent`, `saves\<playerId>\SMLocalEvent.json`). Changing the global configs does not affect existing saves.

* Rates (`Add...Rate`, applied with `Calculate(base, rate)` = base x (1 + rate)): monster Atk/Def/HP/basis, special monsters, tax, inflation, building cost, bank account cost/fee, refine cost, sect exchange, NPC growth, level-up exp, item value, market NPC join/attack.
* Switches: HideSaveButton, HideReloadButton, HideBattleMap, NoRebirth, Onelife, OnlyPortalAtCityAndSect, NoExpFromBattles, SectNoExchange, BossHasShield, NoGrowupFromBattles, AllowUpgradeNaturally, EnableTrainer, AllowTownBuildupOverTime(+_IncludeFirstTown), LostItemWhenDie, MonsterPropertiesDependOnPlayerProperties, AutoSaveAfterLostInBattle, NoMarketItem, NoStalker, NoTaxing, NoQiCultivation, LostLifespanWhenDie, LostMoneyWhenDie, OnlyActiveOnCurOrNearArea, DestroyBuildingAfterTownWar, AllowRepairGodWeapon, AllFunctionsApplyToNearestUnits.
* Values: PriorityDestinyLevel, NPCAmount, GrowUpSpeed (months between growth ticks), StartupPlayerRole, MaxSellersEachTown, MaxItemsOnSeller, MaxBuyersOnSellingItem.

## _EventOrderIndex.json

`{ "<CacheId>": <OrderIndex> }` - execution order of events (see [00-Overview](00-Overview.md#event-order-_eventorderindexjson)).

## Game config overrides and texts

`<group>_<configName>.json` files add or replace rows of base game configs (`roleCreateFeature`, `roleEffect`, `dramaDialogue`, `taskBase`, `dungeonBase`...). Check ids with ModLib's title screen tool (duplicated ids). Formats: [../ModConf/00-SampleConfs-List.md](../ModConf/00-SampleConfs-List.md). Texts: `*_localText.json` (+ translated folders), see [../04-LocalText.md](../04-LocalText.md).
