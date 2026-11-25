using ModLib.Attributes;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ModLib.Helper
{
    /// <summary>
    /// Helpers for the visual effects of a battle (prefabs "Effect\Battle\Skill\...", "Effect\Battle\Unit\...",
    /// "Effect\Battle\Scenes\..."): create one, what it shows (bounds, timing), the list of the game's effects.
    /// </summary>
    [ActionCat("Effect")]
    public static class BattleEffectHelper
    {
        /// <summary>
        /// Creates an effect at <paramref name="pos"/> for <paramref name="lifeSec"/> seconds. <paramref name="disarm"/>:
        /// its unit commands removed (<see cref="Disarm"/>) - needed for an effect created alone, outside a skill.
        /// </summary>
        public static void CreateEffect(string path, Vector3 pos, float lifeSec, Action<GameObject> onCreated = null, bool disarm = false)
        {
            var scene = ModBattleEvent.SceneBattle;
            if (scene == null) return;
            scene.effect.Create(path, pos, lifeSec, ActionHelper.TracedIl2Action<GameObject>(go =>
            {
                if (disarm)
                {
                    Disarm(go);
                    if (RemoveUnitParts(go, true) > 0) NoteUnitEffect(path);
                }
                onCreated?.Invoke(go);
            }), true, true, true);
        }

        /// <summary>
        /// The unit commands of an effect removed (Unit*Ctrl components: UnitCmdCtrl...). A skill gives them their unit;
        /// on an effect created alone they throw (UnitCmdCtrl.TargetUnit) inside the game's TimerMgr, which then stops
        /// every timer of the game for good. A UnityEvent does not call a destroyed component.
        /// </summary>
        public static void Disarm(GameObject go)
        {
            if (go == null) return;
            foreach (var comp in go.GetComponentsInChildren<Component>(true))
            {
                if (comp == null) continue;
                var name = comp.GetIl2CppType().Name;
                if (name.StartsWith("Unit") && name.EndsWith("Ctrl"))
                    UnityEngine.Object.DestroyImmediate(comp);
            }
        }

        /// <summary>
        /// The parts of a unit in an effect taken out: its UnitCtrlBaseMono removed, its body (Rigidbody2D, colliders) turned
        /// off. Some effects of the game are a unit of a skill (heluoyuciji: a tentacle of a boss); created alone, their unit
        /// mono has no unit and the effect commands (EffectCmdBase.InitUnitMono) throw inside the game's TimerMgr, which then
        /// stops every timer of the game; their body would push the units around (destroyed, the effect commands throw on
        /// the colliders they list: turned off instead). Matched by type (base types included). Returns the parts taken out
        /// (<paramref name="monoOnly"/>: the unit monos only - an effect that has one is a unit of a skill).
        /// </summary>
        public static int RemoveUnitParts(GameObject go, bool monoOnly = false)
        {
            if (go == null) return 0;
            var n = 0;
            foreach (var comp in go.GetComponentsInChildren<Component>(true))
            {
                if (comp == null) continue;
                try
                {
                    if (comp.TryCast<UnitCtrlBaseMono>() != null) { UnityEngine.Object.DestroyImmediate(comp); n++; }
                    else if (comp.TryCast<Collider2D>() is Collider2D col) { col.enabled = false; if (!monoOnly) n++; }
                    else if (comp.TryCast<Rigidbody2D>() is Rigidbody2D body) { body.simulated = false; if (!monoOnly) n++; }
                }
                catch (Exception ex) { DebugHelper.WriteLine($"BattleEffectHelper: effect part: {ex.Message}"); }
            }
            return n;
        }

        /// <summary>The effects found to be a unit of a skill this session (paths "Effect\Battle\Skill\x"): not to be created alone again.</summary>
        public static HashSet<string> UnitEffects { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static string NormEffectPath(string path) => path?.Replace('/', '\\').Trim();

        /// <summary>
        /// An effect noted as a unit of a skill (UnitEffects, logged); true the first time (the caller may report it, e.g.
        /// CrashReportHelper.Report from its own code, so that the report goes to its mod).
        /// </summary>
        public static bool NoteUnitEffect(string path)
        {
            if (string.IsNullOrEmpty(path) || !UnitEffects.Add(NormEffectPath(path))) return false;
            DebugHelper.WriteLine($"BattleEffectHelper: the effect {path} is a unit of a skill (UnitCtrlBaseMono): created alone it breaks the battle");
            return true;
        }

        /// <summary>An effect noted as a unit of a skill (NoteUnitEffect).</summary>
        public static bool IsUnitEffect(string path) => !string.IsNullOrEmpty(path) && UnitEffects.Contains(NormEffectPath(path));

        /// <summary>
        /// The camera controls of an effect removed (components moving the battle camera: EffectTargetCameraPosiCtrl,
        /// EffectTargetCameraEdgeCtrl, EffectFootStepCameraCtrl...): the camera stays where it is placed.
        /// </summary>
        public static void RemoveCameraControls(GameObject go)
        {
            if (go == null) return;
            foreach (var comp in go.GetComponentsInChildren<Component>(true))
            {
                if (comp == null) continue;
                var name = comp.GetIl2CppType().Name;
                if (name.StartsWith("Effect") && name.Contains("Camera"))
                    UnityEngine.Object.DestroyImmediate(comp);
            }
        }

        /// <summary>An effect removed.</summary>
        public static void DestroyEffect(GameObject go)
        {
            if (go == null) return;
            try { ModBattleEvent.SceneBattle?.effect?.DestroyEffect(go); } catch { }
        }

        /// <summary>Every effect of the battle removed (buffs and shields of the units too).</summary>
        public static void RemoveAllEffects()
        {
            try { ModBattleEvent.SceneBattle?.effect?.RemoveAll(); } catch { }
        }

        /// <summary>
        /// What the effect shows now: the union of the bounds of its visible renderers (null: nothing shows). An effect
        /// with particle systems shows while particles live (their renderers stay enabled); without, while its renderers
        /// are enabled. Parts bigger than <paramref name="maxPartSize"/> (screen-wide tints) are left out.
        /// </summary>
        public static Bounds? GetVisibleBounds(GameObject go, float maxPartSize = float.MaxValue)
        {
            if (go == null || !go.activeInHierarchy) return null;
            var hasParticles = go.GetComponentsInChildren<ParticleSystem>().Length > 0;
            Bounds? all = null;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
                var ps = r.GetComponent<ParticleSystem>();
                if (hasParticles && (ps == null || ps.particleCount == 0)) continue;
                var b = r.bounds;
                if (b.size.x > maxPartSize || b.size.y > maxPartSize || b.size.sqrMagnitude <= 0f) continue;
                if (all == null) all = b;
                else { var u = all.Value; u.Encapsulate(b); all = u; }
            }
            return all;
        }

        /// <summary>Component types of an effect and its children, counted ("ParticleSystem x4, ...").</summary>
        public static string GetComponentSummary(GameObject go)
        {
            if (go == null) return "";
            return string.Join(", ", go.GetComponentsInChildren<Component>(true).Select(x => x.GetIl2CppType().Name)
                .GroupBy(x => x).Select(x => $"{x.Key} x{x.Count()}"));
        }

        /// <summary>
        /// The effects of a resource folder ("Effect/Battle/Skill"), as paths for <see cref="CreateEffect"/>
        /// ("Effect\Battle\Skill\name"), sorted: the prefabs in Resources and in the loaded asset bundles. Loads every
        /// prefab of the folder: slow, once.
        /// </summary>
        public static List<string> ListEffects(string folder)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var o in Resources.LoadAll(folder, UnhollowerRuntimeLib.Il2CppType.Of<GameObject>()))
                if (o != null) names.Add(o.name);
            var key = folder.ToLowerInvariant() + "/";
            foreach (var ab in g.res.allAB)
            {
                if (ab.Value == null) continue;
                foreach (var n in ab.Value.GetAllAssetNames())
                {
                    var i = n.IndexOf(key, StringComparison.Ordinal);
                    if (i < 0) continue;
                    var rest = n.Substring(i + key.Length);
                    if (!rest.Contains("/")) names.Add(Path.GetFileNameWithoutExtension(rest));
                }
            }
            var prefix = folder.Replace('/', '\\') + "\\";
            return names.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).Select(x => prefix + x).ToList();
        }
    }
}
