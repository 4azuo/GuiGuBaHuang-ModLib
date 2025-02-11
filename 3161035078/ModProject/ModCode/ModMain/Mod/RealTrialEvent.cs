using EBattleTypeData;
using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using static MOD_nE7UL2.Object.GameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.REAL_TRIAL_EVENT)]
    public class RealTrialEvent : ModEvent
    {
        public static RealTrialEvent Instance { get; set; }
        public static _RealTrialConfigs RealTrialConfigs => ModMain.ModObj.GameSettings.RealTrialConfigs;
        public static bool IsInTrial => g.world.battle.data.dungeonBaseItem.id == 110081;

        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);

            var data = e?.data;
            if (IsInTrial && !e.IsPlayer())
            {
                var atk = g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Attack).value;
                var def = g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Defense).value;
                var basisThunder = g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.BasisThunder).value;
                var baseDmg = (atk * (
                        g.data.dataWorld.data.gameLevel.Parse<int>() * RealTrialConfigs.PowerUpOnGameLevel
                        + g.world.playerUnit.GetGradeLvl() * RealTrialConfigs.PowerUpOnGradeLevel
                    )).Parse<int>()
                    - (def * basisThunder / 100);
                data.attack.baseValue = baseDmg;
            }
        }

        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);

            if (IsInTrial && !ModBattleEvent.PlayerUnit.isDie)
            {
                foreach (var p in UnitTypeEnum.Trial.PropIncRatio)
                {
                    var pType = p.Values[0] as UnitPropertyEnum;
                    g.world.playerUnit.AddProperty(pType, UnitTypeEnum.Trial.CalProp(pType, g.world.playerUnit.GetProperty<int>(pType)));
                }
            }
        }
    }
}
