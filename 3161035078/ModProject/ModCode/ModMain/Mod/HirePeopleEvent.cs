using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using static MOD_nE7UL2.Object.ModStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.HIRE_PEOPLE_EVENT)]
    public class HirePeopleEvent : ModEvent
    {
        public static HirePeopleEvent Instance { get; set; }

        public const int TEAM_LUCK_ID = 420041121;
        public const string TEAM_LUCK_DESC = "team420041120desc";
        public const int FRIEND_JOIN_DRAMA = 420041123;
        public const int FRIEND_JOIN_DRAMA_OPT1 = 420041124;
        public const int FRIEND_JOIN_DRAMA_OPT2 = 420041125;
        public const float FRIEND_JOIN_RATE = 10f;
        public const int MONTHLY_PAYMENT_RATIO = 10;
        public const int FRIEND_INTIM = 180;

        public static bool isShowHirePeopleUI = false;
        public static bool isShowManageTeamUI = false;

        public static _BattleConfigs BattleConfigs => ModMain.ModObj.ModSettings.BattleConfigs;

        public Dictionary<string, List<string>> TeamData { get; set; } = new Dictionary<string, List<string>>();

        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.TownBounty.uiName)
            {
                var uiCover = new UICover<UITownBounty>(e.ui);
                {
                    uiCover.AddButton(0, 0, OpenUIHirePeople, GameTool.LS("team420041131")).Size(160, 40).Pos(uiCover.UI.btnTaskPut.transform, 400, 0);
                }
                uiCover.UpdateUI();
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var ui = g.ui.GetUI<UINPCInfo>(e.uiType);
                if (ui.unit.GetUnitId() != g.world.playerUnit.GetUnitId() && !MapBuildPropertyEvent.isShowHirePeopleUI)
                {
                    var uiCover = new UICover<UINPCInfo>(e.ui);
                    //hired unit
                    if (IsHired(uiCover.UI.unit))
                    {
                        if (IsTeam(g.world.playerUnit, uiCover.UI.unit) && !IsFriend(g.world.playerUnit, uiCover.UI.unit))
                            uiCover.AddText(300f, 100f, string.Format(GameTool.LS("other500020043"), GetRequiredSpiritStones(g.world.playerUnit, uiCover.UI.unit) / MONTHLY_PAYMENT_RATIO)).Align().Format(Color.white)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                        uiCover.AddText(300f, 80f, $"Team: {GetTeamInfoStr(uiCover.UI.unit)}").Align().Format(Color.white)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                        if (isShowManageTeamUI || IsTeam(g.world.playerUnit, uiCover.UI.unit))
                        {
                            uiCover.AddButton(200f, 90f, () =>
                            {
                                g.ui.MsgBox(GameTool.LS("team420041121"), GameTool.LS("team420041130"), MsgBoxButtonEnum.YesNo, () =>
                                {
                                    Dismiss(g.world.playerUnit, uiCover.UI.unit);
                                    g.ui.CloseUI(e.ui);
                                });
                            }, GameTool.LS("other500020012")).Format(Color.black).Size(100, 40)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                        }
                    }
                    //hirable unit
                    else
                    {
                        if (!MapBuildPropertyEvent.IsTownGuardian(uiCover.UI.unit))
                        {
                            if (!IsFriend(g.world.playerUnit, uiCover.UI.unit))
                            {
                                uiCover.AddText(300f, 100f, $"{GetRequiredSpiritStones(g.world.playerUnit, uiCover.UI.unit).ToString(ModConst.FORMAT_NUMBER)} Spirit Stones ({GetRequiredSpiritStones(g.world.playerUnit, uiCover.UI.unit) / MONTHLY_PAYMENT_RATIO:#,##0}/month)").Align().Format(Color.white)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                                uiCover.AddText(300f, 80f, $"{GetRequiredReputations(g.world.playerUnit, uiCover.UI.unit).ToString(ModConst.FORMAT_NUMBER)} Reputations").Align().Format(Color.white)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                            }
                            else
                            {
                                uiCover.AddText(300f, 100f, GameTool.LS("team420041132")).Align().Format(Color.white)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                                uiCover.AddText(300f, 80f, $"{GetRequiredReputations(g.world.playerUnit, uiCover.UI.unit):#,##0} Reputations").Align().Format(Color.white)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                            }
                            uiCover.AddButton(200f, 90f, () =>
                            {
                                var player = g.world.playerUnit;
                                var isFriend = IsFriend(player, uiCover.UI.unit);

                                if (IsHired(player) && !IsTeamMaster(player))
                                {
                                    g.ui.MsgBox(GameTool.LS("team420041121"), GameTool.LS("team420041129"));
                                    return;
                                }

                                var requiredSpiritStones = GetRequiredSpiritStones(player, uiCover.UI.unit);
                                if (!isFriend && player.GetUnitMoney() < requiredSpiritStones)
                                {
                                    g.ui.MsgBox(GameTool.LS("team420041121"), $"Require {requiredSpiritStones:#,##0} Spirit Stones");
                                    return;
                                }

                                var requiredReputations = GetRequiredReputations(player, uiCover.UI.unit);
                                if (player.GetDynProperty(UnitDynPropertyEnum.Reputation).value < requiredReputations)
                                {
                                    g.ui.MsgBox(GameTool.LS("team420041121"), $"Require {requiredReputations:#,##0} Reputations");
                                    return;
                                }

                                g.ui.MsgBox(GameTool.LS("team420041121"), GameTool.LS("team420041128"), MsgBoxButtonEnum.YesNo, () =>
                                {
                                    Hire(g.world.playerUnit, uiCover.UI.unit);
                                    if (!isFriend)
                                        player.AddUnitMoney(-requiredSpiritStones);
                                    g.ui.CloseUI(e.ui);
                                });
                            }, GameTool.LS("team420041133")).Format(Color.black).Size(100, 40)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                        }
                    }
                    uiCover.UpdateUI();
                }
            }
            else
            if (e.uiType.uiName == UIType.PlayerInfo.uiName)
            {
                var uiCover = new UICover<UIPlayerInfo>(e.ui);
                {
                    if (IsHired(uiCover.UI.unit))
                    {
                        var x = 300;
                        var pos = uiCover.UI.uiSkill.goActionMartialRoot.Pos();
                        if (IsTeamMaster(g.world.playerUnit))
                        {
                            uiCover.AddButton(0, 0, () =>
                            {
                                g.ui.MsgBox(GameTool.LS("team420041121"), GameTool.LS("team420041134"), MsgBoxButtonEnum.YesNo, () =>
                                {
                                    DismissTeam(g.world.playerUnit);
                                    g.ui.CloseUI(e.ui);
                                });
                            }, GameTool.LS("team420041126")).Size(160, 40).Pos(pos, x + 0, 329);
                        }
                        uiCover.AddButton(0, 0, OpenUIManageTeam, GameTool.LS("team420041127")).Size(160, 40).Pos(pos, x + 170, 329);
                        uiCover.AddText(0, 0, $"Team: {GetTeamInfoStr(uiCover.UI.unit)}").Align().Format().Pos(pos, x + 320, 329);
                    }
                }
                uiCover.IsAutoUpdate = true;
            }
            else
            if (e.uiType.uiName == UIType.SkyTip.uiName)
            {
                var ui = g.ui.GetUI<UISkyTip>(UIType.SkyTip);
                if (ui?.ptextTip.text == TEAM_LUCK_DESC)
                {
                    var a = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);
                    var b = g.ui.GetUI<UIPlayerInfo>(UIType.PlayerInfo);
                    var wunit = a?.unit ?? b?.unit;
                    ui.ptextTip.text = GetTeamInfoStr(wunit);
                }
            }
        }

        public override void OnCloseUIStart(CloseUIStart e)
        {
            base.OnCloseUIStart(e);
            if (e.uiType.uiName == UIType.NPCSearch.uiName)
            {
                if (isShowHirePeopleUI)
                    isShowHirePeopleUI = false;
                else if (isShowManageTeamUI)
                    isShowManageTeamUI = false;
            }
        }

        public static void OpenUIHirePeople()
        {
            var ui = g.ui.OpenUISafe<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = GetHirablePeople().ToIl2CppList();
            ui.UpdateUI();
            isShowHirePeopleUI = true;
        }

        public static void OpenUIManageTeam()
        {
            var ui = g.ui.OpenUISafe<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = GetTeamMember(g.world.playerUnit).ToIl2CppList();
            ui.UpdateUI();
            isShowManageTeamUI = true;

            var uiCover = new UICover<UINPCSearch>(ui);
            {
                uiCover.AddText(uiCover.MidCol, uiCover.MidRow, $"Team: {GetTeamInfoStr(g.world.playerUnit)}").Align().Format();
                uiCover.AddText(uiCover.MidCol, uiCover.MidRow + 1, $"Payment: {GetTotalMonthlyPayment(g.world.playerUnit).ToString("#,##0")} Spirit Stones/month").Align().Format();
            }
            uiCover.UpdateUI();
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);

            if (wunit == null)
                return;

            //teamup with friends
            if (!wunit.IsPlayer() && !IsHired(wunit))
            {
                foreach (var relationWUnitIntim in wunit.data.unitData.relationData.intimToUnit)
                {
                    if (relationWUnitIntim.Value >= FRIEND_INTIM && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, FRIEND_JOIN_RATE))
                    {
                        var relationWUnit = g.world.unit.GetUnit(relationWUnitIntim.Key);
                        if (relationWUnit == null || relationWUnit.isDie)
                            continue;
                        if (/*1*/IsFriend(wunit, relationWUnit) ||
                            /*2*/(
                                    IsFriend(wunit, relationWUnit, 0.3f) && 
                                    relationWUnit.data.school?.schoolNameID != null && 
                                    relationWUnit.data.school?.schoolNameID == wunit.data.school?.schoolNameID
                                ) ||
                            /*3*/(
                                    BattleEvent.IsFriendlyUnit(wunit, relationWUnit) >= 0 &&
                                    relationWUnit.IsRighteous() == wunit.IsRighteous() &&
                                    Math.Abs(relationWUnit.GetStandValue() - wunit.GetStandValue()) < BattleConfigs.DifferenceRighteous
                                )
                            )
                        {
                            if (relationWUnit.IsPlayer())
                            {
                                DramaTool.OpenDrama(FRIEND_JOIN_DRAMA, new DramaData()
                                {
                                    unitLeft = wunit,
                                    unitRight = relationWUnit,
                                    onOptionsClickCall = ModLib.Helper.ActionHelper.TracedIl2Action<ConfDramaOptionsItem>((x) =>
                                    {
                                        switch (x.id)
                                        {
                                            case FRIEND_JOIN_DRAMA_OPT1:
                                                Hire(relationWUnit, wunit);
                                                break;
                                        }
                                    })
                                });
                            }
                            else
                            {
                                Hire(relationWUnit, wunit);
                            }
                        }
                    }
                }
            }
            //payment
            var masterId = wunit.GetUnitId();
            if (TeamData.ContainsKey(masterId))
            {
                if (wunit.isDie)
                {
                    DismissTeam(wunit);
                }
                else
                {
                    var teamData = GetTeamData(wunit);
                    foreach (var memberId in teamData.Value.ToArray())
                    {
                        if (memberId == masterId)
                            continue;
                        var member = g.world.unit.GetUnit(memberId);
                        if (member == null)
                        {
                            teamData.Value.Remove(memberId);
                            continue;
                        }
                        if (member.isDie)
                        {
                            teamData.Value.Remove(memberId);
                            member.DelLuck(TEAM_LUCK_ID);
                        }
                        //free if friend
                        else if (!IsFriend(wunit, member))
                        {
                            var requiredSpiritStones = GetRequiredSpiritStones(wunit, member) / MONTHLY_PAYMENT_RATIO;
                            if (wunit.GetUnitMoney() < requiredSpiritStones)
                            {
                                Dismiss(wunit, member);
                                member.data.unitData.relationData.AddHate(masterId, 50);
                            }
                            else
                            {
                                wunit.AddUnitMoney(-requiredSpiritStones);
                                member.SetUnitPos(wunit.GetUnitPos());
                            }
                        }
                    }

                    if (teamData.Value.Count == 0 || teamData.Value.All(x => x == masterId))
                    {
                        Instance.TeamData.Remove(masterId);
                        wunit.DelLuck(TEAM_LUCK_ID);
                    }
                }
            }
        }

        public static bool IsFriend(WorldUnitBase org, WorldUnitBase tar, float r = 1.0f)
        {
            return tar.data.unitData.relationData.GetIntim(org) >= FRIEND_INTIM * r;
        }

        public static void Hire(WorldUnitBase master, WorldUnitBase member)
        {
            var masterId = master.GetUnitId();
            if (!Instance.TeamData.ContainsKey(masterId))
            {
                Instance.TeamData.Add(masterId, new List<string>());
                Instance.TeamData[masterId].Add(masterId);
                master.AddLuck(TEAM_LUCK_ID);
            }
            var teamData = Instance.TeamData[masterId];
            teamData.Add(member.GetUnitId());
            member.AddLuck(TEAM_LUCK_ID);
        }

        public static void Dismiss(WorldUnitBase master, WorldUnitBase member)
        {
            var masterId = master.GetUnitId();
            var memberId = member.GetUnitId();
            if (masterId == memberId)
            {
                DismissTeam(master);
                return;
            }
            if (!Instance.TeamData.ContainsKey(masterId) || !Instance.TeamData[masterId].Contains(memberId))
                return;
            var teamData = Instance.TeamData[masterId];
            teamData.RemoveAll(x => x == memberId);
            member.DelLuck(TEAM_LUCK_ID);

            if (teamData.Count == 0 || teamData.All(x => x == masterId))
            {
                Instance.TeamData.Remove(masterId);
                master.DelLuck(TEAM_LUCK_ID);
            }
        }

        public static void DismissTeam(WorldUnitBase master)
        {
            var teamData = GetTeamDetailData(master);
            if (teamData != null)
            {
                var masterId = master.GetUnitId();
                foreach (var member in teamData.Item2.ToArray())
                {
                    var memberId = member.GetUnitId();
                    if (masterId != memberId)
                        Dismiss(master, member);
                }
                Instance.TeamData.Remove(masterId);
            }
        }

        public static bool IsMatchCondWUnit(WorldUnitBase wunit)
        {
            return !MapBuildPropertyEvent.IsTownGuardian(wunit) && !IsHired(wunit);
        }

        public static List<WorldUnitBase> GetHirablePeople()
        {
            var rs = new List<WorldUnitBase>();
            foreach (var wunit in g.world.playerUnit.GetUnitsAround(4, false, false))
            {
                if (IsMatchCondWUnit(wunit) && wunit.GetGradeLvl() < g.world.playerUnit.GetGradeLvl() + 2 &&
                    !rs.Any((x) => x.GetUnitId() == wunit.GetUnitId()))
                {
                    rs.Add(wunit);
                }
            }
            return rs;
        }

        public static WorldUnitBase[] GetTeamMember(WorldUnitBase wunit)
        {
            return GetTeamData(wunit).Value.Select(x => g.world.unit.GetUnit(x)).Where(x => x != null).ToArray();
        }

        public static KeyValuePair<string, List<string>> GetTeamData(WorldUnitBase wunit)
        {
            var wunitId = wunit.GetUnitId();
            if (!IsHired(wunit) || !Instance.TeamData.Any(x => x.Value.Contains(wunitId)))
                return new KeyValuePair<string, List<string>>(wunitId, new List<string> { wunitId });
            return Instance.TeamData.First(x => x.Value.Contains(wunitId));
        }

        public static Tuple<WorldUnitBase, WorldUnitBase[]> GetTeamDetailData(WorldUnitBase wunit)
        {
            var teamData = GetTeamData(wunit);
            return Tuple.Create(g.world.unit.GetUnit(teamData.Key), GetTeamMember(wunit));
        }

        public static WorldUnitBase GetTeamMaster(WorldUnitBase wunit)
        {
            return g.world.unit.GetUnit(GetTeamData(wunit).Key);
        }

        public static string GetTeamInfoStr(WorldUnitBase wunit)
        {
            return GetTeamDetailData(wunit)?.Item1.GetName();
        }

        public static bool IsHired(WorldUnitBase wunit)
        {
            return wunit != null && /*!wunit.isDie && */wunit.GetLuck(TEAM_LUCK_ID) != null;
        }

        public static bool IsTeamMaster(WorldUnitBase wunit)
        {
            return Instance.TeamData.ContainsKey(wunit.GetUnitId());
        }

        public static bool IsTeam(WorldUnitBase master, WorldUnitBase member)
        {
            var masterId = master.GetUnitId();
            return Instance.TeamData.ContainsKey(masterId) && Instance.TeamData[masterId].Contains(member.GetUnitId());
        }

        public static int GetTotalMonthlyPayment(WorldUnitBase master)
        {
            var rs = 0;
            var masterId = master.GetUnitId();
            foreach (var member in GetTeamDetailData(master).Item2)
            {
                if (member.GetUnitId() != masterId)
                    rs += GetRequiredSpiritStones(master, member) / MONTHLY_PAYMENT_RATIO;
            }
            return rs;
        }

        public static int GetRequiredSpiritStones(WorldUnitBase master, WorldUnitBase wunit)
        {
            var masterId = master.GetUnitId();
            var lvl = wunit.GetGradeLvl();
            var k = (Instance.TeamData.ContainsKey(masterId) ? Instance.TeamData[masterId].Count : 1) + 1;
            return 
                (
                    (Math.Pow(2, lvl) * 1000 * k).Parse<int>() + 
                    wunit.GetDynProperty(UnitDynPropertyEnum.AbilityPoint).value * lvl
                ) *
                (wunit.IsHero() ? 2 : 1);
        }

        public static int GetRequiredReputations(WorldUnitBase master, WorldUnitBase wunit)
        {
            var masterId = master.GetUnitId();
            var k = (Instance.TeamData.ContainsKey(masterId) ? Instance.TeamData[masterId].Count : 1) + 1;
            return (Math.Pow(2, k) * 100 + Math.Pow(2, wunit.GetGradeLvl()) * 10).Parse<int>();
        }
    }
}
