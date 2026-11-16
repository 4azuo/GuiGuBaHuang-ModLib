# Taoist - Features (ModMain/Mod)

One row per event class. **Hooks** = overridden ModLib events. **Saved** = public properties persisted per save (Local cache) unless noted. **Switch** = S&M option (`SMLocalConfigsEvent.Configs`) or `mod_configs.json` section. Test ids refer to [03-Test-Scenarios](03-Test-Scenarios.md).

## Core / configuration

| Event | What it does | Hooks | Saved | Switch | Tests |
|---|---|---|---|---|---|
| `PrepareEvent` | Restricts the units used by Taoist each month to areas within +-3 of the player | OnRefreshParameterStoreOnMonthly | - | AllFunctionsApplyToNearestUnits | TG-03 |
| `SMGlobalConfigsEvent` (Global, title only) | "S&M Configs" window on the title screen: difficulty options with scores, presets, import/export | OnLoadClass, OnOpenUIEnd(Login) | all options (globally) | - | TT-02 |
| `SMLocalConfigsEvent` | Copy of the S&M configs taken when a save is created; applies AddLevelupExpRate; `Calculate()` | OnLoadClass, OnLoadGame | Configs | - | TG-02 |
| `TrainerEvent` | Cheat trainer, Ctrl+1/2/3 (stats, resources, teleport, god weapons, town master) | OnCacheHandler, OnMonoUpdate | - | EnableTrainer | TG-28, TG-51 |
| `AutoTestEvent` | In-game test runner, loaded only with an autotest request file | timers, battle | - (Global) | request.json | [04-Autotest](04-Autotest.md) |

## World and NPCs

| Event | What it does | Hooks | Saved | Switch | Tests |
|---|---|---|---|---|---|
| `NpcAutoEvent` | NPCs gain exp monthly and break through with a growing chance; one NPC born per year | OnMonthlyForEachWUnit, OnYearly | NpcUpGradeRate | AddNpcGrowRate | TG-06, TM-34 |
| `NpcUpgradeSkillEvent` | NPC martial arts level up / grade up, mastery exp, prefix upgrades | OnMonthlyForEachWUnit | - | NpcUpgradeSkillConfigs | TM-34 |
| `NpcAutoCraftArtifact` | NPCs randomly craft artifacts of their grade | OnLoadGame, OnMonthlyForEachWUnit | - | - | TM-34 |
| `UnitTypeEvent` | Unit roles (Merchant, Hero, SpeedUnit...), rank lucks (artisan, trainee, merchant), passive income, property growth | OnLoadGame, OnMonthlyForEachWUnit, OnYearly | UnitTypeDic | UnitTypeConfigs, StartupPlayerRole, GrowUpSpeed | TG-05 |
| `QiCulEvent` | Qi from SP when exp is full; Qi gives Atk/HP/MP; Qi transfer (receive from master once a year) | OnMonthlyForEachWUnit, OnOpenUIEnd(FateFeature, NPCInfo) | Qi, LastYearReceiveQi | NoQiCultivation | TG-17, TG-25, TM-19 |
| `UpgradeNaturallyEvent` | Break through without items when exp is full | OnOpenUIEnd(FateFeature) | - | AllowUpgradeNaturally | TG-25, TG-50 |
| `UseSpiritStoneCulEvent` | Pay spirit stones for exp | OnOpenUIEnd(FateFeature) | - | - | TG-25, TM-17 |
| `TimeSkipEvent` | Skip 1 / 10 years from the map | OnOpenUIEnd(MapMain) | - | GrowUpSpeed | TG-24, TM-16 |
| `HirePeopleEvent` | Teams: hire NPCs, monthly payment, friends team up, members follow the master | OnOpenUIEnd(TownBounty, NPCInfo), OnCloseUIStart, OnMonthlyForEachWUnit | TeamData | BattleConfigs | TG-09, TM-10 |
| `RebirthEvent` | Rebirth at the pub: reset grade for a permanent luck; monsters scale with total rebirth grades | OnOpenUIEnd(TownPub) | RebirthLevel, RebirthCount, TotalGradeLvl | NoRebirth | TG-22, TM-14 |
| `SliceLifeQuestEvent` | Quest chain: months without battle (levels unlocked by game year) | OnLoadGame, OnMonthly, OnBattleEndHandler | TaskLevel, LastCount | - | TG-21, TB-02, TX-04 |
| `ChallengeQuestEvent` | Quest chain: kill distinct higher-grade opponents | OnLoadGame, OnMonthly, OnBattleUnitDie, OnLoadNewGame | TaskLevel, CurrentWins, DefeatedOpponents | - | TG-21, TB-07 |
| `MissionDeclareEvent` | Commission items at the bounty board, delivered after some months | OnLoadGame, OnOpenUIEnd(TownBounty), OnMonthly | CommissionTasks, LastMonthCommission | MissionDeclareConfigs | TG-23, TM-09 |
| `RepairShinkiEvent` | Pay to repair Devil Demon / God Eye, add fish jade | OnOpenUIEnd | - | AllowRepairGodWeapon | TG-49 |

## Economy

| Event | What it does | Hooks | Saved | Switch | Tests |
|---|---|---|---|---|---|
| `InflationaryEvent` | Yearly world price index; currency reform above 10000 | OnLoadGame, OnYearly | Corruption, WorldPrice | InflationaryConfigs, AddInflationRate | TG-04, TM-20 |
| `MapBuildPropertyEvent` | Town/sect budgets, taxes, town masters/guardians, deposit/withdraw, upgrade to city | OnLoadGame, OnOpenUIEnd, OnMonthly(ForEachWUnit), OnYearly, OnBattleUnitDie | Budget, TaxRate, TownMasters... | NoTaxing, AddTaxRate | TG-10, TM-11 |
| `BankAccountEvent` | Storage needs an account per town (fee to the town budget) | OnOpenUIEnd(TownStorage) | RegisterdTown | BankAccountConfigs | TG-11, TM-03 |
| `RealStorageEvent` | Monthly storage fee, debt blocks storage | OnOpenUIEnd(TownStorageProps), OnTimeUpdate200ms, OnMonthly | Debt | RealStorageConfigs, AddBankFee | TG-12, TM-04 |
| `BuildingArrangeEvent` | Towns build sub-buildings over time from their budget | OnLoadNewGame, OnMonthly | DefaultBuilding | AllowTownBuildupOverTime, OnlyPortalAtCityAndSect | TG-16, TM-12 |
| `RealMarketEvent` | Shop selling: monthly rate, limited by town budget | OnLoadGame, OnMonthly, OnOpenUIEnd(PropSell), OnTimeUpdate100ms | MarketPriceRate | RealMarketConfigs | TG-13, TM-05 |
| `RealMarketEvent2` | Shop buying: monthly rate, money goes to the town | OnLoadGame, OnMonthly, OnOpenUIEnd(TownMarketBuy, PropSelectCount) | MarketPriceRate | RealMarketConfigs | TG-13, TG-45 |
| `RealMarketEvent3` | Player market: listings, offers, negotiation, monthly deals | OnMonthly, OnOpenUIEnd, OnCloseUIStart | MarketStack, NegotiatingDeals, BoughtItems | MarketItemConfigs, NoMarketItem | TG-14, TG-46 |
| `ContributionExchangeEvent` | Exchange items for sect contribution | OnMonthly, OnOpenUIEnd(School), OnTimeUpdate200ms | CurMonthRatio | SectNoExchange | TG-15, TG-47 |

## Unit properties

| Event | What it does | Hooks | Saved | Tests |
|---|---|---|---|---|
| `CustomRefineEvent` | Refine equipment with spirit stones: refine levels and random adjustment slots (`CustomRefine`) | OnMonthly(ForEachWUnit), OnOpenUIEnd(TownFactoryShapeOption) | RefineExp, CustomRefine | TG-08, TG-48 |
| `ExpertEvent` | Expertise exp per skill/step/ability/artifact from use | OnMonthlyForEachWUnit, OnBattleUnitHit, OnBattleUnitUseSkill, OnTimeUpdate200ms | ExpertExps | TG-07, TG-52 |
| `UnitModifyEvent` | Hooks all bonuses into unit DynInts (`GetAdjust*`) and battle cooldowns | OnLoadGame, OnMonthly(ForEachWUnit), OnBattleUnitInto | - (runtime cache) | TG-17 |

## Battle

| Event | What it does | Hooks | Tests |
|---|---|---|---|
| `BattleEvent` | NPCs around may join (enemies, friends, sect, town guards, stalkers); team members join/betray | OnBattleStart, OnBattleEndHandler, OnTimeUpdate1000ms | TB-07 |
| `BattleAfterEvent` | Stalkers after valuable drops | OnMonthly, OnBattleEndHandler | TB-07 |
| `MapBuildBattleEvent` | Town wars and monster waves | OnMonthly, battle events, OnTimeUpdate1000ms | TM-23 |
| `MonstStrongerEvent` | Monster scaling (time, kills, area, level, rebirth, S&M); summons/guardians scale with owner | OnLoadGame, OnMonthly, OnBattleUnitDie, OnBattleUnitInto | TG-20, TB-01, TB-06 |
| `SpecialMonsterEvent` | Shields, explosion, splitting | OnBattleUnitInto, OnBattleUnitDie, OnBattleEnd | TB-06 |
| `SpiritlockEvent` | Spiritlock circle balance | OnBattleUnitInto | TM-24 |
| `RealTrialEvent` | Lightning trial scaling and reward | OnBattleUnitInto, OnBattleEndHandler | TB-08 |
| `BattleModifyEvent` | Damage pipeline (nullify, evade, crit, block, mitigation, min dmg, time multiplier), MP costs, regen | OnOpenUIEnd(BattleInfo), OnBattleUnitInto, OnBattleUnitHitDynIntHandler, skill/step, OnTimeUpdate200ms | TG-18, TM-27 |
| `BattleManashieldEvent` | Mana shield for humans (and special monsters) | OnBattleUnitInto | TG-19, TM-28 |
| `BattleRewardEventV2` | Property growth by damage thresholds, exp, defeat penalties | OnLoadGame, OnBattleUnitHit, OnBattleEndHandler, OnBattleUnitDie | TB-02, TM-29 |

## UI

| Event | What it does | Tests |
|---|---|---|
| `UIEvent` | Hides some map widgets; shows Taoist info (artifact/expert/refine) in item/martial UIs; hides save/reload buttons | TG-24, TG-26, TG-48 |
| `UISettingScreenEvent` | Taoist settings (hide damage multiplier, hide exception logs) in game settings | TG-27, TM-27 |
| `UITitleScreenEvent` | Version button, Workshop description, ModLib version check | TT-01 |
| `UICreatePlayerEvent` | Prioritized destiny level at character creation | TM-02 |
| `UIDifficultyEvent` | Hides vanilla difficulty descriptions | TM-01 |
| `NewGameEvent` | Character creation checkboxes: starting divine artifacts (Bagua Jade, God Eye, Devil Demon pot; any combination) and skip the intro + first battle; choices kept globally, applied on the new map | TP-01 |

## Known issues

See [../Known-Issues.md](../Known-Issues.md) (BUG-01 ... BUG-17, with file/line, suggested fix and reproducing unit tests).
