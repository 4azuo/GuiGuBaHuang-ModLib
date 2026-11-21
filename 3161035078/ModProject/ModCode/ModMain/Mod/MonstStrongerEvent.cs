using EBattleTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using System.Collections.Generic;
using UnityEngine;
using ModLib.Helper;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Monsters get stronger over time, with kills, area, game level, rebirths and S&amp;M rates; summons/guardians scale with their owner.
    /// </summary>
    [Cache(ModConst.MONST_STRONGER_EVENT)]
    public class MonstStrongerEvent : ModEvent
    {
        public static MonstStrongerEvent Instance { get; set; }
        public static MonstStrongerConfig Configs => MonstStrongerConfig.Conf;
        //public static IDictionary<MonstType, float> AdditionalStts { get; set; } = new Dictionary<MonstType, float>
        //{
        //    [MonstType.Common] = 2.00f,
        //    [MonstType.Elite] = 0.20f,
        //    [MonstType.BOSS] = 0.02f,
        //    [MonstType.NPC] = 0.04f,
        //};

        public IDictionary<MonstType, int> KillCounter { get; set; } = new Dictionary<MonstType, int>();
        public IDictionary<MonstType, float> Additional { get; set; } = new Dictionary<MonstType, float>();

        /// <summary>
        /// Initializes counters.
        /// </summary>
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            if (!KillCounter.ContainsKey(MonstType.Common)) KillCounter.Add(MonstType.Common, 0);
            if (!KillCounter.ContainsKey(MonstType.Elite)) KillCounter.Add(MonstType.Elite, 0);
            if (!KillCounter.ContainsKey(MonstType.BOSS)) KillCounter.Add(MonstType.BOSS, 0);
            if (!KillCounter.ContainsKey(MonstType.NPC)) KillCounter.Add(MonstType.NPC, 0);
            if (!Additional.ContainsKey(MonstType.Common)) Additional.Add(MonstType.Common, 0);
            if (!Additional.ContainsKey(MonstType.Elite)) Additional.Add(MonstType.Elite, 0);
            if (!Additional.ContainsKey(MonstType.BOSS)) Additional.Add(MonstType.BOSS, 0);
            if (!Additional.ContainsKey(MonstType.NPC)) Additional.Add(MonstType.NPC, 0);
        }

        public int Counter { get; set; } = 0;

        /// <summary>
        /// Raises the growth counter and pet stats every GrowUpSpeed months.
        /// </summary>
        public override void OnMonthly()
        {
            var ticks = ModLib.Mod.ModMaster.Ticks(SMLocalConfigsEvent.Instance.Configs.GrowUpSpeed);
            for (var t = 0; t < ticks; t++)
            {
                Counter++;
                foreach (var potmonData in g.data.dataWorld.data.devilDemonData.potmonData.potMonList)
                {
                    potmonData.monstPropertyScale.atk = AddPotmonValue(potmonData.monstPropertyScale.atk, 0.01f);
                    potmonData.monstPropertyScale.def = AddPotmonValue(potmonData.monstPropertyScale.def, 0.01f);
                    potmonData.monstPropertyScale.hp = AddPotmonValue(potmonData.monstPropertyScale.hp, 0.01f);
                }
            }
        }

        /// <summary>
        /// Raises a "type|value" pet scale by rate.
        /// </summary>
        private string AddPotmonValue(string scale, float rate)
        {
            var s = scale.Split('|');
            var type = s[0].Parse<int>();
            var value = s[1].Parse<float>();
            return $"{type}|{((double)value + (value * rate)).ToIntClamp()}";
        }

        /// <summary>
        /// Counts kills by monster type.
        /// </summary>
        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            var monstData = e?.unit?.data?.TryCast<UnitDataMonst>();
            if (monstData != null && Configs.KillGrowRate.ContainsKey(monstData.monstType))
            {
                KillCounter[monstData.monstType]++;
            }
        }

        /// <summary>
        /// Applies all monster stat increases when a monster enters battle.
        /// </summary>
        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);

            //DebugHelper.WriteLine("2");
            if (e.IsMonster())
            {
                //DebugHelper.WriteLine("3");
                var monstData = e?.data?.TryCast<UnitDataMonst>();
                var gameLvl = GameHelper.GetGameLevel().Parse<int>();
                var atk = monstData.attack.baseValue;
                var def = monstData.defense.baseValue;
                var mhp = monstData.maxHP.baseValue;

                //S&M
                //DebugHelper.WriteLine($"4: {atk}, {def}, {mhp}");
                monstData.attack.baseValue = SMLocalConfigsEvent.Instance.Calculate(atk, SMLocalConfigsEvent.Instance.Configs.AddAtkRate).ToIntClamp();
                monstData.defense.baseValue = SMLocalConfigsEvent.Instance.Calculate(def, SMLocalConfigsEvent.Instance.Configs.AddDefRate).ToIntClamp();
                monstData.maxHP.baseValue = SMLocalConfigsEvent.Instance.Calculate(mhp, SMLocalConfigsEvent.Instance.Configs.AddHpRate).ToIntClamp();
                monstData.basisBlade.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisBlade.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisEarth.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisEarth.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisFinger.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisFinger.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisFire.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisFire.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisFist.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisFist.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisFroze.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisFroze.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisPalm.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisPalm.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisSpear.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisSpear.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisSword.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisSword.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisThunder.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisThunder.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisWind.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisWind.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();
                monstData.basisWood.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisWood.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).ToIntClamp();

                //potmon
                if (e.IsSummoned())
                {
                    var summoner = e.GetOriginSummoner();
                    var petbuffRatio = 1d + ((summoner != null && summoner.IsWorldUnit()) ?
                        (CustomRefineEvent.GetCustomAdjValue(summoner.GetWorldUnit(), AdjTypeEnum.SummonPower) / 100d) :
                        0d);

                    monstData.attack.baseValue = ValueHelper.SafeAddInt(monstData.attack.baseValue, ModBattleEvent.PlayerUnit.data.attack.value * (Configs.PlayerAffectSummon["Atk"] * petbuffRatio));
                    monstData.defense.baseValue = ValueHelper.SafeAddInt(monstData.defense.baseValue, ModBattleEvent.PlayerUnit.data.defense.value * (Configs.PlayerAffectSummon["Def"] * petbuffRatio));
                    monstData.maxHP.baseValue = ValueHelper.SafeAddInt(monstData.maxHP.baseValue, ModBattleEvent.PlayerUnit.data.hp * (Configs.PlayerAffectSummon["MHp"] * petbuffRatio));

                    var rate = Configs.PlayerAffectSummon["Bas"] * petbuffRatio;
                    monstData.basisBlade.baseValue = ValueHelper.SafeAddInt(monstData.basisBlade.baseValue, ModBattleEvent.PlayerUnit.data.basisBlade.value * rate);
                    monstData.basisEarth.baseValue = ValueHelper.SafeAddInt(monstData.basisEarth.baseValue, ModBattleEvent.PlayerUnit.data.basisEarth.value * rate);
                    monstData.basisFinger.baseValue = ValueHelper.SafeAddInt(monstData.basisFinger.baseValue, ModBattleEvent.PlayerUnit.data.basisFinger.value * rate);
                    monstData.basisFire.baseValue = ValueHelper.SafeAddInt(monstData.basisFire.baseValue, ModBattleEvent.PlayerUnit.data.basisFire.value * rate);
                    monstData.basisFist.baseValue = ValueHelper.SafeAddInt(monstData.basisFist.baseValue, ModBattleEvent.PlayerUnit.data.basisFist.value * rate);
                    monstData.basisFroze.baseValue = ValueHelper.SafeAddInt(monstData.basisFroze.baseValue, ModBattleEvent.PlayerUnit.data.basisFroze.value * rate);
                    monstData.basisPalm.baseValue = ValueHelper.SafeAddInt(monstData.basisPalm.baseValue, ModBattleEvent.PlayerUnit.data.basisPalm.value * rate);
                    monstData.basisSpear.baseValue = ValueHelper.SafeAddInt(monstData.basisSpear.baseValue, ModBattleEvent.PlayerUnit.data.basisSpear.value * rate);
                    monstData.basisSword.baseValue = ValueHelper.SafeAddInt(monstData.basisSword.baseValue, ModBattleEvent.PlayerUnit.data.basisSword.value * rate);
                    monstData.basisThunder.baseValue = ValueHelper.SafeAddInt(monstData.basisThunder.baseValue, ModBattleEvent.PlayerUnit.data.basisThunder.value * rate);
                    monstData.basisWind.baseValue = ValueHelper.SafeAddInt(monstData.basisWind.baseValue, ModBattleEvent.PlayerUnit.data.basisWind.value * rate);
                    monstData.basisWood.baseValue = ValueHelper.SafeAddInt(monstData.basisWood.baseValue, ModBattleEvent.PlayerUnit.data.basisWood.value * rate);
                }

                //sect guardian
                if (e.IsSectGuardian())
                {
                    var battleSchool = g.world.build.GetBuild<MapBuildSchool>(g.world.battle.data.schoolID);
                    var strongestUnit = g.world.unit.GetUnitsInArea(battleSchool.gridData.areaBaseID, true).ToArray().GetStrongestWUnit();
                    if (strongestUnit != null)
                    {
                        monstData.attack.baseValue = ValueHelper.SafeAddInt(monstData.attack.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue);
                        monstData.defense.baseValue = ValueHelper.SafeAddInt(monstData.defense.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue);
                        monstData.maxHP.baseValue = ValueHelper.SafeAddInt(monstData.maxHP.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue);

                        monstData.basisBlade.baseValue = ValueHelper.SafeAddInt(monstData.basisBlade.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisBlade).baseValue);
                        monstData.basisEarth.baseValue = ValueHelper.SafeAddInt(monstData.basisEarth.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisEarth).baseValue);
                        monstData.basisFinger.baseValue = ValueHelper.SafeAddInt(monstData.basisFinger.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisFinger).baseValue);
                        monstData.basisFire.baseValue = ValueHelper.SafeAddInt(monstData.basisFire.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisFire).baseValue);
                        monstData.basisFist.baseValue = ValueHelper.SafeAddInt(monstData.basisFist.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisFist).baseValue);
                        monstData.basisFroze.baseValue = ValueHelper.SafeAddInt(monstData.basisFroze.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisFroze).baseValue);
                        monstData.basisPalm.baseValue = ValueHelper.SafeAddInt(monstData.basisPalm.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisPalm).baseValue);
                        monstData.basisSpear.baseValue = ValueHelper.SafeAddInt(monstData.basisSpear.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisSpear).baseValue);
                        monstData.basisSword.baseValue = ValueHelper.SafeAddInt(monstData.basisSword.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisSword).baseValue);
                        monstData.basisThunder.baseValue = ValueHelper.SafeAddInt(monstData.basisThunder.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisThunder).baseValue);
                        monstData.basisWind.baseValue = ValueHelper.SafeAddInt(monstData.basisWind.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisWind).baseValue);
                        monstData.basisWood.baseValue = ValueHelper.SafeAddInt(monstData.basisWood.baseValue, strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisWood).baseValue);
                    }
                }

                atk = monstData.attack.baseValue;
                def = monstData.defense.baseValue;
                mhp = monstData.maxHP.baseValue;
                //DebugHelper.WriteLine($"5: {atk}, {def}, {mhp}");

                //additional
                //DebugHelper.WriteLine("6");
                //if (AdditionalStts.ContainsKey(monstData.monstType))
                //{
                //    var r = Additional[monstData.monstType];
                //    if (monstData.monstType == MonstType.BOSS)
                //        r += Additional[MonstType.Elite] / 10f;
                //    r *= gameLvl;
                //    monstData.attack.baseValue = ValueHelper.SafeAddInt(monstData.attack.baseValue, atk * r * Configs.AtkR);
                //    monstData.defense.baseValue = ValueHelper.SafeAddInt(monstData.defense.baseValue, def * r * Configs.DefR);
                //    monstData.maxHP.baseValue = ValueHelper.SafeAddInt(monstData.maxHP.baseValue, mhp * r * Configs.MHpR);
                //}

                //area bonus
                //DebugHelper.WriteLine("7");
                var areaId = g.world.playerUnit.GetUnitPosAreaId();
                if (Configs.AreaBonus.ContainsKey(areaId))
                {
                    var r = Configs.AreaBonus[areaId] * gameLvl;
                    monstData.attack.baseValue = ValueHelper.SafeAddInt(monstData.attack.baseValue, atk * r * Configs.AtkR);
                    monstData.defense.baseValue = ValueHelper.SafeAddInt(monstData.defense.baseValue, def * r * Configs.DefR);
                    monstData.maxHP.baseValue = ValueHelper.SafeAddInt(monstData.maxHP.baseValue, mhp * r * Configs.MHpR);
                }

                //monster
                //DebugHelper.WriteLine("8");
                if (Configs.GrowRate.ContainsKey(monstData.monstType))
                {
                    //grow-rate & kill-rate
                    var r = ((Counter * Configs.GrowRate[monstData.monstType]) + (KillCounter[monstData.monstType] * Configs.KillGrowRate[monstData.monstType])) * monstData.grade.value * gameLvl;
                    monstData.attack.baseValue = ValueHelper.SafeAddInt(monstData.attack.baseValue, atk * r * Configs.AtkR);
                    monstData.defense.baseValue = ValueHelper.SafeAddInt(monstData.defense.baseValue, def * r * Configs.DefR);
                    monstData.maxHP.baseValue = ValueHelper.SafeAddInt(monstData.maxHP.baseValue, mhp * r * Configs.MHpR);
                }

                //monster properties depend on player properties
                //DebugHelper.WriteLine("9");
                if (SMLocalConfigsEvent.Instance.Configs.MonsterPropertiesDependOnPlayerProperties && Configs.PlayerAtk2Hp.ContainsKey(monstData.monstType))
                {
                    var r = Configs.PlayerAtk2Hp[monstData.monstType] * gameLvl;
                    monstData.maxHP.baseValue = ValueHelper.SafeAddInt(monstData.maxHP.baseValue, g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Attack).value * r);
                }

                //rebirth
                //DebugHelper.WriteLine("10");
                var rebirthRatio = RebirthEvent.Instance.TotalGradeLvl * Configs.RebirthAffect * gameLvl;
                monstData.attack.baseValue = ValueHelper.SafeAddInt(monstData.attack.baseValue, atk * rebirthRatio * Configs.AtkR);
                monstData.defense.baseValue = ValueHelper.SafeAddInt(monstData.defense.baseValue, def * rebirthRatio * Configs.DefR);
                monstData.maxHP.baseValue = ValueHelper.SafeAddInt(monstData.maxHP.baseValue, mhp * rebirthRatio * Configs.MHpR);

                //basis
                //DebugHelper.WriteLine("11");
                monstData.basisBlade.baseValue = ValueHelper.SafeMulInt(monstData.basisBlade.baseValue, monstData.grade.value * gameLvl);
                monstData.basisEarth.baseValue = ValueHelper.SafeMulInt(monstData.basisEarth.baseValue, monstData.grade.value * gameLvl);
                monstData.basisFinger.baseValue = ValueHelper.SafeMulInt(monstData.basisFinger.baseValue, monstData.grade.value * gameLvl);
                monstData.basisFire.baseValue = ValueHelper.SafeMulInt(monstData.basisFire.baseValue, monstData.grade.value * gameLvl);
                monstData.basisFist.baseValue = ValueHelper.SafeMulInt(monstData.basisFist.baseValue, monstData.grade.value * gameLvl);
                monstData.basisFroze.baseValue = ValueHelper.SafeMulInt(monstData.basisFroze.baseValue, monstData.grade.value * gameLvl);
                monstData.basisPalm.baseValue = ValueHelper.SafeMulInt(monstData.basisPalm.baseValue, monstData.grade.value * gameLvl);
                monstData.basisSpear.baseValue = ValueHelper.SafeMulInt(monstData.basisSpear.baseValue, monstData.grade.value * gameLvl);
                monstData.basisSword.baseValue = ValueHelper.SafeMulInt(monstData.basisSword.baseValue, monstData.grade.value * gameLvl);
                monstData.basisThunder.baseValue = ValueHelper.SafeMulInt(monstData.basisThunder.baseValue, monstData.grade.value * gameLvl);
                monstData.basisWind.baseValue = ValueHelper.SafeMulInt(monstData.basisWind.baseValue, monstData.grade.value * gameLvl);
                monstData.basisWood.baseValue = ValueHelper.SafeMulInt(monstData.basisWood.baseValue, monstData.grade.value * gameLvl);

                //others
                //DebugHelper.WriteLine("12");
                //monstData.attack.baseValue = ValueHelper.SafeAddInt(monstData.attack.baseValue, ??? / 100.00f * monstData.attack.baseValue);
                var adjustDef1 = (((((monstData.basisFist.value + monstData.basisPalm.value + monstData.basisFinger.value) / 3.0f) / 1000.00f) * monstData.defense.baseValue)).ToIntClamp();
                var adjustDef2 = (((monstData.basisEarth.value / 1000.00f) * monstData.defense.baseValue)).ToIntClamp();
                monstData.defense.baseValue = ValueHelper.SafeAddInt(monstData.defense.baseValue, (double)adjustDef1 + adjustDef2);
                var adjustMs = ((monstData.basisWind.value / 100.00f)).ToIntClamp();
                monstData.moveSpeed.baseValue = ValueHelper.SafeAddInt(monstData.moveSpeed.baseValue, adjustMs);

                //heal fullhp
                //DebugHelper.WriteLine("14");
                e.data.hp = e.data.maxHP.value;
            }
            //DebugHelper.Save();
        }

        //BattleModifyEventに参照してください。
        //public override void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        //{
        //    base.OnBattleUnitHitDynIntHandler(e);
        //}
    }
}