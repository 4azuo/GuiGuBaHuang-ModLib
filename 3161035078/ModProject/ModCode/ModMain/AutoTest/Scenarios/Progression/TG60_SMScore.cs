using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Progression
{
    /// <summary>
    /// TG-60 (SMScoreEvent): what the S&amp;M score (P) of the save does, with the P of the save set for the test
    /// (restored at the end): 1. battle exp and loot x the reward multiplier; 2. the title of the tier in the player
    /// info and the record of the character in the global records (Taoist Settings > Score records); 3. the rare
    /// encounter only from the tier Ascetic; 4. the start bonus of a new save (ability points, spirit stones, once);
    /// 5. a descendant of easy settings keeps only a share of the spirit stones.
    /// </summary>
    public class TG60_SMScore : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-60";

        private static SMGlobalConfigsEvent Cfg => SMLocalConfigsEvent.Instance.Configs;

        public Scenario Create()
        {
            var score = 0;
            return new Scenario { Id = "TG-60", Feature = "SMScoreEvent", Title = "S&M score (P): rewards, titles and records, rare encounter, start bonus, inheritance" }
                .Step("score of the save", c =>
                {
                    if (!c.Assert(SMScoreEvent.Instance != null && SMScoreRecordsEvent.Instance != null, "SMScoreEvent and SMScoreRecordsEvent loaded")) return;
                    score = Cfg.Score;
                    c.Info($"P of the save: {score} ({SMScoreEvent.TitleText(score)})");
                    c.AssertEqual(score, SMScoreEvent.P, "P = the score of the S&M settings of the save");
                })
                .Step("1. rewards", c =>
                {
                    Cfg.Score = 40000;
                    c.AssertEqual(120, SMScoreEvent.ScaleExp(100), "40,000 P: battle exp +20 %");
                    var loot = DataProps.PropsData.NewProps(ModLib.Const.ModLibConst.MONEY_PROP_ID, 1000);
                    SMScoreEvent.ScaleLoot(new[] { loot });
                    c.AssertEqual(1200, loot.propsCount, "40,000 P: 1,000 spirit stones of loot -> 1,200");
                    Cfg.Score = -100000;
                    c.AssertEqual(80, SMScoreEvent.ScaleExp(100), "-100,000 P: battle exp -20 % (the floor)");
                })
                .Step("2. title in the player info", c =>
                {
                    Cfg.Score = 65000;
                    g.ui.OpenUISafe<UIPlayerInfo>(UIType.PlayerInfo);
                }, 1500)
                .Step("2. title shown", c =>
                {
                    var texts = UIHelper.GetUICustomBase(UIType.PlayerInfo).SelectMany(x => x.Items).OfType<ModLib.Object.UIItemText>()
                        .Select(x => TextIconHelper.GetShownText(x.Item)).ToList();
                    var title = SMScoreEvent.TitleText(65000);
                    c.Assert(texts.Any(x => x.Contains(title) && x.Contains("65,000")), $"the player info shows the title '{title}' and 65,000 P");
                    c.Snap("title");
                    CloseAll(UIType.PlayerInfo);
                }, 1000)
                .Step("2. record", c =>
                {
                    SMScoreEvent.Instance.UpdateRecord();
                    var rec = SMScoreRecordsEvent.Instance.Records.FirstOrDefault(x => x.UnitId == Player.GetUnitId());
                    c.Assert(rec != null && rec.P == 65000 && rec.Name == Player.GetName(), $"record of the character: {rec?.Name} {rec?.P} P, grade {rec?.BestGrade}, year {rec?.Years}");
                    SMScoreEvent.OpenRecords();
                }, 1500)
                .Step("2. records window", c =>
                {
                    var texts = UIHelper.GetUICustomBase(UIType.TextInfoLong).SelectMany(x => x.Items).OfType<ModLib.Object.UIItemText>()
                        .Select(x => TextIconHelper.GetShownText(x.Item)).ToList();
                    c.Assert(texts.Any(x => x.Contains(Player.GetName()) && x.Contains("65,000")), "the records window lists the character");
                    c.Snap("records");
                    CloseAll(UIType.TextInfoLong);
                }, 1000)
                .Step("3. rare encounter", c =>
                {
                    Cfg.Score = 29999;
                    c.Assert(!EncounterEvent.Possible().Contains(EncounterEvent.Encounter.Heaven), "29,999 P: no heavenly trial");
                    Cfg.Score = 30000;
                    c.Assert(EncounterEvent.Possible().Contains(EncounterEvent.Encounter.Heaven), "30,000 P (Ascetic): the heavenly trial can happen");
                    var exp = Player.GetExp();
                    var key = EncounterEvent.Resolve(EncounterEvent.Encounter.Heaven, 0, null, 10f);
                    c.AssertEqual("enc_heaven_win", key, "trial passed with a good roll");
                    c.Assert(Player.GetExp() > exp, $"exp {exp} -> {Player.GetExp()}");
                    c.AssertEqual("enc_heaven_lose", EncounterEvent.Resolve(EncounterEvent.Encounter.Heaven, 0, null, 90f), "trial failed with a bad roll (HP lost)");
                    c.Assert(EncounterEvent.Resolve(EncounterEvent.Encounter.Heaven, 1, null, 10f) == null, "declined: nothing happens");
                }, 2500)
                .Step("4. start bonus", c =>
                {
                    CloseAll(UIType.DramaDialogue);
                    Cfg.Score = 50000;
                    var points = Player.GetProperty<int>(UnitPropertyEnum.AbilityPoint);
                    var money = Player.GetUnitMoney();
                    SMScoreEvent.Instance.StartBonusPending = true; // as for a new save
                    SMScoreEvent.Instance.GiveStartBonus();
                    c.AssertEqual(points + 5, Player.GetProperty<int>(UnitPropertyEnum.AbilityPoint), "50,000 P: +5 ability points");
                    c.AssertEqual(money + 10000, Player.GetUnitMoney(), "50,000 P: +10,000 spirit stones");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_score_start"), "logged");
                    SMScoreEvent.Instance.GiveStartBonus();
                    c.AssertEqual(money + 10000, Player.GetUnitMoney(), "given once");
                    Player.AddProperty<int>(UnitPropertyEnum.AbilityPoint, -5);
                    Player.AddUnitMoney(-10000);
                })
                .Step("5. inheritance", c =>
                {
                    c.AssertEqual(1f, SMScoreEvent.InheritShare(0), "0 P: the descendant keeps everything");
                    c.AssertEqual(0.75f, SMScoreEvent.InheritShare(-10000), "-10,000 P: 75 %");
                    c.AssertEqual(0.5f, SMScoreEvent.InheritShare(-50000), "-50,000 P: 50 % (the floor)");
                })
                .Step("restore", c =>
                {
                    CloseAll(UIType.DramaDialogue, UIType.TextInfoLong, UIType.PlayerInfo);
                    Cfg.Score = score;
                    SMScoreEvent.Instance.UpdateRecord();
                    c.AssertEqual(score, SMScoreEvent.P, "P of the save restored");
                });
        }
    }
}
