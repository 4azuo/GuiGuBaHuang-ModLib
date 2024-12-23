using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.EXPLODE_MONST_EVENT)]
    public class ExplodeMonsterEvent : ModEvent
    {
        public const float MONST_EXPLODE_CHANCE = 0.50f;
        public const string EXPLODE_EFX = @"Effect\Battle\Skill\baiyuanshizhen";

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            var dieUnit = e.unit;
            if (dieUnit.IsMonster())
            {
                var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
                var monstData = dieUnit?.data?.TryCast<UnitDataMonst>();
                var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);

                if (monstData.monstType == MonstType.Common || monstData.monstType == MonstType.Elite)
                {
                    if (CommonTool.Random(0.0f, 100.0f).IsBetween(0.0f, smConfigs.Calculate(MONST_EXPLODE_CHANCE * monstData.grade.value * gameLvl, smConfigs.Configs.AddSpecialMonsterRate).Parse<float>()))
                    {
                        ModBattleEvent.SceneBattle.effect.CreateSync(EXPLODE_EFX, dieUnit.transform.position, 3f);
                        foreach (var cunit in dieUnit.FindNearCEnemys(3))
                        {
                            MartialTool.HitDanagePow(new MartialTool.HitData(dieUnit, null, 0, 1, dieUnit.data.attack.baseValue), cunit);
                        }
                    }
                }
            }
        }
    }
}
