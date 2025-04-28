using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class CompHelper
{
    private static ulong _compCounter = 0;

    public static T Init<T>(this T newObj) where T : Component
    {
        var rect = newObj.gameObject.GetComponent<RectTransform>();

        // Set anchor
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        // Set pivot
        rect.pivot = new Vector2(0.5f, 0.5f);

        //reset
        if (newObj is Button)
        {
            var o = newObj as Button;
            o.onClick = new Button.ButtonClickedEvent();
        }
        else if (newObj is Slider)
        {
            var o = newObj as Slider;
            o.onValueChanged = new Slider.SliderEvent();
        }
        else if (newObj is Toggle)
        {
            var o = newObj as Toggle;
            o.group = null;
            o.SetIsOnWithoutNotify(false);
            o.onValueChanged = new Toggle.ToggleEvent();
        }
        else if (newObj is InputField)
        {
            var o = newObj as InputField;
            o.text = string.Empty;
            o.SetPlaceHolder(string.Empty);
            o.onValidateInput = null;
            o.characterValidation = InputField.CharacterValidation.None;
            o.characterLimit = -1;
            o.contentType = InputField.ContentType.Standard;
            o.shouldActivateOnSelect = true;
            o.onEndEdit = new InputField.SubmitEvent();
            o.onValueChange = new InputField.OnChangeEvent();
            o.onValueChanged = new InputField.OnChangeEvent();
        }
        else if (newObj is Image)
        {
            var o = newObj as Image;
            o.onCullStateChanged = new Image.CullStateChangedEvent();
        }
        return newObj;
    }

    public static T Create<T>(this T obj, Transform transform = null) where T : Component
    {
        var t = transform ?? obj.transform.parent;
        var newObj = Object.Instantiate(obj, t, false);
        newObj.gameObject.name = $"CompHelper:{_compCounter++}";
        newObj.gameObject.SetActive(false);
        return newObj.Init();
    }

    public static T Replace<T>(this T obj, Transform transform = null) where T : Component
    {
        var t = transform ?? obj.transform.parent;
        var newObj = Object.Instantiate(obj, t, false);
        newObj.Pos(obj);
        newObj.gameObject.name = $"CompHelper:{_compCounter++}";
        newObj.gameObject.SetActive(true);
        obj.gameObject.SetActive(false);
        return newObj.Init();
    }

    public static T Replace<T>(this T obj, UIBase ui) where T : Component
    {
        return Replace<T>(obj, ui.canvas.transform);
    }

    public static T Copy<T>(this T obj, Transform transform = null) where T : Component
    {
        var t = transform ?? obj.transform.parent;
        var newObj = Object.Instantiate(obj, t, false);
        newObj.Pos(obj);
        newObj.gameObject.name = $"CompHelper:{_compCounter++}";
        newObj.gameObject.SetActive(true);
        return newObj.Init();
    }

    public static T Copy<T>(this T obj, UIBase ui) where T : Component
    {
        return Copy<T>(obj, ui.canvas.transform);
    }

    public static Text Align(this Text obj, TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
    {
        obj.alignment = tanchor;
        obj.verticalOverflow = vMode;
        obj.horizontalOverflow = hMode;
        return obj;
    }

    public static Button Align(this Button obj, TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
    {
        var txt = obj.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.Align(tanchor, vMode, hMode);
        }
        return obj;
    }

    public static Toggle Align(this Toggle obj, TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
    {
        var txt = obj.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.Align(tanchor, vMode, hMode);
        }
        return obj;
    }

    public static InputField Align(this InputField obj, TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
    {
        obj.textComponent.Align(tanchor, vMode, hMode);
        return obj;
    }

    public static Text Format(this Text obj, Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
    {
        obj.fontSize = fsize;
        obj.fontStyle = fstype;
        obj.color = color ?? Color.black;
        return obj;
    }

    public static Button Format(this Button obj, Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
    {
        var txt = obj.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.Format(color, fsize, fstype);
        }
        return obj;
    }

    public static Toggle Format(this Toggle obj, Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
    {
        var txt = obj.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.Format(color, fsize, fstype);
        }
        return obj;
    }

    public static InputField Format(this InputField obj, Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
    {
        obj.textComponent.Format(color, fsize, fstype);
        return obj;
    }

    public static Vector2 Pos(this GameObject obj)
    {
        var rect = obj.GetComponent<RectTransform>();
        return rect.anchoredPosition;
    }

    public static Vector2 Pos(this Component obj)
    {
        return obj.gameObject.Pos();
    }

    public static T Pos<T>(this T obj, GameObject origin, float deltaX = 0f, float deltaY = 0f) where T : Component
    {
        var orgPos = origin.Pos();
        var rect = obj.gameObject.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(orgPos.x + deltaX, orgPos.y + deltaY);
        return obj;
    }

    public static T Pos<T>(this T obj, Component origin, float deltaX = 0f, float deltaY = 0f) where T : Component
    {
        return obj.Pos(origin.gameObject, deltaX, deltaY);
    }

    public static T Pos<T>(this T obj, float deltaX, float deltaY) where T : Component
    {
        var rect = obj.gameObject.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(deltaX, deltaY);
        return obj;
    }

    public static T Size<T>(this T obj, float scaleX = 0f, float scaleY = 0f) where T : Component
    {
        Parallel.ForEach(obj.GetComponentsInChildren<RectTransform>(), s => s.sizeDelta = new Vector2(scaleX, scaleY));
        return obj;
    }

    public static InputField Size(this InputField obj, float scaleX = 0f, float scaleY = 0f)
    {
        var r = obj.GetComponentInChildren<RectTransform>();
        r.sizeDelta = new Vector2(scaleX, scaleY);
        return obj;
    }

    public static Image Size(this Image obj, float scaleX = 0f, float scaleY = 0f)
    {
        obj.rectTransform.sizeDelta = new Vector2(scaleX, scaleY);
        return obj;
    }

    public static Toggle Size(this Toggle obj, float scaleX = 0f, float scaleY = 0f)
    {
        var r = obj.GetComponentInChildren<RectTransform>();
        r.sizeDelta = new Vector2(scaleX, scaleY);
        return obj;
    }

    public static T AddSize<T>(this T obj, float scaleX = 0f, float scaleY = 0f) where T : Component
    {
        Parallel.ForEach(obj.GetComponentsInChildren<RectTransform>(), s => s.sizeDelta = new Vector2(s.sizeDelta.x + scaleX, s.sizeDelta.y + scaleY));
        return obj;
    }

    public static InputField AddSize(this InputField obj, float scaleX = 0f, float scaleY = 0f)
    {
        var r = obj.GetComponentInChildren<RectTransform>();
        r.sizeDelta = new Vector2(r.sizeDelta.x + scaleX, r.sizeDelta.y + scaleY);
        return obj;
    }

    public static Image AddSize(this Image obj, float scaleX = 0f, float scaleY = 0f)
    {
        obj.rectTransform.sizeDelta = new Vector2(obj.rectTransform.sizeDelta.x + scaleX, obj.rectTransform.sizeDelta.y + scaleY);
        return obj;
    }

    public static Toggle AddSize(this Toggle obj, float scaleX = 0f, float scaleY = 0f)
    {
        var r = obj.GetComponentInChildren<RectTransform>();
        r.sizeDelta = new Vector2(r.sizeDelta.x + scaleX, r.sizeDelta.y + scaleY);
        return obj;
    }

    public static Vector2 GetSize(this MonoBehaviour obj)
    {
        var rect = obj.GetComponentInChildren<RectTransform>();
        if (rect != null)
            return rect.sizeDelta;
        return Vector2.zero;
    }

    public static Vector2 GetSize(this Image obj)
    {
        return obj.rectTransform.sizeDelta;
    }

    public static Text Set(this Text obj, string def)
    {
        obj.text = def;
        return obj;
    }

    public static Slider Set(this Slider obj, float min, float max, float def)
    {
        obj.minValue = min;
        obj.maxValue = max;
        obj.value = def.FixValue(min, max);
        return obj;
    }

    public static Toggle Set(this Toggle obj, bool def, string defText = null)
    {
        obj.isOn = def;
        var txt = obj.GetComponentInChildren<Text>();
        if (txt != null)
            txt.text = defText;
        return obj;
    }

    public static Button Set(this Button obj, string def)
    {
        var txt = obj.GetComponentInChildren<Text>();
        if (txt != null)
            txt.text = def;
        return obj;
    }

    public static InputField SetPlaceHolder(this InputField obj, string def)
    {
        var txt = obj.placeholder.GetComponentInChildren<Text>();
        if (txt != null)
            txt.text = def;
        return obj;
    }
}