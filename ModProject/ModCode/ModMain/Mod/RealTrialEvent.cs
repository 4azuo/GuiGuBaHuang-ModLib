using EBattleTypeData;
using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Object;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.REAL_TRIAL_EVENT)]
    public class RealTrialEvent : ModEvent
    {
        public static float POWER_UP_ON_GAME_LEVEL
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.RealTrialConfigs.PowerUpOnGameLevel;
            }
        }

        [JsonIgnore]
        public bool IsInTrial { get; set; } = false;

        public override void OnOpenDrama(OpenDrama e)
        {
            IsInTrial = e.dramaID == 20701;
        }

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            var data = e?.data;
            if (IsInTrial && data != null)
            {
                var baseDmg = (g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Attack) * (1.00f + g.data.dataWorld.data.gameLevel.Parse<int>() * POWER_UP_ON_GAME_LEVEL)).Parse<int>();
                baseDmg -= (g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.BasisThunder) / 10);
                data.attack.baseValue = baseDmg;
            }
        }

        public override void OnBattleEnd(BattleEnd e)
        {
            IsInTrial = false;
        }
    }
}
