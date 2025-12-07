using ModLib.Attributes;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for working with divine artifacts (Potmon, God Eye, etc.).
    /// Provides utilities to check artifact states and damage status.
    /// </summary>
    [ActionCat("God Artifact")]
    public static class GodArtifactHelper
    {
        /// <summary>
        /// Checks if Potmon (Devil Demon) is damaged.
        /// </summary>
        /// <returns>True if damaged</returns>
        public static bool IsPotmonDamaged()
        {
            return g.world.devilDemonMgr.devilDemonData.isDamage;
        }

        /// <summary>
        /// Checks if God Eye is damaged.
        /// </summary>
        /// <returns>True if damaged</returns>
        public static bool IsGodEyeDamaged()
        {
            return g.data.dataWorld.data.godEyeData.isDamage;
        }

        /// <summary>
        /// Checks if Pisces Pendant is damaged (no fish jade remaining).
        /// </summary>
        /// <returns>True if damaged</returns>
        public static bool IsPiscesPendantDamaged()
        {
            return g.data.dataWorld.data.resurgeCount <= 0;
        }

        /// <summary>
        /// Repairs Potmon if damaged.
        /// </summary>
        /// <param name="repairMonth">Repair completion month</param>
        public static void RepairPotmonDamaged(int repairMonth = 0)
        {
            if (IsPotmonDamaged())
            {
                g.world.devilDemonMgr.devilDemonData.isDamage = false;
                g.world.devilDemonMgr.devilDemonData.repairMonth = repairMonth;
            }
        }

        /// <summary>
        /// Repairs God Eye if damaged.
        /// </summary>
        /// <param name="repairMonth">Repair completion month</param>
        public static void RepairGodEyeDamaged(int repairMonth = 0)
        {
            if (IsGodEyeDamaged())
            {
                g.data.dataWorld.data.godEyeData.isDamage = false;
                g.data.dataWorld.data.godEyeData.repairMonth = repairMonth;
            }
        }

        /// <summary>
        /// Adds fish jade to Pisces Pendant.
        /// </summary>
        /// <param name="add">Amount to add</param>
        public static void AddFishJade(int add)
        {
            g.data.dataWorld.data.resurgeCount += add;
        }

        /// <summary>
        /// Sets fish jade count for Pisces Pendant.
        /// </summary>
        /// <param name="add">New count</param>
        public static void SetFishJade(int add)
        {
            g.data.dataWorld.data.resurgeCount = add;
        }

        /// <summary>
        /// Gets current fish jade count.
        /// </summary>
        /// <returns>Fish jade count</returns>
        public static int GetFishJade()
        {
            return g.data.dataWorld.data.resurgeCount;
        }

        /// <summary>
        /// Sets active anima weapons.
        /// </summary>
        /// <param name="weapons">Weapons to set</param>
        public static void SetAnimaWeapon(params GameAnimaWeapon[] weapons)
        {
            g.data.dataWorld.data.animaWeapons.Clear();
            g.data.dataWorld.data.animaWeapons.AddRange(weapons.ToIl2CppList());
        }

        /// <summary>
        /// Gets list of active anima weapons.
        /// </summary>
        /// <returns>Anima weapons</returns>
        public static Il2CppSystem.Collections.Generic.List<GameAnimaWeapon> GetAnimaWeapon()
        {
            return g.data.dataWorld.data.animaWeapons;
        }

        /// <summary>
        /// Checks if Pisces Pendant is enabled.
        /// </summary>
        /// <returns>True if enabled</returns>
        public static bool IsEnabledPiscesPendant()
        {
            return g.data.dataWorld.data.animaWeapons[0] == GameAnimaWeapon.PiscesPendant;
        }

        /// <summary>
        /// Checks if Devil Demon (Potmon) is enabled.
        /// </summary>
        /// <returns>True if enabled</returns>
        public static bool IsEnabledDevilDemon()
        {
            return g.data.dataWorld.data.animaWeapons[0] == GameAnimaWeapon.DevilDemon;
        }

        /// <summary>
        /// Checks if God Eye is enabled.
        /// </summary>
        /// <returns>True if enabled</returns>
        public static bool IsEnabledGodEye()
        {
            return g.data.dataWorld.data.animaWeapons[0] == GameAnimaWeapon.HootinEye;
        }

        /// <summary>
        /// Checks if no anima weapon is enabled.
        /// </summary>
        /// <returns>True if disabled</returns>
        public static bool IsDisabledAnimaWeapon()
        {
            return g.data.dataWorld.data.animaWeapons[0] == GameAnimaWeapon.None;
        }

        /// <summary>
        /// Enables only Pisces Pendant, disabling others.
        /// </summary>
        public static void EnableOnlyPiscesPendant()
        {
            g.data.dataWorld.data.animaWeapons.Clear();
            g.data.dataWorld.data.animaWeapons.Add(GameAnimaWeapon.PiscesPendant);
            UpdateUI();
        }

        /// <summary>
        /// Enables only Devil Demon, disabling others.
        /// </summary>
        public static void EnableOnlyDevilDemon()
        {
            g.data.dataWorld.data.animaWeapons.Clear();
            g.data.dataWorld.data.animaWeapons.Add(GameAnimaWeapon.DevilDemon);
            UpdateUI();
        }

        /// <summary>
        /// Enables only God Eye, disabling others.
        /// </summary>
        public static void EnableOnlyGodEye()
        {
            g.data.dataWorld.data.animaWeapons.Clear();
            g.data.dataWorld.data.animaWeapons.Add(GameAnimaWeapon.HootinEye);
            UpdateUI();
        }

        /// <summary>
        /// Disables all anima weapons.
        /// </summary>
        public static void DisableAnimaWeapon()
        {
            g.data.dataWorld.data.animaWeapons.Clear();
            g.data.dataWorld.data.animaWeapons.Add(GameAnimaWeapon.None);
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI to reflect current anima weapon states.
        /// Shows/hides artifact buttons on MapMain UI.
        /// </summary>
        public static void UpdateUI()
        {
            var ui = g.ui.GetUI<UIMapMain>(UIType.MapMain);
            if (ui != null)
            {
                ui.uiPlayerInfo.btnPiscesPendant.gameObject.SetActive(IsEnabledPiscesPendant());
                ui.uiPlayerInfo.btnDevilDemon.gameObject.SetActive(IsEnabledDevilDemon());
                ui.uiPlayerInfo.btnGodEye.gameObject.SetActive(IsEnabledGodEye());
            }
        }

        /// <summary>
        /// Gets the NPC unit ID associated with Pisces Pendant.
        /// </summary>
        /// <returns>NPC unit ID</returns>
        public static string GetPiscesPendantNpcId()
        {
            return g.data.dataWorld.data.piscesData.npcUnitId;
        }
    }
}