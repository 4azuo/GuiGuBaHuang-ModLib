using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_REWARD_EVENT)]
    public class BattleRewardEvent : ModBattleEvent
    {
        private const string PLAYER_D_DMG_OLDKEY = "PdDmg";
        private const string PLAYER_R_DMG_OLDKEY = "PrDmg";

        private static readonly MultiValue[] PLAYER_TRAINING_PROPERTIES_OLDVERSION_KEYS = new MultiValue[]
        {
            MultiValue.Create(UnitPropertyEnum.Attack, "UpAtkRatio", "UpAtkFactor", "UpAtkLimit"),
            MultiValue.Create(UnitPropertyEnum.Defense, "UpDefRatio", "UpDefFactor", "UpDefLimit"),
            MultiValue.Create(UnitPropertyEnum.HpMax, "UpMHpRatio", "UpMHpFactor", "UpMHpLimit"),
            MultiValue.Create(UnitPropertyEnum.MpMax, "UpMMpRatio", "UpMMpFactor", "UpMMpLimit"),
        };
        private static readonly MultiValue[] PLAYER_TRAINING_PROPERTIES = new MultiValue[]
        {
            //common attribute
            MultiValue.Create(UnitPropertyEnum.Attack, 1.070, 50, 100, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.Defense, 1.130, 90, 100, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.HpMax, 1.015, 10, 10, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.MpMax, 1.200, 200, 200, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Magic)) + 
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),

            //physic
            MultiValue.Create(UnitPropertyEnum.BasisBlade, 1.050, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Blade)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Blade)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisSpear, 1.050, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Spear)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Spear)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisSword, 1.050, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Sword)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Sword)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisFist, 1.050, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Fist)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Fist)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisPalm, 1.050, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Palm)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Palm)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisFinger, 1.050, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Finger)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Finger)) > sender.TraningValues[limitKey];
            })),

            //physic (global)
            MultiValue.Create(UnitPropertyEnum.BasisBlade, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Blade)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Blade)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisSpear, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Spear)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Spear)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisSword, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Sword)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Sword)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisFist, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Fist)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Fist)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisPalm, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Palm)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Palm)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisFinger, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Finger)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Finger)) > sender.TraningValues[limitKey];
            })),

            //magic
            MultiValue.Create(UnitPropertyEnum.BasisFire, 1.049, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Fire)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Fire)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisFroze, 1.049, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Frozen)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Frozen)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisThunder, 1.049, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Thunder)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Thunder)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisWind, 1.049, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Wind)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Wind)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisEarth, 1.049, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Earth)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Earth)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisWood, 1.049, 120, 1000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Wood)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Wood)) > sender.TraningValues[limitKey];
            })),

            //magic (global)
            MultiValue.Create(UnitPropertyEnum.BasisFire, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Fire)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Fire)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisFroze, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Frozen)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Frozen)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisThunder, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Thunder)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Thunder)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisWind, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Wind)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Wind)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisEarth, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Earth)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Earth)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create(UnitPropertyEnum.BasisWood, 1.1, 100000, 100000, new Func<BattleRewardEvent, string, bool>((sender, limitKey) =>
            {
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Wood)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Wood)) > sender.TraningValues[limitKey];
            })),
        };

        public IDictionary<string, double> TraningValues { get; set; } = new Dictionary<string, double>();

        [JsonIgnore]
        public bool IsEnd { get; set; }

        #region ###
        public string GetRatioKey(UnitPropertyEnum propEnum)
        {
            var oldKey = PLAYER_TRAINING_PROPERTIES_OLDVERSION_KEYS.FirstOrDefault(x => x.Values[0] == propEnum);
            if (oldKey != null)
            {
                return (string)oldKey.Values[1];
            }
            return $"{propEnum.PropName}Ratio";
        }

        public string GetFactorKey(UnitPropertyEnum propEnum)
        {
            var oldKey = PLAYER_TRAINING_PROPERTIES_OLDVERSION_KEYS.FirstOrDefault(x => x.Values[0] == propEnum);
            if (oldKey != null)
            {
                return (string)oldKey.Values[2];
            }
            return $"{propEnum.PropName}Factor";
        }

        public string GetLimitKey(UnitPropertyEnum propEnum)
        {
            var oldKey = PLAYER_TRAINING_PROPERTIES_OLDVERSION_KEYS.FirstOrDefault(x => x.Values[0] == propEnum);
            if (oldKey != null)
            {
                return (string)oldKey.Values[3];
            }
            return $"{propEnum.PropName}Limit";
        }
        #endregion

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            #region old
            if (!TraningValues.ContainsKey(PLAYER_D_DMG_OLDKEY))
                TraningValues.Add(PLAYER_D_DMG_OLDKEY, 0);
            if (!TraningValues.ContainsKey(PLAYER_R_DMG_OLDKEY))
                TraningValues.Add(PLAYER_R_DMG_OLDKEY, 0);
            if (GameDmg[GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage)] == 0)
                GameDmg[GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage)] = TraningValues[PLAYER_D_DMG_OLDKEY].Parse<long>();
            if (GameDmg[GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)] == 0)
                GameDmg[GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)] = TraningValues[PLAYER_R_DMG_OLDKEY].Parse<long>();
            #endregion
            foreach (var prop in PLAYER_TRAINING_PROPERTIES)
            {
                var propEnum = prop.Values[0] as UnitPropertyEnum;
                if (!TraningValues.ContainsKey(GetLimitKey(propEnum)))
                {
                    TraningValues.Add(GetRatioKey(propEnum), prop.Values[1].Parse<double>());
                    TraningValues.Add(GetFactorKey(propEnum), prop.Values[2].Parse<double>());
                    TraningValues.Add(GetLimitKey(propEnum), prop.Values[3].Parse<double>());
                }
            }
        }

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            IsEnd = false;
        }

        public override void OnBattleUnitHit(UnitHit e)
        {
            base.OnBattleUnitHit(e);
            while (UpProperty(e))
            {
            }
        }

        private bool UpProperty(UnitHit e)
        {
            var player = g.world.playerUnit;
            foreach (var prop in PLAYER_TRAINING_PROPERTIES)
            {
                var propEnum = prop.Values[0] as UnitPropertyEnum;
                var checkFunc = (Func<BattleRewardEvent, string, bool>)prop.Values[4];
                var ratioKey = GetRatioKey(propEnum);
                var factorKey = GetFactorKey(propEnum);
                var limitKey = GetLimitKey(propEnum);
                if (checkFunc.Invoke(this, limitKey))
                {
                    player.AddProperty<int>(propEnum, 1);
                    TraningValues[factorKey] *= TraningValues[ratioKey];
                    TraningValues[limitKey] += TraningValues[factorKey];
                    return true;
                }
            }
            return false;
        }

        public override void OnBattleEnd(BattleEnd e)
        {
            if (!IsEnd)
            {
                var localDmgDealt = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage));
                var localDmgRecv = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage));
                DebugHelper.WriteLine($"Damage dealt: {localDmgDealt}dmg, Damage recieve: {localDmgRecv}dmg");
                var player = g.world.playerUnit;
                if (IsPlayerDie)
                {
                    player.AddProperty<int>(UnitPropertyEnum.Life, -(Math.Max(localDmgRecv / 1000, 1).Parse<int>()));
                    player.ClearExp();
                    player.SetUnitMoney(0);
                    DebugHelper.WriteLine($"BattleRewardEvent: player death");
                }
                else if (e.isWin)
                {
                    var rewardExp1 = Math.Max(localDmgDealt * ModMain.ModObj.InGameSettings.BattleRewardConfigs.ExpPerDmgDealt, 1).Parse<int>();
                    var rewardExp2 = Math.Max(localDmgRecv * ModMain.ModObj.InGameSettings.BattleRewardConfigs.ExpPerDmgRecv, 1).Parse<int>();
                    player.AddExp(rewardExp1 + rewardExp2);
                    DebugHelper.WriteLine($"BattleRewardEvent: +{rewardExp1 + rewardExp2}exp");
                }
                IsEnd = true;
            }
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);
            var dieUnit = e?.unit?.data?.TryCast<UnitDataHuman>();
            if (dieUnit?.worldUnitData?.unit != null && g.world.battle.data.isRealBattle && !IsPlayerDie)
            {
                var attackUnitData = e?.hitData?.attackUnit?.data?.TryCast<UnitDataHuman>();
                if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.worldUnitData.unit.IsPlayer())
                {
                    var drainLife = dieUnit.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Life) / (50 + (g.game.data.dataWorld.data.gameLevel.Parse<int>() * 25));
                    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Life, drainLife);
                }
            }
        }
    }
}
