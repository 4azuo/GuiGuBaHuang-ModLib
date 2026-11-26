using ModLib.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModLib.Object
{
    /// <summary>
    /// Visual effects of the battle played outside a battle, in a window: prefabs loaded with g.res.Load, placed far away on
    /// their own layer and filmed by a camera of their own into a RenderTexture, shown by a framed RawImage, on a background
    /// picture. The scripts of an effect are removed before they wake up (they need a battle: units, battle camera, sort
    /// order); the parts its EffectCmdCtrl shows one after the other (DelayActiveSubobject "child|delay|duration") are timed
    /// here. Show plays one effect (again when it ends: Tick); Spawn puts any number of them on the stage, for a moving scene
    /// (a subclass overrides Update). Update(dt) every frame, Dispose when the window closes.
    /// </summary>
    public class EffectStage : IDisposable
    {
        /// <summary>Layers of the filmed effects and of the background (not used by the game's cameras).</summary>
        public const int LAYER = 31;
        public const int BG_LAYER = 30;
        public const int TEXTURE_SIZE = 512;
        /// <summary>Far from the map and the UI.</summary>
        public static readonly Vector3 ORIGIN = new Vector3(-30000f, -30000f, 0f);
        /// <summary>An effect that ends by itself is played again this long (ms) after its duration.</summary>
        public const int REPLAY_GAP_MS = 700;
        /// <summary>Frame around the picture (px).</summary>
        public const float FRAME = 7f;

        private GameObject _root;
        /// <summary>Inactive parent: effects are made there, so their scripts do not wake up before they are removed.</summary>
        private GameObject _holder;
        protected Camera _camera;
        private RenderTexture _texture;
        private GameObject _effect;
        private GameObject _background;
        private GameObject _frame;
        private readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        private readonly List<KeyValuePair<Text, float>> _numbers = new List<KeyValuePair<Text, float>>();
        private readonly List<Part> _parts = new List<Part>();
        protected float _size;

        /// <summary>A part of an effect shown from Start to End (seconds after the effect).</summary>
        private class Part
        {
            public GameObject Go, Of;
            public float Start, End, Time;
            public bool On;
        }

        /// <summary>The picture in the window.</summary>
        public RawImage Image { get; private set; }
        /// <summary>The effect of Show: its path, how long it lasts (ms, 0: unknown), whether it loops.</summary>
        public string CurrentPath { get; private set; }
        public int CurrentMs { get; private set; }
        public bool CurrentLoop { get; private set; }
        public bool IsDisposed { get; private set; }
        /// <summary>When the current effect started.</summary>
        public DateTime StartedAt { get; private set; }
        public bool HasBackground => _background != null;
        /// <summary>Timed parts made / shown since the stage was made (tests).</summary>
        public int TimedParts { get; private set; }
        public int TimedShown { get; private set; }

        /// <summary>A stage of <paramref name="size"/> pixels at <paramref name="pos"/> (anchored position) in <paramref name="parent"/>; no background when null.</summary>
        public EffectStage(Transform parent, Vector2 pos, float size, Sprite background = null)
        {
            _texture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 16);
            _texture.Create();

            _root = new GameObject("ModLib_EffectStage");
            _root.transform.position = ORIGIN;
            _holder = new GameObject("ModLib_EffectStageHolder");
            _holder.transform.SetParent(_root.transform, false);
            _holder.SetActive(false);

            var camObj = new GameObject("ModLib_EffectStageCamera");
            camObj.transform.SetParent(_root.transform, false);
            camObj.transform.localPosition = new Vector3(0f, 0f, -50f);
            _camera = camObj.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 3f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.06f, 0.06f, 0.09f, 1f);
            _camera.cullingMask = (1 << LAYER) | (1 << BG_LAYER);
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 200f;
            _camera.depth = -100f;
            _camera.targetTexture = _texture;

            if (background != null) MakeBackground(background);

            // the frame, then the picture on it
            _frame = new GameObject("ModLib_EffectStageFrame");
            _frame.transform.SetParent(parent, false);
            var fimg = _frame.AddComponent<UnityEngine.UI.Image>();
            fimg.color = new Color(0.27f, 0.18f, 0.10f, 1f);
            fimg.raycastTarget = false;
            var inner = new GameObject("ModLib_EffectStageFrameInner");
            inner.transform.SetParent(_frame.transform, false);
            var iimg = inner.AddComponent<UnityEngine.UI.Image>();
            iimg.color = new Color(0.78f, 0.66f, 0.42f, 1f);
            iimg.raycastTarget = false;

            var imgObj = new GameObject("ModLib_EffectStageImage");
            imgObj.transform.SetParent(parent, false);
            Image = imgObj.AddComponent<RawImage>();
            Image.texture = _texture;
            Image.raycastTarget = false;
            Place(pos, size);
        }

        private void MakeBackground(Sprite sprite)
        {
            try
            {
                _background = new GameObject("ModLib_EffectStageBackground");
                _background.transform.SetParent(_root.transform, false);
                _background.transform.localPosition = new Vector3(0f, 0f, 20f);
                var sr = _background.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = new Color(0.62f, 0.6f, 0.56f, 1f);
                sr.sortingOrder = -1000;
                _background.layer = BG_LAYER;
            }
            catch (Exception ex) { DebugHelper.WriteLine($"EffectStage: background: {ex.Message}"); }
        }

        /// <summary>The background covers the view of the camera.</summary>
        private void FitBackground()
        {
            var sr = _background?.GetComponent<SpriteRenderer>();
            if (sr?.sprite == null) return;
            var h = sr.sprite.bounds.size.y;
            if (h <= 0) return;
            var s = 2f * _camera.orthographicSize / h * 1.02f;
            _background.transform.localScale = new Vector3(s, s, 1f);
        }

        /// <summary>The view of the camera (half its height, world units); the background follows.</summary>
        public void SetView(float halfHeight)
        {
            _camera.orthographicSize = halfHeight;
            FitBackground();
        }

        /// <summary>Moves / resizes the picture.</summary>
        public virtual void Place(Vector2 pos, float size)
        {
            if (Image == null) return;
            _size = size;
            var rect = Image.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = pos;
            var frt = _frame.GetComponent<RectTransform>();
            frt.sizeDelta = new Vector2(size + FRAME * 2, size + FRAME * 2);
            frt.anchoredPosition = pos;
            var inner = _frame.transform.GetChild(0).GetComponent<RectTransform>();
            inner.sizeDelta = new Vector2(size + 3f, size + 3f);
        }

        /// <summary>Local position of the scene -> point of the picture (from its bottom-left corner).</summary>
        public Vector2 ToPicture(Vector2 local)
        {
            var view = 2f * _camera.orthographicSize;
            return new Vector2((local.x / view + 0.5f) * _size, (local.y / view + 0.5f) * _size);
        }

        // ------------------------------------------------------------------ show one effect

        /// <summary>
        /// Plays an effect (again from the start), the view fitted to its size (world units, 0: unknown); false when its prefab
        /// cannot be loaded.
        /// </summary>
        public virtual bool Show(string path, float extent = 0f, int durationMs = 0, bool loop = false)
        {
            Clear();
            if (IsDisposed || string.IsNullOrEmpty(path)) return false;
            CurrentPath = path;
            CurrentMs = durationMs;
            CurrentLoop = loop;
            StartedAt = DateTime.Now;
            // the measured size of the effect fills ~80% of the picture
            SetView(extent > 0f ? Mathf.Clamp(extent * 0.65f, 1f, 12f) : 3f);
            _effect = Spawn(path, Vector2.zero);
            return _effect != null;
        }

        /// <summary>Plays the current effect again.</summary>
        public virtual bool Replay() => Show(CurrentPath, _camera.orthographicSize / 0.65f, CurrentMs, CurrentLoop);

        /// <summary>The effect of Show removed.</summary>
        public void Clear()
        {
            if (_effect != null) Remove(_effect);
            _effect = null;
            CurrentPath = null;
        }

        /// <summary>Plays again an effect of Show that ends by itself once it is over (called by the window every 100 ms).</summary>
        public virtual void Tick()
        {
            if (IsDisposed || CurrentPath == null || CurrentLoop) return;
            var length = Math.Max(1200, (CurrentMs > 0 ? CurrentMs : 2000) + REPLAY_GAP_MS);
            if ((DateTime.Now - StartedAt).TotalMilliseconds > length) Replay();
        }

        // ------------------------------------------------------------------ effects

        /// <summary>
        /// An effect made on the stage at a local position: scripts removed, its parts timed as its commands say (the others
        /// shown at once), on the layer. Null: no prefab.
        /// </summary>
        public GameObject Spawn(string path, Vector2 pos, float scale = 1f, float angleDeg = 0f)
        {
            if (IsDisposed || string.IsNullOrEmpty(path)) return null;
            if (!_prefabs.TryGetValue(path, out var prefab))
                _prefabs[path] = prefab = Load(path);
            if (prefab == null)
            {
                DebugHelper.WriteLine($"EffectStage: no prefab {path}");
                return null;
            }
            var go = UnityEngine.Object.Instantiate(prefab, _holder.transform, false);
            var parts = ReadParts(go);
            RemoveScripts(go);
            // the parts no command times (other scripts would have shown them) are shown at once
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            foreach (var p in parts)
            {
                p.On = p.Start <= 0f;
                p.Go.SetActive(p.On);
                if (p.On) TimedShown++;
            }
            _parts.AddRange(parts);
            TimedParts += parts.Count;
            SetLayer(go, LAYER);
            go.transform.SetParent(_root.transform, false);
            go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            if (Math.Abs(scale - 1f) > 0.01f) go.transform.localScale = go.transform.localScale * scale;
            if (Math.Abs(angleDeg) > 0.01f) go.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
            go.SetActive(true);
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(false))
                ps.Play(false);
            return go;
        }

        /// <summary>An effect of Spawn taken off the stage.</summary>
        public void Remove(GameObject go)
        {
            if (go == null) return;
            _parts.RemoveAll(x => x.Of == null || x.Of == go);
            UnityEngine.Object.Destroy(go);
        }

        /// <summary>The commands that show a part of an effect for a while.</summary>
        private static readonly string[] PART_COMMANDS = { "DelayActiveSubobject", "DelayActiveSubobjectUntime", "DelayActiveSubObjectWithoutLifeTimeCtrl" };

        /// <summary>The timed parts of an effect (its EffectCmdBase start calls), read before its scripts are removed.</summary>
        private static List<Part> ReadParts(GameObject go)
        {
            var rs = new List<Part>();
            try
            {
                foreach (var cmd in go.GetComponentsInChildren<EffectCmdBase>(true))
                {
                    var calls = cmd?.onStartCall?.m_PersistentCalls?.m_Calls;
                    if (calls == null) continue;
                    foreach (var call in calls)
                    {
                        if (call == null || !PART_COMMANDS.Contains(call.methodName)) continue;
                        if (!ParsePart(call.arguments?.stringArgument, out var name, out var delay, out var length)) continue;
                        var child = cmd.transform.Find(name);
                        if (child == null && int.TryParse(name, out var index) && index >= 0 && index < cmd.transform.childCount) child = cmd.transform.GetChild(index);
                        if (child == null || rs.Any(x => x.Go == child.gameObject)) continue;
                        rs.Add(new Part { Go = child.gameObject, Of = go, Start = delay, End = delay + length });
                    }
                }
            }
            catch (Exception ex) { DebugHelper.WriteLine($"EffectStage: parts: {ex.Message}"); }
            return rs;
        }

        /// <summary>The argument of a part command: "child|delay|duration" (seconds; the child by name, path or index).</summary>
        public static bool ParsePart(string argument, out string child, out float delay, out float length)
        {
            child = null;
            delay = 0f;
            length = 0f;
            var x = (argument ?? "").Split('|');
            if (x.Length < 2 || string.IsNullOrWhiteSpace(x[0])) return false;
            child = x[0].Trim();
            if (!float.TryParse(x[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out delay)) return false;
            if (x.Length < 3 || !float.TryParse(x[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out length)) length = 999f;
            return true;
        }

        /// <summary>Every frame: the parts of the effects, the numbers.</summary>
        public virtual void Update(float dt)
        {
            if (IsDisposed) return;
            for (var i = _parts.Count - 1; i >= 0; i--)
            {
                var p = _parts[i];
                if (p.Go == null || p.Of == null) { _parts.RemoveAt(i); continue; }
                p.Time += dt;
                var on = p.Time >= p.Start && p.Time < p.End;
                if (on == p.On) continue;
                p.On = on;
                p.Go.SetActive(on);
                if (!on) continue;
                TimedShown++;
                foreach (var ps in p.Go.GetComponentsInChildren<ParticleSystem>(false)) ps.Play(false);
            }
            // hit numbers: rise 45 px and fade in 1 s
            for (var i = _numbers.Count - 1; i >= 0; i--)
            {
                var n = _numbers[i];
                var age = n.Value + dt;
                if (n.Key == null || age >= 1f)
                {
                    if (n.Key != null) UnityEngine.Object.Destroy(n.Key.gameObject);
                    _numbers.RemoveAt(i);
                    continue;
                }
                n.Key.rectTransform.anchoredPosition += new Vector2(0f, 45f * dt);
                var col = n.Key.color;
                n.Key.color = new Color(col.r, col.g, col.b, age < 0.6f ? 1f : 1f - (age - 0.6f) / 0.4f);
                _numbers[i] = new KeyValuePair<Text, float>(n.Key, age);
            }
        }

        // ------------------------------------------------------------------ numbers

        /// <summary>A text rising over a point of the scene and fading (1 s).</summary>
        protected void Rise(string text, Vector2 at, Color color, int size)
        {
            try
            {
                var go = new GameObject("ModLib_StageNumber");
                go.transform.SetParent(Image.transform, false);
                var txt = go.AddComponent<Text>();
                txt.font = UISampleHelper.TextSample?.font;
                txt.fontSize = size;
                txt.fontStyle = FontStyle.Bold;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = color;
                txt.text = text;
                txt.raycastTarget = false;
                var outline = go.AddComponent<Outline>();
                outline.effectColor = new Color(0.35f, 0.05f, 0.02f, 1f);
                outline.effectDistance = new Vector2(1.5f, -1.5f);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0f);
                rt.sizeDelta = new Vector2(220f, 32f);
                var t = ToPicture(at);
                rt.anchoredPosition = new Vector2(t.x - 110f + UnityEngine.Random.Range(-18f, 18f), t.y + _size * 0.2f + UnityEngine.Random.Range(-6f, 6f));
                _numbers.Add(new KeyValuePair<Text, float>(txt, 0f));
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"EffectStage: number: {ex.Message}");
            }
        }

        /// <summary>The numbers shown now removed.</summary>
        public void ClearNumbers()
        {
            foreach (var n in _numbers) if (n.Key != null) UnityEngine.Object.Destroy(n.Key.gameObject);
            _numbers.Clear();
        }

        // ------------------------------------------------------------------ end, tests

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            _parts.Clear();
            Clear();
            if (Image != null) UnityEngine.Object.Destroy(Image.gameObject);
            if (_frame != null) UnityEngine.Object.Destroy(_frame);
            if (_root != null) UnityEngine.Object.Destroy(_root);
            if (_texture != null)
            {
                _texture.Release();
                UnityEngine.Object.Destroy(_texture);
            }
            Image = null;
            _frame = null;
            _root = null;
            _texture = null;
        }

        /// <summary>
        /// Pixels of the effects (tests: something is drawn): the effects alone (no background) filmed on black and read
        /// from the RenderTexture, 1 of 16 pixels.
        /// </summary>
        public int CountLitPixels()
        {
            if (IsDisposed || _texture == null) return 0;
            var prev = RenderTexture.active;
            var mask = _camera.cullingMask;
            var bg = _camera.backgroundColor;
            var tex = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGBA32, false);
            try
            {
                _camera.cullingMask = 1 << LAYER;
                _camera.backgroundColor = Color.black;
                _camera.Render();
                RenderTexture.active = _texture;
                tex.ReadPixels(new Rect(0, 0, TEXTURE_SIZE, TEXTURE_SIZE), 0, 0);
                tex.Apply();
                var count = 0;
                for (var y = 0; y < TEXTURE_SIZE; y += 4)
                    for (var x = 0; x < TEXTURE_SIZE; x += 4)
                    {
                        var p = tex.GetPixel(x, y);
                        if (p.r + p.g + p.b > 0.08f) count++;
                    }
                return count;
            }
            finally
            {
                _camera.cullingMask = mask;
                _camera.backgroundColor = bg;
                _camera.Render();
                RenderTexture.active = prev;
                UnityEngine.Object.Destroy(tex);
            }
        }

        /// <summary>What the effect of Show is made of now (tests): particle systems, renderers.</summary>
        public virtual string Diagnose()
        {
            if (_effect == null) return "no effect";
            var ps = _effect.GetComponentsInChildren<ParticleSystem>(true);
            var rs = _effect.GetComponentsInChildren<Renderer>(true);
            return $"systems {ps.Length} (playing {ps.Count(x => x.isPlaying)}, particles {ps.Sum(x => x.particleCount)}), renderers {rs.Length}, timed parts {_parts.Count}, camera {_camera.orthographicSize:0.#}";
        }

        /// <summary>The prefab of an effect ("Effect\Battle\Skill\x", also tried with '/').</summary>
        public static GameObject Load(string path)
        {
            GameObject go = null;
            try { go = g.res.Load<GameObject>(path); } catch { }
            if (go == null)
                try { go = g.res.Load<GameObject>(path.Replace('\\', '/')); } catch { }
            return go;
        }

        /// <summary>Every script (MonoBehaviour) of the effect removed: they work only in a battle.</summary>
        public static void RemoveScripts(GameObject go)
        {
            foreach (var comp in go.GetComponentsInChildren<MonoBehaviour>(true))
                if (comp != null)
                    UnityEngine.Object.DestroyImmediate(comp);
        }

        private static void SetLayer(GameObject go, int layer)
        {
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = layer;
        }
    }
}
