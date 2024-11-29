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

        private Text txtSMScore;
        private Text txtEndNote;
        private Button btnDefault;
        private Button btnLevel01;
        private Button btnLevel02;
        private Button btnLevel03;
        private Button btnLevel04;
        private Button btnLevel05;
        private Button btnLevel06;
        private Button btnLevel07;
        private Button btnLevel08;
        private Button btnLevel09;
        private Button btnLevel10;
        //main-comp
        private Slider slAtkValue;
        private Slider slDefValue;
        private Slider slHpValue;
        private Slider slBasisValue;
        private Slider slSpecialMonsterRate;
        private Slider slTaxRate;
        private Slider slInflationRate;
        private Slider slBuildingCost;
        private Slider slBankAccountCost;
        private Slider slBankFee;
        private Slider slNpcGrowRate;
        private Slider slLevelupExp;
        private Toggle tglHideSaveButton;
        private Toggle tglHideReloadButton;
        private Toggle tglHideBattleMap;
        private Toggle tglNoRebirth;
        //postfix
        private Text pfAtkValue;
        private Text pfDefValue;
        private Text pfHpValue;
        private Text pfBasisValue;
        private Text pfSpecialMonsterRate;
        private Text pfTaxRate;
        private Text pfInflationRate;
        private Text pfBuildingCost;
        private Text pfBankAccountCost;
        private Text pfBankFee;
        private Text pfNpcGrowRate;
        private Text pfLevelupExp;
        private Text pfHideSaveButton;
        private Text pfHideReloadButton;
        private Text pfHideBattleMap;
        private Text pfNoRebirth;

        private readonly IList<dynamic> ScoreCalculator = new List<dynamic>();
        private readonly IList<float> Columns = new List<float>();
        private readonly IList<float> Rows = new List<float>();

        public override void OnLoadGlobal()
        {
            base.OnLoadGlobal();

            Columns.Clear();
            for (var i = -5.5f; i <= 5.5f; i += 0.4f)
                Columns.Add(i);
            Rows.Clear();
            for (var i = -0.5f; i >= -10.0f; i -= 0.25f)
                Rows.Add(i);

            Register(() => slAtkValue, s => true, s => (s.value * 100).Parse<int>());
            Register(() => slDefValue, s => true, s => (s.value * 100).Parse<int>());
            Register(() => slHpValue, s => true, s => (s.value * 100).Parse<int>());
            Register(() => slBasisValue, s => true, s => (s.value * 100).Parse<int>());
            Register(() => slSpecialMonsterRate, s => true, s => (s.value * 3000).Parse<int>());
            Register(() => slTaxRate, s => true, s => (s.value * 100).Parse<int>());
            Register(() => slInflationRate, s => true, s => (s.value * 1000).Parse<int>());
            Register(() => slBuildingCost, s => true, s => (s.value * 100).Parse<int>());
            Register(() => slBankAccountCost, s => true, s => (s.value * 100).Parse<int>());
            Register(() => slBankFee, s => true, s => (s.value * 100).Parse<int>());
            Register(() => slNpcGrowRate, s => true, s => (s.value * 1000).Parse<int>());
            Register(() => slLevelupExp, s => true, s => (s.value * 2000).Parse<int>());
            Register(() => tglHideSaveButton, s => s.isOn, s => 1000);
            Register(() => tglHideReloadButton, s => s.isOn, s => 5000);
            Register(() => tglHideBattleMap,s => s.isOn,s => 2000);
            Register(() => tglNoRebirth, s => s.isOn,s => 10000);
        }

        private void Register<T>(Func<T> funcComp, Func<T, bool> funcCond, Func<T, int> funcCal) where T : UIBehaviour
        {
            ScoreCalculator.Add(new SMItem<T>
            {
                Comp = funcComp,
                Cond = funcCond,
                Cal = funcCal
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
            try
            {
                DebugHelper.WriteLine("0");
                var uiCfg = g.ui.OpenUI<UITextInfoLong>(UIType.TextInfoLong);
                var uiStt = g.ui.OpenUI<UIGameSetting>(UIType.GameSetting);
                uiStt.gameObject.SetActive(false);
                {
                    DebugHelper.WriteLine("1");
                    uiCfg.InitData(TITLE, string.Empty, isShowCancel: true);
                    uiCfg.btnOK.onClick.AddListener((UnityAction)SetSMConfigs);

                    DebugHelper.WriteLine("2");
                    Func<float, float, TextAnchor, Text> Text = (float c, float r, TextAnchor anchor) => uiStt.textSystemOK.Create(uiCfg.canvas.transform).Pos(uiCfg.textTitle.gameObject, c, r).Align(anchor).Format();
                    Func<float, float, Toggle> Toggle = (float c, float r) => uiStt.tglCameraShake.Create(uiCfg.canvas.transform).Pos(uiCfg.textTitle.gameObject, c, r);
                    Func<float, float, Slider> Slider = (float c, float r) => uiStt.sliSoundMain.Create(uiCfg.canvas.transform).Pos(uiCfg.textTitle.gameObject, c, r);
                    Func<float, float, string, Button> Button = (float c, float r, string label) =>
                    {
                        var rs = uiStt.btnSystemOK.Create(uiCfg.canvas.transform).Pos(uiCfg.textTitle.gameObject, c, r);
                        rs.GetComponentInChildren<Text>().text = label;
                        return rs;
                    };

                    //group
                    DebugHelper.WriteLine("3");
                    var txtMonsterGroup = Text(Columns[0], Rows[0], TextAnchor.MiddleRight).Format(fsize: 17, fstype: FontStyle.Italic);
                    var txtEconomicGroup = Text(Columns[0], Rows[8], TextAnchor.MiddleRight).Format(fsize: 17, fstype: FontStyle.Italic);
                    var txtNpcGroup = Text(Columns[0], Rows[15], TextAnchor.MiddleRight).Format(fsize: 17, fstype: FontStyle.Italic);
                    var txtMiscGroup = Text(Columns[0], Rows[18], TextAnchor.MiddleRight).Format(fsize: 17, fstype: FontStyle.Italic);
                    var txtSystemGroup = Text(Columns[14], Rows[0], TextAnchor.MiddleRight).Format(fsize: 17, fstype: FontStyle.Italic);

                    txtMonsterGroup.text = "Monster:";
                    txtEconomicGroup.text = "Economic:";
                    txtNpcGroup.text = "NPC:";
                    txtMiscGroup.text = "Misc:";
                    txtSystemGroup.text = "Systems:";

                    //prefix
                    DebugHelper.WriteLine("4");
                    var txtAtk = Text(Columns[0], Rows[1], TextAnchor.MiddleRight);
                    var txtDef = Text(Columns[0], Rows[2], TextAnchor.MiddleRight);
                    var txtHp = Text(Columns[0], Rows[3], TextAnchor.MiddleRight);
                    var txtBasis = Text(Columns[0], Rows[4], TextAnchor.MiddleRight);
                    var txtBasis2 = Text(Columns[0], Rows[5], TextAnchor.MiddleLeft).Format(fsize: 13);
                    var txtSpecialMonsterRate = Text(Columns[0], Rows[6], TextAnchor.MiddleRight);
                    var txtTaxRate = Text(Columns[0], Rows[9], TextAnchor.MiddleRight);
                    var txtInflationRate = Text(Columns[0], Rows[10], TextAnchor.MiddleRight);
                    var txtBuildingCost = Text(Columns[0], Rows[11], TextAnchor.MiddleRight);
                    var txtBankAccountCost = Text(Columns[0], Rows[12], TextAnchor.MiddleRight);
                    var txtBankFee = Text(Columns[0], Rows[13], TextAnchor.MiddleRight);
                    var txtNpcGrowRate = Text(Columns[0], Rows[16], TextAnchor.MiddleRight);
                    var txtLevelupExp = Text(Columns[0], Rows[19], TextAnchor.MiddleRight);
                    var txtHideSaveBtn = Text(Columns[14], Rows[1], TextAnchor.MiddleRight);
                    var txtHideReloadBtn = Text(Columns[14], Rows[2], TextAnchor.MiddleRight);
                    var txtHideBattleMap = Text(Columns[14], Rows[3], TextAnchor.MiddleRight);
                    var txtNoRebirth = Text(Columns[14], Rows[4], TextAnchor.MiddleRight);

                    txtAtk.text = "ATK";
                    txtDef.text = "DEF";
                    txtHp.text = "Max HP";
                    txtBasis.text = "Basis";
                    txtBasis2.text = "(Included Sword, Blade, Spear, Fist, Finger, Palm, Fire, Water, Thunder, Wood, Wind, Earth)";
                    txtSpecialMonsterRate.text = "Special Monster Rate";
                    txtTaxRate.text = "Special Monster Rate";
                    txtInflationRate.text = "Inflation Rate";
                    txtBuildingCost.text = "Building Cost";
                    txtBankAccountCost.text = "Bank Account Cost";
                    txtBankFee.text = "Bank Fee";
                    txtNpcGrowRate.text = "NPC Grow Rate";
                    txtLevelupExp.text = "Levelup Exp Rate";
                    txtHideSaveBtn.text = "Hide Save-Button";
                    txtHideReloadBtn.text = "Hide Reload-Button";
                    txtHideBattleMap.text = "Hide Battle Map";
                    txtNoRebirth.text = "No Rebirth";

                    //main-comp
                    DebugHelper.WriteLine("5");
                    slAtkValue = Slider(Columns[4], Rows[1]);
                    slDefValue = Slider(Columns[4], Rows[2]);
                    slHpValue = Slider(Columns[4], Rows[3]);
                    slBasisValue = Slider(Columns[4], Rows[4]);
                    slSpecialMonsterRate = Slider(Columns[4], Rows[6]);
                    slTaxRate = Slider(Columns[4], Rows[9]);
                    slInflationRate = Slider(Columns[4], Rows[10]);
                    slBuildingCost = Slider(Columns[4], Rows[11]);
                    slBankAccountCost = Slider(Columns[4], Rows[12]);
                    slBankFee = Slider(Columns[4], Rows[13]);
                    slNpcGrowRate = Slider(Columns[4], Rows[16]);
                    slLevelupExp = Slider(Columns[4], Rows[19]);
                    tglHideSaveButton = Toggle(Columns[16], Rows[1]);
                    tglHideReloadButton = Toggle(Columns[16], Rows[2]);
                    tglHideBattleMap = Toggle(Columns[16], Rows[3]);
                    tglNoRebirth = Toggle(Columns[16], Rows[4]);

                    slAtkValue.minValue = -10.00f;
                    slAtkValue.maxValue = 10.00f;
                    slAtkValue.value = AddAtkRate;
                    slDefValue.minValue = -10.00f;
                    slDefValue.maxValue = 10.00f;
                    slDefValue.value = AddDefRate;
                    slHpValue.minValue = -10.00f;
                    slHpValue.maxValue = 10.00f;
                    slHpValue.value = AddHpRate;
                    slBasisValue.minValue = -10.00f;
                    slBasisValue.maxValue = 10.00f;
                    slBasisValue.value = AddBasisRate;
                    slSpecialMonsterRate.minValue = -1.00f;
                    slSpecialMonsterRate.maxValue = 1.00f;
                    slSpecialMonsterRate.value = AddSpecialMonsterRate;
                    slTaxRate.minValue = 0.00f;
                    slTaxRate.maxValue = 10.00f;
                    slTaxRate.value = AddTaxRate;
                    slInflationRate.minValue = -1.00f;
                    slInflationRate.maxValue = 3.00f;
                    slInflationRate.value = AddInflationRate;
                    slBuildingCost.minValue = 0.00f;
                    slBuildingCost.maxValue = 10.00f;
                    slBuildingCost.value = AddBuildingCostRate;
                    slBankAccountCost.minValue = 0.00f;
                    slBankAccountCost.maxValue = 10.00f;
                    slBankAccountCost.value = AddBankAccountCostRate;
                    slBankFee.minValue = 0.00f;
                    slBankFee.maxValue = 100.00f;
                    slBankFee.value = AddBankFee;
                    slNpcGrowRate.minValue = 0.00f;
                    slNpcGrowRate.maxValue = 10.00f;
                    slNpcGrowRate.value = AddNpcGrowRate;
                    slLevelupExp.minValue = 0.00f;
                    slLevelupExp.maxValue = 1.00f;
                    slLevelupExp.value = AddLevelupExpRate;
                    tglHideSaveButton.isOn = HideSaveButton;
                    tglHideReloadButton.isOn = HideReloadButton;
                    tglHideBattleMap.isOn = HideBattleMap;
                    tglNoRebirth.isOn = NoRebirth;

                    //postfix
                    DebugHelper.WriteLine("6");
                    pfAtkValue = Text(Columns[8], Rows[1], TextAnchor.MiddleLeft);
                    pfDefValue = Text(Columns[8], Rows[2], TextAnchor.MiddleLeft);
                    pfHpValue = Text(Columns[8], Rows[3], TextAnchor.MiddleLeft);
                    pfBasisValue = Text(Columns[8], Rows[4], TextAnchor.MiddleLeft);
                    pfSpecialMonsterRate = Text(Columns[8], Rows[6], TextAnchor.MiddleLeft);
                    pfTaxRate = Text(Columns[8], Rows[9], TextAnchor.MiddleLeft);
                    pfInflationRate = Text(Columns[8], Rows[10], TextAnchor.MiddleLeft);
                    pfBuildingCost = Text(Columns[8], Rows[11], TextAnchor.MiddleLeft);
                    pfBankAccountCost = Text(Columns[8], Rows[12], TextAnchor.MiddleLeft);
                    pfBankFee = Text(Columns[8], Rows[13], TextAnchor.MiddleLeft);
                    pfNpcGrowRate = Text(Columns[8], Rows[16], TextAnchor.MiddleLeft);
                    pfLevelupExp = Text(Columns[8], Rows[19], TextAnchor.MiddleLeft);
                    pfHideSaveButton = Text(Columns[18], Rows[1], TextAnchor.MiddleLeft);
                    pfHideReloadButton = Text(Columns[18], Rows[2], TextAnchor.MiddleLeft);
                    pfHideBattleMap = Text(Columns[18], Rows[3], TextAnchor.MiddleLeft);
                    pfNoRebirth = Text(Columns[18], Rows[4], TextAnchor.MiddleLeft);

                    //sum
                    DebugHelper.WriteLine("7");
                    txtSMScore = Text(Columns[27], Rows[0], TextAnchor.MiddleRight);
                    txtEndNote = Text(Columns[11], Rows[20], TextAnchor.MiddleCenter);
                    btnDefault = Button(Columns[27], Rows[2], "Default");
                    btnLevel01 = Button(Columns[27], Rows[4], "Level 1");
                    btnLevel02 = Button(Columns[27], Rows[6], "Level 2");
                    btnLevel03 = Button(Columns[27], Rows[8], "Level 3");
                    btnLevel04 = Button(Columns[27], Rows[10], "Level 4");
                    btnLevel05 = Button(Columns[27], Rows[12], "Level 5");
                    btnLevel06 = Button(Columns[27], Rows[14], "Level 6");
                    btnLevel07 = Button(Columns[27], Rows[16], "Level 7");
                    btnLevel08 = Button(Columns[27], Rows[18], "Level 8");
                    btnLevel09 = Button(Columns[27], Rows[20], "Level 9");
                    btnLevel10 = Button(Columns[27], Rows[22], "Level 10");

                    txtEndNote.text = $"You have to start a new game to apply these configs!";
                }
                DebugHelper.WriteLine("8");
                uiStt.gameObject.SetActive(true);
                g.ui.CloseUI(uiStt);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
            }
        }

        [ErrorIgnore]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            if (tglHideReloadButton.isOn)
                tglHideSaveButton.isOn = true;
            txtSMScore.text = $"Total score: {CalSMTotalScore()} points";
            pfAtkValue.text = $"+{slAtkValue.value * 100f:0}% atk ({CalCompScore(slAtkValue)} points)";
            pfDefValue.text = $"+{slDefValue.value * 100f:0}% def ({CalCompScore(slDefValue)} points)";
            pfHpValue.text = $"+{slHpValue.value * 100f:0}% hp ({CalCompScore(slHpValue)} points)";
            pfBasisValue.text = $"+{slBasisValue.value * 100f:0}% basis ({CalCompScore(slBasisValue)} points)";
            pfSpecialMonsterRate.text = $"+{slSpecialMonsterRate.value * 100f:0}% basis ({CalCompScore(slSpecialMonsterRate)} points)";
            pfTaxRate.text = $"+{slTaxRate.value * 100f:0}% basis ({CalCompScore(slTaxRate)} points)";
            pfInflationRate.text = $"+{slInflationRate.value * 100f:0}% basis ({CalCompScore(slInflationRate)} points)";
            pfBuildingCost.text = $"+{slBuildingCost.value * 100f:0}% basis ({CalCompScore(slBuildingCost)} points)";
            pfBankAccountCost.text = $"+{slBankAccountCost.value * 100f:0}% basis ({CalCompScore(slBankAccountCost)} points)";
            pfBankFee.text = $"+{slBankFee.value * 100f:0}% basis ({CalCompScore(slBankFee)} points)";
            pfNpcGrowRate.text = $"+{slNpcGrowRate.value * 100f:0}% basis ({CalCompScore(slNpcGrowRate)} points)";
            pfLevelupExp.text = $"+{slLevelupExp.value * 100f:0}% basis ({CalCompScore(slLevelupExp)} points)";
            pfHideSaveButton.text = $"({CalCompScore(tglHideSaveButton)} points)";
            pfHideReloadButton.text = $"({CalCompScore(tglHideReloadButton)} points)";
            pfHideBattleMap.text = $"({CalCompScore(tglHideBattleMap)} points)";
            pfNoRebirth.text = $"({CalCompScore(tglNoRebirth)} points)";
        }

        private void SetSMConfigs()
        {
            AddAtkRate = slAtkValue.value;
            AddDefRate = slDefValue.value;
            AddHpRate = slHpValue.value;
            AddBasisRate = slHpValue.value;
            AddSpecialMonsterRate = slSpecialMonsterRate.value;
            AddTaxRate = slTaxRate.value;
            AddInflationRate = slInflationRate.value;
            AddBuildingCostRate = slBuildingCost.value;
            AddBankAccountCostRate = slBankAccountCost.value;
            AddBankFee = slBankFee.value;
            AddNpcGrowRate = slNpcGrowRate.value;
            AddLevelupExpRate = slLevelupExp.value;
            HideSaveButton = tglHideSaveButton.hasSelection;
            HideReloadButton = tglHideReloadButton.hasSelection;
            HideBattleMap = tglHideBattleMap.hasSelection;
            NoRebirth = tglNoRebirth.hasSelection;
            CacheHelper.Save();
        }

        private int CalCompScore(dynamic comp)
        {
            if (comp == null)
                return 0;
            var x = ScoreCalculator.FirstOrDefault(k => k.Comp.Invoke() == comp);
            if (x == null || !x.Cond.Invoke(comp))
                return 0;
            return x.Cal.Invoke(comp);
        }

        private bool IsEnableComp(dynamic comp)
        {
            if (comp == null)
                return false;
            var x = ScoreCalculator.FirstOrDefault(k => k.Comp.Invoke() == comp);
            return x != null && x.Cond.Invoke(comp);
        }

        private int CalSMTotalScore()
        {
            return ScoreCalculator.Where(x => IsEnableComp(x.Comp.Invoke())).Sum(x => CalCompScore(x.Comp.Invoke()));
        }
    }
}
