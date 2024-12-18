using EGameTypeData;
using ModLib.Object;
using System.Diagnostics;
using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$TITLES$", OrderIndex = 90, CacheType = CacheAttribute.CType.Global)]
    public class ModTitleEvent : ModEvent
    {
        //public static List<string> TranslateCode { get; } = new List<string>
        //{
        //    null,
        //    "en",
        //    "ja",
        //    "vi",
        //    "ru",
        //    "la",
        //    "es",
        //};

        public int TranslateIndex { get; set; }

        [EventCondition(IsInGame = 0)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var uiCustom = new UICover<UILogin>(UIType.Login, (ui) =>
                {
                    ui.AddButton(ui.LastCol - 5, ui.FirstRow, () =>
                    {
                        Process.Start("https://github.com/4azuo/GuiGuBaHuang-ModLib");
                    }, "Powered by Fouru's ModLib")
                        .Size(300, 60)
                        .Align(TextAnchor.MiddleCenter)
                        .Format(Color.black, 19);

                    //ui.AddSelect(ui.LastCol - 14, ui.FirstRow, new string[] { "Translation: Off", "English", "Japanese", "Vietnamese", "Russian", "Latin", "Spanish" }, TranslateIndex)
                    //    .Align(TextAnchor.MiddleCenter)
                    //    .SetWork(new UIItemBase.UIItemWork
                    //    {
                    //        ChangeAct = (a, b) =>
                    //        {
                    //            TranslateIndex = b.Parse<int>();
                    //            CacheHelper.SaveGlobalCache(this);
                    //            ModTranslateEvent.ClearCache();
                    //        }
                    //    });
                });
            }
        }

        //public static string GetTranslateLanguage()
        //{
        //    var t = EventHelper.GetEvent<ModTitleEvent>("$TITLES$");
        //    return TranslateCode[t.TranslateIndex];
        //}
    }
}
