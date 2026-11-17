using ModLib.Attributes;
using UnityEngine;
using ModLib.Helper;

namespace ModLib.Mod
{
    /// <summary>
    /// Built-in UI refresher (Global, order 10).
    /// </summary>
    [Cache("$UI$", OrderIndex = 10, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class ModUIEvent : ModEvent
    {
        /// <summary>
        /// Refreshes every custom UI with IsAutoUpdate = true.
        /// </summary>
        [ErrorIgnore]
        public override void OnTimeUpdate100ms()
        {
            base.OnTimeUpdate100ms();
            UIHelper.DisposeClosedUI();
            UIHelper.UpdateAllUI(x => x.IsAutoUpdate);
        }

        //public override void OnMonoUpdate()
        //{
        //    base.OnMonoUpdate();
        //    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.P))
        //    {
        //        DebugHelper.WriteLine("==================");
        //        DebugHelper.WriteLine(g.res.abPath);
        //        DebugHelper.WriteLine("===== allRes =====");
        //        foreach (var r in g.res.allRes)
        //        {
        //            DebugHelper.WriteLine($"{r.key}");
        //        }
        //        DebugHelper.WriteLine("===== allAB =====");
        //        foreach (var r in g.res.allAB)
        //        {
        //            DebugHelper.WriteLine($"{r.key}");
        //        }
        //        DebugHelper.Save();
        //    }
        //}
    }
}
