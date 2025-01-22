using MOD_nE7UL2.Enum;
using ModLib.Object;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Object
{
    public class GameStts
    {
        #region HideButtonConfigs
        public class _HideButtonConfigs
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum SelectOption
            {
                Hide,
                Show,
            }
            public IDictionary<string, IDictionary<string, SelectOption>> ButtonConfigs { get; set; }
        }
        public _HideButtonConfigs HideButtonConfigs { get; set; }
        #endregion

        #region ContributionExchangeConfigs
        public class _ContributionExchangeConfigs
        {
            public int ExchangeRatio { get; set; }
        }
        public _ContributionExchangeConfigs ContributionExchangeConfigs { get; set; }
        #endregion

        #region MissionDeclareConfigs
        public class _MissionDeclareConfigs
        {
            public float FeeRate { get; set; }
            public int FeeMinCost { get; set; }
            public float DegreeCostRate { get; set; }
            public int DegreeMinCost { get; set; }
            public int[] CostTime { get; set; }
            public float[] SuccessRate { get; set; }
        }
        public _MissionDeclareConfigs MissionDeclareConfigs { get; set; }
        #endregion

        #region BattleManashieldConfigs
        public class _BattleManashieldConfigs
        {
            public float ManaShieldRate1 { get; set; }
            public float ManaShieldRate2 { get; set; }
        }
        public _BattleManashieldConfigs BattleManashieldConfigs { get; set; }
        #endregion

        #region UnitTypeConfigs
        public class _UnitTypeConfigs
        {
            public Dictionary<string, float> UnitTypes { get; set; }

            public UnitTypeEnum RandomUnitType(float r)
            {
                var min = 0.00f;
                var max = min;
                foreach (var ut in UnitTypes)
                {
                    min = max;
                    max = min + ut.Value;
                    if (ValueHelper.IsBetween(r, min, max))
                    {
                        return UnitTypeEnum.GetEnumByName<UnitTypeEnum>(ut.Key);
                    }
                }
                return UnitTypeEnum.Default;
            }
        }
        public _UnitTypeConfigs UnitTypeConfigs { get; set; }
        #endregion

        #region RealTrialConfigs
        public class _RealTrialConfigs
        {
            public float PowerUpOnGameLevel { get; set; }
            public float PowerUpOnGradeLevel { get; set; }
        }
        public _RealTrialConfigs RealTrialConfigs { get; set; }
        #endregion

        #region RealStorageConfigs
        public class _RealStorageConfigs
        {
            public float FeeRate { get; set; }
        }
        public _RealStorageConfigs RealStorageConfigs { get; set; }
        #endregion

        #region RealMarketConfigs
        public class _RealMarketConfigs
        {
            public class _RealMarketConfigs_Event : GameEvent
            {
                public float AddSellRate { get; set; }
                public float AddBuyRate { get; set; }
            }

            public float MinSellRate { get; set; }
            public float MaxSellRate { get; set; }
            public float MinBuyRate { get; set; }
            public float MaxBuyRate { get; set; }
            public float TownGReduceBuyRate { get; set; }
            public float TownMReduceBuyRate { get; set; }
            public Dictionary<string, _RealMarketConfigs_Event> Events { get; set; }

            public float GetAddSellRate()
            {
                return Events.Where(e => e.Value.IsRunningEvent()).Sum(e => e.Value.AddSellRate);
            }

            public float GetAddBuyRate()
            {
                return Events.Where(e => e.Value.IsRunningEvent()).Sum(e => e.Value.AddBuyRate);
            }
        }
        public _RealMarketConfigs RealMarketConfigs { get; set; }
        #endregion

        #region MonstStrongerConfigs
        public class _MonstStrongerConfigs
        {
            public float AtkR { get; set; }
            public float DefR { get; set; }
            public float MHpR { get; set; }
            public float Additionnal { get; set; }
            public float RebirthAffect { get; set; }
            public Dictionary<MonstType, float> GrowRate { get; set; }
            public Dictionary<MonstType, float> KillGrowRate { get; set; }
            public Dictionary<MonstType, float> PlayerAtk2Hp { get; set; }
            public Dictionary<int, float> AreaBonus { get; set; }
        }
        public _MonstStrongerConfigs MonstStrongerConfigs { get; set; }
        #endregion

        #region NpcUpgradeSkillConfigs
        public class _NpcUpgradeSkillConfigs
        {
            public Dictionary<MartialType, float> UpgradeRates { get; set; }

            public MartialType RandomUpgradingMartial(float r, float ratio)
            {
                var min = 0.00f;
                var max = min;
                foreach (var ut in UpgradeRates)
                {
                    min = max;
                    max = min + ut.Value * ratio;
                    if (ValueHelper.IsBetween(r, min, max))
                    {
                        return ut.Key;
                    }
                }
                return MartialType.None;
            }
        }
        public _NpcUpgradeSkillConfigs NpcUpgradeSkillConfigs { get; set; }
        #endregion

        #region ExpertConfigs
        public class _ExpertConfigs
        {
            public Dictionary<MartialType, float> SkillDmgRatios { get; set; }
            public Dictionary<MartialType, float> SkillExpRatios { get; set; }
            public float AutoArtifactExpRate { get; set; }
            public float BattleArtifactExpRate { get; set; }
        }
        public _ExpertConfigs ExpertConfigs { get; set; }
        #endregion

        #region BattleRewardConfigs
        public class _BattleRewardConfigs
        {
            public float ExpPerDmgDealt { get; set; }
            public float ExpPerDmgRecv { get; set; }
        }
        public _BattleRewardConfigs BattleRewardConfigs { get; set; }
        #endregion

        #region BankAccountConfigs
        public class _BankAccountConfigs
        {
            public IDictionary<int, int> OpenFee { get; set; }
        }
        public _BankAccountConfigs BankAccountConfigs { get; set; }
        #endregion

        #region InflationaryConfigs
        public class _InflationaryConfigs
        {
            public float InflationaryRate { get; set; }
        }
        public _InflationaryConfigs InflationaryConfigs { get; set; }
        #endregion
    }
}
