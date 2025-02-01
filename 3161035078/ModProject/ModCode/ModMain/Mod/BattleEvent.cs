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
        public const int ENEMY_JOIN_DRAMA = 480110100;
        public const int FRIENDLY_JOIN_DRAMA = 480110200;
        public const int SECT_MEMBER_JOIN_DRAMA = 480110300;
        public const int TOWN_GUARD_JOIN_DRAMA = 480110400;
        public const int TEAM_MEMBER_BETRAY_DRAMA = 480110500;

        private static float _avgGrade;
        private static List<UnitCtrlBase> _teamMember;
        private static List<WorldUnitBase> _aroundUnits;
        public static readonly int[] friendlyInTraits = new int[] { UnitTraitEnum.Selfless.Parse<int>() };
        public static readonly int[] enemyInTraits = new int[] { UnitTraitEnum.Wicked.Parse<int>(), UnitTraitEnum.Selfish.Parse<int>(), UnitTraitEnum.Evil.Parse<int>() };
        public static readonly int[] betrayInTraits = new int[] { UnitTraitEnum.Selfish.Parse<int>(), UnitTraitEnum.Evil.Parse<int>() };
        public static readonly int[] betrayOutTraits = new int[] { UnitTraitEnum.Power_hungry.Parse<int>() };

        //public override void OnOpenUIEnd(OpenUIEnd e)
        //{
        //    base.OnOpenUIEnd(e);
        //    if (e.uiType.uiName == UIType.BattleDie.uiName)
        //    {
        //        g.ui.CloseUI(e.ui);
        //    }
        //}

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);

            _avgGrade = g.world.playerUnit.GetGradeLvl() - 1;
            _aroundUnits = null;
            _teamMember = null;
            if (g.world.battle.data.isRealBattle && IsJoinableBattle())
            {
                var player = g.world.playerUnit;

                //lock end battle
                //ModBattleEvent.SceneBattle.battleEnd.lockEndBattle = true;
                //ModBattleEvent.SceneBattle.battleEnd.openBattleEndUI = false;

                //setup around units
                _aroundUnits = player.GetUnitsAround(JOIN_RANGE, false, false).ToArray().Where(x =>
                {
                    return CondJoinBattle(x) && (IsFriendlyUnit(x) || IsEnemyUnit(x) || MapBuildPropertyEvent.IsTownGuardian(x) || MapBuildPropertyEvent.IsSchoolMember(ModBattleEvent.School, x));
                }).ToList();

                //team member join
                NPCJoin(UnitType.PlayerNPC, HirePeopleEvent.GetTeamDetailData(g.world.playerUnit).Item2.Where(x => x.GetUnitId() != g.world.playerUnit.GetUnitId()).ToArray());
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
                    if (_aroundUnits != null)
                    {
                        //enemy unit join battle
                        if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, RANDOM_NPC_JOIN_RATE))
                        {
                            var enemyUnit = _aroundUnits.FirstOrDefault(x => IsEnemyUnit(x));
                            if (enemyUnit != null)
                            {
                                _aroundUnits.Remove(enemyUnit);
                                NPCJoin(UnitType.Monst, HirePeopleEvent.GetTeamDetailData(enemyUnit).Item2);

                                DramaTool.OpenDrama(ENEMY_JOIN_DRAMA, new DramaData() { unitLeft = enemyUnit, unitRight = g.world.playerUnit });
                            }
                        }

                        //friendly unit join battle
                        if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, RANDOM_NPC_JOIN_RATE))
                        {
                            var friendlyUnit = _aroundUnits.FirstOrDefault(x => IsFriendlyUnit(x));
                            if (friendlyUnit != null)
                            {
                                _aroundUnits.Remove(friendlyUnit);
                                NPCJoin(UnitType.PlayerNPC, HirePeopleEvent.GetTeamDetailData(friendlyUnit)?.Item2);

                                DramaTool.OpenDrama(FRIENDLY_JOIN_DRAMA, new DramaData() { unitLeft = friendlyUnit, unitRight = g.world.playerUnit });
                            }
                        }

                        //sect member join battle
                        if (ModBattleEvent.School != null && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, SECT_NPC_JOIN_RATE))
                        {
                            var sectMember = _aroundUnits.FirstOrDefault(x => MapBuildPropertyEvent.IsSchoolMember(ModBattleEvent.School, x));
                            if (sectMember != null)
                            {
                                _aroundUnits.Remove(sectMember);
                                var ut = IsFriendlyUnit(sectMember) ? UnitType.PlayerNPC : UnitType.Monst;
                                NPCJoin(ut, HirePeopleEvent.GetTeamDetailData(sectMember).Item2);

                                DramaTool.OpenDrama(SECT_MEMBER_JOIN_DRAMA, new DramaData() { unitLeft = sectMember, unitRight = null });
                            }
                        }

                        //town guard join battle
                        if (ModBattleEvent.Town != null && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, TOWN_GUARD_NPC_JOIN_RATE))
                        {
                            var townguard = _aroundUnits.FirstOrDefault(x => MapBuildPropertyEvent.IsTownGuardian(ModBattleEvent.Town, x));
                            if (townguard != null)
                            {
                                _aroundUnits.Remove(townguard);
                                var ut = IsFriendlyUnit(townguard) ? UnitType.PlayerNPC : UnitType.Monst;
                                NPCJoin(ut, HirePeopleEvent.GetTeamDetailData(townguard).Item2);

                                DramaTool.OpenDrama(TOWN_GUARD_JOIN_DRAMA, new DramaData() { unitLeft = townguard, unitRight = null });
                            }
                        }
                    }
                    if (_teamMember != null && _teamMember.Count > 0)
                    {
                        foreach (var cmember in _teamMember.ToArray())
                        {
                            if (!cmember.isDie && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, TEAM_MEMBER_BETRAY_RATE))
                            {
                                var wmember = g.world.unit.GetUnit(cmember);
                                if (IsBetrayUnit(wmember))
                                {
                                    cmember.data.unitType = UnitType.Monst; //change team
                                    HirePeopleEvent.Dismiss(g.world.playerUnit, wmember); //quit player team
                                    wmember.data.unitData.relationData.AddHate(g.world.playerUnit.GetUnitId(), 100);
                                    g.world.playerUnit.data.unitData.relationData.AddHate(wmember.GetUnitId(), 100);

                                    DramaTool.OpenDrama(TEAM_MEMBER_BETRAY_DRAMA, new DramaData() { unitLeft = wmember, unitRight = g.world.playerUnit });
                                }
                            }
                        }
                    }
                }
            }
        }

        private void NPCJoin(UnitType ut, params WorldUnitBase[] wunits)
        {
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
                _avgGrade = ((_avgGrade + wunit.GetGradeLvl()) / 2f) + 0.1f;
            }
        }

        private bool CondJoinBattle(WorldUnitBase wunit)
        {
            return wunit.GetGradeLvl() >= _avgGrade &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Hp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value * 0.7f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Mp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 0.5f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Sp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 0.3f);
        }

        private bool IsFriendlyUnit(WorldUnitBase wunit)
        {
            return wunit.GetGradeLvl() >= _avgGrade &&
                (
                    friendlyInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                    wunit.data.unitData.relationData.GetIntim(g.world.playerUnit) >= 200 ||
                    (g.world.playerUnit.data.school?.schoolData.GetSchoolIntim(wunit) ?? 0) >= 200 ||
                    (wunit.data.school?.schoolData.GetSchoolIntim(g.world.playerUnit) ?? 0) >= 200
                );
        }

        private bool IsEnemyUnit(WorldUnitBase wunit)
        {
            return wunit.GetGradeLvl() >= _avgGrade &&
                (
                    enemyInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                    wunit.data.unitData.relationData.GetIntim(g.world.playerUnit) <= -200 ||
                    (g.world.playerUnit.data.school?.schoolData.GetSchoolIntim(wunit) ?? 0) <= -200 ||
                    (wunit.data.school?.schoolData.GetSchoolIntim(g.world.playerUnit) ?? 0) <= -200
                );
        }

        private bool IsBetrayUnit(WorldUnitBase wunit)
        {
            return wunit.GetGradeLvl() >= _avgGrade &&
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
            return !MapBuildBattleEvent.IsBattleTownWar() && !MapBuildBattleEvent.IsBattleMonstWave();
            //should add self battles
        }
    }
}
