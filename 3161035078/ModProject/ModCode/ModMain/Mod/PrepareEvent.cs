using ModLib.Attributes;
using ModLib.Mod;
using ModLib.Object;
using System.Linq;
using ModLib.Helper;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Runs first every month (order 1000): when "All functions apply to nearest units" is on, limits the units
    /// used by Taoist events to areas within 3 of the player (performance).
    /// </summary>
    [Cache("$PrepareEvent$")]
    public class PrepareEvent : ModEvent
    {
        /// <summary>
        /// Sets the Taoist ParameterStore to the nearby units.
        /// </summary>
        public override void OnRefreshParameterStoreOnMonthly()
        {
            base.OnRefreshParameterStoreOnMonthly();
            if (SMLocalConfigsEvent.Instance.Configs.AllFunctionsApplyToNearestUnits)
            {
                var curAreaId = g.world.playerUnit.GetUnitPosAreaId();
                SetModChildParameterStore(ParameterStore.CreateModChildParameterStore
                (
                    wunits: ModLib.Mod.ModMaster.ModObj.ParameterStore.WUnits.Where( // the full list of ModLib (not the nearby units of last month)
                    x => x != null && x.GetUnitPosAreaId().IsBetween(curAreaId - 3, curAreaId + 3)).ToArray()
                ));
            }
        }
    }
}
