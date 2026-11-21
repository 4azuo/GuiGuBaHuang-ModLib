# Taoist - Overview

Taoist (Workshop `3161035078`, mod id / CacheId `nE7UL2`, namespace `MOD_nE7UL2`) is a large gameplay overhaul built on ModLib and the reference example of a ModLib mod.

Related docs: [01-Features](01-Features.md) · [02-Configs](02-Configs.md) · [03-Test-Scenarios](03-Test-Scenarios.md) · [04-Autotest](04-Autotest.md) · ModLib: [../ModLib/00-Architecture.md](../ModLib/00-Architecture.md)

## Source layout (`3161035078/ModProject/ModCode/ModMain/`)

| Path | Content |
|---|---|
| `ModMain.cs` | `ModMain : ModChild` (`[Cache("nE7UL2")]`, `[ModOrder("_EventOrderIndex.json")]`). `OnLoadClass` loads `mod_configs.json` into `ModSettings` and the feature config files (`<Class>Config.Load`) and rebalances base configs once (exp table, attribute limits, artifacts, skill mastery/MP/CD, item and skill prices, pills, furnaces, recipes, pets) |
| `Mod/*.cs` | One `ModEvent` per feature (see [01-Features](01-Features.md)) |
| `Object/` | Data classes: `ModStts` (mod_configs.json: BaseConfigs), `JsonConfig<T>` (base of the feature config files), `CustomRefine`, `MarketItem`, `NegotiatingDeal/Item`, `CommissionTask`, `GameEvent`, `SMItemWork` |
| `Enum/` | EnumObjects: unit roles (`UnitTypeEnum`), rank lucks (`ApprenticeLuckEnum`, `TraineeLuckEnum`, `MerchantLuckEnum`), refine adjustments (`AdjTypeEnum`, `AdjLevelEnum`), `BuildingCostEnum`, `ModItemTypeEnum`... |
| `Helper/` | Formulas: `UnitModifyHelper`, `PriceHelper`, `UnitPowerCalHelper`, `ItemHelper`, `UILocalHelper` |
| `Const/ModConst.cs` | Version (`TAOIST_VERSION`), required ModLib version, CacheIds of all events |
| `AutoTest/` | In-game autotest framework and scenarios (one file per scenario id) - see [04-Autotest](04-Autotest.md) |

ModConf (`3161035078/ModProject/ModConf/`): `mod_configs.json` (rebalancing of the base game configs), `<feature>_configs.json` (one settings file per feature, see [02-Configs](02-Configs.md); e.g. `estate_configs.json`: town estates), `_EventOrderIndex.json` (event order), game config overrides `<group>_<configName>.json` (lucks, effects, dramas, tasks, dungeons...), `*_localText.json` + translations in `de/ es/ fr/ ja/ la/ ru/ th/ vi/`.

## Event order (`_EventOrderIndex.json`)

| Range | Group | Events |
|---|---|---|
| 1000-1030 | common | PrepareEvent, SMGlobal/SMLocal configs, Inflationary, Trainer |
| 2000-2090 | world (no stat impact) | SliceLifeQuest, Rebirth, MissionDeclare, ContributionExchange, NpcAuto, NpcUpgradeSkill, NpcAutoCraftArtifact, UnitType, UpgradeNaturally, QiCul, HirePeople |
| 3000-3060 | economy | MapBuildProperty, BankAccount, BuildingArrange, RealStorage, RealMarket 1/2/3 |
| 4000-4020 | unit properties | CustomRefine, Expert, UnitModify |
| 5000-5060 | battle | Battle, BattleAfter, MapBuildBattle, MonstStronger, RealTrial, Spiritlock, BattleModify, BattleManashield, SpecialMonster, BattleRewardV2 |
| 6000-6030 | global UI | UIDifficulty, UICreatePlayer, UITitleScreen, UIEvent |
| 9990 | test | AutoTestEvent (only with an autotest request) |

Events not listed get 9000+ automatically.

## Conventions

* Every event exposes `public static XxxEvent Instance { get; set; }` (set by ModLib) and is reached from other features through it.
* Difficulty switches live in `SMLocalConfigsEvent.Instance.Configs` (per save copy of the S&M configs); `SMLocalConfigsEvent.Calculate(base, rate)` applies an S&M rate.
* Prices go through `InflationaryEvent.CalculateInflationary`; town/sect money through `MapBuildPropertyEvent.Get/AddBuildProperty`.
* User text: `GameTool.LS("<key>")` with keys from `*_localText.json`.
