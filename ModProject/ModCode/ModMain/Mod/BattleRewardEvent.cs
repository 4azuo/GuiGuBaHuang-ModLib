using EBattleTypeData;
using MOD_JhUKQ7.Const;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_JhUKQ7.Mod
{
    [Cache(ModConst.BATTLE_REWARD_EVENT_KEY)]
    public sealed class BattleRewardEvent : ModEvent
    {
        public override int OrderIndex => 2;

        private const string PLAYER_D_DMG_KEY = "PdDmg";
        private const string PLAYER_R_DMG_KEY = "PrDmg";

        private const string PLAYER_UP_ATK_FACTOR = "UpAtkFactor";
        private const string PLAYER_UP_DEF_FACTOR = "UpDefFactor";
        private const string PLAYER_UP_MHP_FACTOR = "UpMHpFactor";
        private const string PLAYER_UP_MAP_FACTOR = "UpMApFactor";

        private const string PLAYER_UP_ATK_RATIO = "UpAtkRatio";
        private const string PLAYER_UP_DEF_RATIO = "UpDefRatio";
        private const string PLAYER_UP_MHP_RATIO = "UpMHpRatio";
        private const string PLAYER_UP_MAP_RATIO = "UpMApRatio";

        private const string PLAYER_UP_ATK_LIMIT = "UpAtkLimit";
        private const string PLAYER_UP_DEF_LIMIT = "UpDefLimit";
        private const string PLAYER_UP_MHP_LIMIT = "UpMHpLimit";
        private const string PLAYER_UP_MAP_LIMIT = "UpMApLimit";
        public IDictionary<string, double> TraningValues { get; set; } = new Dictionary<string, double>
        {
            [PLAYER_D_DMG_KEY] = 0,
            [PLAYER_R_DMG_KEY] = 0,

            [PLAYER_UP_ATK_FACTOR] = 100,
            [PLAYER_UP_DEF_FACTOR] = 100,
            [PLAYER_UP_MHP_FACTOR] = 10,
            [PLAYER_UP_MAP_FACTOR] = 300,

            [PLAYER_UP_ATK_RATIO] = 1.1,
            [PLAYER_UP_DEF_RATIO] = 1.15,
            [PLAYER_UP_MHP_RATIO] = 1.02,
            [PLAYER_UP_MAP_RATIO] = 1.25,

            [PLAYER_UP_ATK_LIMIT] = 100, //PLAYER_D_DMG_KEY
            [PLAYER_UP_DEF_LIMIT] = 100, //PLAYER_R_DMG_KEY
            [PLAYER_UP_MHP_LIMIT] = 10,  //PLAYER_R_DMG_KEY
            [PLAYER_UP_MAP_LIMIT] = 300, //PLAYER_D_DMG_KEY
        };
        [JsonIgnore]
        public int PlayerDealtDamage { get; set; }
        [JsonIgnore]
        public int PlayerRecvDamage { get; set; }
        [JsonIgnore]
        public bool IsPlayerDie { get; set; }

        public override void OnBattleStart(ETypeData e)
        {
            PlayerDealtDamage = 0;
            PlayerRecvDamage = 0;
            IsPlayerDie = false;
        }

        public override void OnUnitHit(UnitHit e)
        {
            var dmg = Math.Abs(e.hitData.hitValue);
            if (e.attackUnit.data.unitType == UnitType.Player)
            {
                PlayerDealtDamage += dmg;
                TraningValues[PLAYER_D_DMG_KEY] += dmg;
            }
            else if (e.hitUnit.data.unitType == UnitType.Player)
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
                DebugHelper.WriteLine($"BattleRewardEvent: +1atk, next {TraningValues[PLAYER_UP_ATK_LIMIT]}dmg");
                return true;
            }
            if (TraningValues[PLAYER_R_DMG_KEY] > TraningValues[PLAYER_UP_DEF_LIMIT])
            {
                player.AddProperty<int>(UnitPropertyEnum.Defense, 1);
                TraningValues[PLAYER_UP_DEF_FACTOR] *= TraningValues[PLAYER_UP_DEF_RATIO];
                TraningValues[PLAYER_UP_DEF_LIMIT] += TraningValues[PLAYER_UP_DEF_FACTOR];
                DebugHelper.WriteLine($"BattleRewardEvent: +1def, next {TraningValues[PLAYER_UP_DEF_LIMIT]}dmg");
                return true;
            }
            if (TraningValues[PLAYER_R_DMG_KEY] > TraningValues[PLAYER_UP_MHP_LIMIT])
            {
                player.AddProperty<int>(UnitPropertyEnum.HpMax, 1);
                TraningValues[PLAYER_UP_MHP_FACTOR] *= TraningValues[PLAYER_UP_MHP_RATIO];
                TraningValues[PLAYER_UP_MHP_LIMIT] += TraningValues[PLAYER_UP_MHP_FACTOR];
                DebugHelper.WriteLine($"BattleRewardEvent: +1hp, next {TraningValues[PLAYER_UP_MHP_LIMIT]}dmg");
                return true;
            }
            if (TraningValues[PLAYER_D_DMG_KEY] > TraningValues[PLAYER_UP_MAP_LIMIT])
            {
                player.AddProperty<int>(UnitPropertyEnum.MpMax, 1);
                TraningValues[PLAYER_UP_MAP_FACTOR] *= TraningValues[PLAYER_UP_MAP_RATIO];
                TraningValues[PLAYER_UP_MAP_LIMIT] += TraningValues[PLAYER_UP_MAP_FACTOR];
                DebugHelper.WriteLine($"BattleRewardEvent: +1ap, next {TraningValues[PLAYER_UP_MAP_LIMIT]}dmg");
                return true;
            }
            return false;
        }

        public override void OnBattleEnd(BattleEnd e)
        {
            var player = g.world.playerUnit;
            if (IsPlayerDie)
            {
                player.AddProperty<int>(UnitPropertyEnum.Life, -(Math.Max(PlayerRecvDamage / 1000, 1)));
                player.SetProperty<int>(UnitPropertyEnum.Exp, 0);
                DebugHelper.WriteLine($"BattleRewardEvent: player death");
                return;
            }

            if (e.isWin)
            {
                var rewardExp1 = Math.Max(PlayerDealtDamage / 100, 1);
                var rewardExp2 = Math.Max(PlayerRecvDamage / 10, 1);
                player.AddProperty<int>(UnitPropertyEnum.Exp, rewardExp1 + rewardExp2);
                DebugHelper.WriteLine($"BattleRewardEvent: +{rewardExp1 + rewardExp2}exp");
            }
        }

        public override void OnUnitDie(UnitDie e)
        {
            IsPlayerDie = e.unit.data.unitType == UnitType.Player;
            if (e.unit.data.unitType == UnitType.Monst && e.hitData.attackUnit.data.unitType == UnitType.Player)
            {
                var dieUnit = e.unit.data.TryCast<UnitDataHuman>();
                if (dieUnit != null)
                {
                    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Life, dieUnit.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Life) / 50);
                }
            }
        }
    }
}
