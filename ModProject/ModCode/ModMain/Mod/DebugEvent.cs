//using ModLib.Enum;
//using ModLib.Mod;

//namespace MOD_nE7UL2.Mod
//{
//    [Cache("DEBUG")]
//    public class DebugEvent : ModEvent
//    {
//        public override void OnLoadGame()
//        {
//            g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Attack, 1000000);
//            g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Defense, 1000000);
//            g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.HpMax, 1000000);
//            g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.MpMax, 1000000);
//            g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.SpMax, 1000000);
//            g.world.playerUnit.data.RewardPropMoney(1000000);

//            g.world.playerUnit.data.unitData.propertyData.footSpeed = 10000;
//            g.world.playerUnit.data.dynUnitData.playerView.baseValue = 20;
//        }
//    }
//}
