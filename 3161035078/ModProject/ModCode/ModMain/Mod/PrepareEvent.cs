using ModLib.Mod;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    [Cache("$PrepareEvent$")]
    public class PrepareEvent : ModEvent
    {
        public override void OnRefreshParameterStoreOnMonthly()
        {
            base.OnRefreshParameterStoreOnMonthly();
            if (SMLocalConfigsEvent.Instance.Configs.AllFunctionsApplyToNearestUnits)
            {
                var curAreaId = g.world.playerUnit.GetUnitPosAreaId();
                ModMaster.ModObj.ParameterStore.WUnits = g.world.unit.GetUnits().ToArray().Where(x => x.GetUnitPosAreaId().IsBetween(curAreaId - 3, curAreaId + 3)).ToArray();
            }
        }
    }
}
