using MOD_nE7UL2.Helper;
using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Object;
using ModLib.Attributes;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// S&amp;M (sadism &amp; masochism) difficulty configs, edited on the title screen and saved globally.
    /// Copied into each new save by SMLocalConfigsEvent. Each option has a score; the total gives the difficulty.
    /// </summary>
    [Cache(ModConst.SM_GLOBAL_CONFIGS_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
    public class SMGlobalConfigsEvent : ModEvent
    {
        public static SMGlobalConfigsEvent Instance { get; set; }

        public const string TITLE = "S&M Configs";

        [JsonIgnore]
        public UnitTypeEnum[] StartupPlayerRoles { get; } = {
            null,
            UnitTypeEnum.PowerUnit,
            UnitTypeEnum.SpeedUnit,
            UnitTypeEnum.TaoistUnit,
            UnitTypeEnum.AtkUnit,
            UnitTypeEnum.DefUnit,
            UnitTypeEnum.Angel,
            UnitTypeEnum.Evil,
            UnitTypeEnum.Merchant
        };

        //Configs
        public float AddAtkRate { get; set; } = 0f;
        public float AddDefRate { get; set; } = 0f;
        public float AddHpRate { get; set; } = 0f;
        public float AddBasisRate { get; set; } = 0f;
        public float AddSpecialMonsterRate { get; set; } = 0f;
        public float AddTaxRate { get; set; } = 0f;
        public float AddInflationRate { get; set; } = 0f;
        public float AddBuildingCostRate { get; set; } = 0f;
        public float AddBankAccountCostRate { get; set; } = 0f;
        public float AddBankFee { get; set; } = 0f;
        public float AddRefineCost { get; set; } = 0f;
        public float AddSectExchangeRate { get; set; } = 0f;
        public float AddNpcGrowRate { get; set; } = 0f;
        public float AddLevelupExpRate { get; set; } = 0f;
        public float AddItemValueRate { get; set; } = 0f;
        public bool HideSaveButton { get; set; } = false;
        public bool HideReloadButton { get; set; } = false;
        public bool HideBattleMap { get; set; } = false;
        public bool NoRebirth { get; set; } = false;
        public bool Onelife { get; set; } = false;
        public bool OnlyPortalAtCityAndSect { get; set; } = false;
        public bool NoExpFromBattles { get; set; } = false;
        public bool SectNoExchange { get; set; } = false;
        public bool BossHasShield { get; set; } = false;
        public bool NoGrowupFromBattles { get; set; } = false;
        public int PriorityDestinyLevel { get; set; } = 0;
        public bool AllowUpgradeNaturally { get; set; } = false;
        public bool EnableTrainer { get; set; } = false;
        public int NPCAmount { get; set; } = 2000;
        public bool AllowTownBuildupOverTime { get; set; } = false;
        public bool AllowTownBuildupOverTime_IncludeFirstTown { get; set; } = false;
        public int GrowUpSpeed { get; set; } = 3;
        public bool LostItemWhenDie { get; set; } = false;
        public bool MonsterPropertiesDependOnPlayerProperties { get; set; } = false;
        public bool AutoSaveAfterLostInBattle { get; set; } = false;
        public int StartupPlayerRole { get; set; } = 0;
        public float MarketItemNpcJoinRate { get; set; } = 0f;
        public float MarketItemGetAttackedRate { get; set; } = 0f;
        public bool NoMarketItem { get; set; } = false;
        public bool NoStalker { get; set; } = false;
        public bool NoTaxing { get; set; } = false;
        public bool NoQiCultivation { get; set; } = false;
        public bool NoBodyCultivation { get; set; } = false;
        public bool NoMerit { get; set; } = false;
        public bool NoArtisan { get; set; } = false;
        public bool NoDaoHeart { get; set; } = false;
        public bool NoSpiritSense { get; set; } = false;
        /// <summary>Gains of the cultivation paths, added to x1 (0: as designed; -0.5 half, 2 three times).</summary>
        public float BodyExpRate { get; set; } = 0f;
        public float MeritRate { get; set; } = 0f;
        public float ArtisanExpRate { get; set; } = 0f;
        public float DaoHeartRate { get; set; } = 0f;
        public float SpiritSenseRate { get; set; } = 0f;
        /// <summary>Estates (EstateEvent): no plots to buy; NPCs buy no houses; prices and house tax added to x1.</summary>
        public bool NoEstate { get; set; } = false;
        public bool NoNpcHouses { get; set; } = false;
        public float EstatePriceRate { get; set; } = 0f;
        public float EstateTaxRate { get; set; } = 0f;

        /// <summary>The configs of the game in progress (null outside a game: the defaults).</summary>
        public static SMGlobalConfigsEvent Current => SMLocalConfigsEvent.Instance?.Configs;
        /// <summary>A gain of a path x (1 + its rate), the rate read from the configs (0 without).</summary>
        public static double Rate(System.Func<SMGlobalConfigsEvent, float> rate) => Math.Max(0, 1 + (Current == null ? 0f : rate(Current)));
        // page 3 (features added later; "No..." so that saves made before keep them on)
        public bool NoEncounter { get; set; } = false;
        public bool NoBankLoan { get; set; } = false;
        public bool NoBuyOrder { get; set; } = false;
        public bool NoBounty { get; set; } = false;
        public bool NoSeclusion { get; set; } = false;
        public bool NoForcedSeclusion { get; set; } = false;
        public bool NoFamily { get; set; } = false;
        public bool AllowSameSexCouple { get; set; } = false;
        /// <summary>Surname of a child: 0 father, 1 mother, 2 random (FamilyEvent.Surname).</summary>
        public int ChildSurname { get; set; } = 0;
        /// <summary>When the player dies, play on as a grown-up child or grandchild (FamilyEvent).</summary>
        public bool ContinueAsDescendant { get; set; } = false;
        /// <summary>No NPC storylines (StoryEvent).</summary>
        public bool NoStory { get; set; } = false;
        /// <summary>Chance of a random encounter each month (%).</summary>
        public float EncounterRate { get; set; } = 3f;
        /// <summary>Total score (P) of these settings, worked out when the window is confirmed (SMScoreEvent: what P does).</summary>
        public int Score { get; set; } = 0;
        /// <summary>Chance of a seclusion offer each month (%); forced ones scale with it.</summary>
        public float SeclusionRate { get; set; } = 0.5f;
        public bool LostLifespanWhenDie { get; set; } = false;
        public bool LostMoneyWhenDie { get; set; } = true;
        public float MaxSellersEachTown { get; set; } = 8;
        public float MaxItemsOnSeller { get; set; } = 4;
        public float MaxBuyersOnSellingItem { get; set; } = 4;
        public bool OnlyActiveOnCurOrNearArea { get; set; } = true;
        public bool DestroyBuildingAfterTownWar { get; set; } = true;
        public bool AllowRepairGodWeapon { get; set; } = false;
        public bool AllFunctionsApplyToNearestUnits { get; set; } = false;

        //UI
        private UICustom1 uiCustom;
        private UIItemComposite slMonstAtk;
        private UIItemComposite slMonstDef;
        private UIItemComposite slMonstHp;
        private UIItemComposite slMonstBasis;
        private UIItemComposite slMonstSpecialRate;
        private UIItemComposite slEcoTaxRate;
        private UIItemComposite slEcoInfRate;
        private UIItemComposite slEcoBuildingCost;
        private UIItemComposite slEcoBankAccCost;
        private UIItemComposite slEcoBankFee;
        private UIItemComposite slEcoRefineCost;
        private UIItemComposite slEcoSectExchangeRate;
        private UIItemComposite slEcoItemValue;
        private UIItemComposite slNpcGrowRate;
        private UIItemComposite slMiscLevelupExp;
        private UIItemComposite tglSysHideSave;
        private UIItemComposite tglSysHideReload;
        private UIItemComposite tglSysHideBattleMap;
        private UIItemComposite tglSysNoRebirth;
        private UIItemComposite tglSysOnelife;
        private UIItemComposite tglSysOnlyPortalAtCityAndSect;
        private UIItemComposite tglSysNoExpFromBattle;
        private UIItemComposite tglSysSectNoExchange;
        private UIItemComposite tglSysBossHasShield;
        private UIItemComposite tglNoGrowupFromBattles;
        private UIItemComposite cbPriorityDestinyLevel;
        private UIItemComposite tglAllowUpgradeNaturally;
        private UIItemComposite tglEnableTrainer;
        private UIItemComposite slNPCAmount;
        private UIItemComposite tglAllowTownBuildupOverTime;
        private UIItemComposite tglAllowTownBuildupOverTime_IncludeFirstTown;
        private UIItemComposite slGrowUpSpeed;
        private UIItemComposite tglLostItemWhenDie;
        private UIItemComposite tglMonsterPropertiesDependOnPlayerProperties;
        private UIItemComposite tglAutoSaveAfterLostInBattle;
        private UIItemComposite cbStartupPlayerRole;
        private UIItemComposite slMarketItemNpcJoinRate;
        private UIItemComposite slMarketItemGetAttackedRate;
        private UIItemComposite tglNoMarketItem;
        private UIItemComposite tglNoStalker;
        private UIItemComposite tglNoTaxing;
        private UIItemComposite tglNoQiCultivation;
        private UIItemComposite tglNoBodyCultivation;
        private UIItemComposite tglNoMerit;
        private UIItemComposite tglNoArtisan;
        private UIItemComposite tglNoDaoHeart;
        private UIItemComposite tglNoSpiritSense;
        private UIItemComposite slBodyExpRate, slMeritRate, slArtisanExpRate, slDaoHeartRate, slSpiritSenseRate;
        private UIItemComposite tglNoEstate, tglNoNpcHouses, slEstatePriceRate, slEstateTaxRate;
        private UIItemComposite tglNoEncounter;
        private UIItemComposite tglNoBankLoan;
        private UIItemComposite tglNoBuyOrder;
        private UIItemComposite tglNoBounty;
        private UIItemComposite tglNoSeclusion;
        private UIItemComposite tglNoForcedSeclusion;
        private UIItemComposite tglNoFamily;
        private UIItemComposite tglAllowSameSexCouple;
        private UIItemComposite selChildSurname;
        private UIItemComposite tglContinueAsDescendant;
        private UIItemComposite tglNoStory;
        /// <summary>One line per NPC storyline under the "No NPC storylines" toggle (lines A-L).</summary>
        private static readonly string[] STORY_DESCRIPTIONS = { "smcfgs169", "smcfgs170", "smcfgs171", "smcfgs172", "smcfgs173", "smcfgs174",
            "smcfgs186", "smcfgs187", "smcfgs188", "smcfgs189", "smcfgs190", "smcfgs191",
            "smcfgs208", "smcfgs209", "smcfgs210", "smcfgs211" };
        /// <summary>
        /// Widths of the lines of page 3, in Latin characters: a storyline (a text box wraps itself at ~150 and the
        /// overflow covers the next row), the hint of the features (right of the toggles, left of the level buttons).
        /// </summary>
        private const int WRAP_STORY = 140, WRAP_HINT = 80;
        private UIItemComposite slEncounterRate;
        private UIItemComposite slSeclusionRate;
        private UIItemComposite tglLostLifespanWhenDie;
        private UIItemComposite tglLostMoneyWhenDie;
        private UIItemComposite slMaxSellersEachTown;
        private UIItemComposite slMaxItemsOnSeller;
        private UIItemComposite slMaxBuyersOnSellingItem;
        private UIItemComposite tglOnlyActiveOnCurOrNearArea;
        private UIItemComposite tglDestroyBuildingAfterTownWar;
        private UIItemComposite tglAllowRepairGodWeapon;
        private UIItemComposite tglAllFunctionsApplyToNearestUnits;

        //Score
        [JsonIgnore]
        public IList<SMItemWork> ScoreCalculator { get; } = new List<SMItemWork>();

        /// <summary>
        /// Builds the option/score registry.
        /// </summary>
        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);

            ScoreCalculator.Clear();
            Register(() => slMonstAtk,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstDef,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstHp,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstBasis,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstSpecialRate,
                funcCal: s => (s.Get().Parse<float>() * 3000).Parse<int>());
            Register(() => slEcoTaxRate,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoInfRate,
                funcCal: s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slEcoBuildingCost,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoBankAccCost,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoBankFee,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoRefineCost,
                funcCal: s => (s.Get().Parse<float>() * 2000).Parse<int>());
            Register(() => slEcoSectExchangeRate,
                funcCal: s => (s.Get().Parse<float>() * 2000).Parse<int>(),
                funcCond: s => !tglSysSectNoExchange.Get().Parse<bool>());
            Register(() => slEcoItemValue,
                funcCal: s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slNpcGrowRate,
                funcCal: s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slMiscLevelupExp,
                funcCal: s => (s.Get().Parse<float>() * 2000).Parse<int>());
            Register(() => tglSysHideSave,
                funcCal: s => 1000,
                funcCond: s => s.Get().Parse<bool>(),
                onChange: (s, v) =>
                {
                    var tgl = v.Parse<bool>();
                    tglSysHideReload.SetEnable(tgl);
                    tglSysHideReload.Set(false);
                });
            Register(() => tglSysHideReload,
                funcCal: s => 5000,
                funcCond: s => s.Get().Parse<bool>(),
                onChange: (s, v) =>
                {
                    var tgl = v.Parse<bool>();
                    tglSysOnelife.SetEnable(tgl);
                    tglSysOnelife.Set(false);
                });
            Register(() => tglSysHideBattleMap,
                funcCal: s => 2000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglSysNoRebirth,
                funcCal: s => 10000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglSysOnelife,
                funcCal: s => 20000,
                funcCond: s => s.Get().Parse<bool>(),
                onChange: (s, v) =>
                {
                    var tgl = v.Parse<bool>();
                    tglAutoSaveAfterLostInBattle.SetEnable(tgl);
                    tglAutoSaveAfterLostInBattle.Set(tgl);
                });
            Register(() => tglSysOnlyPortalAtCityAndSect, 
                funcCal: s => 1000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglSysNoExpFromBattle, 
                funcCal: s => 1000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglSysSectNoExchange, 
                funcCal: s => 10000,
                funcCond: s => s.Get().Parse<bool>(),
                onChange: (s, v) =>
                {
                    var tgl = v.Parse<bool>();
                    slEcoSectExchangeRate.SetEnable(!tgl);
                });
            Register(() => tglSysBossHasShield, 
                funcCal: s => 10000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoGrowupFromBattles, 
                funcCal: s => 10000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => cbPriorityDestinyLevel, 
                funcCal: s => (s.Get().Parse<int>() == 0 ? 0 : (3 - s.Get().Parse<int>())) * 1000);
            Register(() => tglAllowUpgradeNaturally, 
                funcCal: s => -2000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglEnableTrainer, 
                funcCal: s => -1000000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => slNPCAmount,
                funcCal: s => s.Get().Parse<int>(),
                funcFormatter: s =>
                {
                    var rs = new object[] { 0, 0 };
                    if (s.Parent != null)
                    {
                        //point
                        rs[0] = CalCompScore(s.Parent);
                        //value
                        var x = s.Parent as UIItemComposite;
                        if (x.MainComponent is UIItemSlider)
                        {
                            rs[1] = x.Get().Parse<int>();
                        }
                    }
                    return rs;
                });
            Register(() => tglAllowTownBuildupOverTime,
                funcCal: s => 1000,
                funcCond: s => s.Get().Parse<bool>(),
                onChange: (s, v) =>
                {
                    var tgl = v.Parse<bool>();
                    tglAllowTownBuildupOverTime_IncludeFirstTown.SetEnable(tgl);
                    tglAllowTownBuildupOverTime_IncludeFirstTown.Set(false);
                });
            Register(() => slGrowUpSpeed,
                funcCal: s => ((12 - s.Get().Parse<int>()) * 1000).Parse<int>(),
                funcFormatter: s =>
                {
                    var rs = new object[] { 0, 0 };
                    if (s.Parent != null)
                    {
                        //point
                        rs[0] = CalCompScore(s.Parent);
                        //value
                        var x = s.Parent as UIItemComposite;
                        if (x.MainComponent is UIItemSlider)
                        {
                            rs[1] = x.Get().Parse<int>();
                        }
                    }
                    return rs;
                });
            Register(() => tglLostItemWhenDie,
                funcCal: s => 10000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglAllowTownBuildupOverTime_IncludeFirstTown,
                funcCal: s => 1000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglMonsterPropertiesDependOnPlayerProperties,
                funcCal: s => 5000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglAutoSaveAfterLostInBattle,
                funcCal: s => 20000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => cbStartupPlayerRole,
                funcCal: s => s.Get().Parse<int>() == 0 ? 0 : -10000);
            Register(() => slMarketItemNpcJoinRate,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>(),
                funcCond: s => !tglNoMarketItem.Get().Parse<bool>());
            Register(() => slMarketItemGetAttackedRate,
                funcCal: s => (s.Get().Parse<float>() * 1000).Parse<int>(),
                funcCond: s => !tglNoMarketItem.Get().Parse<bool>());
            Register(() => tglNoMarketItem,
                funcCal: s => 10000,
                funcCond: s => s.Get().Parse<bool>(),
                onChange: (s, v) =>
                {
                    var tgl = v.Parse<bool>();
                    slMarketItemNpcJoinRate.SetEnable(!tgl);
                    slMarketItemGetAttackedRate.SetEnable(!tgl);
                });
            Register(() => tglNoStalker,
                funcCal: s => -3000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoTaxing,
                funcCal: s => -2000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoQiCultivation,
                funcCal: s => 2000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoBodyCultivation,
                funcCal: s => 1000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoMerit, funcCal: s => 500, funcCond: s => s.Get().Parse<bool>());               // merit helps more than karma hurts
            Register(() => tglNoArtisan, funcCal: s => 500, funcCond: s => s.Get().Parse<bool>());             // craft ranks and masterpieces help
            Register(() => tglNoDaoHeart, funcCal: s => 500, funcCond: s => s.Get().Parse<bool>());            // max SP / MP, heart demons
            Register(() => tglNoSpiritSense, funcCal: s => 500, funcCond: s => s.Get().Parse<bool>());         // max MP / SP, ambushes avoided
            // gains of the paths: faster is easier (-300 P a +100 %), slower harder; nothing when the path is off
            Register(() => slBodyExpRate, funcCal: s => (-s.Get().Parse<float>() * 300).Parse<int>(), funcCond: s => !tglNoBodyCultivation.Get().Parse<bool>());
            Register(() => slMeritRate, funcCal: s => (-s.Get().Parse<float>() * 300).Parse<int>(), funcCond: s => !tglNoMerit.Get().Parse<bool>());
            Register(() => slArtisanExpRate, funcCal: s => (-s.Get().Parse<float>() * 300).Parse<int>(), funcCond: s => !tglNoArtisan.Get().Parse<bool>());
            Register(() => slDaoHeartRate, funcCal: s => (-s.Get().Parse<float>() * 300).Parse<int>(), funcCond: s => !tglNoDaoHeart.Get().Parse<bool>());
            Register(() => slSpiritSenseRate, funcCal: s => (-s.Get().Parse<float>() * 300).Parse<int>(), funcCond: s => !tglNoSpiritSense.Get().Parse<bool>());
            // estates: no house (its rooms and bonuses) is harder; dearer houses and taxes harder
            Register(() => tglNoEstate, funcCal: s => 500, funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoNpcHouses, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            Register(() => slEstatePriceRate, funcCal: s => (s.Get().Parse<float>() * 500).Parse<int>(), funcCond: s => !tglNoEstate.Get().Parse<bool>());
            Register(() => slEstateTaxRate, funcCal: s => (s.Get().Parse<float>() * 500).Parse<int>(), funcCond: s => !tglNoEstate.Get().Parse<bool>());
            // additional features (page 3): turning off a feature that helps the player costs points (harder), turning
            // off one that hurts the player gives points back (easier); the rates count from their defaults
            Register(() => tglNoEncounter, funcCal: s => 1000, funcCond: s => s.Get().Parse<bool>());          // encounters reward more than they hurt
            Register(() => tglNoBankLoan, funcCal: s => 1000, funcCond: s => s.Get().Parse<bool>());           // no credit when short of stones
            Register(() => tglNoBuyOrder, funcCal: s => 500, funcCond: s => s.Get().Parse<bool>());            // a convenience of the market
            Register(() => tglNoBounty, funcCal: s => -2000, funcCond: s => s.Get().Parse<bool>());            // no hunters after the player
            Register(() => tglNoSeclusion, funcCal: s => 1000, funcCond: s => s.Get().Parse<bool>());          // no offered seclusions (exp), no forced ones
            Register(() => tglNoForcedSeclusion, funcCal: s => -2000,                                            // no heart demon / lost years
                funcCond: s => s.Get().Parse<bool>() && !tglNoSeclusion.Get().Parse<bool>());
            Register(() => tglNoFamily, funcCal: s => 2000, funcCond: s => s.Get().Parse<bool>());             // no children, no heir
            Register(() => tglAllowSameSexCouple, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());      // no effect on difficulty
            Register(() => selChildSurname, funcCal: s => 0, funcCond: s => false);                              // no effect on difficulty
            Register(() => tglContinueAsDescendant, funcCal: s => -10000,                                        // a second life (as No rebirth +10000)
                funcCond: s => s.Get().Parse<bool>() && !tglNoFamily.Get().Parse<bool>());
            Register(() => tglNoStory, funcCal: s => 1000, funcCond: s => s.Get().Parse<bool>());              // no storyline allies and rewards
            // rates in % a month, shown as they are ("3 %") with their points
            Func<UIItemBase, object[]> rateFormatter = s =>
            {
                var x = s.Parent as UIItemComposite;
                return new object[] { x == null ? 0 : CalCompScore(x), x == null ? "0" : x.Get().Parse<float>().ToString("0.0") + " %" };
            };
            // encounters: 3 % (default) = 0 P, 300 P a point below / above (0 % = +900, 10 % = -2100)
            Register(() => slEncounterRate, funcCal: s => ((3f - s.Get().Parse<float>()) * 300).Parse<int>(),
                funcCond: s => !tglNoEncounter.Get().Parse<bool>(), funcFormatter: rateFormatter);
            // seclusion events: 0.5 % (default) = 0 P, 2000 P a point (0 % = +1000, 3 % = -5000)
            Register(() => slSeclusionRate, funcCal: s => ((0.5f - s.Get().Parse<float>()) * 2000).Parse<int>(),
                funcCond: s => !tglNoSeclusion.Get().Parse<bool>(), funcFormatter: rateFormatter);
            Register(() => tglLostLifespanWhenDie,
                funcCal: s => 2000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglLostMoneyWhenDie,
                funcCal: s => 1000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => slMaxSellersEachTown,
                funcCal: s => 0,
                funcFormatter: s =>
                {
                    var rs = new object[] { 0 };
                    if (s.Parent != null)
                    {
                        //value
                        var x = s.Parent as UIItemComposite;
                        if (x.MainComponent is UIItemSlider)
                        {
                            rs[0] = x.Get().Parse<int>();
                        }
                    }
                    return rs;
                });
            Register(() => slMaxItemsOnSeller,
                funcCal: s => 0,
                funcFormatter: s =>
                {
                    var rs = new object[] { 0 };
                    if (s.Parent != null)
                    {
                        //value
                        var x = s.Parent as UIItemComposite;
                        if (x.MainComponent is UIItemSlider)
                        {
                            rs[0] = x.Get().Parse<int>();
                        }
                    }
                    return rs;
                });
            Register(() => slMaxBuyersOnSellingItem,
                funcCal: s => 0,
                funcFormatter: s =>
                {
                    var rs = new object[] { 0 };
                    if (s.Parent != null)
                    {
                        //value
                        var x = s.Parent as UIItemComposite;
                        if (x.MainComponent is UIItemSlider)
                        {
                            rs[0] = x.Get().Parse<int>();
                        }
                    }
                    return rs;
                });
            Register(() => tglOnlyActiveOnCurOrNearArea,
                funcCal: s => -5000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglDestroyBuildingAfterTownWar,
                funcCal: s => 2000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglAllowRepairGodWeapon,
                funcCal: s => -5000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglAllFunctionsApplyToNearestUnits,
                funcCal: s => -1000,
                funcCond: s => s.Get().Parse<bool>());
        }

        /// <summary>
        /// Registers one UI component with its score calculator and condition.
        /// </summary>
        private void Register(
            Func<UIItemBase> funcComp,
            Func<UIItemBase, int> funcCal,
            Func<UIItemBase, bool> funcCond = null,
            Func<UIItemBase, object[]> funcFormatter = null,
            Action<UIItemBase, object> onChange = null)
        {
            var formatter = funcFormatter ?? (s =>
            {
                var rs = new object[] { 0, 0 };
                if (s.Parent != null)
                {
                    //point
                    rs[0] = CalCompScore(s.Parent);
                    //%
                    var x = s.Parent as UIItemComposite;
                    if (x.MainComponent is UIItemSlider)
                    {
                        rs[1] = (x.Get().Parse<float>() * 100).Parse<int>().ToString("+#;-#;0");
                    }
                }
                return rs;
            });
            ScoreCalculator.Add(new SMItemWork
            {
                Comp = ActionHelper.TracedFunc(funcComp),
                Cal = ActionHelper.TracedFunc(funcCal),
                Cond = ActionHelper.TracedFunc(funcCond, true),
                Formatter = formatter != null ? ActionHelper.WTracedFunc(formatter) : null,
                ChangeAct = onChange != null ? ActionHelper.WTracedAction(onChange) : null,
            });
        }

        /// <summary>
        /// Adds the "S&amp;M Configs" button to the title screen.
        /// </summary>
        /// <summary>The S&amp;M button of the title screen (tests, label refresh).</summary>
        public static UIItemButton TitleButton { get; private set; }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var ui = new UICover<UILogin>(e.ui);
                {
                    var parentTransform = ui.UI.btnSet.transform.parent;
                    TitleButton = ui.AddButton(ui.MidCol, ui.FirstRow + 5, OpenSMConfigs, TITLE, ui.UI.btnSet);
                    TitleButton.Align(TextAnchor.MiddleCenter)
                        .Format(Color.black, 24)
                        .SetParentTransform(parentTransform);
                    UITitleScreenEvent.StripGameText(TitleButton);
                }
                ui.UpdateUI();
            }
        }

        // look of the window: section titles in the colour of the seal with an icon, the total score in colour
        public static Color SECTION_COLOR => new Color(0.45f, 0.12f, 0.08f);
        public static Color SCORE_POSITIVE => new Color(0.10f, 0.45f, 0.20f);
        public static Color SCORE_NEGATIVE => new Color(0.75f, 0.10f, 0.08f);
        private static readonly Dictionary<string, string> SECTION_ICONS = new Dictionary<string, string>
        {
            { "smcfgs000", "gongji" }, { "smcfgs016", "zm_renshu" }, { "smcfgs018", "xingyun" }, { "smcfgs104", "daodian" },
            { "smcfgs127", "zhandouxiaoguo" }, { "smcfgs007", "zm_lingshi" }, { "smcfgs116", "yaocai" }, { "smcfgs020", "jineng" },
            { "smcfgs150", "wuxing" }, { "smcfgs168", "shengwang" },
        };

        /// <summary>A section title: its icon and text.</summary>
        public static string SectionTitle(string key) =>
            (SECTION_ICONS.TryGetValue(key, out var icon) ? TextIconHelper.ToSprites($"[icon:{icon}]") + " " : string.Empty) + GameTool.LS(key);

        /// <summary>
        /// Opens the S&amp;M configs window (sliders/toggles for every option, presets, import/export).
        /// </summary>
        private void OpenSMConfigs()
        {
            uiCustom = new UICustom1(TITLE, string.Empty, SetSMConfigs, true);
            uiCustom.IsShowNavigationButtons = true;

            //page 1
            uiCustom.AddPage();
            {
                int col, row;

                uiCustom.AddToolTipButton(GameTool.LS("other500020039")).Pos(uiCustom.Columns[0], uiCustom.Rows[0] + UIHelper.GetUIDeltaY());   // above the section titles (they covered it)

                //
                col = 2; row = 0;
                uiCustom.AddText(col, row++, SectionTitle("smcfgs000")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                slMonstAtk = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs001"), -0.50f, 8.00f, AddAtkRate, GameTool.LS("smcfgs101"));
                slMonstDef = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs002"), -0.50f, 8.00f, AddDefRate, GameTool.LS("smcfgs101"));
                slMonstHp = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs003"), -0.50f, 8.00f, AddHpRate, GameTool.LS("smcfgs101"));
                slMonstBasis = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs004"), -0.50f, 8.00f, AddBasisRate, GameTool.LS("smcfgs101"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs005")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                slMonstSpecialRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs006"), -0.50f, 1.00f, AddSpecialMonsterRate, GameTool.LS("smcfgs101"));
                uiCustom.AddText(col + 2, row++, GameTool.LS("smcfgs049")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                tglMonsterPropertiesDependOnPlayerProperties = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs058"), MonsterPropertiesDependOnPlayerProperties, GameTool.LS("smcfgs102"));
                tglSysBossHasShield = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs038"), BossHasShield, GameTool.LS("smcfgs102"));
                //
                row++;
                uiCustom.AddText(col, row++, SectionTitle("smcfgs016")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                slNpcGrowRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs017"), 0.00f, 8.00f, AddNpcGrowRate, GameTool.LS("smcfgs101"));
                slNPCAmount = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs047"), 1000, 10000, NPCAmount, GameTool.LS("smcfgs103"));
                uiCustom.AddText(col + 3, row++, GameTool.LS("smcfgs048")).Format(null, 13).Align(TextAnchor.MiddleLeft).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((x) => new string[] { (slNPCAmount.Get().Parse<int>() / 2).ToString() }),
                });
                //
                row++;
                uiCustom.AddText(col, row++, SectionTitle("smcfgs018")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                slMiscLevelupExp = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs019"), 0.00f, 1.00f, AddLevelupExpRate, GameTool.LS("smcfgs101"));
                slGrowUpSpeed = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs051"), 1, 24, GrowUpSpeed, GameTool.LS("smcfgs053"));
                uiCustom.AddText(col + 2, row++, GameTool.LS("smcfgs052")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                uiCustom.AddText(col + 2, row++, GameTool.LS("smcfgs128")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                //
                row++;
                uiCustom.AddText(col, row++, SectionTitle("smcfgs104")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                uiCustom.AddToolTipButton(0, row, GameTool.LS("smcfgs105"));
                cbStartupPlayerRole = uiCustom.AddCompositeSelect(col, row++, GameTool.LS("smcfgs106"),
                    new string[] {
                        GameTool.LS("smcfgs107"),
                        GameTool.LS("smcfgs108"),
                        GameTool.LS("smcfgs109"),
                        GameTool.LS("smcfgs110"),
                        GameTool.LS("smcfgs111"),
                        GameTool.LS("smcfgs112"),
                        GameTool.LS("smcfgs113"),
                        GameTool.LS("smcfgs114"),
                        GameTool.LS("smcfgs115")
                    }, StartupPlayerRole, GameTool.LS("smcfgs102"));
                cbPriorityDestinyLevel = uiCustom.AddCompositeSelect(col, row++, GameTool.LS("smcfgs025"),
                    new string[] {
                        GameTool.LS("smcfgs026"),
                        GameTool.LS("smcfgs027"),
                        GameTool.LS("smcfgs028"),
                        GameTool.LS("smcfgs029"),
                        GameTool.LS("smcfgs030"),
                        GameTool.LS("smcfgs031"),
                        GameTool.LS("smcfgs032")
                    }, PriorityDestinyLevel, GameTool.LS("smcfgs102"));

                col = 16; row = 0;
                uiCustom.AddText(col, row++, SectionTitle("smcfgs127")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                tglSysHideSave = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs021"), HideSaveButton, GameTool.LS("smcfgs102"));
                tglSysHideReload = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs022"), HideReloadButton, GameTool.LS("smcfgs102"));
                tglLostItemWhenDie = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs056"), LostItemWhenDie, GameTool.LS("smcfgs102"));
                tglLostMoneyWhenDie = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs139"), LostMoneyWhenDie, GameTool.LS("smcfgs102"));
                tglLostLifespanWhenDie = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs129"), LostLifespanWhenDie, GameTool.LS("smcfgs102"));
                tglSysOnelife = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs023"), Onelife, GameTool.LS("smcfgs102"));
                tglAutoSaveAfterLostInBattle = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs059"), AutoSaveAfterLostInBattle, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs060")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                row++;
                tglEnableTrainer = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs041"), EnableTrainer, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs042")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                uiCustom.AddButton(col + 1, row + 1, ExportConfigs, GameTool.LS("smcfgs054")).Size(200, 40);
                uiCustom.AddButton(col + 1, row + 3, ImportConfigs, GameTool.LS("smcfgs055")).Size(200, 40);
                uiCustom.AddButton(col + 1, row + 5, OpenSaveDataFolder, GameTool.LS("smcfgs120")).Size(200, 40);
                uiCustom.AddButton(col + 1, row + 7, OpenModConfigs, GameTool.LS("smcfgs121")).Size(200, 40);
                row += 9;
                tglAllFunctionsApplyToNearestUnits = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs138"), AllFunctionsApplyToNearestUnits, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs135")).Format(null, 13).Align(TextAnchor.MiddleLeft);

                AddDifButtons();
            }

            //page 2
            uiCustom.AddPage();
            {
                int col, row;

                uiCustom.AddToolTipButton(GameTool.LS("other500020039")).Pos(uiCustom.Columns[0], uiCustom.Rows[0] + UIHelper.GetUIDeltaY());   // above the section titles (they covered it)

                col = 2; row = 0;
                //
                uiCustom.AddText(col, row++, SectionTitle("smcfgs007")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                slEcoTaxRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs008"), -0.50f, 5.00f, AddTaxRate, GameTool.LS("smcfgs101"));
                slEcoInfRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs009"), -0.50f, 2.00f, AddInflationRate, GameTool.LS("smcfgs101"));
                slEcoBuildingCost = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs010"), 0.00f, 8.00f, AddBuildingCostRate, GameTool.LS("smcfgs101"));
                slEcoBankAccCost = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs011"), 0.00f, 8.00f, AddBankAccountCostRate, GameTool.LS("smcfgs101"));
                slEcoBankFee = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs012"), 0.00f, 100.00f, AddBankFee, GameTool.LS("smcfgs101"));
                slEcoRefineCost = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs013"), -0.50f, 1.00f, AddRefineCost, GameTool.LS("smcfgs101"));
                slEcoSectExchangeRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs014"), -0.50f, 1.00f, AddSectExchangeRate, GameTool.LS("smcfgs101"));
                slEcoItemValue = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs015"), -0.50f, 1.00f, AddItemValueRate, GameTool.LS("smcfgs101"));
                //
                row++;
                uiCustom.AddText(col, row++, SectionTitle("smcfgs116")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                slMarketItemNpcJoinRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs117"), -0.50f, 1.00f, MarketItemNpcJoinRate, GameTool.LS("smcfgs101"));
                slMarketItemGetAttackedRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs118"), -0.50f, 1.00f, MarketItemGetAttackedRate, GameTool.LS("smcfgs101"));
                slMaxSellersEachTown = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs130"), 1, 30, MaxSellersEachTown, GameTool.LS("smcfgs132"));
                slMaxItemsOnSeller = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs133"), 1, 10, MaxItemsOnSeller, GameTool.LS("smcfgs132"));
                slMaxBuyersOnSellingItem = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs131"), 1, 20, MaxBuyersOnSellingItem, GameTool.LS("smcfgs132"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs119")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                tglOnlyActiveOnCurOrNearArea = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs134"), OnlyActiveOnCurOrNearArea, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs135")).Format(null, 13).Align(TextAnchor.MiddleLeft);

                col = 16; row = 0;
                uiCustom.AddText(col, row++, SectionTitle("smcfgs020")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                tglSysHideBattleMap = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs024"), HideBattleMap, GameTool.LS("smcfgs102"));
                tglDestroyBuildingAfterTownWar = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs136"), DestroyBuildingAfterTownWar, GameTool.LS("smcfgs102"));
                tglAllowTownBuildupOverTime = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs050"), AllowTownBuildupOverTime, GameTool.LS("smcfgs102"));
                tglAllowTownBuildupOverTime_IncludeFirstTown = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs057"), AllowTownBuildupOverTime_IncludeFirstTown, GameTool.LS("smcfgs102"));
                tglSysOnlyPortalAtCityAndSect = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs033"), OnlyPortalAtCityAndSect, GameTool.LS("smcfgs102"));
                row++;
                tglSysSectNoExchange = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs034"), SectNoExchange, GameTool.LS("smcfgs102"));
                tglSysNoRebirth = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs035"), NoRebirth, GameTool.LS("smcfgs102"));
                tglNoGrowupFromBattles = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs039"), NoGrowupFromBattles, GameTool.LS("smcfgs102"));
                tglSysNoExpFromBattle = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs040"), NoExpFromBattles, GameTool.LS("smcfgs102"));
                tglNoTaxing = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs122"), NoTaxing, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs126")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                tglNoMarketItem = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs123"), NoMarketItem, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs128")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                tglNoStalker = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs124"), NoStalker, GameTool.LS("smcfgs102"));
                tglNoQiCultivation = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs125"), NoQiCultivation, GameTool.LS("smcfgs102"));
                tglAllowRepairGodWeapon = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs137"), AllowRepairGodWeapon, GameTool.LS("smcfgs102"));
                tglAllowUpgradeNaturally = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs036"), AllowUpgradeNaturally, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs037")).Format(null, 13).Align(TextAnchor.MiddleLeft);

                AddDifButtons();
            }

            //page 3: features added later
            uiCustom.AddPage();
            {
                int col, row;

                uiCustom.AddToolTipButton(GameTool.LS("other500020039")).Pos(uiCustom.Columns[0], uiCustom.Rows[0] + UIHelper.GetUIDeltaY());   // above the section titles (they covered it)

                col = 2; row = 0;
                uiCustom.AddText(col, row++, SectionTitle("smcfgs150")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                tglNoEncounter = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs151"), NoEncounter, GameTool.LS("smcfgs102"));
                tglNoBankLoan = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs152"), NoBankLoan, GameTool.LS("smcfgs102"));
                tglNoBuyOrder = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs153"), NoBuyOrder, GameTool.LS("smcfgs102"));
                tglNoBounty = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs154"), NoBounty, GameTool.LS("smcfgs102"));
                tglNoSeclusion = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs156"), NoSeclusion, GameTool.LS("smcfgs102"));
                tglNoForcedSeclusion = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs157"), NoForcedSeclusion, GameTool.LS("smcfgs102"));
                tglNoFamily = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs158"), NoFamily, GameTool.LS("smcfgs102"));
                tglAllowSameSexCouple = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs159"), AllowSameSexCouple, GameTool.LS("smcfgs102"));
                selChildSurname = uiCustom.AddCompositeSelect(col, row++, GameTool.LS("smcfgs160"), new[] { GameTool.LS("smcfgs161"), GameTool.LS("smcfgs162"), GameTool.LS("smcfgs163") }, ChildSurname);
                tglContinueAsDescendant = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs164"), ContinueAsDescendant, GameTool.LS("smcfgs102"));
                // rates of the random events (second column)
                slEncounterRate = uiCustom.AddCompositeSlider(16, 1, GameTool.LS("smcfgs165"), 0f, 10f, EncounterRate, GameTool.LS("smcfgs103"));
                slSeclusionRate = uiCustom.AddCompositeSlider(16, 2, GameTool.LS("smcfgs166"), 0f, 3f, SeclusionRate, GameTool.LS("smcfgs103"));
                // the hint in the free space under the sliders
                var hintRow = 4;
                foreach (var line in WrapText(GameTool.LS("smcfgs155"), WRAP_HINT))
                    uiCustom.AddText(Math.Min(13, uiCustom.Columns.Count - 1), hintRow++, line).Format(null, 13).Align(TextAnchor.MiddleLeft);

                // the cultivation paths: on / off (left), how fast they grow (right)
                row++;
                var pathRow = row;
                uiCustom.AddText(col, row++, SectionTitle("smcfgs197")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                tglNoBodyCultivation = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs192"), NoBodyCultivation, GameTool.LS("smcfgs102"));
                tglNoMerit = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs193"), NoMerit, GameTool.LS("smcfgs102"));
                tglNoArtisan = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs194"), NoArtisan, GameTool.LS("smcfgs102"));
                tglNoDaoHeart = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs195"), NoDaoHeart, GameTool.LS("smcfgs102"));
                tglNoSpiritSense = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs196"), NoSpiritSense, GameTool.LS("smcfgs102"));
                var r2 = pathRow + 1;
                slBodyExpRate = uiCustom.AddCompositeSlider(16, r2++, GameTool.LS("smcfgs199"), -0.5f, 2f, BodyExpRate, GameTool.LS("smcfgs101"));
                slMeritRate = uiCustom.AddCompositeSlider(16, r2++, GameTool.LS("smcfgs200"), -0.5f, 2f, MeritRate, GameTool.LS("smcfgs101"));
                slArtisanExpRate = uiCustom.AddCompositeSlider(16, r2++, GameTool.LS("smcfgs201"), -0.5f, 2f, ArtisanExpRate, GameTool.LS("smcfgs101"));
                slDaoHeartRate = uiCustom.AddCompositeSlider(16, r2++, GameTool.LS("smcfgs202"), -0.5f, 2f, DaoHeartRate, GameTool.LS("smcfgs101"));
                slSpiritSenseRate = uiCustom.AddCompositeSlider(16, r2++, GameTool.LS("smcfgs203"), -0.5f, 2f, SpiritSenseRate, GameTool.LS("smcfgs101"));

                // estates: on / off and the NPCs (left), prices and taxes (right)
                row++;
                var estateRow = row;
                uiCustom.AddText(col, row++, SectionTitle("smcfgs198")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                tglNoEstate = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs204"), NoEstate, GameTool.LS("smcfgs102"));
                tglNoNpcHouses = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs205"), NoNpcHouses, GameTool.LS("smcfgs102"));
                slEstatePriceRate = uiCustom.AddCompositeSlider(16, estateRow + 1, GameTool.LS("smcfgs206"), -0.5f, 3f, EstatePriceRate, GameTool.LS("smcfgs101"));
                slEstateTaxRate = uiCustom.AddCompositeSlider(16, estateRow + 2, GameTool.LS("smcfgs207"), -1f, 3f, EstateTaxRate, GameTool.LS("smcfgs101"));

                AddDifButtons();
            }

            //page 4: NPC storylines, one toggle for all, a line per story
            uiCustom.AddPage();
            {
                int col = 2, row = 0;
                uiCustom.AddToolTipButton(GameTool.LS("other500020039")).Pos(uiCustom.Columns[0], uiCustom.Rows[0] + UIHelper.GetUIDeltaY());
                uiCustom.AddText(col, row++, SectionTitle("smcfgs168")).Format(SECTION_COLOR, 18, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                tglNoStory = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs167"), NoStory, GameTool.LS("smcfgs102"));
                row++;
                foreach (var key in STORY_DESCRIPTIONS)
                    foreach (var line in WrapText("- " + GameTool.LS(key), WRAP_STORY))
                        uiCustom.AddText(col - 1, row++, line).Format(null, 13).Align(TextAnchor.MiddleLeft);

                AddDifButtons();
            }

            SetWork();
            uiCustom.Pages[0].Active();
        }

        /// <summary>
        /// Splits a text into lines of about max Latin characters: at spaces, or between two CJK characters (Chinese,
        /// Japanese have no spaces); a CJK character counts as 2 (it is twice as wide).
        /// </summary>
        public static List<string> WrapText(string text, int max)
        {
            bool Wide(char c) => c >= 0x2E80 && c <= 0xFFEF;
            var lines = new List<string>();
            var cur = new StringBuilder();
            var width = 0;
            var lastBreak = -1;   // index in cur after which the line can break (a space or a CJK character)
            foreach (var c in text ?? string.Empty)
            {
                cur.Append(c);
                width += Wide(c) ? 2 : 1;
                if (c == ' ' || Wide(c))
                    lastBreak = cur.Length;
                if (width > max && lastBreak > 0 && lastBreak < cur.Length)
                {
                    lines.Add(cur.ToString(0, lastBreak).TrimEnd());
                    var rest = cur.ToString(lastBreak, cur.Length - lastBreak);
                    cur.Clear().Append(rest);
                    width = rest.Sum(x => Wide(x) ? 2 : 1);
                    lastBreak = -1;
                }
            }
            if (cur.ToString().Trim().Length > 0)
                lines.Add(cur.ToString().TrimEnd());
            return lines;
        }

        /// <summary>
        /// Writes the configs to a file.
        /// </summary>
        private void ExportConfigs()
        {
            CacheHelper.SaveGlobalCache(this);
            using (var dialog = new SaveFileDialog
            {
                RestoreDirectory = true,
                AddExtension = true,
                DefaultExt = "json",
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var stream = dialog.OpenFile())
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(File.ReadAllText(CacheHelper.GetGlobalCacheFilePath(ModId, CacheId)));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads configs from a file.
        /// </summary>
        private void ImportConfigs()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog
            {
                RestoreDirectory = true,
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var confPath = CacheHelper.GetGlobalCacheFilePath(ModId, CacheId);
                    if (FileHelper.IsReadable<SMGlobalConfigsEvent>(confPath))
                    {
                        uiCustom.Dispose();
                        File.Copy(dialog.FileName, confPath, true);
                        CacheHelper.ReloadGlobalCachableObject(this);
                    }
                    else
                    {
                        InfoDialog.Msg(GameTool.LS("ui_error"), GameTool.LS("ui_file_errors"));
                    }
                }
            }
        }

        /// <summary>
        /// Opens the mod cache folder.
        /// </summary>
        private void OpenSaveDataFolder()
        {
            Process.Start("explorer.exe", CacheHelper.GetCacheFolderName(ModId));
        }

        /// <summary>
        /// Opens the ModConf folder (mod_configs.json and the settings files of the features, *_configs.json).
        /// </summary>
        private void OpenModConfigs()
        {
            Process.Start("explorer.exe", ConfHelper.GetConfFolderPath(ModId));
        }

        /// <summary>
        /// Adds the difficulty preset buttons.
        /// </summary>
        private void AddDifButtons()
        {
            int col = Math.Min(30, uiCustom.Columns.Count - 1), row = 0;   // fewer columns in a small game window
            // the total score, green when positive, red when negative; the title of its tier (SMScoreEvent) under it
            // at the level of the title of the window (as the "?" buttons): the score, then the title of its tier
            var header = uiCustom.Rows[0] + UIHelper.GetUIDeltaY();
            var scoreText = uiCustom.AddText(col, row, "{0}").Format(Color.red, 19, FontStyle.Bold).Align(TextAnchor.MiddleRight);
            scoreText.SetWork(new UIItemWork
            {
                Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((x) =>
                {
                    var score = CalSMTotalScore();
                    var t = (UIItemText)x;
                    var color = score >= 0 ? SCORE_POSITIVE : SCORE_NEGATIVE;
                    if (t.Item.color != color) t.Item.color = color;
                    return new object[] { string.Format(GameTool.LS("smcfgs043"), score.ToString("#,##0")) };
                }),
            });
            scoreText.Pos(uiCustom.Columns[col], header + UIHelper.GetUIDeltaY() * 0.3f);
            // the title of the tier on its own line under the score (one line was too long: over the section titles)
            var tierText = uiCustom.AddText(col, row, "{0}").Format(SECTION_COLOR, 15, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            tierText.Pos(uiCustom.Columns[col], header - UIHelper.GetUIDeltaY() * 0.6f);
            tierText.SetWork(new UIItemWork
            {
                Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((x) =>
                {
                    var score = CalSMTotalScore();
                    return new object[] { TextIconHelper.ToSprites($"[icon:{SMScoreEvent.TIER_ICONS[SMScoreEvent.Tier(score)]}]") + " " + SMScoreEvent.TitleText(score) };
                }),
            });
            uiCustom.AddButton(col, row += 2, () => SetLevelBase(), GameTool.LS("smcfgs044"));
            uiCustom.AddButton(col, row += 2, () => SetLevel(0), $"{GameTool.LS("smcfgs045")} 0");
            uiCustom.AddButton(col, row += 2, () => SetLevel(1), $"{GameTool.LS("smcfgs045")} 1");
            uiCustom.AddButton(col, row += 2, () => SetLevel(2), $"{GameTool.LS("smcfgs045")} 2");
            uiCustom.AddButton(col, row += 2, () => SetLevel(3), $"{GameTool.LS("smcfgs045")} 3");
            uiCustom.AddButton(col, row += 2, () => SetLevel(4), $"{GameTool.LS("smcfgs045")} 4");
            uiCustom.AddButton(col, row += 2, () => SetLevel(5), $"{GameTool.LS("smcfgs045")} 5");
            uiCustom.AddButton(col, row += 2, () => SetLevel(6), $"{GameTool.LS("smcfgs045")} 6");
            uiCustom.AddButton(col, row += 2, () => SetLevel(7), $"{GameTool.LS("smcfgs045")} 7");
            uiCustom.AddButton(col, row += 2, () => SetLevel(8), $"{GameTool.LS("smcfgs045")} 8");
            uiCustom.AddButton(col, row += 2, () => SetLevel(9), $"{GameTool.LS("smcfgs045")} 9");
            uiCustom.AddButton(col, row += 2, () => SetLevel(10), $"{GameTool.LS("smcfgs045")} 10");
            uiCustom.AddText(uiCustom.MidCol, uiCustom.LastRow, GameTool.LS("smcfgs046")).Format(Color.red, 17);
        }

        /// <summary>
        /// Binds UI components to config properties.
        /// </summary>
        private void SetWork()
        {
            foreach (var wk in ScoreCalculator)
            {
                var item = wk.Comp.Invoke();
                item.ItemWork = wk;
            }
        }

        /// <summary>
        /// Resets every option to the base preset.
        /// </summary>
        private void SetLevelBase()
        {
            slMonstAtk.Set(0f);
            slMonstDef.Set(0f);
            slMonstHp.Set(0f);
            slMonstBasis.Set(0f);
            slMonstSpecialRate.Set(0f);
            slEcoTaxRate.Set(0f);
            slEcoInfRate.Set(0f);
            slEcoBuildingCost.Set(0f);
            slEcoBankAccCost.Set(0f);
            slEcoBankFee.Set(0f);
            slEcoRefineCost.Set(0f);
            slEcoSectExchangeRate.Set(0f);
            slEcoItemValue.Set(0f);
            slNpcGrowRate.Set(0f);
            slMiscLevelupExp.Set(0f);
            tglSysHideBattleMap.Set(true);
            tglSysHideSave.Set(false);
            tglSysNoExpFromBattle.Set(false);
            tglSysOnlyPortalAtCityAndSect.Set(false);
            tglSysBossHasShield.Set(false);
            tglSysHideReload.Set(false);
            tglSysSectNoExchange.Set(false);
            tglSysNoRebirth.Set(false);
            tglSysOnelife.Set(false);
            tglNoGrowupFromBattles.Set(false);
            cbPriorityDestinyLevel.Set(0);
            tglAllowUpgradeNaturally.Set(false);
            tglEnableTrainer.Set(false);
            slNPCAmount.Set(2000f);
            tglAllowTownBuildupOverTime.Set(false);
            tglAllowTownBuildupOverTime_IncludeFirstTown.Set(false);
            slGrowUpSpeed.Set(3f);
            tglLostItemWhenDie.Set(false);
            tglMonsterPropertiesDependOnPlayerProperties.Set(false);
            tglAutoSaveAfterLostInBattle.Set(false);
            cbStartupPlayerRole.Set(0);
            slMarketItemNpcJoinRate.Set(0f);
            slMarketItemGetAttackedRate.Set(0f);
            tglNoMarketItem.Set(false);
            tglNoStalker.Set(false);
            tglNoTaxing.Set(false);
            tglNoQiCultivation.Set(false);
            tglNoBodyCultivation.Set(false);
            tglNoMerit.Set(false);
            tglNoArtisan.Set(false);
            tglNoDaoHeart.Set(false);
            tglNoSpiritSense.Set(false);
            slBodyExpRate.Set(0f); slMeritRate.Set(0f); slArtisanExpRate.Set(0f); slDaoHeartRate.Set(0f); slSpiritSenseRate.Set(0f);
            tglNoEstate.Set(false); tglNoNpcHouses.Set(false); slEstatePriceRate.Set(0f); slEstateTaxRate.Set(0f);
            tglNoEncounter.Set(false);
            tglNoBankLoan.Set(false);
            tglNoBuyOrder.Set(false);
            tglNoBounty.Set(false);
            tglNoSeclusion.Set(false);
            tglNoForcedSeclusion.Set(false);
            tglNoFamily.Set(false);
            tglAllowSameSexCouple.Set(false);
            selChildSurname.Set(0);
            tglContinueAsDescendant.Set(false);
            tglNoStory.Set(false);
            slEncounterRate.Set(3f);
            slSeclusionRate.Set(0.5f);
            tglLostLifespanWhenDie.Set(false);
            tglLostMoneyWhenDie.Set(true);
            slMaxSellersEachTown.Set(8f);
            slMaxItemsOnSeller.Set(4f);
            slMaxBuyersOnSellingItem.Set(4f);
            tglOnlyActiveOnCurOrNearArea.Set(true);
            tglDestroyBuildingAfterTownWar.Set(true);
            tglAllowRepairGodWeapon.Set(false);
            tglAllFunctionsApplyToNearestUnits.Set(false);
        }

        /// <summary>
        /// Applies a difficulty preset.
        /// </summary>
        private void SetLevel(int level)
        {
            slMonstAtk.SetPercent(level * 0.04000f);
            slMonstDef.SetPercent(level * 0.01000f);
            slMonstHp.SetPercent(level * 0.05000f, 0f);
            slMonstBasis.SetPercent(level * 0.04000f);
            slMonstSpecialRate.SetPercent(level * 0.10000f);
            slEcoTaxRate.SetPercent(level * 0.00500f);
            slEcoInfRate.SetPercent(level * 0.05000f);
            slEcoBuildingCost.SetPercent(level * 0.04000f);
            slEcoBankAccCost.SetPercent(level * 0.05000f);
            slEcoBankFee.SetPercent(level * 0.01000f);
            slEcoRefineCost.SetPercent(level * 0.10000f);
            slEcoSectExchangeRate.SetPercent(level * 0.10000f);
            slEcoItemValue.SetPercent(level * 0.10000f);
            slNpcGrowRate.SetPercent(level * 0.02000f);
            slMiscLevelupExp.SetPercent(level * 0.10000f);
            tglAllowUpgradeNaturally.Set(false);
            tglEnableTrainer.Set(false);
            tglSysHideBattleMap.Set(level > 0);
            tglSysHideSave.Set(level > 1);
            tglSysNoExpFromBattle.Set(level > 2);
            tglSysOnlyPortalAtCityAndSect.Set(level > 3);
            tglSysBossHasShield.Set(level > 4);
            tglNoGrowupFromBattles.Set(level > 5);
            cbPriorityDestinyLevel.Set(0);
            tglSysHideReload.Set(level > 6);
            tglSysSectNoExchange.Set(level > 7);
            tglSysNoRebirth.Set(level > 8);
            tglSysOnelife.Set(level > 9);
            slNPCAmount.SetPercent(level * 0.05000f, 800f, 3200);
            tglAllowTownBuildupOverTime.Set(level > 0);
            tglAllowTownBuildupOverTime_IncludeFirstTown.Set(false);
            slGrowUpSpeed.SetPercent(1.0000f - (level * 0.10000f), 2, 12);
            tglLostItemWhenDie.Set(level > 4);
            tglMonsterPropertiesDependOnPlayerProperties.Set(level > 3);
            tglAutoSaveAfterLostInBattle.Set(level > 6);
            cbStartupPlayerRole.Set(0);
            slMarketItemNpcJoinRate.Set(0f);
            slMarketItemGetAttackedRate.Set(0f);
            tglNoMarketItem.Set(false);
            tglNoStalker.Set(false);
            tglNoTaxing.Set(false);
            tglNoQiCultivation.Set(false);
            tglNoBodyCultivation.Set(false);
            tglNoMerit.Set(false);
            tglNoArtisan.Set(false);
            tglNoDaoHeart.Set(false);
            tglNoSpiritSense.Set(false);
            slBodyExpRate.Set(0f); slMeritRate.Set(0f); slArtisanExpRate.Set(0f); slDaoHeartRate.Set(0f); slSpiritSenseRate.Set(0f);
            tglNoEstate.Set(false); tglNoNpcHouses.Set(false); slEstatePriceRate.Set(0f); slEstateTaxRate.Set(0f);
            tglNoEncounter.Set(false);
            tglNoBankLoan.Set(false);
            tglNoBuyOrder.Set(false);
            tglNoBounty.Set(false);
            tglNoSeclusion.Set(false);
            tglNoForcedSeclusion.Set(false);
            tglNoFamily.Set(false);
            tglAllowSameSexCouple.Set(false);
            selChildSurname.Set(0);
            tglContinueAsDescendant.Set(false);
            tglNoStory.Set(false);
            slEncounterRate.Set(3f);
            slSeclusionRate.Set(0.5f);
            tglLostLifespanWhenDie.Set(level > 0);
            slMaxSellersEachTown.Set(8f);
            slMaxItemsOnSeller.Set(4f);
            slMaxBuyersOnSellingItem.Set(4f);
            tglOnlyActiveOnCurOrNearArea.Set(true);
            tglDestroyBuildingAfterTownWar.Set(true);
            tglAllowRepairGodWeapon.Set(false);
            tglAllFunctionsApplyToNearestUnits.Set(false);
            tglLostMoneyWhenDie.Set(level > 0);
        }

        /// <summary>
        /// Copies the UI values into the config properties.
        /// </summary>
        private void SetSMConfigs()
        {
            Score = CalSMTotalScore();
            AddAtkRate = slMonstAtk.Get().Parse<float>();
            AddDefRate = slMonstDef.Get().Parse<float>();
            AddHpRate = slMonstHp.Get().Parse<float>();
            AddBasisRate = slMonstBasis.Get().Parse<float>();
            AddSpecialMonsterRate = slMonstSpecialRate.Get().Parse<float>();
            AddTaxRate = slEcoTaxRate.Get().Parse<float>();
            AddInflationRate = slEcoInfRate.Get().Parse<float>();
            AddBuildingCostRate = slEcoBuildingCost.Get().Parse<float>();
            AddBankAccountCostRate = slEcoBankAccCost.Get().Parse<float>();
            AddBankFee = slEcoBankFee.Get().Parse<float>();
            AddRefineCost = slEcoRefineCost.Get().Parse<float>();
            AddSectExchangeRate = slEcoSectExchangeRate.Get().Parse<float>();
            AddItemValueRate = slEcoItemValue.Get().Parse<float>();
            AddNpcGrowRate = slNpcGrowRate.Get().Parse<float>();
            AddLevelupExpRate = slMiscLevelupExp.Get().Parse<float>();
            HideSaveButton = tglSysHideSave.Get().Parse<bool>();
            HideReloadButton = tglSysHideReload.Get().Parse<bool>();
            HideBattleMap = tglSysHideBattleMap.Get().Parse<bool>();
            NoRebirth = tglSysNoRebirth.Get().Parse<bool>();
            Onelife = tglSysOnelife.Get().Parse<bool>();
            OnlyPortalAtCityAndSect = tglSysOnlyPortalAtCityAndSect.Get().Parse<bool>();
            NoExpFromBattles = tglSysNoExpFromBattle.Get().Parse<bool>();
            SectNoExchange = tglSysSectNoExchange.Get().Parse<bool>();
            BossHasShield = tglSysBossHasShield.Get().Parse<bool>();
            NoGrowupFromBattles = tglNoGrowupFromBattles.Get().Parse<bool>();
            PriorityDestinyLevel = cbPriorityDestinyLevel.Get().Parse<int>();
            AllowUpgradeNaturally = tglAllowUpgradeNaturally.Get().Parse<bool>();
            EnableTrainer = tglEnableTrainer.Get().Parse<bool>();
            NPCAmount = slNPCAmount.Get().Parse<int>();
            AllowTownBuildupOverTime = tglAllowTownBuildupOverTime.Get().Parse<bool>();
            AllowTownBuildupOverTime_IncludeFirstTown = tglAllowTownBuildupOverTime_IncludeFirstTown.Get().Parse<bool>();
            GrowUpSpeed = slGrowUpSpeed.Get().Parse<int>();
            LostItemWhenDie = tglLostItemWhenDie.Get().Parse<bool>();
            MonsterPropertiesDependOnPlayerProperties = tglMonsterPropertiesDependOnPlayerProperties.Get().Parse<bool>();
            AutoSaveAfterLostInBattle = tglAutoSaveAfterLostInBattle.Get().Parse<bool>();
            StartupPlayerRole = cbStartupPlayerRole.Get().Parse<int>();
            MarketItemNpcJoinRate = slMarketItemNpcJoinRate.Get().Parse<float>();
            MarketItemGetAttackedRate = slMarketItemGetAttackedRate.Get().Parse<float>();
            NoMarketItem = tglNoMarketItem.Get().Parse<bool>();
            NoStalker = tglNoStalker.Get().Parse<bool>();
            NoTaxing = tglNoTaxing.Get().Parse<bool>();
            NoQiCultivation = tglNoQiCultivation.Get().Parse<bool>();
            NoBodyCultivation = tglNoBodyCultivation.Get().Parse<bool>();
            NoMerit = tglNoMerit.Get().Parse<bool>();
            NoArtisan = tglNoArtisan.Get().Parse<bool>();
            NoDaoHeart = tglNoDaoHeart.Get().Parse<bool>();
            NoSpiritSense = tglNoSpiritSense.Get().Parse<bool>();
            BodyExpRate = slBodyExpRate.Get().Parse<float>();
            MeritRate = slMeritRate.Get().Parse<float>();
            ArtisanExpRate = slArtisanExpRate.Get().Parse<float>();
            DaoHeartRate = slDaoHeartRate.Get().Parse<float>();
            SpiritSenseRate = slSpiritSenseRate.Get().Parse<float>();
            NoEstate = tglNoEstate.Get().Parse<bool>();
            NoNpcHouses = tglNoNpcHouses.Get().Parse<bool>();
            EstatePriceRate = slEstatePriceRate.Get().Parse<float>();
            EstateTaxRate = slEstateTaxRate.Get().Parse<float>();
            NoEncounter = tglNoEncounter.Get().Parse<bool>();
            NoBankLoan = tglNoBankLoan.Get().Parse<bool>();
            NoBuyOrder = tglNoBuyOrder.Get().Parse<bool>();
            NoBounty = tglNoBounty.Get().Parse<bool>();
            NoSeclusion = tglNoSeclusion.Get().Parse<bool>();
            NoForcedSeclusion = tglNoForcedSeclusion.Get().Parse<bool>();
            NoFamily = tglNoFamily.Get().Parse<bool>();
            AllowSameSexCouple = tglAllowSameSexCouple.Get().Parse<bool>();
            ChildSurname = selChildSurname.Get().Parse<int>();
            ContinueAsDescendant = tglContinueAsDescendant.Get().Parse<bool>();
            NoStory = tglNoStory.Get().Parse<bool>();
            EncounterRate = slEncounterRate.Get().Parse<float>();
            SeclusionRate = slSeclusionRate.Get().Parse<float>();
            LostLifespanWhenDie = tglLostLifespanWhenDie.Get().Parse<bool>();
            MaxSellersEachTown = slMaxSellersEachTown.Get().Parse<int>();
            MaxItemsOnSeller = slMaxItemsOnSeller.Get().Parse<int>();
            MaxBuyersOnSellingItem = slMaxBuyersOnSellingItem.Get().Parse<int>();
            OnlyActiveOnCurOrNearArea = tglOnlyActiveOnCurOrNearArea.Get().Parse<bool>();
            DestroyBuildingAfterTownWar = tglDestroyBuildingAfterTownWar.Get().Parse<bool>();
            AllowRepairGodWeapon = tglAllowRepairGodWeapon.Get().Parse<bool>();
            AllFunctionsApplyToNearestUnits = tglAllFunctionsApplyToNearestUnits.Get().Parse<bool>();
            LostMoneyWhenDie = tglLostMoneyWhenDie.Get().Parse<bool>();
            CacheHelper.SaveGlobalCache(this);

            //edit conf
            g.conf.npcInitParameters.GetItem(1).value = NPCAmount;
        }

        /// <summary>The trainer toggle of the open window (promotion pictures: the score without the trainer; not saved
        /// unless the window is confirmed).</summary>
        public void ShowTrainer(bool on) => tglEnableTrainer?.Set(on);

        /// <summary>
        /// Score of one component.
        /// </summary>
        public static int CalCompScore(UIItemBase comp)
        {
            var x = Instance.ScoreCalculator.FirstOrDefault(k => k.Comp.Invoke() == comp);
            if (x == null || !x.Cond.Invoke(comp))
                return 0;
            return x.Cal.Invoke(comp);
        }

        /// <summary>
        /// True when the component is enabled.
        /// </summary>
        public static bool IsEnableComp(UIItemBase comp)
        {
            var x = Instance.ScoreCalculator.FirstOrDefault(k => k.Comp.Invoke() == comp);
            if (x == null)
                return false;
            return x != null && x.Cond.Invoke(comp) && comp.IsEnable();
        }

        /// <summary>
        /// Total difficulty score.
        /// </summary>
        public static int CalSMTotalScore()
        {
            var rs = 0;
            foreach (var item in Instance.ScoreCalculator)
            {
                var comp = item.Comp.Invoke();
                if (comp != null && IsEnableComp(comp))
                {
                    rs += CalCompScore(comp);
                }
            }
            return rs;
        }
    }
}
