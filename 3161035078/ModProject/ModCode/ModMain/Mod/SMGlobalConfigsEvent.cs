using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Object;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SM_GLOBAL_CONFIGS_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
    public class SMGlobalConfigsEvent : ModEvent
    {
        public static SMGlobalConfigsEvent Instance { get; set; }

        public const string TITLE = "S&M Configs";

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

        //Score
        public static IList<SMItemWork> ScoreCalculator { get; } = new List<SMItemWork>();

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
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>(),
                funcEna: s => tglAllowTownBuildupOverTime.Get().Parse<bool>());
            Register(() => slEcoBankAccCost,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoBankFee,
                funcCal: s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoRefineCost,
                funcCal: s => (s.Get().Parse<float>() * 2000).Parse<int>());
            Register(() => slEcoSectExchangeRate,
                funcCal: s => (s.Get().Parse<float>() * 2000).Parse<int>(),
                funcCond: s => !tglSysSectNoExchange.Get().Parse<bool>(),
                funcEna: s => !tglSysSectNoExchange.Get().Parse<bool>());
            Register(() => slEcoItemValue,
                funcCal: s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slNpcGrowRate,
                funcCal: s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slMiscLevelupExp,
                funcCal: s => (s.Get().Parse<float>() * 2000).Parse<int>());
            Register(() => tglSysHideSave,
                funcCal: s => 1000,
                funcCond: s => s.Get().Parse<bool>(),
                onChange: (s, v) => tglSysHideReload.Set(false));
            Register(() => tglSysHideReload,
                funcCal: s => 5000,
                funcCond: s => s.Get().Parse<bool>(),
                funcEna: s => tglSysHideSave.Get().Parse<bool>(),
                onChange: (s, v) => tglSysOnelife.Set(false));
            Register(() => tglSysHideBattleMap,
                funcCal: s => 2000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglSysNoRebirth,
                funcCal: s => 10000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglSysOnelife,
                funcCal: s => 20000,
                funcCond: s => s.Get().Parse<bool>(),
                funcEna: s => tglSysHideReload.Get().Parse<bool>(),
                onChange: (s, v) => { if (v.Parse<bool>()) tglAutoSaveAfterLostInBattle.Set(true); });
            Register(() => tglSysOnlyPortalAtCityAndSect, 
                funcCal: s => 1000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglSysNoExpFromBattle, 
                funcCal: s => 1000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglSysSectNoExchange, 
                funcCal: s => 10000,
                funcCond: s => s.Get().Parse<bool>());
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
                funcCond: s => s.Get().Parse<bool>());
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
                funcCond: s => s.Get().Parse<bool>(),
                funcEna: s => tglAllowTownBuildupOverTime.Get().Parse<bool>());
            Register(() => tglMonsterPropertiesDependOnPlayerProperties,
                funcCal: s => 5000,
                funcCond: s => s.Get().Parse<bool>());
            Register(() => tglAutoSaveAfterLostInBattle,
                funcCal: s => 20000,
                funcCond: s => s.Get().Parse<bool>());
        }

        private void Register(
            Func<UIItemBase> funcComp, 
            Func<UIItemBase, int> funcCal, 
            Func<UIItemBase, bool> funcCond = null,
            Func<UIItemBase, bool> funcEna = null,
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
                Comp = funcComp,
                Cal = funcCal,
                Cond = funcCond ?? (s => true),
                EnableAct = funcEna,
                Formatter = formatter,
                ChangeAct = onChange,
            });
        }

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

        private void OpenSMConfigs()
        {
            uiCustom = new UICustom1(TITLE, string.Empty, SetSMConfigs, true);
            {
                int col, row;

                uiCustom.AddToolTipButton(GameTool.LS("other500020039"));

                col = 2; row = 0;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs000")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
                slMonstAtk = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs001"), -0.50f, 8.00f, AddAtkRate, GameTool.LS("smcfgs101"));
                slMonstDef = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs002"), -0.50f, 8.00f, AddDefRate, GameTool.LS("smcfgs101"));
                slMonstHp = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs003"), -0.50f, 8.00f, AddHpRate, GameTool.LS("smcfgs101"));
                slMonstBasis = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs004"), -0.50f, 8.00f, AddBasisRate, GameTool.LS("smcfgs101"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs005")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                slMonstSpecialRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs006"), -0.50f, 1.00f, AddSpecialMonsterRate, GameTool.LS("smcfgs101"));
                uiCustom.AddText(col + 2, row++, GameTool.LS("smcfgs049")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                row++;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs007")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
                slEcoTaxRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs008"), -0.50f, 5.00f, AddTaxRate, GameTool.LS("smcfgs101"));
                slEcoInfRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs009"), -0.50f, 2.00f, AddInflationRate, GameTool.LS("smcfgs101"));
                slEcoBuildingCost = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs010"), 0.00f, 8.00f, AddBuildingCostRate, GameTool.LS("smcfgs101"));
                slEcoBankAccCost = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs011"), 0.00f, 8.00f, AddBankAccountCostRate, GameTool.LS("smcfgs101"));
                slEcoBankFee = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs012"), 0.00f, 100.00f, AddBankFee, GameTool.LS("smcfgs101"));
                slEcoRefineCost = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs013"), -0.50f, 1.00f, AddRefineCost, GameTool.LS("smcfgs101"));
                slEcoSectExchangeRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs014"), -0.50f, 1.00f, AddSectExchangeRate, GameTool.LS("smcfgs101"));
                slEcoItemValue = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs015"), -0.50f, 1.00f, AddItemValueRate, GameTool.LS("smcfgs101"));
                row++;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs016")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
                slNpcGrowRate = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs017"), 0.00f, 8.00f, AddNpcGrowRate, GameTool.LS("smcfgs101"));
                slNPCAmount = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs047"), 1000, 10000, NPCAmount, GameTool.LS("smcfgs103"));
                uiCustom.AddText(col + 3, row++, GameTool.LS("smcfgs048")).Format(null, 13).Align(TextAnchor.MiddleLeft).SetWork(new UIItemWork
                {
                    Formatter = (x) => new string[] { (slNPCAmount.Get().Parse<int>() / 2).ToString() },
                });
                row++;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs018")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
                slMiscLevelupExp = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs019"), 0.00f, 1.00f, AddLevelupExpRate, GameTool.LS("smcfgs101"));
                slGrowUpSpeed = uiCustom.AddCompositeSlider(col, row++, GameTool.LS("smcfgs051"), 1, 36, GrowUpSpeed, GameTool.LS("smcfgs053"));
                uiCustom.AddText(col + 2, row++, GameTool.LS("smcfgs052")).Format(null, 13).Align(TextAnchor.MiddleLeft);

                col = 16; row = 0;
                uiCustom.AddText(col, row++, GameTool.LS("smcfgs020")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
                tglSysHideSave = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs021"), HideSaveButton, GameTool.LS("smcfgs102"));
                tglSysHideReload = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs022"), HideReloadButton, GameTool.LS("smcfgs102"));
                tglLostItemWhenDie = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs056"), HideReloadButton, GameTool.LS("smcfgs102"));
                tglSysOnelife = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs023"), Onelife, GameTool.LS("smcfgs102"));
                tglAllowTownBuildupOverTime = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs050"), AllowTownBuildupOverTime, GameTool.LS("smcfgs102"));
                tglAllowTownBuildupOverTime_IncludeFirstTown = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs057"), AllowTownBuildupOverTime_IncludeFirstTown, GameTool.LS("smcfgs102"));
                tglSysHideBattleMap = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs024"), HideBattleMap, GameTool.LS("smcfgs102"));
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
                tglSysOnlyPortalAtCityAndSect = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs033"), OnlyPortalAtCityAndSect, GameTool.LS("smcfgs102"));
                tglSysSectNoExchange = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs034"), SectNoExchange, GameTool.LS("smcfgs102"));
                tglSysNoRebirth = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs035"), NoRebirth, GameTool.LS("smcfgs102"));
                tglAllowUpgradeNaturally = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs036"), AllowUpgradeNaturally, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs037")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                tglMonsterPropertiesDependOnPlayerProperties = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs058"), MonsterPropertiesDependOnPlayerProperties, GameTool.LS("smcfgs102"));
                tglAutoSaveAfterLostInBattle = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs059"), AutoSaveAfterLostInBattle, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs060")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                tglSysBossHasShield = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs038"), BossHasShield, GameTool.LS("smcfgs102"));
                tglNoGrowupFromBattles = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs039"), NoGrowupFromBattles, GameTool.LS("smcfgs102"));
                tglSysNoExpFromBattle = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs040"), NoExpFromBattles, GameTool.LS("smcfgs102"));
                tglEnableTrainer = uiCustom.AddCompositeToggle(col, row++, GameTool.LS("smcfgs041"), EnableTrainer, GameTool.LS("smcfgs102"));
                uiCustom.AddText(col - 1, row++, GameTool.LS("smcfgs042")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                uiCustom.AddButton(col + 1, row + 1, ExportConfigs, GameTool.LS("smcfgs054")).Size(200, 40);
                uiCustom.AddButton(col + 1, row + 3, ImportConfigs, GameTool.LS("smcfgs055")).Size(200, 40);

                col = 30; row = 0;
                uiCustom.AddText(col, row, GameTool.LS("smcfgs043")).Format(Color.red, 17).Align(TextAnchor.MiddleRight).SetWork(new UIItemWork
                {
                    Formatter = (x) => new string[] { CalSMTotalScore().ToString() },
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

                SetWork();
            }
            uiCustom.UpdateUI();
        }

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
                        g.ui.MsgBox("Error", "This file has some errors!");
                    }
                }
            }
        }

        private void SetWork()
        {
            foreach (var wk in ScoreCalculator)
            {
                var item = wk.Comp.Invoke();
                item.ItemWork = wk;
            }
        }

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
        }

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
            slNPCAmount.SetPercent(level * 0.02000f, 2000f);
            tglAllowTownBuildupOverTime.Set(level > 0);
            tglAllowTownBuildupOverTime_IncludeFirstTown.Set(level > 5);
            slGrowUpSpeed.SetPercent(1.0000f - (level * 0.10000f), 3, 24);
            tglLostItemWhenDie.Set(level > 4);
            tglMonsterPropertiesDependOnPlayerProperties.Set(level > 3);
            tglAutoSaveAfterLostInBattle.Set(level > 6);
        }

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
            CacheHelper.SaveGlobalCache(this);

            //edit conf
            g.conf.npcInitParameters.GetItem(1).value = NPCAmount;
        }

        public static int CalCompScore(UIItemBase comp)
        {
            var x = ScoreCalculator.FirstOrDefault(k => k.Comp.Invoke() == comp);
            if (x == null || !x.Cond.Invoke(comp))
                return 0;
            return x.Cal.Invoke(comp);
        }

        public static bool IsEnableComp(UIItemBase comp)
        {
            var x = ScoreCalculator.FirstOrDefault(k => k.Comp.Invoke() == comp);
            if (x == null)
                return false;
            return x != null && x.Cond.Invoke(comp) && comp.IsEnable();
        }

        public static int CalSMTotalScore()
        {
            var rs = 0;
            foreach (var item in ScoreCalculator)
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
