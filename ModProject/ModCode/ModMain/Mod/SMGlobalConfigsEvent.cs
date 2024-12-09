using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Object;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SM_GLOBAL_CONFIGS_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
    public class SMGlobalConfigsEvent : ModEvent
    {
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
        public bool LowGradeDestiniesAtBeginning { get; set; } = false;

        //UI
        private UIHelper.UICustom1 uiCustom;
        private UIItemBase.UIItemText txtTotalScore;
        private UIItemBase.UIItemComposite slMonstAtk;
        private UIItemBase.UIItemComposite slMonstDef;
        private UIItemBase.UIItemComposite slMonstHp;
        private UIItemBase.UIItemComposite slMonstBasis;
        private UIItemBase.UIItemComposite slMonstSpecialRate;
        private UIItemBase.UIItemComposite slEcoTaxRate;
        private UIItemBase.UIItemComposite slEcoInfRate;
        private UIItemBase.UIItemComposite slEcoBuildingCost;
        private UIItemBase.UIItemComposite slEcoBankAccCost;
        private UIItemBase.UIItemComposite slEcoBankFee;
        private UIItemBase.UIItemComposite slEcoRefineCost;
        private UIItemBase.UIItemComposite slEcoSectExchangeRate;
        private UIItemBase.UIItemComposite slEcoItemValue;
        private UIItemBase.UIItemComposite slNpcGrowRate;
        private UIItemBase.UIItemComposite slMiscLevelupExp;
        private UIItemBase.UIItemComposite tglSysHideSave;
        private UIItemBase.UIItemComposite tglSysHideReload;
        private UIItemBase.UIItemComposite tglSysHideBattleMap;
        private UIItemBase.UIItemComposite tglSysNoRebirth;
        private UIItemBase.UIItemComposite tglSysOnelife;
        private UIItemBase.UIItemComposite tglSysOnlyPortalAtCityAndSect;
        private UIItemBase.UIItemComposite tglSysNoExpFromBattle;
        private UIItemBase.UIItemComposite tglSysSectNoExchange;
        private UIItemBase.UIItemComposite tglSysBossHasShield;
        private UIItemBase.UIItemComposite tglNoGrowupFromBattles;
        private UIItemBase.UIItemComposite tglLowGradeDestiniesAtBeginning;

        //Score
        public static IList<SMItemWork> ScoreCalculator { get; } = new List<SMItemWork>();

        public override void OnLoadClass(bool isNew)
        {
            base.OnLoadClass(isNew);

            ScoreCalculator.Clear();
            Register(() => slMonstAtk, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstDef, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstHp, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstBasis, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstSpecialRate, s => (s.Get().Parse<float>() * 3000).Parse<int>());
            Register(() => slEcoTaxRate, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoInfRate, s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slEcoBuildingCost, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoBankAccCost, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoBankFee, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoRefineCost, s => (s.Get().Parse<float>() * 2000).Parse<int>());
            Register(() => slEcoSectExchangeRate, s => (s.Get().Parse<float>() * 2000).Parse<int>(), s => !tglSysSectNoExchange.Get().Parse<bool>(), s => !tglSysSectNoExchange.Get().Parse<bool>());
            Register(() => slEcoItemValue, s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slNpcGrowRate, s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slMiscLevelupExp, s => (s.Get().Parse<float>() * 2000).Parse<int>());
            Register(() => tglSysHideSave, s => 1000, s => s.Get().Parse<bool>(), onChange: (s, v) => tglSysHideReload.Set(false));
            Register(() => tglSysHideReload, s => 5000, s => s.Get().Parse<bool>(), s => tglSysHideSave.Get().Parse<bool>(), onChange: (s, v) => tglSysOnelife.Set(false));
            Register(() => tglSysHideBattleMap, s => 2000, s => s.Get().Parse<bool>());
            Register(() => tglSysNoRebirth, s => 10000, s => s.Get().Parse<bool>());
            Register(() => tglSysOnelife, s => 20000, s => s.Get().Parse<bool>(), s => tglSysHideReload.Get().Parse<bool>());
            Register(() => tglSysOnlyPortalAtCityAndSect, s => 1000, s => s.Get().Parse<bool>());
            Register(() => tglSysNoExpFromBattle, s => 1000, s => s.Get().Parse<bool>());
            Register(() => tglSysSectNoExchange, s => 10000, s => s.Get().Parse<bool>());
            Register(() => tglSysBossHasShield, s => 10000, s => s.Get().Parse<bool>());
            Register(() => tglNoGrowupFromBattles, s => 10000, s => s.Get().Parse<bool>());
            Register(() => tglLowGradeDestiniesAtBeginning, s => 1000, s => s.Get().Parse<bool>());
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
                    var x = s.Parent as UIItemBase.UIItemComposite;
                    if (x.MainComponent is UIItemBase.UIItemSlider)
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

        [EventCondition(IsInGame = 0)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var uiLogin = g.ui.GetUI<UILogin>(UIType.Login);
                var modConfigBtn = uiLogin.btnSet.Create().Pos(0f, 3.9f);
                modConfigBtn.Setup(TITLE);
                modConfigBtn.onClick.AddListener((UnityAction)OpenSMConfigs);
            }
        }

        private void OpenSMConfigs()
        {
            uiCustom = UIHelper.UICustom1.Create(TITLE, SetSMConfigs, true);
            int col, row;

            col = 2; row = 0;
            uiCustom.AddText(col, row++, "Monster:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            slMonstAtk = uiCustom.AddCompositeSlider(col, row++, "ATK", -0.50f, 10.00f, AddAtkRate, "{1}% ({0}P)");
            slMonstDef = uiCustom.AddCompositeSlider(col, row++, "DEF", -0.50f, 10.00f, AddDefRate, "{1}% ({0}P)");
            slMonstHp = uiCustom.AddCompositeSlider(col, row++, "Max HP", -0.50f, 10.00f, AddHpRate, "{1}% ({0}P)");
            slMonstBasis = uiCustom.AddCompositeSlider(col, row++, "Basis", -0.50f, 10.00f, AddBasisRate, "{1}% ({0}P)");
            uiCustom.AddText(col, row++, "(Included Sword, Blade, Spear, Fist, Finger, Palm, Fire, Water, Thunder, Wood, Wind, Earth)").Format(null, 13).Align(TextAnchor.MiddleLeft);
            slMonstSpecialRate = uiCustom.AddCompositeSlider(col, row++, "Special Monster Rate", -0.50f, 1.00f, AddSpecialMonsterRate, "{1}% ({0}P)");

            col = 2; row = 8;
            uiCustom.AddText(col, row++, "Economic:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            slEcoTaxRate = uiCustom.AddCompositeSlider(col, row++, "Tax Rate", 0.00f, 10.00f, AddTaxRate, "{1}% ({0}P)");
            slEcoInfRate = uiCustom.AddCompositeSlider(col, row++, "Inflation Rate", -0.50f, 3.00f, AddInflationRate, "{1}% ({0}P)");
            slEcoBuildingCost = uiCustom.AddCompositeSlider(col, row++, "Building Cost", 0.00f, 10.00f, AddBuildingCostRate, "{1}% ({0}P)");
            slEcoBankAccCost = uiCustom.AddCompositeSlider(col, row++, "Bank Account Cost", 0.00f, 10.00f, AddBankAccountCostRate, "{1}% ({0}P)");
            slEcoBankFee = uiCustom.AddCompositeSlider(col, row++, "Bank Fee", 0.00f, 100.00f, AddBankFee, "{1}% ({0}P)");
            slEcoRefineCost = uiCustom.AddCompositeSlider(col, row++, "Refine Cost", -0.50f, 1.00f, AddRefineCost, "{1}% ({0}P)");
            slEcoSectExchangeRate = uiCustom.AddCompositeSlider(col, row++, "Sect Exchange Fee", -0.50f, 1.00f, AddSectExchangeRate, "{1}% ({0}P)");
            slEcoItemValue = uiCustom.AddCompositeSlider(col, row++, "Item Value", -0.50f, 1.00f, AddItemValueRate, "{1}% ({0}P)");

            col = 2; row = 18;
            uiCustom.AddText(col, row++, "NPC:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            slNpcGrowRate = uiCustom.AddCompositeSlider(col, row++, "Grow Rate", 0.00f, 10.00f, AddNpcGrowRate, "{1}% ({0}P)");

            col = 2; row = 21;
            uiCustom.AddText(col, row++, "Misc:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            slMiscLevelupExp = uiCustom.AddCompositeSlider(col, row++, "Levelup Exp", 0.00f, 1.00f, AddLevelupExpRate, "{1}% ({0}P)");

            col = 18; row = 0;
            uiCustom.AddText(col, row++, "Systems:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            tglSysHideSave = uiCustom.AddCompositeToggle(col, row++, "Hide Save Button", HideSaveButton, "({0}P)");
            tglSysHideReload = uiCustom.AddCompositeToggle(col, row++, "Hide Reload Button", HideReloadButton, "({0}P)");
            tglSysOnelife = uiCustom.AddCompositeToggle(col, row++, "One life", Onelife, "({0}P)");
            row++;
            tglSysHideBattleMap = uiCustom.AddCompositeToggle(col, row++, "Hide Battle Map", HideBattleMap, "({0}P)");
            tglLowGradeDestiniesAtBeginning = uiCustom.AddCompositeToggle(col, row++, "Low Grade Destinies At Beginning", LowGradeDestiniesAtBeginning, "({0}P)");
            tglSysOnlyPortalAtCityAndSect = uiCustom.AddCompositeToggle(col, row++, "Only Portal at City and Sect", OnlyPortalAtCityAndSect, "({0}P)");
            tglSysSectNoExchange = uiCustom.AddCompositeToggle(col, row++, "Sect No Exchange", SectNoExchange, "({0}P)");
            tglSysNoRebirth = uiCustom.AddCompositeToggle(col, row++, "No Rebirth", NoRebirth, "({0}P)");
            row++;
            tglSysBossHasShield = uiCustom.AddCompositeToggle(col, row++, "Boss Has Shield", BossHasShield, "({0}P)");
            tglNoGrowupFromBattles = uiCustom.AddCompositeToggle(col, row++, "No Growup From Battles", NoGrowupFromBattles, "({0}P)");
            tglSysNoExpFromBattle = uiCustom.AddCompositeToggle(col, row++, "No Exp from Battles", NoExpFromBattles, "({0}P)");

            col = 30; row = 0;
            txtTotalScore = uiCustom.AddText(col, row, "Total score: {0}P").Format(Color.red, 17).Align(TextAnchor.MiddleRight);
            uiCustom.AddButton(col, row += 2, () => SetLevelBase(), "Default");
            uiCustom.AddButton(col, row += 2, () => SetLevel(0), "Level 0");
            uiCustom.AddButton(col, row += 2, () => SetLevel(1), "Level 1");
            uiCustom.AddButton(col, row += 2, () => SetLevel(2), "Level 2");
            uiCustom.AddButton(col, row += 2, () => SetLevel(3), "Level 3");
            uiCustom.AddButton(col, row += 2, () => SetLevel(4), "Level 4");
            uiCustom.AddButton(col, row += 2, () => SetLevel(5), "Level 5");
            uiCustom.AddButton(col, row += 2, () => SetLevel(6), "Level 6");
            uiCustom.AddButton(col, row += 2, () => SetLevel(7), "Level 7");
            uiCustom.AddButton(col, row += 2, () => SetLevel(8), "Level 8");
            uiCustom.AddButton(col, row += 2, () => SetLevel(9), "Level 9");
            uiCustom.AddButton(col, row += 2, () => SetLevel(10), "Level 10");
            uiCustom.AddText(15, 26, "You have to start a new game to apply these configs!").Format(Color.red, 17);

            SetWork();
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
            tglLowGradeDestiniesAtBeginning.Set(false);
        }

        private void SetLevel(int level)
        {
            slMonstAtk.SetPercent(level * 0.04000f);
            slMonstDef.SetPercent(level * 0.01000f);
            slMonstHp.SetPercent(level * 0.05000f, 0f);
            slMonstBasis.SetPercent(level * 0.04000f);
            slMonstSpecialRate.SetPercent(level * 0.10000f);
            slEcoTaxRate.SetPercent(level * 0.01000f);
            slEcoInfRate.SetPercent(level * 0.10000f);
            slEcoBuildingCost.SetPercent(level * 0.04000f);
            slEcoBankAccCost.SetPercent(level * 0.10000f);
            slEcoBankFee.SetPercent(level * 0.05000f);
            slEcoRefineCost.SetPercent(level * 0.10000f);
            slEcoSectExchangeRate.SetPercent(level * 0.10000f);
            slEcoItemValue.SetPercent(level * 0.10000f);
            slNpcGrowRate.SetPercent(level * 0.02000f);
            slMiscLevelupExp.SetPercent(level * 0.10000f);
            tglSysHideBattleMap.Set(level > 0);
            tglSysHideSave.Set(level > 1);
            tglSysNoExpFromBattle.Set(level > 2);
            tglSysOnlyPortalAtCityAndSect.Set(level > 3);
            tglSysBossHasShield.Set(level > 4);
            tglNoGrowupFromBattles.Set(level > 5);
            tglLowGradeDestiniesAtBeginning.Set(level > 5);
            tglSysHideReload.Set(level > 6);
            tglSysSectNoExchange.Set(level > 7);
            tglSysNoRebirth.Set(level > 8);
            tglSysOnelife.Set(level > 9);
        }

        [ErrorIgnore]
        [EventCondition(IsInGame = 0)]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            uiCustom.UpdateUI();
            txtTotalScore.Set($"Total score: {CalSMTotalScore()}P");
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
            LowGradeDestiniesAtBeginning = tglLowGradeDestiniesAtBeginning.Get().Parse<bool>();
            CacheHelper.SaveGlobalCache(this);
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
