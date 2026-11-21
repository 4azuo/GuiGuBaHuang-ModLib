using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Const;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Household of the player (FamilyEvent): the companion, the living descendants and the children not grown up yet.
    /// - Upkeep (monthly): every member costs spirit stones by grade (a child not grown up costs a little); paid from the
    ///   player's money, then from the spirit stones of the bank storage; what cannot be paid cools every member (-5 intimacy).
    /// - Gifts (monthly): every grown-up member has a small chance to bring spirit stones or a precious item of their
    ///   grade; it goes to the bank storage.
    /// - Use (monthly): a small chance that a member takes one of the gifted items still in the storage when in need.
    /// - Accounts window (map button "Family"): the members (NPC search window), the monthly upkeep, the totals of the year
    ///   and the last entries.
    /// Off with S&amp;M "No family system".
    /// </summary>
    [Cache(ModConst.FAMILY_FUND_EVENT)]
    public class FamilyFundEvent : ModEvent
    {
        public static FamilyFundEvent Instance { get; set; }

        public const int MINOR_UPKEEP = 20;
        public const int UPKEEP_PER_GRADE2 = 30;
        public const float GIFT_RATE = 1.5f;        // % a month for every grown-up member
        public const float GIFT_ITEM_SHARE = 35f;   // % of the gifts that are items
        public const float USE_RATE = 2f;           // % a month that a member takes one gifted item
        public const int UNPAID_INTIM = -5;
        public const int MAX_LEDGER = 200;

        public enum Kind { Upkeep, Unpaid, GiftStones, GiftItem, Used }

        public class Entry
        {
            public int Month { get; set; }
            public Kind Kind { get; set; }
            public string UnitId { get; set; }
            public string Name { get; set; }
            public long Amount { get; set; }
            public int ItemId { get; set; }
        }

        public List<Entry> Ledger { get; set; } = new List<Entry>();
        /// <summary>Items gifted by the family still owed to it (item id -> count), the only ones it may take back.</summary>
        public Dictionary<int, int> GiftedItems { get; set; } = new Dictionary<int, int>();

        private static WorldUnitBase Player => g.world.playerUnit;

        // ------------------------------------------------------------------ formulas (pure, unit tests)

        /// <summary>Monthly upkeep of a member before inflation: grade² x 30, a child not grown up 20.</summary>
        public static int BaseUpkeep(int grade, bool minor) => minor ? MINOR_UPKEEP : UPKEEP_PER_GRADE2 * Math.Max(1, grade) * Math.Max(1, grade);

        /// <summary>Spirit stones of a gift before inflation: 2 to 4 months of the member's upkeep (by the roll 0-99).</summary>
        public static int BaseGift(int grade, int roll) => BaseUpkeep(grade, false) * (2 + Math.Max(0, Math.Min(99, roll)) / 34);

        /// <summary>Split of an upkeep between the money and the storage stones; the rest is unpaid.</summary>
        public static (long fromMoney, long fromStorage, long unpaid) Split(long upkeep, long money, long storage)
        {
            var a = Math.Min(upkeep, Math.Max(0, money));
            var b = Math.Min(upkeep - a, Math.Max(0, storage));
            return (a, b, upkeep - a - b);
        }

        // ------------------------------------------------------------------ household

        /// <summary>Living companion and descendants (grown up) of the player.</summary>
        public static List<WorldUnitBase> Members()
        {
            var rs = new List<WorldUnitBase>();
            var spouse = FamilyEvent.Unit(FamilyEvent.Spouse(Player));
            if (spouse != null && !spouse.isDie) rs.Add(spouse);
            rs.AddRange(FamilyEvent.Heirs());
            return rs.GroupBy(x => x.GetUnitId()).Select(x => x.First()).ToList();
        }

        /// <summary>Children not grown up yet of the player and of its descendants.</summary>
        public static List<FamilyEvent.Child> Minors()
        {
            var ids = new HashSet<string>(Members().Select(x => x.GetUnitId())) { Player.GetUnitId() };
            return FamilyEvent.Instance?.Children.Where(x => !x.Grown && (ids.Contains(x.FatherId) || ids.Contains(x.MotherId))).ToList() ?? new List<FamilyEvent.Child>();
        }

        public static long Upkeep(WorldUnitBase u) => InflationaryEvent.CalculateInflationary(BaseUpkeep(u.GetGradeLvl(), false));
        public static long MinorUpkeep() => InflationaryEvent.CalculateInflationary(BaseUpkeep(1, true));
        /// <summary>Monthly upkeep of the household, lowered by an ancestral hall in one of the player's houses (EstateEvent).</summary>
        public static long MonthlyUpkeep() => (long)Math.Round((Members().Sum(Upkeep) + Minors().Count * MinorUpkeep()) * EstateEvent.UpkeepRate());

        private static int StorageStones() => RealStorageEvent.GetStorageSpiritStones();

        // ------------------------------------------------------------------ monthly

        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnMonthly()
        {
            base.OnMonthly();
            if (FamilyEvent.Disabled)
                return;
            // one dispatch may stand for several months (time skip)
            var months = Math.Max(1, ModLib.Mod.ModMaster.MonthsPassed);
            try { PayUpkeep(months); } catch (Exception ex) { DebugHelper.WriteLine($"FamilyFundEvent.PayUpkeep: {ex}"); }
            for (var m = 0; m < months; m++)
            {
                try { Gifts(); } catch (Exception ex) { DebugHelper.WriteLine($"FamilyFundEvent.Gifts: {ex}"); }
                try { UseGifts(); } catch (Exception ex) { DebugHelper.WriteLine($"FamilyFundEvent.UseGifts: {ex}"); }
            }
            if (Ledger.Count > MAX_LEDGER) Ledger.RemoveRange(0, Ledger.Count - MAX_LEDGER);
        }

        public void PayUpkeep(int months = 1)
        {
            var members = Members();
            var minors = Minors().Count;
            if (members.Count == 0 && minors == 0) return;
            var upkeep = MonthlyUpkeep() * Math.Max(1, months);
            var (fromMoney, fromStorage, unpaid) = Split(upkeep, Player.GetUnitMoney(), StorageStones());
            if (fromMoney > 0) Player.AddUnitMoney(-(int)fromMoney);
            if (fromStorage > 0) GameHelper.GetStorage().data.propData.DelProps(ModLibConst.MONEY_PROP_ID, (int)fromStorage);
            Add(Kind.Upkeep, null, null, fromMoney + fromStorage);
            if (unpaid > 0)
            {
                Add(Kind.Unpaid, null, null, unpaid);
                foreach (var m in members)
                {
                    Cool(m.data.unitData.relationData, Player.GetUnitId());
                    Cool(Player.data.unitData.relationData, m.GetUnitId());
                }
                TaoistLogEvent.Log("log_family_unpaid", unpaid);
            }
        }

        /// <summary>Lowers an intimacy by UNPAID_INTIM (AddIntim with no clamp does not change it).</summary>
        private static void Cool(DataUnit.RelationData r, string id) => FamilyEvent.ChangeIntim(r, id, UNPAID_INTIM);

        public void Gifts()
        {
            foreach (var m in Members())
            {
                if (!(CommonTool.Random(0f, 100f) < GIFT_RATE)) continue;
                Gift(m, CommonTool.Random(0f, 100f) < GIFT_ITEM_SHARE);
            }
        }

        /// <summary>A member brings spirit stones or a precious item of their grade to the bank storage; returns the log key.</summary>
        public string Gift(WorldUnitBase m, bool item)
        {
            var storage = GameHelper.GetStorage();
            if (storage == null) return null;
            if (item)
            {
                var conf = EncounterEvent.PickItem(x => x.IsPill() != null || x.IsHerbItem() != null || x.IsMineItem() != null, m.GetGradeLvl());
                if (conf != null)
                {
                    storage.data.propData.AddProps(conf.id, 1);
                    GiftedItems[conf.id] = (GiftedItems.TryGetValue(conf.id, out var n) ? n : 0) + 1;
                    Add(Kind.GiftItem, m, conf.id, 1);
                    TaoistLogEvent.Log("log_family_gift_item", m.GetName(), GameTool.LS(conf.name));
                    return "log_family_gift_item";
                }
            }
            var stones = InflationaryEvent.CalculateInflationary(BaseGift(m.GetGradeLvl(), CommonTool.Random(0, 100)));
            storage.data.propData.AddProps(ModLibConst.MONEY_PROP_ID, stones);
            Add(Kind.GiftStones, m, 0, stones);
            TaoistLogEvent.Log("log_family_gift_stones", m.GetName(), stones);
            return "log_family_gift_stones";
        }

        /// <summary>Now and then a member in need takes back one gifted item still in the storage.</summary>
        public void UseGifts(bool force = false)
        {
            if (!force && !(CommonTool.Random(0f, 100f) < USE_RATE)) return;
            var members = Members();
            var storage = GameHelper.GetStorage();
            if (members.Count == 0 || storage == null) return;
            var stored = storage.data.propData.allProps.ToArray().GroupBy(x => x.propsID).ToDictionary(x => x.Key, x => x.Sum(y => y.propsCount));
            var usable = GiftedItems.Where(x => x.Value > 0 && stored.TryGetValue(x.Key, out var c) && c > 0).Select(x => x.Key).ToList();
            if (usable.Count == 0) return;
            var id = usable[CommonTool.Random(0, usable.Count * 1000) % usable.Count];
            var m = members[CommonTool.Random(0, members.Count * 1000) % members.Count];
            storage.data.propData.DelProps(id, 1);
            if (--GiftedItems[id] <= 0) GiftedItems.Remove(id);
            Add(Kind.Used, m, id, 1);
            TaoistLogEvent.Log("log_family_used", m.GetName(), GameTool.LS(g.conf.itemProps.GetItem(id)?.name ?? "?"));
        }

        private void Add(Kind kind, WorldUnitBase u, int? itemId, long amount) =>
            Ledger.Add(new Entry { Month = GameHelper.GetGameTotalMonth(), Kind = kind, UnitId = u?.GetUnitId(), Name = u?.GetName(), Amount = amount, ItemId = itemId ?? 0 });

        // ------------------------------------------------------------------ accounts window

        /// <summary>MapMain: the "Family" button (accounts of the household).</summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (FamilyEvent.Disabled || e.uiType.uiName != UIType.MapMain.uiName)
                return;
            var uiMain = new UICover<UIMapMain>(e.ui);
            _button = uiMain.AddButton(uiMain.FirstCol + 18, uiMain.LastRow - 4, OpenAccounts, GameTool.LS("fund_button")).Size(140, 40);
            UpdateButton();
        }

        private UIItemButton _button;

        /// <summary>The "Family" button is shown only when the player is married (a companion, alive or not).</summary>
        public static bool ShowButton => !FamilyEvent.Disabled && !string.IsNullOrEmpty(FamilyEvent.Spouse(Player));

        private void UpdateButton()
        {
            try
            {
                var go = _button?.Item?.gameObject;
                if (go != null && go.activeSelf != ShowButton) go.SetActive(ShowButton);
            }
            catch { _button = null; }
        }

        /// <summary>Marriage / inheritance change the button.</summary>
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnTimeUpdate1000ms()
        {
            base.OnTimeUpdate1000ms();
            UpdateButton();
        }

        public string Line(Entry x)
        {
            var item = x.ItemId == 0 ? string.Empty : GameTool.LS(g.conf.itemProps.GetItem(x.ItemId)?.name ?? "?");
            var when = string.Format(GameTool.LS("fund_when"), x.Month / 12 + 1, x.Month % 12 + 1);
            return when + string.Format(GameTool.LS("fund_entry_" + x.Kind), x.Name, x.Amount, item);
        }

        /// <summary>Accounts of the household (NPC search window with the members, like the town management).</summary>
        public static void OpenAccounts()
        {
            var I = Instance;
            var ui = g.ui.OpenUISafe<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            var members = Members();
            ui.units = members.ToIl2CppList();
            ui.UpdateUI();

            var year = GameHelper.GetGameTotalMonth() / 12;
            var thisYear = I.Ledger.Where(x => x.Month / 12 == year).ToList();
            long Sum(Kind k) => thisYear.Where(x => x.Kind == k).Sum(x => x.Amount);
            var cover = new UICover<UINPCSearch>(ui);
            var col = cover.MidCol - 11;
            var row = cover.MidRow - 1;
            var minors = Minors();
            var lines = new List<string>
            {
                string.Format(GameTool.LS("fund_title"), Player.GetName(), FamilyEvent.Instance?.Generation ?? 1),
                string.Format(GameTool.LS("fund_members"), members.Count, minors.Count),
                string.Format(GameTool.LS("fund_upkeep"), MonthlyUpkeep(), members.Count == 0 ? 0 : members.Sum(Upkeep), minors.Count * MinorUpkeep()),
                string.Format(GameTool.LS("fund_year"), Sum(Kind.Upkeep), Sum(Kind.Unpaid), Sum(Kind.GiftStones), thisYear.Count(x => x.Kind == Kind.GiftItem), thisYear.Count(x => x.Kind == Kind.Used)),
                string.Format(GameTool.LS("fund_stored"), I.GiftedItems.Values.Sum()),
                GameTool.LS("fund_last"),
            };
            lines.AddRange(I.Ledger.Where(x => x.Kind != Kind.Upkeep).Reverse<Entry>().Take(8).Select(I.Line));
            if (I.Ledger.All(x => x.Kind == Kind.Upkeep)) lines.Add(GameTool.LS("log_empty"));
            foreach (var l in lines)
                cover.AddText(col, row++, l).Format(null, 15).Align();
            cover.AddToolTipButton(col - 2, cover.MidRow + 1, GameTool.LS("fund_help"));
            cover.AddButton(cover.LastCol - 7, cover.LastRow - 8, () => FamilyEvent.OpenFamily(Player), GameTool.LS("fund_tree")).Format(Color.black, 17).Align(TextAnchor.MiddleCenter).Size(300, 64);
            cover.UpdateUI();
        }
    }
}
