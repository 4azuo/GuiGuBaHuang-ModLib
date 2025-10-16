public static class GodArtifactHelper
{
    public static bool IsPotmonDamaged()
    {
        return g.world.devilDemonMgr.devilDemonData.isDamage;
    }

    public static bool IsGodEyeDamaged()
    {
        return g.data.dataWorld.data.godEyeData.isDamage;
    }

    public static bool IsPiscesPendantDamaged()
    {
        return g.data.dataWorld.data.resurgeCount <= 0;
    }

    public static void RepairPotmonDamaged(int repairMonth = 0)
    {
        if (IsPotmonDamaged())
        {
            g.world.devilDemonMgr.devilDemonData.isDamage = false;
            g.world.devilDemonMgr.devilDemonData.repairMonth = repairMonth;
        }
    }

    public static void RepairGodEyeDamaged(int repairMonth = 0)
    {
        if (IsGodEyeDamaged())
        {
            g.data.dataWorld.data.godEyeData.isDamage = false;
            g.data.dataWorld.data.godEyeData.repairMonth = repairMonth;
        }
    }

    public static void AddFishJade(int add)
    {
        g.data.dataWorld.data.resurgeCount += add;
    }

    public static void SetFishJade(int add)
    {
        g.data.dataWorld.data.resurgeCount = add;
    }

    public static int GetFishJade()
    {
        return g.data.dataWorld.data.resurgeCount;
    }

    public static void SetAnimaWeapon(params GameAnimaWeapon[] weapons)
    {
        g.data.dataWorld.data.animaWeapons.Clear();
        g.data.dataWorld.data.animaWeapons.AddRange(weapons.ToIl2CppList());
    }

    public static Il2CppSystem.Collections.Generic.List<GameAnimaWeapon> GetAnimaWeapon()
    {
        return g.data.dataWorld.data.animaWeapons;
    }

    public static bool IsEnabledPiscesPendant()
    {
        return g.data.dataWorld.data.animaWeapons[0] == GameAnimaWeapon.PiscesPendant;
    }

    public static bool IsEnabledDevilDemon()
    {
        return g.data.dataWorld.data.animaWeapons[0] == GameAnimaWeapon.DevilDemon;
    }

    public static bool IsEnabledGodEye()
    {
        return g.data.dataWorld.data.animaWeapons[0] == GameAnimaWeapon.HootinEye;
    }

    public static bool IsDisabledAnimaWeapon()
    {
        return g.data.dataWorld.data.animaWeapons[0] == GameAnimaWeapon.None;
    }

    public static void EnableOnlyPiscesPendant()
    {
        g.data.dataWorld.data.animaWeapons.Clear();
        g.data.dataWorld.data.animaWeapons.Add(GameAnimaWeapon.PiscesPendant);
        UpdateUI();
    }

    public static void EnableOnlyDevilDemon()
    {
        g.data.dataWorld.data.animaWeapons.Clear();
        g.data.dataWorld.data.animaWeapons.Add(GameAnimaWeapon.DevilDemon);
        UpdateUI();
    }

    public static void EnableOnlyGodEye()
    {
        g.data.dataWorld.data.animaWeapons.Clear();
        g.data.dataWorld.data.animaWeapons.Add(GameAnimaWeapon.HootinEye);
        UpdateUI();
    }

    public static void DisableAnimaWeapon()
    {
        g.data.dataWorld.data.animaWeapons.Clear();
        g.data.dataWorld.data.animaWeapons.Add(GameAnimaWeapon.None);
        UpdateUI();
    }

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

    public static string GetPiscesPendantNpcId()
    {
        return g.data.dataWorld.data.piscesData.npcUnitId;
    }
}