using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SPECIAL_MONST_EVENT)]
    public class SpecialMonsterEvent : ModEvent
    {
        public static SpecialMonsterEvent Instance { get; set; }

        public const float MONST_SHIELD_CHANCE = 0.60f;
        public const float MONST_EXPLODE_CHANCE = 0.40f;
        public const float MONST_MULTIPLY_CHANCE = 0.20f;

        public const float EXPLODE_RADIUS = 2.5f;
        public const int MULTIPLY = 1;

        public const string EXPLODE_EFX = @"Effect\Battle\Skill\baiyuanshizhen";
        public const string MULTIPLY_EFX = @"Effect\Battle\Skill\changhenfu_bao";

        [JsonIgnore]
        public List<UnitCtrlBase> MultipliedUnits { get; } = new List<UnitCtrlBase>();

        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);

            if (e.IsMonster() && !e.IsPotmon() && !e.IsSummoned())
            {
                var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
                var monstData = e?.data?.TryCast<UnitDataMonst>();

                if (monstData.monstType == MonstType.Common || monstData.monstType == MonstType.Elite)
                {
                    if (CommonTool.Random(0.0f, 100.0f).IsBetween(0.0f, SMLocalConfigsEvent.Instance.Calculate(MONST_SHIELD_CHANCE * monstData.grade.value * gameLvl, SMLocalConfigsEvent.Instance.Configs.AddSpecialMonsterRate).Parse<float>()))
                    {
                        BattleManashieldEvent.ShieldUp(monstData.unit, monstData.maxHP.value, int.MaxValue);
                    }
                }
                else if (SMLocalConfigsEvent.Instance.Configs.BossHasShield && (monstData.monstType == MonstType.BOSS || monstData.monstType == MonstType.NPC))
                {
                    BattleManashieldEvent.ShieldUp(monstData.unit, monstData.maxHP.value, int.MaxValue);
                }
            }
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            var dieUnit = e.unit;
            if (dieUnit.IsMonster() && !dieUnit.IsPotmon() && !dieUnit.IsSummoned())
            {
                var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
                var monstData = dieUnit?.data?.TryCast<UnitDataMonst>();

                if (monstData.monstType == MonstType.Common || monstData.monstType == MonstType.Elite)
                {
                    if (CommonTool.Random(0.0f, 100.0f).IsBetween(0.0f, SMLocalConfigsEvent.Instance.Calculate(MONST_EXPLODE_CHANCE * monstData.grade.value * gameLvl, SMLocalConfigsEvent.Instance.Configs.AddSpecialMonsterRate).Parse<float>()))
                    {
                        ModBattleEvent.SceneBattle.effect.CreateSync(EXPLODE_EFX, dieUnit.transform.position, 3f);
                        foreach (var cunit in dieUnit.FindNearCEnemys(EXPLODE_RADIUS))
                        {
                            MartialTool.HitDanagePow(new MartialTool.HitData(dieUnit, null, 0, 1, dieUnit.data.attack.baseValue), cunit);
                        }
                    }

                    if (!MultipliedUnits.Contains(dieUnit) &&
                        CommonTool.Random(0.0f, 100.0f).IsBetween(0.0f, SMLocalConfigsEvent.Instance.Calculate(MONST_MULTIPLY_CHANCE * monstData.grade.value * gameLvl, SMLocalConfigsEvent.Instance.Configs.AddSpecialMonsterRate).Parse<float>()))
                    {
                        ModBattleEvent.SceneBattle.effect.CreateSync(MULTIPLY_EFX, dieUnit.transform.position, 3f);
                        var count = CommonTool.Random(1, MULTIPLY * gameLvl);
                        for (var i = 0; i < count; i++)
                            MultipliedUnits.Add(SceneType.battle.unit.CreateUnitMonst(monstData.unitAttrItem.id, monstData.unit.posiDown.position, monstData.unitType));
                    }
                }
            }
        }

        public override void OnBattleEnd(BattleEnd e)
        {
            base.OnBattleEnd(e);
            MultipliedUnits.Clear();
        }
    }
}
