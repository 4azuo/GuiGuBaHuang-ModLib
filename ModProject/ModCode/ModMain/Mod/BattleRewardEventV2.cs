using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_REWARD_EVENT_V2)]
    public class BattleRewardEventV2 : ModBattleEvent
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
        private const int P_OLDV = 7;

        private static readonly MultiValue[] PLAYER_TRAINING_PROPERTIES = new MultiValue[]
        {
            #region common attribute
            MultiValue.Create("Atk1", UnitPropertyEnum.Attack, 1.070, 50, 100, 
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            }), "UpAtk"),
            MultiValue.Create("Atk3", UnitPropertyEnum.Attack, 1.080, 100, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 3),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Atk5",UnitPropertyEnum.Attack, 1.090, 200, 800,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 5),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Atk7", UnitPropertyEnum.Attack, 1.100, 400, 3200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 7),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Atk9", UnitPropertyEnum.Attack, 1.110, 800, 12800,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 9),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Def1", UnitPropertyEnum.Defense, 1.130, 100, 100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            }), "UpDef"),
            MultiValue.Create("Def4", UnitPropertyEnum.Defense, 1.150, 1000, 1000,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 4),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("Def8", UnitPropertyEnum.Defense, 1.200, 10000, 10000,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetGradeLvl() >= 8),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("HpMax", UnitPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            }), "UpMMp"),
            MultiValue.Create("1000P+HpMax", UnitPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 1000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("3000P+HpMax", UnitPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 3000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("6000P+HpMax", UnitPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 6000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("12000P+HpMax", UnitPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 12000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("24000P+HpMax", UnitPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 24000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("48000P+HpMax", UnitPropertyEnum.HpMax, 1.015, 10, 10,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisPhysicSum() >= 48000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("MpMax", UnitPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Magic)) + 
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            }), "UpDef"),
            MultiValue.Create("1000M+MpMax", UnitPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 1000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Magic)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("3000M+MpMax", UnitPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 3000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Magic)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("6000M+MpMax", UnitPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 6000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Magic)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("12000M+MpMax", UnitPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 12000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Magic)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("24000M+MpMax", UnitPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 24000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Magic)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("48000M+MpMax", UnitPropertyEnum.MpMax, 1.200, 200, 200,
                new Func<BattleRewardEventV2, bool>((sender) => g.world.playerUnit.GetBasisMagicSum() >= 48000),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Magic)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Magic)) > sender.TraningValues[limitKey];
            })),
            #endregion

            #region physic
            MultiValue.Create("Blade", UnitPropertyEnum.BasisBlade, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Blade)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Blade)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisBlade.PropName),
            MultiValue.Create("Spear", UnitPropertyEnum.BasisSpear, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Spear)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Spear)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisSpear.PropName),
            MultiValue.Create("Sword", UnitPropertyEnum.BasisSword, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Sword)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Sword)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisSword.PropName),
            MultiValue.Create("Fist", UnitPropertyEnum.BasisFist, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Fist)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Fist)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisFist.PropName),
            MultiValue.Create("Palm", UnitPropertyEnum.BasisPalm, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Palm)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Palm)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisPalm.PropName),
            MultiValue.Create("Finger", UnitPropertyEnum.BasisFinger, 1.050, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Finger)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Finger)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisFinger.PropName),
            #endregion

            #region physic (global)
            MultiValue.Create("GBlade", UnitPropertyEnum.BasisBlade, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Blade)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Blade)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GSpear", UnitPropertyEnum.BasisSpear, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Spear)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Spear)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GSword", UnitPropertyEnum.BasisSword, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Sword)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Sword)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GFist", UnitPropertyEnum.BasisFist, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Fist)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Fist)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GPalm", UnitPropertyEnum.BasisPalm, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Palm)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Palm)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GFinger", UnitPropertyEnum.BasisFinger, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Finger)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Finger)) > sender.TraningValues[limitKey];
            })),
            #endregion

            #region magic
            MultiValue.Create("Fire", UnitPropertyEnum.BasisFire, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Fire)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Fire)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisFire.PropName),
            MultiValue.Create("Froze", UnitPropertyEnum.BasisFroze, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Frozen)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Frozen)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisFroze.PropName),
            MultiValue.Create("Thunder", UnitPropertyEnum.BasisThunder, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Thunder)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Thunder)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisThunder.PropName),
            MultiValue.Create("Wind", UnitPropertyEnum.BasisWind, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Wind)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Wind)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisWind.PropName),
            MultiValue.Create("Earth", UnitPropertyEnum.BasisEarth, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Earth)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Earth)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisEarth.PropName),
            MultiValue.Create("Wood", UnitPropertyEnum.BasisWood, 1.049, 125, 1100,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Wood)) +
                sender.GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Wood)) > sender.TraningValues[limitKey];
            }), UnitPropertyEnum.BasisWood.PropName),
            #endregion

            #region magic (global)
            MultiValue.Create("GFire", UnitPropertyEnum.BasisFire, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Fire)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Fire)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GFroze", UnitPropertyEnum.BasisFroze, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Frozen)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Frozen)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GThunder", UnitPropertyEnum.BasisThunder, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Thunder)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Thunder)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GWind", UnitPropertyEnum.BasisWind, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Wind)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Wind)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GEarth", UnitPropertyEnum.BasisEarth, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Earth)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Earth)) > sender.TraningValues[limitKey];
            })),
            MultiValue.Create("GWood", UnitPropertyEnum.BasisWood, 1.1, 100000, 100000,
                new Func<BattleRewardEventV2, bool>((sender) => true),
                new Func<BattleRewardEventV2, MultiValue, bool>((sender, prop) =>
            {
                var limitKey = sender.GetLimitKey(prop);
                return sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Wood)) +
                sender.GetDmg(DmgSaveEnum.Global, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Wood)) > sender.TraningValues[limitKey];
            })),
            #endregion
        };

        public IDictionary<string, double> TraningValues { get; set; } = new Dictionary<string, double>();

        [JsonIgnore]
        public bool IsEnd { get; set; }

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
            var oldVersion = EventHelper.GetEvent<BattleRewardEvent>(ModConst.BATTLE_REWARD_EVENT);
            foreach (var prop in PLAYER_TRAINING_PROPERTIES)
            {
                if (!TraningValues.ContainsKey(GetLimitKey(prop)))
                {
                    var oldValueName = prop.Get(P_OLDV) as string;
                    if (!string.IsNullOrEmpty(oldValueName))
                    {
                        var rKey = GetRatioKey(oldValueName);
                        var fKey = GetFactorKey(oldValueName);
                        var lKey = GetLimitKey(oldValueName);
                        if (oldVersion.TraningValues.ContainsKey(rKey) &&
                            oldVersion.TraningValues.ContainsKey(fKey) &&
                            oldVersion.TraningValues.ContainsKey(lKey))
                        {
                            TraningValues.Add(GetRatioKey(prop), oldVersion.TraningValues[rKey]);
                            TraningValues.Add(GetFactorKey(prop), oldVersion.TraningValues[fKey]);
                            TraningValues.Add(GetLimitKey(prop), oldVersion.TraningValues[lKey]);
                        }
                    }
                    else
                    {
                        TraningValues.Add(GetRatioKey(prop), prop.Values[P_RATIO].Parse<double>());
                        TraningValues.Add(GetFactorKey(prop), prop.Values[P_FACTOR].Parse<double>());
                        TraningValues.Add(GetLimitKey(prop), prop.Values[P_LIMIT].Parse<double>());
                    }
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
                var condFunc = (Func<BattleRewardEventV2, bool>)prop.Values[P_INVOKE_COND];
                if (condFunc.Invoke(this))
                {
                    var propEnum = prop.Values[P_PROP] as UnitPropertyEnum;
                    var checkFunc = (Func<BattleRewardEventV2, MultiValue, bool>)prop.Values[P_UPPROP_COND];
                    var ratioKey = GetRatioKey(prop);
                    var factorKey = GetFactorKey(prop);
                    var limitKey = GetLimitKey(prop);
                    if (checkFunc.Invoke(this, prop))
                    {
                        player.AddProperty<int>(propEnum, 1);
                        TraningValues[factorKey] *= TraningValues[ratioKey];
                        TraningValues[limitKey] += TraningValues[factorKey];
                        return true;
                    }
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
                    var insight = g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
                    var aBestBasis = GetDmgPropertyEnum(GetHighestDealtDmgTypeEnum());
                    if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, insight / 10))
                    {
                        g.world.playerUnit.AddProperty<int>(aBestBasis, 1);
                    }
                    var bBestBasis = GetDmgPropertyEnum(GetHighestRecvDmgTypeEnum());
                    if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, insight / 10))
                    {
                        g.world.playerUnit.AddProperty<int>(bBestBasis, 1);
                    }

                    var rewardExp1 = Math.Max(localDmgDealt * ModMain.ModObj.InGameCustomSettings.BattleRewardConfigs.ExpPerDmgDealt, 1).Parse<int>();
                    var rewardExp2 = Math.Max(localDmgRecv * ModMain.ModObj.InGameCustomSettings.BattleRewardConfigs.ExpPerDmgRecv, 1).Parse<int>();
                    player.AddExp(rewardExp1 + rewardExp2);
                    DebugHelper.WriteLine($"BattleRewardEvent: +{rewardExp1 + rewardExp2}exp");
                }
                IsEnd = true;
            }
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            var attackUnitData = e?.hitData?.attackUnit?.data?.TryCast<UnitDataHuman>();
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.worldUnitData.unit.IsPlayer())
            {
                var dieUnit = e?.unit?.data?.TryCast<UnitDataHuman>();
                if (dieUnit?.worldUnitData?.unit != null && !IsPlayerDie)
                {
                    if (g.world.battle.data.isRealBattle)
                    {
                        var drainLife = dieUnit.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Life) / (50 + (g.game.data.dataWorld.data.gameLevel.Parse<int>() * 25));
                        g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Life, drainLife);
                    }

                    var insight = g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
                    var bBestBasis = dieUnit.worldUnitData.unit.GetBestBasis();
                    if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, insight / 10) &&
                        dieUnit.worldUnitData.unit.GetProperty<int>(bBestBasis) - g.world.playerUnit.GetProperty<int>(bBestBasis) >= 6 * g.world.playerUnit.GetGradeLvl())
                    {
                        g.world.playerUnit.AddProperty<int>(bBestBasis, 1);
                    }
                }
            }
        }
    }
}
