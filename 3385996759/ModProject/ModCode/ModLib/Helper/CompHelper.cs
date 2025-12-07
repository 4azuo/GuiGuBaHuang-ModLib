using ModLib.Attributes;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for working with Unity UI components.
    /// Provides utilities for creating, initializing, and managing UI elements.
    /// </summary>
    [ActionCat("Component")]
    public static class CompHelper
    {
        private static ulong _compCounter = 0;

        /// <summary>
        /// Initializes a UI component by resetting its properties and event handlers.
        /// Handles different component types (Button, Slider, Toggle, InputField, Image, etc.).
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="newObj">The component to initialize</param>
        /// <returns>The initialized component</returns>
        public static T Init<T>(this T newObj) where T : Component
        {
            var rect = newObj.gameObject.GetComponent<RectTransform>();

            //reset trigger
            var trigger = newObj.gameObject.GetComponent<EventTrigger>();
            if (trigger != null)
            {
                UnityEngine.Object.Destroy(trigger);
            }

            //reset
            if (newObj is Text)
            {
                // Set anchor
                //rect.anchorMin = new Vector2(0.5f, 0.5f);
                //rect.anchorMax = new Vector2(0.5f, 0.5f);

                // Set pivot
                //rect.pivot = new Vector2(0.5f, 0.5f);
            }
            else
            {
                // Set anchor
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);

                // Set pivot
                rect.pivot = new Vector2(0.5f, 0.5f);

                //init comp
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
            }
            return newObj;
        }

        /// <summary>
        /// Creates a new instance of a component by cloning an existing one.
        /// The new component is initialized and set to inactive.
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="obj">The component to clone</param>
        /// <param name="transform">Optional parent transform (defaults to original parent)</param>
        /// <returns>The newly created component</returns>
        public static T Create<T>(this T obj, Transform transform = null) where T : Component
        {
            var t = transform ?? obj.transform.parent;
            var newObj = UnityEngine.Object.Instantiate(obj, t, false);
            newObj.gameObject.name = $"CompHelper:{_compCounter++}";
            newObj.gameObject.SetActive(false);
            return newObj.Init();
        }

        /// <summary>
        /// Replaces a component with a new clone, copying position and deactivating the original.
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="obj">The component to replace</param>
        /// <param name="transform">Optional parent transform (defaults to original parent)</param>
        /// <returns>The replacement component</returns>
        public static T Replace<T>(this T obj, Transform transform = null) where T : Component
        {
            var t = transform ?? obj.transform.parent;
            var newObj = UnityEngine.Object.Instantiate(obj, t, false);
            newObj.Pos(obj);
            newObj.gameObject.name = $"CompHelper:{_compCounter++}";
            newObj.gameObject.SetActive(true);
            obj.gameObject.SetActive(false);
            return newObj.Init();
        }

        /// <summary>
        /// Replaces a component with a new clone within a UI canvas.
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="obj">The component to replace</param>
        /// <param name="ui">The UI containing the canvas</param>
        /// <returns>The replacement component</returns>
        public static T Replace<T>(this T obj, UIBase ui) where T : Component
        {
            return Replace<T>(obj, ui.canvas.transform);
        }

        /// <summary>
        /// Creates a copy of a component at the same position and activates it.
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="obj">The component to copy</param>
        /// <param name="transform">Optional parent transform</param>
        /// <returns>The copied component</returns>
        public static T Copy<T>(this T obj, Transform transform = null) where T : Component
        {
            var t = transform ?? obj.transform.parent;
            var newObj = UnityEngine.Object.Instantiate(obj, t, false);
            newObj.Pos(obj);
            newObj.gameObject.name = $"CompHelper:{_compCounter++}";
            newObj.gameObject.SetActive(true);
            return newObj.Init();
        }

        /// <summary>
        /// Creates a copy of a component within a UI canvas.
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="obj">The component to copy</param>
        /// <param name="ui">The UI containing the canvas</param>
        /// <returns>The copied component</returns>
        public static T Copy<T>(this T obj, UIBase ui) where T : Component
        {
            return Copy<T>(obj, ui.canvas.transform);
        }

        /// <summary>
        /// Sets text alignment and wrap modes for a Text component.
        /// </summary>
        /// <param name="obj">The Text component</param>
        /// <param name="tanchor">Text anchor (default: MiddleLeft)</param>
        /// <param name="vMode">Vertical wrap mode (default: Overflow)</param>
        /// <param name="hMode">Horizontal wrap mode (default: Overflow)</param>
        /// <returns>The modified Text component</returns>
        public static Text Align(this Text obj, TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            obj.alignment = tanchor;
            obj.verticalOverflow = vMode;
            obj.horizontalOverflow = hMode;
            return obj;
        }

        /// <summary>
        /// Sets text alignment for a Button's text component.
        /// </summary>
        public static Button Align(this Button obj, TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            var txt = obj.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.Align(tanchor, vMode, hMode);
            }
            return obj;
        }

        /// <summary>
        /// Sets text alignment for a Toggle's text component.
        /// </summary>
        public static Toggle Align(this Toggle obj, TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            var txt = obj.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.Align(tanchor, vMode, hMode);
            }
            return obj;
        }

        /// <summary>
        /// Sets text alignment for an InputField's text component.
        /// </summary>
        public static InputField Align(this InputField obj, TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            obj.textComponent.Align(tanchor, vMode, hMode);
            return obj;
        }

        /// <summary>
        /// Sets text formatting (color, size, style) for a Text component.
        /// </summary>
        /// <param name="obj">The Text component</param>
        /// <param name="color">Text color (default: black)</param>
        /// <param name="fsize">Font size (default: 15)</param>
        /// <param name="fstype">Font style (default: Normal)</param>
        /// <returns>The modified Text component</returns>
        public static Text Format(this Text obj, Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
        {
            obj.fontSize = fsize;
            obj.fontStyle = fstype;
            obj.color = color ?? Color.black;
            return obj;
        }

        /// <summary>
        /// Sets text formatting for a Button's text component.
        /// </summary>
        public static Button Format(this Button obj, Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
        {
            var txt = obj.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.Format(color, fsize, fstype);
            }
            return obj;
        }

        /// <summary>
        /// Sets text formatting for a Toggle's text component.
        /// </summary>
        public static Toggle Format(this Toggle obj, Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
        {
            var txt = obj.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.Format(color, fsize, fstype);
            }
            return obj;
        }

        /// <summary>
        /// Sets text formatting for an InputField's text component.
        /// </summary>
        public static InputField Format(this InputField obj, Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
        {
            obj.textComponent.Format(color, fsize, fstype);
            return obj;
        }

        /// <summary>
        /// Gets the anchored position of a GameObject.
        /// </summary>
        /// <param name="obj">The GameObject</param>
        /// <returns>The anchored position</returns>
        public static Vector2 Pos(this GameObject obj)
        {
            var rect = obj.GetComponent<RectTransform>();
            return rect.anchoredPosition;
        }

        /// <summary>
        /// Gets the anchored position of a Component's GameObject.
        /// </summary>
        public static Vector2 Pos(this Component obj)
        {
            return obj.gameObject.Pos();
        }

        /// <summary>
        /// Sets the position of a component with optional delta offsets.
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="obj">The component to position</param>
        /// <param name="orgPos">Original position</param>
        /// <param name="deltaX">X offset (default: 0)</param>
        /// <param name="deltaY">Y offset (default: 0)</param>
        /// <returns>The positioned component</returns>
        public static T Pos<T>(this T obj, Vector2 orgPos, float deltaX = 0f, float deltaY = 0f) where T : Component
        {
            var rect = obj.gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(orgPos.x + deltaX, orgPos.y + deltaY);
            return obj;
        }

        /// <summary>
        /// Sets the position of a component relative to another GameObject.
        /// </summary>
        public static T Pos<T>(this T obj, GameObject origin, float deltaX = 0f, float deltaY = 0f) where T : Component
        {
            return obj.Pos(origin.Pos(), deltaX, deltaY);
        }

        /// <summary>
        /// Sets the position of a component relative to another component.
        /// </summary>
        public static T Pos<T>(this T obj, Component origin, float deltaX = 0f, float deltaY = 0f) where T : Component
        {
            return obj.Pos(origin.gameObject, deltaX, deltaY);
        }

        /// <summary>
        /// Sets the absolute position of a component.
        /// </summary>
        public static T Pos<T>(this T obj, float deltaX, float deltaY) where T : Component
        {
            var rect = obj.gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(deltaX, deltaY);
            return obj;
        }

        /// <summary>
        /// Sets the size of a component and all its children.
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="obj">The component</param>
        /// <param name="scaleX">Width (default: 0)</param>
        /// <param name="scaleY">Height (default: 0)</param>
        /// <returns>The resized component</returns>
        public static T Size<T>(this T obj, float scaleX = 0f, float scaleY = 0f) where T : Component
        {
            Parallel.ForEach(obj.GetComponentsInChildren<RectTransform>(), s => s.sizeDelta = new Vector2(scaleX, scaleY));
            return obj;
        }

        /// <summary>
        /// Sets the size of an InputField.
        /// </summary>
        public static InputField Size(this InputField obj, float scaleX = 0f, float scaleY = 0f)
        {
            var r = obj.GetComponentInChildren<RectTransform>();
            r.sizeDelta = new Vector2(scaleX, scaleY);
            return obj;
        }

        /// <summary>
        /// Sets the size of an Image.
        /// </summary>
        public static Image Size(this Image obj, float scaleX = 0f, float scaleY = 0f)
        {
            obj.rectTransform.sizeDelta = new Vector2(scaleX, scaleY);
            return obj;
        }

        /// <summary>
        /// Sets the size of a Toggle.
        /// </summary>
        public static Toggle Size(this Toggle obj, float scaleX = 0f, float scaleY = 0f)
        {
            var r = obj.GetComponentInChildren<RectTransform>();
            r.sizeDelta = new Vector2(scaleX, scaleY);
            return obj;
        }

        /// <summary>
        /// Adds to the current size of a component and all its children.
        /// </summary>
        public static T AddSize<T>(this T obj, float scaleX = 0f, float scaleY = 0f) where T : Component
        {
            Parallel.ForEach(obj.GetComponentsInChildren<RectTransform>(), s => s.sizeDelta = new Vector2(s.sizeDelta.x + scaleX, s.sizeDelta.y + scaleY));
            return obj;
        }

        /// <summary>
        /// Adds to the current size of an InputField.
        /// </summary>
        /// <param name="obj">The InputField</param>
        /// <param name="scaleX">Width to add (default: 0)</param>
        /// <param name="scaleY">Height to add (default: 0)</param>
        /// <returns>The resized InputField</returns>
        public static InputField AddSize(this InputField obj, float scaleX = 0f, float scaleY = 0f)
        {
            var r = obj.GetComponentInChildren<RectTransform>();
            r.sizeDelta = new Vector2(r.sizeDelta.x + scaleX, r.sizeDelta.y + scaleY);
            return obj;
        }

        /// <summary>
        /// Adds to the current size of an Image.
        /// </summary>
        /// <param name="obj">The Image</param>
        /// <param name="scaleX">Width to add (default: 0)</param>
        /// <param name="scaleY">Height to add (default: 0)</param>
        /// <returns>The resized Image</returns>
        public static Image AddSize(this Image obj, float scaleX = 0f, float scaleY = 0f)
        {
            obj.rectTransform.sizeDelta = new Vector2(obj.rectTransform.sizeDelta.x + scaleX, obj.rectTransform.sizeDelta.y + scaleY);
            return obj;
        }

        /// <summary>
        /// Adds to the current size of a Toggle.
        /// </summary>
        /// <param name="obj">The Toggle</param>
        /// <param name="scaleX">Width to add (default: 0)</param>
        /// <param name="scaleY">Height to add (default: 0)</param>
        /// <returns>The resized Toggle</returns>
        public static Toggle AddSize(this Toggle obj, float scaleX = 0f, float scaleY = 0f)
        {
            var r = obj.GetComponentInChildren<RectTransform>();
            r.sizeDelta = new Vector2(r.sizeDelta.x + scaleX, r.sizeDelta.y + scaleY);
            return obj;
        }

        /// <summary>
        /// Gets the size of a MonoBehaviour's RectTransform.
        /// </summary>
        /// <param name="obj">The MonoBehaviour</param>
        /// <returns>The size delta, or Vector2.zero if no RectTransform</returns>
        public static Vector2 GetSize(this MonoBehaviour obj)
        {
            var rect = obj.GetComponentInChildren<RectTransform>();
            if (rect != null)
                return rect.sizeDelta;
            return Vector2.zero;
        }

        /// <summary>
        /// Gets the size of an Image.
        /// </summary>
        public static Vector2 GetSize(this Image obj)
        {
            return obj.rectTransform.sizeDelta;
        }

        /// <summary>
        /// Sets the text content of a Text component.
        /// </summary>
        public static Text Set(this Text obj, string def)
        {
            obj.text = def;
            return obj;
        }

        /// <summary>
        /// Sets the range and value of a Slider.
        /// </summary>
        public static Slider Set(this Slider obj, float min, float max, float def)
        {
            obj.minValue = min;
            obj.maxValue = max;
            obj.value = def.FixValue(min, max);
            return obj;
        }

        /// <summary>
        /// Sets the state and text of a Toggle.
        /// </summary>
        public static Toggle Set(this Toggle obj, bool def, string defText = null)
        {
            obj.isOn = def;
            var txt = obj.GetComponentInChildren<Text>();
            if (txt != null)
                txt.text = defText;
            return obj;
        }

        /// <summary>
        /// Sets the text of a Button.
        /// </summary>
        public static Button Set(this Button obj, string def)
        {
            var txt = obj.GetComponentInChildren<Text>();
            if (txt != null)
                txt.text = def;
            return obj;
        }

        /// <summary>
        /// Sets the placeholder text of an InputField.
        /// </summary>
        public static InputField SetPlaceHolder(this InputField obj, string def)
        {
            var txt = obj.placeholder.GetComponentInChildren<Text>();
            if (txt != null)
                txt.text = def;
            return obj;
        }
    }
}