using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using Il2CppSystem;
using System.Collections.Generic;
using System.Linq;
using ModLib.Object;
using ModLib.Enum;

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
                    ui.AddText(0, 0, "Team:").Align(TextAnchor.MiddleRight).Pos(ui.UI.uiProperty.textTrait.transform, 0f, 0.2f);
                    ui.AddText(0, 0, "").Align(TextAnchor.MiddleLeft).Pos(ui.UI.uiProperty.textTrait.transform, 0f, 0.2f);
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
                    ui.AddButton(0, 0, OpenUIManageTeam, "Manage Team").Size(160, 40).Pos(ui.UI.uiProperty.textStand1_En.transform, 0f, 0.5f).SetWork(new UIItemWork
                    {
                        UpdateAct = (x) => x.Component.gameObject.SetActive(TeamData.ContainsKey(g.world.playerUnit.GetUnitId())),
                    });
                    ui.AddText(0, 0, "Team:").Align(TextAnchor.MiddleRight).Pos(ui.UI.uiProperty.textStand1_En.transform, 0f, 0.2f);
                    ui.AddText(0, 0, "").Align(TextAnchor.MiddleLeft).Pos(ui.UI.uiProperty.textStand2_En.transform, 0f, 0.2f);
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
                    var teamId = TeamData.First(x => x.Value.Contains(wunitId)).Key;
                    var master = g.world.unit.GetUnit(teamId);
                    ui.ptextTip.text = $"{string.Join(" ", master.data.unitData.propertyData.name)}'s team";
                }
            }
        }

        public override void OnCloseUIStart(CloseUIStart e)
        {
            base.OnCloseUIStart(e);
            if (isShowHirePeopleUI && e.uiType.uiName == UIType.TownBounty.uiName)
            {
                isShowHirePeopleUI = false;
            }
        }

        public static void OpenUIHirePeople()
        {
            if (g.ui.HasUI(UIType.TownBounty))
            {
                //open select ui
                var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
                ui.units = GetHirablePeople();
                isShowHirePeopleUI = true;
            }
        }

        public static void OpenUIManageTeam()
        {
            if (g.ui.HasUI(UIType.PlayerInfo))
            {
                //open select ui
                var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
                ui.units = GetHiredPeople();
                isShowManageTeamUI = true;
            }
        }

        public static Il2CppSystem.Collections.Generic.List<WorldUnitBase> GetHirablePeople()
        {
            var rs = new Il2CppSystem.Collections.Generic.List<WorldUnitBase>();
            foreach (var wunit in g.world.playerUnit.GetUnitsAround(4, false, false))
            {
                if (wunit.GetLuck(MapBuildPropertyEvent.TOWN_MASTER_LUCK_ID) == null &&
                    wunit.GetLuck(MapBuildPropertyEvent.TOWN_GUARDIAN_LUCK_ID) == null &&
                    wunit.GetLuck(TEAM_LUCK_ID) == null)
                {
                    rs.Add(wunit);
                }
            }
            return rs;
        }

        public static Il2CppSystem.Collections.Generic.List<WorldUnitBase> GetHiredPeople()
        {
            var rs = new Il2CppSystem.Collections.Generic.List<WorldUnitBase>();
            foreach (var wunitId in Instance.TeamData[g.world.playerUnit.GetUnitId()])
            {
                rs.Add(g.world.unit.GetUnit(wunitId));
            }
            return rs;
        }

        public static bool Check(WorldUnitBase wunit)
        {
            var player = g.world.playerUnit;
            var playerId = player.GetUnitId();
            if (!Instance.TeamData.ContainsKey(playerId))
                Instance.TeamData.Add(playerId, new List<string>());

            if (Instance.TeamData.Any(x => x.Value.Contains(playerId)))
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

        public void Hire(WorldUnitBase wunit)
        {
        }

        public static int GetRequiredSpiritStones(WorldUnitBase master, WorldUnitBase wunit)
        {
            var k = Instance.TeamData[master.GetUnitId()].Count + 1;
            return (Math.Pow(2, wunit.GetGradeLvl()) * 1000 * k).Parse<int>();
        }

        public static int GetRequiredReputations(WorldUnitBase master, WorldUnitBase wunit)
        {
            var k = Instance.TeamData[master.GetUnitId()].Count + 1;
            return (Math.Pow(2, k) * 1000).Parse<int>();
        }
    }
}
