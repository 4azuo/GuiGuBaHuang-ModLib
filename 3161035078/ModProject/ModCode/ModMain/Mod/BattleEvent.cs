using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_EVENT)]
    public class BattleEvent : ModEvent
    {
        public const int ADD_ROOM_RATE = 3;

        public int PvPCount { get; set; } = 0;

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);

            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            var count = CommonTool.Random(1, ADD_ROOM_RATE * gameLvl);
            //SceneType.battle.room.room.allRoom.Add(new BattleRoomNode()
            //{

            //});
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
