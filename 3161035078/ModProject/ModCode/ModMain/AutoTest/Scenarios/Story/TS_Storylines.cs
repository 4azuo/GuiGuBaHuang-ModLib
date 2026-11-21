using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using MOD_nE7UL2.Story;
using System;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Story
{
    /// <summary>
    /// Story line tests (TS-01..TS-06): a line is opened (its NPCs cast or created), then driven to an ending: every
    /// drama is answered (first option, or the given plan), the waits between steps are skipped, the battles are won at
    /// once (StoryEvent.NoBattles), and the conditions of the steps are met by the test (the player in the right town,
    /// Granny's lifespan...). Changes the save (restored by run-autotest.ps1).
    /// </summary>
    public static class StoryDriver
    {
        /// <summary>Items and spirit stones the steps may ask for.</summary>
        public static void Supply()
        {
            Player.SetUnitMoney(100_000_000);
            var grade = Player.GetGradeLvl() + 2;
            for (var i = 0; i < 6; i++)
            {
                EncounterEvent.GiveItem(x => x.IsPill() != null, grade);
                EncounterEvent.GiveItem(x => x.IsHerbItem() != null, grade);
                EncounterEvent.GiveItem(x => x.IsHerbItem() != null, 1);
            }
            EncounterEvent.GiveItem(x => x.IsArtifact() != null, grade); // A4 "Give her a sword"
        }

        /// <summary>A town (the player is moved there).</summary>
        public static void GoToTown(string townId = null)
        {
            var town = townId != null ? g.world.build.GetBuild(townId)?.TryCast<MapBuildTown>()
                : (ModMain.Store?.Towns ?? new MapBuildTown[0]).FirstOrDefault(t => t != null && t.GetBuildSub<MapBuildTownMarket>() != null);
            if (town != null && Player.GetMapBuild<MapBuildTown>()?.buildData.id != town.buildData.id)
                Player.SetUnitPos(town.GetOrigiPoint());
        }

        /// <summary>Out of towns (a point near the player that is not a town).</summary>
        public static void LeaveTown()
        {
            if (Player.GetMapBuild<MapBuildTown>() == null) return;
            var p = Player.GetUnitPos();
            for (var d = 3; d < 30; d += 3)
                foreach (var q in new[] { new UnityEngine.Vector2Int(p.x + d, p.y), new UnityEngine.Vector2Int(p.x - d, p.y), new UnityEngine.Vector2Int(p.x, p.y + d), new UnityEngine.Vector2Int(p.x, p.y - d) })
                {
                    if (!WUnitHelper.IsValidPoint(q) || g.world.mapEvent.GetGridEvent(q) != null) continue; // not on a map event (its own drama)
                    Player.SetUnitPos(q);
                    if (Player.GetMapBuild<MapBuildTown>() == null) return;
                }
        }

        /// <summary>
        /// One poll: answers the open drama (plan[n] of the n-th drama, else 0), meets the conditions (prep), skips the
        /// wait of the step. True when the line has ended.
        /// </summary>
        public static bool Drive(AutoTestContext c, string id, Action<StoryState> prep = null, int[] plan = null)
        {
            var s = StoryEvent.Instance.State(id);
            if (StoryEvent.LastError != null) { c.Info($"error of a step: {StoryEvent.LastError}"); StoryEvent.LastError = null; }
            if (s.Closed) return true;
            var key = $"{s.Id}{s.Act}.{s.Step}";
            // an option whose condition is not met (proofs, intimacy...; spirit stones and items: Supply): its message
            // box, captured and closed; the choice comes again and the next option is taken
            if (g.ui.HasUI(UIType.CheckPopup))
            {
                var popup = g.ui.GetUI<UICheckPopup>(UIType.CheckPopup);
                var text = string.Join(" / ", popup.GetComponentsInChildren<UnityEngine.UI.Text>().Select(t => t.text?.Trim()).Where(t => !string.IsNullOrEmpty(t)));
                c.Info($"{key}: condition of the option not met: {text}");
                c.Snap("condition");
                c.Bag["skip_" + key] = (c.Bag.TryGetValue("picked_" + key, out var p) ? (int)p : 0) + 1;
                // its only button (the label depends on the game: "Yes" for the Ok popup)
                popup.btn1?.onClick?.Invoke();
                return false;
            }
            if (DramaUI() != null)
            {
                if (!ReadDrama(c)) return false;
                var labels = DramaOptions().Select(x => x.Item2).ToList();
                if (labels.Count == 0) { DramaNext(); return false; }
                var n = c.Bag.TryGetValue("dramas_" + id, out var v) ? (int)v : 0;
                c.Bag["dramas_" + id] = n + 1;
                var pick = plan != null && n < plan.Length ? Math.Min(plan[n], labels.Count - 1) : 0;
                if (c.Bag.TryGetValue("skip_" + key, out var skip)) pick = Math.Min(Math.Max(pick, (int)skip), labels.Count - 1);
                c.Bag["picked_" + key] = pick;
                c.Info($"{s.Id}{s.Act}.{s.Step}: {string.Join(" | ", labels)} -> {labels[pick]}");
                ChooseOption(labels[pick]);
                return false;
            }
            if (!s.Open) return false;
            prep?.Invoke(s);
            if (s.Battle == null)
            {
                s.NextMonth = GameHelper.GetGameTotalMonth();
                s.Soon = true;
            }
            return false;
        }

        /// <summary>
        /// Lets the drama be read before it is answered (evidence): its text typed out (unchanged since the last poll and
        /// at least 1.5 s + 30 ms a character, at most 8 s), then a screenshot, then the answer at the next poll (the
        /// capture takes ~0.3 s). True when it can be answered now.
        /// </summary>
        private static bool ReadDrama(AutoTestContext c)
        {
            var text = DramaText();
            var now = DateTime.Now;
            var prev = c.Bag.TryGetValue("drama_text", out var o) ? (string)o : null;
            if (prev != text)
            {
                // a new drama (the clock starts), or its text still typing (it grows from the previous one)
                if (string.IsNullOrEmpty(prev) || !text.StartsWith(prev)) c.Bag["drama_since"] = now;
                c.Bag["drama_text"] = text;
                c.Bag.Remove("drama_snap");
                return false;
            }
            var since = (DateTime)c.Bag["drama_since"];
            var need = System.Math.Min(8.0, 1.5 + 0.03 * text.Length);
            if ((now - since).TotalSeconds < need) return false;
            if (!c.Bag.ContainsKey("drama_snap"))
            {
                c.Bag["drama_snap"] = now;
                c.Snap("drama");
                return false;
            }
            if ((now - (DateTime)c.Bag["drama_snap"]).TotalSeconds < 0.6) return false;
            c.Bag.Remove("drama_text");
            c.Bag.Remove("drama_snap");
            return true;
        }

        public static Scenario Build(string scenarioId, string line, string title, Action<AutoTestContext> before, Action<StoryState> prep, string[] endings, int[] plan = null, Action<AutoTestContext, StoryState> after = null)
        {
            var sc = new Scenario { Id = scenarioId, Feature = "StoryEvent", Title = title, Overlay = false }
                .Step("open", c =>
                {
                    DismissDramas();
                    StoryEvent.NoBattles = true;
                    StoryEvent.NoGates = true;   // the long lines (G-L): their acts open without waiting for years / realms
                    Mod.AutoTestEvent.DramaTimeoutSec = 20; // every drama is read and captured before its answer
                    Supply();
                    before?.Invoke(c);
                    // from a clean state (a line may have been opened by another one, e.g. C opens E); the other lines
                    // closed so that only this one shows dramas
                    foreach (var other in StoryEvent.Instance.Lines.Values.Where(x => x.Open && x.Id != line).ToList())
                        StoryLine.Close(other, "test of another line");
                    StoryEvent.Instance.Lines.Remove(line);
                    var s = StoryEvent.Instance.State(line);
                    var opened = StoryEvent.Instance.Open(line, true);
                    c.Assert(opened, $"line {line} opened: roles {string.Join(", ", s.Roles.Select(x => $"{x.Key}={FamilyEvent.Unit(x.Value)?.GetName() ?? x.Value}"))}");
                    // the quest of the line: name, progress (act), guide of the act with names
                    StoryEvent.Instance.UpdateTasks();
                    var task = Player.GetTask(StoryEvent.TaskId(line)).ToArray().FirstOrDefault();
                    var guide = GameTool.LS("story_task_desc_" + line);
                    c.Assert(task != null && !string.IsNullOrEmpty(guide) && !guide.Contains("{"), $"quest of the line: {GameTool.LS("story_name_" + line)} - {guide}");
                    // the first drama waits for the evidence of the quest (next steps)
                    s.Soon = false;
                    s.NextMonth = GameHelper.GetGameTotalMonth() + 1;
                }, 1000)
                .Step("open the Mission window", c =>
                {
                    // a game event of the place may have opened meanwhile; the window opened like its map button
                    DismissDramas();
                    g.ui.GetUI<UIMapMain>(UIType.MapMain)?.uiPlayerInfo?.OnTaskClick();
                }, 2500)
                .Step("select the quest", c =>
                {
                    // the quest of the line selected: its guide in Mission Info
                    var ui = g.ui.GetUI<UIPlayerTask>(UIType.PlayerTask);
                    var task = Player.GetTask(StoryEvent.TaskId(line)).ToArray().FirstOrDefault();
                    if (!c.Assert(ui != null && task != null, "Mission window and quest")) return;
                    ui.uiTaskInfo.InitData(task);
                    c.Info($"Mission Info: {GameTool.LS("story_name_" + line)} - {GameTool.LS("story_task_desc_" + line)}");
                }, 1500)
                .Step("mission window", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.PlayerTask), "Mission window opened (screenshot of the step before)");
                    g.ui.CloseUI(UIType.PlayerTask);
                }, 1000);
            // one step per act (its dramas and screenshots grouped in the viewer), then the rest (fail branches) to the ending
            var acts = StoryEvent.Line(line)?.Acts ?? 5;
            for (var a = 0; a < acts; a++)
            {
                var act = a;
                sc = sc.Wait($"{line}{act}", c => Drive(c, line, prep, plan) || StoryEvent.Instance.State(line).Act > act, 600000, 400);
            }
            return sc
                .Wait("to the ending", c => Drive(c, line, prep, plan), 600000, 400)
                .Step("ending", c =>
                {
                    DismissDramas();
                    StoryEvent.NoBattles = false;
                    StoryEvent.NoGates = false;
                    Mod.AutoTestEvent.DramaTimeoutSec = 5;
                    var s = StoryEvent.Instance.State(line);
                    c.Assert(s.Closed && s.Ending != null && endings.Contains(s.Ending), $"line {line} ended: {s.Ending} (expected one of {string.Join(", ", endings)}); flags {string.Join(", ", s.Flags.Where(x => !x.Key.StartsWith("seen_")).Select(x => $"{x.Key}={x.Value}"))}");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_story_end"), "ending in the Taoist log");
                    StoryEvent.Instance.UpdateTasks();
                    c.Assert(!Player.GetTask(StoryEvent.TaskId(line)).ToArray().Any(), "quest of the line removed at the end");
                    after?.Invoke(c, s);
                });
        }
    }

    public class TS01_ColdSword : IScenarioDefinition
    {
        public string Id => "TS-01";
        public Scenario Create() => StoryDriver.Build(Id, "A", "Story A Cold Sword: A0-A5 to an ending", c => StoryDriver.LeaveTown(), s =>
        {
            if (s.Act == 0 && s.Step == 3) StoryDriver.GoToTown();
            else if (s.Act == 2 && s.Step == 2 && s.Roles.TryGetValue("T", out var t)) StoryDriver.GoToTown(t);
            else if (s.Act >= 1 && s.Act != 2) StoryDriver.LeaveTown();
        }, new[] { "A_SectMaster", "A_Companion" });
    }

    public class TS02_OldHuangDebt : IScenarioDefinition
    {
        public string Id => "TS-02";
        public Scenario Create() => StoryDriver.Build(Id, "B", "Story B Old Huang's Debt: B0-B5 to an ending", c => StoryDriver.GoToTown(), s =>
        {
            if (s.Act == 2 && s.Step == 11 && s.Roles.TryGetValue("T", out var t))
            {
                var other = (ModMain.Store?.Towns ?? new MapBuildTown[0]).FirstOrDefault(x => x != null && x.buildData.id != t);
                if (other != null) StoryDriver.GoToTown(other.buildData.id);
            }
        }, new[] { "B_MerchantPrince", "B_Betrayed" });
    }

    public class TS03_ExplodingFurnace : IScenarioDefinition
    {
        public string Id => "TS-03";
        // C2 fire control with the first options: 2 of 3 right (big fire is wrong for q1), the exam is passed
        public Scenario Create() => StoryDriver.Build(Id, "C", "Story C Exploding Furnace: C0-C4 to an ending", c => StoryDriver.GoToTown(), null,
            new[] { "C_MasterAlchemist", "C_Companion", "C_LastExplosion" });
    }

    public class TS04_SwornBrothers : IScenarioDefinition
    {
        public string Id => "TS-04";
        public Scenario Create() => StoryDriver.Build(Id, "D", "Story D Sworn Brothers: D0-D4 to an ending", c => StoryDriver.GoToTown(), null,
            new[] { "D_ThreeSworn", "D_BrotherLost", "D_MurongCompanion", "D_Rivals" });
    }

    public class TS05_HerbGarden : IScenarioDefinition
    {
        public string Id => "TS-05";
        public Scenario Create() => StoryDriver.Build(Id, "E", "Story E Granny's Herb Garden: E0-E4, A Li, garden seclusion", c => StoryDriver.LeaveTown(), s =>
        {
            var o = StoryLine.U(s, "O");
            if (o == null || o.isDie) return;
            // E3 needs her last 3 years; E4 her death
            if (s.Act == 3 && s.Step == 1) o.SetProperty<int>(UnitPropertyEnum.Life, o.GetProperty<int>(UnitPropertyEnum.Age) + 30);
            if (s.Act == 4 && !s.Has("O_dead")) o.Die();
        }, new[] { "E_GardenOfGenerations", "E_Successor", "E_LostChild" }, null, (c, s) =>
        {
            c.Assert(s.Has("Y_born") && s.Roles.ContainsKey("Yrec"), "A Li adopted (a child record of the family)");
            if (s.Ending == "E_GardenOfGenerations")
            {
                c.Assert(LineE_HerbGarden.GardenOpen(s), $"garden seclusion open (intimacy at her death {s.Flag("O_intim")})");
                var point = StoryEvent.GardenPoint(s);
                c.Assert(point != null, $"garden on the world map at {point}");
                StoryEvent.Instance.UpdateGarden();
                c.Assert(StoryEvent.GardenMarkerShown, "garden icon (fruit tree) on the world map");
                // away from the garden: the quest leads there, no window
                StoryDriver.LeaveTown();
                Player.SetUnitPos(new UnityEngine.Vector2Int(point.Value.x + 2, point.Value.y));
                StoryEvent.Instance.UpdateGarden();
                c.Snap("garden-icon-on-the-map");
                var task = Player.GetTask(StoryEvent.GARDEN_TASK_ID).ToArray().FirstOrDefault();
                c.Assert(task != null, "quest \"Granny Shi's garden\" given");
                c.Assert(g.conf.mapPosition.GetItem(StoryEvent.GARDEN_POSITION_ID)?.posiID == $"{point.Value.x}_{point.Value.y}", $"quest position = garden ({g.conf.mapPosition.GetItem(StoryEvent.GARDEN_POSITION_ID)?.posiID})");
                c.Assert(DramaUI() == null, "no garden window away from the garden");
                // at the garden: the window opens
                Player.SetUnitPos(point.Value);
                StoryEvent.Instance.UpdateGarden();
                var labels = DramaOptions().Select(x => x.Item2).ToList();
                c.Assert(labels.Count == StoryEvent.GARDEN_YEARS.Length + 1, $"garden options: {string.Join(" | ", labels)}");
                if (labels.Count > 0) ChooseOption(labels.Last());
            }
        });
    }

    public class TS06_RedLotus : IScenarioDefinition
    {
        public string Id => "TS-06";
        public Scenario Create() => StoryDriver.Build(Id, "F", "Story F Red Lotus: allies, the Lotus Pavilion, victory", null, null, new[] { "F_Victory" }, null, (c, s) =>
        {
            var hired = s.Roles.Keys.Where(k => k.StartsWith("ally_") && s.Has("hired_" + k.Substring(5))).Select(k => FamilyEvent.Unit(s.Roles[k])).Where(u => u != null).ToList();
            var team = HirePeopleEvent.Instance.TeamData.TryGetValue(Player.GetUnitId(), out var t) ? t : new List<string>();
            c.Assert(!hired.Any(u => team.Contains(u.GetUnitId())), $"allies hired for the battle dismissed ({hired.Count} hired)");
        });
    }

    // ---- the long lines G-L (their acts open at once in the tests: StoryEvent.NoGates)

    public class TS07_HundredYearWager : IScenarioDefinition
    {
        public string Id => "TS-07";
        public Scenario Create() => StoryDriver.Build(Id, "G", "Story G Hundred-Year Wager: G0-G5 over a century, to an ending", null, null,
            new[] { "G_Friend", "G_Rival", "G_Surpassed", "G_Grave" });
    }

    public class TS08_RoadsideInn : IScenarioDefinition
    {
        public string Id => "TS-08";
        public Scenario Create() => StoryDriver.Build(Id, "H", "Story H Roadside Inn: H0-H5, the wedding, the fire, her son, her last years", c => StoryDriver.GoToTown(), null,
            new[] { "H_InnOfAges", "H_Lineage", "H_Ashes" }, null, (c, s) =>
            {
                c.Assert(s.Roles.ContainsKey("R") && s.Roles.ContainsKey("K2"), "her husband and her son cast");
                c.Assert(LineH_RoadsideInn.InnStands(s) == (s.Ending != "H_Ashes"), "the inn stands unless burned (read by line J)");
            });
    }

    public class TS09_SealedTomb : IScenarioDefinition
    {
        public string Id => "TS-09";
        public Scenario Create() => StoryDriver.Build(Id, "I", "Story I Sealed Tomb: the map, the halls, Gu's betrayal, the tomb owner", c => StoryDriver.LeaveTown(), null,
            new[] { "I_Heir", "I_FreedSoul", "I_Sealer", "I_Looted" });
    }

    public class TS10_CursedDisciple : IScenarioDefinition
    {
        public string Id => "TS-10";
        // J1: mercy twice (option 0), J5: side by side against the sect master (option 0)
        public Scenario Create() => StoryDriver.Build(Id, "J", "Story J Cursed Disciple: the execution ground, 3 lessons, the hunters, the last duel", c => StoryDriver.GoToTown(), null,
            new[] { "J_Righteous", "J_Demon", "J_Sealed" }, null, (c, s) =>
                c.Assert(s.Flag("mercy") + s.Flag("strength") + s.Flag("freedom") >= 3, $"3 lessons taught (mercy {s.Flag("mercy")}, strength {s.Flag("strength")}, freedom {s.Flag("freedom")})"));
    }

    public class TS11_ThunderRemnant : IScenarioDefinition
    {
        public string Id => "TS-11";
        public Scenario Create() => StoryDriver.Build(Id, "K", "Story K Thunder Remnant: the voice, the platform, his descendants, the second tribulation", null, null,
            new[] { "K_Guided", "K_Devourer", "K_SecondChance" });
    }

    public class TS12_HeartDemon : IScenarioDefinition
    {
        public string Id => "TS-12";
        public Scenario Create() => StoryDriver.Build(Id, "L", "Story L Heart Demon: dreams of the other lines, the mirror lake, the copy, clear heart", null, null,
            new[] { "L_ClearHeart", "L_Scarred", "L_Embraced" }, null, (c, s) =>
                c.Assert(!string.IsNullOrEmpty(LineL_HeartDemon.Dreams()), $"dreams of the other lines: {LineL_HeartDemon.Dreams()}"));
    }

    // ---- the lines of 6.0 (the new cultivation paths and the estates)

    public class TS13_OldTieForge : IScenarioDefinition
    {
        public string Id => "TS-13";
        public Scenario Create() => StoryDriver.Build(Id, "M", "Story M Old Tie's Forge: the burned hand, the ore of the vein, the apprentice, the guild, the last blade", c => StoryDriver.GoToTown(), null,
            new[] { "M_Masterwork", "M_Heir", "M_Secret" }, null, (c, s) =>
                c.Assert(!ArtisanEvent.Enabled || ArtisanEvent.Rank(Player, ArtisanEvent.Craft.Forge) >= 1, $"the player learned the forge (rank {ArtisanEvent.Rank(Player, ArtisanEvent.Craft.Forge)})"));
    }

    public class TS14_HouseSeven : IScenarioDefinition
    {
        public string Id => "TS-14";
        // the player needs a house: one is bought in the town of the test (released after)
        public Scenario Create() => StoryDriver.Build(Id, "N", "Story N House on the Old Street: the cellar, the bandits, the notes, the attack, the street's verdict", c =>
            {
                StoryDriver.GoToTown();
                var town = Player.GetMapBuild<MapBuildTown>();
                if (town != null && !EstateEvent.HasHouse(town, Player))
                {
                    // a free plot, else a mortal house (a town grown for years has no free plot): lent, given back after
                    var plots = EstateEvent.Get(town)?.Plots;
                    var p = plots?.FirstOrDefault(x => x.Level <= 0 && x.OwnerId == null && x.MainIndex == null) ?? plots?.FirstOrDefault(EstateEvent.IsMortalHome);
                    if (p != null) { c.Bag["ts14_level"] = p.Level; p.OwnerId = Player.GetUnitId(); p.Level = System.Math.Max(1, p.Level); c.Bag["ts14_plot"] = p; }
                }
                c.Assert(LineN_HouseSeven.HouseTown() != null, $"a house of the player ({LineN_HouseSeven.HouseTown()?.name})");
            }, null,
            new[] { "N_Home", "N_Manor", "N_Empty" }, null, (c, s) =>
            {
                if (c.Bag.TryGetValue("ts14_plot", out var o)) { var p = (EstateEvent.Plot)o; p.OwnerId = null; p.Level = (int)c.Bag["ts14_level"]; }
            });
    }

    public class TS15_AshesOfTown : IScenarioDefinition
    {
        public string Id => "TS-15";
        public Scenario Create() => StoryDriver.Build(Id, "O", "Story O Ashes of the Town: the child in the ashes, the question, the training, the new town", null, null,
            new[] { "O_Atonement", "O_Ashes", "O_Rebuilt" });
    }

    public class TS16_SenseVoice : IScenarioDefinition
    {
        public string Id => "TS-16";
        public Scenario Create() => StoryDriver.Build(Id, "P", "Story P Heartbeat in the Stone: the sense finds him, the stone man, the body, his tribulation", c => StoryDriver.LeaveTown(), null,
            new[] { "P_Brothers", "P_Alone", "P_Devourer" }, null, (c, s) =>
                c.Assert(!BodyCulEvent.Enabled || BodyCulEvent.GetLevel(Player) >= 1, $"the player refines the body (level {BodyCulEvent.GetLevel(Player)})"));
    }
}
