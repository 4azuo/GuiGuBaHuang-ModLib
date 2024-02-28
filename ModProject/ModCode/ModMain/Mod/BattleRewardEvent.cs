using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_REWARD_EVENT_KEY)]
    public class BattleRewardEvent : ModEvent
    {
        public override int OrderIndex => 2;

        private const string PLAYER_D_DMG_KEY = "PdDmg";
        private const string PLAYER_R_DMG_KEY = "PrDmg";

        private const string PLAYER_UP_ATK_FACTOR = "UpAtkFactor";
        private const string PLAYER_UP_DEF_FACTOR = "UpDefFactor";
        private const string PLAYER_UP_MHP_FACTOR = "UpMHpFactor";
        private const string PLAYER_UP_MMP_FACTOR = "UpMMpFactor";

        private const string PLAYER_UP_ATK_RATIO = "UpAtkRatio";
        private const string PLAYER_UP_DEF_RATIO = "UpDefRatio";
        private const string PLAYER_UP_MHP_RATIO = "UpMHpRatio";
        private const string PLAYER_UP_MMP_RATIO = "UpMMpRatio";

        private const string PLAYER_UP_ATK_LIMIT = "UpAtkLimit";
        private const string PLAYER_UP_DEF_LIMIT = "UpDefLimit";
        private const string PLAYER_UP_MHP_LIMIT = "UpMHpLimit";
        private const string PLAYER_UP_MMP_LIMIT = "UpMMpLimit";
        public IDictionary<string, double> TraningValues { get; set; } = new Dictionary<string, double>
        {
            [PLAYER_D_DMG_KEY] = 0,
            [PLAYER_R_DMG_KEY] = 0,

            [PLAYER_UP_ATK_FACTOR] = 50,
            [PLAYER_UP_DEF_FACTOR] = 90,
            [PLAYER_UP_MHP_FACTOR] = 10,
            [PLAYER_UP_MMP_FACTOR] = 200,

            [PLAYER_UP_ATK_RATIO] = 1.070,
            [PLAYER_UP_DEF_RATIO] = 1.130,
            [PLAYER_UP_MHP_RATIO] = 1.015,
            [PLAYER_UP_MMP_RATIO] = 1.200,

            [PLAYER_UP_ATK_LIMIT] = 100, //PLAYER_D_DMG_KEY
            [PLAYER_UP_DEF_LIMIT] = 100, //PLAYER_R_DMG_KEY
            [PLAYER_UP_MHP_LIMIT] = 10,  //PLAYER_R_DMG_KEY
            [PLAYER_UP_MMP_LIMIT] = 200, //PLAYER_D_DMG_KEY
        };
        [JsonIgnore]
        public int PlayerDealtDamage { get; set; }
        [JsonIgnore]
        public int PlayerRecvDamage { get; set; }
        [JsonIgnore]
        public bool IsPlayerDie { get; set; }
        [JsonIgnore]
        public bool IsEnd { get; set; }

        public override void OnBattleStart(ETypeData e)
        {
            PlayerDealtDamage = 0;
            PlayerRecvDamage = 0;
            IsPlayerDie = false;
            IsEnd = false;
        }

        public override void OnBattleUnitHit(UnitHit e)
        {
            var dmg = Math.Abs(e.hitData.hitValue);
            var attackUnitData = e?.hitData?.attackUnit?.data?.TryCast<UnitDataHuman>();
            var hitUnitData = e?.hitUnit?.data?.TryCast<UnitDataHuman>();
            if (attackUnitData?.worldUnitData?.unit?.IsPlayer() ?? false)
            {
                PlayerDealtDamage += dmg;
                TraningValues[PLAYER_D_DMG_KEY] += dmg;
            }
            else if (hitUnitData?.worldUnitData?.unit?.IsPlayer() ?? false)
            {
                PlayerRecvDamage += dmg;
                TraningValues[PLAYER_R_DMG_KEY] += dmg;
            }

            while (UpProperty())
            {
            }
        }

        private bool UpProperty()
        {
            var player = g.world.playerUnit;
            if (TraningValues[PLAYER_D_DMG_KEY] > TraningValues[PLAYER_UP_ATK_LIMIT])
            {
                player.AddProperty<int>(UnitPropertyEnum.Attack, 1);
                TraningValues[PLAYER_UP_ATK_FACTOR] *= TraningValues[PLAYER_UP_ATK_RATIO];
                TraningValues[PLAYER_UP_ATK_LIMIT] += TraningValues[PLAYER_UP_ATK_FACTOR];
                //DebugHelper.WriteLine($"BattleRewardEvent: +1atk, next {TraningValues[PLAYER_UP_ATK_LIMIT]}dmg");
                return true;
            }
            if (TraningValues[PLAYER_R_DMG_KEY] > TraningValues[PLAYER_UP_DEF_LIMIT])
            {
                player.AddProperty<int>(UnitPropertyEnum.Defense, 1);
                TraningValues[PLAYER_UP_DEF_FACTOR] *= TraningValues[PLAYER_UP_DEF_RATIO];
                TraningValues[PLAYER_UP_DEF_LIMIT] += TraningValues[PLAYER_UP_DEF_FACTOR];
                //DebugHelper.WriteLine($"BattleRewardEvent: +1def, next {TraningValues[PLAYER_UP_DEF_LIMIT]}dmg");
                return true;
            }
            if (TraningValues[PLAYER_R_DMG_KEY] > TraningValues[PLAYER_UP_MHP_LIMIT])
            {
                player.AddProperty<int>(UnitPropertyEnum.HpMax, 1);
                TraningValues[PLAYER_UP_MHP_FACTOR] *= TraningValues[PLAYER_UP_MHP_RATIO];
                TraningValues[PLAYER_UP_MHP_LIMIT] += TraningValues[PLAYER_UP_MHP_FACTOR];
                //DebugHelper.WriteLine($"BattleRewardEvent: +1hp, next {TraningValues[PLAYER_UP_MHP_LIMIT]}dmg");
                return true;
            }
            if (TraningValues[PLAYER_D_DMG_KEY] > TraningValues[PLAYER_UP_MMP_LIMIT])
            {
                player.AddProperty<int>(UnitPropertyEnum.MpMax, 1);
                TraningValues[PLAYER_UP_MMP_FACTOR] *= TraningValues[PLAYER_UP_MMP_RATIO];
                TraningValues[PLAYER_UP_MMP_LIMIT] += TraningValues[PLAYER_UP_MMP_FACTOR];
                //DebugHelper.WriteLine($"BattleRewardEvent: +1mp, next {TraningValues[PLAYER_UP_MMP_LIMIT]}dmg");
                return true;
            }
            return false;
        }

        public override void OnBattleEnd(BattleEnd e)
        {
            if (!IsEnd)
            {
                DebugHelper.WriteLine($"Damage dealt: {PlayerDealtDamage}dmg, Damage recieve: {PlayerRecvDamage}dmg");
                var player = g.world.playerUnit;
                if (IsPlayerDie)
                {
                    player.AddProperty<int>(UnitPropertyEnum.Life, -(Math.Max(PlayerRecvDamage / 1000, 1)));
                    player.AddExp(int.MinValue);
                    DebugHelper.WriteLine($"BattleRewardEvent: player death");
                }
                else if (e.isWin)
                {
                    var rewardExp1 = Math.Max(PlayerDealtDamage / 100, 1);
                    var rewardExp2 = Math.Max(PlayerRecvDamage / 10, 1);
                    player.AddExp(rewardExp1 + rewardExp2);
                    DebugHelper.WriteLine($"BattleRewardEvent: +{rewardExp1 + rewardExp2}exp");
                }
                IsEnd = true;
            }
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            var dieUnit = e?.unit?.data?.TryCast<UnitDataHuman>();
            if (dieUnit?.worldUnitData != null && g.world.battle.data.isRealBattle)
            {
                if (dieUnit.worldUnitData.unit.IsPlayer())
                {
                    IsPlayerDie = true;
                }

                var attackUnitData = e?.hitData?.attackUnit?.data?.TryCast<UnitDataHuman>();
                if (attackUnitData?.worldUnitData?.unit?.IsPlayer() ?? false)
                {
                    var attackUnit = attackUnitData.worldUnitData.unit;
                    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Life, attackUnit.GetProperty<int>(UnitPropertyEnum.Life) / (50 + (g.game.data.dataWorld.data.gameLevel.Parse<int>() * 25)));
                }
            }
        }
    }
}
