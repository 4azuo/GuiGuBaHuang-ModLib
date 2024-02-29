using EBattleTypeData;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.REAL_TRIAL_EVENT)]
    public class RealTrialEvent : ModEvent
    {
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
                data.attack.baseValue = (g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Attack) * (1.00f + g.data.dataWorld.data.gameLevel.Parse<int>() * 0.50f)).Parse<int>();
            }
        }

        public override void OnBattleEnd(BattleEnd e)
        {
            IsInTrial = false;
        }
    }
}
