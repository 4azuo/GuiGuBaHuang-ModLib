using ModLib.Attributes;
using UnityEngine;
using ModLib.Helper;

namespace ModLib.Mod
{
    /// <summary>
    /// Built-in UI refresher (Global, order 10); private mods (PrivateModHelper): no Steam Workshop upload in the mod editor.
    /// </summary>
    [Cache("$UI$", OrderIndex = 10, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class ModUIEvent : ModEvent
    {
        /// <summary>
        /// Mod editor: the private_mod.json files read again when a project window opens; its Workshop button hidden and
        /// the upload window of a private project closed (InitData sets the project after the opening: one frame later).
        /// </summary>
        public override void OnOpenUIEnd(EGameTypeData.OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.ModMainFunction.uiName)
            {
                PrivateModHelper.ClearCache();
                g.timer.Frame(ActionHelper.TracedIl2Action(() => PrivateModHelper.HideUploadButton(g.ui.GetUI<UIModMainFunction>(UIType.ModMainFunction))), 1, false);
            }
            else if (e.uiType.uiName == UIType.ModWorkshopUpload.uiName)
            {
                g.timer.Frame(ActionHelper.TracedIl2Action(() => PrivateModHelper.BlockUploadWindow(g.ui.GetUI<UIModWorkshopUpload>(UIType.ModWorkshopUpload))), 1, false);
            }
        }

        /// <summary>
        /// Refreshes every custom UI with IsAutoUpdate = true; keeps the Workshop button of a private project hidden (the
        /// game shows it again when it updates the window).
        /// </summary>
        [ErrorIgnore]
        public override void OnTimeUpdate100ms()
        {
            base.OnTimeUpdate100ms();
            UIHelper.DisposeClosedUI();
            UIHelper.UpdateAllUI(x => x.IsAutoUpdate);
            if (g.ui.HasUI(UIType.ModMainFunction))
                PrivateModHelper.HideUploadButton(g.ui.GetUI<UIModMainFunction>(UIType.ModMainFunction));
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
