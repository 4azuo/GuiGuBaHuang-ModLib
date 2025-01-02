using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System;
using static MOD_nE7UL2.Object.GameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_MANASHIELD_EVENT)]
    public class BattleManashieldEvent : ModEvent
    {
        public const int MANASHIELD_EFFECT_MAIN_ID = 903151120;
        public const int MANASHIELD_EFFECT_EFX_ID = 903151121;

        public static _BattleManashieldConfigs ManashieldConfigs => ModMain.ModObj.GameSettings.BattleManashieldConfigs;

        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);

            if (e.IsWorldUnit())
            {
                var humanData = e?.data?.TryCast<UnitDataHuman>();
                var shield = GetManashieldBase(humanData.worldUnitData.unit) + GetManashieldBasePlus(humanData.worldUnitData.unit);
                var maxShield = humanData.maxHP.value;
                ShieldUp(humanData.unit, shield, maxShield);
            }
        }

        public static EffectBase ShieldUp(UnitCtrlBase cunit, int shield, int maxShield)
        {
            if (cunit == null || ModBattleEvent.SceneBattle.battleEnd.isEnd)
                return null;
            var efx = cunit.AddEffect(MANASHIELD_EFFECT_MAIN_ID, cunit, new SkillCreateData
            {
                mainSkillID = MANASHIELD_EFFECT_MAIN_ID,
                valueData = new BattleSkillValueData
                {
                    grade = cunit.data.grade.value,
                    level = 1,
                    data = new BattleSkillValueData.Data
                    {
                        unitCtrlBase = cunit,
                    }
                }
            });
            Effect3017.AddShield(efx, cunit, MANASHIELD_EFFECT_EFX_ID, shield, maxShield, int.MaxValue);
            return efx;
        }

        private static int GetBloodEnergyLevel(WorldUnitBase wunit)
        {
            if (wunit.GetLuck(700094) != null)
                return 3;
            if (wunit.GetLuck(700093) != null)
                return 2;
            if (wunit.GetLuck(700092) != null)
                return 1;
            return 0;
        }

        private static bool IsWarlordPhantom(UnitDataHuman humanData)
        {
            return humanData.worldUnitData.unit.GetLuck(700026) != null;
        }

        public static int GetManashieldBase(WorldUnitBase wunit)
        {
            return (wunit.GetDynProperty(UnitDynPropertyEnum.Mp).value * ManashieldConfigs.ManaShieldRate1).Parse<int>() +
                (wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value * ManashieldConfigs.ManaShieldRate2).Parse<int>() +
                (wunit.GetDynProperty(UnitDynPropertyEnum.Hp).value * (0.05f * GetBloodEnergyLevel(wunit))).Parse<int>() +
                (wunit.GetDynProperty(UnitDynPropertyEnum.BasisFist).value / 100.00f * wunit.GetDynProperty(UnitDynPropertyEnum.Defense).value).Parse<int>();
        }

        public static int GetManashieldBasePlus(WorldUnitBase wunit)
        {
            return Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.Manashield));
        }
    }
}
