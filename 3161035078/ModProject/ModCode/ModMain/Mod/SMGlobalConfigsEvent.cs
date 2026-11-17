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
        /// <summary>One line per NPC storyline under the "No NPC storylines" toggle (lines A-F).</summary>
        private static readonly string[] STORY_DESCRIPTIONS = { "smcfgs169", "smcfgs170", "smcfgs171", "smcfgs172", "smcfgs173", "smcfgs174" };
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
            // optional content: no difficulty points
            Register(() => tglNoEncounter, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoBankLoan, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoBuyOrder, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoBounty, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoSeclusion, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoForcedSeclusion, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoFamily, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            Register(() => tglAllowSameSexCouple, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            Register(() => selChildSurname, funcCal: s => 0, funcCond: s => false);
            Register(() => tglContinueAsDescendant, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            Register(() => tglNoStory, funcCal: s => 0, funcCond: s => s.Get().Parse<bool>());
            // rates in % a month, shown as they are ("3 %")
            Func<UIItemBase, object[]> rateFormatter = s =>
            {
                var x = s.Parent as UIItemComposite;
                return new object[] { 0, x == null ? "0" : x.Get().Parse<float>().ToString("0.0") + " %" };
            };
            Register(() => slEncounterRate, funcCal: s => 0, funcCond: s => false, funcFormatter: rateFormatter);
            Register(() => slSeclusionRate, funcCal: s => 0, funcCond: s => false, funcFormatter: rateFormatter);
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
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var ui = new UICover<UILogin>(e.ui);
                {
                    var parentTransform = ui.UI.btnSet.transform.parent;
                    ui.AddButton(ui.MidCol, ui.FirstRow + 5, OpenSMConfigs, TITLE, ui.UI.btnSet)
                        .Align(TextAnchor.MiddleCenter)
                        .Format(Color.black, 24)
                        .SetParentTransform(parentTransform);
                }
                ui.UpdateUI();
            }
        }

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

                uiCustom.AddToolTipButton(GameTool.LS("other500020039"));

                //
                col = 2; row = 0;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs000")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
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
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs016")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
                slNpcGrowRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs017"), 0.00f, 8.00f, AddNpcGrowRate, GameTool.LS("smcfgs101"));
                slNPCAmount = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs047"), 1000, 10000, NPCAmount, GameTool.LS("smcfgs103"));
                uiCustom.AddText(col + 3, row++, GameTool.LS("smcfgs048")).Format(null, 13).Align(TextAnchor.MiddleLeft).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((x) => new string[] { (slNPCAmount.Get().Parse<int>() / 2).ToString() }),
                });
                //
                row++;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs018")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
                slMiscLevelupExp = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs019"), 0.00f, 1.00f, AddLevelupExpRate, GameTool.LS("smcfgs101"));
                slGrowUpSpeed = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs051"), 1, 24, GrowUpSpeed, GameTool.LS("smcfgs053"));
                uiCustom.AddText(col + 2, row++, GameTool.LS("smcfgs052")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                uiCustom.AddText(col + 2, row++, GameTool.LS("smcfgs128")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                //
                row++;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs104")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
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
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs127")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
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

                uiCustom.AddToolTipButton(GameTool.LS("other500020039"));

                col = 2; row = 0;
                //
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs007")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
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
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs116")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
                slMarketItemNpcJoinRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs117"), -0.50f, 1.00f, MarketItemNpcJoinRate, GameTool.LS("smcfgs101"));
                slMarketItemGetAttackedRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs118"), -0.50f, 1.00f, MarketItemGetAttackedRate, GameTool.LS("smcfgs101"));
                slMaxSellersEachTown = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs130"), 1, 30, MaxSellersEachTown, GameTool.LS("smcfgs132"));
                slMaxItemsOnSeller = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs133"), 1, 10, MaxItemsOnSeller, GameTool.LS("smcfgs132"));
                slMaxBuyersOnSellingItem = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs131"), 1, 20, MaxBuyersOnSellingItem, GameTool.LS("smcfgs132"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs119")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                tglOnlyActiveOnCurOrNearArea = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs134"), OnlyActiveOnCurOrNearArea, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs135")).Format(null, 13).Align(TextAnchor.MiddleLeft);

                col = 16; row = 0;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs020")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
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
                row++;
                tglAllowRepairGodWeapon = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs137"), AllowRepairGodWeapon, GameTool.LS("smcfgs102"));
                tglAllowUpgradeNaturally = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs036"), AllowUpgradeNaturally, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs037")).Format(null, 13).Align(TextAnchor.MiddleLeft);

                AddDifButtons();
            }

            //page 3: features added later
            uiCustom.AddPage();
            {
                int col, row;

                uiCustom.AddToolTipButton(GameTool.LS("other500020039"));

                col = 2; row = 0;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs150")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
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
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs155")).Format(null, 13).Align(TextAnchor.MiddleLeft);

                // NPC storylines: one toggle for all, a line per story
                row++;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs168")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
                tglNoStory = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs167"), NoStory, GameTool.LS("smcfgs102"));
                for (var i = 0; i < STORY_DESCRIPTIONS.Length; i++)
                    uiCustom.AddText(col - 1, row++, "- " + GameTool.LS(STORY_DESCRIPTIONS[i])).Format(null, 13).Align(TextAnchor.MiddleLeft);

                AddDifButtons();
            }

            SetWork();
            uiCustom.Pages[0].Active();
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
                        g.ui.MsgBox(GameTool.LS("ui_error"), GameTool.LS("ui_file_errors"));
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
        /// Opens mod_configs.json.
        /// </summary>
        private void OpenModConfigs()
        {
            Process.Start("notepad.exe", ConfHelper.GetConfFilePath(ModId, "mod_configs.json"));
        }

        /// <summary>
        /// Adds the difficulty preset buttons.
        /// </summary>
        private void AddDifButtons()
        {
            int col = 30, row = 0;
            uiCustom.AddText(col, row, GameTool.LS("smcfgs043")).Format(Color.red, 17).Align(TextAnchor.MiddleRight).SetWork(new UIItemWork
            {
                Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((x) => new string[] { CalSMTotalScore().ToString() }),
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
