using EBattleTypeData;
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
        public const float RANDOM_NPC_JOIN_RATE = 0.2f;
        public const float SECT_NPC_JOIN_RATE = 2.0f;
        public const float TOWN_GUARD_NPC_JOIN_RATE = 5.0f;
        public const int ENEMY_JOIN_DRAMA = 480110100;
        public const int FRIENDLY_JOIN_DRAMA = 480110200;
        public const int SECT_MEMBER_JOIN_DRAMA = 480110300;
        public const int TOWN_GUARD_JOIN_DRAMA = 480110400;

        public int PvPCount { get; set; } = 0;

        private static List<WorldUnitBase> _aroundUnits;
        private static readonly int[] friendlyInTraits = new int[] { UnitTraitEnum.Selfless.Parse<int>() };
        private static readonly int[] enemyInTraits = new int[] { UnitTraitEnum.Wicked.Parse<int>(), UnitTraitEnum.Selfish.Parse<int>(), UnitTraitEnum.Evil.Parse<int>() };

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            _aroundUnits = g.world.playerUnit.GetUnitsAround(JOIN_RANGE, false, false).ToArray().Where(x =>
            {
                return CondJoinBattle(x) && (IsFriendlyUnit(x) || IsEnemyUnit(x));
            }).ToList();
        }

        [EventCondition(IsInGame = HandleEnum.Ignore, IsInBattle = HandleEnum.True)]
        public override void OnTimeUpdate1s()
        {
            base.OnTimeUpdate1s();
            if (g.world.battle.data.isRealBattle && _aroundUnits != null)
            {
                //enemy unit join battle
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, RANDOM_NPC_JOIN_RATE))
                {
                    var enemyUnit = _aroundUnits.FirstOrDefault(x => IsEnemyUnit(x));
                    if (enemyUnit != null)
                    {
                        _aroundUnits.Remove(enemyUnit);
                        SceneType.battle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(enemyUnit.data, UnitType.Alone);

                        //var enemyUnitAllies = _aroundUnits.Where(x => enemyUnit.data.unitData.relationData.GetIntim(x) >= 350f);
                        //foreach (var enemyUnitAllyUnit in enemyUnitAllies)
                        //{
                        //    _aroundUnits.Remove(enemyUnitAllyUnit);
                        //    SceneType.battle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(enemyUnitAllyUnit.data, UnitType.Monst);
                        //}

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
                        SceneType.battle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(friendlyUnit.data, UnitType.PlayerNPC);

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
                        SceneType.battle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(sectMember.data, IsEnemyUnit(sectMember) ? UnitType.Monst : UnitType.PlayerNPC);

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
                        SceneType.battle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(townguard.data, IsEnemyUnit(townguard) ? UnitType.Monst : UnitType.PlayerNPC);

                        DramaTool.OpenDrama(TOWN_GUARD_JOIN_DRAMA, new DramaData() { unitLeft = townguard, unitRight = null });
                    }
                }
            }
        }

        private bool CondJoinBattle(WorldUnitBase wunit)
        {
            return !MapBuildPropertyEvent.IsTownGuardian(wunit) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Hp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value * 0.7f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Mp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 0.5f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Sp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 0.3f) &&
                wunit.GetGradeLvl() >= (g.world.playerUnit.GetGradeLvl() - 1);
        }

        private bool IsFriendlyUnit(WorldUnitBase wunit)
        {
            return friendlyInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                wunit.data.unitData.relationData.GetIntim(g.world.playerUnit) >= 300f ||
                (g.world.playerUnit.data.school?.schoolData.GetSchoolIntim(wunit) ?? 0) >= 300f ||
                (wunit.data.school?.schoolData.GetSchoolIntim(g.world.playerUnit) ?? 0) >= 300f;
        }

        private bool IsEnemyUnit(WorldUnitBase wunit)
        {
            return enemyInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                wunit.data.unitData.relationData.GetIntim(g.world.playerUnit) <= -300f ||
                (g.world.playerUnit.data.school?.schoolData.GetSchoolIntim(wunit) ?? 0) <= -300f ||
                (wunit.data.school?.schoolData.GetSchoolIntim(g.world.playerUnit) ?? 0) <= -300f;
        }

        //player kill npc
        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);
            var dieUnit = e?.unit?.data?.TryCast<UnitDataHuman>();
            var attackUnitData = e?.hitData?.attackUnit?.data?.TryCast<UnitDataHuman>();
            if (dieUnit?.worldUnitData != null && 
                g.world.battle.data.isRealBattle &&
                (attackUnitData?.worldUnitData?.unit?.IsPlayer() ?? false))
            {
                PvPCount++;
            }
        }

        //npc revenge player
        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);
            var humanData = e?.data?.TryCast<UnitDataHuman>();
            if (g.world.battle.data.isRealBattle &&
                (!humanData?.worldUnitData?.unit?.IsPlayer() ?? false) &&
                humanData?.worldUnitData?.GetRelationType(g.world.playerUnit) == UnitBothRelationType.Hater)
            {
                humanData.attack.baseValue += (int)(humanData.attack.value * (PvPCount / 100.00f));
            }
        }
    }
}
