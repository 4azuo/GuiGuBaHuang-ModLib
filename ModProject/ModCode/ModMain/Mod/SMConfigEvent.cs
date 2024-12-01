using Boo.Lang.Compiler.TypeSystem;
using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Object;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SM_CONFIG_EVENT, IsGlobal = true)]
    public class SMConfigEvent : ModEvent
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
        public float AddNpcGrowRate { get; set; } = 0f;
        public float AddLevelupExpRate { get; set; } = 0f;
        public bool HideSaveButton { get; set; } = false;
        public bool HideReloadButton { get; set; } = false;
        public bool HideBattleMap { get; set; } = false;
        public bool NoRebirth { get; set; } = false;
        public bool Onelife { get; set; } = false;
        public bool OnlyPortalAtCityAndSect { get; set; } = false;
        public bool NoExpFromBattles { get; set; } = false;

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
        private UIItemBase.UIItemComposite slNpcGrowRate;
        private UIItemBase.UIItemComposite slMiscLevelupExp;
        private UIItemBase.UIItemComposite tglSysHideSave;
        private UIItemBase.UIItemComposite tglSysHideReload;
        private UIItemBase.UIItemComposite tglSysHideBattleMap;
        private UIItemBase.UIItemComposite tglSysNoRebirth;
        private UIItemBase.UIItemComposite tglSysOnelife;
        private UIItemBase.UIItemComposite tglSysOnlyPortalAtCityAndSect;
        private UIItemBase.UIItemComposite tglSysNoExpFromBattle;

        //Score
        private readonly IList<SMItem> ScoreCalculator = new List<SMItem>();

        public override void OnLoadGlobal()
        {
            base.OnLoadGlobal();

            DebugHelper.WriteLine("1");
            Register(() => txtTotalScore, s => CalSMTotalScore());
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
            Register(() => slNpcGrowRate, s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slMiscLevelupExp, s => (s.Get().Parse<float>() * 2000).Parse<int>());
            Register(() => tglSysHideSave, s => 1000, s => s.Get().Parse<bool>()/*, onChange: s => tglSysHideReload.Set(false)*/);
            Register(() => tglSysHideReload, s => 5000, s => s.Get().Parse<bool>()/*, s => tglSysHideSave.Get().Parse<bool>()*//*, onChange: s => tglSysOnelife.Set(false)*/);
            Register(() => tglSysHideBattleMap, s => 2000, s => s.Get().Parse<bool>());
            Register(() => tglSysNoRebirth, s => 10000, s => s.Get().Parse<bool>());
            Register(() => tglSysOnelife, s => 20000, s => s.Get().Parse<bool>()/*, s => tglSysHideReload.Get().Parse<bool>()*/);
            Register(() => tglSysOnlyPortalAtCityAndSect, s => 1000, s => s.Get().Parse<bool>());
            Register(() => tglSysNoExpFromBattle, s => 1000, s => s.Get().Parse<bool>());
            DebugHelper.WriteLine(ScoreCalculator.Count.ToString());
            DebugHelper.Save();
        }

        private void Register(
            Func<UIItemBase> funcComp, 
            Func<UIItemBase, int> funcCal, 
            Func<UIItemBase, bool> funcCond = null,
            Func<UIItemBase, bool> funcEna = null,
            Func<UIItemBase, object[]> funcFormatter = null,
            Action<UIItemBase> onChange = null)
        {
            var formatter = funcFormatter ?? (s => new object[] { funcCal.Invoke(s), s.Get().Parse<float>() });
            ScoreCalculator.Add(new SMItem
            {
                Comp = funcComp,
                Cal = funcCal,
                Cond = funcCond ?? (s => true),
                EnaAct = funcEna ?? (s => true),
                Formatter = formatter,
                Change = onChange,
            });
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var uiLogin = g.ui.GetUI<UILogin>(UIType.Login);
                var modConfigBtn = uiLogin.btnSet.Create().Pos(0f, 3.3f, uiLogin.btnPaperChange.transform.position.z);
                var modConfigText = modConfigBtn.GetComponentInChildren<Text>().Align(TextAnchor.MiddleCenter);
                modConfigText.text = TITLE;
                modConfigBtn.onClick.AddListener((UnityAction)OpenSMConfigs);
            }
        }

        private void OpenSMConfigs()
        {
            uiCustom = UIHelper.UICustom1.Create(TITLE, SetSMConfigs, true);
            int col, row;

            col = 1; row = 0;
            uiCustom.AddText(col, row++, "Monster:").Format(null, 17, FontStyle.Italic);
            slMonstAtk = uiCustom.AddCompositeSlider(col, row++, "ATK", -10.00f, 10.00f, AddAtkRate, "+{1}% ({0}P)");
            slMonstDef = uiCustom.AddCompositeSlider(col, row++, "DEF", -10.00f, 10.00f, AddDefRate, "+{1}% ({0}P)");
            slMonstHp = uiCustom.AddCompositeSlider(col, row++, "Max HP", -10.00f, 10.00f, AddHpRate, "+{1}% ({0}P)");
            slMonstBasis = uiCustom.AddCompositeSlider(col, row++, "Basis", -10.00f, 10.00f, AddBasisRate, "+{1}% ({0}P)");
            uiCustom.AddText(col, row++, "(Included Sword, Blade, Spear, Fist, Finger, Palm, Fire, Water, Thunder, Wood, Wind, Earth)").Format(null, 13).Align(TextAnchor.MiddleLeft);
            slMonstSpecialRate = uiCustom.AddCompositeSlider(col, row++, "Special Monster Rate", -1.00f, 1.00f, AddSpecialMonsterRate, "+{1}% ({0}P)");

            col = 1; row = 8;
            uiCustom.AddText(col, row++, "Economic:").Format(null, 17, FontStyle.Italic);
            slEcoTaxRate = uiCustom.AddCompositeSlider(col, row++, "Tax Rate", -10.00f, 10.00f, AddTaxRate, "+{1}% ({0}P)");
            slEcoInfRate = uiCustom.AddCompositeSlider(col, row++, "Inflation Rate", -10.00f, 10.00f, AddInflationRate, "+{1}% ({0}P)");
            slEcoBuildingCost = uiCustom.AddCompositeSlider(col, row++, "Building Cost", -10.00f, 10.00f, AddBuildingCostRate, "+{1}% ({0}P)");
            slEcoBankAccCost = uiCustom.AddCompositeSlider(col, row++, "Bank Account Cost", -10.00f, 10.00f, AddBankAccountCostRate, "+{1}% ({0}P)");
            slEcoBankFee = uiCustom.AddCompositeSlider(col, row++, "Bank Fee", -10.00f, 10.00f, AddBankFee, "+{1}% ({0}P)");

            col = 1; row = 15;
            uiCustom.AddText(col, row++, "NPC:").Format(null, 17, FontStyle.Italic);
            slNpcGrowRate = uiCustom.AddCompositeSlider(col, row++, "Grow Rate", -10.00f, 10.00f, AddNpcGrowRate, "+{1}% ({0}P)");

            col = 1; row = 18;
            uiCustom.AddText(col, row++, "Misc:").Format(null, 17, FontStyle.Italic);
            slMiscLevelupExp = uiCustom.AddCompositeSlider(col, row++, "Levelup Exp", -10.00f, 10.00f, AddLevelupExpRate, "+{1}% ({0}P)");

            col = 16; row = 0;
            uiCustom.AddText(col, row++, "Systems:").Format(null, 17, FontStyle.Italic);
            tglSysHideSave = uiCustom.AddCompositeToggle(col, row++, "Hide Save Button", HideSaveButton, "({0}P)");
            tglSysHideReload = uiCustom.AddCompositeToggle(col, row++, "Hide Reload Button", HideReloadButton, "({0}P)");
            tglSysHideBattleMap = uiCustom.AddCompositeToggle(col, row++, "Hide Battle Map", HideBattleMap, "({0}P)");
            tglSysNoRebirth = uiCustom.AddCompositeToggle(col, row++, "No Rebirth", NoRebirth, "({0}P)");
            tglSysOnelife = uiCustom.AddCompositeToggle(col, row++, "One life", Onelife, "({0}P)");
            tglSysOnlyPortalAtCityAndSect = uiCustom.AddCompositeToggle(col, row++, "Only Portal at City and Sect", OnlyPortalAtCityAndSect, "({0}P)");
            tglSysNoExpFromBattle = uiCustom.AddCompositeToggle(col, row++, "No Exp from Battles", NoExpFromBattles, "({0}P)");

            col = 30; row = 0;
            txtTotalScore = uiCustom.AddText(col, 0, "Total score: {0}P").Format(Color.red, 17).Align(TextAnchor.MiddleRight);
            uiCustom.AddButton(col, 2, () => { }, "Level 1");
            uiCustom.AddButton(col, 4, () => { }, "Level 2");
            uiCustom.AddButton(col, 6, () => { }, "Level 3");
            uiCustom.AddButton(col, 8, () => { }, "Level 4");
            uiCustom.AddButton(col, 10, () => { }, "Level 5");
            uiCustom.AddButton(col, 12, () => { }, "Level 6");
            uiCustom.AddButton(col, 14, () => { }, "Level 7");
            uiCustom.AddButton(col, 16, () => { }, "Level 8");
            uiCustom.AddButton(col, 18, () => { }, "Level 9");
            uiCustom.AddButton(col, 20, () => { }, "Level 10");
            uiCustom.AddText(14, 26, "You have to start a new game to apply these configs!").Format(Color.red, 17);

            SetWork();
            try
            {
                txtTotalScore.Set($"Total score: {CalSMTotalScore()}P");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
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

        [ErrorIgnore]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            //uiCustom.UpdateUI();
            //txtTotalScore.Set($"Total score: {CalSMTotalScore()}P");
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
            AddNpcGrowRate = slNpcGrowRate.Get().Parse<float>();
            AddLevelupExpRate = slMiscLevelupExp.Get().Parse<float>();
            HideSaveButton = tglSysHideSave.Get().Parse<bool>();
            HideReloadButton = tglSysHideReload.Get().Parse<bool>();
            HideBattleMap = tglSysHideBattleMap.Get().Parse<bool>();
            NoRebirth = tglSysNoRebirth.Get().Parse<bool>();
            Onelife = tglSysOnelife.Get().Parse<bool>();
            OnlyPortalAtCityAndSect = tglSysOnlyPortalAtCityAndSect.Get().Parse<bool>();
            NoExpFromBattles = tglSysNoExpFromBattle.Get().Parse<bool>();
            CacheHelper.Save();
        }

        private int CalCompScore(UIItemBase comp)
        {
            if (comp == null)
                return 0;
            var x = ScoreCalculator.FirstOrDefault(k => k.Comp.Invoke() == comp);
            if (x == null || !x.Cond.Invoke(comp))
                return 0;
            return x.Cal.Invoke(comp);
        }

        private bool IsEnableComp(UIItemBase comp)
        {
            if (comp == null)
                return false;
            var x = ScoreCalculator.FirstOrDefault(k => k.Comp.Invoke() == comp);
            if (x == null)
                return false;
            var item = x.Comp.Invoke();
            return x != null && x.Cond.Invoke(comp) && item.IsEnable();
        }

        private int CalSMTotalScore()
        {
            return ScoreCalculator.Where(x => IsEnableComp(x.Comp.Invoke())).Sum(x => CalCompScore(x.Comp.Invoke()));
        }
    }
}
