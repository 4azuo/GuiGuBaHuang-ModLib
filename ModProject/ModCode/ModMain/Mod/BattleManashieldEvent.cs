﻿using MOD_nE7UL2.Const;
using ModLib.Mod;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Mod
{
    [TraceIgnore]
    [Cache(ModConst.BATTLE_MANASHIELD_EVENT)]
    public class BattleManashieldEvent : ModBattleEvent
    {
        public const int MANASHIELD_EFFECT_MAIN_ID = 903151120;
        public const int MANASHIELD_EFFECT_EFX_ID = 903151121;

        public static _BattleManashieldConfigs ManashieldConfigs
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.BattleManashieldConfigs;
            }
        }

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            var humanData = e?.data?.TryCast<UnitDataHuman>();
            if (humanData?.unit != null && humanData?.worldUnitData?.unit != null)
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
                var ms = (humanData.mp * ManashieldConfigs.ManaShieldRate1) + (humanData.maxMP.value * ManashieldConfigs.ManaShieldRate2) + (humanData.hp * (0.05f * GetBloodEnergyLevel(humanData)));
                Effect3017.AddShield(efx, humanData.unit, MANASHIELD_EFFECT_EFX_ID, ms.Parse<int>(), humanData.maxHP.value * 2, int.MaxValue);
            }
        }

        [EventCondition(IsInBattle = true)]
        public override void OnTimeUpdate()
        {
            foreach (var unit in DungeonUnits)
            {
                if (!unit.isDie && unit.data.mp <= 0)
                {
                    var humanData = unit.data.TryCast<UnitDataHuman>();
                    if (humanData?.unit != null && humanData?.worldUnitData?.unit != null)
                    {
                        Effect3017.AddShieldValue(unit, MANASHIELD_EFFECT_EFX_ID, int.MinValue);
                    }
                }
            }
        }

        private int GetBloodEnergyLevel(UnitDataHuman humanData)
        {
            if (humanData.worldUnitData.unit.GetLuck(700094) != null)
                return 3;
            if (humanData.worldUnitData.unit.GetLuck(700093) != null)
                return 2;
            if (humanData.worldUnitData.unit.GetLuck(700092) != null)
                return 1;
            return 0;
        }
    }
}
