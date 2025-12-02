using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Helper;
using System;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.NPC_AUTO_CRAFT_ARTIFACT)]
    public class NpcAutoCraftArtifact : ModEvent
    {
        public static NpcAutoCraftArtifact Instance { get; set; }
        public static ConfItemPropsItem[] ArtifactConfs { get; set; }

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            ArtifactConfs = g.conf.itemProps._allConfList.ToArray().Where(x => x.IsArtifact() != null).ToArray();
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (!wunit.IsPlayer())
            {
                var wunitGrade = wunit.GetGradeLvl();
                var availableArtifacts = ArtifactConfs.Where(x => wunitGrade >= x.IsArtifact().initGrade).ToArray();
                if (availableArtifacts.Length > 0)
                {
                    var curArts = wunit.GetEquippedArtifacts();
                    var refine = wunit.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value;
                    var luck = wunit.GetDynProperty(UnitDynPropertyEnum.Luck).value;
                    if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, refine / 1000f + luck / 100f))
                    {
                        wunit.AddUnitProp(availableArtifacts[CommonTool.Random(0, availableArtifacts.Length - 1)].id, 1);
                        var addedArt = WUnitHelper.LastAddedItems.ToArray().FirstOrDefault();
                        if (addedArt != null)
                        {
                            var art = addedArt.To<DataProps.PropsArtifact>();
                            art.grade = Random(Math.Max(wunitGrade - 2, 1), wunitGrade);
                            art.level = Random(1, 6);
                            art.SetAttrrRate(
                                CommonTool.Random(70, 130),
                                CommonTool.Random(70, 130),
                                CommonTool.Random(70, 130),
                                CommonTool.Random(70, 130),
                                CommonTool.Random(70, 130),
                                false);
                        }
                    }
                }
            }
        }

        private int Random(int min, int max)
        {
            for (var i = min; i < max; i++)
            {
                if (CommonTool.Random(0f, 100f).IsBetween(0f, 80f))
                    return i;
            }
            return max;
        }
    }
}
