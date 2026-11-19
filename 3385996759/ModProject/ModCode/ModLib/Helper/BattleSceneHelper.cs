using ModLib.Attributes;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModLib.Helper
{
    /// <summary>
    /// Helpers for the battle scene: the map, the camera, and what is shown (units, HUD, weather, the player's view, the
    /// mouse). The Hide* methods return the objects they turned off (null / empty when nothing): give them back to
    /// <see cref="ShowAgain"/> to restore the scene.
    /// </summary>
    [ActionCat("Battle")]
    public static class BattleSceneHelper
    {
        /// <summary>The battle scene (null outside a battle).</summary>
        public static SceneBattle Scene => ModBattleEvent.SceneBattle;

        /// <summary>The battle camera controller (follows a target, keeps the camera inside the map, sets its size).</summary>
        public static CameraCtrl CameraCtrl => Scene?.cameraCtrl;

        #region Map

        /// <summary>Bounds of the battle map: the union of the floor's renderers (null when there is none).</summary>
        public static Bounds? GetMapBounds()
        {
            var floor = Scene?.battleMap?.floorRoot;
            if (floor == null) return null;
            Bounds? all = null;
            foreach (var r in floor.GetComponentsInChildren<Renderer>())
            {
                if (r == null || !r.enabled) continue;
                if (all == null) all = r.bounds;
                else { var b = all.Value; b.Encapsulate(r.bounds); all = b; }
            }
            return all;
        }

        /// <summary>Middle of the battle map (null when the map has no floor).</summary>
        public static Vector2? GetMapCentre()
        {
            var b = GetMapBounds();
            return b.HasValue ? (Vector2?)new Vector2(b.Value.center.x, b.Value.center.y) : null;
        }

        /// <summary>Moves a unit at once (its transform and its body: no slide back), keeping its depth (z).</summary>
        public static void MoveUnit(this UnitCtrlBase unit, Vector2 pos)
        {
            if (unit == null) return;
            var t = unit.transform;
            var to = new Vector3(pos.x, pos.y, t.position.z);
            t.position = to;
            var rb = t.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.position = to; rb.velocity = Vector2.zero; }
        }

        #endregion

        #region Camera

        /// <summary>
        /// The camera placed by hand: its controller off (else it follows its target, stays inside the map and takes its
        /// size back), centred on <paramref name="centre"/> with <paramref name="orthoSize"/> (half the height in world units).
        /// <see cref="ReleaseCamera"/> gives it back to the game.
        /// </summary>
        public static void PlaceCamera(Vector2 centre, float orthoSize)
        {
            var ctrl = CameraCtrl;
            if (ctrl == null) return;
            ctrl.enabled = false;
            var cam = ctrl.ca;
            cam.orthographicSize = orthoSize;
            cam.transform.position = new Vector3(centre.x, centre.y, cam.transform.position.z);
        }

        /// <summary>
        /// The camera shakes of the game off (the impacts of skills shake it with a tween, controller or not): the game
        /// setting off and a running shake stopped. Returns the setting before (for <see cref="SetCameraShake"/>).
        /// </summary>
        public static bool StopCameraShake()
        {
            var before = true;
            try
            {
                var gs = g.data.globle.gameSetting;
                before = gs.cameraShake;
                gs.cameraShake = false;
            }
            catch { }
            try { CameraCtrl?.StopShake(); } catch { }
            return before;
        }

        /// <summary>The camera shake setting of the game (on / off).</summary>
        public static void SetCameraShake(bool on)
        {
            try { g.data.globle.gameSetting.cameraShake = on; } catch { }
        }

        private static Vector2? _holdCentre;
        private static float _holdSize;
        private static bool _holdHooked;

        /// <summary>
        /// The camera held at <paramref name="centre"/> / <paramref name="orthoSize"/> every frame (LateUpdate: over the
        /// shakes and camera moves the effects of the game make), until <see cref="ReleaseCamera"/> or the battle ends.
        /// </summary>
        public static void HoldCamera(Vector2 centre, float orthoSize)
        {
            _holdCentre = centre;
            _holdSize = orthoSize;
            if (!_holdHooked)
            {
                // LateUpdate, and again just before the camera culls (after every Update / LateUpdate / tween of the
                // frame: the effects of the game move the camera in their own LateUpdate, in any order)
                ModLib.Object.MonoUpdater.LateUpdates += HoldTick;
                var cb = UnhollowerRuntimeLib.DelegateSupport.ConvertDelegate<Camera.CameraCallback>(new System.Action<Camera>(HoldPreCull));
                Camera.onPreCull = Camera.onPreCull == null ? cb : Il2CppSystem.Delegate.Combine(Camera.onPreCull, cb).Cast<Camera.CameraCallback>();
                _holdHooked = true;
            }
            PlaceCamera(centre, orthoSize);
        }

        private static void HoldPreCull(Camera cam)
        {
            if (_holdCentre == null || cam == null) return;
            var main = CameraCtrl?.ca;
            if (main == null || main.Pointer != cam.Pointer) return;
            cam.orthographicSize = _holdSize;
            cam.transform.position = new Vector3(_holdCentre.Value.x, _holdCentre.Value.y, cam.transform.position.z);
        }

        private static void HoldTick()
        {
            if (_holdCentre == null) return;
            var ctrl = CameraCtrl;
            if (ctrl == null) { _holdCentre = null; return; }   // the battle is over
            ctrl.enabled = false;
            var cam = ctrl.ca;
            cam.orthographicSize = _holdSize;
            cam.transform.position = new Vector3(_holdCentre.Value.x, _holdCentre.Value.y, cam.transform.position.z);
        }

        /// <summary>The camera back to the game: its controller on, following <paramref name="target"/> (default: the player).</summary>
        public static void ReleaseCamera(Transform target = null)
        {
            _holdCentre = null;
            var ctrl = CameraCtrl;
            if (ctrl == null) return;
            ctrl.enabled = true;
            target = target ?? ModBattleEvent.PlayerUnit?.transform;
            if (target != null) ctrl.SetTarget(target);
        }

        /// <summary>
        /// A camera view kept inside the battle map (no ground outside it on screen; the game's camera controller would
        /// also pull the camera back in): the size at most what the map holds, the centre moved in from the edges.
        /// </summary>
        public static void FitInMap(ref Vector2 centre, ref float orthoSize)
        {
            var b = GetMapBounds();
            if (b == null) return;
            var aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
            var max = Mathf.Min(b.Value.extents.y, b.Value.extents.x / aspect);
            if (max > 0f) orthoSize = Mathf.Min(orthoSize, max);
            var halfW = orthoSize * aspect;
            centre.x = Mathf.Clamp(centre.x, b.Value.min.x + halfW, b.Value.max.x - halfW);
            centre.y = Mathf.Clamp(centre.y, b.Value.min.y + orthoSize, b.Value.max.y - orthoSize);
        }

        /// <summary>Camera size (orthographic) that shows an area of <paramref name="size"/> with a margin.</summary>
        public static float GetOrthoSizeFor(Vector2 size, float margin = 1.3f, float min = 1.2f, float max = 8f)
        {
            var aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
            return Mathf.Clamp(Mathf.Max(size.y / 2f, size.x / 2f / aspect) * margin, min, max);
        }

        #endregion

        #region Hide / show

        /// <summary>
        /// The mouse out of the picture: the cursor hidden, and the effects following it off (the player's aim:
        /// EffectTraceMousePosiAngleCtrl, only on objects under the battle's effect root - the same component on a unit or
        /// a scene object is left alone: turning its object off stops the battle; MouseVisibilityController is on the
        /// game's root object "Game", with its timers and input). The game may make the effects again:
        /// call it again when needed.
        /// </summary>
        public static List<GameObject> HideMouse()
        {
            Cursor.visible = false;
            var rs = new List<GameObject>();
            var root = Scene?.effect?.effectRoot?.transform;
            if (root == null) return rs;
            foreach (var go in UnityEngine.Object.FindObjectsOfType<EffectTraceMousePosiAngleCtrl>().Select(x => x.gameObject))
            {
                if (go == null || !go.activeSelf || !go.transform.IsChildOf(root)) continue;
                go.SetActive(false);
                rs.Add(go);
            }
            return rs;
        }

        /// <summary>The battle HUD (BattleInfo) hidden.</summary>
        public static GameObject HideHud()
        {
            return Hide(g.ui.GetUI(UIType.BattleInfo)?.gameObject);
        }

        /// <summary>The weather of the battle (rain, night...) off.</summary>
        public static GameObject HideWeather()
        {
            return Hide(Scene?.battleMap?.goWeatherEffect);
        }

        /// <summary>
        /// The player's view off: the dark cover with a hole around the player (PlayerMask), the whole map lit.
        /// </summary>
        public static GameObject HidePlayerView()
        {
            return Hide(ModBattleEvent.PlayerUnit?.transform.Find("PlayerMask")?.gameObject);
        }

        /// <summary>The picture of a unit hidden (its body, clothes and shadow: "Root"); the unit stays in the battle.</summary>
        public static GameObject HideUnitBody(this UnitCtrlBase unit)
        {
            return Hide(unit?.transform.Find("Root")?.gameObject);
        }

        /// <summary>
        /// A unit out of the battle for a while: its AI stopped, moved <paramref name="moveAway"/> away (the effects that
        /// follow it too), then hidden. <see cref="ShowAgain"/> shows it and starts its AI again (it stays where it is).
        /// </summary>
        public static GameObject StopAndHideUnit(this UnitCtrlBase unit, float moveAway = 500f)
        {
            if (unit == null || !unit.gameObject.activeSelf) return null;
            try { unit.TryCast<UnitCtrlHumanNPC>()?.StopAI(true); } catch { }
            unit.transform.position = unit.transform.position + new Vector3(moveAway, moveAway, 0f);
            return Hide(unit.gameObject);
        }

        /// <summary>Every unit of the battle but the player: <see cref="StopAndHideUnit"/>.</summary>
        public static List<GameObject> StopAndHideOtherUnits(float moveAway = 500f)
        {
            var player = ModBattleEvent.PlayerUnit?.Pointer ?? System.IntPtr.Zero;
            return ModBattleEvent.BattleUnits.ToArray().Where(u => u != null && u.Pointer != player)
                .Select(u => u.StopAndHideUnit(moveAway)).Where(x => x != null).ToList();
        }

        /// <summary>Objects turned off by the Hide* methods shown again (the AI of hidden units started again); the cursor shown.</summary>
        public static void ShowAgain(IEnumerable<GameObject> hidden)
        {
            Cursor.visible = true;
            if (hidden == null) return;
            foreach (var go in hidden)
            {
                if (go == null) continue;
                go.SetActive(true);
                try { go.GetComponent<UnitCtrlHumanNPC>()?.StopAI(false); } catch { }
            }
        }

        private static GameObject Hide(GameObject go)
        {
            if (go == null || !go.activeSelf) return null;
            go.SetActive(false);
            return go;
        }

        #endregion

        #region Diagnostic

        /// <summary>
        /// The hierarchy under <paramref name="t"/> as text, one object a line: name, "(off)" when inactive, its
        /// components, and the sprite / material and size of its renderer (to find what draws something on screen).
        /// </summary>
        public static string DumpHierarchy(Transform t, int maxDepth = 4)
        {
            var sb = new StringBuilder();
            Dump(t, 0, maxDepth, sb);
            return sb.ToString();
        }

        private static void Dump(Transform t, int depth, int maxDepth, StringBuilder sb)
        {
            if (t == null || depth > maxDepth) return;
            var comps = string.Join("+", t.GetComponents<Component>().Select(x => x.GetIl2CppType().Name).Where(x => x != "Transform"));
            var extra = "";
            var sr = t.GetComponent<SpriteRenderer>();
            var r = t.GetComponent<Renderer>();
            if (sr != null) extra = $" [{sr.sprite?.name} {sr.bounds.size.x:0.#}x{sr.bounds.size.y:0.#}]";
            else if (r != null) extra = $" [{r.sharedMaterial?.name} {r.bounds.size.x:0.#}x{r.bounds.size.y:0.#}]";
            sb.AppendLine($"{new string(' ', depth * 2)}{t.name}{(t.gameObject.activeSelf ? "" : " (off)")} {{{comps}}}{extra}");
            for (var i = 0; i < t.childCount; i++) Dump(t.GetChild(i), depth + 1, maxDepth, sb);
        }

        #endregion
    }
}
