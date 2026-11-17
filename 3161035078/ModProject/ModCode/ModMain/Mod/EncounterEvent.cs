using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Random encounters: at most one a month (S&M EncounterRate %, default 3), on the map, not during a time skip. Each one is a
    /// drama with choices and a reward or a price:
    /// - Wounded cultivator (an NPC nearby): pay for medicine (friendship + a herb) / rob him (his money, his hatred) / walk on.
    /// - Chess hermit: play for a stake (win chance from Talent: exp + the stake; lose: the stake) / decline.
    /// - Hooded peddler (in a town): buy a sealed box (nothing / an item of the player's grade / of the next grade),
    ///   haggle (30 % off or he leaves) / ignore.
    /// - Collapsed cave (outside towns): explore (materials / spirit stones / injury) / leave.
    /// - Spirit beast cub (outside towns): feed it a herb (+Luck) / leave it.
    /// - Town festival (in a town): join (Mood, the townspeople like the player more) / skip.
    /// Every result is written in the Taoist log.
    /// </summary>
    [Cache(ModConst.ENCOUNTER_EVENT)]
    public class EncounterEvent : ModEvent
    {
        public static EncounterEvent Instance { get; set; }

        public enum Encounter { Wounded, Chess, Peddler, Cave, Cub, Festival }

        /// <summary>Encounters on/off (Taoist Settings).</summary>
        public bool Enabled { get; set; } = true;
        /// <summary>Chance of an encounter each month (%, S&M).</summary>
        public static float EncounterRate => SMLocalConfigsEvent.Instance?.Configs?.EncounterRate ?? 3f;
        /// <summary>Encounters seen (statistics).</summary>
        public int Count { get; set; }

        private static WorldUnitBase Player => g.world.playerUnit;
        private static int Grade => Math.Max(1, Player.GetGradeLvl());
        private static MapBuildTown Town => Player.GetMapBuild<MapBuildTown>();

        /// <summary>Base price of an encounter (100 x grade, inflation).</summary>
        public static int Cost(int mul = 1) => InflationaryEvent.CalculateInflationary(100 * Grade * mul);

        /// <summary>Win chance of the chess game (%): 25 + Talent / 4, 25..75.</summary>
        public static float ChessChance() => Math.Max(25f, Math.Min(75f, 25f + Player.GetDynProperty(UnitDynPropertyEnum.Talent).value / 4f));

        /// <summary>Monthly: maybe one encounter.</summary>
        public override void OnMonthly()
        {
            base.OnMonthly();
            if (!Enabled || (SMLocalConfigsEvent.Instance?.Configs?.NoEncounter ?? false) || GameHelper.IsInBattlle() || (TimeSkipEvent.Instance?.IsProcessing ?? false))
                return;
            if (!CommonTool.Random(0f, 100f).IsBetween(0f, EncounterRate))
                return;
            var possible = Possible();
            if (possible.Count == 0)
                return;
            Open(possible[CommonTool.Random(0, possible.Count * 1000) % possible.Count]);
        }

        // ------------------------------------------------------------------ NPC fortunes

        /// <summary>NPC fortune (% a year for each NPC): a small encounter (exp, an item).</summary>
        public const float NPC_FORTUNE_RATE = 0.2f;
        /// <summary>NPC inheritance (% a year for each NPC): an ancient legacy (a lot of exp, Talent, Luck).</summary>
        public const float NPC_INHERITANCE_RATE = 0.02f;
        public const int NPC_FORTUNES_A_YEAR = 10;

        /// <summary>NPC fortune of one NPC (roll 0..100 picks the kind). Returns the log key.</summary>
        public static string NpcFortune(WorldUnitBase npc, bool inheritance)
        {
            var phase = Math.Max(50, npc.GetMaxExpCurrentPhase() - npc.GetMinExpCurrentPhase());
            if (inheritance)
            {
                npc.AddExp(phase);
                npc.AddProperty<int>(UnitPropertyEnum.Talent, 3);
                npc.AddProperty<int>(UnitPropertyEnum.Luck, 1);
                return "log_npc_inheritance";
            }
            npc.AddExp(phase / 10);
            return "log_npc_fortune";
        }

        /// <summary>Yearly: a few NPCs find their own fortune (very rare); known people are written in the Taoist log.</summary>
        [ModLib.Attributes.EventCondition(IsInGame = HandleEnum.True)]
        public override void OnYearly()
        {
            base.OnYearly();
            if (!Enabled || (SMLocalConfigsEvent.Instance?.Configs?.NoEncounter ?? false))
                return;
            var count = 0;
            var friends = new HashSet<string>(Player.data.unitData.relationData.GetAllGoodRelationUnitID(false, false).ToArray());
            foreach (var npc in (ModMain.Store?.WUnits ?? new WorldUnitBase[0]).Where(x => x != null && !x.isDie && !x.IsPlayer()))
            {
                if (count >= NPC_FORTUNES_A_YEAR * Math.Max(1, ModLib.Mod.ModMaster.YearsPassed)) break;
                var roll = CommonTool.Random(0f, 100f);
                var inheritance = roll < ModLib.Mod.ModMaster.ChanceOver(NPC_INHERITANCE_RATE, ModLib.Mod.ModMaster.YearsPassed);
                if (!inheritance && roll >= ModLib.Mod.ModMaster.ChanceOver(NPC_FORTUNE_RATE, ModLib.Mod.ModMaster.YearsPassed))
                    continue;
                var key = NpcFortune(npc, inheritance);
                count++;
                TaoistLogEvent.LogIf(friends.Contains(npc.GetUnitId()) || FamilyEvent.IsFamilyOfPlayer(npc), key, npc.GetName());
            }
        }

        /// <summary>Encounters possible where the player is.</summary>
        public static List<Encounter> Possible()
        {
            var rs = new List<Encounter> { Encounter.Chess };
            if (Town != null)
            {
                rs.Add(Encounter.Peddler);
                rs.Add(Encounter.Festival);
            }
            else
            {
                rs.Add(Encounter.Cave);
                rs.Add(Encounter.Cub);
                if (WoundedNpc() != null) rs.Add(Encounter.Wounded);
            }
            return rs;
        }

        private static WorldUnitBase WoundedNpc() =>
            Player.GetUnitsAround(8, false, false).ToArray().FirstOrDefault(x => x != null && !x.isDie && !x.IsPlayer() && !HirePeopleEvent.IsHired(x));

        // ------------------------------------------------------------------ open

        /// <summary>Opens an encounter drama (its options call Resolve).</summary>
        public static void Open(Encounter e)
        {
            Instance.Count++;
            DebugHelper.WriteLine($"EncounterEvent: {e}");
            switch (e)
            {
                case Encounter.Wounded:
                    var npc = WoundedNpc();
                    if (npc == null) return;
                    DramaHelper.OpenDrama1(GameTool.LS("enc_wounded"), new List<string>
                    {
                        string.Format(GameTool.LS("enc_wounded_o1"), Cost(2)), GameTool.LS("enc_wounded_o2"), GameTool.LS("enc_wounded_o3"),
                    }, x => Resolve(e, Opt(x), npc), npc, Player);
                    break;
                case Encounter.Chess:
                    DramaHelper.OpenDrama2(string.Format(GameTool.LS("enc_chess"), Cost(3)), new List<string>
                    {
                        string.Format(GameTool.LS("enc_chess_o1"), ChessChance()), GameTool.LS("enc_chess_o2"),
                    }, x => Resolve(e, Opt(x)), Background(Encounter.Chess));
                    break;
                case Encounter.Peddler:
                    DramaHelper.OpenDrama2(string.Format(GameTool.LS("enc_peddler"), Cost(5)), new List<string>
                    {
                        GameTool.LS("enc_peddler_o1"), GameTool.LS("enc_peddler_o2"), GameTool.LS("enc_peddler_o3"),
                    }, x => Resolve(e, Opt(x)), Background(Encounter.Peddler));
                    break;
                case Encounter.Cave:
                    DramaHelper.OpenDrama2(GameTool.LS("enc_cave"), new List<string> { GameTool.LS("enc_cave_o1"), GameTool.LS("enc_cave_o2") },
                        x => Resolve(e, Opt(x)), Background(Encounter.Cave));
                    break;
                case Encounter.Cub:
                    DramaHelper.OpenDrama2(GameTool.LS("enc_cub"), new List<string> { GameTool.LS("enc_cub_o1"), GameTool.LS("enc_cub_o2") },
                        x => Resolve(e, Opt(x)), Background(Encounter.Cub));
                    break;
                case Encounter.Festival:
                    DramaHelper.OpenDrama2(string.Format(GameTool.LS("enc_festival"), Town?.name, Cost()), new List<string> { GameTool.LS("enc_festival_o1"), GameTool.LS("enc_festival_o2") },
                        x => Resolve(e, Opt(x)), Background(Encounter.Festival));
                    break;
            }
        }

        /// <summary>Drama picture of an encounter (chosen to match its text).</summary>
        public static string Background(Encounter e)
        {
            switch (e)
            {
                case Encounter.Chess: return "xianrencijiu";        // bamboo grove, go board and tea
                case Encounter.Peddler: return "hundengshangren";   // hooded figure with a lantern
                case Encounter.Cave: return "dixiamushi";           // dark opening in the rock
                case Encounter.Cub: return "xingxingqiyu";          // small beast lying under a tree
                case Encounter.Festival: return "shimengzhai_denglong"; // house with lanterns at night
                default: return "huapaonanziganxie";                // cultivator bowing
            }
        }

        // option index from the default option ids of DramaHelper (999790000 + index)
        private static int Opt(ConfDramaOptionsItem x) => x.id % 10;

        // ------------------------------------------------------------------ results

        /// <summary>
        /// Applies the chosen option (index from 0). roll: 0..100 (random when below 0), used by chance results.
        /// Returns the result text key (null: nothing happened).
        /// </summary>
        public static string Resolve(Encounter e, int opt, WorldUnitBase npc = null, float roll = -1f)
        {
            if (roll < 0f) roll = CommonTool.Random(0f, 100f);
            string key = null;
            object[] args = new object[0];
            switch (e)
            {
                case Encounter.Wounded:
                    if (opt == 0)
                    {
                        if (!Pay(Cost(2))) { key = "enc_nomoney"; break; }
                        npc?.data.unitData.relationData.AddIntim(Player.GetUnitId(), 100);
                        if (npc != null) Player.data.unitData.relationData.AddIntim(npc.GetUnitId(), 100);
                        var herb = GiveItem(x => x.IsHerbItem() != null, Grade);
                        key = "enc_wounded_r1"; args = new object[] { herb };
                    }
                    else if (opt == 1 && npc != null)
                    {
                        var take = Math.Max(1, npc.GetUnitMoney() * 15 / 100);
                        npc.AddUnitMoney(-take);
                        Player.AddUnitMoney(take);
                        npc.data.unitData.relationData.AddHate(Player.GetUnitId(), 300);
                        key = "enc_wounded_r2"; args = new object[] { take };
                    }
                    break;
                case Encounter.Chess:
                    if (opt == 0)
                    {
                        var stake = Cost(3);
                        if (Player.GetUnitMoney() < stake) { key = "enc_nomoney"; break; }
                        if (roll < ChessChance())
                        {
                            var exp = 100 * Grade;
                            Player.AddExp(exp);
                            Player.AddUnitMoney(stake);
                            key = "enc_chess_win"; args = new object[] { exp, stake };
                        }
                        else
                        {
                            Player.AddUnitMoney(-stake);
                            key = "enc_chess_lose"; args = new object[] { stake };
                        }
                    }
                    break;
                case Encounter.Peddler:
                    if (opt == 0 || opt == 1)
                    {
                        var price = Cost(5);
                        if (opt == 1)
                        {
                            // haggle: the first half of the roll gets 30 % off, the second half makes him leave
                            if (roll >= 50f) { key = "enc_peddler_leave"; break; }
                            price = price * 7 / 10;
                            roll *= 2f; // reuse the roll for the box
                        }
                        if (!Pay(price)) { key = "enc_nomoney"; break; }
                        if (roll < 10f) { key = "enc_peddler_empty"; break; }
                        var item = GiveItem(x => x.IsHerbItem() != null || x.IsMineItem() != null || x.IsPill() != null, roll < 15f ? Grade + 1 : Grade);
                        key = opt == 1 ? "enc_peddler_deal" : "enc_peddler_item"; args = new object[] { item, price };
                    }
                    break;
                case Encounter.Cave:
                    if (opt == 0)
                    {
                        if (roll < 30f)
                        {
                            var items = new List<string>();
                            for (var i = 0; i < (roll < 10f ? 2 : 1); i++)
                                items.Add(GiveItem(x => x.IsHerbItem() != null || x.IsMineItem() != null, Grade));
                            key = "enc_cave_items"; args = new object[] { string.Join(", ", items) };
                        }
                        else if (roll < 55f)
                        {
                            var stones = Cost(2);
                            Player.AddUnitMoney(stones);
                            key = "enc_cave_stones"; args = new object[] { stones };
                        }
                        else
                        {
                            var hpMax = Player.GetDynProperty(UnitDynPropertyEnum.HpMax).value;
                            var hp = Player.GetProperty<int>(UnitPropertyEnum.Hp);
                            var lost = Math.Min(hpMax * 3 / 10, Math.Max(0, hp - 1));
                            Player.SetProperty<int>(UnitPropertyEnum.Hp, hp - lost);
                            key = "enc_cave_hurt"; args = new object[] { lost };
                        }
                    }
                    break;
                case Encounter.Cub:
                    if (opt == 0)
                    {
                        var herb = Player.GetUnitProps().FirstOrDefault(x => x.propsItem?.IsHerbItem() != null);
                        if (herb == null) { key = "enc_cub_noherb"; break; }
                        Player.RemoveUnitProp(herb.soleID, 1);
                        var luck = roll < 10f ? 2 : 1;
                        Player.AddProperty<int>(UnitPropertyEnum.Luck, luck);
                        key = "enc_cub_r"; args = new object[] { luck };
                    }
                    break;
                case Encounter.Festival:
                    if (opt == 0)
                    {
                        if (!Pay(Cost(), Town)) { key = "enc_nomoney"; break; }
                        var moodMax = Player.GetProperty<int>(UnitPropertyEnum.MoodMax);
                        var mood = Player.GetProperty<int>(UnitPropertyEnum.Mood);
                        var add = Math.Max(0, Math.Min(20, moodMax - mood));
                        Player.AddProperty<int>(UnitPropertyEnum.Mood, add);
                        var town = Town;
                        if (town != null)
                            foreach (var w in g.world.unit.GetUnitExact(town.GetOrigiPoint(), 6).ToArray().Where(x => !x.IsPlayer()).Take(5))
                                w.data.unitData.relationData.AddIntim(Player.GetUnitId(), 20);
                        key = "enc_festival_r"; args = new object[] { add };
                    }
                    break;
            }
            if (key != null)
            {
                var text = string.Format(GameTool.LS(key), args);
                TaoistLogEvent.Log(key, args);
                DebugHelper.WriteLine($"EncounterEvent: {e} option {opt} -> {key} ({text})");
                ModDelayEvent.Instance.DelayEvent(Instance, () =>
                {
                    // the wounded cultivator answers in person; the other results keep the picture of their encounter
                    if (e == Encounter.Wounded && npc != null)
                        DramaHelper.OpenDrama1(text, new List<string> { GameTool.LS("enc_leave") }, null, npc, Player);
                    else
                        DramaHelper.OpenDrama2(text, new List<string> { GameTool.LS("enc_leave") }, null, Background(e));
                }, 300);
            }
            return key;
        }

        /// <summary>Pays spirit stones (to the town budget when in a town); false when the player cannot.</summary>
        private static bool Pay(int cost, MapBuildTown town = null)
        {
            if (Player.GetUnitMoney() < cost)
                return false;
            Player.AddUnitMoney(-cost);
            if (town != null)
                MapBuildPropertyEvent.AddBuildProperty(town, cost);
            return true;
        }

        /// <summary>Gives one random item of the kind, of the grade (nearest grade when none); returns its name.</summary>
        public static string GiveItem(Func<ConfItemPropsItem, bool> kind, int grade)
        {
            var conf = PickItem(kind, grade);
            if (conf == null)
                return string.Empty;
            Player.AddUnitProp(conf.id, 1);
            return GameTool.LS(conf.name);
        }

        /// <summary>One random sellable item of the kind, of the grade (nearest grade when none); null when none.</summary>
        public static ConfItemPropsItem PickItem(Func<ConfItemPropsItem, bool> kind, int grade)
        {
            var all = g.conf.itemProps._allConfList.ToArray().Where(x => x.sale > 0 && kind(x)).ToArray();
            if (all.Length == 0)
                return null;
            var best = all.Min(x => Math.Abs(x.Grade() - grade));
            var pool = all.Where(x => Math.Abs(x.Grade() - grade) == best).ToArray();
            return pool[CommonTool.Random(0, pool.Length * 1000) % pool.Length];
        }
    }
}
