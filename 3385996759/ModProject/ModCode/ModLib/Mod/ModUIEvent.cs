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
            // icons in the texts the game sets itself (drama options...)
            foreach (var t in DRAMA_UIS)
            {
                if (!g.ui.HasUI(t)) continue;
                var ui = g.ui.GetUI(t);
                if (ui != null) TextIconHelper.Refresh(ui);
            }
        }

        private static readonly UIType.UITypeBase[] DRAMA_UIS = { UIType.DramaDialogue, UIType.DramaFortuitous, UIType.DramaDialogueBigTexture, UIType.DramaBigTexture };

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
