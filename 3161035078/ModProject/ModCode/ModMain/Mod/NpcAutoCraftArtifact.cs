using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// NPCs occasionally craft an artifact of their grade with random stats.
    /// </summary>
    [Cache(ModConst.NPC_AUTO_CRAFT_ARTIFACT)]
    public class NpcAutoCraftArtifact : ModEvent
    {
        public static NpcAutoCraftArtifact Instance { get; set; }
        public static ConfItemPropsItem[] ArtifactConfs { get; set; }

        /// <summary>
        /// Caches the artifact item configs.
        /// </summary>
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            ArtifactConfs = g.conf.itemProps._allConfList.ToArray().Where(x => x.IsArtifact() != null).ToArray();
        }

        /// <summary>
        /// Chance (refine skill, luck) to give the NPC a random artifact with random grade/level/attributes.
        /// </summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (!wunit.IsPlayer())
            {
                var wunitGrade = wunit.GetGradeLvl();
                var availableArtifacts = ArtifactConfs.Where(x => wunitGrade >= x.IsArtifact().initGrade).ToArray();
                if (availableArtifacts.Length > 0)
                {
                    var refine = wunit.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value;
                    var luck = wunit.GetDynProperty(UnitDynPropertyEnum.Luck).value;
                    if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, ModLib.Mod.ModMaster.ChanceOver(refine / 1000f + luck / 100f, ModLib.Mod.ModMaster.MonthsPassed)))
                    {
                        // never an artifact of a kind and grade the NPC already owns (equipped or in the bag): it used to
                        // craft without any limit, its bag filling up over the years
                        var grade = Random(Math.Max(wunitGrade - 2, 1), wunitGrade);
                        var owned = OwnedArtifactKeys(wunit);
                        var candidates = availableArtifacts.Where(x => !owned.Contains(ArtifactKey(x.id, grade))).ToArray();
                        if (candidates.Length == 0)
                            return;
                        wunit.AddUnitProp(candidates[CommonTool.Random(0, candidates.Length - 1)].id, 1);
                        var addedArt = WUnitHelper.LastAddedItems.ToArray().FirstOrDefault();
                        if (addedArt != null)
                        {
                            var art = addedArt.To<DataProps.PropsArtifact>();
                            art.grade = grade;
                            // higher levels with a better forge skill: the stop chance falls 1 % every 100 RefineWeapon
                            art.level = Random(1, 6, LevelStopRate(refine));
                            // the better the forge skill, the better the attributes: +1 % every 50 RefineWeapon
                            var bonus = AttrBonus(refine);
                            art.SetAttrrRate(
                                CommonTool.Random(ATTR_MIN + bonus, ATTR_MAX + bonus),
                                CommonTool.Random(ATTR_MIN + bonus, ATTR_MAX + bonus),
                                CommonTool.Random(ATTR_MIN + bonus, ATTR_MAX + bonus),
                                CommonTool.Random(ATTR_MIN + bonus, ATTR_MAX + bonus),
                                CommonTool.Random(ATTR_MIN + bonus, ATTR_MAX + bonus),
                                false);
                        }
                    }
                }
            }
        }

        /// <summary>Range (%) of each attribute rate of a crafted artifact, before the forge bonus.</summary>
        public const int ATTR_MIN = 70;
        public const int ATTR_MAX = 130;
        /// <summary>RefineWeapon points for +1 % on every attribute rate.</summary>
        public const int REFINE_PER_ATTR_PERCENT = 50;

        /// <summary>Attribute rate bonus (%) of a forge skill: +1 every 50 RefineWeapon (none below 0).</summary>
        public static int AttrBonus(int refineWeapon) => Math.Max(0, refineWeapon) / REFINE_PER_ATTR_PERCENT;

        /// <summary>Key of an artifact kind (item id) at a grade.</summary>
        public static string ArtifactKey(int propsId, int grade) => $"{propsId}_{grade}";

        /// <summary>Kinds and grades of the artifacts a unit owns (equipped or in the bag).</summary>
        public static HashSet<string> OwnedArtifactKeys(WorldUnitBase wunit)
        {
            var rs = new HashSet<string>();
            foreach (var p in wunit.GetUnitProps().Concat(wunit.GetEquippedArtifacts()))
            {
                if (p?.propsItem?.IsArtifact() == null)
                    continue;
                rs.Add(ArtifactKey(p.propsID, p.To<DataProps.PropsArtifact>().grade));
            }
            return rs;
        }

        /// <summary>Chance (%) to stop at each step of the biased random (lower = higher values).</summary>
        public const float STOP_RATE = 80f;
        /// <summary>RefineWeapon points that lower the stop chance of the level by 1 % (more high levels).</summary>
        public const int REFINE_PER_LEVEL_PERCENT = 100;

        /// <summary>Stop chance (%) of the level roll: 80 - 1 every 100 RefineWeapon, 0 .. 80.</summary>
        public static float LevelStopRate(int refineWeapon) =>
            Math.Max(0f, Math.Min(STOP_RATE, STOP_RATE - Math.Max(0, refineWeapon) / (float)REFINE_PER_LEVEL_PERCENT));

        /// <summary>
        /// Biased random between min and max (stopRate % chance to stop at each step, 80 by default).
        /// </summary>
        private int Random(int min, int max, float stopRate = STOP_RATE)
        {
            for (var i = min; i < max; i++)
            {
                if (CommonTool.Random(0f, 100f).IsBetween(0f, stopRate))
                    return i;
            }
            return max;
        }
    }
}
