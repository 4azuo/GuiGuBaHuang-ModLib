using EGameTypeData;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModLib.Mod
{
    [Cache("$TRANSLATE$", OrderIndex = 99999, CacheType = CacheAttribute.CType.Global)]
    public class ModTranslateEvent : ModEvent
    {
        public static Dictionary<string, string> TranslatedText { get; } = new Dictionary<string, string>();
        public static List<Component> TranslatingComp { get; } = new List<Component>();

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            Load(e.ui);
        }

        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            var code = ModTitleEvent.GetTranslateCode();
            if (code != null)
            {
                if (TranslatingComp.Count > 0)
                {
                    var i = TranslatingComp.Count - 1;
                    var c = TranslatingComp[i];
                    if (c == null)
                    {
                        TranslatingComp.RemoveAt(i);
                        return;
                    }

                    string orgText = GetText(c);
                    string translatedText;
                    if (!TranslatedText.TryGetValue(orgText, out translatedText))
                    {
                        translatedText = TranslateHelper.Translate(orgText, code);
                        TranslatedText.Add(orgText, translatedText);
                    }
                    SetText(c, translatedText);
                    TranslatingComp.RemoveAt(i);
                }
            }
        }

        private string GetText(Component comp)
        {
            if (comp is Text)
                return ((Text)comp).text;
            else if (comp is TextMesh)
                return ((TextMesh)comp).text;
            else if (comp is TMP_Text)
                return ((TMP_Text)comp).text;
            return string.Empty;
        }

        private void SetText(Component comp, string str)
        {
            if (comp is Text)
                ((Text)comp).text = str;
            else if (comp is TextMesh)
                ((TextMesh)comp).text = str;
            else if (comp is TMP_Text)
                ((TMP_Text)comp).text = str;
        }

        public static void Load(UIBase ui)
        {
            TranslatingComp.AddRange(ui.GetComponentsInChildren<Text>());
            TranslatingComp.AddRange(ui.GetComponentsInChildren<TextMesh>());
            TranslatingComp.AddRange(ui.GetComponentsInChildren<TMP_Text>());
        }

        public static void Reload()
        {
            TranslatedText.Clear();
            TranslatingComp.Clear();

            TranslatingComp.AddRange(g.root.GetComponentsInChildren<Text>());
            TranslatingComp.AddRange(g.root.GetComponentsInChildren<TextMesh>());
            TranslatingComp.AddRange(g.root.GetComponentsInChildren<TMP_Text>());
        }
    }
}
