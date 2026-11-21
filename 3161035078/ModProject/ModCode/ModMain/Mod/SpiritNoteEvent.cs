using MOD_nE7UL2.Helper;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Spirit notes: an item worth 10,000 spirit stones (ModConf/note_itemProps.json, value and fee in
    /// estate_configs.json "Notes"), kept in the bag or the storage. The spirit stones of a unit are an int (at most
    /// 2,147,483,647): big prices (estates) are paid with notes first (Pay), the rest with spirit stones.
    /// The bank (the town storage, with an account) changes stones into notes and back, always for a fee (to the town
    /// budget).
    /// </summary>
    [Cache(ModConst.SPIRIT_NOTE_EVENT)]
    public class SpiritNoteEvent : ModEvent
    {
        public static SpiritNoteEvent Instance { get; set; }

        public const int NOTE_ID = 484410200;

        public static long NoteValue => EstateEvent.Conf.Notes.Value;

        /// <summary>Fee of an exchange (estate_configs.json, S&amp;M bank fee applied).</summary>
        public static double FeeRate() =>
            Math.Max(0, Math.Min(0.9, SMLocalConfigsEvent.Instance?.Calculate(EstateEvent.Conf.Notes.ExchangeFee, SMLocalConfigsEvent.Instance.Configs.AddBankFee) ?? EstateEvent.Conf.Notes.ExchangeFee));

        // ------------------------------------------------------------------ formulas (pure, unit tests)

        /// <summary>
        /// How to pay a cost with notes and stones: notes first (whole notes), the rest in stones; when the stones are
        /// short, one more note and the change back (stones &lt; 0). (-1, -1) when the unit cannot pay.
        /// </summary>
        public static (long notes, long stones) Split(long cost, long money, long notes, long value)
        {
            if (cost <= 0)
                return (0, 0);
            if (money + notes * value < cost)
                return (-1, -1);
            var n = Math.Min(notes, cost / value);
            var rest = cost - n * value;
            if (rest > money)
            {
                n++;
                rest -= value;
            }
            return (n, rest);
        }

        /// <summary>Stones to buy notes (fee included).</summary>
        public static long BuyPrice(long count, long value, double fee) => count * value + (long)Math.Ceiling(count * value * fee);
        /// <summary>Stones given for notes (fee taken).</summary>
        public static long SellPrice(long count, long value, double fee) => count * value - (long)Math.Ceiling(count * value * fee);

        // ------------------------------------------------------------------ units

        public static int Notes(WorldUnitBase wunit) => wunit == null ? 0 : wunit.GetUnitPropCount(NOTE_ID);

        /// <summary>Spirit stones and notes of a unit, in spirit stones.</summary>
        public static long Wealth(WorldUnitBase wunit) => wunit == null ? 0 : wunit.GetUnitMoney() + (long)Notes(wunit) * NoteValue;

        public static bool CanPay(WorldUnitBase wunit, long cost) => Wealth(wunit) >= cost;

        /// <summary>Pays a cost with the notes in the bag first, the rest (or the change) in spirit stones.</summary>
        public static bool Pay(WorldUnitBase wunit, long cost)
        {
            if (wunit == null)
                return false;
            var money = wunit.GetUnitMoney();
            var (n, s) = Split(cost, money, Notes(wunit), NoteValue);
            if (n < 0)
                return false;
            if (n > 0)
                wunit.RemoveUnitProp(NOTE_ID, (int)n);
            if (s > 0)
                wunit.AddUnitMoney(-(int)s);
            else if (s < 0)
                wunit.AddUnitMoney((int)Math.Min(int.MaxValue - (long)money, -s));
            return true;
        }

        /// <summary>Banknotes a price takes (whole banknotes, rounded up): houses, land and rooms are priced in banknotes.</summary>
        public static long NotesFor(long cost) => cost <= 0 ? 0 : (cost + NoteValue - 1) / NoteValue;

        /// <summary>Pays banknotes of the bag only (no spirit stones).</summary>
        public static bool PayNotes(WorldUnitBase wunit, long notes)
        {
            if (wunit == null || notes < 0 || Notes(wunit) < notes)
                return false;
            if (notes > 0)
                wunit.RemoveUnitProp(NOTE_ID, (int)notes);
            return true;
        }

        /// <summary>
        /// The banknote picture right after the text of a button (the text ends with the number of banknotes; the game's
        /// text icons have no banknote). (x, y): the center of the button. The label moves left by half the icon so text
        /// and icon stay centered.
        /// </summary>
        public static UIItemButton WithIcon(UICustomBase ui, UIItemButton btn, float x, float y, float px = 24f)
        {
            var label = btn.ButtonLabel;
            if (label == null) return btn;
            var rt = btn.Item.GetComponent<RectTransform>();
            var w = Math.Min(label.preferredWidth, rt.rect.width - px - 12f);
            var lp = label.rectTransform.anchoredPosition;
            label.rectTransform.anchoredPosition = new Vector2(lp.x - px / 2f - 2f, lp.y);
            ui.AddPropIcon(x + w / 2f + 4f, y, NOTE_ID, px);
            return btn;
        }

        /// <summary>
        /// Currency reform (InflationaryEvent): a banknote is 10,000 stones, so it follows the stones: banknotes of the bags
        /// become 1 spirit stone each (stones / 10,000), banknotes of the player's storage are lost like its stones.
        /// </summary>
        public static void Reform(WorldUnitBase wunit)
        {
            if (wunit == null) return;
            var n = Notes(wunit);
            if (n > 0)
            {
                wunit.RemoveUnitProp(NOTE_ID, n);
                wunit.AddUnitMoney((int)Math.Min(int.MaxValue - (long)wunit.GetUnitMoney(), n * NoteValue / 10000));
            }
            if (wunit.IsPlayer())
                wunit.RemoveStorageItem(NOTE_ID);
        }

        /// <summary>Buys notes with spirit stones at the bank of a town; the fee goes to its budget.</summary>
        public static bool Buy(WorldUnitBase wunit, MapBuildTown town, int count)
        {
            var fee = FeeRate();
            var cost = BuyPrice(count, NoteValue, fee);
            if (count <= 0 || wunit.GetUnitMoney() < cost)
                return false;
            wunit.AddUnitMoney(-(int)cost);
            wunit.AddUnitProp(NOTE_ID, count);
            if (town != null)
                MapBuildPropertyEvent.AddBuildProperty(town, cost - count * NoteValue);
            return true;
        }

        /// <summary>Changes notes back into spirit stones at the bank of a town; the fee goes to its budget.</summary>
        public static bool Sell(WorldUnitBase wunit, MapBuildTown town, int count)
        {
            var fee = FeeRate();
            var gain = SellPrice(count, NoteValue, fee);
            if (count <= 0 || Notes(wunit) < count || wunit.GetUnitMoney() + gain > int.MaxValue)
                return false;
            wunit.RemoveUnitProp(NOTE_ID, count);
            wunit.AddUnitMoney((int)gain);
            if (town != null)
                MapBuildPropertyEvent.AddBuildProperty(town, count * NoteValue - gain);
            return true;
        }

        /// <summary>Most notes the unit can buy / sell now (the stones are an int).</summary>
        public static int MaxBuy(WorldUnitBase wunit) => (int)Math.Max(0, wunit.GetUnitMoney() / Math.Max(1, BuyPrice(1, NoteValue, FeeRate())));
        public static int MaxSell(WorldUnitBase wunit) =>
            (int)Math.Max(0, Math.Min(Notes(wunit), (int.MaxValue - (long)wunit.GetUnitMoney()) / Math.Max(1, SellPrice(1, NoteValue, FeeRate()))));

        // ------------------------------------------------------------------ the bank

        /// <summary>The bank (town storage with an account): a "Spirit notes" button.</summary>
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName != UIType.TownStorage.uiName)
                return;
            var player = g.world.playerUnit;
            var town = player.GetMapBuild<MapBuildTown>();
            if (town == null || !HasAccount(town))
                return;
            // below the game's "Storage" button and the "Bank loan" one (BankLoanEvent), same size
            var ui = new UICover<UITownStorage>(e.ui);
            var size = ui.UI.btnProps.GetComponent<RectTransform>().rect.size;
            var btn = ui.AddButton(ui.MidCol, ui.MidRow, () => OpenBank(town), GameTool.LS("note_button")).Size(size.x, size.y);
            BankLoanEvent.PlaceBelow(btn.Item, ui.UI.btnProps, BankLoanEvent.LoanButtonShown() ? 16f + size.y : 8f);
        }

        public static bool HasAccount(MapBuildTown town) =>
            BankAccountEvent.Instance?.RegisterdTown.Contains(town.buildData.id) == true || MapBuildPropertyEvent.IsTownGuardian(town, g.world.playerUnit);

        /// <summary>The exchange window: stones into notes and back.</summary>
        public static void OpenBank(MapBuildTown town)
        {
            var player = g.world.playerUnit;
            var ui = new UICustom1(GameTool.LS("note_title"));
            var row = ui.FirstRow + 1;
            var fee = FeeRate();
            // the banknote item, as in the bag
            ui.AddPropIcon(ui.Columns[ui.MidCol], ui.Rows[row] + 10f, NOTE_ID, 64f);
            row += 3;
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("note_have"), player.GetUnitMoney(), Notes(player), NoteValue)).Format(null, 16);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("note_fee"), Math.Round(fee * 100, 2), BuyPrice(1, NoteValue, fee), SellPrice(1, NoteValue, fee))).Format(null, 14);
            ui.AddText(ui.MidCol, row++, GameTool.LS("note_hint")).Format(Color.gray, 13);
            row += 2;
            var counts = new[] { 1, 10, 100 };
            for (var i = 0; i <= counts.Length; i++)
            {
                var all = i == counts.Length;
                var n = all ? 0 : counts[i];
                // four buttons a row, 220 px apart around the middle of the window
                var x = ui.Columns[ui.MidCol] + (i - 1.5f) * 220f;
                ui.AddButton(x, ui.Rows[row], () => Exchange(town, true, all ? MaxBuy(player) : n),
                    all ? GameTool.LS("note_buy_max") : string.Format(GameTool.LS("note_buy"), n)).Size(200, 36);
                ui.AddButton(x, ui.Rows[row + 3], () => Exchange(town, false, all ? MaxSell(player) : n),
                    all ? GameTool.LS("note_sell_all") : string.Format(GameTool.LS("note_sell"), n)).Size(200, 36);
            }
        }

        private static void Exchange(MapBuildTown town, bool buy, int count)
        {
            var player = g.world.playerUnit;
            var ok = buy ? Buy(player, town, count) : Sell(player, town, count);
            OpenBank(town);
            if (!ok)
                InfoDialog.Msg(GameTool.LS("note_title"), GameTool.LS(buy ? "note_buy_fail" : "note_sell_fail"));
        }
    }
}
