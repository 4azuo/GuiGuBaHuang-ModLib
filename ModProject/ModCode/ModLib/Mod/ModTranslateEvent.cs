using EGameTypeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UIItemBase;

namespace ModLib.Mod
{
    [Cache("$TRANSLATE$", OrderIndex = 99999, CacheType = CacheAttribute.CType.Global)]
    public class ModTranslateEvent : ModEvent
    {
        public static List<GameObject> IgnoreGameObjects { get; } = new List<GameObject>();
        public static Dictionary<string, string> TranslatedText { get; } = new Dictionary<string, string>();
        public static List<object> TranslatingComp { get; } = new List<object>();
        //public static Font AriaFont { get; } = Resources.GetBuiltinResource<Font>("Arial.ttf");

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.ui == null || !g.ui.HasUI(e.uiType))
            {
                return;
            }
            if (IgnoreGameObjects.Contains(e.ui.gameObject))
            {
                return;
            }
            Translate(e.ui.gameObject);
        }

        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            var code = ModTitleEvent.GetTranslateCode();
            if (!string.IsNullOrEmpty(code) && !GameHelper.IsLoadingScreen())
            {
                if (TranslatingComp.Count > 0)
                {
                    var i = TranslatingComp.Count - 1;
                    var c = TranslatingComp[i];
                    TranslatingComp.RemoveAt(i);
                    if (c == null)
                        return;

                    //Task.Run(() =>
                    //{
                    //    try
                    //    {

                            //string orgText = GetText(c);
                            //string translatedText;
                            //if (!TranslatedText.TryGetValue(orgText, out translatedText))
                            //{
                            //    translatedText = TranslateHelper.Translate(orgText, code);
                            //    TranslatedText.Add(orgText, translatedText);
                            //}
                            //SetText(c, translatedText);
                            SetText(c, TranslateHelper.Translate(GetText(c), code));

                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        DebugHelper.WriteLine(ex);
                    //    }
                    //});
                }
            }
        }

        private string GetText(object comp)
        {
            if (comp == null)
                return string.Empty;
            if (comp is UIItemText)
            {
                var x = (UIItemText)comp;
                if (x.HasText)
                {
                    return ((UIItemText)comp).FormatStr;
                }
            }
            else if (comp is UIItemButton)
            {
                var x = (UIItemButton)comp;
                if (x.HasText)
                {
                    return ((UIItemButton)comp).FormatStr;
                }
            }
            else if (comp is Text)
                return ((Text)comp).text;
            else if (comp is TextMesh)
                return ((TextMesh)comp).text;
            else if (comp is TMP_Text)
                return ((TMP_Text)comp).text;
            return string.Empty;
        }

        private void SetText(object comp, string str)
        {
            if (comp == null)
                return;
            if (comp is UIItemText)
            {
                var x = (UIItemText)comp;
                if (x != null && x.HasText)
                {
                    //x.font = AriaFont;
                    x.Set(str);
                }
            }
            else if (comp is UIItemButton)
            {
                var x = (UIItemButton)comp;
                if (x != null && x.HasText)
                {
                    //x.font = AriaFont;
                    x.Set(str);
                }
            }
            else if (comp is Text)
            {
                var x = (Text)comp;
                if (x != null)
                {
                    //x.font = AriaFont;
                    x.text = str;
                }
            }
            else if (comp is TextMesh)
            {
                var x = (TextMesh)comp;
                if (x != null)
                {
                    //x.font = AriaFont;
                    x.text = str;
                }
            }
            else if (comp is TMP_Text)
            {
                var x = (TMP_Text)comp;
                if (x != null)
                {
                    //x.font = AriaFont;
                    x.SetText(str);
                    //x.text = str;
                }
            }
        }

        public static void Translate(GameObject ui)
        {
            var myCompItems = UIHelper.Items.Where(x => typeof(UIItemComposite).IsAssignableFrom(x.GetType())).ToList();
            var myItems = UIHelper.Items.Where(x => typeof(UIItemText).IsAssignableFrom(x.GetType()) || typeof(UIItemButton).IsAssignableFrom(x.GetType())).ToList();
            myItems.AddRange(myCompItems.Select(x => (x as UIItemComposite).Prefix).ToArray());
            myItems.AddRange(myCompItems.Select(x => (x as UIItemComposite).MainComponent).ToArray());
            myItems.AddRange(myCompItems.Select(x => (x as UIItemComposite).Postfix).ToArray());

            var myTexts = myItems.Select(x => x.InnerText).ToList();

            var texts = ui.GetComponentsInChildren<Text>().Where(x => !myTexts.Contains(x));
            TranslatingComp.AddRange(myItems);
            TranslatingComp.AddRange(texts);
            TranslatingComp.AddRange(ui.GetComponentsInChildren<Text>());
            TranslatingComp.AddRange(ui.GetComponentsInChildren<TextMesh>());
            TranslatingComp.AddRange(ui.GetComponentsInChildren<TMP_Text>());
        }

        public static void Reload()
        {
            TranslatingComp.Clear();
            Translate(g.root);
        }
    }
}
