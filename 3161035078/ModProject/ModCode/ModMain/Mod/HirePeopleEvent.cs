using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ModLib.Object;
using ModLib.Enum;
using System;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.HIRE_PEOPLE_EVENT)]
    public class HirePeopleEvent : ModEvent
    {
        public static HirePeopleEvent Instance { get; set; }

        public const int TEAM_LUCK_ID = 420041121;
        public const string TEAM_LUCK_DESC = "team420041120desc";
        public const int FRIEND_JOIN_DRAMA = 420041123;
        public const float FRIEND_JOIN_RATE = 5f;
        public const int MONTHLY_PAYMENT_RATIO = 10;

        public static UINPCInfo UINPCInfo;
        public static bool isShowHirePeopleUI = false;
        public static bool isShowManageTeamUI = false;

        public Dictionary<string, List<string>> TeamData { get; set; } = new Dictionary<string, List<string>>();

        [EventCondition]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.TownBounty.uiName)
            {
                var ui = new UICover<UITownBounty>(e.ui);
                {
                    ui.AddButton(0, 0, OpenUIHirePeople, "Hire People").Size(160, 40).Pos(ui.UI.btnTaskPut.transform, 4f, 0f);
                }
                ui.UpdateUI();
            }
            else
            if (e.uiType.uiName == UIType.NPCSearch.uiName && isShowManageTeamUI)
            {
                var ui = new UICover<UINPCSearch>(e.ui);
                {
                    ui.AddText(ui.MidCol, ui.MidRow, $"Team: {GetTeamInfoStr(g.world.playerUnit)}").Align().Format();
                    ui.AddText(ui.MidCol, ui.MidRow + 1, $"Payment: {GetTotalMonthlyPayment(g.world.playerUnit).ToString("#,##0")} Spirit Stones/month").Align().Format();
                }
                ui.UpdateUI();
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var ui = new UICover<UINPCInfo>(e.ui);
                if (ui.UI.unit.GetUnitId() == g.world.playerUnit.GetUnitId())
                {
                    ui.Dispose();
                }
                else
                {
                    if (IsHired(ui.UI.unit))
                    {
                        if (IsTeam(g.world.playerUnit, ui.UI.unit) && ui.UI.unit.data.unitData.relationData.GetIntim(g.world.playerUnit) < 200)
                            ui.AddText(0, 0, $"{GetRequiredSpiritStones(g.world.playerUnit, ui.UI.unit) / MONTHLY_PAYMENT_RATIO:#,##0} Spirit Stones/month").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.5f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                        ui.AddText(0, 0, $"Team: {GetTeamInfoStr(ui.UI.unit)}").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.25f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                    }
                    else
                    if (isShowHirePeopleUI)
                    {
                        if (ui.UI.unit.data.unitData.relationData.GetIntim(g.world.playerUnit) < 200)
                        {
                            ui.AddText(0, 0, $"{GetRequiredSpiritStones(g.world.playerUnit, ui.UI.unit):#,##0} Spirit Stones ({GetRequiredSpiritStones(g.world.playerUnit, ui.UI.unit) / MONTHLY_PAYMENT_RATIO:#,##0}/month)").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.5f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                            ui.AddText(0, 0, $"{GetRequiredReputations(g.world.playerUnit, ui.UI.unit):#,##0} Reputations").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.25f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                        }
                        else
                        {
                            ui.AddText(0, 0, $"You dont have to pay for friends!").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.5f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                        }
                        ui.AddButton(0, 0, () =>
                        {
                            PreHire(ui.UI.unit);
                        }, "Hire").Format(Color.black).Size(100, 40).Pos(ui.UI.uiProperty.textInTrait1.transform, -1.1f, 0.4f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                    }
                    else
                    if (isShowManageTeamUI)
                    {
                        ui.AddText(0, 0, $"{GetRequiredSpiritStones(g.world.playerUnit, ui.UI.unit):#,##0} Spirit Stones ({GetRequiredSpiritStones(g.world.playerUnit, ui.UI.unit) / MONTHLY_PAYMENT_RATIO:#,##0}/month)").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.5f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                        ui.AddText(0, 0, $"{GetRequiredReputations(g.world.playerUnit, ui.UI.unit):#,##0} Reputations").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.25f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                        ui.AddButton(0, 0, () =>
                        {
                            PreDismiss(ui.UI.unit);
                        }, "Dismiss").Format(Color.black).Size(100, 40).Pos(ui.UI.uiProperty.textInTrait1.transform, -1.1f, 0.4f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                    }
                    ui.UpdateUI();
                    UINPCInfo = ui.UI;
                }
            }
            else
            if (e.uiType.uiName == UIType.PlayerInfo.uiName)
            {
                var ui = new UICover<UIPlayerInfo>(e.ui);
                {
                    if (IsHired(ui.UI.unit))
                    {
                        ui.AddButton(0, 0, () => DismissTeam(g.world.playerUnit), "Dismiss Team").Size(160, 40).Pos(ui.UI.uiProperty.textInTrait_En.transform, -3.0f, 0.5f).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Component.gameObject.SetActive(TeamData.ContainsKey(g.world.playerUnit.GetUnitId())),
                        }).SetParentTransform(ui.UI.uiProperty.textInTrait_En.transform);
                        ui.AddButton(0, 0, OpenUIManageTeam, "Manage Team").Size(160, 40).Pos(ui.UI.uiProperty.textInTrait_En.transform, -1.5f, 0.5f).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Component.gameObject.SetActive(TeamData.ContainsKey(g.world.playerUnit.GetUnitId())),
                        }).SetParentTransform(ui.UI.uiProperty.textInTrait_En.transform);
                        ui.AddText(0, 0, "Team: {0}").Align().Format().Pos(ui.UI.uiProperty.textInTrait_En.transform, 0f, 0.5f).SetWork(new UIItemWork
                        {
                            Formatter = (x) => new object[] { GetTeamInfoStr(ui.UI.unit) },
                        }).SetParentTransform(ui.UI.uiProperty.textInTrait_En.transform);
                    }
                }
                ui.IsAutoUpdate = true;
            }
            else
            if (e.uiType.uiName == UIType.SkyTip.uiName)
            {
                var ui = g.ui.GetUI<UISkyTip>(UIType.SkyTip);
                if (ui.ptextTip.text == TEAM_LUCK_DESC)
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
            var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = GetHirablePeople().ToIl2CppList();
            ui.UpdateUI();
            isShowHirePeopleUI = true;
        }

        public static void OpenUIManageTeam()
        {
            var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = GetTeamMember(g.world.playerUnit).ToIl2CppList();
            ui.UpdateUI();
            isShowManageTeamUI = true;
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            //teamup with friends
            if (!wunit.IsPlayer() && !IsHired(wunit))
            {
                foreach (var relationWUnitIntim in wunit.data.unitData.relationData.intimToUnit)
                {
                    if (relationWUnitIntim.Value >= 200 && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, FRIEND_JOIN_RATE))
                    {
                        var relationWUnit = g.world.unit.GetUnit(relationWUnitIntim.Key);
                        if (relationWUnit == null || relationWUnit.isDie)
                            continue;
                        if (relationWUnit.data.unitData.relationData.GetIntim(wunit) >= 200)
                        {
                            if (relationWUnit.IsPlayer())
                            {
                                DramaTool.OpenDrama(FRIEND_JOIN_DRAMA, new DramaData() { unitLeft = wunit, unitRight = relationWUnit });
                                g.ui.MsgBox("Team", "Are you sure about adding this person?", MsgBoxButtonEnum.YesNo, () =>
                                {
                                    Hire(relationWUnit, wunit);
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
                var teamData = GetTeamData(wunit);
                if (teamData.HasValue)
                {
                    foreach (var memberId in teamData.Value.Value.ToArray())
                    {
                        if (memberId == masterId)
                            continue;
                        var member = g.world.unit.GetUnit(memberId);
                        if (member == null)
                        {
                            teamData.Value.Value.Remove(memberId);
                            continue;
                        }
                        if (member.isDie)
                        {
                            teamData.Value.Value.Remove(memberId);
                            member.DelLuck(TEAM_LUCK_ID);
                        }
                        //free if intim >= 400
                        else if (member.data.unitData.relationData.GetIntim(wunit) < 200)
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

                    if (teamData.Value.Value.Count == 0 || teamData.Value.Value.All(x => x == masterId))
                    {
                        Instance.TeamData.Remove(masterId);
                        wunit.DelLuck(TEAM_LUCK_ID);
                    }
                }
            }
        }

        public static void PreHire(WorldUnitBase wunit)
        {
            var player = g.world.playerUnit;
            var playerId = player.GetUnitId();

            if (IsHired(player) && !IsTeamMaster(player))
            {
                g.ui.MsgBox("Team", "You have to quit current team!");
                return;
            }

            var requiredSpiritStones = GetRequiredSpiritStones(player, wunit);
            if (player.GetUnitMoney() < requiredSpiritStones)
            {
                g.ui.MsgBox("Team", $"Require {requiredSpiritStones:#,##0} Spirit Stones");
                return;
            }

            var requiredReputations = GetRequiredReputations(player, wunit);
            if (player.GetDynProperty(UnitDynPropertyEnum.Reputation).value < requiredReputations)
            {
                g.ui.MsgBox("Team", $"Require {requiredReputations:#,##0} Reputations");
                return;
            }

            g.ui.MsgBox("Team", "Are you sure about adding this person?", MsgBoxButtonEnum.YesNo, () =>
            {
                Hire(g.world.playerUnit, wunit);
                if (wunit.data.unitData.relationData.GetIntim(player) < 200)
                    player.AddUnitMoney(-requiredSpiritStones);
                g.ui.CloseUI(UINPCInfo);
            });
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

        public static void PreDismiss(WorldUnitBase wunit)
        {
            g.ui.MsgBox("Team", "Are you sure about dismissing this person?", MsgBoxButtonEnum.YesNo, () =>
            {
                Dismiss(g.world.playerUnit, wunit);
                g.ui.CloseUI(UINPCInfo);
            });
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

        public static List<WorldUnitBase> GetHirablePeople()
        {
            var rs = new List<WorldUnitBase>();
            foreach (var wunit in g.world.playerUnit.GetUnitsAround(4, false, false))
            {
                if (!MapBuildPropertyEvent.IsTownGuardian(wunit) && !IsHired(wunit) &&
                    !rs.Any((x) => x.GetUnitId() == wunit.GetUnitId()))
                {
                    rs.Add(wunit);
                }
            }
            return rs;
        }

        public static WorldUnitBase[] GetTeamMember(WorldUnitBase wunit)
        {
            return GetTeamData(wunit).Value.Value.Select(x => g.world.unit.GetUnit(x)).Where(x => x != null).ToArray();
        }

        public static KeyValuePair<string, List<string>>? GetTeamData(WorldUnitBase wunit)
        {
            var wunitId = wunit.GetUnitId();
            if (!IsHired(wunit))
                return new KeyValuePair<string, List<string>>(wunitId, new List<string> { wunitId });
            return Instance.TeamData.First(x => x.Value.Contains(wunitId));
        }

        public static Tuple<WorldUnitBase, WorldUnitBase[]> GetTeamDetailData(WorldUnitBase wunit)
        {
            var teamData = GetTeamData(wunit);
            return Tuple.Create(g.world.unit.GetUnit(teamData.Value.Key), GetTeamMember(wunit));
        }

        public static WorldUnitBase GetTeamMaster(WorldUnitBase wunit)
        {
            return g.world.unit.GetUnit(GetTeamData(wunit).Value.Key);
        }

        public static string GetTeamInfoStr(WorldUnitBase wunit)
        {
            return GetTeamDetailData(wunit)?.Item1.data.unitData.propertyData.GetName();
        }

        public static bool IsHired(WorldUnitBase wunit)
        {
            return wunit.GetLuck(TEAM_LUCK_ID) != null;
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
            var k = (Instance.TeamData.ContainsKey(masterId) ? Instance.TeamData[masterId].Count : 1) + 1;
            return (Math.Pow(2, wunit.GetGradeLvl()) * 1000 * k).Parse<int>();
        }

        public static int GetRequiredReputations(WorldUnitBase master, WorldUnitBase wunit)
        {
            var masterId = master.GetUnitId();
            var k = (Instance.TeamData.ContainsKey(masterId) ? Instance.TeamData[masterId].Count : 1) + 1;
            return (Math.Pow(2, k) * 100 + Math.Pow(2, wunit.GetGradeLvl()) * 10).Parse<int>();
        }
    }
}
