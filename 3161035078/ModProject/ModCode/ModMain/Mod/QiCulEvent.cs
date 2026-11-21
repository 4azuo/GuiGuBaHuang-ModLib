using MOD_nE7UL2.Helper;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Helper;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Qi cultivation: units with full exp accumulate Qi (from SP) which gives Atk/HP/MP bonuses; Qi can be transferred.
    /// </summary>
    [Cache(ModConst.QI_CUL_EVENT)]
    public class QiCulEvent : ModEvent
    {
        public static QiCulEvent Instance { get; set; }

        public const int QI_TRANSFER_DRAMA = 480010100;

        public int LastYearReceiveQi { get; set; }
        public IDictionary<string, long> Qi { get; set; } = new Dictionary<string, long>();

        /// <summary>
        /// Disabled by NoQiCultivation.
        /// </summary>
        public override bool OnCacheHandler()
        {
            return !SMLocalConfigsEvent.Instance.Configs.NoQiCultivation;
        }

        /// <summary>
        /// Adds Qi to units with full exp every GrowUpSpeed months.
        /// </summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            var ticks = ModLib.Mod.ModMaster.Ticks(SMLocalConfigsEvent.Instance.Configs.GrowUpSpeed);
            if (wunit.IsFullExp() && ticks > 0)
            {
                var unitId = wunit.GetUnitId();
                if (!Qi.ContainsKey(unitId))
                    Qi.Add(unitId, 0);
                // body refiners (BodyCulEvent) gather less Qi; a spirit gathering array at home (EstateEvent) more
                Qi[unitId] += (wunit.GetDynProperty(UnitDynPropertyEnum.Sp).value * CommonTool.Random(0.8f, 1.2f) * ticks * BodyCulEvent.QiRate(wunit) * EstateEvent.QiRate(wunit)).Parse<int>();
            }
        }

        /// <summary>
        /// Shows Qi and bonuses in FateFeature and NPCInfo; adds "transfer Qi" (and "receive from master" once a year).
        /// </summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.FateFeature.uiName)
            {
                var ui = new UICover<UIFateFeature>(e.ui);
                {
                    ui.AddToolTipButton(ui.MidCol - 4, ui.MidRow - 9, GameTool.LS("other500020019"));
                    ui.AddText(ui.MidCol, ui.MidRow - 9, ModTextHelper.LSF("ui_qi", Qi[g.world.playerUnit.GetUnitId()])).Format(Color.black, 16).SetParentTransform(ui.UI.gradeInfo.goBgProTop);
                    ui.AddText(ui.MidCol, ui.MidRow - 8, ModTextHelper.LSF("ui_atk", UnitModifyHelper.GetQiAdjAtk(g.world.playerUnit))).Format(Color.black, 16).SetParentTransform(ui.UI.gradeInfo.goBgProTop);
                    ui.AddText(ui.MidCol, ui.MidRow - 7, ModTextHelper.LSF("ui_hp", UnitModifyHelper.GetQiAdjHp(g.world.playerUnit))).Format(Color.black, 16).SetParentTransform(ui.UI.gradeInfo.goBgProTop);
                    ui.AddText(ui.MidCol, ui.MidRow - 6, ModTextHelper.LSF("ui_mp", UnitModifyHelper.GetQiAdjMp(g.world.playerUnit))).Format(Color.black, 16).SetParentTransform(ui.UI.gradeInfo.goBgProTop);
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
                        ui.AddText(ui.MidCol - 6, ui.MidRow - 4, ModTextHelper.LSF("ui_qi", Qi[unitId].ToString(ModConst.FORMAT_NUMBER))).Align().Format(Color.white, 16).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddText(ui.MidCol - 6, ui.MidRow - 3, ModTextHelper.LSF("ui_atk", UnitModifyHelper.GetQiAdjAtk(uiBase.unit))).Align().Format(Color.white, 16).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddText(ui.MidCol - 6, ui.MidRow - 2, ModTextHelper.LSF("ui_hp", UnitModifyHelper.GetQiAdjHp(uiBase.unit))).Align().Format(Color.white, 16).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                        ui.AddText(ui.MidCol - 6, ui.MidRow - 1, ModTextHelper.LSF("ui_mp", UnitModifyHelper.GetQiAdjMp(uiBase.unit))).Align().Format(Color.white, 16).SetParentTransform(ui.UI.uiHeart.goGroupRoot);
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

        /// <summary>
        /// Moves about 10 % of the sender Qi to the receiver (scaled by the receiver talent).
        /// </summary>
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
            InfoDialog.Msg(GameTool.LS("ui_qi_transfer"), ModTextHelper.LSF("ui_qi_change", oldFqi, Instance.Qi[fwunitId]));
        }
    }
}
