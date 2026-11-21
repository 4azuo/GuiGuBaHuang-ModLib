using MOD_nE7UL2.Helper;
using EBattleTypeData;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Bounties (wanted notices) paid from a town budget.
    /// - A town master (the player from the NPC info, NPC masters once a year on units they hate, the player included)
    ///   posts a bounty on any NPC or the player; the reward is taken from the town budget at once (refunded when the
    ///   bounty expires after 36 months, is cancelled, or the target dies of something else).
    /// - Each month bounty hunters may go after the target (the higher the reward, the more often): an NPC target fights
    ///   a hunter (battle power comparison, the loser of the hunter side only gets hurt; a killed target pays the hunter);
    ///   the player is ambushed: fight the hunter, or pay 1.5 x the reward to the town to clear the name.
    /// - The player can hunt too: the bounty board (town bounty building) lists the wanted units of the area; killing a
    ///   target in a battle pays its reward.
    /// Off with S&amp;M "No bounty".
    /// </summary>
    [Cache(ModConst.WANTED_EVENT)]
    public class WantedEvent : ModEvent
    {
        public static WantedEvent Instance { get; set; }

        public const int EXPIRE_MONTHS = 36;
        public const float HUNT_RATE = 15f;
        public const float NPC_POST_RATE = 20f;
        public const float CLEAR_NAME_RATE = 1.5f;
        public const int NPC_POST_HATE = 200;

        public class Bounty
        {
            public string Id { get; set; } = Guid.NewGuid().ToString("N");
            public string TownId { get; set; }
            public string TargetId { get; set; }
            /// <summary>Unit id of the town master who posted it.</summary>
            public string PosterId { get; set; }
            public int Reward { get; set; }
            public int PostedMonth { get; set; }
            public int Attempts { get; set; }

            // the player is not found by g.world.unit.GetUnit
            [JsonIgnore] public WorldUnitBase Target => Unit(TargetId);
            [JsonIgnore] public WorldUnitBase Poster => Unit(PosterId);
            [JsonIgnore] public MapBuildTown Town => g.world.build.GetBuild(TownId)?.TryCast<MapBuildTown>();
            [JsonIgnore] public bool OnPlayer => TargetId == g.world.playerUnit.GetUnitId();
        }

        public List<Bounty> Bounties { get; set; } = new List<Bounty>();

        /// <summary>Running battle: an ambush of a bounty hunter on the player (bounty id).</summary>
        [JsonIgnore]
        public static string HuntBattleBountyId { get; set; }

        /// <summary>A unit by id, the player included.</summary>
        public static WorldUnitBase Unit(string id) =>
            id == null ? null : id == g.world.playerUnit.GetUnitId() ? g.world.playerUnit : g.world.unit.GetUnit(id);

        public static bool Disabled => SMLocalConfigsEvent.Instance?.Configs?.NoBounty ?? false;
        private static WorldUnitBase Player => g.world.playerUnit;

        /// <summary>Lowest reward on a unit: 1,000 x grade² (inflation).</summary>
        public static int MinReward(WorldUnitBase target)
        {
            var grade = Math.Max(1, target?.GetGradeLvl() ?? 1);
            return InflationaryEvent.CalculateInflationary(1000 * grade * grade);
        }

        /// <summary>Towns where the player is the master.</summary>
        public static List<MapBuildTown> PlayerTowns() =>
            (ModMain.Store?.Towns ?? new MapBuildTown[0]).Where(t => t != null && MapBuildPropertyEvent.IsTownMaster(t, Player)).ToList();

        public static List<Bounty> On(WorldUnitBase target) =>
            Instance?.Bounties.Where(x => target != null && x.TargetId == target.GetUnitId()).ToList() ?? new List<Bounty>();

        /// <summary>
        /// Posts a bounty: the reward leaves the town budget at once. Null when done, otherwise the reason (localText key).
        /// </summary>
        public string Post(MapBuildTown town, WorldUnitBase poster, WorldUnitBase target, int reward)
        {
            if (Disabled) return "wanted_err_off";
            if (town == null || poster == null || target == null || target.isDie) return "wanted_err_target";
            if (target.GetUnitId() == poster.GetUnitId()) return "wanted_err_target";
            if (Bounties.Any(x => x.TownId == town.buildData.id && x.TargetId == target.GetUnitId())) return "wanted_err_already";
            if (reward < MinReward(target)) return "wanted_err_min";
            if (MapBuildPropertyEvent.GetBuildProperty(town) < reward) return "wanted_err_budget";
            MapBuildPropertyEvent.AddBuildProperty(town, -reward);
            var b = new Bounty { TownId = town.buildData.id, TargetId = target.GetUnitId(), PosterId = poster.GetUnitId(), Reward = reward, PostedMonth = GameHelper.GetGameTotalMonth() };
            Bounties.Add(b);
            TaoistLogEvent.Log("log_wanted_post", town.name, target.GetName(), reward);
            DebugHelper.WriteLine($"WantedEvent: {town.name} posts {reward} on {target.GetName()}");
            if (b.OnPlayer)
                DramaHelper.OpenDrama2(string.Format(GameTool.LS("wanted_on_player"), town.name, reward), new List<string> { GameTool.LS("enc_leave") }, null, "xinjianhualuo1");
            return null;
        }

        /// <summary>Removes a bounty; the reward goes back to the town budget.</summary>
        public void Refund(Bounty b, string logKey)
        {
            if (b == null || !Bounties.Remove(b)) return;
            var town = b.Town;
            if (town != null) MapBuildPropertyEvent.AddBuildProperty(town, b.Reward);
            TaoistLogEvent.Log(logKey, b.Target?.GetName() ?? "?", b.Reward, town?.name);
        }

        /// <summary>The target is dead: the hunter gets the reward.</summary>
        public void Claim(Bounty b, WorldUnitBase hunter)
        {
            if (b == null || !Bounties.Remove(b)) return;
            hunter?.AddUnitMoney(b.Reward);
            TaoistLogEvent.Log("log_wanted_claimed", hunter?.GetName(), b.Target?.GetName() ?? "?", b.Reward);
            DebugHelper.WriteLine($"WantedEvent: {hunter?.GetName()} claims {b.Reward} for {b.Target?.GetName()}");
        }

        // ------------------------------------------------------------------ monthly / yearly

        /// <summary>Expired/dead targets refunded; hunters go after the targets.</summary>
        public override void OnMonthly()
        {
            base.OnMonthly();
            if (Disabled)
                return;
            var cur = GameHelper.GetGameTotalMonth();
            foreach (var b in Bounties.ToArray())
            {
                var target = b.Target;
                if (cur - b.PostedMonth >= EXPIRE_MONTHS)
                    Refund(b, "log_wanted_expired");
                else if (target == null || target.isDie)
                    Refund(b, "log_wanted_dead");
                else if (b.OnPlayer && SeclusionEvent.IsSecluded)
                    continue; // nobody finds the player in seclusion
                else if (CommonTool.Random(0f, 100f).IsBetween(0f, ModLib.Mod.ModMaster.ChanceOver(HuntChance(b), ModLib.Mod.ModMaster.MonthsPassed)))
                    Hunt(b);
            }
        }

        /// <summary>Chance (%) that a hunter goes after the target this month: 15 % x reward / minimum reward (max x3).</summary>
        public static float HuntChance(Bounty b) => HUNT_RATE * Math.Min(3f, b.Reward / (float)Math.Max(1, MinReward(b.Target)));

        /// <summary>A bounty hunter: an NPC at least as strong as the target (the weakest of them), not in its team.</summary>
        public static WorldUnitBase FindHunter(Bounty b)
        {
            var target = b.Target;
            if (target == null) return null;
            var team = HirePeopleEvent.GetTeamDetailData(target).Item2.Select(x => x.GetUnitId()).ToList();
            var pool = (ModMain.Store?.WUnits ?? new WorldUnitBase[0])
                .Where(x => x != null && !x.isDie && !x.IsPlayer() && x.GetUnitId() != target.GetUnitId() && x.GetUnitId() != b.PosterId && !team.Contains(x.GetUnitId()))
                .ToList();
            return pool.Where(x => x.GetGradeLvl() >= target.GetGradeLvl()).OrderBy(x => x.GetGradeLvl()).ThenBy(x => CommonTool.Random(0, 1000)).FirstOrDefault()
                ?? pool.OrderByDescending(x => x.GetGradeLvl()).FirstOrDefault();
        }

        /// <summary>A hunter goes after the target (roll 0..100 decides an NPC fight; below 0: random).</summary>
        public void Hunt(Bounty b, WorldUnitBase hunter = null, float roll = -1f)
        {
            hunter = hunter ?? FindHunter(b);
            var target = b.Target;
            if (hunter == null || target == null)
                return;
            b.Attempts++;
            if (b.OnPlayer)
            {
                Ambush(b, hunter);
                return;
            }
            var hp = UnitPowerCalHelper.CalWUnitBattlePower(hunter);
            var tp = UnitPowerCalHelper.CalWUnitBattlePower(target);
            var winChance = 100.0 * hp / Math.Max(1.0, hp + tp);
            if (roll < 0f) roll = CommonTool.Random(0f, 100f);
            if (roll < winChance)
            {
                target.Die();
                Claim(b, hunter);
                if (b.PosterId == Player.GetUnitId())
                    InfoDialog.Msg(GameTool.LS("wanted_title"), string.Format(GameTool.LS("wanted_done"), hunter.GetName(), target.GetName(), b.Reward));
            }
            else
            {
                target.data.unitData.relationData.AddHate(hunter.GetUnitId(), 100);
                TaoistLogEvent.Log("log_wanted_failed", hunter.GetName(), target.GetName());
            }
        }

        /// <summary>A hunter finds the player: fight, or pay 1.5 x the reward to the town to clear the name.</summary>
        public void Ambush(Bounty b, WorldUnitBase hunter)
        {
            if (GameHelper.IsInBattlle() || (TimeSkipEvent.Instance?.IsProcessing ?? false))
                return;
            // the spirit sense feels the hunter coming: the player slips away
            if (SpiritSenseEvent.SenseAmbush(hunter))
                return;
            var price = ClearNamePrice(b);
            hunter.SetUnitPos(Player.GetUnitPos());
            TaoistLogEvent.Log("log_wanted_ambush", hunter.GetName(), b.Town?.name);
            DramaHelper.OpenDrama1(string.Format(GameTool.LS("wanted_ambush"), b.Town?.name, b.Reward), new List<string>
            {
                GameTool.LS("wanted_ambush_o1"), string.Format(GameTool.LS("wanted_ambush_o2"), price),
            }, x => OnAmbushChoice(b, hunter, x.id % 10), hunter, Player);
        }

        public static int ClearNamePrice(Bounty b) => (int)Math.Ceiling(b.Reward * CLEAR_NAME_RATE);

        /// <summary>0: fight the hunter; 1: clear the name (pay the town, the bounty is cancelled and refunded to it).</summary>
        public void OnAmbushChoice(Bounty b, WorldUnitBase hunter, int opt)
        {
            var price = ClearNamePrice(b);
            if (opt == 1 && Player.GetUnitMoney() >= price)
            {
                Player.AddUnitMoney(-price);
                var town = b.Town;
                if (town != null) MapBuildPropertyEvent.AddBuildProperty(town, price);
                Refund(b, "log_wanted_cleared");
                return;
            }
            HuntBattleBountyId = b.Id;
            g.world.battle.IntoBattle(hunter, MapBuildPropertyEvent.CATCH_SNEAKY_DUNGEON_ID, battleData: new WorldBattleData { npcUnit = hunter });
        }

        /// <summary>Ambush battle ended: the player lost -> the hunter takes the reward (the bounty is done).</summary>
        public override void OnBattleEndHandler(BattleEndHandler e)
        {
            base.OnBattleEndHandler(e);
            if (HuntBattleBountyId == null)
                return;
            var b = Bounties.FirstOrDefault(x => x.Id == HuntBattleBountyId);
            HuntBattleBountyId = null;
            if (b != null && !e.isWin)
                Claim(b, ModBattleEvent.SceneBattle != null ? g.world.unit.GetUnit(ModBattleEvent.BattleUnits.ToArray().FirstOrDefault(x => x.IsWorldUnit() && !x.IsPlayer())) : null);
        }

        /// <summary>The player kills a wanted unit in a battle: the reward is paid.</summary>
        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);
            if (Disabled || e?.unit == null || !e.unit.IsWorldUnit())
                return;
            var killer = e.hitData?.attackUnit?.GetWorldUnit();
            if (killer == null || !killer.IsPlayer())
                return;
            var dead = e.unit.GetWorldUnit();
            foreach (var b in On(dead))
            {
                Claim(b, Player);
                TaoistLogEvent.Log("log_wanted_player_claimed", dead.GetName(), b.Reward, b.Town?.name);
            }
        }

        /// <summary>Yearly: NPC town masters post bounties on a unit they hate (the player included).</summary>
        public override void OnYearly()
        {
            base.OnYearly();
            if (Disabled)
                return;
            foreach (var town in ModMain.Store?.Towns ?? new MapBuildTown[0])
            {
                var master = town == null ? null : MapBuildPropertyEvent.GetTownMaster(town);
                if (master == null || master.IsPlayer() || !CommonTool.Random(0f, 100f).IsBetween(0f, ModLib.Mod.ModMaster.ChanceOver(NPC_POST_RATE, ModLib.Mod.ModMaster.YearsPassed)))
                    continue;
                var hated = master.data.unitData.relationData.GetAllRelation().ToArray()
                    .Select(id => g.world.unit.GetUnit(id)).Where(x => x != null && !x.isDie && master.data.unitData.relationData.GetIntim(x) <= -NPC_POST_HATE)
                    .OrderBy(x => master.data.unitData.relationData.GetIntim(x)).FirstOrDefault();
                if (hated == null)
                    continue;
                var reward = Math.Max(MinReward(hated), (int)Math.Min(int.MaxValue, MapBuildPropertyEvent.GetBuildProperty(town) / 10));
                Post(town, master, hated, reward);
            }
        }

        // ------------------------------------------------------------------ UI

        /// <summary>NPC info (the player is a town master): "Post a bounty". Bounty board: "Wanted list".</summary>
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (Disabled)
                return;
            if (e.uiType.uiName == UIType.NPCInfo.uiName && IsOpenWantedList)
            {
                ShowBountyInfo(e.ui);
            }
            else if (e.uiType.uiName == UIType.NPCInfo.uiName && !RealMarketEvent3.IsOpenMarket && !RealMarketEvent3.IsOpenMySellingList && PlayerTowns().Count > 0)
            {
                var cover = new UICover<UINPCInfo>(e.ui);
                cover.AddButton(200f, 140f, () => OpenPost(cover.UI.unit), GameTool.LS("wanted_post")).Format(Color.black).Size(100, 40)
                    .SetParentTransform(cover.UI.uiProperty.textAddLuckTitle);
            }
            else if (e.uiType.uiName == UIType.TownBounty.uiName)
            {
                var cover = new UICover<UITownBounty>(e.ui);
                cover.AddButton(cover.LastCol - 6, cover.FirstRow + 5, () => OpenList(), GameTool.LS("wanted_list")).Size(200, 40);
            }
        }

        /// <summary>Post window: the town (first one of the player), the reward (budget), post.</summary>
        public void OpenPost(WorldUnitBase target)
        {
            var town = PlayerTowns().FirstOrDefault();
            if (town == null || target == null)
                return;
            g.ui.CloseUI(UIType.TextInfoLong);
            var ui = new UICustom1(GameTool.LS("wanted_title"));
            var row = ui.FirstRow + 2;
            var min = MinReward(target);
            var max = (int)Math.Min(int.MaxValue / 2, Math.Max(min, MapBuildPropertyEvent.GetBuildProperty(town)));
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("wanted_post_desc"), target.GetName(), target.GetGradeLvl(), town.name)).Format(null, 16);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("wanted_post_budget"), MapBuildPropertyEvent.GetBuildProperty(town), min)).Format(null, 14);
            row++;
            var reward = ui.AddCompositeSlider(ui.MidCol - 3, row++, GameTool.LS("wanted_reward"), min, max, min, "{0}");
            reward.Postfix.SetWork(new UIItemWork { Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { reward.MainComponent.Get().Parse<int>() }) });
            ui.AddText(ui.MidCol, row++, GameTool.LS("wanted_post_rule")).Format(null, 13);
            ui.IsAutoUpdate = true;
            row++;
            ui.AddButton(ui.MidCol, row, () =>
            {
                var err = Post(town, Player, target, reward.MainComponent.Get().Parse<int>());
                InfoDialog.Msg(GameTool.LS("wanted_title"), err == null ? string.Format(GameTool.LS("wanted_posted"), target.GetName()) : GameTool.LS(err));
                if (err == null) g.ui.CloseUI(UIType.TextInfoLong);
            }, GameTool.LS("wanted_post")).Size(200, 40);
        }

        /// <summary>The wanted list (NPC search) is open: the NPC info shows the bounty details, first tab only.</summary>
        [JsonIgnore]
        public static bool IsOpenWantedList { get; private set; }

        /// <summary>Wanted list: portraits of the wanted units (NPC search window); a click shows the details.</summary>
        public void OpenList()
        {
            var ui = g.ui.OpenUISafe<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = Bounties.Select(x => x.Target).Where(x => x != null && !x.isDie).GroupBy(x => x.GetUnitId()).Select(x => x.First()).ToIl2CppList();
            ui.UpdateUI();
            IsOpenWantedList = true;
            var cover = new UICover<UINPCSearch>(ui);
            cover.AddText(cover.MidCol, cover.FirstRow + 2, ui.units.Count == 0 ? GameTool.LS("wanted_none") : GameTool.LS("wanted_list_desc")).Format(Color.white, 18);
        }

        /// <summary>NPC info opened from the wanted list: first tab only, with the bounties on the unit.</summary>
        private void ShowBountyInfo(UIBase uiBase)
        {
            var cover = new UICover<UINPCInfo>(uiBase);
            var info = cover.UI;
            foreach (var t in new[] { info.tglTitle2, info.tglTitle3, info.tglTitle4, info.tglTitle5, info.tglTitle6, info.tglTitle7 })
                t?.gameObject.SetActive(false);
            if (info.tglTitle1 != null) info.tglTitle1.isOn = true;
            var cur = GameHelper.GetGameTotalMonth();
            // free space between the basic info and the stats table (relative to the luck title, like the team buttons)
            const float x = -180f;
            var y = 70f;
            cover.AddText(x, y, GameTool.LS("wanted_title")).Align().Format(Color.red, 18).SetParentTransform(info.uiProperty.textAddLuckTitle);
            foreach (var b in On(info.unit))
            {
                var bb = b;
                y -= 22f;
                cover.AddText(x, y, string.Format(GameTool.LS("wanted_detail"), b.Reward, b.Town?.name, b.Poster?.GetName(), EXPIRE_MONTHS - (cur - b.PostedMonth)))
                    .Align().Format(Color.white, 15).SetParentTransform(info.uiProperty.textAddLuckTitle);
                if (b.PosterId == Player.GetUnitId())
                    cover.AddButton(x + 420f, y, () => { Refund(bb, "log_wanted_cancel"); g.ui.CloseUI(uiBase); }, GameTool.LS("other500020090")).Format(Color.black).Size(100, 30)
                        .SetParentTransform(info.uiProperty.textAddLuckTitle);
            }
        }

        /// <summary>Wanted list closed.</summary>
        public override void OnCloseUIStart(CloseUIStart e)
        {
            base.OnCloseUIStart(e);
            if (e.uiType.uiName == UIType.NPCSearch.uiName)
                IsOpenWantedList = false;
        }
    }
}
