using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System.Collections.Generic;
using UnityEngine;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MONST_STRONGER_EVENT)]
    public class MonstStrongerEvent : ModEvent
    {
        public override int OrderIndex => 8010;

        public static _MonstStrongerConfigs Configs => ModMain.ModObj.InGameCustomSettings.MonstStrongerConfigs;
        public static IDictionary<MonstType, float> AdditionalStts { get; set; } = new Dictionary<MonstType, float>
        {
            [MonstType.Common] = 2.00f,
            [MonstType.Elite] = 0.20f,
            [MonstType.BOSS] = 0.02f,
            [MonstType.NPC] = 0.04f,
        };

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
            Counter++;
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

            DebugHelper.WriteLine("OnBattleUnitInto: 1");
            if ((ModMaster.ModObj?.InGameSettings?.LoadFirstMonth).Is(true) == 1)
                return;

            DebugHelper.WriteLine("OnBattleUnitInto: 2");
            if (e.IsMonster())
            {
                var monstData = e?.data?.TryCast<UnitDataMonst>();
                var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
                var atk = monstData.attack.baseValue;
                var def = monstData.defense.baseValue;
                var mhp = monstData.maxHP.baseValue;

                //S&M
                var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
                monstData.attack.baseValue = smConfigs.Calculate(atk, smConfigs.Configs.AddAtkRate).Parse<int>();
                monstData.defense.baseValue = smConfigs.Calculate(def, smConfigs.Configs.AddDefRate).Parse<int>();
                monstData.maxHP.baseValue = smConfigs.Calculate(mhp, smConfigs.Configs.AddHpRate).Parse<int>();
                monstData.basisBlade.baseValue = smConfigs.Calculate(monstData.basisBlade.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisEarth.baseValue = smConfigs.Calculate(monstData.basisEarth.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisFinger.baseValue = smConfigs.Calculate(monstData.basisFinger.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisFire.baseValue = smConfigs.Calculate(monstData.basisFire.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisFist.baseValue = smConfigs.Calculate(monstData.basisFist.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisFroze.baseValue = smConfigs.Calculate(monstData.basisFroze.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisPalm.baseValue = smConfigs.Calculate(monstData.basisPalm.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisSpear.baseValue = smConfigs.Calculate(monstData.basisSpear.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisSword.baseValue = smConfigs.Calculate(monstData.basisSword.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisThunder.baseValue = smConfigs.Calculate(monstData.basisThunder.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisWind.baseValue = smConfigs.Calculate(monstData.basisWind.value, smConfigs.Configs.AddBasisRate).Parse<int>();
                monstData.basisWood.baseValue = smConfigs.Calculate(monstData.basisWood.value, smConfigs.Configs.AddBasisRate).Parse<int>();

                atk = monstData.attack.baseValue;
                def = monstData.defense.baseValue;
                mhp = monstData.maxHP.baseValue;

                //additional
                if (AdditionalStts.ContainsKey(monstData.monstType))
                {
                    var r = Additional[monstData.monstType];
                    if (monstData.monstType == MonstType.BOSS)
                        r += Additional[MonstType.Elite] / 10f;
                    r *= gameLvl;
                    monstData.attack.baseValue += (atk * r * Configs.AtkR).Parse<int>();
                    monstData.defense.baseValue += (def * r * Configs.DefR).Parse<int>();
                    monstData.maxHP.baseValue += (mhp * r * Configs.MHpR).Parse<int>();
                }

                //area bonus
                var areaId = g.world.playerUnit.data.unitData.pointGridData.areaBaseID;
                if (Configs.AreaBonus.ContainsKey(areaId))
                {
                    var r = Configs.AreaBonus[areaId] * gameLvl;
                    monstData.attack.baseValue += (atk * r * Configs.AtkR).Parse<int>();
                    monstData.defense.baseValue += (def * r * Configs.DefR).Parse<int>();
                    monstData.maxHP.baseValue += (mhp * r * Configs.MHpR).Parse<int>();
                }

                //monster
                if (Configs.GrowRate.ContainsKey(monstData.monstType))
                {
                    //grow-rate & kill-rate
                    var r = ((Counter * Configs.GrowRate[monstData.monstType]) + (KillCounter[monstData.monstType] * Configs.KillGrowRate[monstData.monstType])) * monstData.grade.value * gameLvl;
                    monstData.attack.baseValue += (atk * r * Configs.AtkR).Parse<int>();
                    monstData.defense.baseValue += (def * r * Configs.DefR).Parse<int>();
                    monstData.maxHP.baseValue += (mhp * r * Configs.MHpR).Parse<int>();
                }

                //level oppressive
                if (Configs.Player2Rate.ContainsKey(monstData.monstType))
                {
                    var r = Configs.Player2Rate[monstData.monstType] * gameLvl;
                    monstData.attack.baseValue += (g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Attack).value * r * Configs.AtkR).Parse<int>();
                    monstData.defense.baseValue += (g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Defense).value * r * Configs.DefR).Parse<int>();
                    monstData.maxHP.baseValue += (g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.HpMax).value * r * Configs.MHpR).Parse<int>();
                }

                //rebirth
                var rebirthRatio = EventHelper.GetEvent<RebirthEvent>(ModConst.REBIRTH_EVENT).TotalGradeLvl * Configs.RebirthAffect;
                rebirthRatio *= gameLvl;
                monstData.attack.baseValue += (atk * rebirthRatio * Configs.AtkR).Parse<int>();
                monstData.defense.baseValue += (def * rebirthRatio * Configs.DefR).Parse<int>();
                monstData.maxHP.baseValue += (mhp * rebirthRatio * Configs.MHpR).Parse<int>();

                //basis
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
                //monstData.attack.baseValue += (??? / 100.00f * monstData.attack.baseValue).Parse<int>();
                var adjustDef1 = ((((monstData.basisFist.value + monstData.basisPalm.value + monstData.basisFinger.value) / 3.0f) / 1000.00f) * monstData.defense.baseValue).Parse<int>();
                var adjustDef2 = ((monstData.basisEarth.value / 1000.00f) * monstData.defense.baseValue).Parse<int>();
                monstData.defense.baseValue += adjustDef1 + adjustDef2;
                var adjustMs = (monstData.basisWind.value / 100.00f).Parse<int>();
                monstData.moveSpeed.baseValue += adjustMs;

                //heal fullhp
                e.data.unit.data.hp = e.data.unit.data.maxHP.value;
            }
        }

        //BattleModifyEventに参照してください。
        //public override void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        //{
        //    base.OnBattleUnitHitDynIntHandler(e);
        //}
    }
}