using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_EVENT)]
    public class BattleEvent : ModEvent
    {
        public static BattleEvent Instance { get; set; }

        public const int JOIN_RANGE = 6;
        public const float RANDOM_NPC_JOIN_RATE = 0.1f;
        public const float SECT_NPC_JOIN_RATE = 1.0f;
        public const float TOWN_GUARD_NPC_JOIN_RATE = 3.0f;
        public const float TEAM_MEMBER_BETRAY_RATE = 0.2f;
        public const float STALKER_JOIN_BATTLE = 1f;
        public const int ENEMY_JOIN_DRAMA = 480110100;
        public const int FRIENDLY_JOIN_DRAMA = 480110200;
        public const int SECT_MEMBER_JOIN_DRAMA = 480110300;
        public const int TOWN_GUARD_JOIN_DRAMA = 480110400;
        public const int TEAM_MEMBER_BETRAY_DRAMA = 480110500;
        public const int STALKER_JOIN_DRAMA = 480110700;

        public static float avgGrade;
        public static List<UnitCtrlBase> TeamMembers { get; } = new List<UnitCtrlBase>();
        public static List<WorldUnitBase> AroundUnits { get; } = new List<WorldUnitBase>();

        public static readonly int[] friendlyInTraits = new int[] { UnitTraitEnum.Selfless.Parse<int>() };
        public static readonly int[] enemyInTraits = new int[] { UnitTraitEnum.Wicked.Parse<int>(), UnitTraitEnum.Selfish.Parse<int>(), UnitTraitEnum.Evil.Parse<int>() };
        public static readonly int[] betrayInTraits = new int[] { UnitTraitEnum.Selfish.Parse<int>(), UnitTraitEnum.Evil.Parse<int>() };
        public static readonly int[] betrayOutTraits = new int[] { UnitTraitEnum.Power_hungry.Parse<int>() };

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);

            avgGrade = g.world.playerUnit.GetGradeLvl() - 1;
            AroundUnits.Clear();
            TeamMembers.Clear();
            if (g.world.battle.data.isRealBattle && IsJoinableBattle())
            {
                var player = g.world.playerUnit;

                //setup around units
                AroundUnits.AddRange(player.GetUnitsAround(JOIN_RANGE, false, false).ToArray().Where(x =>
                {
                    return !BattleAfterEvent.Stalkers.Contains(x) && CondJoinBattle(x) && (IsFriendlyUnit(x) || IsEnemyUnit(x) || MapBuildPropertyEvent.IsTownGuardian(x) || MapBuildPropertyEvent.IsSchoolMember(ModBattleEvent.School, x));
                }));

                //team member join
                TeamMembers.AddRange(NPCJoin(UnitType.PlayerNPC, HirePeopleEvent.GetTeamDetailData(player).Item2.Where(x => !x.isDie && !x.IsPlayer()).ToArray()));
            }
        }

        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.Ignore, IsInBattle = HandleEnum.True)]
        public override void OnTimeUpdate1s()
        {
            base.OnTimeUpdate1s();
            if (ModBattleEvent.SceneBattle != null && IsJoinableBattle())
            {
                if (g.world.battle.data.isRealBattle && ModBattleEvent.SceneBattle.battleMap.isStartBattle &&
                    ModBattleEvent.PlayerUnit != null && !ModBattleEvent.PlayerUnit.isDie && !ModBattleEvent.SceneBattle.unit.IsUnitHide(ModBattleEvent.PlayerUnit))
                {
                    if (BattleAfterEvent.Stalkers.Count > 0)
                    {
                        //enemy unit join battle
                        if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, STALKER_JOIN_BATTLE))
                        {
                            var stalker = BattleAfterEvent.Stalkers.FirstOrDefault();
                            if (stalker != null)
                            {
                                var units = HirePeopleEvent.GetTeamDetailData(stalker).Item2;
                                units.ToList().ForEach(x => x.SetUnitPos(g.world.playerUnit.GetUnitPos()));
                                BattleAfterEvent.Stalkers.RemoveAll(x => units.Any(y => y.GetUnitId() == x.GetUnitId()));
                                NPCJoin(UnitType.Monst, units);

                                DramaTool.OpenDrama(STALKER_JOIN_DRAMA, new DramaData() { unitLeft = stalker, unitRight = g.world.playerUnit });
                            }
                        }
                    }
                    if (AroundUnits.Count > 0)
                    {
                        //enemy unit join battle
                        if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, RANDOM_NPC_JOIN_RATE))
                        {
                            var enemyUnit = AroundUnits.FirstOrDefault(x => IsEnemyUnit(x));
                            if (enemyUnit != null)
                            {
                                var units = HirePeopleEvent.GetTeamDetailData(enemyUnit).Item2;
                                units.ToList().ForEach(x => x.SetUnitPos(g.world.playerUnit.GetUnitPos()));
                                AroundUnits.RemoveAll(x => units.Any(y => y.GetUnitId() == x.GetUnitId()));
                                NPCJoin(UnitType.Monst, units);

                                DramaTool.OpenDrama(ENEMY_JOIN_DRAMA, new DramaData() { unitLeft = enemyUnit, unitRight = g.world.playerUnit });
                            }
                        }

                        //friendly unit join battle
                        if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, RANDOM_NPC_JOIN_RATE))
                        {
                            var friendlyUnit = AroundUnits.FirstOrDefault(x => IsFriendlyUnit(x));
                            if (friendlyUnit != null)
                            {
                                var units = HirePeopleEvent.GetTeamDetailData(friendlyUnit).Item2;
                                units.ToList().ForEach(x => x.SetUnitPos(g.world.playerUnit.GetUnitPos()));
                                AroundUnits.RemoveAll(x => units.Any(y => y.GetUnitId() == x.GetUnitId()));
                                NPCJoin(UnitType.PlayerNPC, units);

                                DramaTool.OpenDrama(FRIENDLY_JOIN_DRAMA, new DramaData() { unitLeft = friendlyUnit, unitRight = g.world.playerUnit });
                            }
                        }

                        //sect member join battle
                        if (ModBattleEvent.School != null && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, SECT_NPC_JOIN_RATE))
                        {
                            var sectMember = AroundUnits.FirstOrDefault(x => MapBuildPropertyEvent.IsSchoolMember(ModBattleEvent.School, x));
                            if (sectMember != null)
                            {
                                var units = HirePeopleEvent.GetTeamDetailData(sectMember).Item2;
                                units.ToList().ForEach(x => x.SetUnitPos(g.world.playerUnit.GetUnitPos()));
                                AroundUnits.RemoveAll(x => units.Any(y => y.GetUnitId() == x.GetUnitId()));
                                var ut = IsFriendlyUnit(sectMember) ? UnitType.PlayerNPC : UnitType.Monst;
                                NPCJoin(ut, units);

                                DramaTool.OpenDrama(SECT_MEMBER_JOIN_DRAMA, new DramaData() { unitLeft = sectMember, unitRight = null });
                            }
                        }

                        //town guard join battle
                        if (ModBattleEvent.Town != null && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, TOWN_GUARD_NPC_JOIN_RATE))
                        {
                            var townguard = AroundUnits.FirstOrDefault(x => MapBuildPropertyEvent.IsTownGuardian(ModBattleEvent.Town, x));
                            if (townguard != null)
                            {
                                var units = HirePeopleEvent.GetTeamDetailData(townguard).Item2;
                                units.ToList().ForEach(x => x.SetUnitPos(g.world.playerUnit.GetUnitPos()));
                                AroundUnits.RemoveAll(x => units.Any(y => y.GetUnitId() == x.GetUnitId()));
                                var ut = IsFriendlyUnit(townguard) ? UnitType.PlayerNPC : UnitType.Monst;
                                NPCJoin(ut, units);

                                DramaTool.OpenDrama(TOWN_GUARD_JOIN_DRAMA, new DramaData() { unitLeft = townguard, unitRight = null });
                            }
                        }
                    }
                    if (TeamMembers.Count > 0)
                    {
                        foreach (var cmember in TeamMembers.ToArray())
                        {
                            if (!cmember.isDie && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, TEAM_MEMBER_BETRAY_RATE))
                            {
                                var wmember = g.world.unit.GetUnit(cmember);
                                if (IsBetrayUnit(wmember))
                                {
                                    cmember.data.unitType = UnitType.Monst; //change team
                                    HirePeopleEvent.Dismiss(g.world.playerUnit, wmember); //quit player team
                                    wmember.data.unitData.relationData.AddHate(g.world.playerUnit.GetUnitId(), 200);
                                    g.world.playerUnit.data.unitData.relationData.AddHate(wmember.GetUnitId(), 200);

                                    DramaTool.OpenDrama(TEAM_MEMBER_BETRAY_DRAMA, new DramaData() { unitLeft = wmember, unitRight = g.world.playerUnit });
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<UnitCtrlBase> NPCJoin(UnitType ut, params WorldUnitBase[] wunits)
        {
            var rs = new List<UnitCtrlBase>();
            var wpos = g.world.playerUnit.GetUnitPos();
            foreach (var wunit in wunits.Where(x => !x.isDie))
            {
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
                avgGrade = ((avgGrade + wunit.GetGradeLvl()) / 2f) + 0.1f;

                rs.Add(cunit);
            }
            return rs;
        }

        private bool CondJoinBattle(WorldUnitBase wunit)
        {
            return !wunit.isDie && wunit.GetGradeLvl() >= avgGrade &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Hp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value * 0.7f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Mp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 0.5f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Sp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 0.3f);
        }

        private bool IsFriendlyUnit(WorldUnitBase wunit)
        {
            return wunit.GetGradeLvl() >= avgGrade &&
                (
                    friendlyInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                    wunit.data.unitData.relationData.GetIntim(g.world.playerUnit) >= 200 ||
                    (g.world.playerUnit.data.school?.schoolData.GetSchoolIntim(wunit) ?? 0) >= 200 ||
                    (wunit.data.school?.schoolData.GetSchoolIntim(g.world.playerUnit) ?? 0) >= 200
                );
        }

        private bool IsEnemyUnit(WorldUnitBase wunit)
        {
            return wunit.GetGradeLvl() >= avgGrade &&
                (
                    enemyInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                    wunit.data.unitData.relationData.GetIntim(g.world.playerUnit) <= -200 ||
                    (g.world.playerUnit.data.school?.schoolData.GetSchoolIntim(wunit) ?? 0) <= -200 ||
                    (wunit.data.school?.schoolData.GetSchoolIntim(g.world.playerUnit) ?? 0) <= -200
                );
        }

        private bool IsBetrayUnit(WorldUnitBase wunit)
        {
            return wunit.GetGradeLvl() >= avgGrade &&
                (
                    betrayInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                    betrayOutTraits.Contains(wunit.data.unitData.propertyData.outTrait1) ||
                    betrayOutTraits.Contains(wunit.data.unitData.propertyData.outTrait2) ||
                    wunit.data.unitData.relationData.GetIntim(g.world.playerUnit) <= -200 ||
                    (g.world.playerUnit.data.school?.schoolData.GetSchoolIntim(wunit) ?? 0) <= -200 ||
                    (wunit.data.school?.schoolData.GetSchoolIntim(g.world.playerUnit) ?? 0) <= -200
                );
        }

        public static bool IsJoinableBattle()
        {
            return !MapBuildBattleEvent.IsBattleTownWar() && !MapBuildBattleEvent.IsBattleMonstWave() &&
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
                !DungeonHelper.IsArean() /*Arena*/ &&
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
