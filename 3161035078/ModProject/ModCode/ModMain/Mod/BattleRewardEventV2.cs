using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MOD_nE7UL2.Enum;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_REWARD_EVENT_V2)]
    public class BattleRewardEventV2 : ModEvent
    {
        public static BattleRewardEventV2 Instance { get; set; }

        public const float ANGEL_UP_RATE = 0.20f;
        public const float EVIL_UP_RATE = 0.01f;

        //public const int QI_ITEM = 484410100;
        public const float TEAMMATE_QUIT_RATE = 10f;

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

        private Dictionary<int, int[]> BodyReconstructions = new Dictionary<int, int[]>
        {
            [4] = new int[] { 1011721 },
            [5] = new int[] { 1011722 },
            [6] = new int[] { 1011722 },
            [7] = new int[] { 1011723 },
            [8] = new int[] { 1011723, 1011725 },
            [9] = new int[] { 1011724, 1011726 },
            [10] = new int[] { 1011724, 1011726 },
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
                    TraningValues.Add(GetFactorKey(prop), prop.Values[P_FACTOR].Parse<double>() * SMLocalConfigsEvent.Instance.Configs.GrowUpSpeed);
                    TraningValues.Add(GetLimitKey(prop), prop.Values[P_LIMIT].Parse<double>());
                }
            }
        }

        public override void OnBattleUnitHit(UnitHit e)
        {
            base.OnBattleUnitHit(e);
            if (!SMLocalConfigsEvent.Instance.Configs.NoGrowupFromBattles)
            {
                while (UpProperty(e))
                {
                }
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

            if (e.isWin)
            {
                DebugHelper.WriteLine($"BattleRewardEvent: win");
                var insight = player.GetDynProperty(UnitDynPropertyEnum.Talent).value;

                //player growup after battles
                if (!SMLocalConfigsEvent.Instance.Configs.NoGrowupFromBattles)
                {
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
                }

                //player +exp
                if (!SMLocalConfigsEvent.Instance.Configs.NoExpFromBattles)
                {
                    var rewardExp1 = Math.Max(localDmgDealt * ModMain.ModObj.ModSettings.BattleRewardConfigs.ExpPerDmgDealt, 1).Parse<int>();
                    var rewardExp2 = Math.Max(localDmgRecv * ModMain.ModObj.ModSettings.BattleRewardConfigs.ExpPerDmgRecv, 1).Parse<int>();
                    var rewardExp = rewardExp1 + rewardExp2;

                    var myRewardExp = (rewardExp * (insight / 100f)).Parse<int>();
                    player.AddExp(myRewardExp);
                    DebugHelper.WriteLine($"BattleRewardEvent: +{myRewardExp}exp");
                }
            }
            else
            {
                DebugHelper.WriteLine($"BattleRewardEvent: lose");
                if (g.world.battle.data.isRealBattle && !MapBuildBattleEvent.IsBattleTownWar() && !MapBuildBattleEvent.IsBattleMonstWave())
                {
                    if (ModBattleEvent.PlayerUnit.isDie)
                    {
                        //exp
                        var gradeLvl = player.GetGradeLvl();
                        var bodyReconstructionItemId = BodyReconstructions.ContainsKey(gradeLvl) ? BodyReconstructions[gradeLvl] : new int[0];
                        if (bodyReconstructionItemId.Length == 0 || bodyReconstructionItemId.All(x => player.GetUnitPropCount(x) <= 0))
                        {
                            player.ClearExp();
                        }
                        else
                        {
                            var item = bodyReconstructionItemId.First(x => player.GetUnitPropCount(x) > 0);
                            player.RemoveUnitProp(item, 1);
                        }

                        if (SMLocalConfigsEvent.Instance.Configs.Onelife)
                        {
                            player.SetProperty<int>(UnitPropertyEnum.Life, 0);
                        }
                        if (SMLocalConfigsEvent.Instance.Configs.Onelife || SMLocalConfigsEvent.Instance.Configs.AutoSaveAfterLostInBattle)
                        {
                            TimeSkipEvent.Instance.SkipTime(1);
                        }
                    }

                    //teammate quit
                    var teamData = HirePeopleEvent.GetTeamDetailData(player);
                    if (teamData != null)
                    {
                        foreach (var member in teamData.Item2)
                        {
                            if (!member.IsPlayer() && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, TEAMMATE_QUIT_RATE))
                            {
                                HirePeopleEvent.Dismiss(player, member);
                                member.data.unitData.relationData.AddHate(player.GetUnitId(), 20);
                            }
                        }
                    }
                }
            }
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            if (e?.unit != null && e.unit.IsWorldUnit() && !MapBuildBattleEvent.IsBattleTownWar() && !MapBuildBattleEvent.IsBattleMonstWave())
            {
                //DebugHelper.WriteLine("1");
                var dieUnit = e.unit;
                var dieUnitWUnit = dieUnit.GetWorldUnit();
                //var isDieUnitWUnit = dieUnit?.IsWorldUnit() ?? false; /*↑↑↑ checked ↑↑*/
                var killer = e?.hitData?.attackUnit;
                var killerWUnit = killer?.GetWorldUnit();
                var isKillerWUnit = killer?.IsWorldUnit() ?? false;

                //npc exp
                //DebugHelper.WriteLine("2");
                if (isKillerWUnit && !killerWUnit.IsPlayer())
                {
                    var insight = killerWUnit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
                    var rewardExp = ((dieUnit.data.maxHP.value + killer.data.maxHP.value) * (insight / 100f)).Parse<int>();
                    killerWUnit.AddExp(rewardExp);
                }

                if (g.world.battle.data.isRealBattle)
                {
                    //life drain
                    if (SMLocalConfigsEvent.Instance.Configs.LostLifespanWhenDie)
                    {
                        //DebugHelper.WriteLine("3");
                        var drainLife = Math.Max(6/*month*/ * g.game.data.dataWorld.data.gameLevel.Parse<int>(), dieUnit.data.maxHP.value / (3000 / g.game.data.dataWorld.data.gameLevel.Parse<int>()));
                        dieUnitWUnit.AddProperty<int>(UnitPropertyEnum.Life, -drainLife);
                        if (isKillerWUnit)
                            killerWUnit.AddProperty<int>(UnitPropertyEnum.Life, drainLife / 2);
                    }

                    //item
                    //DebugHelper.WriteLine("4");
                    if (SMLocalConfigsEvent.Instance.Configs.LostItemWhenDie)
                    {
                        var ringLockScore = dieUnitWUnit.GetEquippedRing()?.propsItem?.IsRing()?.lockScore ?? 0;
                        var luck = dieUnitWUnit.GetDynProperty(UnitDynPropertyEnum.Luck).value;
                        foreach (var item in dieUnitWUnit.GetUnitProps())
                        {
                            if (item.propsInfoBase.sale > 0 &&
                                !CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, (luck / 10.0f) + (ringLockScore / 100.0f)))
                            {
                                dieUnitWUnit.RemoveUnitProp(item.soleID);
                                if (isKillerWUnit)
                                    killerWUnit.AddUnitProp(item);
                            }
                        }
                    }

                    //spirit stones
                    //DebugHelper.WriteLine("5");
                    if (isKillerWUnit)
                        killerWUnit.AddUnitMoney(dieUnitWUnit.GetUnitMoney());
                    dieUnitWUnit.SetUnitMoney(0);

                    //reputation
                    //DebugHelper.WriteLine("6");
                    if (isKillerWUnit)
                    {
                        var dieUnitRep = dieUnitWUnit.GetDynProperty(UnitDynPropertyEnum.Reputation).value;
                        var killerRep = killerWUnit.GetDynProperty(UnitDynPropertyEnum.Reputation).value;
                        if (dieUnitRep > killerRep)
                        {
                            killerWUnit.AddProperty<int>(UnitPropertyEnum.Reputation, dieUnitRep / 10);
                        }
                        if (dieUnitRep > killerRep * 2)
                        {
                            dieUnitWUnit.AddProperty<int>(UnitPropertyEnum.Reputation, -(dieUnitRep / 10));
                        }
                    }

                    //intim
                    //DebugHelper.WriteLine("7");
                    if (isKillerWUnit)
                        dieUnitWUnit.data.unitData.relationData.AddHate(killerWUnit.GetUnitId(), 200f);

                    //angel kill evil
                    if (isKillerWUnit &&
                        killerWUnit.GetLuck(UnitTypeLuckEnum.Angel.Value.Parse<int>()) != null &&
                        dieUnitWUnit.GetLuck(UnitTypeLuckEnum.Evil.Value.Parse<int>()) != null)
                    {
                        foreach (var prop in new UnitPropertyEnum[] {
                            UnitPropertyEnum.HpMax,
                            UnitPropertyEnum.MpMax,
                            UnitPropertyEnum.SpMax,
                            UnitPropertyEnum.Attack,
                            UnitPropertyEnum.Defense,
                            killerWUnit.GetBestBasis().GetPropertyEnum()
                        })
                        {
                            killerWUnit.AddProperty<int>(prop, (dieUnitWUnit.GetProperty<int>(prop) * ANGEL_UP_RATE).Parse<int>());
                        }
                    }

                    //evil kill people
                    if (isKillerWUnit &&
                        killerWUnit.GetLuck(UnitTypeLuckEnum.Evil.Value.Parse<int>()) != null)
                    {
                        foreach (var prop in new UnitPropertyEnum[] {
                            UnitPropertyEnum.HpMax,
                            UnitPropertyEnum.MpMax,
                            UnitPropertyEnum.SpMax,
                            UnitPropertyEnum.Attack,
                            UnitPropertyEnum.Defense,

                            UnitPropertyEnum.BasisBlade,
                            UnitPropertyEnum.BasisEarth,
                            UnitPropertyEnum.BasisFinger,
                            UnitPropertyEnum.BasisFire,
                            UnitPropertyEnum.BasisFist,
                            UnitPropertyEnum.BasisFroze,
                            UnitPropertyEnum.BasisPalm,
                            UnitPropertyEnum.BasisSpear,
                            UnitPropertyEnum.BasisSword,
                            UnitPropertyEnum.BasisThunder,
                            UnitPropertyEnum.BasisWind,
                            UnitPropertyEnum.BasisWood
                        })
                        {
                            killerWUnit.AddProperty<int>(prop, (dieUnitWUnit.GetProperty<int>(prop) * EVIL_UP_RATE).Parse<int>());
                        }
                    }
                }
            }
        }
    }
}
