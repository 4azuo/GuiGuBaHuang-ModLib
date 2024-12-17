using EGameTypeData;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UIItemBase;

namespace ModLib.Mod
{
    [Cache("$TRANSLATE$", OrderIndex = 99999, CacheType = CacheAttribute.CType.Global)]
    public class ModTranslateEvent : ModEvent
    {
        public const int TRANS_SPEED = 16;
        public const int ITEM_MAX_LEN = 200;

        public static List<object> TranslatingComp { get; } = new List<object>();
        public static Dictionary<string, string> TranslatedText { get; } = new Dictionary<string, string>();

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            var lang = ModTitleEvent.GetTranslateLanguage();
            if (lang != null)
            {
                if (e?.ui?.gameObject == null || !g.ui.HasUI(e.uiType))
                {
                    return;
                }
                Translate(e.ui.gameObject);
                foreach (var c in TranslatingComp.ToArray())
                {
                    var orgText = GetText(c);
                    string translatedText;
                    if (TranslatedText.TryGetValue(orgText, out translatedText))
                    {
                        SetText(c, translatedText);
                        TranslatingComp.Remove(c);
                    }
                }
            }
        }

        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            var lang = ModTitleEvent.GetTranslateLanguage();
            if (lang != null && !GameHelper.IsLoadingScreen())
            {
                if (TranslatingComp.Count > 0)
                {
                    var i = TranslatingComp.Count - 1;
                    var c = TranslatingComp[i];
                    TranslatingComp.RemoveAt(i);
                    if (c == null)
                        return;

                    var orgText = GetText(c);
                    if (!TranslatedText.ContainsKey(orgText))
                    {
                        var translatedText = TranslateHelper.Translate(orgText, lang);
                        TranslatedText.Add(orgText, translatedText);
                        SetText(c, translatedText);
                    }
                }
            }
        }

        private string GetText(object comp)
        {
            if (comp == null)
                return string.Empty;
            if (comp is ITextFormat)
                return ((ITextFormat)comp).GetFormat();
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
            if (comp is ITextFormat)
            {
                var x = (ITextFormat)comp;
                if (x != null)
                {
                    x.SetFormat(str);
                }
            }
            else if (comp is Text)
            {
                var x = (Text)comp;
                if (x != null)
                {
                    x.text = str;
                }
            }
            else if (comp is TextMesh)
            {
                var x = (TextMesh)comp;
                if (x != null)
                {
                    x.text = str;
                }
            }
            else if (comp is TMP_Text)
            {
                var x = (TMP_Text)comp;
                if (x != null)
                {
                    x.text = str;
                }
            }
        }

        public static List<object> GetTranslatingObjects(GameObject go)
        {
            var translatingComp = new List<object>();

            translatingComp.AddRange(go.GetComponentsInChildren<Text>(true));
            translatingComp.AddRange(go.GetComponentsInChildren<TextMesh>());
            translatingComp.AddRange(go.GetComponentsInChildren<TMP_Text>());
            translatingComp.AddRange(UIHelper.AllItems.Where(x => typeof(ITextFormat).IsAssignableFrom(x.GetType())));

            return translatingComp;
        }

        public static void Translate(GameObject go)
        {
            TranslatingComp.AddRange(GetTranslatingObjects(go));
        }

        public static void ClearCache()
        {
            TranslatedText.Clear();
        }
    }
}
