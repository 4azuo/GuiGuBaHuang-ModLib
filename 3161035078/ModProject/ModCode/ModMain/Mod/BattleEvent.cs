using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.Object.ModStts;
using ModLib.Helper;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_EVENT)]
    public class BattleEvent : ModEvent
    {
        public static BattleEvent Instance { get; set; }
        public static _BattleConfigs Configs => ModMain.ModObj.ModSettings.BattleConfigs;

        public const int ENEMY_JOIN_DRAMA = 480110100;
        public const int FRIENDLY_JOIN_DRAMA = 480110200;
        public const int SECT_MEMBER_JOIN_DRAMA = 480110300;
        public const int TOWN_GUARD_JOIN_DRAMA = 480110400;
        public const int TEAM_MEMBER_BETRAY_DRAMA = 480110500;
        //public const int STALKER_KILLBOSS_DRAMA = 480110700;
        public const int RIGHTEOUS_DRAMA = 480110501;
        public const int EVIL_DRAMA = 480110502;
        public const float ARGUE_RATE = 20.00f;

        public float AvgGrade { get; private set; }
        [JsonIgnore]
        public List<WorldUnitBase> AroundUnits { get; } = new List<WorldUnitBase>();
        [JsonIgnore]
        public List<WorldUnitBase> TeamMembers { get; } = new List<WorldUnitBase>();
        [JsonIgnore]
        public List<UnitCtrlBase> JointTeamMembers { get; } = new List<UnitCtrlBase>();

        public static readonly int[] friendlyInTraits = new int[] { UnitTraitEnum.Selfless.Parse<int>() };
        public static readonly int[] enemyInTraits = new int[] { UnitTraitEnum.Wicked.Parse<int>(), UnitTraitEnum.Selfish.Parse<int>(), UnitTraitEnum.Evil.Parse<int>() };
        public static readonly int[] betrayInTraits = new int[] { UnitTraitEnum.Selfish.Parse<int>(), UnitTraitEnum.Evil.Parse<int>() };
        public static readonly int[] betrayOutTraits = new int[] { UnitTraitEnum.Power_hungry.Parse<int>() };

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);

            AvgGrade = g.world.playerUnit.GetGradeLvl() - 1;
            AroundUnits.Clear();
            TeamMembers.Clear();
            JointTeamMembers.Clear();
            if (DungeonHelper.IsRealBattle() && IsJoinableBattle())
            {
                var player = g.world.playerUnit;

                //setup around units
                AroundUnits.AddRange(player.GetUnitsAround(Configs.JoinRange, false, false).ToArray().Where(x =>
                {
                    return CondJoinBattle(x) && 
                        (
                            IsFriendlyUnit(g.world.playerUnit, x) >= 0 || 
                            IsEnemyUnit(g.world.playerUnit, x) >= 0 || 
                            MapBuildPropertyEvent.IsTownGuardian(x) || 
                            MapBuildPropertyEvent.IsSchoolMember(ModBattleEvent.School, x) ||
                            BattleAfterEvent.Instance.Stalkers.ContainsKey(x.GetUnitId())
                        );
                }));

                //team member join
                TeamMembers.AddRange(HirePeopleEvent.GetTeamDetailData(player).Item2.Where(x => !x.isDie && !x.IsPlayer()));
            }
        }

        public override void OnBattleEndHandler(BattleEndHandler e)
        {
            base.OnBattleEndHandler(e);

            var player = g.world.playerUnit;
            var playerId = g.world.playerUnit.GetUnitId();
            var teamInfo = HirePeopleEvent.GetTeamDetailData(player);
            foreach (var wunit in teamInfo.Item2)
            {
                if (HirePeopleEvent.IsFriend(player, wunit, 0.2f) && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, ARGUE_RATE))
                {
                    wunit.data.unitData.relationData.AddHate(playerId, 5);
                }
            }
        }

        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.Ignore, IsInBattle = HandleEnum.True)]
        public override void OnTimeUpdate1000ms()
        {
            base.OnTimeUpdate1000ms();
            if (ModBattleEvent.SceneBattle != null && IsJoinableBattle())
            {
                if (DungeonHelper.IsRealBattle() && ModBattleEvent.SceneBattle.battleMap.isStartBattle &&
                    ModBattleEvent.PlayerUnit != null && !ModBattleEvent.PlayerUnit.isDie && !ModBattleEvent.SceneBattle.unit.IsUnitHide(ModBattleEvent.PlayerUnit))
                {
                    if (AroundUnits.Count > 0)
                    {
                        //stalker join battle
                        if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.StalkerJoinRate))
                        {
                            var stalker = AroundUnits.FirstOrDefault(x => BattleAfterEvent.Instance.Stalkers.ContainsKey(x.GetUnitId()));
                            if (stalker != null)
                            {
                                var wunitId = stalker.GetUnitId();
                                DebugHelper.WriteLine($"Stalker join: {stalker.GetName()} ({wunitId})");
                                var units = HirePeopleEvent.GetTeamDetailData(stalker).Item2;
                                AroundUnits.RemoveAll(x => units.Any(y => y.GetUnitId() == x.GetUnitId()));
                                foreach (var unit in units) { BattleAfterEvent.Instance.Stalkers.Remove(unit.GetUnitId()); }
                                NPCJoin(UnitType.Monst, units);

                                var stalkReason = BattleAfterEvent.Instance.Stalkers[wunitId];
                                switch (stalkReason)
                                {
                                    case Enum.StalkReasonEnum.KillBoss:
                                        DramaHelper.OpenDrama1(GameTool.LS("battleevent480110600"), new List<string> { GameTool.LS("other500020045") }, null, stalker, g.world.playerUnit);
                                        break;
                                    case Enum.StalkReasonEnum.MarketBuyerShowUp_SpiritStones:
                                        DramaHelper.OpenDrama1(GameTool.LS("battleevent480110601"), new List<string> { GameTool.LS("other500020045") }, null, stalker, g.world.playerUnit);
                                        break;
                                    case Enum.StalkReasonEnum.MarketBuyerShowUp_Items:
                                        DramaHelper.OpenDrama1(GameTool.LS("battleevent480110602"), new List<string> { GameTool.LS("other500020045") }, null, stalker, g.world.playerUnit);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        //enemy unit join battle
                        if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.RandomNpcJoinRate))
                        {
                            var enemyUnit = AroundUnits.FirstOrDefault(x => IsEnemyUnit(g.world.playerUnit, x) >= 0);
                            if (enemyUnit != null)
                            {
                                DebugHelper.WriteLine($"Enemy join: {enemyUnit.GetName()} ({enemyUnit.GetUnitId()})");
                                var t = IsEnemyUnit(g.world.playerUnit, enemyUnit);
                                var units = HirePeopleEvent.GetTeamDetailData(enemyUnit).Item2;
                                AroundUnits.RemoveAll(x => units.Any(y => y.GetUnitId() == x.GetUnitId()));
                                NPCJoin(UnitType.Monst, units);

                                switch (t)
                                {
                                    case 2:
                                        DramaTool.OpenDrama(enemyUnit.IsRighteous() ? RIGHTEOUS_DRAMA : EVIL_DRAMA, new DramaData() { unitLeft = enemyUnit, unitRight = g.world.playerUnit });
                                        break;
                                    default:
                                        DramaTool.OpenDrama(ENEMY_JOIN_DRAMA, new DramaData() { unitLeft = enemyUnit, unitRight = g.world.playerUnit });
                                        break;
                                }
                            }
                        }

                        //friendly unit join battle
                        if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.RandomNpcJoinRate))
                        {
                            var friendlyUnit = AroundUnits.FirstOrDefault(x => IsFriendlyUnit(g.world.playerUnit, x) >= 0);
                            if (friendlyUnit != null)
                            {
                                DebugHelper.WriteLine($"Friend join: {friendlyUnit.GetName()} ({friendlyUnit.GetUnitId()})");
                                var units = HirePeopleEvent.GetTeamDetailData(friendlyUnit).Item2;
                                AroundUnits.RemoveAll(x => units.Any(y => y.GetUnitId() == x.GetUnitId()));
                                NPCJoin(UnitType.PlayerNPC, units);

                                DramaTool.OpenDrama(FRIENDLY_JOIN_DRAMA, new DramaData() { unitLeft = friendlyUnit, unitRight = g.world.playerUnit });
                            }
                        }

                        //sect member join battle
                        if (ModBattleEvent.School != null && DungeonHelper.IsArena() && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.SectNpcJoinRate))
                        {
                            var sectMember = AroundUnits.FirstOrDefault(x => MapBuildPropertyEvent.IsSchoolMember(ModBattleEvent.School, x));
                            if (sectMember != null)
                            {
                                DebugHelper.WriteLine($"Sect-member ({ModBattleEvent.School.GetName(true)}) join: {sectMember.GetName()} ({sectMember.GetUnitId()})");
                                var units = HirePeopleEvent.GetTeamDetailData(sectMember).Item2;
                                AroundUnits.RemoveAll(x => units.Any(y => y.GetUnitId() == x.GetUnitId()));
                                var ut = IsFriendlyUnit(g.world.playerUnit, sectMember) >= 0 || g.world.playerUnit.IsSameSect(sectMember) ? UnitType.PlayerNPC : UnitType.Monst;
                                NPCJoin(ut, units);

                                DramaTool.OpenDrama(SECT_MEMBER_JOIN_DRAMA, new DramaData() { unitLeft = sectMember, unitRight = null });
                            }
                        }

                        //town guard join battle
                        if (ModBattleEvent.Town != null && DungeonHelper.IsArena() && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.TownGuardNpcJoinRate))
                        {
                            var townguard = AroundUnits.FirstOrDefault(x => MapBuildPropertyEvent.IsTownGuardian(ModBattleEvent.Town, x));
                            if (townguard != null)
                            {
                                DebugHelper.WriteLine($"Town-guard ({ModBattleEvent.Town.name}) join: {townguard.GetName()} ({townguard.GetUnitId()})");
                                var units = HirePeopleEvent.GetTeamDetailData(townguard).Item2;
                                AroundUnits.RemoveAll(x => units.Any(y => y.GetUnitId() == x.GetUnitId()));
                                var ut = IsFriendlyUnit(g.world.playerUnit, townguard) >= 0 || g.world.playerUnit.IsSameSect(townguard) ? UnitType.PlayerNPC : UnitType.Monst;
                                NPCJoin(ut, units);

                                DramaTool.OpenDrama(TOWN_GUARD_JOIN_DRAMA, new DramaData() { unitLeft = townguard, unitRight = null });
                            }
                        }
                    }
                    if (TeamMembers.Count > 0)
                    {
                        foreach (var wmember in TeamMembers.ToArray())
                        {
                            if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.TeamMemberJoinRate))
                            {
                                DebugHelper.WriteLine($"Team-member ({HirePeopleEvent.GetTeamInfoStr(g.world.playerUnit)}) join: {wmember.GetName()} ({wmember.GetUnitId()})");
                                JointTeamMembers.AddRange(NPCJoin(UnitType.PlayerNPC, wmember));
                                TeamMembers.Remove(wmember);
                            }
                        }
                    }
                    if (JointTeamMembers.Count > 0)
                    {
                        foreach (var cmember in JointTeamMembers.ToArray())
                        {
                            if (!cmember.isDie && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.TeamMemberBetrayRate))
                            {
                                var wmember = g.world.unit.GetUnit(cmember);
                                DebugHelper.WriteLine($"Team-member ({HirePeopleEvent.GetTeamInfoStr(g.world.playerUnit)}) betray: {wmember.GetName()} ({wmember.GetUnitId()})");
                                var t = IsBetrayUnit(g.world.playerUnit, wmember);
                                if (t >= 0)
                                {
                                    cmember.data.unitType = UnitType.Monst; //change team
                                    HirePeopleEvent.Dismiss(g.world.playerUnit, wmember); //quit player team
                                    wmember.data.unitData.relationData.AddHate(g.world.playerUnit.GetUnitId(), 200);
                                    g.world.playerUnit.data.unitData.relationData.AddHate(wmember.GetUnitId(), 200);

                                    switch (t)
                                    {
                                        case 2:
                                            DramaTool.OpenDrama(wmember.IsRighteous() ? RIGHTEOUS_DRAMA : EVIL_DRAMA, new DramaData() { unitLeft = wmember, unitRight = g.world.playerUnit });
                                            break;
                                        default:
                                            DramaTool.OpenDrama(TEAM_MEMBER_BETRAY_DRAMA, new DramaData() { unitLeft = wmember, unitRight = g.world.playerUnit });
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static List<UnitCtrlBase> NPCJoin(UnitType ut, params WorldUnitBase[] wunits)
        {
            var rs = new List<UnitCtrlBase>();
            var wpos = g.world.playerUnit.GetUnitPos();
            foreach (var wunit in wunits.Where(x => x != null && x.data != null && !x.isDie))
            {
                if (ModBattleEvent.SceneBattle.unit.allUnitIncludeDie.ToList().Any(x => x.IsWorldUnit() && x.GetWorldUnit().GetUnitId() == wunit.GetUnitId()))
                    continue;

                //set wpos
                wunit.SetUnitPos(wpos);

                //create cunit
                var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(wunit.data, ut);

                //set cpos
                var posi = new UnityEngine.Vector2[]
                {
                        new UnityEngine.Vector2(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.x + -16, ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y +   0),
                        new UnityEngine.Vector2(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.x +  16, ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y +   0),
                        new UnityEngine.Vector2(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.x +   0, ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y + -16),
                        new UnityEngine.Vector2(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.x +   0, ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y +  16),
                };
                cunit.move.SetPosition(posi.Random());

                //set avgGrade
                Instance.AvgGrade = ((Instance.AvgGrade + wunit.GetGradeLvl()) / 2f) + 0.1f;

                rs.Add(cunit);
            }
            return rs;
        }

        public static bool CondJoinBattle(WorldUnitBase wunit)
        {
            return !wunit.isDie && wunit.GetGradeLvl() >= Instance.AvgGrade &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Hp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value * 0.7f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Mp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 0.5f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Sp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 0.3f);
        }

        public static int IsFriendlyUnit(WorldUnitBase originWUnit, WorldUnitBase targetWUnit)
        {
            //from friend
            if (friendlyInTraits.Contains(targetWUnit.data.unitData.propertyData.inTrait) ||
                targetWUnit.data.unitData.relationData.GetIntim(originWUnit) >= Configs.FriendlyIntim ||
                (originWUnit.data.school?.schoolData.GetSchoolIntim(targetWUnit) ?? 0) >= Configs.FriendlyIntim ||
                (targetWUnit.data.school?.schoolData.GetSchoolIntim(originWUnit) ?? 0) >= Configs.FriendlyIntim)
            {
                return 1;
            }
            //from same sect member
            if (originWUnit.IsSameSect(targetWUnit))
            {
                return 2;
            }
            return -1;
        }

        public static int IsEnemyUnit(WorldUnitBase originWUnit, WorldUnitBase targetWUnit)
        {
            //from hater
            if (targetWUnit.data.unitData.relationData.GetIntim(originWUnit) <= Configs.EnemyIntim ||
                (originWUnit.data.school?.schoolData.GetSchoolIntim(targetWUnit) ?? 0) <= Configs.EnemyIntim ||
                (targetWUnit.data.school?.schoolData.GetSchoolIntim(originWUnit) ?? 0) <= Configs.EnemyIntim)
            {
                return 1;
            }
            //from bad guys
            if (enemyInTraits.Contains(targetWUnit.data.unitData.propertyData.inTrait))
            {
                return 0;
            }
            //from good vs evil
            if (targetWUnit.IsRighteous() != originWUnit.IsRighteous() &&
                Math.Abs(targetWUnit.GetStandValue() - originWUnit.GetStandValue()) > Configs.DifferenceRighteous)
            {
                return 2;
            }
            return -1;
        }

        public static int IsBetrayUnit(WorldUnitBase originWUnit, WorldUnitBase targetWUnit)
        {
            //from hater
            if (targetWUnit.data.unitData.relationData.GetIntim(originWUnit) <= Configs.EnemyIntim ||
                (originWUnit.data.school?.schoolData.GetSchoolIntim(targetWUnit) ?? 0) <= Configs.EnemyIntim ||
                (targetWUnit.data.school?.schoolData.GetSchoolIntim(originWUnit) ?? 0) <= Configs.EnemyIntim)
            {
                return 0;
            }
            //from bad guys
            if (betrayInTraits.Contains(targetWUnit.data.unitData.propertyData.inTrait) ||
                betrayOutTraits.Contains(targetWUnit.data.unitData.propertyData.outTrait1) ||
                betrayOutTraits.Contains(targetWUnit.data.unitData.propertyData.outTrait2))
            {
                return 1;
            }
            //from good vs evil
            if (targetWUnit.IsRighteous() != originWUnit.IsRighteous() &&
                Math.Abs(targetWUnit.GetStandValue() - originWUnit.GetStandValue()) > Configs.DifferenceRighteous)
            {
                return 2;
            }
            return -1;
        }

        public static bool IsJoinableBattle()
        {
            return !MapBuildBattleEvent.IsBattleTownWar() && !MapBuildBattleEvent.IsBattleMonstWave() &&
                g.world.battle.data.dungeonBaseItem.id != MapBuildPropertyEvent.CATCH_SNEAKY_DUNGEON_ID /*Sneaky Battle*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3901 /*Qi Refining Realm*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3902 /*Foundation Realm*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3903 /*Qi Condensation Realm*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3904 /*Golden Core Realm*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3905 /*Origin Spirit Realm*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3906 /*Nascent Soul Realm*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3907 /*Soul Formation Realm*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3908 /*Enlightenment Realm*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3909 /*Reborn Realm*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3910 /*Transcendent Realm*/ &&
                g.world.battle.data.dungeonBaseItem.id != 3911 /*Tian Yuan Summit*/ &&
                g.world.battle.data.dungeonBaseItem.id != 4601 /*Sect Position Dungeon*/ &&
                g.world.battle.data.dungeonBaseItem.id != 5016850 /*Mind Demon*/ &&
                !DungeonHelper.IsArena() /*Arena*/ &&
                !DungeonHelper.IsChallenge() /*Challenge*/ &&
                !DungeonHelper.IsSpar() /*Spar*/ &&
                !DungeonHelper.IsTrailOfLightning() /*Trial of Lightning*/ &&
                !DungeonHelper.IsRebornBambooChallenge() /*Bamboo Challenge*/ &&
                g.world.battle.data.dungeonBaseItem.id != 120 /*Sect Points Race*/ &&
                g.world.battle.data.dungeonBaseItem.id != 121 /*Sect Open Challenge Match*/ &&
                g.world.battle.data.dungeonBaseItem.id != 122 /*Sect Array Core Battle*/ &&
                g.world.battle.data.dungeonBaseItem.id != 123 /*Sect battle*/ &&
                g.world.battle.data.dungeonBaseItem.id != 124 /*Sect Flying Canoe Battle*/ &&
                g.world.battle.data.dungeonBaseItem.id != 125 /*Sect Guardian Battle*/ &&
                g.world.battle.data.dungeonBaseItem.id != 15 /*New Game Battle*/
                //g.world.battle.data.dungeonBaseItem.id != 7007 /*Spiritlock Circle*/ &&
                //g.world.battle.data.dungeonBaseItem.id != 7008 /*Spiritlock Circle*/ &&
                //g.world.battle.data.dungeonBaseItem.id != 7009 /*Spiritlock Circle*/ &&
                //g.world.battle.data.dungeonBaseItem.id != 7010 /*Spiritlock Circle*/ &&
                //g.world.battle.data.dungeonBaseItem.id != 7011 /*Demonseal Circle*/ &&
                //g.world.battle.data.dungeonBaseItem.id != 7012 /*Demonseal Circle*/ &&
                //g.world.battle.data.dungeonBaseItem.id != 7013 /*Demonseal Circle*/ &&
                ;
            //should block self battles
        }
    }
}
