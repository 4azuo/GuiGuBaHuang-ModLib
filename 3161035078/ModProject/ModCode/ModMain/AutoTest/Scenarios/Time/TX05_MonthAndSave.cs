using EGameTypeData;
using ModLib.Helper;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using System;
using System.IO;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TX-05 (ModLib save / monthly tick): one month passes through the world-run events (ModLib monthly tick +
    /// save), the Taoist caches written to disk (mod\nE7UL2\saves\&lt;player&gt;\*.json) contain the values in
    /// memory, and two saves requested at the same time do not overlap (the second waits: an overlapping
    /// g.data.SaveData failed with FileNotFoundException and hung the game). Changes the save (restored by
    /// run-autotest.ps1).
    /// </summary>
    public class TX05_MonthAndSave : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TX-05";

        private const string MARKER = "__autotest_persist__";
        private const int MARKER_VALUE = 424242;

        private static string PlayerLog => Path.Combine(UnityEngine.Application.persistentDataPath, "Player.log");

        private static int CountInPlayerLog(string text)
        {
            try
            {
                using (var fs = new FileStream(PlayerLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    var all = sr.ReadToEnd();
                    var n = 0;
                    for (var i = all.IndexOf(text, StringComparison.Ordinal); i >= 0; i = all.IndexOf(text, i + text.Length, StringComparison.Ordinal)) n++;
                    return n;
                }
            }
            catch { return -1; }
        }

        private static T ReadCache<T>(string cacheId) where T : ModLib.Object.CachableObject =>
            CacheHelper.ReadGameCacheFile<T>(ModMain.ModObj.ModId, cacheId);

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var start = DateTime.Now;
            return new Scenario { Id = "TX-05", Feature = "ModLib save/monthly tick", Title = "Month passes and the Taoist data is saved to disk" }
                .Step("month passes", c =>
                {
                    start = DateTime.Now;
                    c.Bag["month"] = GameHelper.GetGameTotalMonth();
                    c.Bag["fnf"] = CountInPlayerLog("FileNotFoundException");
                    g.world.run.AddDay(g.world.run.roundDayMax + 1);
                    EventHelper.CallGameEvent<WorldRunStart>(EGameType.WorldRunStart);
                    EventHelper.CallGameEvent<WorldRunEnd>(EGameType.WorldRunEnd);
                    c.AssertEqual((int)c.Bag["month"] + 1, GameHelper.GetGameTotalMonth(), "one month passed");
                    c.Assert(ContributionExchangeEvent.Instance.CurMonthRatio > 0, $"monthly handlers ran (sect exchange ratio drawn: {ContributionExchangeEvent.Instance.CurMonthRatio})");
                })
                .Wait("monthly save done", c => !DataMgr.isSaveData, 60000, 1000)
                .Step("change and save", c =>
                {
                    MonstStrongerEvent.Instance.Counter += 7;
                    ExpertEvent.GetExpertExps()[MARKER] = MARKER_VALUE;
                    c.Info($"monster growth counter {MonstStrongerEvent.Instance.Counter}, expertise marker {MARKER}={MARKER_VALUE}");
                    GameHelper.SaveGame();
                }, 1000)
                .Wait("save done", c => !DataMgr.isSaveData, 60000, 1000)
                .Step("caches on disk", c =>
                {
                    var file = CacheHelper.GetGameCacheFilePath(ModMain.ModObj.ModId, ModConst.MONST_STRONGER_EVENT);
                    c.Assert(File.Exists(file) && File.GetLastWriteTime(file) >= start, $"cache file written: {Path.GetFileName(file)} at {(File.Exists(file) ? File.GetLastWriteTime(file).ToString("HH:mm:ss") : "-")}");
                    var ms = ReadCache<MonstStrongerEvent>(ModConst.MONST_STRONGER_EVENT);
                    c.AssertEqual(MonstStrongerEvent.Instance.Counter, ms?.Counter ?? -1, "MonstStrongerEvent.Counter saved");
                    c.Assert(ms != null && MonstStrongerEvent.Instance.KillCounter.All(x => ms.KillCounter.ContainsKey(x.Key) && ms.KillCounter[x.Key] == x.Value), "kill counters saved");
                    var ex = ReadCache<ExpertEvent>(ModConst.EXPERT_EVENT);
                    c.Assert(ex?.ExpertExps != null && ex.ExpertExps.ContainsKey(MARKER) && ex.ExpertExps[MARKER] == MARKER_VALUE, "expertise exp saved");
                    var inf = ReadCache<InflationaryEvent>(ModConst.INFLATIONARY_EVENT);
                    c.AssertEqual(InflationaryEvent.Instance.WorldPrice, inf?.WorldPrice ?? -1, "world price index saved");
                    var ce = ReadCache<ContributionExchangeEvent>(ModConst.CONTRIBUTION_EXCHANGE_EVENT);
                    c.AssertEqual(ContributionExchangeEvent.Instance.CurMonthRatio, ce?.CurMonthRatio ?? -1, "sect exchange ratio saved");
                    var mk = ReadCache<RealMarketEvent3>(ModConst.REAL_MARKET_EVENT3);
                    c.AssertEqual(RealMarketEvent3.Instance.MarketStack.Count, mk?.MarketStack?.Count ?? -1, "player market listings saved");
                })
                .Step("two saves at once", c =>
                {
                    GameHelper.SaveGame();
                    GameHelper.SaveGame();
                    c.Info("SaveGame called twice in the same frame");
                })
                .Wait("saves done", c => !DataMgr.isSaveData, 60000, 2000, 3000)
                .Step("no overlap", c =>
                {
                    var before = (int)c.Bag["fnf"];
                    var now = CountInPlayerLog("FileNotFoundException");
                    if (before < 0 || now < 0) c.Info($"Player.log not readable: {PlayerLog}");
                    else c.AssertEqual(before, now, "no FileNotFoundException from overlapping saves in Player.log");
                    c.Assert(!DataMgr.isSaveData, "game not stuck in a save");
                })
                .Step("restore", c =>
                {
                    MonstStrongerEvent.Instance.Counter -= 7;
                    ExpertEvent.GetExpertExps().Remove(MARKER);
                    GameHelper.SaveGame();
                    c.Assert(!ExpertEvent.GetExpertExps().ContainsKey(MARKER), "marker removed");
                }, 1000)
                .Wait("restore saved", c => !DataMgr.isSaveData, 60000, 1000);
        }
    }
}
