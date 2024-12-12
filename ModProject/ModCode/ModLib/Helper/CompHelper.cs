using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class CompHelper
{
    public static T Replace<T>(this T obj, Transform transform = null) where T : MonoBehaviour
    {
        var t = transform ?? obj.transform.parent;
        var newObj = MonoBehaviour.Instantiate(obj, t, false).Pos(obj.transform);
        newObj.gameObject.SetActive(true);
        obj.gameObject.SetActive(false);
        if (newObj is Button)
        {
            var o = newObj as Button;
            o.onClick.RemoveAllListeners();
        }
        return newObj;
    }

    public static T Replace<T>(this T obj, UIBase ui) where T : MonoBehaviour
    {
        return Replace<T>(obj, ui.canvas.transform);
    }

    public static T Copy<T>(this T obj, Transform transform = null) where T : MonoBehaviour
    {
        var t = transform ?? obj.transform.parent;
        var newObj = MonoBehaviour.Instantiate(obj, t, false).Pos(obj.transform);
        newObj.gameObject.SetActive(true);
        if (newObj is Text)
        {
            var o = newObj as Text;
            o.text = string.Empty;
        }
        if (newObj is Button)
        {
            var o = newObj as Button;
            o.onClick.RemoveAllListeners();
        }
        return newObj;
    }

    public static T Copy<T>(this T obj, UIBase ui) where T : MonoBehaviour
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

    public static T Pos<T>(this T obj, Transform origin, float deltaX = 0f, float deltaY = 0f, float deltaZ = 0f) where T : MonoBehaviour
    {
        obj.transform.position = new Vector3((origin?.position.x ?? 0) + deltaX, (origin?.position.y ?? 0) + deltaY, (origin?.position.z ?? 0) + deltaZ);
        return obj;
    }

    public static T Pos<T>(this T obj, GameObject origin, float deltaX = 0f, float deltaY = 0f, float deltaZ = 0f) where T : MonoBehaviour
    {
        obj.transform.position = new Vector3((origin?.transform?.position.x ?? 0) + deltaX, (origin?.transform?.position.y ?? 0) + deltaY, (origin?.transform?.position.z ?? 0) + deltaZ);
        return obj;
    }

    public static T Pos<T>(this T obj, float deltaX, float deltaY, float deltaZ) where T : MonoBehaviour
    {
        obj.transform.position = new Vector3(deltaX, deltaY, deltaZ);
        return obj;
    }

    public static T Pos<T>(this T obj, float deltaX, float deltaY) where T : MonoBehaviour
    {
        obj.transform.position = new Vector3(deltaX, deltaY, obj.transform.position.z);
        return obj;
    }

    public static T Size<T>(this T obj, float scaleX = 0f, float scaleY = 0f) where T : MonoBehaviour
    {
        Parallel.ForEach(obj.GetComponentsInChildren<RectTransform>(), s => s.sizeDelta = new Vector2(scaleX, scaleY));
        return obj;
    }

    public static T AddSize<T>(this T obj, float scaleX = 0f, float scaleY = 0f) where T : MonoBehaviour
    {
        Parallel.ForEach(obj.GetComponentsInChildren<RectTransform>(), s => s.sizeDelta = new Vector2(s.sizeDelta.x + scaleX, s.sizeDelta.y + scaleY));
        return obj;
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

    public static Toggle Set(this Toggle obj, bool def)
    {
        obj.isOn = def;
        return obj;
    }

    public static Button Set(this Button obj, string def)
    {
        var txt = obj.GetComponentInChildren<Text>();
        if (txt != null)
            txt.text = def;
        return obj;
    }
}