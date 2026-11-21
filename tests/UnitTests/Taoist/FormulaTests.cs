using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOD_nE7UL2.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnitTests.Taoist
{
    /// <summary>Pure Taoist formulas (no game state).</summary>
    [TestClass]
    public class FormulaTests
    {
        [TestMethod, Description("ExpertEvent.GetExpertNeedExp(level, grade, itemLevel): exp needed per expertise level, doubles each level")]
        public void ExpertNeedExp_DoublesEachLevel()
        {
            Check.Eq(0, ExpertEvent.GetExpertNeedExp(0, 5, 5), "level 0");
            Check.Eq(1000, ExpertEvent.GetExpertNeedExp(1, 1, 0), "level 1, grade 1, item level 0");
            Check.Eq(1200, ExpertEvent.GetExpertNeedExp(1, 1, 1), "level 1, grade 1, item level 1");
            Check.Eq(4000, ExpertEvent.GetExpertNeedExp(1, 2, 5), "level 1, grade 2, item level 5");
            for (var lvl = 1; lvl < 10; lvl++)
                Check.Eq(2d * ExpertEvent.GetExpertNeedExp(lvl, 3, 2), ExpertEvent.GetExpertNeedExp(lvl + 1, 3, 2), 1, $"level {lvl + 1} = 2 x level {lvl} (grade 3, item level 2)");
        }

        [TestMethod, Description("NpcAutoCraftArtifact.CraftSkill(forge, talismans, formations) = forge + (talismans + formations) / 2; MountSkill(herbs, alchemy) = herbs + alchemy; negatives count as 0")]
        public void CraftAndMountSkill()
        {
            Check.Eq(200, NpcAutoCraftArtifact.CraftSkill(200, 0, 0), "forge only");
            Check.Eq(300, NpcAutoCraftArtifact.CraftSkill(200, 100, 100), "forge 200, talismans 100, formations 100");
            Check.Eq(50, NpcAutoCraftArtifact.CraftSkill(0, 101, 0), "talismans 101 alone (integer half)");
            Check.Eq(100, NpcAutoCraftArtifact.CraftSkill(-50, 100, 100), "negative forge counts as 0");
            Check.Eq(6, NpcAutoCraftArtifact.AttrBonus(NpcAutoCraftArtifact.CraftSkill(200, 100, 100)), "attribute bonus of skill 300");
            Check.Eq(300, NpcAutoCraftArtifact.MountSkill(200, 100), "herbs 200, alchemy 100");
            Check.Eq(0, NpcAutoCraftArtifact.MountSkill(-10, -10), "negatives");
        }

        [TestMethod, Description("SMLocalConfigsEvent.Calculate(value, rate) = value x (1 + rate), not below 0")]
        public void SMCalculate_AppliesRate()
        {
            var sm = new SMLocalConfigsEvent();
            Check.Eq(150d, sm.Calculate(100, 0.5), "100 with rate +0.5");
            Check.Eq(100d, sm.Calculate(100, 0), "100 with rate 0");
            Check.Eq(50d, sm.Calculate(100, -0.5), "100 with rate -0.5");
            Check.Eq(0d, sm.Calculate(100, -1), "100 with rate -1");
        }

        [TestMethod, Description("PriceHelper.UpPrice(price, grade, level, ratio, min, max): price x grade ratio x level ratio x ratio, clamped")]
        public void UpPrice_UsesGradeAndLevelRatios()
        {
            Check.Info($"GRADE_RATIO = {Check.Fmt(PriceHelper.GRADE_RATIO)}");
            Check.Info($"LEVEL_RATIO = {Check.Fmt(PriceHelper.LEVEL_RATIO)}");
            Check.Eq(100, PriceHelper.UpPrice(100, 1, 1), "100, grade 1, level 1");
            Check.Eq(400, PriceHelper.UpPrice(100, 10, 1), "100, grade 10 (ratio 4.0)");
            Check.Eq(180, PriceHelper.UpPrice(100, 1, 6), "100, level 6 (ratio 1.8)");
            Check.Eq(PriceHelper.UpPrice(100, 10, 6), PriceHelper.UpPrice(100, 99, 99), "grade/level 99 clamped to 10/6");
            Check.Eq(PriceHelper.UpPrice(100, 1, 1), PriceHelper.UpPrice(100, 0, 0), "grade/level 0 clamped to 1");
            Check.Eq(200, PriceHelper.UpPrice(100, 1, 1, 2f), "custom ratio 2");
            Check.Eq(150, PriceHelper.UpPrice(100, 10, 6, 1f, -1, 150), "max 150");
            Check.Eq(500, PriceHelper.UpPrice(10, 1, 1, 1f, 500), "min 500");
        }

        [TestMethod, Description("PriceHelper ratio tables: one ratio per grade/level, never decreasing; item type ratios >= 1")]
        public void PriceRatios_AreSortedAndComplete()
        {
            Check.Eq(10, PriceHelper.GRADE_RATIO.Count, "GRADE_RATIO count (one per grade)");
            Check.Eq(6, PriceHelper.LEVEL_RATIO.Count, "LEVEL_RATIO count (one per item level)");
            for (var i = 1; i < PriceHelper.GRADE_RATIO.Count; i++)
                Check.True(PriceHelper.GRADE_RATIO[i] >= PriceHelper.GRADE_RATIO[i - 1], $"grade ratio {i + 1} >= grade ratio {i}", $"{PriceHelper.GRADE_RATIO[i - 1]} -> {PriceHelper.GRADE_RATIO[i]}");
            for (var i = 1; i < PriceHelper.LEVEL_RATIO.Count; i++)
                Check.True(PriceHelper.LEVEL_RATIO[i] >= PriceHelper.LEVEL_RATIO[i - 1], $"level ratio {i + 1} >= level ratio {i}", $"{PriceHelper.LEVEL_RATIO[i - 1]} -> {PriceHelper.LEVEL_RATIO[i]}");
            foreach (var r in PriceHelper.ITEM_TYPE_RATIO)
                Check.True(r.Value >= 1f, $"{r.Key} ratio >= 1", r.Value);
        }

        [TestMethod, Description("BattleModifyEvent.BlockRatio: block ratio per grade never decreases; null unit -> 0")]
        public void BlockRatio_GrowsWithGrade()
        {
            var t = BattleModifyEvent.BlockRatio;
            Check.Eq(10, t.Count, "BlockRatio entries (one per grade)");
            for (var g = 2; g <= 10; g++)
                Check.True(t[g] >= t[g - 1], $"grade {g} >= grade {g - 1}", $"{t[g - 1]} -> {t[g]}");
            Check.Eq(0f, BattleModifyEvent.GetBlockRatio(null), "GetBlockRatio(null)");
            Check.Eq(0f, BattleModifyEvent.GetBlockMpCost(null), "GetBlockMpCost(null)");
        }

        [TestMethod, Description("BodyCulEvent: realms / layers of the 15 levels, exp per level, bottlenecks, breakthrough chance, grade gate, hit exp")]
        public void BodyCul_Formulas()
        {
            Check.Eq(0, BodyCulEvent.Realm(0), "level 0: no realm");
            Check.Eq(1, BodyCulEvent.Realm(1), "level 1: realm 1");
            Check.Eq(1, BodyCulEvent.Realm(3), "level 3: realm 1");
            Check.Eq(2, BodyCulEvent.Realm(4), "level 4: realm 2");
            Check.Eq(5, BodyCulEvent.Realm(15), "level 15: realm 5");
            Check.Eq(3, BodyCulEvent.Layer(3), "level 3: layer 3");
            Check.Eq(1, BodyCulEvent.Layer(13), "level 13: layer 1");
            Check.Eq(200L, BodyCulEvent.NeedExp(1), "exp of level 1");
            Check.Eq(45000L, BodyCulEvent.NeedExp(15), "exp of level 15");
            Check.True(BodyCulEvent.IsRealmEnd(3) && BodyCulEvent.IsRealmEnd(12), "levels 3 and 12 end a realm", null);
            Check.True(!BodyCulEvent.IsRealmEnd(15) && !BodyCulEvent.IsRealmEnd(4), "level 15 (top) and 4 do not", null);
            Check.Eq(70f, BodyCulEvent.BreakChance(3), "chance from realm 1");
            Check.Eq(40f, BodyCulEvent.BreakChance(12), "chance from realm 4");
            Check.Eq(1, BodyCulEvent.RealmGrade(1), "realm 1 needs grade 1");
            Check.Eq(3, BodyCulEvent.RealmGrade(4), "realm 2 needs grade 3");
            Check.Eq(9, BodyCulEvent.RealmGrade(13), "realm 5 needs grade 9");
            Check.Eq(50f, BodyCulEvent.HitExp(500, 1000), "half the max HP");
            Check.Eq(100f, BodyCulEvent.HitExp(5000, 1000), "at most one max HP");
            Check.Eq(0f, BodyCulEvent.HitExp(10, 0), "no max HP");
            Check.Eq(40L, BodyCulEvent.BathExp(1), "bath exp of level 1");
            Check.Eq(2.5f, BodyCulEvent.Share(5, true), "injured: half");
            // cultivation: exp needed x(1 + 0.1 x realm), Qi by realm, breakthrough bonuses by realm
            Check.Eq(1f, BodyCulEvent.ExpRate(0), "not a body refiner: x1.00");
            Check.Eq(1.1f, BodyCulEvent.ExpRate(3), "Copper Skin: x1.10");
            Check.Eq(1.5f, BodyCulEvent.ExpRate(15), "Undying Body: x1.50");
            Check.Eq(0.9f, BodyCulEvent.QiRateOf(1), "Qi of Copper Skin");
            Check.Eq(0.85f, BodyCulEvent.QiRateOf(4), "Qi of Iron Bone");
            Check.Eq(0.7f, BodyCulEvent.QiRateOf(15), "Qi of Undying Body");
            Check.Eq(1f, BodyCulEvent.QiRateOf(0), "Qi of a unit without body refining");
            Check.Eq(1000, BodyCulEvent.KeptExp(1100, 2), "1100 exp at x1.10: 1000 kept");
            Check.Eq(1000, BodyCulEvent.KeptExp(1500, 13), "1500 exp at x1.50: 1000 kept");
            Check.Eq(50, BodyCulEvent.BreakBonus(1000, BodyCulEvent.BREAK_HP_PER_REALM, 1), "HP +5 % of the gain (realm 1)");
            Check.Eq(150, BodyCulEvent.BreakBonus(1000, BodyCulEvent.BREAK_DEF_PER_REALM, 13), "defense +15 % (realm 5)");
            Check.Eq(40, BodyCulEvent.BreakBonus(1000, BodyCulEvent.BREAK_ATK_PER_REALM, 10), "attack +4 % (realm 4)");
            Check.Eq(0, BodyCulEvent.BreakBonus(-50, BodyCulEvent.BREAK_HP_PER_REALM, 10), "no bonus for a loss");
        }

        [TestMethod, Description("BodyCulEvent: tempering places (exp a month, months at a bottleneck), sects that teach body refining (hash, training, start rate)")]
        public void BodyCul_PlacesAndSects()
        {
            Check.Eq(8L, BodyCulEvent.PlaceExp(BodyCulEvent.Place.Waterfall, 1), "waterfall, level 1: 200 / 24");
            Check.Eq(2500L, BodyCulEvent.PlaceExp(BodyCulEvent.Place.Thunder, 10), "thunder pool, level 10: 20000 / 8");
            Check.Eq(96L, BodyCulEvent.PlaceTotalExp(BodyCulEvent.Place.Waterfall, 1, 12), "a year at the waterfall from level 1");
            Check.Eq(900L, BodyCulEvent.PlaceTotalExp(BodyCulEvent.Place.Waterfall, 3, 12), "a year at a realm end: the level stays");
            Check.True(BodyCulEvent.PlaceTotalExp(BodyCulEvent.Place.Lava, 4, 60) > 60 * BodyCulEvent.PlaceExp(BodyCulEvent.Place.Lava, 4), "5 years of lava: the month exp grows with the level", null);
            Check.Eq(1, BodyCulEvent.PlaceRealm(BodyCulEvent.Place.Waterfall), "waterfall from Copper Skin");
            Check.Eq(4, BodyCulEvent.PlaceRealm(BodyCulEvent.Place.Thunder), "thunder pool from Golden Body");
            // sects by style: the third of the sects (of 3 members or more) whose members fight the most with bare hands
            Check.Eq(0.5, BodyCulEvent.HandShare(50, 100), "hand share");
            Check.Eq(0.0, BodyCulEvent.HandShare(10, 0), "no basis: no share");
            var members = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, double>>();
            void Add(string sect, double share, int n) { for (var i = 0; i < n; i++) members.Add(new System.Collections.Generic.KeyValuePair<string, double>(sect, share)); }
            Add("fist", 0.6, 3); Add("sword", 0.1, 5); Add("palm", 0.4, 4); Add("small", 0.9, 2); Add("blade", 0.2, 3); Add("spear", 0.15, 3); Add(null, 0.9, 5);
            var sects = BodyCulEvent.PickBodySects(members);
            Check.Eq("fist,palm", string.Join(",", sects), "5 sects of 3+ members: the first 2 by hand share (a sect of 2 has no style)");
            Check.Eq(0, BodyCulEvent.PickBodySects(new System.Collections.Generic.KeyValuePair<string, double>[0]).Count, "no sect: none");
            Check.True(!BodyCulEvent.IsBodySect(null) && !BodyCulEvent.IsBodySect(""), "no sect: not a body sect", null);
            Check.Eq(10, BodyCulEvent.TrainExp(true, false, 1), "player training a month");
            Check.Eq(180, BodyCulEvent.TrainExp(false, true, 3), "NPC of a body sect, 3 months: 30 x 2 x 3");
            Check.Eq(2f, BodyCulEvent.StartRate(true), "NPC of a body sect takes up the path at 2 % a month");
            // NPC tribulation: 45 + 40 x toughness + 3 x merit level, 15-90
            Check.Eq(45f, BodyCulEvent.TribulationChance(0f, 0), "tribulation chance of a soft body");
            Check.Eq(90f, BodyCulEvent.TribulationChance(1f, 6), "at most 90 %");
            Check.Eq(36f, BodyCulEvent.TribulationChance(0f, -3), "karma: lower");
        }

        [TestMethod, Description("ArtisanEvent: rank exp, craft exp, usual level, masterpieces, accidents, exam chance, the first rank, the bag gain")]
        public void Artisan_Formulas()
        {
            Check.Eq(100L, ArtisanEvent.NeedExp(0), "rank 0 -> 1");
            Check.Eq(6400L, ArtisanEvent.NeedExp(3), "rank 3 -> 4");
            Check.Eq(25600L, ArtisanEvent.NeedExp(4), "rank 4 -> 5");
            Check.Eq(10L, ArtisanEvent.CraftExp(1, 1), "one item of level 1");
            Check.Eq(360L, ArtisanEvent.CraftExp(3, 16), "16 items of level 3: 10 x 9 x 4");
            Check.Eq(1, ArtisanEvent.UsualLevel(1), "grade 1: level 1");
            Check.Eq(5, ArtisanEvent.UsualLevel(10), "grade 10: level 5");
            Check.True(ArtisanEvent.IsMasterpiece(2, 1) && !ArtisanEvent.IsMasterpiece(1, 2), "a level above the usual one is a masterpiece", null);
            Check.True(ArtisanEvent.CanBlowUp(3, 1) && !ArtisanEvent.CanBlowUp(2, 1), "two levels above: the furnace may blow up", null);
            Check.Eq(30L, ArtisanEvent.MasterpieceExp(1000, 2, 1, 0), "3 % of the phase a level above");
            Check.Eq(90L, ArtisanEvent.MasterpieceExp(1000, 3, 1, 2), "2 levels above, rank 2: 6 % x 1.5");
            Check.Eq(0L, ArtisanEvent.MasterpieceExp(1000, 1, 1, 5), "usual level: none");
            Check.Eq(40f, ArtisanEvent.ExamChance(200, 1, false), "exam: 30 + 200 / 20");
            Check.Eq(90f, ArtisanEvent.ExamChance(5000, 1, true), "at most 90 %");
            Check.Eq(10f, ArtisanEvent.ExamChance(0, 5, false), "at least 10 %");
            var d = new ArtisanEvent.CraftData();
            Check.Eq(1, ArtisanEvent.AddExp(d, 150), "the first rank comes by itself");
            Check.Eq(0L, d.Exp, "exp of the new rank");
            ArtisanEvent.AddExp(d, 1000);
            Check.Eq(400L, d.Exp, "rank 1: the exp stops at the exam");
            Check.True(ArtisanEvent.ExamReady(d), "ready for the exam", null);
            var before = new System.Collections.Generic.Dictionary<int, int> { { 1, 5 }, { 2, 3 } };
            var after = new System.Collections.Generic.Dictionary<int, int> { { 1, 2 }, { 2, 7 }, { 3, 1 } };
            var gained = ArtisanEvent.Gained(before, after);
            Check.Eq("2:4,3:1", string.Join(",", gained.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}")), "items that came into the bag (materials used are not counted)");
            // a Saint: essence, return to simplicity; commissions, apprentices, market
            var saint = new ArtisanEvent.CraftData { Rank = 5 };
            ArtisanEvent.AddExp(saint, 25600 * 3 + 100);
            Check.Eq(3, ArtisanEvent.EssenceSteps(saint.Essence), "a Saint's exp becomes essence: 3 steps");
            Check.True(!ArtisanEvent.CanReturn(saint), "3 steps: not yet", null);
            saint.Essence = 25600L * 40;
            Check.True(ArtisanEvent.CanReturn(saint), "40 steps: the return to simplicity", null);
            Check.True(!ArtisanEvent.CanReturn(new ArtisanEvent.CraftData { Rank = 4, Essence = 25600L * 99 }), "not a Saint: no return", null);
            Check.Eq(0.04, Math.Round(ArtisanEvent.StatRate(40), 4), "40 essence steps: +4 % (the returns are destinies)");
            saint.Returned = true;
            Check.True(!ArtisanEvent.CanReturn(saint), "a craft returns once", null);
            Check.Eq(420201005, ArtisanEvent.ReturnDestiny(ArtisanEvent.Craft.Herbal), "destiny of the return of Herbs");
            Check.True(ArtisanEvent.IsGathering(ArtisanEvent.Craft.Mining) && !ArtisanEvent.IsGathering(ArtisanEvent.Craft.Forge), "herbs and mining are gathering", null);
            Check.True(ArtisanEvent.Blocking(new[] { "MapMain", "TownMarketPill" }) && !ArtisanEvent.Blocking(new[] { "MapMain" }), "a market window blocks the gathering", null);
            Check.Eq(10f, ArtisanEvent.SellRateOf(5), "a Saint sells +10 %");
            Check.Eq(0, ArtisanEvent.MaxApprentices(2), "a Craftsman takes no apprentice");
            Check.Eq(3, ArtisanEvent.MaxApprentices(5), "a Saint takes 3");
            Check.Eq(2400, ArtisanEvent.CommissionReward(3, 200), "commission: 4 x level 3 x cost 200");
        }

        [TestMethod, Description("DaoHeartEvent and SpiritSenseEvent: levels, max SP / MP, heart demons, breakthroughs, risks, ambushes, scan range; the gathering read from the cells")]
        public void Mind_Formulas()
        {
            Check.Eq(1, DaoHeartEvent.LevelOf(DaoHeartEvent.START), "a unit starts Steady");
            Check.Eq(0, DaoHeartEvent.LevelOf(99), "below 100: Wavering");
            Check.Eq(5, DaoHeartEvent.LevelOf(2000), "2,000: Heart of Heaven");
            Check.Eq(600L, DaoHeartEvent.NextOf(400), "next of 400");
            Check.Eq(0.1, Math.Round(DaoHeartEvent.SpRateOf(5, 0), 4), "Dao heart 5: max SP +10 %");
            Check.Eq(0.13, Math.Round(DaoHeartEvent.SpRateOf(5, 3), 4), "3 essence steps: SP +3 % more");
            Check.Eq(0.1, Math.Round(DaoHeartEvent.MpRateOf(5), 4), "Dao heart 5: max MP +10 %");
            Check.Eq(90f, DaoHeartEvent.HeartDemonWinOf(5), "heart demon beaten 90 % at the top");
            Check.Eq(50f, DaoHeartEvent.HeartDemonWinOf(0), "50 % when wavering");
            Check.Eq(10f, DaoHeartEvent.BreakBonusOf(5), "body breakthrough +10");
            Check.Eq(0.6, Math.Round(DaoHeartEvent.RiskRateOf(5), 4), "risks -40 %");
            Check.Eq(1, SpiritSenseEvent.LevelOf(200), "200: Spirit Perception");
            Check.Eq(5, SpiritSenseEvent.LevelOf(50000), "50,000: Sense of Heaven");
            Check.Eq(0.1, Math.Round(SpiritSenseEvent.MpRateOf(5), 4), "sense 5: max MP +10 %");
            Check.Eq(0.2, Math.Round(SpiritSenseEvent.SpRateOf(5, 0), 4), "sense 5: max SP +20 %");
            Check.Eq(0.22, Math.Round(SpiritSenseEvent.SpRateOf(5, 2), 4), "2 essence steps: SP +2 % more");
            Check.Eq(60f, SpiritSenseEvent.AmbushChanceOf(3), "ambush sensed 60 % at level 3");
            Check.Eq(90f, SpiritSenseEvent.AmbushChanceOf(5), "at most 90 %");
            Check.Eq(0.25, Math.Round(SpiritSenseEvent.HeartDemonRateOf(5), 4), "heart demons x0.25 at the top");
            Check.Eq(6, SpiritSenseEvent.MonthlyExp(80), "monthly exp: 2 + 80 / 20");
            Check.Eq(15, SpiritSenseEvent.ScanRange(3), "scan range: 6 + 3 a level");
            // the gathering: an herb counts when the cells lost herbs, an ore when they lost ores
            Check.True(ArtisanEvent.GatheredFromCell(ArtisanEvent.Craft.Herbal, 100, 90, 50, 50), "herbs of the cells down: gathered", null);
            Check.True(!ArtisanEvent.GatheredFromCell(ArtisanEvent.Craft.Herbal, 100, 100, 50, 40), "only the ores went down: an herb is not gathered", null);
            Check.True(ArtisanEvent.GatheredFromCell(ArtisanEvent.Craft.Mining, 100, 100, 50, 40), "ores of the cells down: gathered", null);
            Check.True(!ArtisanEvent.Blocking(new[] { "MapMain", "GetReward" }), "the reward window of the gathering does not block", null);
            // mine veins and herb valleys
            Check.Eq(24, ArtisanEvent.SpotItems(12, 0), "a year at a spot: 24 items");
            Check.Eq(48, ArtisanEvent.SpotItems(12, 4), "a Grandmaster: +100 %");
            Check.Eq(0, ArtisanEvent.Direction(0, 0, 0, 5), "north");
            Check.Eq(2, ArtisanEvent.Direction(0, 0, 5, 0), "east");
            Check.Eq(5, ArtisanEvent.Direction(0, 0, -5, -5), "south-west");
        }

        [TestMethod, Description("MeritEvent: levels and karma, next level, bonuses, essence, Luck, lightning, intimacy, kills")]
        public void Merit_Formulas()
        {
            Check.Eq(0, MeritEvent.LevelOf(0), "0 points: no merit");
            Check.Eq(0, MeritEvent.LevelOf(99), "99: no merit");
            Check.Eq(1, MeritEvent.LevelOf(100), "100: Good Karma");
            Check.Eq(4, MeritEvent.LevelOf(6000), "6,000: Golden Light");
            Check.Eq(6, MeritEvent.LevelOf(40000), "40,000: Boundless Merit");
            Check.Eq(0, MeritEvent.LevelOf(-99), "-99: no karma yet");
            Check.Eq(-1, MeritEvent.LevelOf(-100), "-100: Sinful");
            Check.Eq(-3, MeritEvent.LevelOf(-5000), "-5,000: Damned");
            Check.Eq(500L, MeritEvent.NextOf(100), "next of level 1");
            Check.Eq(-100L, MeritEvent.NextOf(-600), "karma -2: back to -1 at -100");
            Check.Eq(0.06, Math.Round(MeritEvent.AtkRate(6, 0), 4), "attack +6 % at the top");
            Check.Eq(0.07, Math.Round(MeritEvent.AtkRate(6, 2), 4), "essence 2 steps: +1 %");
            Check.Eq(0.09, Math.Round(MeritEvent.DefRate(6, 0), 4), "defense +9 % at the top");
            Check.Eq(0.06, Math.Round(MeritEvent.AtkRate(-3, 0), 4), "karma: attack +6 %");
            Check.Eq(-0.06, Math.Round(MeritEvent.DefRate(-3, 0), 4), "karma: defense -6 %");
            Check.Eq(3, MeritEvent.LuckOf(6, 0), "Luck +3 at the top");
            Check.Eq(5, MeritEvent.LuckOf(6, 10), "10 essence steps: Luck +2 more");
            Check.Eq(-2, MeritEvent.LuckOf(-2, 0), "karma: Luck -2");
            Check.Eq(0.7, Math.Round(MeritEvent.LightningRateOf(6), 4), "lightning -30 % at the top");
            Check.Eq(1.3, Math.Round(MeritEvent.LightningRateOf(-3), 4), "lightning +30 % for heavy karma");
            Check.Eq(8, MeritEvent.IntimOf(4, true), "righteous NPC: +2 a level");
            Check.Eq(-4, MeritEvent.IntimOf(4, false), "evil NPC: -1 a level");
            Check.Eq(4, MeritEvent.IntimOf(-2, false), "karma: evil NPC likes it");
            Check.Eq(0, MeritEvent.IntimOf(0, true), "no merit: nothing");
            Check.Eq(10, MeritEvent.KillMerit(true, 5, 0), "beast of grade 5");
            Check.Eq(15, MeritEvent.KillMerit(false, 3, -1), "evil cultivator of grade 3");
            Check.Eq(-60, MeritEvent.KillMerit(false, 3, 1), "righteous cultivator of grade 3");
            Check.Eq(2.5, MeritEvent.EssenceSteps(25000), "essence steps");
            // a flat part for the low grades: attack 15 at level 3: 3 % rounds to 0, +3 flat
            Check.Eq(3, MeritEvent.AtkBonus(15, 3, 0), "level 3, attack 15: +3");
            Check.Eq(33, MeritEvent.AtkBonus(1000, 3, 0), "level 3, attack 1,000: +30 +3");
            Check.Eq(-63, MeritEvent.DefBonus(1000, -3, 0), "karma -3, defense 1,000: -60 -3");
            Check.Eq(66, MeritEvent.AtkBonus(1000, -3, 0), "karma -3, attack 1,000: +60 +6");
            // the world: town wars, gifts and revenge, town growth, the massacre
            Check.Eq(15, MeritEvent.TownWarMerit(true, 5), "defending a town: +3 x grade");
            Check.Eq(-15, MeritEvent.TownWarMerit(false, 5), "attacking a town: -3 x grade");
            Check.Eq(0f, MeritEvent.GiftChance(1), "no gift at level 1");
            Check.Eq(5f, MeritEvent.GiftChance(6), "gifts: 1 % a level above 1");
            Check.Eq(3f, MeritEvent.RevengeChance(-3), "revenge: 1.5 % a karma level below -1");
            Check.Eq(0f, MeritEvent.RevengeChance(-1), "no revenge at -1");
            Check.Eq(1.12, Math.Round(MeritEvent.TownPopRate(4, true), 4), "town of a merit 4 house: +12 %");
            Check.Eq(1.0, Math.Round(MeritEvent.TownPopRate(4, false), 4), "no house: no change");
            Check.True(MeritEvent.Defends(0, 5, 5, false, false) && MeritEvent.Defends(1, 4, 5, false, false), "a neutral or righteous cultivator of grade >= player - 1 defends", null);
            Check.True(!MeritEvent.Defends(-1, 9, 5, false, false) && !MeritEvent.Defends(1, 3, 5, false, false) && !MeritEvent.Defends(1, 9, 5, true, false), "evil, weak or family: flees", null);
            Check.Eq(8, MeritEvent.GuardCount(3), "guards of a palace level 3");
            Check.Eq(50L, MeritEvent.Survivors(5000), "1 % of the mortals survive");
            Check.Eq(1L, MeritEvent.Survivors(10), "at least one");
        }

        [TestMethod, Description("EstateEvent: base population, budget / tax effects, population move, mortal tax, plots, prices, house tax")]
        public void Estate_Formulas()
        {
            Check.Eq(10000L, EstateEvent.BasePopulation(false, 1), "town of area 1");
            Check.Eq(30000L, EstateEvent.BasePopulation(false, 20), "town: at most 30,000");
            Check.Eq(40000L, EstateEvent.BasePopulation(true, 1), "city of area 1");
            Check.Eq(100000L, EstateEvent.BasePopulation(true, 20), "city: at most 100,000");
            Check.Eq(5000L, EstateEvent.BudgetBonus(10000, 10, 10000), "budget 10,000 / base tax 10 x 5");
            Check.Eq(10000L, EstateEvent.BudgetBonus(10000000, 10, 10000), "budget bonus at most the base");
            Check.Eq(1.0f, EstateEvent.TaxFactor(1f), 0.0001, "tax rate 1");
            Check.Eq(0.03f, EstateEvent.TaxFactor(10f), 0.0001, "tax rate 10: at least 0.03");
            // the mortal tax (rate x factor) is best at a rate about 2, not at the highest rates
            Check.True(2f * EstateEvent.TaxFactor(2f) > 1f * EstateEvent.TaxFactor(1f) && 2f * EstateEvent.TaxFactor(2f) > 4f * EstateEvent.TaxFactor(4f) && 2f * EstateEvent.TaxFactor(2f) > 10f * EstateEvent.TaxFactor(10f),
                "mortal tax best near rate 2", $"{EstateEvent.TaxFactor(1f)} {2 * EstateEvent.TaxFactor(2f)} {4 * EstateEvent.TaxFactor(4f)} {10 * EstateEvent.TaxFactor(10f)}");
            Check.Eq(1.15f, EstateEvent.TaxFactor(0.4f), "tax rate 0.4: at most 1.15");
            Check.Eq(10600L, EstateEvent.TargetPopulation(10000, 0, 1f, 2), "base x 1.0 + 2 house levels x 300");
            Check.Eq(18000L, EstateEvent.TargetPopulation(10000, 0, 1f, 0, 5), "palace level 5: +80 %");
            Check.Eq(12400L, EstateEvent.TargetPopulation(10000, 0, 1f, 0, 1, 3), "3 town buildings: +24 %");
            Check.Eq(11000L, EstateEvent.NextPopulation(10000, 20000, 1, 5), "5 town buildings: 10 % of the gap a month");
            Check.Eq(10500L, EstateEvent.NextPopulation(10000, 20000, 1), "5 % of the gap a month");
            Check.True(EstateEvent.NextPopulation(10000, 20000, 12) < 20000, "never past the target", EstateEvent.NextPopulation(10000, 20000, 12));
            Check.Eq(100L, EstateEvent.MortalTax(10000, 10), "10,000 mortals x 10 / 1000");
            var market = EstateEvent.Prosperity('M', false, 0, 3);
            var outskirts = EstateEvent.Prosperity('O', true, 0, 3);
            Check.True(market > outskirts, "the market is busier than the outskirts", $"{market} / {outskirts}");
            Check.Eq(41, EstateEvent.MortalPlotsTarget(82, 10000, 10000), "half the plots at the base population");
            Check.Eq(12, EstateEvent.MortalPlotsTarget(82, 0, 10000), "at least 15 %");
            Check.Eq(50, EstateEvent.BasePriceTax(1, false), "house price tax, area 1: 100 x 1 x 0.5");
            Check.Eq(500, EstateEvent.BasePriceTax(10, false), "area 10: x5 (100 x 10 x 0.5)");
            Check.Eq(1000, EstateEvent.BasePriceTax(10, true), "a city: x2");
            Check.Eq(15200L, EstateEvent.BaseLandPrice(10, 'M', 90), "market land: price tax x 400 x 2.0 x 1.9");
            Check.Eq(2300L, EstateEvent.BaseLandPrice(10, 'O', 15), "outskirts land: price tax x 400 x 0.5 x 1.15");
            Check.Eq(12000L, EstateEvent.BaseBuildCost(10, 1), "hut: price tax x 1,200");
            Check.Eq(384000L, EstateEvent.BaseBuildCost(10, 6), "immortal palace: x 2^5");
            Check.Eq(1, EstateEvent.RequiredGrade(2), "tiled house: any grade");
            Check.Eq(5, EstateEvent.RequiredGrade(6), "immortal palace: grade 5");
            Check.Eq(60L, EstateEvent.HouseTax(10, 6, 'R'), "residential tax: 6 base taxes at level 6");
            Check.Eq(30L, EstateEvent.HouseTax(10, 2, 'M'), "market tax: 1.5 x 2.0 base taxes at level 2");
            Check.Eq(5L, EstateEvent.HouseTax(10, 1, 'O'), "outskirts tax: half");
            Check.Eq(70000L, EstateEvent.BasePalaceCost(10, 2), "palace level 2: base tax x 7,000");
            Check.Eq(172226L, EstateEvent.BasePalaceCost(10, 5), "palace level 5: x 1.35^3");
            // prices the player can pay (his money is an int), whatever the inflation
            Check.Eq(39000000000L, EstateEvent.Payable(39000000000L), "an immortal palace of a big city at x100 inflation: paid with spirit notes");
            Check.Eq(1000000000000L, EstateEvent.Payable(5000000000000L), "at most 1,000,000,000,000");
            // rooms (B2)
            Check.Eq(1f, EstateEvent.WallDefenseRate(0), "no wall");
            Check.Eq(1.10f, EstateEvent.WallDefenseRate(1), 0.0001, "wall level 1: +10 %");
            Check.Eq(1.30f, EstateEvent.WallDefenseRate(5), 0.0001, "wall level 5: +10 % + 4 x 5 %");
            Check.Eq(6, EstateEvent.FootprintSize(6), "an immortal palace takes 6 plots");
            Check.Eq(1, EstateEvent.FootprintSize(2), "a tiled house takes 1 plot");
            Check.Eq(3, EstateEvent.RoomSlots(3), "a courtyard house has 3 rooms");
            Check.Eq(15000L, EstateEvent.BaseRoomCost(10, 1), "room level 1: price tax x 1,500");
            Check.Eq(60000L, EstateEvent.BaseRoomCost(10, 3), "room level 3: x 4");
            Check.Eq(1.45, EstateEvent.QiRateOfArray(3), 0.0001, "array level 3: Qi +45 %");
            Check.Eq(0.7, EstateEvent.UpkeepRateOfHall(3), 0.0001, "hall level 3: upkeep -30 %");
            Check.Eq(1f, EstateEvent.UpkeepRateOfHall(0), "no hall");
            Check.Eq(120, EstateEvent.QuietExp(2000, 3, 1), "quiet room level 3: 6 % of a phase of 2,000");
        }

        [TestMethod, Description("ModConf/estate_configs.json: valid, the same as the defaults in the code; broken values fixed")]
        public void Estate_Config()
        {
            var path = Path.Combine(TestSetup.RepoRoot, "3161035078", "ModProject", "ModConf", EstateConfig.FILE);
            var file = EstateConfig.Parse(File.ReadAllText(path));
            Check.True(Newtonsoft.Json.JsonConvert.SerializeObject(file) == Newtonsoft.Json.JsonConvert.SerializeObject(EstateConfig.Parse(null)),
                "the file holds the defaults of the code (change both)", null);
            var broken = EstateConfig.Parse("{ \"Houses\": [ { \"Sprite\": \"X/Y\", \"Width\": 9 } ], \"Map\": { \"TownSize\": [ 3 ] }, \"Districts\": { \"M\": { \"Price\": 3 } } }");
            Check.Eq(EstateEvent.MAX_LEVEL, broken.Houses.Count, "missing house levels: the defaults");
            Check.Eq(4, broken.Houses[0].Width, "a house at most 4 plots wide");
            Check.Eq(24, broken.Map.TownSize[0], "a wrong size: the default");
            Check.Eq(5, broken.Districts.Count, "missing districts: the defaults");
            Check.True(broken.Districts["M"].Sprites.Count > 0, "a district without pictures gets some", null);
            Check.True(EstateConfig.ParseColor("#FF000080")[3] > 0.49f && EstateConfig.ParseColor("zz") == null, "colors", null);
            Check.Eq("TownCommon", EstateConfig.SplitSprite("TownCommon/Build2005")[0], "atlas of a picture");
        }

        [TestMethod, Description("EstateEvent: how far the mortal houses grow (population, prosperity)")]
        public void Estate_MortalGrowth()
        {
            Check.Eq(2, EstateEvent.MortalCap(2, 50, 5000, 10000), "small population, plain plot: the district's level");
            Check.Eq(3, EstateEvent.MortalCap(2, 50, 10000, 10000), "population at the base: +1");
            Check.Eq(3, EstateEvent.MortalCap(2, 80, 5000, 10000), "prosperous plot: +1");
            Check.Eq(4, EstateEvent.MortalCap(2, 80, 12000, 10000), "both: +2");
            Check.Eq(EstateEvent.MAX_LEVEL, EstateEvent.MortalCap(6, 90, 20000, 10000), "never above the highest house");
            var e = new EstateEvent.TownEstate();
            e.Plots.Add(new EstateEvent.Plot { District = "R", Level = 3 });
            e.Plots.Add(new EstateEvent.Plot { District = "O", Level = 1 });
            e.Plots.Add(new EstateEvent.Plot { District = "M", Level = 5, OwnerId = "x" });
            Check.Eq(1, EstateEvent.GrownLevels(e), "grown levels: the mortals' only (residential 2 -> 3)");
        }
        [TestMethod, Description("ClanEvent: the relation of two clans, the chance to win a fight")]
        public void Clan_Formulas()
        {
            Check.Eq(10, ClanEvent.Relation(-10, 40, 0), "base -10 + intimacy 40 x 0.5");
            Check.Eq(40, ClanEvent.Relation(-10, 0, 2), "2 marriages: +25 each");
            Check.Eq(-100, ClanEvent.Relation(-90, -100, 0), "at least -100");
            Check.Eq(100, ClanEvent.Relation(90, 100, 3), "at most 100");
            Check.Eq(0.75, ClanEvent.WinChance(300, 100), 0.0001, "3 times stronger: 75 %");
            Check.Eq(0.5, ClanEvent.WinChance(0, 0), 0.0001, "no power on both sides: even");
            Check.Eq(ClanEvent.Key("t", "a", "b"), ClanEvent.Key("t", "b", "a"), "the key of a pair does not depend on the order");
        }

        [TestMethod, Description("SpiritNoteEvent: notes first, the change back in stones, the fee both ways")]
        public void SpiritNote_Formulas()
        {
            Check.True(SpiritNoteEvent.Split(25000, 500, 3, 10000) == (3L, -5000L), "3 notes, 5,000 back", SpiritNoteEvent.Split(25000, 500, 3, 10000));
            Check.True(SpiritNoteEvent.Split(25000, 6000, 3, 10000) == (2L, 5000L), "2 notes and 5,000 stones", SpiritNoteEvent.Split(25000, 6000, 3, 10000));
            Check.True(SpiritNoteEvent.Split(25000, 30000, 0, 10000) == (0L, 25000L), "no notes: all in stones", null);
            Check.True(SpiritNoteEvent.Split(100000, 500, 3, 10000) == (-1L, -1L), "not enough", null);
            Check.True(SpiritNoteEvent.Split(2500000000L, 0, 300000, 10000) == (250000L, 0L), "above the int limit", null);
            Check.Eq(101000L, SpiritNoteEvent.BuyPrice(10, 10000, 0.01), "10 notes at 1 %: 101,000");
            Check.Eq(99000L, SpiritNoteEvent.SellPrice(10, 10000, 0.01), "10 notes back at 1 %: 99,000");
        }

        [TestMethod, Description("BodyCulEvent: natural treasures of a medicinal bath by realm")]
        public void BodyCul_Bath()
        {
            Check.True(BodyCulEvent.BathNeed(1) == (2, 4), "Copper Skin: 2 treasures of item level 4+", BodyCulEvent.BathNeed(1));
            Check.True(BodyCulEvent.BathNeed(4) == (4, 4), "Iron Bone: 4 of level 4+", BodyCulEvent.BathNeed(4));
            Check.True(BodyCulEvent.BathNeed(9) == (2, 5), "Jade Marrow: 2 of level 5+", BodyCulEvent.BathNeed(9));
            Check.True(BodyCulEvent.BathNeed(15) == (2, 6), "Undying Body: 2 of level 6", BodyCulEvent.BathNeed(15));
            Check.True(BodyCulEvent.HerbCounts(BodyCulEvent.BathNeeds(15)[1]) == (500, 200, 100), "Undying Body: 500 low, 200 middle or 100 high herbs a bath", BodyCulEvent.HerbCounts(BodyCulEvent.BathNeeds(15)[1]));
            Check.Eq(1, BodyCulEvent.HerbFloor(2), "grade 2: herbs of item level 1+");
            Check.Eq(5, BodyCulEvent.HerbFloor(10), "grade 10: herbs of item level 5+");
            Check.Eq(0, BodyCulEvent.HerbWeight(2, 3), "an herb below the floor is useless");
            Check.True(BodyCulEvent.HerbWeight(3, 3) == 2 && BodyCulEvent.HerbWeight(4, 3) == 5 && BodyCulEvent.HerbWeight(6, 3) == 10, "low 2, middle 5, high 10", null);
            Check.True(BodyCulEvent.BreakNeeds(3)[0].Count == 6 && BodyCulEvent.HerbCounts(BodyCulEvent.BreakNeeds(3)[1]) == (250, 100, 50), "into Iron Bone: 6 treasures, 250 low / 100 middle / 50 high herbs", null);
            Check.True(BodyCulEvent.BreakNeeds(12)[0].Count == 24 && BodyCulEvent.BreakNeeds(12)[0].Level == 6 && BodyCulEvent.HerbCounts(BodyCulEvent.BreakNeeds(12)[1]).high == 400, "into Undying Body: 24 treasures of level 6, 400 high herbs", null);
            Check.Eq(1.0, BodyCulEvent.EssenceSteps(BodyCulEvent.NeedExp(BodyCulEvent.MAX_LEVEL)), 0.0001, "a level's exp of essence: one step");
            Check.Eq(BodyCulEvent.BathExp(4) * 3 * 3 / 2, BodyCulEvent.BathExp(4, 3), "a x3 bath: 3 baths +50 %");
            Check.Eq(BodyCulEvent.BathExp(4) * 2 * 6 / 5, BodyCulEvent.BathExp(4, 2), "a x2 bath: 2 baths +20 %");
            Check.Eq(BodyCulEvent.BreakChance(3) - 20f, BodyCulEvent.BreakChance(3, 3), "a x3 breakthrough: -20 points");
            Check.Eq(5f, BodyCulEvent.BathRisk(1, 0f), "x1 backlash 5 %");
            Check.Eq(30f, BodyCulEvent.BathRisk(3, 0f), "x3 backlash 30 %");
            Check.Eq(21f, BodyCulEvent.BathRisk(3, 1f), 0.001, "the toughest body: x3 backlash 21 %");
            Check.Eq(0.1f, BodyCulEvent.Toughness(10000, 1000, 0), 0.0001, "toughness of 10,000 HP and 1,000 defense");
            Check.Eq(1f, BodyCulEvent.Toughness(int.MaxValue, 0, 0), "toughness at most 1");
            Check.Eq(3 * 12 * 5, BodyCulEvent.BreakLifeLoss(12, 3), "a failed x3 breakthrough into Undying Body: 15 years");
            Check.Eq(3 * 12 * 5 / 2, BodyCulEvent.BreakLifeLoss(12, 3, 1f), "a tough body loses half");
            Check.Eq(BodyCulEvent.BACKLASH_LIFE[2] * 2, BodyCulEvent.BacklashLife(3, 0f, 2.0), "the worst x3 backlash");
            Check.Eq(3, BodyCulEvent.Scale(BodyCulEvent.BreakNeeds(3), 3)[0].Count / BodyCulEvent.BreakNeeds(3)[0].Count, "x3 materials");
            Check.True(BodyCulEvent.Scale(BodyCulEvent.BreakNeeds(3), 3)[1].IsHerbs, "a scaled need stays herbs (tiers)", null);        }

        [TestMethod]
        public void Estate_Maps()
        {
            var styles = new HashSet<int>();
            var path = Environment.GetEnvironmentVariable("ESTATE_MAP_DUMP");
            if (!string.IsNullOrEmpty(path))
            {
                var dump = new StringBuilder();
                for (var seed = 0; seed < 10; seed++)
                    foreach (var city in new[] { false, true })
                        dump.AppendLine($"{(city ? "city" : "town")} {seed}").AppendLine(string.Join("\n", EstateEvent.GenerateMap(city, seed, out var st))).AppendLine(st.ToString()).AppendLine();
                File.WriteAllText(path, dump.ToString());
            }
            for (var seed = 0; seed < 60; seed++)
                foreach (var city in new[] { false, true })
                {
                    var name = $"{(city ? "city" : "town")} {seed}";
                    var map = EstateEvent.GenerateMap(city, seed, out var style);
                    styles.Add(style);
                    Check.True(string.Join("\n", map) == string.Join("\n", EstateEvent.GenerateMap(city, seed, out _)), $"{name}: the same map for the same seed", null);
                    var (w, h) = EstateEvent.MapSize(city);
                    Check.True(map.Length == h && map.All(r => r.Length == w), $"{name}: {w} x {h}", null);
                    Check.Eq(EstateEvent.PalaceSize(city).w * EstateEvent.PalaceSize(city).h, map.Sum(r => r.Count(c => c == EstateEvent.PALACE)), $"{name}: the palace");
                    var missing = new string("MNRCO".Where(c => !map.Any(r => r.Contains(c))).ToArray());
                    Check.True(missing.Length == 0, $"{name}: every district", missing);
                    var plots = EstateEvent.PlotCount(map);
                    Check.True(plots >= (city ? 220 : 140), $"{name}: plots", plots);
                    // every plot near a road (breadth search over the plots from the roads)
                    Check.True(FarPlots(map) == 0, $"{name}: every plot at most {EstateEvent.RoadReach} cells from a road", FarPlots(map));
                    // the roads are one network
                    Check.True(RoadParts(map) == 1, $"{name}: one road network", RoadParts(map));
                    // no little square of main road (bends at crossings; over a river a bridge may need one)
                    var squares = Enumerable.Range(0, map.Length - 1).Sum(y => Enumerable.Range(0, map[0].Length - 1).Count(x => new[] { map[y][x], map[y][x + 1], map[y + 1][x], map[y + 1][x + 1] }.All(c => c == EstateEvent.MAIN_ROAD)));
                    Check.True(squares == 0, $"{name}: no 2 x 2 square of main road", squares == 0 ? (object)0 : "\n" + string.Join("\n", map));
                    Check.True(EstateEvent.Blocks(map).Count(b => b.Count >= EstateEvent.LabelBlock) >= 5, $"{name}: named blocks", EstateEvent.Blocks(map).Count(b => b.Count >= EstateEvent.LabelBlock));
                }
            Check.True(styles.Select(x => EstateEvent.TerrainOf(x)).Distinct().Count() == EstateEvent.TERRAIN_COUNT, "every landscape is made", string.Join(",", styles.Select(x => EstateEvent.TerrainOf(x)).Distinct()));
            Check.Eq(EstateEvent.ROAD_COUNT, styles.Select(x => EstateEvent.RoadsOf(x)).Distinct().Count(), "every road layout is made");
            Check.True(styles.Count >= 18, "most of the 24 terrain x road styles in 120 maps", styles.Count);
        }

        [TestMethod]
        public void Estate_Footprints()
        {
            // a block of 3 x 2 plots between roads: a house grows over its neighbours, never over a road or another owner
            var map = new[] { "######", "#RRRR#", "#RRRR#", "######" };
            var e = EstateEvent.MakeEstate(map, 0, 1, null);
            Check.Eq(8, e.Plots.Count, "plots");
            var p = e.Plots.First(x => x.X == 1 && x.Y == 1);
            p.OwnerId = "me"; p.Level = 2;
            Check.Eq(0, EstateEvent.Expansion(e, p, 2, _ => 1).Count, "a tiled house: no more plots");
            var add = EstateEvent.Expansion(e, p, 3, x => x.Level > 0 ? 100 : 1);
            Check.Eq(1, add.Count, "a courtyard house takes a neighbour");
            e.Plots.First(x => x.X == 2 && x.Y == 1).Level = 2;   // mortals: dearer
            add = EstateEvent.Expansion(e, p, 3, x => x.Level > 0 ? 100 : 1);
            Check.True(add[0].X == 1 && add[0].Y == 2, "the free plot below rather than the mortals' house", $"{add[0].X},{add[0].Y}");
            Check.Eq(6, EstateEvent.MaxLevelAt(e, p), "room for an immortal palace (3 x 2)");
            e.Plots.First(x => x.X == 3 && x.Y == 1).OwnerId = "other";
            e.Plots.First(x => x.X == 3 && x.Y == 2).OwnerId = "other";
            Check.Eq(5, EstateEvent.MaxLevelAt(e, p), "another owner on the right: a manor at most");
            // the parts of a house
            foreach (var a in EstateEvent.Expansion(e, p, 5, _ => 1)) { a.OwnerId = "me"; a.MainIndex = p.Index; a.Level = 0; }
            Check.Eq(4, EstateEvent.PartsOf(e, p).Count, "a manor on 4 plots");
            EstateEvent.Release(e, p, true);
            Check.True(e.Plots.Where(x => x.X <= 2).All(x => x.OwnerId == null && x.Level > 0), "taken back: the mortals move in", null);
            // a house moved to a new map keeps its level when it has room
            var old = EstateEvent.MakeEstate(map, 0, 1, null);
            var h = old.Plots[0]; h.OwnerId = "me"; h.Level = 5; h.Rooms[1] = 2;
            var moved = EstateEvent.MakeEstate(EstateEvent.GenerateMap(false, 7, out var style), style, 7, old);
            var main = moved.Plots.Single(x => x.OwnerId == "me" && x.Level > 0);
            Check.Eq(5, main.Level, "moved with its level");
            Check.Eq(4, EstateEvent.PartsOf(moved, main).Count, "and its 4 plots");
            Check.Eq(2, main.Rooms[1], "and its rooms");
        }

        [TestMethod, Description("EstateEvent.ExpandMap / ExpandEstate: a town grown into a city keeps its map and its plots")]
        public void Estate_TownToCity()
        {
            for (var seed = 0; seed < 20; seed++)
            {
                var town = EstateEvent.GenerateMap(false, seed, out var style);
                var e = EstateEvent.MakeEstate(town, style, seed, null);
                var house = e.Plots.First(x => EstateEvent.MaxLevelAt(e, x) >= 5);
                house.OwnerId = "me"; house.Level = 5; house.Rooms[1] = 3;
                foreach (var a in EstateEvent.Expansion(e, house, 5, _ => 0)) EstateEvent.Take(a, house);
                var mortal = e.Plots.First(x => x.OwnerId == null); mortal.Level = 2;
                var city = EstateEvent.ExpandMap(town, true, seed, out var ox, out var oy);
                var (w, h) = EstateEvent.MapSize(true);
                Check.True(city.Length == h && city.All(r => r.Length == w), $"seed {seed}: city size", null);
                // the town unchanged in the middle, its woods cleared (the city grows over them)
                var same = Enumerable.Range(0, town.Length).All(y => Enumerable.Range(0, town[0].Length).All(x => town[y][x] == EstateEvent.WOODS || city[y + oy][x + ox] == town[y][x]));
                Check.True(same, $"seed {seed}: the town map unchanged in the middle", null);
                Check.True(Enumerable.Range(0, town.Length).All(y => Enumerable.Range(0, town[0].Length).All(x => town[y][x] != EstateEvent.WOODS || city[y + oy][x + ox] != EstateEvent.WOODS || HasWaterNear(town, x, y))), $"seed {seed}: the old woods cleared", null);
                Check.True(FarPlots(city) == 0, $"seed {seed}: every new plot near a road", FarPlots(city));
                Check.True(RoadParts(city) == 1, $"seed {seed}: one road network", RoadParts(city));
                var c = EstateEvent.ExpandEstate(e, city, style, seed, ox, oy);
                var moved = c.Plots.Single(x => x.OwnerId == "me" && x.Level > 0);
                Check.True(moved.X == house.X + ox && moved.Y == house.Y + oy && moved.Level == 5 && moved.Rooms[1] == 3, $"seed {seed}: the house kept (moved by {ox},{oy})", null);
                Check.Eq(4, EstateEvent.PartsOf(c, moved).Count, $"seed {seed}: its 4 plots kept");
                Check.Eq(2, c.Plots.Single(x => x.X == mortal.X + ox && x.Y == mortal.Y + oy).Level, $"seed {seed}: the mortals' house kept");
                Check.True(c.Plots.Count > e.Plots.Count + 80, $"seed {seed}: more plots", c.Plots.Count);
            }
        }

        private static bool HasWaterNear(string[] map, int x, int y) =>
            DIRS4.Any(d => y + d.dy >= 0 && y + d.dy < map.Length && x + d.dx >= 0 && x + d.dx < map[0].Length && map[y + d.dy][x + d.dx] == EstateEvent.WATER);

        private static readonly (int dx, int dy)[] DIRS4 = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        private static int FarPlots(string[] map)
        {
            var dist = new Dictionary<(int, int), int>();
            var q = new Queue<(int x, int y)>();
            for (var y = 0; y < map.Length; y++)
                for (var x = 0; x < map[y].Length; x++)
                    if (EstateEvent.IsRoad(map[y][x])) { dist[(x, y)] = 0; q.Enqueue((x, y)); }
            while (q.Count > 0)
            {
                var v = q.Dequeue();
                foreach (var (dx, dy) in DIRS4)
                {
                    int nx = v.x + dx, ny = v.y + dy;
                    if (ny < 0 || ny >= map.Length || nx < 0 || nx >= map[ny].Length || dist.ContainsKey((nx, ny)) || !EstateEvent.IsPlotCell(map[ny][nx])) continue;
                    dist[(nx, ny)] = dist[v] + 1;
                    q.Enqueue((nx, ny));
                }
            }
            var far = 0;
            for (var y = 0; y < map.Length; y++)
                for (var x = 0; x < map[y].Length; x++)
                    if (EstateEvent.IsPlotCell(map[y][x]) && (!dist.TryGetValue((x, y), out var d) || d > EstateEvent.RoadReach)) far++;
            return far;
        }

        private static int RoadParts(string[] map)
        {
            var seen = new HashSet<(int, int)>();
            var parts = 0;
            for (var y = 0; y < map.Length; y++)
                for (var x = 0; x < map[y].Length; x++)
                {
                    if (!EstateEvent.IsRoad(map[y][x]) || seen.Contains((x, y))) continue;
                    parts++;
                    var st = new Stack<(int x, int y)>();
                    st.Push((x, y)); seen.Add((x, y));
                    while (st.Count > 0)
                    {
                        var v = st.Pop();
                        foreach (var (dx, dy) in DIRS4)
                        {
                            int nx = v.x + dx, ny = v.y + dy;
                            if (ny < 0 || ny >= map.Length || nx < 0 || nx >= map[ny].Length || seen.Contains((nx, ny)) || !EstateEvent.IsRoad(map[ny][nx])) continue;
                            seen.Add((nx, ny)); st.Push((nx, ny));
                        }
                    }
                }
            return parts;
        }
    }
}
