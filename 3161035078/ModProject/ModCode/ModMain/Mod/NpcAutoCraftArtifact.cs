using EGameTypeData;
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
    /// NPCs occasionally craft an artifact of their grade with random stats; the player's forged artifacts get the same
    /// attribute bonus. The craft skill is forge + (talismans + formations) / 2.
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
                    var skill = CraftSkill(wunit);
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
                            // higher levels with a better craft skill: the stop chance falls 1 % every 100 skill points
                            art.level = Random(1, 6, LevelStopRate(skill));
                            // the better the craft skill, the better the attributes: +1 % every 50 skill points
                            var bonus = AttrBonus(skill);
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

        /// <summary>
        /// Player forge: the made artifact gets +1 % on every attribute rate every 50 craft skill points (once per artifact).
        /// </summary>
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName != UIType.ArtifactShapeMadeSuccess.uiName)
                return;
            var ui = g.ui.GetUI<UIArtifactShapeMadeSuccess>(UIType.ArtifactShapeMadeSuccess);
            var prop = ui?.shapeProp;
            if (prop?.propsItem?.IsArtifact() == null || !BoostedSoleIds.Add(prop.soleID))
                return;
            var bonus = AttrBonus(CraftSkill(g.world.playerUnit));
            if (bonus <= 0)
                return;
            var art = prop.To<DataProps.PropsArtifact>();
            art.SetAttrrRate(art.hpRate + bonus, art.atkRate + bonus, art.defRate + bonus, art.soulCapcityRate + bonus, art.equipSpRate + bonus, false);
            ui.UpdateUI();
            DebugHelper.WriteLine($"NpcAutoCraftArtifact: player forged {prop.propsInfoBase.name}, attribute rates +{bonus} %");
        }

        /// <summary>Artifacts already boosted by the player forge bonus (this session; the success window opens once per artifact).</summary>
        private static readonly HashSet<string> BoostedSoleIds = new HashSet<string>();

        /// <summary>Craft skill of a unit: forge + (talismans + formations) / 2.</summary>
        public static int CraftSkill(WorldUnitBase wunit) => CraftSkill(
            wunit.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value,
            wunit.GetDynProperty(UnitDynPropertyEnum.Symbol).value,
            wunit.GetDynProperty(UnitDynPropertyEnum.Geomancy).value);

        /// <summary>Craft skill: forge + (talismans + formations) / 2, negatives counted as 0.</summary>
        public static int CraftSkill(int refineWeapon, int symbol, int geomancy) =>
            Math.Max(0, refineWeapon) + (Math.Max(0, symbol) + Math.Max(0, geomancy)) / 2;

        /// <summary>Mount skill of a unit: herbs + alchemy (beasts are raised on herbs and pills).</summary>
        public static int MountSkill(WorldUnitBase wunit) => MountSkill(
            wunit.GetDynProperty(UnitDynPropertyEnum.Herbal).value,
            wunit.GetDynProperty(UnitDynPropertyEnum.RefineElixir).value);

        /// <summary>Mount skill: herbs + alchemy, negatives counted as 0.</summary>
        public static int MountSkill(int herbal, int refineElixir) =>
            Math.Max(0, herbal) + Math.Max(0, refineElixir);

        /// <summary>Range (%) of each attribute rate of a crafted artifact, before the forge bonus.</summary>
        public const int ATTR_MIN = 70;
        public const int ATTR_MAX = 130;
        /// <summary>Craft skill points for +1 % on every attribute rate.</summary>
        public const int REFINE_PER_ATTR_PERCENT = 50;

        /// <summary>Attribute rate bonus (%) of a craft skill: +1 every 50 points (none below 0).</summary>
        public static int AttrBonus(int craftSkill) => Math.Max(0, craftSkill) / REFINE_PER_ATTR_PERCENT;

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
        /// <summary>Craft skill points that lower the stop chance of the level by 1 % (more high levels).</summary>
        public const int REFINE_PER_LEVEL_PERCENT = 100;

        /// <summary>Stop chance (%) of the level roll: 80 - 1 every 100 craft skill points, 0 .. 80.</summary>
        public static float LevelStopRate(int craftSkill) =>
            Math.Max(0f, Math.Min(STOP_RATE, STOP_RATE - Math.Max(0, craftSkill) / (float)REFINE_PER_LEVEL_PERCENT));

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
