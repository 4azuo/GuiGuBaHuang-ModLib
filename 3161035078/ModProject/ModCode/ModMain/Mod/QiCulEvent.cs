using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.QI_CUL_EVENT)]
    public class QiCulEvent : ModEvent
    {
        public static QiCulEvent Instance { get; set; }

        public const int QI_TRANSFER_DRAMA = 480010100;

        public int LastYearReceiveQi { get; set; }
        public IDictionary<string, long> Qi { get; set; } = new Dictionary<string, long>();

        public override bool OnCacheHandler()
        {
            return !SMLocalConfigsEvent.Instance.Configs.NoQiCultivation;
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (wunit.IsFullExp() && GameHelper.GetGameTotalMonth() % SMLocalConfigsEvent.Instance.Configs.GrowUpSpeed == 0)
            {
                var unitId = wunit.GetUnitId();
                if (!Qi.ContainsKey(unitId))
                    Qi.Add(unitId, 0);
                Qi[unitId] += (wunit.GetDynProperty(UnitDynPropertyEnum.Sp).value * CommonTool.Random(0.8f, 1.2f)).Parse<int>();
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.FateFeature.uiName)
            {
                var ui = new UICover<UIFateFeature>(e.ui);
                {
                    ui.AddToolTipButton(ui.MidCol - 4, ui.MidRow - 9, GameTool.LS("other500020019"));
                    ui.AddText(ui.MidCol, ui.MidRow - 9, $"Qi: {Qi[g.world.playerUnit.GetUnitId()]}").Format(Color.black, 16).SetParentTransform(ui.UI.gradeInfo.goBgProTop);
                    ui.AddText(ui.MidCol, ui.MidRow - 8, $"Atk: {UnitModifyHelper.GetQiAdjAtk(g.world.playerUnit)}").Format(Color.black, 16).SetParentTransform(ui.UI.gradeInfo.goBgProTop);
                    ui.AddText(ui.MidCol, ui.MidRow - 7, $"Hp: {UnitModifyHelper.GetQiAdjHp(g.world.playerUnit)}").Format(Color.black, 16).SetParentTransform(ui.UI.gradeInfo.goBgProTop);
                    ui.AddText(ui.MidCol, ui.MidRow - 6, $"Mp: {UnitModifyHelper.GetQiAdjMp(g.world.playerUnit)}").Format(Color.black, 16).SetParentTransform(ui.UI.gradeInfo.goBgProTop);
                }
                ui.UpdateUI();
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var uiBase = g.ui.GetUI<UINPCInfo>(e.ui.uiType);
                var unitId = uiBase.unit.GetUnitId();
                if (Qi.ContainsKey(unitId) && !HirePeopleEvent.isShowHirePeopleUI)
                {
                    var ui = new UICover<UINPCInfo>(uiBase);
                    {
                        ui.AddText(ui.MidCol - 6, ui.MidRow - 4, $"Qi: {Qi[unitId].ToString(ModConst.FORMAT_NUMBER)}").Align().Format(Color.white, 16).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddText(ui.MidCol - 6, ui.MidRow - 3, $"Atk: {UnitModifyHelper.GetQiAdjAtk(uiBase.unit)}").Align().Format(Color.white, 16).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddText(ui.MidCol - 6, ui.MidRow - 2, $"Hp: {UnitModifyHelper.GetQiAdjHp(uiBase.unit)}").Align().Format(Color.white, 16).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddText(ui.MidCol - 6, ui.MidRow - 1, $"Mp: {UnitModifyHelper.GetQiAdjMp(uiBase.unit)}").Align().Format(Color.white, 16).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddButton(ui.MidCol - 6, ui.MidRow, () =>
                        {
                            QiTransmit(g.world.playerUnit, uiBase.unit);
                            g.ui.CloseUI(uiBase);
                        }, GameTool.LS("other500020071")).Format(Color.black, 14).Size(160, 30).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        if (!uiBase.unit.isDie &&
                            LastYearReceiveQi != GameHelper.GetGameYear() &&
                            g.world.playerUnit.data.allUnitRelation.ContainsKey(unitId) &&
                            g.world.playerUnit.data.allUnitRelation[unitId] == UnitRelationType.Master)
                        {
                            ui.AddButton(ui.MidCol - 6, ui.MidRow + 2, () =>
                            {
                                LastYearReceiveQi = GameHelper.GetGameYear();
                                QiTransmit(uiBase.unit, g.world.playerUnit);
                                g.ui.CloseUI(uiBase);
                            }, GameTool.LS("other500020072")).Format(Color.black, 14).Size(160, 30).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        }
                    }
                    ui.IsAutoUpdate = true;
                }
            }
        }

        public static void QiTransmit(WorldUnitBase fwunit, WorldUnitBase twunit)
        {
            var fwunitId = fwunit.GetUnitId();
            var twunitId = twunit.GetUnitId();
            var oldFqi = Instance.Qi[fwunitId];
            double fqi = (oldFqi / 10d) * CommonTool.Random(0.8f, 1.1f);
            var insightRate = twunit.GetDynProperty(UnitDynPropertyEnum.Talent).value / 100f;
            double tqi = fqi * insightRate * CommonTool.Random(0.8f, 1.1f);
            Instance.Qi[fwunitId] -= fqi.Parse<long>();
            Instance.Qi[twunitId] += tqi.Parse<long>();

            DramaTool.OpenDrama(QI_TRANSFER_DRAMA);
            g.ui.MsgBox("Qi Transfer", $"Qi {oldFqi}→{Instance.Qi[fwunitId]}");
        }
    }
}
