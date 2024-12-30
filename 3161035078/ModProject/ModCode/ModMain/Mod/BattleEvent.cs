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
        public const int JOIN_RANGE = 5;
        public const float NPC_JOIN_RATE = 0.20f;
        public const int ENEMY_JOIN_DRAMA = 480110100;
        public const int FRIENDLY_JOIN_DRAMA = 480110200;

        public int PvPCount { get; set; } = 0;

        private static List<WorldUnitBase> _aroundUnits;
        private static readonly int[] friendlyInTraits = new int[] { UnitTraitEnum.Selfless.Parse<int>() };
        private static readonly int[] enemyInTraits = new int[] { UnitTraitEnum.Wicked.Parse<int>(), UnitTraitEnum.Selfish.Parse<int>(), UnitTraitEnum.Evil.Parse<int>() };

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            _aroundUnits = g.world.unit.GetUnitExact(g.world.playerUnit.GetUnitPos(), JOIN_RANGE, false, false).ToArray().Where(x =>
            {
                return
                    x.GetDynProperty(UnitDynPropertyEnum.Hp).value > (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value * 0.8f) &&
                    x.GetDynProperty(UnitDynPropertyEnum.Mp).value > (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 0.5f) &&
                    x.GetDynProperty(UnitDynPropertyEnum.Sp).value > (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 0.3f) &&
                    x.GetGradeLvl() >= (g.world.playerUnit.GetGradeLvl() - 1) &&
                    (IsFriendlyUnit(x) || IsEnemyUnit(x));
            }).ToList();
        }

        [EventCondition(IsInGame = HandleEnum.False, IsInBattle = HandleEnum.True, IsWorldRunning = HandleEnum.False)]
        public override void OnTimeUpdate1s()
        {
            base.OnTimeUpdate1s();
            if (!g.world.battle.data.isSelfBattle &&
                _aroundUnits != null)
            {
                //enemy unit join battle
                var enemyUnit = _aroundUnits.Where(x => IsEnemyUnit(x)).FirstOrDefault();
                if (enemyUnit != null &&
                    CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, NPC_JOIN_RATE))
                {
                    _aroundUnits.Remove(enemyUnit);
                    SceneType.battle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(enemyUnit.data, UnitType.Alone);
                    DramaTool.OpenDrama(ENEMY_JOIN_DRAMA, new DramaData() { unitLeft = enemyUnit, unitRight = null });
                }

                //friendly unit join battle
                var friendlyUnit = _aroundUnits.Where(x => IsFriendlyUnit(x)).FirstOrDefault();
                if (friendlyUnit != null &&
                    CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, NPC_JOIN_RATE))
                {
                    _aroundUnits.Remove(friendlyUnit);
                    SceneType.battle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(friendlyUnit.data, UnitType.PlayerNPC);
                    DramaTool.OpenDrama(FRIENDLY_JOIN_DRAMA, new DramaData() { unitLeft = friendlyUnit, unitRight = null });
                }
            }
        }

        private bool IsFriendlyUnit(WorldUnitBase wunit)
        {
            return friendlyInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                wunit.data.GetRelationType(g.world.playerUnit) == UnitBothRelationType.Parents ||
                wunit.data.GetRelationType(g.world.playerUnit) == UnitBothRelationType.Married;
        }

        private bool IsEnemyUnit(WorldUnitBase wunit)
        {
            return enemyInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                wunit.data.GetRelationType(g.world.playerUnit) == UnitBothRelationType.Hater;
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
