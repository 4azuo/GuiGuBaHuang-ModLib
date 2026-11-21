# SMGlobalConfigsEvent

`3161035078/ModProject/ModCode/ModMain/Mod/SMGlobalConfigsEvent.cs` - cache key `ModConst.SM_GLOBAL_CONFIGS_EVENT` (Global cache, works on the title screen) - group: Core

The S&M ("sadism & masochism") difficulty configs, edited in the "S&M Configs" window of the title screen and saved globally. Every option has a score; the total **P** is the difficulty (and drives [SMScoreEvent](SMScoreEvent.md)). A new save copies all of it into [SMLocalConfigsEvent](SMLocalConfigsEvent.md), which is what the features read.

## Options and scores

"Rate" sliders are multipliers added to the base (`Calculate(base, rate) = base x (1 + rate)`); the score of a slider is `value x factor`. Toggles score only when on (and when their condition holds). The UI shows each option's points.

### Page 1

| Option | Range / default | Score | Used by |
|---|---|---|---|
| `AddAtkRate`, `AddDefRate`, `AddHpRate`, `AddBasisRate` (monsters) | -0.5 .. 8 / 0 | x 100 | [MonstStrongerEvent](MonstStrongerEvent.md) |
| `AddSpecialMonsterRate` | -0.5 .. 1 / 0 | x 3,000 | [SpecialMonsterEvent](SpecialMonsterEvent.md) |
| `MonsterPropertiesDependOnPlayerProperties` | off | +5,000 | MonstStrongerEvent |
| `BossHasShield` | off | +10,000 | SpecialMonsterEvent |
| `AddNpcGrowRate` | 0 .. 8 / 0 | x 1,000 | [NpcAutoEvent](NpcAutoEvent.md), [UnitTypeEvent](UnitTypeEvent.md) |
| `NPCAmount` | 1,000 .. 10,000 / 2,000 | = value | the game's initial NPC count of a new world: OK writes it into `g.conf.npcInitParameters` item 1 (only in that game session: after a restart it applies once the window is confirmed again) |
| `AddLevelupExpRate` | 0 .. 1 / 0 | x 2,000 | SMLocalConfigsEvent |
| `GrowUpSpeed` (months per growth tick) | 1 .. 24 / 3 | `(12 - value) x 1,000` | Qi, unit types, expertise, refine, monsters, training |
| `StartupPlayerRole` | none / 8 roles | -10,000 if set | UnitTypeEvent |
| `PriorityDestinyLevel` | 0 .. 6 / 0 | `(3 - level) x 1,000` if set | [UICreatePlayerEvent](UICreatePlayerEvent.md) |
| `HideSaveButton` | off | +1,000 (enables HideReload) | [UIEvent](UIEvent.md) |
| `HideReloadButton` | off | +5,000 (enables Onelife) | UIEvent |
| `LostItemWhenDie` | off | +10,000 | [BattleRewardEventV2](BattleRewardEventV2.md) |
| `LostMoneyWhenDie` | **on** | +1,000 | BattleRewardEventV2 |
| `LostLifespanWhenDie` | off | +2,000 | BattleRewardEventV2 |
| `Onelife` | off | +20,000 (forces AutoSaveAfterLostInBattle) | BattleRewardEventV2 |
| `AutoSaveAfterLostInBattle` | off | +20,000 | BattleRewardEventV2 |
| `EnableTrainer` | off | -1,000,000 | [TrainerEvent](TrainerEvent.md) |
| `AllFunctionsApplyToNearestUnits` | off | -1,000 | [PrepareEvent](PrepareEvent.md) |

Buttons: Export (save dialog, copies the global cache file), Import (open dialog, replaces and reloads; unreadable file -> `ui_file_errors`), open the save data folder, open the ModConf folder (Explorer: `mod_configs.json` and the `*_configs.json` settings files of the features).

### Page 2 (economy, market, systems)

| Option | Range / default | Score | Used by |
|---|---|---|---|
| `AddTaxRate` | -0.5 .. 5 | x 100 | [MapBuildPropertyEvent](MapBuildPropertyEvent.md) |
| `AddInflationRate` | -0.5 .. 2 | x 1,000 | [InflationaryEvent](InflationaryEvent.md) |
| `AddBuildingCostRate` | 0 .. 8 | x 100 | [BuildingArrangeEvent](BuildingArrangeEvent.md) |
| `AddBankAccountCostRate` | 0 .. 8 | x 100 | [BankAccountEvent](BankAccountEvent.md) |
| `AddBankFee` | 0 .. 100 | x 100 | [RealStorageEvent](RealStorageEvent.md) |
| `AddRefineCost` | -0.5 .. 1 | x 2,000 | [CustomRefineEvent](CustomRefineEvent.md) |
| `AddSectExchangeRate` | -0.5 .. 1 | x 2,000 (not with SectNoExchange) | [ContributionExchangeEvent](ContributionExchangeEvent.md) |
| `AddItemValueRate` | -0.5 .. 1 | x 1,000 | InflationaryEvent (start index) |
| `MarketItemNpcJoinRate`, `MarketItemGetAttackedRate` | -0.5 .. 1 | x 100 / x 1,000 (market on) | [RealMarketEvent3](RealMarketEvent3.md) |
| `MaxSellersEachTown`, `MaxItemsOnSeller`, `MaxBuyersOnSellingItem` | 1..30 / 8, 1..10 / 4, 1..20 / 4 | 0 | RealMarketEvent3 |
| `NoMarketItem` | off | +10,000 | RealMarketEvent3 |
| `NoStalker` | off | -3,000 | [BattleAfterEvent](BattleAfterEvent.md) |
| `NoTaxing` | off | -2,000 | MapBuildPropertyEvent |
| `NoQiCultivation` | off | +2,000 | [QiCulEvent](QiCulEvent.md) |
| `HideBattleMap` | off | +2,000 | [BattleModifyEvent](BattleModifyEvent.md) |
| `NoRebirth` | off | +10,000 | [RebirthEvent](RebirthEvent.md) |
| `OnlyPortalAtCityAndSect` | off | +1,000 | BuildingArrangeEvent |
| `NoExpFromBattles` | off | +1,000 | BattleRewardEventV2 |
| `SectNoExchange` | off | +10,000 | ContributionExchangeEvent |
| `NoGrowupFromBattles` | off | +10,000 | BattleRewardEventV2 |
| `AllowUpgradeNaturally` | off | -2,000 | [UpgradeNaturallyEvent](UpgradeNaturallyEvent.md) |
| `AllowTownBuildupOverTime` (+ `_IncludeFirstTown`) | off | +1,000 (+1,000) | BuildingArrangeEvent |
| `OnlyActiveOnCurOrNearArea` | **on** | -5,000 | RealMarketEvent3 |
| `DestroyBuildingAfterTownWar` | **on** | +2,000 | [MapBuildBattleEvent](MapBuildBattleEvent.md) |
| `AllowRepairGodWeapon` | off | -5,000 | [RepairShinkiEvent](RepairShinkiEvent.md) |

### Page 3 (additional features)

Turning off a feature that helps the player costs points; turning off one that hurts gives points back.

| Option | Score | Used by |
|---|---|---|
| `NoEncounter` | +1,000 | [EncounterEvent](EncounterEvent.md) |
| `EncounterRate` (0 .. 10 %, default 3) | `(3 - rate) x 300` (encounters on) | EncounterEvent |
| `NoBankLoan` | +1,000 | [BankLoanEvent](BankLoanEvent.md) |
| `NoBuyOrder` | +500 | RealMarketEvent3 |
| `NoBounty` | -2,000 | [WantedEvent](WantedEvent.md) |
| `NoSeclusion` | +1,000 | [SeclusionEvent](SeclusionEvent.md) |
| `SeclusionRate` (0 .. 3 %, default 0.5) | `(0.5 - rate) x 2,000` (seclusion on) | SeclusionEvent |
| `NoForcedSeclusion` | -2,000 (seclusion on) | SeclusionEvent |
| `NoFamily` | +2,000 | [FamilyEvent](FamilyEvent.md), [FamilyFundEvent](FamilyFundEvent.md) |
| `AllowSameSexCouple`, `ChildSurname` (father / mother / random) | 0 | FamilyEvent |
| `ContinueAsDescendant` | -10,000 (family on) | FamilyEvent |

**Cultivation paths** (section `smcfgs197`, 6.0): on / off on the left, how fast each path grows on the right. A gain
rate is added to x1 (-0.5 .. +2: half .. three times) and applied where the path gains (`SMGlobalConfigsEvent.Rate`):
`BodyCulEvent.AddExp`, `MeritEvent.Add` (merit only, not karma), `ArtisanEvent.AddExp`, `DaoHeartEvent.Add` (gains
only), `SpiritSenseEvent.AddExp`.

| Option | Score | Used by |
|---|---|---|
| `NoBodyCultivation` | +1,000 | [BodyCulEvent](BodyCulEvent.md) |
| `NoMerit` | +500 | [MeritEvent](MeritEvent.md) |
| `NoArtisan` | +500 | [ArtisanEvent](ArtisanEvent.md) |
| `NoDaoHeart` | +500 | [DaoHeartEvent](DaoHeartEvent.md) |
| `NoSpiritSense` | +500 | [SpiritSenseEvent](SpiritSenseEvent.md) |
| `BodyExpRate`, `MeritRate`, `ArtisanExpRate`, `DaoHeartRate`, `SpiritSenseRate` (-0.5 .. 2, default 0) | `-rate x 300` each (its path on) | the path |

**Estates** (section `smcfgs198`, 6.0):

| Option | Score | Used by |
|---|---|---|
| `NoEstate` | +500 | [EstateEvent](EstateEvent.md): no "Plots" button (town window, town management), NPCs buy no house (`EstateEvent.Enabled`); houses already owned stay |
| `NoNpcHouses` | 0 | EstateEvent: NPCs buy and upgrade no house (`EstateEvent.NpcHouses`) |
| `EstatePriceRate` (-0.5 .. 3, default 0) | `rate x 500` (estates on) | EstateEvent: land and building prices x (1 + rate) (`LandPrice`, `BuildCost`: also the upgrades and the mortal houses bought back) |
| `EstateTaxRate` (-1 .. 3, default 0) | `rate x 500` (estates on) | EstateEvent: house tax x (1 + rate) |

Page 3 shows a hint of the features under the sliders, wrapped by `WrapText` (about 80 Latin characters; a CJK
character counts as 2 and may be split between two CJK characters).

### Page 4 (NPC storylines)

| Option | Score | Used by |
|---|---|---|
| `NoStory` | +1,000 | StoryEvent (`ModMain/Story/`) |

One line per storyline A-P under "No NPC storylines" (`smcfgs169-174`, `smcfgs186-191`, `smcfgs208-211`), wrapped
by `WrapText` (about 140 Latin characters).

## Presets (right column)

- Total score (green >= 0, red < 0, `smcfgs043`) and the title of its tier.
- **Base** (`SetLevelBase`): every option to its default (battle map hidden).
- **Level 0 .. 10** (`SetLevel(n)`): sliders to a share of their range by level (e.g. monster Atk `n x 4 %`, HP `n x 5 %`, inflation `n x 5 %`, refine `n x 10 %`; GrowUpSpeed from 12 down to 2; NPC amount 800 .. 3,200), and toggles switched on by level: battle map hidden (>0), lifespan / money lost (>0), town build-up (>0), save hidden (>1), no battle exp (>2), portals only in cities (>3), monsters follow the player (>3), boss shield (>4), items lost (>4), no growth from battles (>5), reload hidden and auto-save after a loss (>6), no sect exchange (>7), no rebirth (>8), one life (>9). The additional features go back to their defaults.

## Flow

| Hook | What happens |
|---|---|
| `OnLoadClass` | builds `ScoreCalculator` (`Register(component, score, condition, formatter, onChange)`, `Object/SMItemWork.cs`) |
| `OnOpenUIEnd(Login)` | button "S&M Configs" in the title buttons -> `OpenSMConfigs()` (3 pages, navigation, OK = `SetSMConfigs`) |
| OK (`SetSMConfigs`) | `Score = CalSMTotalScore()`; every UI value copied into its property; saved globally; `NPCAmount` written into the game config `npcInitParameters` (item 1) |

`CalSMTotalScore()` sums the score of every enabled component whose condition holds; `CalCompScore(comp)`, `IsEnableComp(comp)`. `ShowTrainer(on)` (tests) sets the trainer toggle.

## Tests

TT-02.
