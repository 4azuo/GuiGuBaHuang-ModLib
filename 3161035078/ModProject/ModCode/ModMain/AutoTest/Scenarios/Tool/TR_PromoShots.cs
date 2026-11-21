using ModLib.Helper;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Tool
{
    /// <summary>
    /// Promotion pictures (tools, run by scripts/game/run-promo.ps1): the windows of the features opened one after the
    /// other, each captured by a "promo-&lt;name&gt;" screenshot, copied to ModProject/SteamImg/promo_&lt;name&gt;.jpg by
    /// collect-promo.ps1 ("promo" of the scenario in tests/scenarios.json: step = promo-&lt;name&gt;). Not tests: no
    /// check fails the run, a missing feature only skips its picture. The run restores the save afterwards.
    /// </summary>
    internal static class PromoShot
    {
        /// <summary>Wait before a capture: the window opened by the previous step is drawn.</summary>
        public const int RENDER_MS = 1500;

        /// <summary>
        /// Appends: open (a step), wait until ready (the window is there; at most 10 s), then the capture once the
        /// window is drawn; false from open = no picture (nothing worth showing in this save).
        /// </summary>
        public static Scenario Shot(this Scenario s, string name, Func<AutoTestContext, bool> open, Func<bool> ready = null)
        {
            var ok = false;
            var waited = DateTime.MinValue;
            return s
                .Step($"open {name}", c =>
                {
                    // the mouse out of the game window: no hover tooltip in the picture
                    MoveCursorAway();
                    try { ok = open(c); }
                    catch (Exception ex) { ok = false; c.Info($"{name}: {ex.Message}"); }
                    if (!ok) c.Info($"{name}: not available, no picture");
                    waited = DateTime.Now;
                })
                .Wait($"ready {name}", c => !ok || ready == null || ready() || (DateTime.Now - waited).TotalSeconds > 10, 15000, 300)
                .Step($"promo {name}", c => { if (ok) c.Snap($"promo-{name}"); }, RENDER_MS);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        /// <summary>Puts the mouse cursor in the bottom right corner of the screen (outside the game window of a promotion run).</summary>
        public static void MoveCursorAway()
        {
            try { SetCursorPos(GetSystemMetrics(0) - 1, GetSystemMetrics(1) - 1); }
            catch (Exception ex) { DebugHelper.WriteLine($"PromoShot: cursor not moved: {ex.Message}"); }
        }

        /// <summary>Appends a step confirming the message box left by a step (e.g. the town seal of BecomeTownMaster).</summary>
        public static Scenario Confirm(this Scenario s) =>
            s.Step("confirm", c => c.Info($"clicked '{ClickButton(CONFIRM)}'"), RENDER_MS);

        /// <summary>
        /// Appends a step closing the windows (after the capture was taken). A lambda: UIType is not touched while the
        /// scenario is built (the unit tests build every scenario outside the game).
        /// </summary>
        public static Scenario Close(this Scenario s, Action close) =>
            s.Step("close", c => close(), RENDER_MS);
    }

    /// <summary>TR-01 (title screen): the S&amp;M configs window, its 3 pages.</summary>
    public class TR01_PromoTitle : IScenarioDefinition
    {
        public string Id => "TR-01";

        private static ModLib.Object.UICustomBase Window => UIHelper.GetUICustomBase(UIType.TextInfoLong).LastOrDefault();

        public Scenario Create()
        {
            return new Scenario { Id = "TR-01", Feature = "Promo", Title = "Promotion pictures: S&M configs", Scope = ScenarioScope.Title, Kind = ScenarioKind.Tool, Overlay = false, SnapSteps = false }
                // the title screen plays its intro first: the S&M button is clicked again until the window is open
                .Wait("open S&M configs", c =>
                {
                    if (Window != null) return true;
                    FindButtons(UIType.Login, SMGlobalConfigsEvent.TITLE).FirstOrDefault()?.Item.onClick.Invoke();
                    return false;
                }, 90000, 2000)
                .Shot("sm-configs-1", c => { SMGlobalConfigsEvent.Instance?.ShowTrainer(false); return Window != null; })
                .Shot("sm-configs-2", c => { Window?.NextPage(); return Window != null; })
                .Shot("sm-configs-3", c => { Window?.NextPage(); return Window != null; })
                .Close(() => CloseAll(UIType.TextInfoLong));
        }
    }

    /// <summary>
    /// TR-02 (in the game): trainer, Taoist settings, bank (account, loan), weapon details, market (player market, buy
    /// orders, shop prices), team, town master, bounties, family accounts, Taoist log.
    /// </summary>
    public class TR02_PromoGame : IScenarioDefinition
    {
        public string Id => "TR-02";

        /// <summary>Realm of the character in the pictures (TR-02 raises it with the trainer; the save is restored).</summary>
        public const int PROMO_GRADE = 5;

        /// <summary>The 4 kinds of techniques shown: attack, ultimate, movement, passive.</summary>
        /// (a property: the Il2Cpp enum is not touched while the scenario is built, the unit tests build it outside the game)
        private static MartialType[] TECHNIQUES => new[] { MartialType.SkillLeft, MartialType.SkillRight, MartialType.Step, MartialType.Ability };

        /// <summary>Picture names of the techniques, same order as TECHNIQUES.</summary>
        private static readonly string[] TECHNIQUE_NAMES = { "attack", "ultimate", "movement", "passive" };

        /// <summary>The player's best item of a kind (the best of the game added to the bag when none).</summary>
        private static DataProps.PropsData BestProp(Func<ConfItemPropsItem, bool> kind)
        {
            var prop = Player.GetUnitProps().ToArray().Where(x => x.propsItem != null && kind(x.propsItem))
                .OrderByDescending(x => x.propsInfoBase.grade).FirstOrDefault();
            if (prop != null) return prop;
            var conf = g.conf.itemProps._allConfList.ToArray().Where(kind).OrderByDescending(x => x.level).FirstOrDefault();
            if (conf == null) return null;
            Player.AddUnitProp(conf.id, 1);
            return Player.GetUnitProps(conf.id).LastOrDefault();
        }

        /// <summary>The prop info of the player's best item of a kind, over the player info.</summary>
        private static bool OpenPropInfo(Func<ConfItemPropsItem, bool> kind)
        {
            if (!g.ui.HasUI(UIType.PlayerInfo)) return false;
            var prop = BestProp(kind);
            if (prop == null) return false;
            g.ui.OpenUISafe<UIPropInfo>(UIType.PropInfo).InitData(Player, prop, UnityEngine.Vector3.zero, false);
            return true;
        }

        /// <summary>Switches the player info to a tab (its toggle).</summary>
        private static bool Tab(Func<UIPlayerInfo, UnityEngine.UI.Toggle> toggle)
        {
            var ui = g.ui.GetUI<UIPlayerInfo>(UIType.PlayerInfo);
            if (ui == null) return false;
            var t = toggle(ui);
            if (t == null) return false;
            t.isOn = true;
            return true;
        }

        /// <summary>The player's technique of a type with the highest grade.</summary>
        private static DataUnit.ActionMartialData BestMartial(MartialType type)
        {
            DataUnit.ActionMartialData best = null;
            var bestGrade = -1;
            foreach (var m in Player.GetActionMartials())
            {
                var md = m.Value.data.To<DataProps.MartialData>();
                if (md == null || md.martialType != type) continue;
                if (md.martialInfo.grade > bestGrade) { best = m.Value; bestGrade = md.martialInfo.grade; }
            }
            return best;
        }

        /// <summary>
        /// Learns a technique of a type a bit above the player's (grade near 5 when the world has one), copied from a
        /// cultivator of the world, and equips it. Returns the learnt technique (the player's own when none found).
        /// </summary>
        private static DataUnit.ActionMartialData LearnTechnique(MartialType type)
        {
            // the technique of a cultivator of the world (grade near 5) moves to the player: the game's study of a book
            // (StudyActionMartialProp) fails outside its own window; the save is restored after the run
            WorldUnitBase owner = null;
            string key = null;
            var pickScore = int.MaxValue;
            foreach (var u in g.world.unit.GetUnits().ToArray())
            {
                if (u == null || u.isDie || u.IsPlayer()) continue;
                foreach (var m in u.GetActionMartials())
                {
                    var md = m.Value?.data?.To<DataProps.MartialData>();
                    if (md?.martialInfo == null || md.martialType != type) continue;
                    var score = Math.Abs(md.martialInfo.grade - 5) * 10 - md.martialInfo.level;
                    if (score < pickScore) { owner = u; key = m.Key; pickScore = score; }
                }
            }
            DataUnit.ActionMartialData learnt = null;
            if (owner != null)
            {
                try
                {
                    var from = owner.data.unitData.allActionMartial;
                    learnt = from[key];
                    from.Remove(key);
                    Player.data.unitData.allActionMartial[key] = learnt;
                    Player.CreateAction(new UnitActionMartialEquip(learnt, 0));
                }
                catch (Exception ex) { DebugHelper.WriteLine($"TR-02: learn {type}: {ex.Message}"); }
            }
            return learnt ?? BestMartial(type);
        }

        /// <summary>
        /// Foes for the second page of the damage calculation when the save has none: 3 cultivators of the world
        /// around the player's grade, 3 monsters of the battle configs (their base numbers). Restored with the save.
        /// </summary>
        private static void SampleFoes(AutoTestContext c)
        {
            if (DamageInfoEvent.Instance == null) return;
            if (DamageInfo.RecentFoes(true).Count == 0)
                foreach (var npc in Npcs.OrderBy(x => Math.Abs(x.GetGradeLvl() - Player.GetGradeLvl() - 1)).Take(DamageInfo.RECENT_FOES).Reverse())
                    DamageInfo.AddRecord(DamageInfo.Snapshot(DamageInfo.FromWorld(npc)));
            if (DamageInfo.RecentFoes(false).Count == 0)
            {
                var grade = Math.Max(1, Player.GetGradeLvl());
                // named monsters of the battle configs (ConfBattleUnitAttr: name, base = model and numbers)
                var monsters = g.conf.battleUnitAttr._allConfList.ToArray()
                    .Select(a => Tuple.Create(a, g.conf.battleUnitBase.GetItem(a.baseID)))
                    .Where(x => x.Item2 != null && x.Item2.atk > 0 && x.Item2.hp > 0 && !string.IsNullOrEmpty(x.Item2.model)
                        && !string.IsNullOrEmpty(x.Item1.name) && !string.IsNullOrWhiteSpace(GameTool.LS(x.Item1.name)) && GameTool.LS(x.Item1.name) != x.Item1.name)
                    .GroupBy(x => x.Item2.model).Select(x => x.First()).OrderBy(x => x.Item2.id).ToList();
                var step = Math.Max(1, monsters.Count / 7);
                var picks = monsters.Where((x, i) => i % step == 0).Skip(1).Take(DamageInfo.RECENT_FOES).ToList();
                foreach (var m in picks)
                {
                    var b0 = m.Item2;
                    var side = new DamageInfo.Side { Name = GameTool.LS(m.Item1.name), Grade = grade, Atk = b0.atk * grade, Def = b0.def * grade, Hp = b0.hp * grade, HpMax = b0.hp * grade };
                    foreach (var b in DamageInfo.BASIS) side.Basis[b] = 0;
                    DamageInfo.AddRecord(DamageInfo.Snapshot(side, b0.id));
                }
            }
            c.Info($"foes: {DamageInfo.RecentFoes(false).Count} monsters, {DamageInfo.RecentFoes(true).Count} cultivators");
        }

        public Scenario Create()
        {
            MapBuildTown town = null;
            var s = new Scenario { Id = "TR-02", Feature = "Promo", Title = "Promotion pictures: feature windows", Kind = ScenarioKind.Tool, Overlay = false, SnapSteps = false }
                .Step("travel", c =>
                {
                    var t = FindTown(x => Has(x, MapBuildSubType.TownStorage) && x.GetBuildSub<MapBuildTownMarket>() != null && Has(x, MapBuildSubType.TownMarketPill))
                        ?? FindTown(x => Has(x, MapBuildSubType.TownStorage));
                    TravelTo(c, t);
                    town = BagTown(c);
                    c.Info($"town: {town?.name}");
                    // rich enough for the windows to show real numbers (the save is restored after the run)
                    if (Player.GetUnitMoney() < 100000) Player.AddUnitMoney(100000);
                    // a developed character: the trainer's "Level up" to the realm of the pictures (Origin Spirit)
                    var trainer = FeatureEvent<TrainerEvent>(ModConst.TRAINER_EVENT);
                    for (var i = 0; i < 40 && Player.GetGradeLvl() < PROMO_GRADE; i++) trainer.Levelup();
                    c.Info($"realm: grade {Player.GetGradeLvl()}");
                    if (town != null && !BankAccountEvent.Instance.RegisterdTown.Contains(town.buildData.id))
                        BankAccountEvent.Instance.RegisterdTown.Add(town.buildData.id);
                }, 1500);

            s.Shot("trainer", c =>
            {
                var trainer = FeatureEvent<TrainerEvent>(ModConst.TRAINER_EVENT);
                trainer.OpenTrainer(0);
                return g.ui.HasUI(UIType.TextInfoLong);
            }).Close(() => CloseAll(UIType.TextInfoLong));
            // its other pages: teleport to any town / sect, breakthrough items of every realm
            foreach (var page in new[] { Tuple.Create(1, "trainer-teleport"), Tuple.Create(2, "trainer-breakthrough") })
            {
                var p = page;
                s.Shot(p.Item2, c =>
                {
                    FeatureEvent<TrainerEvent>(ModConst.TRAINER_EVENT).OpenTrainer(p.Item1);
                    return g.ui.HasUI(UIType.TextInfoLong);
                }).Close(() => CloseAll(UIType.TextInfoLong));
            }

            s.Shot("taoist-settings", c =>
            {
                if (UISettingScreenEvent.Instance == null) return false;
                // from the game settings, as a player opens it: the hotkey rows are copies of the game's key rows
                g.ui.OpenUISafe<UIGameSetting>(UIType.GameSetting);
                UISettingScreenEvent.Instance.OpenTaoistSettings();
                return g.ui.HasUI(UIType.TextInfoLong);
            }).Close(() => CloseAll(UIType.TextInfoLong, UIType.GameSetting));

            // bank: the storage with an account, then the loan window
            s.Shot("bank", c =>
            {
                if (town == null) return false;
                // inside the bank: the storage window with the account (stored value, fee) and the bag
                OpenTownUI<UITownStorage>(UIType.TownStorage, ui => ui.InitData(town));
                OpenTownUI<UITownStorageProps>(UIType.TownStorageProps, ui => ui.InitData(town));
                return g.ui.HasUI(UIType.TownStorageProps);
            }, () => g.ui.HasUI(UIType.TownStorageProps));
            s.Shot("bank-loan", c =>
            {
                if (BankLoanEvent.Instance == null || town == null) return false;
                BankLoanEvent.Instance.OpenLoan(town);
                return g.ui.HasUI(UIType.TextInfo);
            }).Step("close loan", c => CloseAll(UIType.TextInfo), PromoShot.RENDER_MS);

            // the storage closed (the windows of the character come next)
            s.Step("close storage", c => CloseAll(UIType.TownStorageProps, UIType.TownStorage), PromoShot.RENDER_MS);

            // equipment and techniques of a developed character (the save is restored after the run): a ring, an
            // outfit and a weapon with random refine levels (the refine slots of the mod unlocked), 4 techniques of a
            // higher grade (attack, ultimate, movement, passive) learnt from the cultivators of the world with random
            // mastery levels, all equipped
            s.Step("equipment", c =>
            {
                foreach (var kind in new Func<ConfItemPropsItem, bool>[] { x => x.IsRing() != null, x => x.IsOutfit() != null, x => x.IsArtifact() != null })
                {
                    var prop = BestProp(kind);
                    if (prop == null) continue;
                    var lvl = CommonTool.Random(120, 360);
                    var need = CustomRefineEvent.ExpForLevel(prop, lvl) - CustomRefineEvent.GetRefineExp(prop);
                    if (need > 0) CustomRefineEvent.AddRefineExp(prop, need);
                    c.Info($"{GameTool.LS(prop.propsItem.name)}: refine {CustomRefineEvent.GetRefineLvl(prop)}");
                    try
                    {
                        if (prop.propsItem.IsRing() != null) Player.CreateAction(new UnitActionEquipRingChange(prop.soleID));
                        else if (prop.propsItem.IsArtifact() != null) Player.CreateAction(new UnitActionEquipEquip(prop, 0, false));
                    }
                    catch (Exception ex) { c.Info($"equip {prop.propsItem.name}: {ex.Message}"); }
                }
            }, PromoShot.RENDER_MS);
            s.Step("techniques", c =>
            {
                foreach (var type in TECHNIQUES)
                {
                    try
                    {
                        var am = LearnTechnique(type);
                        var md = am?.data?.To<DataProps.MartialData>();
                        if (md?.martialInfo == null) { c.Info($"{type}: no technique to learn"); continue; }
                        int grade = md.martialInfo.grade, level = md.martialInfo.level;
                        ExpertEvent.GetExpertExps()[am.data.soleID] = ExpertEvent.GetExpertNeedExp(CommonTool.Random(4, 15), grade, level);
                        c.Info($"{type}: {GameTool.LS(md.martialInfo.name)} grade {grade}, mastery {ExpertEvent.GetExpertLvl(am.data.soleID, grade, level)}");
                    }
                    catch (Exception ex) { c.Info($"{type}: {ex.Message}"); }
                }
            }, PromoShot.RENDER_MS);

            // character: the player info (the mod's bonuses next to the game's numbers) on its tabs, the details over
            // them (the mod adds mastery / refine bonuses only with the player info open), the damage calculation
            s.Shot("player-info", c =>
            {
                g.ui.OpenUISafe<UIPlayerInfo>(UIType.PlayerInfo);
                return g.ui.HasUI(UIType.PlayerInfo);
            }, () => g.ui.HasUI(UIType.PlayerInfo));
            s.Shot("skills", c => Tab(ui => ui.tglSkill));
            for (var i = 0; i < TECHNIQUE_NAMES.Length; i++)
            {
                var index = i;
                s.Shot($"technique-{TECHNIQUE_NAMES[index]}", c =>
                {
                    if (!g.ui.HasUI(UIType.PlayerInfo)) return false;
                    var martial = BestMartial(TECHNIQUES[index]);
                    if (martial == null) return false;
                    g.ui.OpenUISafe<UIMartialInfo>(UIType.MartialInfo).InitData(Player, martial, UnityEngine.Vector2.zero);
                    return true;
                }).Close(() => CloseAll(UIType.MartialInfo));
            }
            s.Shot("items", c => Tab(ui => ui.tglProp));
            s.Shot("weapon-detail", c =>
            {
                var artifact = BestProp(x => x.IsArtifact() != null);
                if (artifact == null || !g.ui.HasUI(UIType.PlayerInfo)) return false;
                var ui = g.ui.OpenUISafe<UIArtifactInfo>(UIType.ArtifactInfo);
                ui.InitData(Player, artifact, UnityEngine.Vector2.zero, false);
                return ui != null;
            }).Close(() => CloseAll(UIType.ArtifactInfo));
            s.Shot("ring-detail", c => OpenPropInfo(x => x.IsRing() != null)).Close(() => CloseAll(UIType.PropInfo));
            s.Shot("outfit-detail", c => OpenPropInfo(x => x.IsOutfit() != null)).Close(() => CloseAll(UIType.PropInfo));
            s.Step("stats tab", c => Tab(ui => ui.tglPrpperty), PromoShot.RENDER_MS);
            s.Shot("damage-calc", c =>
            {
                SampleFoes(c);
                DamageInfo.Open();
                return DamageInfo.IsOpen;
            });
            s.Shot("damage-calc-foes", c =>
            {
                DamageInfo.Window?.NextPage();
                return DamageInfo.IsOpen;
            }).Close(() => { DamageInfo.Close(); CloseAll(UIType.PlayerInfo); });

            // market: the player market, its buy orders, the shop with the mod's prices
            s.Shot("market", c =>
            {
                if (town?.GetBuildSub<MapBuildTownMarket>() == null || SMLocalConfigsEvent.Instance.Configs.NoMarketItem) return false;
                OpenTownUI<UITownMarket>(UIType.TownMarket, ui => ui.InitData(town));
                return g.ui.HasUI(UIType.TownMarket);
            });
            s.Shot("market-buy-orders", c =>
            {
                if (RealMarketEvent3.Instance == null || !g.ui.HasUI(UIType.TownMarket)) return false;
                // a few orders of the player: the window lists them (the save is restored after the run)
                if (RealMarketEvent3.Instance.BuyOrders.Count == 0)
                {
                    var goods = g.conf.itemProps._allConfList.ToArray()
                        .Where(x => x.sale > 0 && (x.IsHerbItem() != null || x.IsMineItem() != null || x.IsPill() != null))
                        .GroupBy(x => x.IsHerbItem() != null ? 0 : x.IsMineItem() != null ? 1 : 2).Select(x => x.OrderByDescending(y => y.level).First());
                    foreach (var conf in goods)
                        RealMarketEvent3.AddBuyOrder(town, conf.id, 3, Math.Max(RealMarketEvent3.MIN_PRICE, conf.sale));
                }
                RealMarketEvent3.Instance.OpenBuyOrders(town);
                return true;
            }).Close(() => CloseAll(UIType.TextInfoLong, UIType.TownMarket));
            s.Shot("shop", c =>
            {
                if (!Has(town, MapBuildSubType.TownMarketPill)) return false;
                OpenTownUI<UITownMarketBuy>(UIType.TownMarketBuy, ui => ui.InitData(town));
                return g.ui.HasUI(UIType.TownMarketBuy);
            }).Close(() => CloseAll(UIType.TownMarketBuy));

            // team up: a strong NPC's info (hire button and price), then the team with 2 hired members
            WorldUnitBase hire = null;
            s.Shot("npc-hire", c =>
            {
                hire = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && !HirePeopleEvent.IsTeamMaster(x))
                    .OrderByDescending(x => x.GetGradeLvl()).FirstOrDefault();
                if (hire == null) return false;
                g.ui.OpenUISafe<UINPCInfo>(UIType.NPCInfo).InitData(hire, false);
                return true;
            }).Close(() => CloseAll(UIType.NPCInfo));
            s.Shot("team", c =>
            {
                if (!HirePeopleEvent.IsTeam(Player, hire) && hire != null) HirePeopleEvent.Hire(Player, hire);
                var second = Npcs.Where(x => x != hire && !HirePeopleEvent.IsHired(x) && !HirePeopleEvent.IsTeamMaster(x))
                    .OrderByDescending(x => x.GetGradeLvl()).FirstOrDefault();
                if (second != null) HirePeopleEvent.Hire(Player, second);
                HirePeopleEvent.OpenUIManageTeam();
                return g.ui.HasUI(UIType.NPCSearch);
            }).Close(() => CloseAll(UIType.NPCSearch));

            // town master: the town management window (tax, budget, guardians, buildings); the town seal message
            // of BecomeTownMaster confirmed first (it stays over the next windows otherwise)
            s.Step("become master", c =>
            {
                if (town != null && !MapBuildPropertyEvent.IsTownMaster(town, Player)) FeatureEvent<TrainerEvent>(ModConst.TRAINER_EVENT).BecomeTownMaster();
            }).Confirm();
            // the guardians to hire (the people of the town), then the town with 3 of them hired
            s.Shot("town-hire", c =>
            {
                if (town == null || !MapBuildPropertyEvent.IsTownMaster(town, Player)) return false;
                if (MapBuildPropertyEvent.GetHirablePeople().Count == 0) return false;
                MapBuildPropertyEvent.OpenUIHirePeople();
                return g.ui.HasUI(UIType.NPCSearch);
            }).Close(() => CloseAll(UIType.NPCSearch));
            s.Shot("town-master", c =>
            {
                if (town == null || !MapBuildPropertyEvent.IsTownMaster(town, Player)) return false;
                var guardians = MapBuildPropertyEvent.GetTownGuardians(town).Count;
                foreach (var npc in MapBuildPropertyEvent.GetHirablePeople().Where(x => !x.IsPlayer()).OrderByDescending(x => x.GetGradeLvl()).Take(Math.Max(0, 4 - guardians)))
                    MapBuildPropertyEvent.Hire(town, npc, MapBuildPropertyEvent.TOWN_GUARDIAN_LUCK_ID);
                c.Info($"guardians: {MapBuildPropertyEvent.GetTownGuardians(town).Count}");
                MapBuildPropertyEvent.OpenUITownManage(town);
                return g.ui.HasUI(UIType.NPCSearch);
            }).Close(() => CloseAll(UIType.NPCSearch, UIType.TextInfoLong, UIType.TextInfo));

            s.Shot("wanted", c =>
            {
                if (WantedEvent.Instance == null || WantedEvent.Disabled) return false;
                // bounties on a few strong NPCs when the world has none (the save is restored after the run)
                if (!WantedEvent.Instance.Bounties.Any(b => b.Target != null && !b.Target.isDie))
                    foreach (var npc in Npcs.Where(x => !HirePeopleEvent.IsTeam(Player, x)).OrderByDescending(x => x.GetGradeLvl()).Take(4))
                        WantedEvent.Instance.Bounties.Add(new WantedEvent.Bounty { TownId = town?.buildData.id, TargetId = npc.GetUnitId(),
                            PosterId = Player.GetUnitId(), Reward = WantedEvent.MinReward(npc) * 2, PostedMonth = GameHelper.GetGameTotalMonth() });
                WantedEvent.Instance.OpenList();
                return g.ui.HasUI(UIType.NPCSearch);
            }).Close(() => CloseAll(UIType.NPCSearch));

            // family: the player married with 2 children (an adult heir, a child growing up); the family tree, the
            // household accounts, the choice of an heir (continue as a descendant), a seclusion offer
            s.Step("family", c =>
            {
                var F = FamilyEvent.Instance;
                if (F == null || FamilyEvent.Disabled) { c.Info("family off"); return; }
                if (string.IsNullOrEmpty(FamilyEvent.Spouse(Player)))
                {
                    var mate = Npcs.Where(x => string.IsNullOrEmpty(FamilyEvent.Spouse(x)) && !HirePeopleEvent.IsHired(x) && FamilyEvent.IsMan(x) != FamilyEvent.IsMan(Player))
                        .OrderByDescending(x => x.GetGradeLvl()).FirstOrDefault();
                    if (mate == null) { c.Info("no one to marry"); return; }
                    FamilyEvent.SetMarried(Player, mate);
                }
                var spouse = FamilyEvent.Unit(FamilyEvent.Spouse(Player));
                var father = FamilyEvent.IsMan(Player) ? Player : spouse;
                var mother = FamilyEvent.IsMan(Player) ? spouse : Player;
                if (FamilyEvent.Family(Player).Children.Count == 0)
                {
                    F.Birth(father, mother, GameHelper.GetGameTotalMonth() - FamilyEvent.ADULT_YEARS * 12);
                    F.Birth(father, mother, GameHelper.GetGameTotalMonth() - 5 * 12);
                    F.OnMonthly();
                }
                DismissDramas();
                c.Info($"married to {spouse?.GetName()}, children: {FamilyEvent.Family(Player).Children.Count}, unborn {FamilyEvent.Family(Player).Unborn.Count}");
            }, PromoShot.RENDER_MS);
            s.Shot("family-tree", c =>
            {
                if (FamilyEvent.Disabled || string.IsNullOrEmpty(FamilyEvent.Spouse(Player))) return false;
                DismissDramas();
                FamilyEvent.OpenFamily(Player);
                return g.ui.HasUI(UIType.NPCInfoRelation);
            }).Close(() => CloseAll(UIType.NPCInfoRelation));
            s.Shot("family-heir", c =>
            {
                var cfg = SMLocalConfigsEvent.Instance?.Configs;
                if (FamilyEvent.Instance == null || cfg == null || FamilyEvent.Heirs().Count == 0) return false;
                cfg.ContinueAsDescendant = true;   // the save is restored after the run
                DismissDramas();
                return FamilyEvent.Instance.OfferInherit(null);
            }).Close(() => DismissDramas());
            s.Shot("seclusion", c =>
            {
                if (SeclusionEvent.Instance == null || (SMLocalConfigsEvent.Instance?.Configs?.NoSeclusion ?? false)) return false;
                DismissDramas();
                SeclusionEvent.Instance.Offer(SeclusionEvent.Kind.SpiritVein, 1);
                return true;
            }).Close(() => DismissDramas());

            s.Shot("family-fund", c =>
            {
                if (FamilyEvent.Disabled || FamilyFundEvent.Members().Count == 0) return false;
                FamilyFundEvent.OpenAccounts();
                return g.ui.HasUI(UIType.NPCSearch);
            }).Close(() => CloseAll(UIType.NPCSearch, UIType.TextInfoLong));

            s.Shot("taoist-log", c =>
            {
                if ((TaoistLogEvent.Instance?.Entries.Count ?? 0) < 5) return false;
                TaoistLogEvent.Instance.OpenLog();
                return true;
            }).Close(() => CloseAll(UIType.TextInfoLong));

            return s.Step("cleanup", c =>
            {
                CloseAll(UIType.NPCSearch, UIType.TextInfoLong, UIType.TextInfo, UIType.NPCInfo);
                c.Info("done (the save is restored by run-autotest.ps1)");
            });
        }
    }
    /// <summary>
    /// TR-03 (in the game): houses and the city. A city (or a town) of the player: master, palace level 4, wall level 3,
    /// the player's immortal palace with its rooms, NPC cultivators' houses, grown mortal houses, three clans (a blood feud,
    /// an alliance); the town map, the house, the town management with its buildings, the clans, a clan war asking the
    /// player, the banknote bank. The save is restored after the run.
    /// </summary>
    public class TR03_PromoCity : IScenarioDefinition
    {
        public string Id => "TR-03";

        public Scenario Create()
        {
            MapBuildTown town = null;
            EstateEvent.Plot house = null;
            var heads = new List<WorldUnitBase>();
            var s = new Scenario { Id = "TR-03", Feature = "Promo", Title = "Promotion pictures: houses and the city", Kind = ScenarioKind.Tool, Overlay = false, SnapSteps = false }
                .Step("travel", c =>
                {
                    TravelTo(c, FindTown(x => x.IsCity()) ?? FindTown(x => true));
                    town = BagTown(c);
                    c.Info($"town: {town?.name} (city {town?.IsCity()})");
                    var trainer = FeatureEvent<TrainerEvent>(ModConst.TRAINER_EVENT);
                    for (var i = 0; i < 40 && Player.GetGradeLvl() < TR02_PromoGame.PROMO_GRADE; i++) trainer.Levelup();
                    if (Player.GetUnitMoney() < 1000000) Player.AddUnitMoney(1000000);
                    Player.AddUnitProp(SpiritNoteEvent.NOTE_ID, 120);
                    if (town != null && !MapBuildPropertyEvent.IsTownMaster(town, Player)) trainer.BecomeTownMaster();
                }, 1500).Confirm()
                .Step("the city lives", c =>
                {
                    if (town == null) return;
                    var e = EstateEvent.Get(town);
                    MapBuildPropertyEvent.AddBuildProperty(town, 50000000);
                    e.PalaceLevel = 4;
                    e.WallLevel = 3;
                    // the player's immortal palace with its rooms
                    foreach (var p in e.Plots.Where(x => x.OwnerId == Player.GetUnitId() && x.Level > 0).ToList()) EstateEvent.Release(e, p);
                    house = e.Plots.Where(x => x.OwnerId == null).OrderByDescending(x => EstateEvent.MaxLevelAt(e, x)).ThenByDescending(x => x.Prosperity).First();
                    var level = EstateEvent.MaxLevelAt(e, house);
                    var add = EstateEvent.Expansion(e, house, level, _ => 0) ?? new List<EstateEvent.Plot>();
                    house.OwnerId = Player.GetUnitId(); house.Level = level; house.MainIndex = null;
                    foreach (var a in add) EstateEvent.Take(a, house);
                    house.Rooms = new Dictionary<int, int> { [1] = 3, [2] = 2, [3] = 2, [4] = 1, [5] = 1 };
                    house.Herbs = new List<int>();
                    // grown mortal houses (years of a busy city)
                    e.Population = EstateEvent.BasePopulation(town) * 3 / 2;
                    var k = 0;
                    EstateEvent.GrowMortals(town, e, 120, () => k++ % 3 == 0 ? 0 : 0.99);
                    // NPC cultivators: families first (clans), then others
                    var families = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && !EstateEvent.HousesOf(x).Any() && ClanEvent.Members(x).Count >= 2 && ClanEvent.Members(x).All(m => !m.IsPlayer()))
                        .OrderByDescending(x => x.GetGradeLvl()).ToList();
                    foreach (var h in families)
                    {
                        if (heads.Count == 3) break;
                        var ids = ClanEvent.Members(h).Select(x => x.GetUnitId()).ToList();
                        if (heads.Any(o => ClanEvent.Members(o).Any(m => ids.Contains(m.GetUnitId())))) continue;
                        heads.Add(h);
                    }
                    var others = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && !EstateEvent.HousesOf(x).Any() && !heads.Contains(x)).OrderByDescending(x => x.GetGradeLvl()).Take(5);
                    foreach (var u in heads.Concat(others))
                    {
                        u.SetUnitPos(town.GetOrigiPoint());
                        u.SetUnitMoney(int.MaxValue / 2);
                        EstateEvent.NpcHouse(u, 1, () => 0, town);
                        for (var i = 0; i < EstateEvent.MAX_LEVEL; i++) EstateEvent.NpcHouse(u, 1, () => 0);
                        var h = EstateEvent.HouseOf(town, u);
                        if (h != null)
                        {
                            var next = Math.Min(EstateEvent.MAX_LEVEL, h.Level + 1);
                            var (more, _, price) = EstateEvent.UpgradeCost(town, h);
                            c.Info($"{u.GetName()} grade {u.GetGradeLvl()}: {EstateEvent.HouseName(h.Level)}; next: grade {EstateEvent.RequiredGrade(next)}, room {more != null}, price {price:#,##0} / half {u.GetUnitMoney() / 2:#,##0}");
                        }
                    }
                    // a blood feud and an alliance
                    if (heads.Count >= 3)
                    {
                        ClanEvent.Instance.Bases[ClanEvent.Key(town.buildData.id, heads[0].GetUnitId(), heads[1].GetUnitId())] = -100;
                        ClanEvent.Instance.Bases[ClanEvent.Key(town.buildData.id, heads[0].GetUnitId(), heads[2].GetUnitId())] = 80;
                        ClanEvent.Instance.Bases[ClanEvent.Key(town.buildData.id, heads[1].GetUnitId(), heads[2].GetUnitId())] = -45;
                    }
                    c.Info($"clans {ClanEvent.Heads(town).Count}, NPC houses {e.Plots.Count(x => x.OwnerId != null && x.OwnerId != Player.GetUnitId() && x.MainIndex == null)}, grown {EstateEvent.GrownLevels(e)}, house level {house.Level}");
                }, PromoShot.RENDER_MS);

            s.Shot("estate-town-map", c => { if (town == null) return false; EstateEvent.OpenPlots(town); return g.ui.HasUI(UIType.TextInfoLong); })
                .Close(() => CloseAll(UIType.TextInfoLong));
            s.Shot("estate-house", c => { if (town == null || house == null) return false; EstateEvent.OpenHouse(town, house); return g.ui.HasUI(UIType.TextInfoLong); })
                .Close(() => CloseAll(UIType.TextInfoLong));
            s.Shot("town-buildings", c =>
            {
                if (town == null || !MapBuildPropertyEvent.IsTownMaster(town, Player)) return false;
                // a building taken down: its "Build" button shows (the save is restored after the run)
                var down = MOD_nE7UL2.Enum.BuildingCostEnum.GetAllEnums<MOD_nE7UL2.Enum.BuildingCostEnum>().FirstOrDefault(x => !x.IsSchool && BuildingArrangeEvent.IsDestroyable(town, x));
                if (down != null) BuildingArrangeEvent.Destroy(town, down);
                MapBuildPropertyEvent.OpenUITownManage(town);
                return g.ui.HasUI(UIType.NPCSearch);
            }).Close(() => CloseAll(UIType.NPCSearch, UIType.TextInfoLong, UIType.TextInfo));
            s.Shot("clans", c => { if (town == null || ClanEvent.Heads(town).Count < 2) return false; ClanEvent.OpenClans(town); return g.ui.HasUI(UIType.TextInfoLong); })
                .Close(() => CloseAll(UIType.TextInfoLong));
            s.Shot("clan-war", c =>
            {
                if (town == null || heads.Count < 3) return false;
                // the feuding clan marches on the player's ally
                ClanEvent.Instance.Pending = null;
                ClanEvent.Ask(town, new ClanEvent.Fight
                {
                    TownId = town.buildData.id, War = true, Ours = heads[0].GetUnitId(), Theirs = heads[1].GetUnitId(),
                    OurFighter = ClanEvent.Fighter(heads[0], true).GetUnitId(), TheirFighter = ClanEvent.Fighter(heads[1], true).GetUnitId(),
                });
                return true;
            }, () => DramaUI() != null).Close(() => { DismissDramas(); ClanEvent.Instance.Pending = null; });
            s.Shot("spirit-banknotes", c => { if (town == null) return false; SpiritNoteEvent.OpenBank(town); return g.ui.HasUI(UIType.TextInfoLong); })
                .Close(() => CloseAll(UIType.TextInfoLong));

            return s.Step("cleanup", c =>
            {
                DismissDramas();
                CloseAll(UIType.NPCSearch, UIType.TextInfoLong, UIType.TextInfo);
                c.Info("done (the save is restored by run-autotest.ps1)");
            });
        }
    }
}
