using EBattleTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Helper;
using System;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Trial of lightning: lightning strength scales with the player; surviving grants properties.
    /// </summary>
    [Cache(ModConst.REAL_TRIAL_EVENT)]
    public class RealTrialEvent : ModEvent
    {
        public static RealTrialEvent Instance { get; set; }
        public static RealTrialConfig RealTrialConfigs => RealTrialConfig.Conf;
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
                // merit weakens the lightning, karma strengthens it (MeritEvent)
                data.attack.baseValue = (int)Math.Round(baseDmg * (double)MeritEvent.LightningRate(g.world.playerUnit));
            }
        }

        /// <summary>
        /// Survived: grows player properties.
        /// </summary>
        public override void OnBattleEndHandler(BattleEndHandler e)
        {
            base.OnBattleEndHandler(e);

            // a body tribulation (BodyCulEvent) is a breakthrough, not the trial: no trial properties
            if (IsInTrial && !ModBattleEvent.PlayerUnit.isDie && !BodyCulEvent.InTribulation && !BodyCulEvent.TribulationEnded)
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
