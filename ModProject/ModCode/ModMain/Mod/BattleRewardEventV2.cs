using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_REWARD_EVENT_V2)]
    public class BattleRewardEventV2 : ModEvent
    {
        private const int QI_ITEM = 484410100;

        private const string RATIO_KEY = "Ratio";
        private const string FACTOR_KEY = "Factor";
        private const string LIMIT_KEY = "Limit";

        private const int P_NAME = 0;
        private const int P_PROP = 1;
        private const int P_RATIO = 2;
        private const int P_FACTOR = 3;
        private const int P_LIMIT = 4;
        private const int P_INVOKE_COND = 5;
        private const int P_UPPROP_COND = 6;

        private Dictionary<int, int> BodyReconstructions = new Dictionary<int, int>
        {
            [4] = 1011721,
            [5] = 1011722,
            [6] = 1011722,
            [7] = 1011723,
            [8] = 1011723,
            [9] = 1011724,
            [10] = 1011724,
            [8] = 1011725,
            [9] = 1011726,
            [10] = 1011726,
        };

        private static readonly MultiValue[] PLAYER_TRAINING_PROPERTIES = new MultiValue[]
        {
            #region common attribute
            MultiValue.Create("Atk1", UnitDynPropertyEnum.Attack, 1.070, 50, 100, 
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Atk3", UnitDynPropertyEnum.Attack, 1.080, 100, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 3),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Atk5",UnitDynPropertyEnum.Attack, 1.090, 200, 800,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 5),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Atk7", UnitDynPropertyEnum.Attack, 1.100, 400, 3200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 7),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Atk9", UnitDynPropertyEnum.Attack, 1.110, 800, 12800,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 9),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Def1", UnitDynPropertyEnum.Defense, 1.130, 100, 100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Def4", UnitDynPropertyEnum.Defense, 1.150, 1000, 1000,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 4),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Def8", UnitDynPropertyEnum.Defense, 1.200, 10000, 10000,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 8),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("HpMax", UnitDynPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("1000P+HpMax", UnitDynPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 1000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("3000P+HpMax", UnitDynPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 3000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("6000P+HpMax", UnitDynPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 6000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("12000P+HpMax", UnitDynPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 12000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("24000P+HpMax", UnitDynPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 24000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("48000P+HpMax", UnitDynPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 48000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("MpMax", UnitDynPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Magic)) + 
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("1000M+MpMax", UnitDynPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 1000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Magic)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("3000M+MpMax", UnitDynPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 3000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Magic)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("6000M+MpMax", UnitDynPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 6000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Magic)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("12000M+MpMax", UnitDynPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 12000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Magic)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("24000M+MpMax", UnitDynPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 24000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Magic)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("48000M+MpMax", UnitDynPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 48000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Magic)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            #endregion

            #region physic
            MultiValue.Create("Blade", UnitDynPropertyEnum.BasisBlade, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Blade)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Blade)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Spear", UnitDynPropertyEnum.BasisSpear, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Spear)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Spear)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Sword", UnitDynPropertyEnum.BasisSword, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Sword)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Sword)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Fist", UnitDynPropertyEnum.BasisFist, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Fist)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Fist)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Palm", UnitDynPropertyEnum.BasisPalm, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Palm)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Palm)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Finger", UnitDynPropertyEnum.BasisFinger, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Finger)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Finger)) > sender.TraningValues[limitKey];
            })),
            #endregion

            #region physic (global)
            MultiValue.Create("GBlade", UnitDynPropertyEnum.BasisBlade, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Blade)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Blade)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GSpear", UnitDynPropertyEnum.BasisSpear, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Spear)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Spear)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GSword", UnitDynPropertyEnum.BasisSword, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Sword)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Sword)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GFist", UnitDynPropertyEnum.BasisFist, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Fist)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Fist)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GPalm", UnitDynPropertyEnum.BasisPalm, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Palm)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Palm)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GFinger", UnitDynPropertyEnum.BasisFinger, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Finger)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Finger)) > sender.TraningValues[limitKey];
            })),
            #endregion

            #region magic
            MultiValue.Create("Fire", UnitDynPropertyEnum.BasisFire, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Fire)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Fire)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Froze", UnitDynPropertyEnum.BasisFroze, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Frozen)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Frozen)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Thunder", UnitDynPropertyEnum.BasisThunder, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Thunder)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Thunder)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Wind", UnitDynPropertyEnum.BasisWind, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Wind)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Wind)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Earth", UnitDynPropertyEnum.BasisEarth, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Earth)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Earth)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Wood", UnitDynPropertyEnum.BasisWood, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Wood)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Wood)) > sender.TraningValues[limitKey];
            })),
            #endregion

            #region magic (global)
            MultiValue.Create("GFire", UnitDynPropertyEnum.BasisFire, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Fire)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Fire)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GFroze", UnitDynPropertyEnum.BasisFroze, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Frozen)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Frozen)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GThunder", UnitDynPropertyEnum.BasisThunder, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Thunder)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Thunder)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GWind", UnitDynPropertyEnum.BasisWind, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Wind)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Wind)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GEarth", UnitDynPropertyEnum.BasisEarth, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Earth)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Earth)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GWood", UnitDynPropertyEnum.BasisWood, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Wood)) +
                ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Global, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Wood)) > sender.TraningValues[limitKey];
            })),
            #endregion
        };

        public IDictionary<string, double> TraningValues { get; set; } = new Dictionary<string, double>();

        public string GetRatioKey(MultiValue p)
        {
            return $"{p.Value0}{RATIO_KEY}";
        }
        public string GetRatioKey(string p)
        {
            return $"{p}{RATIO_KEY}";
        }

        public string GetFactorKey(MultiValue p)
        {
            return $"{p.Value0}{FACTOR_KEY}";
        }
        public string GetFactorKey(string p)
        {
            return $"{p}{FACTOR_KEY}";
        }

        public string GetLimitKey(MultiValue p)
        {
            return $"{p.Value0}{LIMIT_KEY}";
        }
        public string GetLimitKey(string p)
        {
            return $"{p}{LIMIT_KEY}";
        }

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            foreach (var prop in PLAYER_TRAINING_PROPERTIES)
            {
                if (!TraningValues.ContainsKey(GetLimitKey(prop)))
                {
                    TraningValues.Add(GetRatioKey(prop), prop.Values[P_RATIO].Parse<double>());
                    TraningValues.Add(GetFactorKey(prop), prop.Values[P_FACTOR].Parse<double>());
                    TraningValues.Add(GetLimitKey(prop), prop.Values[P_LIMIT].Parse<double>());
                }
            }
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
                var condFunc = (Func<BattleRewardEventV2, bool>)prop.Values[P_INVOKE_COND];
                if (condFunc.Invoke(this))
                {
                    var propEnum = prop.Values[P_PROP] as UnitDynPropertyEnum;
                    var checkFunc = (Func<BattleRewardEventV2, MultiValue, bool>)prop.Values[P_UPPROP_COND];
                    var ratioKey = GetRatioKey(prop);
                    var factorKey = GetFactorKey(prop);
                    var limitKey = GetLimitKey(prop);
                    if (checkFunc.Invoke(this, prop))
                    {
                        player.AddProperty<int>(propEnum.GetPropertyEnum(), 1);
                        TraningValues[factorKey] *= TraningValues[ratioKey];
                        TraningValues[limitKey] += TraningValues[factorKey];
                        return true;
                    }
                }
            }
            return false;
        }

        [Trace]
        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);

            var localDmgDealt = ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Damage));
            var localDmgRecv = ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage));
            DebugHelper.WriteLine($"Damage dealt: {localDmgDealt}dmg, Damage recieve: {localDmgRecv}dmg");
            var player = g.world.playerUnit;
            if (g.world.battle.data.isRealBattle && ModBattleEvent.PlayerUnit.isDie)
            {
                DebugHelper.WriteLine($"BattleRewardEvent: lose");
                //life
                player.AddProperty<int>(UnitPropertyEnum.Life, -unchecked((int)Math.Max(localDmgRecv / 2000, 1)));
                //exp
                var gradeLvl = player.GetGradeLvl();
                var bodyReconstructionItemId = BodyReconstructions.ContainsKey(gradeLvl) ? BodyReconstructions[gradeLvl] : 0;
                if (bodyReconstructionItemId == 0 || player.GetUnitPropCount(bodyReconstructionItemId) <= 0)
                {
                    player.ClearExp();
                }
                else
                {
                    player.RemoveUnitProp(bodyReconstructionItemId, 1);
                }
                //item
                var ringLockScore = player.GetEquippedRing()?.propsItem?.IsRing()?.lockScore ?? 0;
                var luck = g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Luck).value;
                foreach (var item in player.GetUnitProps())
                {
                    if (item.propsInfoBase.sale > 0 &&
                        !CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, (luck / 10.0f) + (ringLockScore / 100.0f)))
                    {
                        player.RemoveUnitProp(item.soleID);
                    }
                }
                //spirit stones
                player.SetUnitMoney(0);
                //log
                DebugHelper.WriteLine($"BattleRewardEvent: player death");
            }
            else if (e.isWin)
            {
                DebugHelper.WriteLine($"BattleRewardEvent: win");
                var insight = player.GetDynProperty(UnitDynPropertyEnum.Talent).value;
                var aBestBasis = ModBattleEvent.GetDmgPropertyEnum(ModBattleEvent.sGetHighestDealtDmgTypeEnum());
                if (aBestBasis != null && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, insight / 10))
                {
                    player.AddProperty<int>(aBestBasis.GetPropertyEnum(), 1);
                }
                var bBestBasis = ModBattleEvent.GetDmgPropertyEnum(ModBattleEvent.sGetHighestRecvDmgTypeEnum());
                if (bBestBasis != null && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, insight / 10))
                {
                    player.AddProperty<int>(bBestBasis.GetPropertyEnum(), 1);
                }

                var rewardExp1 = Math.Max(localDmgDealt * ModMain.ModObj.InGameCustomSettings.BattleRewardConfigs.ExpPerDmgDealt, 1).Parse<int>();
                var rewardExp2 = Math.Max(localDmgRecv * ModMain.ModObj.InGameCustomSettings.BattleRewardConfigs.ExpPerDmgRecv, 1).Parse<int>();
                var rewardExp = rewardExp1 + rewardExp2;
                var myRewardExp = (rewardExp * (insight / 100f)).Parse<int>();
                player.AddExp(myRewardExp);
                DebugHelper.WriteLine($"BattleRewardEvent: +{myRewardExp}exp");

                foreach (var unit in ModBattleEvent.DungeonUnits)
                {
                    var unitData = unit?.data?.TryCast<UnitDataHuman>();
                    if (!unit.isDie && unitData?.worldUnitData?.unit != null)
                    {
                        var unitRewardExp = (rewardExp * (unitData.worldUnitData.unit.GetDynProperty(UnitDynPropertyEnum.Talent).value / 100f)).Parse<int>();
                        unitData.worldUnitData.unit.AddExp(unitRewardExp);
                    }
                }
            }
            //if (IsForeSaveConfigOK())
            //{
            //    //force save
            //}
        }

        //private bool IsForeSaveConfigOK()
        //{
        //    var conf = ModMain.ModObj.InGameCustomSettings.ForceSaveCondition;
        //    if (string.IsNullOrEmpty(conf))
        //        return false;
        //    conf = conf.Replace("${gradelevel}", g.world.playerUnit.GetGradeLvl().ToString())
        //        .Replace("${gamelevel}", g.data.dataWorld.data.gameLevel.Parse<int>().ToString());
        //    return Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync<bool>(conf).Result;
        //}

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            var attackUnitData = e?.hitData?.attackUnit?.data?.TryCast<UnitDataHuman>();
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.worldUnitData.unit.IsPlayer())
            {
                var dieUnit = e?.unit?.data?.TryCast<UnitDataHuman>();
                if (dieUnit?.worldUnitData?.unit != null && !ModBattleEvent.PlayerUnit.isDie)
                {
                    if (g.world.battle.data.isRealBattle)
                    {
                        var drainLife = dieUnit.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Life) / (g.game.data.dataWorld.data.gameLevel.Parse<int>() * 20);
                        g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Life, drainLife);
                    }

                    var insight = g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
                    var bBestBasis = dieUnit.worldUnitData.unit.GetBestBasis();
                    if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, insight / 10) &&
                        dieUnit.worldUnitData.unit.GetDynProperty(bBestBasis).value - g.world.playerUnit.GetDynProperty(bBestBasis).value >= 6 * g.world.playerUnit.GetGradeLvl())
                    {
                        g.world.playerUnit.AddProperty<int>(bBestBasis.GetPropertyEnum(), 1);
                    }
                }
            }
        }
    }
}
