using MOD_nE7UL2.Const;
using MOD_nE7UL2.Object;
using ModLib.Mod;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_MANASHIELD_EVENT)]
    public class BattleManashieldEvent : ModEvent
    {
        public const int MANASHIELD_EFFECT_MAIN_ID = 903151120;
        public const int MANASHIELD_EFFECT_EFX_ID = 903151121;

        public static float MANA_SHIELD_RATE
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.BattleManashieldConfigs.ManaShieldRate;
            }
        }

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            var humanData = e?.data?.TryCast<UnitDataHuman>();
            if (humanData?.unit != null)
            {
                var efx = humanData.unit.AddEffect(MANASHIELD_EFFECT_MAIN_ID, humanData.unit, new SkillCreateData
                {
                    valueData = new BattleSkillValueData
                    {
                        grade = 1,
                        level = 1,
                        data = new BattleSkillValueData.Data(),
                    }
                });
                Effect3017.AddShield(efx, humanData.unit, MANASHIELD_EFFECT_EFX_ID, (humanData.mp * MANA_SHIELD_RATE).Parse<int>(), humanData.maxHP.value * 2, int.MaxValue);
            }
        }
    }
}
