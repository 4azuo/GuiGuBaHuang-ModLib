using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System.Collections.Generic;
using UnityEngine;
using static MOD_nE7UL2.Object.GameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MONST_STRONGER_EVENT)]
    public class MonstStrongerEvent : ModEvent
    {
        public static MonstStrongerEvent Instance { get; set; }
        public static _MonstStrongerConfigs Configs => ModMain.ModObj.GameSettings.MonstStrongerConfigs;
        //public static IDictionary<MonstType, float> AdditionalStts { get; set; } = new Dictionary<MonstType, float>
        //{
        //    [MonstType.Common] = 2.00f,
        //    [MonstType.Elite] = 0.20f,
        //    [MonstType.BOSS] = 0.02f,
        //    [MonstType.NPC] = 0.04f,
        //};

        public IDictionary<MonstType, int> KillCounter { get; set; } = new Dictionary<MonstType, int>();
        public IDictionary<MonstType, float> Additional { get; set; } = new Dictionary<MonstType, float>();

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

        public override void OnMonthly()
        {
            if (GameHelper.GetGameTotalMonth() % SMLocalConfigsEvent.Instance.Configs.GrowUpSpeed == 0)
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

        private string AddPotmonValue(string scale, float rate)
        {
            var s = scale.Split('|');
            var type = s[0].Parse<int>();
            var value = s[1].Parse<float>();
            return $"{s[0]}|{(value + (value * rate)).Parse<int>()}";
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            var monstData = e?.unit?.data?.TryCast<UnitDataMonst>();
            if (monstData != null && Configs.KillGrowRate.ContainsKey(monstData.monstType))
            {
                KillCounter[monstData.monstType]++;
            }
        }

        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);

            //DebugHelper.WriteLine("2");
            if (e.IsMonster())
            {
                //DebugHelper.WriteLine("3");
                var monstData = e?.data?.TryCast<UnitDataMonst>();
                var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
                var atk = monstData.attack.baseValue;
                var def = monstData.defense.baseValue;
                var mhp = monstData.maxHP.baseValue;

                //S&M
                //DebugHelper.WriteLine($"4: {atk}, {def}, {mhp}");
                monstData.attack.baseValue = SMLocalConfigsEvent.Instance.Calculate(atk, SMLocalConfigsEvent.Instance.Configs.AddAtkRate).Parse<int>();
                monstData.defense.baseValue = SMLocalConfigsEvent.Instance.Calculate(def, SMLocalConfigsEvent.Instance.Configs.AddDefRate).Parse<int>();
                monstData.maxHP.baseValue = SMLocalConfigsEvent.Instance.Calculate(mhp, SMLocalConfigsEvent.Instance.Configs.AddHpRate).Parse<int>();
                monstData.basisBlade.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisBlade.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisEarth.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisEarth.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisFinger.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisFinger.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisFire.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisFire.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisFist.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisFist.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisFroze.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisFroze.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisPalm.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisPalm.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisSpear.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisSpear.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisSword.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisSword.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisThunder.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisThunder.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisWind.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisWind.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();
                monstData.basisWood.baseValue = SMLocalConfigsEvent.Instance.Calculate(monstData.basisWood.value, SMLocalConfigsEvent.Instance.Configs.AddBasisRate).Parse<int>();

                //potmon
                if (e.IsPotmon())
                {
                    monstData.attack.baseValue += (ModBattleEvent.PlayerUnit.data.attack.value * 0.05f).Parse<int>();
                    monstData.defense.baseValue += (ModBattleEvent.PlayerUnit.data.defense.value * 0.01f).Parse<int>();
                    monstData.maxHP.baseValue += (ModBattleEvent.PlayerUnit.data.hp * 0.10f).Parse<int>();

                    monstData.basisBlade.baseValue += (ModBattleEvent.PlayerUnit.data.basisBlade.value * 0.01f).Parse<int>();
                    monstData.basisEarth.baseValue += (ModBattleEvent.PlayerUnit.data.basisEarth.value * 0.01f).Parse<int>();
                    monstData.basisFinger.baseValue += (ModBattleEvent.PlayerUnit.data.basisFinger.value * 0.01f).Parse<int>();
                    monstData.basisFire.baseValue += (ModBattleEvent.PlayerUnit.data.basisFire.value * 0.01f).Parse<int>();
                    monstData.basisFist.baseValue += (ModBattleEvent.PlayerUnit.data.basisFist.value * 0.01f).Parse<int>();
                    monstData.basisFroze.baseValue += (ModBattleEvent.PlayerUnit.data.basisFroze.value * 0.01f).Parse<int>();
                    monstData.basisPalm.baseValue += (ModBattleEvent.PlayerUnit.data.basisPalm.value * 0.01f).Parse<int>();
                    monstData.basisSpear.baseValue += (ModBattleEvent.PlayerUnit.data.basisSpear.value * 0.01f).Parse<int>();
                    monstData.basisSword.baseValue += (ModBattleEvent.PlayerUnit.data.basisSword.value * 0.01f).Parse<int>();
                    monstData.basisThunder.baseValue += (ModBattleEvent.PlayerUnit.data.basisThunder.value * 0.01f).Parse<int>();
                    monstData.basisWind.baseValue += (ModBattleEvent.PlayerUnit.data.basisWind.value * 0.01f).Parse<int>();
                    monstData.basisWood.baseValue += (ModBattleEvent.PlayerUnit.data.basisWood.value * 0.01f).Parse<int>();
                }

                //sect guardian
                if (e.IsSectGuardian())
                {
                    var battleSchool = g.world.build.GetBuild<MapBuildSchool>(g.world.battle.data.schoolID);
                    var strongestUnit = g.world.unit.GetUnitsInArea(battleSchool.gridData.areaBaseID, true).ToArray().GetStrongestWUnit();
                    if (strongestUnit != null)
                    {
                        monstData.attack.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue;
                        monstData.defense.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue;
                        monstData.maxHP.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue;

                        monstData.basisBlade.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisBlade).baseValue;
                        monstData.basisEarth.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisEarth).baseValue;
                        monstData.basisFinger.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisFinger).baseValue;
                        monstData.basisFire.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisFire).baseValue;
                        monstData.basisFist.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisFist).baseValue;
                        monstData.basisFroze.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisFroze).baseValue;
                        monstData.basisPalm.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisPalm).baseValue;
                        monstData.basisSpear.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisSpear).baseValue;
                        monstData.basisSword.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisSword).baseValue;
                        monstData.basisThunder.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisThunder).baseValue;
                        monstData.basisWind.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisWind).baseValue;
                        monstData.basisWood.baseValue += strongestUnit.GetDynProperty(UnitDynPropertyEnum.BasisWood).baseValue;
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
                //    monstData.attack.baseValue += (atk * r * Configs.AtkR).Parse<int>();
                //    monstData.defense.baseValue += (def * r * Configs.DefR).Parse<int>();
                //    monstData.maxHP.baseValue += (mhp * r * Configs.MHpR).Parse<int>();
                //}

                //area bonus
                //DebugHelper.WriteLine("7");
                var areaId = g.world.playerUnit.data.unitData.pointGridData.areaBaseID;
                if (Configs.AreaBonus.ContainsKey(areaId))
                {
                    var r = Configs.AreaBonus[areaId] * gameLvl;
                    monstData.attack.baseValue += (atk * r * Configs.AtkR).Parse<int>();
                    monstData.defense.baseValue += (def * r * Configs.DefR).Parse<int>();
                    monstData.maxHP.baseValue += (mhp * r * Configs.MHpR).Parse<int>();
                }

                //monster
                //DebugHelper.WriteLine("8");
                if (Configs.GrowRate.ContainsKey(monstData.monstType))
                {
                    //grow-rate & kill-rate
                    var r = ((Counter * Configs.GrowRate[monstData.monstType]) + (KillCounter[monstData.monstType] * Configs.KillGrowRate[monstData.monstType])) * monstData.grade.value * gameLvl;
                    monstData.attack.baseValue += (atk * r * Configs.AtkR).Parse<int>();
                    monstData.defense.baseValue += (def * r * Configs.DefR).Parse<int>();
                    monstData.maxHP.baseValue += (mhp * r * Configs.MHpR).Parse<int>();
                }

                //level oppressive
                //DebugHelper.WriteLine("9");
                if (Configs.PlayerAtk2Hp.ContainsKey(monstData.monstType))
                {
                    var r = Configs.PlayerAtk2Hp[monstData.monstType] * gameLvl;
                    monstData.maxHP.baseValue += (g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Attack).value * r).Parse<int>();
                }

                //rebirth
                //DebugHelper.WriteLine("10");
                var rebirthRatio = RebirthEvent.Instance.TotalGradeLvl * Configs.RebirthAffect;
                rebirthRatio *= gameLvl;
                monstData.attack.baseValue += (atk * rebirthRatio * Configs.AtkR).Parse<int>();
                monstData.defense.baseValue += (def * rebirthRatio * Configs.DefR).Parse<int>();
                monstData.maxHP.baseValue += (mhp * rebirthRatio * Configs.MHpR).Parse<int>();

                //basis
                //DebugHelper.WriteLine("11");
                monstData.basisBlade.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisEarth.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisFinger.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisFire.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisFist.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisFroze.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisPalm.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisSpear.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisSword.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisThunder.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisWind.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();
                monstData.basisWood.baseValue *= (monstData.grade.value * gameLvl).Parse<int>();

                //others
                //DebugHelper.WriteLine("12");
                //monstData.attack.baseValue += (??? / 100.00f * monstData.attack.baseValue).Parse<int>();
                var adjustDef1 = ((((monstData.basisFist.value + monstData.basisPalm.value + monstData.basisFinger.value) / 3.0f) / 1000.00f) * monstData.defense.baseValue).Parse<int>();
                var adjustDef2 = ((monstData.basisEarth.value / 1000.00f) * monstData.defense.baseValue).Parse<int>();
                monstData.defense.baseValue += adjustDef1 + adjustDef2;
                var adjustMs = (monstData.basisWind.value / 100.00f).Parse<int>();
                monstData.moveSpeed.baseValue += adjustMs;

                //heal fullhp
                //DebugHelper.WriteLine("13");
                e.data.unit.data.hp = e.data.unit.data.maxHP.value;
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