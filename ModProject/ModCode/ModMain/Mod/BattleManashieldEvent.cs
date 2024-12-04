using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using static DataMap;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_MANASHIELD_EVENT)]
    public class BattleManashieldEvent : ModEvent
    {
        public override int OrderIndex => 9001;

        public const int MANASHIELD_EFFECT_MAIN_ID = 903151120;
        public const int MANASHIELD_EFFECT_EFX_ID = 903151121;
        public const float MONST_SHIELD_CHANCE = 0.50f;

        public static _BattleManashieldConfigs ManashieldConfigs => ModMain.ModObj.InGameCustomSettings.BattleManashieldConfigs;

        [JsonIgnore]
        public static EffectBase PlayerShieldEfx { get; private set; }

        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);

            if (e.IsWorldUnit())
            {
                var humanData = e?.data?.TryCast<UnitDataHuman>();
                var shield = GetManashieldBase(humanData.worldUnitData.unit) + GetManashieldBasePlus(humanData.worldUnitData.unit);
                var maxShield = humanData.maxHP.value;
                var efx = ShieldUp(humanData.unit, shield, maxShield);
                if (humanData.worldUnitData.unit.IsPlayer())
                    PlayerShieldEfx = efx;
            }
            else if (e.IsMonster())
            {
                var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
                var monstData = e?.data?.TryCast<UnitDataMonst>();
                var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);

                if (monstData.monstType == MonstType.Common || monstData.monstType == MonstType.Elite)
                {
                    //add manashield
                    if (CommonTool.Random(0.0f, 100.0f).IsBetween(0.0f, smConfigs.Calculate(MONST_SHIELD_CHANCE * monstData.grade.value * gameLvl, smConfigs.Configs.AddSpecialMonsterRate).Parse<float>()))
                    {
                        var shield = monstData.maxHP.value;
                        ShieldUp(monstData.unit, shield, int.MaxValue);
                    }
                }
                else if (smConfigs.Configs.BossHasShield && (monstData.monstType == MonstType.BOSS || monstData.monstType == MonstType.NPC))
                {
                    var shield = monstData.maxHP.value;
                    ShieldUp(monstData.unit, shield, int.MaxValue);
                }
            }
        }

        private EffectBase ShieldUp(UnitCtrlBase cunit, int shield, int maxShield)
        {
            var efx = cunit.AddEffect(MANASHIELD_EFFECT_MAIN_ID, cunit, new SkillCreateData
            {
                mainSkillID = MANASHIELD_EFFECT_MAIN_ID,
                valueData = new BattleSkillValueData
                {
                    grade = cunit.data.grade.value,
                    level = 1,
                    data = new BattleSkillValueData.Data(),
                }
            });
            Effect3017.AddShield(efx, cunit, MANASHIELD_EFFECT_EFX_ID, shield, maxShield, int.MaxValue);
            return efx;
        }

        [ErrorIgnore]
        [EventCondition(IsInBattle = 1)]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();

            foreach (var unit in ModBattleEvent.DungeonUnits)
            {
                if (!unit.isDie)
                {
                    var humanData = unit.data.TryCast<UnitDataHuman>();
                    if (humanData?.worldUnitData?.unit != null)
                    {
                        if (unit.data.mp <= 0 && !IsWarlordPhantom(humanData))
                        {
                            Effect3017.AddShieldValue(unit, MANASHIELD_EFFECT_EFX_ID, int.MinValue);
                        }
                        //if (unit.data.mp > 0)// && EffectTool.GetEffects(unit, MANASHIELD_EFFECT_EFX_ID.ToString()).Count > 0)
                        //{
                        //    //var recoverShield = (((humanData.basisFist.value + humanData.basisPalm.value + humanData.basisFinger.value) / 3.0f) / 1000.00f).Parse<int>();
                        //    //Effect3017.AddShieldValue(unit, MANASHIELD_EFFECT_EFX_ID, 100);
                        //}
                    }
                }
            }
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
            return Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.Manashield));
        }
    }
}
