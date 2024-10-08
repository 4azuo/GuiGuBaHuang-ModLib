using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.REVENGE_EVENT)]
    public class RevengeEvent : ModEvent
    {
        public int Counter { get; set; } = 0;

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
                Counter++;
            }
        }

        //npc revenge player
        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            base.OnIntoBattleFirst(e);
            var humanData = e?.data?.TryCast<UnitDataHuman>();
            if (g.world.battle.data.isRealBattle &&
                (!humanData?.worldUnitData?.unit?.IsPlayer() ?? false) &&
                humanData?.worldUnitData?.GetRelationType(g.world.playerUnit) == UnitBothRelationType.Hater)
            {
                humanData.attack.baseValue += (int)(humanData.attack.value * (Counter / 100.00f));
            }
        }
    }
}
