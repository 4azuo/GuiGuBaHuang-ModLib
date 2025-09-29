using MOD_nE7UL2.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace MOD_nE7UL2.Object
{
    public class ModStts
    {
        #region BaseConfigs
        public class _BaseConfigs
        {
            public float LevelExpRatio { get; set; }



            public float ArtifactDurableRatio { get; set; }
            public float ArtifactSpCostRatio { get; set; }
            public float ArtifactHpRatio { get; set; }
            public float ArtifactAtkRatio { get; set; }
            public float ArtifactDefRatio { get; set; }



            public float SkillMasteryExpRatio1 { get; set; }
            public float SkillMasteryExpRatio2 { get; set; }
            public float SkillMasteryExpRatio3 { get; set; }
            public float SkillMasteryExpRatio4 { get; set; }
            public float SkillMasteryExpRatio5 { get; set; }
            public float SkillMasteryExpRatio6 { get; set; }
            public float SkillMasteryExpRatio7 { get; set; }
            public float SkillMasteryExpRatio8 { get; set; }
            public float SkillMasteryExpRatio9 { get; set; }
            public float SkillMasteryExpRatio10 { get; set; }



            public float SkillMpCostRatio1 { get; set; }
            public float SkillMpCostRatio2 { get; set; }
            public float SkillMpCostRatio3 { get; set; }
            public float SkillMpCostRatio4 { get; set; }
            public float SkillMpCostRatio5 { get; set; }
            public float SkillMpCostRatio6 { get; set; }
            public float SkillMpCostRatio7 { get; set; }
            public float SkillMpCostRatio8 { get; set; }
            public float SkillMpCostRatio9 { get; set; }
            public float SkillMpCostRatio10 { get; set; }



            public float SkillCdRatio1 { get; set; }
            public float SkillCdRatio2 { get; set; }
            public float SkillCdRatio3 { get; set; }
            public float SkillCdRatio4 { get; set; }
            public float SkillCdRatio5 { get; set; }
            public float SkillCdRatio6 { get; set; }
            public float SkillCdRatio7 { get; set; }
            public float SkillCdRatio8 { get; set; }
            public float SkillCdRatio9 { get; set; }
            public float SkillCdRatio10 { get; set; }



            public float SkillZizhiBaseRatio1 { get; set; }
            public float SkillZizhiBaseRatio2 { get; set; }
            public float SkillZizhiBaseRatio3 { get; set; }
            public float SkillZizhiBaseRatio4 { get; set; }
            public float SkillZizhiBaseRatio5 { get; set; }
            public float SkillZizhiBaseRatio6 { get; set; }
            public float SkillZizhiBaseRatio7 { get; set; }
            public float SkillZizhiBaseRatio8 { get; set; }
            public float SkillZizhiBaseRatio9 { get; set; }
            public float SkillZizhiBaseRatio10 { get; set; }
            public float SkillZizhiAddRatio1 { get; set; }
            public float SkillZizhiAddRatio2 { get; set; }
            public float SkillZizhiAddRatio3 { get; set; }
            public float SkillZizhiAddRatio4 { get; set; }
            public float SkillZizhiAddRatio5 { get; set; }
            public float SkillZizhiAddRatio6 { get; set; }
            public float SkillZizhiAddRatio7 { get; set; }
            public float SkillZizhiAddRatio8 { get; set; }
            public float SkillZizhiAddRatio9 { get; set; }
            public float SkillZizhiAddRatio10 { get; set; }



            public float SkillDaodianBaseRatio1 { get; set; }
            public float SkillDaodianBaseRatio2 { get; set; }
            public float SkillDaodianBaseRatio3 { get; set; }
            public float SkillDaodianBaseRatio4 { get; set; }
            public float SkillDaodianBaseRatio5 { get; set; }
            public float SkillDaodianBaseRatio6 { get; set; }
            public float SkillDaodianBaseRatio7 { get; set; }
            public float SkillDaodianBaseRatio8 { get; set; }
            public float SkillDaodianBaseRatio9 { get; set; }
            public float SkillDaodianBaseRatio10 { get; set; }
            public float SkillDaodianAddRatio1 { get; set; }
            public float SkillDaodianAddRatio2 { get; set; }
            public float SkillDaodianAddRatio3 { get; set; }
            public float SkillDaodianAddRatio4 { get; set; }
            public float SkillDaodianAddRatio5 { get; set; }
            public float SkillDaodianAddRatio6 { get; set; }
            public float SkillDaodianAddRatio7 { get; set; }
            public float SkillDaodianAddRatio8 { get; set; }
            public float SkillDaodianAddRatio9 { get; set; }
            public float SkillDaodianAddRatio10 { get; set; }



            public float PowerUpPillEfxRatio1 { get; set; }
            public float PowerUpPillEfxRatio2 { get; set; }
            public float PowerUpPillEfxRatio3 { get; set; }
            public float PowerUpPillEfxRatio4 { get; set; }
            public float PowerUpPillEfxRatio5 { get; set; }
            public float PowerUpPillEfxRatio6 { get; set; }
            public float PowerUpPillEfxRatio7 { get; set; }
            public float PowerUpPillEfxRatio8 { get; set; }
            public float PowerUpPillEfxRatio9 { get; set; }
            public float PowerUpPillEfxRatio10 { get; set; }
        }
        public _BaseConfigs BaseConfigs { get; set; }
        #endregion

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
            public IDictionary<string, float> UnitTypes { get; set; }

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
            public IDictionary<string, _RealMarketConfigs_Event> Events { get; set; }

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
            public IDictionary<MonstType, float> GrowRate { get; set; }
            public IDictionary<MonstType, float> KillGrowRate { get; set; }
            public IDictionary<MonstType, float> PlayerAtk2Hp { get; set; }
            public IDictionary<int, float> AreaBonus { get; set; }
        }
        public _MonstStrongerConfigs MonstStrongerConfigs { get; set; }
        #endregion

        #region NpcUpgradeSkillConfigs
        public class _NpcUpgradeSkillConfigs
        {
            public IDictionary<MartialType, float> UpgradeRates { get; set; }

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
            public IDictionary<MartialType, float> SkillDmgRatios { get; set; }
            public IDictionary<MartialType, float> SkillExpRatios { get; set; }
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

        #region MapBuildBattleConfigs
        public class _MapBuildBattleConfigs
        {
            public float EscapeChance { get; set; }
        }
        public _MapBuildBattleConfigs MapBuildBattleConfigs { get; set; }
        #endregion

        #region MarketItemConfigs
        public class _MarketItemConfigs
        {
            public int JoinRange { get; set; }
            public float SellerJoinMarketRate { get; set; }
            public float SellRateDependOnGrade { get; set; }
            public float BuyerJoinMarketRate { get; set; }
            public float BuyRateDependOnGrade { get; set; }
            public float GetDealRate { get; set; }
            public float CancelDealOnMonth { get; set; }
            public float CancelDealLastMinuteRate { get; set; }
            public float HiddenRate { get; set; }
            public float GetStalkedRate { get; set; }
        }
        public _MarketItemConfigs MarketItemConfigs { get; set; }
        #endregion

        #region BattleConfigs
        public class _BattleConfigs
        {
            public int JoinRange { get; set; }
            public float RandomNpcJoinRate { get; set; }
            public float SectNpcJoinRate { get; set; }
            public float TownGuardNpcJoinRate { get; set; }
            public float TeamMemberJoinRate { get; set; }
            public float TeamMemberBetrayRate { get; set; }
            public float StalkerJoinRate { get; set; }
            public int FriendlyIntim { get; set; }
            public int EnemyIntim { get; set; }
            public int DifferenceRighteous { get; set; }
        }
        public _BattleConfigs BattleConfigs { get; set; }
        #endregion

        #region CustomRefineConfigs
        public class _CustomRefineConfigs
        {
            public IDictionary<string, float> LevelRates { get; set; }

            public AdjLevelEnum RandomRefineLevel(float r)
            {
                var min = 0.00f;
                var max = min;
                foreach (var ut in LevelRates)
                {
                    min = max;
                    max = min + ut.Value;
                    if (ValueHelper.IsBetween(r, min, max))
                    {
                        return AdjLevelEnum.GetEnumByName<AdjLevelEnum>(ut.Key);
                    }
                }
                return AdjLevelEnum.None;
            }
        }
        public _CustomRefineConfigs CustomRefineConfigs { get; set; }
        #endregion
    }
}
