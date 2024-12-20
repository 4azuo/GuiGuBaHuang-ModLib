﻿using EGameTypeData;
using ModLib.Object;
using System.Diagnostics;
using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$TITLES$", OrderIndex = 90, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
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
                var ui = new UICover<UILogin>(UIType.Login);
                {
                    var ver = g.mod.GetModProjectData(ModMaster.ModObj.ModId).ver;
                    var parentTransform = ui.UI.btnSet.transform.parent;
                    ui.AddButton(ui.LastCol - 5, ui.FirstRow, () => Process.Start("https://github.com/4azuo/GuiGuBaHuang-ModLib"), $"Powered by Fouru's ModLib\n{ver}")
                        .Align(TextAnchor.MiddleCenter)
                        .Format(Color.black, 18)
                        .Size(300, 74)
                        .SetParentTransform(parentTransform);

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
                }
            }
        }

        //public static string GetTranslateLanguage()
        //{
        //    var t = EventHelper.GetEvent<ModTitleEvent>("$TITLES$");
        //    return TranslateCode[t.TranslateIndex];
        //}
    }
}
