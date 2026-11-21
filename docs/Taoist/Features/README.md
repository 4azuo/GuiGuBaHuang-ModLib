# Taoist - one page per feature class

Detailed documentation of every class in `3161035078/ModProject/ModCode/ModMain/Mod/`: what it does, saved data, switches (S&M options, `ModConf/<feature>_configs.json`), hooks and flow, formulas and numbers, UI, API, notes and tests. The summary table is [01-Features](../01-Features.md); S&M options with their scores are in [SMGlobalConfigsEvent](SMGlobalConfigsEvent.md).

Each page follows the same order: file and cache key, summary, **Saved data**, **Switches**, **Flow** (hooks), formulas / rules, **UI**, **API**, **Notes**, **Tests**. Keep it in sync when the code changes (numbers come straight from the code and the `ModConf/*_configs.json` files).

## Core / configuration

[PrepareEvent](PrepareEvent.md) - [SMGlobalConfigsEvent](SMGlobalConfigsEvent.md) - [SMLocalConfigsEvent](SMLocalConfigsEvent.md) - [TrainerEvent](TrainerEvent.md) - [AutoTestEvent](AutoTestEvent.md)

## World and NPCs

[NpcAutoEvent](NpcAutoEvent.md) - [NpcUpgradeSkillEvent](NpcUpgradeSkillEvent.md) - [NpcAutoCraftArtifact](NpcAutoCraftArtifact.md) - [UnitTypeEvent](UnitTypeEvent.md) - [QiCulEvent](QiCulEvent.md) - [BodyCulEvent](BodyCulEvent.md) - [MeritEvent](MeritEvent.md) - [ArtisanEvent](ArtisanEvent.md) - [DaoHeartEvent](DaoHeartEvent.md) - [SpiritSenseEvent](SpiritSenseEvent.md) - [UpgradeNaturallyEvent](UpgradeNaturallyEvent.md) - [UseSpiritStoneCulEvent](UseSpiritStoneCulEvent.md) - [TimeSkipEvent](TimeSkipEvent.md) - [HirePeopleEvent](HirePeopleEvent.md) - [RebirthEvent](RebirthEvent.md) - [SliceLifeQuestEvent](SliceLifeQuestEvent.md) - [ChallengeQuestEvent](ChallengeQuestEvent.md) - [MissionDeclareEvent](MissionDeclareEvent.md) - [RepairShinkiEvent](RepairShinkiEvent.md) - [EncounterEvent](EncounterEvent.md) - [SeclusionEvent](SeclusionEvent.md) - [FamilyEvent](FamilyEvent.md)

## Economy

[InflationaryEvent](InflationaryEvent.md) - [MapBuildPropertyEvent](MapBuildPropertyEvent.md) - [EstateEvent](EstateEvent.md) - [SpiritNoteEvent](SpiritNoteEvent.md) - [BankAccountEvent](BankAccountEvent.md) - [BankLoanEvent](BankLoanEvent.md) - [WantedEvent](WantedEvent.md) - [ClanEvent](ClanEvent.md) - [FamilyFundEvent](FamilyFundEvent.md) - [RealStorageEvent](RealStorageEvent.md) - [BuildingArrangeEvent](BuildingArrangeEvent.md) - [RealMarketEvent](RealMarketEvent.md) - [RealMarketEvent2](RealMarketEvent2.md) - [RealMarketEvent3](RealMarketEvent3.md) - [ContributionExchangeEvent](ContributionExchangeEvent.md)

## Unit properties

[CustomRefineEvent](CustomRefineEvent.md) - [ExpertEvent](ExpertEvent.md) - [UnitModifyEvent](UnitModifyEvent.md)

## Battle

[BattleEvent](BattleEvent.md) - [BattleAfterEvent](BattleAfterEvent.md) - [MapBuildBattleEvent](MapBuildBattleEvent.md) - [MonstStrongerEvent](MonstStrongerEvent.md) - [SpecialMonsterEvent](SpecialMonsterEvent.md) - [SpiritlockEvent](SpiritlockEvent.md) - [RealTrialEvent](RealTrialEvent.md) - [BattleModifyEvent](BattleModifyEvent.md) - [DamageInfo](DamageInfo.md) - [SMScoreEvent](SMScoreEvent.md) - [BattleManashieldEvent](BattleManashieldEvent.md) - [BattleRewardEventV2](BattleRewardEventV2.md)

## UI

[UIEvent](UIEvent.md) - [UISettingScreenEvent](UISettingScreenEvent.md) - [AnimaWeaponEvent](AnimaWeaponEvent.md) - [TaoistLogEvent](TaoistLogEvent.md) - [UITitleScreenEvent](UITitleScreenEvent.md) - [UICreatePlayerEvent](UICreatePlayerEvent.md) - [UIDifficultyEvent](UIDifficultyEvent.md) - [NewGameEvent](NewGameEvent.md)

Storylines (`ModMain/Story/`, not in `Mod/`): [06-Storylines-Draft](../06-Storylines-Draft.md) and [Storylines/](../Storylines/).

## Points found while writing these pages

Fixed on 2026-11-19:

- [BattleEvent](BattleEvent.md): the sect-member and town-guard joins required the arena dungeon, which `IsJoinableBattle()` excludes, so they never happened.
- [RealMarketEvent](RealMarketEvent.md) / [RealMarketEvent2](RealMarketEvent2.md): the Merchant role and rank bonuses (fractions such as 0.10) were added to rates in percent (+0.1 % instead of +10 %).
- [MapBuildBattleEvent](MapBuildBattleEvent.md): escaping a town war / monster wave added reputation instead of removing it.
- [NpcAutoCraftArtifact](NpcAutoCraftArtifact.md): NPCs crafted artifacts without limit; now never one of a kind and grade they already own.

Still open (see the page):

- [SMGlobalConfigsEvent](SMGlobalConfigsEvent.md): `NPCAmount` is written into the game config only when the S&M window is confirmed, so after a game restart a new world uses the game's default until the window is confirmed again.
