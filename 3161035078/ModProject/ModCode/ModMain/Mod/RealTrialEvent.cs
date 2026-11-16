using EBattleTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Helper;
using static MOD_nE7UL2.Object.ModStts;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Trial of lightning: lightning strength scales with the player; surviving grants properties.
    /// </summary>
    [Cache(ModConst.REAL_TRIAL_EVENT)]
    public class RealTrialEvent : ModEvent
    {
        public static RealTrialEvent Instance { get; set; }
        public static _RealTrialConfigs RealTrialConfigs => ModMain.ModObj.ModSettings.RealTrialConfigs;
        public static bool IsInTrial => DungeonHelper.IsTrailOfLightning();

        /// <summary>
        /// Sets the attack of trial units from player attack, game level and grade.
        /// </summary>
        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);

            var data = e?.data;
            if (IsInTrial && !e.IsPlayer())
            {
                var atk = g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Attack).value;
                var def = g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Defense).value;
                var basisThunder = g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.BasisThunder).value;
                var baseDmg = ((double)atk * (
                        GameHelper.GetGameLevel().Parse<int>() * RealTrialConfigs.PowerUpOnGameLevel
                        + g.world.playerUnit.GetGradeLvl() * RealTrialConfigs.PowerUpOnGradeLevel
                    )).ToIntClamp()
                    - (int)((long)def * basisThunder / 100);
                data.attack.baseValue = baseDmg;
            }
        }

        /// <summary>
        /// Survived: grows player properties.
        /// </summary>
        public override void OnBattleEndHandler(BattleEndHandler e)
        {
            base.OnBattleEndHandler(e);

            if (IsInTrial && !ModBattleEvent.PlayerUnit.isDie)
            {
                foreach (var p in UnitTypeEnum.Trial.PropIncRatio)
                {
                    var pType = p.Values[0] as UnitPropertyEnum;
                    g.world.playerUnit.AddProperty(pType, UnitTypeEnum.Trial.CalProp(pType, g.world.playerUnit.GetProperty<int>(pType)));
                }
            }
        }
    }
}
