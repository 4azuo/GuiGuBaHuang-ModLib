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
            if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var ui = new UICover<UINPCInfo>(e.ui);
                {
                    ui.AddText(0, 0, "Team:").Align(TextAnchor.MiddleRight).Format(Color.white).Pos(ui.UI.uiProperty.textInTrait2.transform, 0f, 0.2f);
                    ui.AddText(0, 0, GetTeamInfoStr(ui.UI.unit)).Align(TextAnchor.MiddleLeft).Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.2f);
                    if (isShowHirePeopleUI)
                    {
                        ui.AddText(0, 0, $"{GetRequiredSpiritStones(g.world.playerUnit, ui.UI.unit)} Spirit Stones").Align().Format(Color.white, 16).Pos(ui.UI.goFocal.transform, 2f, 0.5f);
                        ui.AddText(0, 0, $"{GetRequiredReputations(g.world.playerUnit, ui.UI.unit)} Reputations").Align().Format(Color.white, 16).Pos(ui.UI.goFocal.transform, 2f, 0.3f);
                        ui.AddButton(0, 0, () =>
                        {
                            if (Check(ui.UI.unit))
                            {
                                Hire(ui.UI.unit);
                                g.ui.CloseUI(ui.UI);
                            }
                        }, "Hire").Format(Color.black, 14).Size(160, 30).Pos(ui.UI.goFocal.transform, 2f, 0f);
                    }
                }
                ui.UpdateUI();
            }
            else
            if (e.uiType.uiName == UIType.PlayerInfo.uiName)
            {
                var ui = new UICover<UIPlayerInfo>(e.ui);
                {
                    ui.AddButton(0, 0, OpenUIManageTeam, "Manage Team").Size(160, 40).Pos(ui.UI.uiProperty.textInTrait_En.transform, 0f, 0.5f).SetWork(new UIItemWork
                    {
                        UpdateAct = (x) => x.Component.gameObject.SetActive(TeamData.ContainsKey(g.world.playerUnit.GetUnitId())),
                    });
                    ui.AddText(0, 0, "Team:").Align(TextAnchor.MiddleRight).Pos(ui.UI.uiProperty.textInTrait_En.transform, 0f, 0.2f);
                    ui.AddText(0, 0, GetTeamInfoStr(ui.UI.unit)).Align(TextAnchor.MiddleLeft).Pos(ui.UI.uiProperty.textInTrait_En.transform, 0f, 0.2f);
                }
                ui.UpdateUI();
            }
            else
            if (e.uiType.uiName == UIType.SkyTip.uiName)
            {
                var ui = g.ui.GetUI<UISkyTip>(UIType.SkyTip);
                if (ui.ptextTip.text == TEAM_LUCK_DESC)
                {
                    var a = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);
                    var b = g.ui.GetUI<UIPlayerInfo>(UIType.PlayerInfo);
                    var wunitId = (a.unit ?? b.unit).GetUnitId();
                    var master = GetTeamMaster(g.world.unit.GetUnit(wunitId));
                    ui.ptextTip.text = $"{string.Join(" ", master.data.unitData.propertyData.name)}'s team";
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
            if (g.ui.HasUI(UIType.TownBounty))
            {
                //open select ui
                var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
                ui.units = GetHirablePeople();
                ui.Init();
                ui.UpdateUI();
                isShowHirePeopleUI = true;
            }
        }

        public static void OpenUIManageTeam()
        {
            if (g.ui.HasUI(UIType.PlayerInfo))
            {
                //open select ui
                var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
                ui.units = GetTeamMember(g.world.playerUnit);
                ui.Init();
                ui.UpdateUI();
                isShowManageTeamUI = true;
            }
        }

        public static bool Check(WorldUnitBase wunit)
        {
            var player = g.world.playerUnit;
            var playerId = player.GetUnitId();
            if (!Instance.TeamData.ContainsKey(playerId))
                Instance.TeamData.Add(playerId, new List<string>());

            if (IsHired(player))
            {
                g.ui.MsgBox("Team", "You have to quit current team!");
                return false;
            }

            var requiredSpiritStones = GetRequiredSpiritStones(player, wunit);
            if (player.GetUnitMoney() < requiredSpiritStones)
            {
                g.ui.MsgBox("Team", $"Require {requiredSpiritStones:0,000} Spirit Stones");
                return false;
            }

            var k = Instance.TeamData[playerId].Count + 1;
            var requiredReputations = GetRequiredReputations(player, wunit);
            if (player.GetDynProperty(UnitDynPropertyEnum.Reputation).value < requiredReputations)
            {
                g.ui.MsgBox("Team", $"Require {requiredReputations:0,000} Reputations");
                return false;
            }

            return true;
        }

        public static void Hire(WorldUnitBase wunit)
        {
        }

        public static Il2CppSystem.Collections.Generic.List<WorldUnitBase> GetHirablePeople()
        {
            var rs = new Il2CppSystem.Collections.Generic.List<WorldUnitBase>();
            foreach (var wunit in g.world.playerUnit.GetUnitsAround(4, false, false))
            {
                if (wunit.GetLuck(MapBuildPropertyEvent.TOWN_MASTER_LUCK_ID) == null &&
                    wunit.GetLuck(MapBuildPropertyEvent.TOWN_GUARDIAN_LUCK_ID) == null &&
                    !IsHired(wunit))
                {
                    rs.Add(wunit);
                }
            }
            return rs;
        }

        public static Il2CppSystem.Collections.Generic.List<WorldUnitBase> GetTeamMember(WorldUnitBase wunit)
        {
            var rs = new Il2CppSystem.Collections.Generic.List<WorldUnitBase>();
            var teamData = GetTeamData(wunit);
            if (!teamData.HasValue)
                return rs;
            foreach (var wunitId in teamData.Value.Value)
            {
                rs.Add(g.world.unit.GetUnit(wunitId));
            }
            return rs;
        }

        public static KeyValuePair<string, List<string>>? GetTeamData(WorldUnitBase wunit)
        {
            if (!IsHired(wunit))
                return null;
            var wunitId = wunit.GetUnitId();
            var teamData = Instance.TeamData.First(x => x.Value.Contains(wunitId));
            return teamData;
        }

        public static Tuple<WorldUnitBase, List<WorldUnitBase>> GetTeamDetailData(WorldUnitBase wunit)
        {
            var teamData = GetTeamData(wunit);
            if (!teamData.HasValue)
                return null;
            return Tuple.Create(g.world.unit.GetUnit(teamData.Value.Key), teamData.Value.Value.Select(x => g.world.unit.GetUnit(x)).ToList());
        }

        public static WorldUnitBase GetTeamMaster(WorldUnitBase wunit)
        {
            var teamData = GetTeamData(wunit);
            if (!teamData.HasValue)
                return null;
            return g.world.unit.GetUnit(teamData.Value.Key);
        }

        public static string GetTeamInfoStr(WorldUnitBase wunit)
        {
            var teamData = GetTeamDetailData(wunit);
            if (teamData == null)
                return null;
            return $"{string.Join(" ", teamData.Item1.data.unitData.propertyData.name)} ({teamData.Item2.Count} members)";
        }

        public static bool IsHired(WorldUnitBase wunit)
        {
            return wunit.GetLuck(TEAM_LUCK_ID) != null;
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
            return (Math.Pow(2, k) * 1000).Parse<int>();
        }
    }
}
