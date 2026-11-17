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
| `TimeSkipEvent` | Skip 1 / 10 years from the map (also seclusions). The month of the mods runs every `Stride` months (1 a year skipped: 12 runs whatever the length; each stands for N months: handlers scale by `ModMaster.MonthsPassed` / `YearsPassed` / `Ticks` / `ChanceOver`); 4 times (evenly spread) the month run of the game itself runs instead (NPCs cultivate, break through, marry, die; the game saves), every unit first aging by the months passed without one (up to life - 1: the game handles deaths); a drama (an enemy, a visitor, an ambush...) or a battle during the skip interrupts it after the month in progress (`Interrupted`; the run waits while the player answers); a month run still running 20 s after its save or after 2 min is considered over; the game loading window shows "n / N months"; `FullSimulation` runs the month of the game every month; `LastTiming` = timings of the last skip | OnOpenUIEnd(MapMain) | - | GrowUpSpeed | TG-24, TM-16, TX-07, TX-08, TX-09 |
| `HirePeopleEvent` | Teams: hire NPCs, monthly payment (friends serve for free; members who fight beside the player grow closer, see `BattleEvent`), friends team up, members follow the master | OnOpenUIEnd(TownBounty, NPCInfo), OnCloseUIStart, OnMonthlyForEachWUnit | TeamData | BattleConfigs | TG-09, TM-10 |
| `RebirthEvent` | Rebirth at the pub: reset grade for a permanent luck; monsters scale with total rebirth grades | OnOpenUIEnd(TownPub) | RebirthLevel, RebirthCount, TotalGradeLvl | NoRebirth | TG-22, TM-14 |
| `SliceLifeQuestEvent` | Quest chain: months without battle (levels unlocked by game year) | OnLoadGame, OnMonthly, OnBattleEndHandler | TaskLevel, LastCount | - | TG-21, TB-02, TX-04 |
| `ChallengeQuestEvent` | Quest chain: kill distinct higher-grade opponents | OnLoadGame, OnMonthly, OnBattleUnitDie, OnLoadNewGame | TaskLevel, CurrentWins, DefeatedOpponents | - | TG-21, TB-07 |
| `MissionDeclareEvent` | Commission items at the bounty board, delivered after some months | OnLoadGame, OnOpenUIEnd(TownBounty), OnMonthly | CommissionTasks, LastMonthCommission | MissionDeclareConfigs | TG-23, TM-09 |
| `RepairShinkiEvent` | Pay to repair Devil Demon / God Eye, add fish jade | OnOpenUIEnd | - | AllowRepairGodWeapon | TG-49 |

## Economy

| Event | What it does | Hooks | Saved | Switch | Tests |
|---|---|---|---|---|---|
| `InflationaryEvent` | Yearly world price index; currency reform above 10000 | OnLoadGame, OnYearly | Corruption, WorldPrice | InflationaryConfigs, AddInflationRate | TG-04, TM-20 |
| `MapBuildPropertyEvent` | Town/sect budgets, taxes (tax statistics of the month and the previous one in the town management window; x3 for a player blacklisted by the bank of the town), town masters/guardians, deposit/withdraw, upgrade to city | OnLoadGame, OnOpenUIEnd, OnMonthly(ForEachWUnit), OnYearly, OnBattleUnitDie | Budget, TaxRate, TownMasters... | NoTaxing, AddTaxRate | TG-10, TM-11 |
| `BankAccountEvent` | Storage needs an account per town (fee to the town budget) | OnOpenUIEnd(TownStorage) | RegisterdTown | BankAccountConfigs | TG-11, TM-03 |
| `BankLoanEvent` | Bank loans from the town budget (limit 2,000 x grade², 3 %/month, 12 months); overdue: +5 %, reminder letters, then debt collectors (pay / hand over money and items / fight); 2 beaten collectors: debt written off, blacklisted (no loan, account closed, taxes x3 there) | OnMonthly, OnOpenUIEnd(TownStorage), OnBattleEndHandler | Debt, DueMonth, OverdueMonths, CollectorsBeaten, Blacklisted | - | TG-57 |
| `WantedEvent` | Bounties paid from a town budget: town masters (the player from the NPC info, NPC masters yearly on hated units) post wanted notices; monthly hunters (chance x reward) fight NPC targets by battle power or ambush the player (fight / pay 1.5 x to clear the name); the player claims rewards by killing targets; expire after 36 months (refund); bounty board "Wanted list" | OnMonthly, OnYearly, OnOpenUIEnd(NPCInfo, TownBounty), OnBattleUnitDie, OnBattleEndHandler | Bounties | NoBounty | TG-59 |
| `SeclusionEvent` | Seclusion / enlightenment over years (time skip, world simulated; interrupted by a drama / battle: ends early, rewards of the months passed): offered (spirit vein 3-8 y, ancient inheritance 10-30 y, ancient tree 5 y; lifespan shown, red warning) and forced (heart demon 2-5 y, time rift 10-20 y; shortened to keep 1 year of lifespan); no encounters, loan penalty or hunters meanwhile; summary drama (exp, prices, friends who died, births) | OnMonthly, OnLoadGame | LastMonth, LastForcedMonth, Current | NoSeclusion, NoForcedSeclusion, SeclusionRate | TX-08 |
| `FamilyEvent` | NPC dao companions yearly (18+, chance by age / share of life lived, same area, not same blood, same-sex option); births (chance falls with age and grade, max 4); the child becomes a grade-1 NPC of 16 years 16 years later (surname father / mother / random, parents linked); "Family" window in NPC info; relatives who are town masters put a bounty on the player who kills a family member; continue as a grown-up child / grandchild when the player dies (old age, one-life): the player unit takes the heir's base properties (look, age, grade, reputation...), family, bonds, sect post and town seats; keeps its items, stones, acquired destinies and loans; techniques of the old player or of the heir (2nd option); intimacies: old friendships halved, enmities in full (`InheritIntim`); the old player's sect post, town seats and bounties end | OnYearly, OnMonthly, OnBattleUnitDie, OnOpenUIEnd(NPCInfo) | Children, Marriages, Generation, InheritDeclinedMonth | NoFamily, AllowSameSexCouple, ChildSurname, ContinueAsDescendant | TX-08 |
| `FamilyFundEvent` | Household of the player (companion, grown-up descendants, children not grown up): monthly upkeep by grade (grade² x 30, child 20, inflation) from the money then the bank storage stones, unpaid -> -5 intimacy each; gifts (1.5 %/month each: stones 2-4 months of upkeep, or an item of the grade 35 %) to the bank storage; 2 %/month a member takes a gifted item back; map button "Family" -> accounts window (NPC search: members, upkeep, year totals, last entries) | OnMonthly, OnOpenUIEnd(MapMain) | Ledger, GiftedItems | NoFamily | TX-10 |
| `StoryEvent` (`ModMain/Story/`) | NPC storylines A-F (Cold Sword, Old Huang's Debt, Exploding Furnace, Sworn Brothers, Herb Garden, Red Lotus final): at most one line opens a month (chance per line); roles cast from existing NPCs by sex / share of life lived / beauty / traits / grade / sect (`StoryRole`), or a new NPC with a chance growing 2 %/year up to 50 % (`StoryCasting`); acts of 3+ steps with time limits, late or failed acts open fail branches and other lines; dramas with the role portraits, real battles (`Fight`), spars, beasts; endings write `log_story_end`; yearly perks of finished lines (gifts, tea, A Li growing up, bounty refunds); a quest per open line in the Mission window (`story_taskBase.json` 444447011-016, side group): progress = act, description = the guide of the act with names and places (`story_task_<line><act>`, written at runtime into `story_task_desc_<line>`); a vital role dying closes or branches its line; Granny Shi's garden (line E, intimacy ≥ 300): a fixed point of the world map (where she met the player, outside towns) shown by a fruit tree icon (`story_worldFortuitousEventBase.json` 444447001, a map event without drama), quest "Granny Shi's garden" (`story_taskBase.json` 444447001, its `story_mapPosition.json` point set at runtime: the quest guide leads there); standing there opens the garden window -> seclusion 1-20 years (`SeclusionEvent.Kind.Garden`) | OnMonthly, OnTimeUpdate1000ms, OnBattleEnd | Lines (per line: act, step, roles, flags, ending) | NoStory | TS-01..TS-06 |
| `RealStorageEvent` | Monthly storage fee, debt blocks storage | OnOpenUIEnd(TownStorageProps), OnTimeUpdate200ms, OnMonthly | Debt | RealStorageConfigs, AddBankFee | TG-12, TM-04 |
| `BuildingArrangeEvent` | Towns build sub-buildings over time from their budget | OnLoadNewGame, OnMonthly | DefaultBuilding | AllowTownBuildupOverTime, OnlyPortalAtCityAndSect | TG-16, TM-12 |
| `RealMarketEvent` | Shop selling: monthly rate, limited by town budget | OnLoadGame, OnMonthly, OnOpenUIEnd(PropSell), OnTimeUpdate100ms | MarketPriceRate | RealMarketConfigs | TG-13, TM-05 |
| `RealMarketEvent2` | Shop buying: monthly rate, money goes to the town | OnLoadGame, OnMonthly, OnOpenUIEnd(TownMarketBuy, PropSelectCount) | MarketPriceRate | RealMarketConfigs | TG-13, TG-45 |
| `RealMarketEvent3` | Player market: listings, offers, negotiation (spirit stones and barter items), monthly deals; recent deal prices shown; new offers on the player listings notified; buy orders (held stones, filled by NPCs of the town, refunded after 12 months) (`RealMarketEvent3.Extra.cs`) | OnMonthly, OnOpenUIEnd, OnCloseUIStart | MarketStack, NegotiatingDeals, BoughtItems, PriceHistory, BuyOrders | MarketItemConfigs, NoMarketItem | TG-14, TG-46 |
| `ContributionExchangeEvent` | Exchange items for sect contribution | OnMonthly, OnOpenUIEnd(School), OnTimeUpdate200ms | CurMonthRatio | SectNoExchange | TG-15, TG-47 |

## Unit properties

| Event | What it does | Hooks | Saved | Tests |
|---|---|---|---|---|
| `CustomRefineEvent` | Refine equipment with spirit stones: refine levels and random adjustment slots (`CustomRefine`, slot n unlocks at level n x 100); the forge shows the progress and rerolls unlocked slots for spirit stones | OnMonthly(ForEachWUnit), OnOpenUIEnd(TownFactoryShapeOption) | RefineExp, CustomRefine | TG-08, TG-48 |
| `ExpertEvent` | Expertise exp per skill/step/ability/artifact from use | OnMonthlyForEachWUnit, OnBattleUnitHit, OnBattleUnitUseSkill, OnTimeUpdate200ms | ExpertExps | TG-07, TG-52 |
| `UnitModifyEvent` | Hooks all bonuses into unit DynInts (`GetAdjust*`) and battle cooldowns | OnLoadGame, OnMonthly(ForEachWUnit), OnBattleUnitInto | - (runtime cache) | TG-17 |

## Battle

| Event | What it does | Hooks | Tests |
|---|---|---|---|
| `BattleEvent` | NPCs around may join (enemies, friends, sect, town guards, stalkers: fight or pay them off); team members join/betray, members who fought beside the player gain intimacy; random drama variants (`ModTextHelper.RandomLS`) | OnBattleStart, OnBattleEndHandler, OnTimeUpdate1000ms | TB-07 |
| `BattleAfterEvent` | Stalkers after valuable drops | OnMonthly, OnBattleEndHandler | TB-07 |
| `MapBuildBattleEvent` | Town wars and monster waves; the player's own town attacked far away calls the player back (rush back and defend: +reputation on victory / trust the guardians) | OnMonthly, battle events, OnTimeUpdate1000ms | TM-23 |
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
| `UISettingScreenEvent` | Taoist settings (large window): hide damage multiplier, hide exception logs, market notifications, random encounters, Taoist log | TG-27, TM-27 |
| `AnimaWeaponEvent` | Several divine artifacts: buttons side by side on the map and in battles; a hotkey per artifact bound in Settings > Bindings (rows added to the combat and map lists); map: opens the artifact window, battle: uses its skill (global) | TB-16 |
| `TaoistLogEvent` | Taoist log of the save (taxes, fees, deals, buy orders, loans, encounters, stalkers...), shown in the current language from Taoist Settings | TG-27 |
| `UITitleScreenEvent` | Version button, Workshop description, ModLib version check | TT-01 |
| `UICreatePlayerEvent` | Prioritized destiny level at character creation | TM-02 |
| `UIDifficultyEvent` | Hides vanilla difficulty descriptions | TM-01 |
| `EncounterEvent` | Random monthly encounters (S&M EncounterRate, default 3 %) with choices and small rewards: wounded cultivator, chess hermit, hooded peddler, collapsed cave, spirit beast cub, town festival; NPCs get their own fortunes (0.2 %/year: exp) and ancient inheritances (0.02 %/year: exp, Talent, Luck), max 10 a year | TG-55, TX-08 |
| `NewGameEvent` | Character creation checkboxes (one line level with START GAME): starting divine artifacts (Bagua Jade, God Eye, Devil Demon pot; any combination) and skip the intro + first battle; choices kept globally, applied on the new map | TP-01 |

## Known issues

See [../Known-Issues.md](../Known-Issues.md) (BUG-01 ... BUG-21, with file/line, suggested fix and reproducing unit tests).
