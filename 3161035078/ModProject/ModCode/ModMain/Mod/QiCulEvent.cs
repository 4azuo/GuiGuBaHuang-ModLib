using EGameTypeData;
using MOD_nE7UL2.Const;
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

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            foreach (var wunit in g.world.unit.GetUnits())
            {
                var unitId = wunit.GetUnitId();
                if (!Qi.ContainsKey(unitId))
                    Qi.Add(unitId, 0);
            }
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (wunit.IsFullExp())
            {
                var unitId = wunit.GetUnitId();
                if (!Qi.ContainsKey(unitId))
                    Qi.Add(unitId, 0);
                Qi[unitId] += (wunit.GetDynProperty(UnitDynPropertyEnum.Mp).value * CommonTool.Random(0.8f, 1.2f)).Parse<int>();
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.FateFeature.uiName)
            {
                var ui = new UICover<UIFateFeature>(e.ui);
                {
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
                if (Qi.ContainsKey(unitId))
                {
                    var ui = new UICover<UINPCInfo>(uiBase);
                    {
                        ui.AddText(0, 0, $"Qi: {Qi[unitId]}").Align().Format(Color.white, 16).Pos(ui.UI.uiHeart.goGroupRoot.transform, -2.5f, 1.2f).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddText(0, 0, $"Atk: {UnitModifyHelper.GetQiAdjAtk(uiBase.unit)}").Align().Format(Color.white, 16).Pos(ui.UI.uiHeart.goGroupRoot.transform, -2.5f, 1.0f).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddText(0, 0, $"Hp: {UnitModifyHelper.GetQiAdjHp(uiBase.unit)}").Align().Format(Color.white, 16).Pos(ui.UI.uiHeart.goGroupRoot.transform, -2.5f, 0.8f).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddText(0, 0, $"Mp: {UnitModifyHelper.GetQiAdjMp(uiBase.unit)}").Align().Format(Color.white, 16).Pos(ui.UI.uiHeart.goGroupRoot.transform, -2.5f, 0.6f).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddButton(0, 0, () =>
                        {
                            QiTransmit(g.world.playerUnit, uiBase.unit);
                            g.ui.CloseUI(uiBase);
                        }, "Qi Transfer").Format(Color.black, 14).Size(160, 30).Pos(ui.UI.uiHeart.goGroupRoot.transform, -2.5f, 0.3f).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        if (!uiBase.unit.isDie &&
                            LastYearReceiveQi != GameHelper.GetGameYear() &&
                            g.world.playerUnit.data.allUnitRelation.ContainsKey(unitId) &&
                            g.world.playerUnit.data.allUnitRelation[unitId] == UnitRelationType.Master)
                        {
                            ui.AddButton(0, 0, () =>
                            {
                                LastYearReceiveQi = GameHelper.GetGameYear();
                                QiTransmit(uiBase.unit, g.world.playerUnit);
                                g.ui.CloseUI(uiBase);
                            }, "Recieve Qi").Format(Color.black, 14).Size(160, 30).Pos(ui.UI.uiHeart.goGroupRoot.transform, -2.5f, 0f).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        }
                    }
                    ui.IsAutoUpdate = true;
                }
            }
        }

        public static void QiTransmit(WorldUnitBase fwunit, WorldUnitBase twunit)
        {
            var fwunitId = fwunit.GetUnitId();
            var twunitId = fwunit.GetUnitId();
            var oldFqi = Instance.Qi[fwunitId];
            var fqi = (oldFqi / 10d) * CommonTool.Random(0.8f, 1.1f);
            var tqi = fqi * (twunit.GetDynProperty(UnitDynPropertyEnum.Talent).value / 100d) * CommonTool.Random(0.8f, 1.1f);
            Instance.Qi[fwunitId] -= fqi.Parse<long>();
            Instance.Qi[twunitId] += tqi.Parse<long>();

            DramaTool.OpenDrama(QI_TRANSFER_DRAMA);
            g.ui.MsgBox("Qi Transfer", $"Qi {oldFqi}→{Instance.Qi[fwunitId]}");
        }
    }
}
