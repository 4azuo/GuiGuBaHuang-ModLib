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
    /// Bank loans (town storage with an account, button "Bank loan"). One loan at a time, lent from the town budget.
    /// - Limit 2,000 x grade² (inflation); 3 % interest a month; due after 12 months. Repaid money goes to the town budget.
    /// - Overdue: +5 % penalty interest a month; months 1-2 a reminder letter; from month 3 a debt collector comes every
    ///   2 months: pay everything / hand over what you have (money, then the most valuable items) / fight.
    /// - Collector wins: the debt is seized (money, then items), -100 reputation.
    /// - Player wins: the next collector is stronger. After 2 beaten collectors the bank writes the debt off but the player
    ///   is blacklisted: no loan anywhere, the account of that town is closed, taxes x3 in that town, -300 reputation.
    /// </summary>
    [Cache(ModConst.BANK_LOAN_EVENT)]
    public class BankLoanEvent : ModEvent
    {
        public static BankLoanEvent Instance { get; set; }

        public const float INTEREST = 0.03f;
        public const float PENALTY = 0.05f;
        public const int TERM_MONTHS = 12;
        public const int REMINDER_MONTHS = 2;
        public const int COLLECTOR_EVERY = 2;
        public const int MAX_BEATEN = 2;
        public const int WANTED_TAX_RATE = 3;

        public string TownId { get; set; }
        public long Debt { get; set; }
        public long Borrowed { get; set; }
        public int DueMonth { get; set; }
        public int OverdueMonths { get; set; }
        public int CollectorsBeaten { get; set; }
        public int LastCollectorMonth { get; set; } = -99;
        public bool Blacklisted { get; set; }
        public string BlacklistTownId { get; set; }

        /// <summary>The running battle is against a debt collector.</summary>
        [JsonIgnore]
        public static bool CollectorBattle { get; set; }

        private static WorldUnitBase Player => g.world.playerUnit;
        public bool HasLoan => Debt > 0;
        public MapBuildTown Town => string.IsNullOrEmpty(TownId) ? null : g.world.build.GetBuild(TownId)?.TryCast<MapBuildTown>();

        /// <summary>Most the player can borrow: 2,000 x grade² (inflation).</summary>
        public static int Limit()
        {
            var grade = Math.Max(1, Player.GetGradeLvl());
            return InflationaryEvent.CalculateInflationary(2000 * grade * grade);
        }

        /// <summary>True when the player is wanted by the bank of this town (taxes x3).</summary>
        public static bool IsWanted(MapBuildBase build) =>
            Instance != null && Instance.Blacklisted && build != null && build.buildData.id == Instance.BlacklistTownId;

        /// <summary>Why the player cannot borrow in the town (localText key), null when allowed.</summary>
        public string CannotBorrow(MapBuildTown town, int amount)
        {
            if (SMLocalConfigsEvent.Instance?.Configs?.NoBankLoan ?? false) return "loan_err_off";
            if (Blacklisted) return "loan_err_blacklist";
            if (HasLoan) return "loan_err_loan";
            if (town == null || (!BankAccountEvent.Instance.RegisterdTown.Contains(town.buildData.id) && !MapBuildPropertyEvent.IsTownGuardian(town, Player))) return "loan_err_account";
            if (amount <= 0 || amount > Limit()) return "loan_err_limit";
            if (MapBuildPropertyEvent.GetBuildProperty(town) < amount) return "loan_err_budget";
            return null;
        }

        /// <summary>Borrows from the town bank. Null when done, otherwise the reason (localText key).</summary>
        public string Borrow(MapBuildTown town, int amount)
        {
            var err = CannotBorrow(town, amount);
            if (err != null)
                return err;
            MapBuildPropertyEvent.AddBuildProperty(town, -amount);
            Player.AddUnitMoney(amount);
            TownId = town.buildData.id;
            Debt = Borrowed = amount;
            DueMonth = GameHelper.GetGameTotalMonth() + TERM_MONTHS;
            OverdueMonths = 0;
            CollectorsBeaten = 0;
            LastCollectorMonth = -99;
            TaoistLogEvent.Log("log_loan_borrow", amount, town.name, TERM_MONTHS);
            DebugHelper.WriteLine($"BankLoanEvent: borrow {amount} in {town.name}");
            return null;
        }

        /// <summary>Repays up to the amount (and the player's money); returns the amount paid.</summary>
        public long Repay(long amount)
        {
            if (!HasLoan)
                return 0;
            var pay = Math.Min(Math.Min(amount, Debt), Player.GetUnitMoney());
            if (pay <= 0)
                return 0;
            Player.AddUnitMoney(-(int)pay);
            var town = Town;
            if (town != null)
                MapBuildPropertyEvent.AddBuildProperty(town, pay);
            Debt -= pay;
            TaoistLogEvent.Log("log_loan_repay", pay, Debt);
            if (Debt <= 0)
                Close("log_loan_closed");
            return pay;
        }

        private void Close(string logKey)
        {
            Debt = 0;
            OverdueMonths = 0;
            TaoistLogEvent.Log(logKey, Town?.name);
        }

        // ------------------------------------------------------------------ monthly

        /// <summary>Interest, reminders and debt collectors.</summary>
        public override void OnMonthly()
        {
            base.OnMonthly();
            if (!HasLoan)
                return;
            var cur = GameHelper.GetGameTotalMonth();
            var overdue = cur > DueMonth;
            // in seclusion: normal interest only, no reminders / collectors; one dispatch may stand for several months (time skip)
            var months = ModLib.Mod.ModMaster.MonthsPassed;
            var overdueMonths = 0;
            for (var m = cur - months + 1; m <= cur; m++)
            {
                var late = m > DueMonth;
                if (late) overdueMonths++;
                Debt += (long)Math.Ceiling(Debt * (INTEREST + (late && !SeclusionEvent.IsSecluded ? PENALTY : 0f)));
            }
            if (!overdue || SeclusionEvent.IsSecluded)
                return;
            OverdueMonths += Math.Max(1, overdueMonths);
            TaoistLogEvent.Log("log_loan_overdue", OverdueMonths, Debt);
            if (GameHelper.IsInBattlle() || (TimeSkipEvent.Instance?.IsProcessing ?? false))
                return;
            if (OverdueMonths <= REMINDER_MONTHS)
                DramaHelper.OpenDrama2(string.Format(GameTool.LS("loan_letter"), Town?.name, Debt, OverdueMonths), new List<string> { GameTool.LS("loan_letter_o1") }, null, "xinjianhualuo1");
            else if (cur - LastCollectorMonth >= COLLECTOR_EVERY)
                SendCollector();
        }

        /// <summary>The debt collector: a town guardian of the bank town, or a strong NPC (stronger after each defeat).</summary>
        public WorldUnitBase FindCollector()
        {
            var minGrade = Player.GetGradeLvl() + CollectorsBeaten;
            var town = Town;
            var guards = town == null ? new List<WorldUnitBase>() : MapBuildPropertyEvent.GetTownGuardians(town).Where(x => x != null && !x.isDie && !x.IsPlayer()).ToList();
            var candidates = guards.Concat((ModMain.Store?.WUnits ?? new WorldUnitBase[0]).Where(x => x != null && !x.isDie && !x.IsPlayer() && !HirePeopleEvent.IsHired(x)));
            return candidates.Where(x => x.GetGradeLvl() >= minGrade).OrderBy(x => x.GetGradeLvl()).FirstOrDefault()
                ?? candidates.OrderByDescending(x => x.GetGradeLvl()).FirstOrDefault();
        }

        /// <summary>A collector shows up: pay / hand over / fight.</summary>
        public void SendCollector(WorldUnitBase collector = null)
        {
            collector = collector ?? FindCollector();
            if (collector == null)
                return;
            LastCollectorMonth = GameHelper.GetGameTotalMonth();
            collector.SetUnitPos(Player.GetUnitPos());
            TaoistLogEvent.Log("log_loan_collector", collector.GetName(), Debt);
            DramaHelper.OpenDrama1(string.Format(GameTool.LS("loan_collector"), Town?.name, Debt), new List<string>
            {
                string.Format(GameTool.LS("loan_collector_o1"), Debt), GameTool.LS("loan_collector_o2"), GameTool.LS("loan_collector_o3"),
            }, x => OnCollectorChoice(x.id % 10, collector), collector, Player);
        }

        /// <summary>0 pay all (if possible, otherwise as 1), 1 hand over, 2 fight.</summary>
        public void OnCollectorChoice(int opt, WorldUnitBase collector)
        {
            if (opt == 0 && Player.GetUnitMoney() >= Debt)
            {
                Repay(Debt);
                Result("loan_paid");
            }
            else if (opt == 2)
            {
                CollectorBattle = true;
                g.world.battle.IntoBattle(collector, MapBuildPropertyEvent.CATCH_SNEAKY_DUNGEON_ID, battleData: new WorldBattleData { npcUnit = collector });
            }
            else
            {
                var seized = Seize();
                Result(HasLoan ? "loan_seized_part" : "loan_seized", seized, Debt);
            }
        }

        /// <summary>Takes money, then the most valuable unequipped items, up to the debt. Returns the value taken.</summary>
        public long Seize()
        {
            long taken = 0;
            var money = Math.Min(Debt, Player.GetUnitMoney());
            if (money > 0)
            {
                Player.AddUnitMoney(-(int)money);
                taken += money;
                Debt -= money;
            }
            foreach (var prop in Player.GetUnequippedProps().Where(x => x.propsInfoBase != null && x.propsInfoBase.sale > 0 && x.propsID != ModLib.Const.ModLibConst.MONEY_PROP_ID)
                .OrderByDescending(x => (long)x.propsInfoBase.sale * x.propsCount).ToArray())
            {
                if (Debt <= 0) break;
                var value = (long)prop.propsInfoBase.sale * prop.propsCount;
                Player.RemoveUnitProp(prop.soleID);
                taken += value;
                Debt -= value;
            }
            var town = Town;
            if (town != null)
                MapBuildPropertyEvent.AddBuildProperty(town, taken);
            Player.AddProperty<int>(UnitPropertyEnum.Reputation, -100);
            TaoistLogEvent.Log("log_loan_seized", taken, Math.Max(0, Debt));
            if (Debt <= 0)
                Close("log_loan_closed");
            return taken;
        }

        /// <summary>Battle against a collector ended.</summary>
        public override void OnBattleEndHandler(BattleEndHandler e)
        {
            base.OnBattleEndHandler(e);
            if (!CollectorBattle)
                return;
            CollectorBattle = false;
            OnCollectorBattleEnd(e.isWin);
        }

        /// <summary>Lost: seized. Won: the bank tries again later, or blacklists the player after MAX_BEATEN.</summary>
        public void OnCollectorBattleEnd(bool playerWin)
        {
            if (!playerWin)
            {
                var seized = Seize();
                Result("loan_lost", seized, Math.Max(0, Debt));
                return;
            }
            CollectorsBeaten++;
            TaoistLogEvent.Log("log_loan_beaten", CollectorsBeaten);
            if (CollectorsBeaten < MAX_BEATEN)
            {
                Result("loan_beaten");
                return;
            }
            Blacklisted = true;
            BlacklistTownId = TownId;
            BankAccountEvent.Instance.RegisterdTown.Remove(TownId);
            Player.AddProperty<int>(UnitPropertyEnum.Reputation, -300);
            var written = Debt;
            Close("log_loan_blacklisted");
            Result("loan_blacklisted", written, Town?.name);
        }

        private static void Result(string key, params object[] args)
        {
            var text = string.Format(GameTool.LS(key), args);
            DebugHelper.WriteLine($"BankLoanEvent: {key} ({text})");
            ModDelayEvent.Instance.DelayEvent(Instance, () => DramaHelper.OpenDrama2(text, new List<string> { GameTool.LS("enc_leave") }, null, "dizizhenjing1"), 300);
        }

        // ------------------------------------------------------------------ UI

        /// <summary>Town storage with an account: "Bank loan" button.</summary>
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName != UIType.TownStorage.uiName)
                return;
            // off in the S&M configs: only to repay a running loan
            if ((SMLocalConfigsEvent.Instance?.Configs?.NoBankLoan ?? false) && !HasLoan)
                return;
            var town = Player.GetMapBuild<MapBuildTown>();
            if (town == null || (!BankAccountEvent.Instance.RegisterdTown.Contains(town.buildData.id) && !MapBuildPropertyEvent.IsTownGuardian(town, Player)))
                return;
            var cover = new UICover<UITownStorage>(e.ui);
            cover.AddButton(cover.LastCol - 6, cover.FirstRow + 3, () => OpenLoan(town), GameTool.LS("loan_title")).Size(200, 40);
        }

        /// <summary>Loan window: state, borrow (amount), repay.</summary>
        /// <summary>Characters per line of the loan window (UICustom2 is ~550 px wide).</summary>
        private const int WRAP_CHARS = 70;

        /// <summary>Splits a text into lines of at most max characters, at spaces (a longer word stays whole).</summary>
        public static List<string> WrapWords(string text, int max)
        {
            var lines = new List<string>();
            var cur = string.Empty;
            foreach (var word in (text ?? string.Empty).Split(' '))
            {
                if (cur.Length > 0 && cur.Length + 1 + word.Length > max) { lines.Add(cur); cur = word; }
                else cur = cur.Length == 0 ? word : cur + " " + word;
            }
            if (cur.Length > 0) lines.Add(cur);
            return lines;
        }

        public void OpenLoan(MapBuildTown town)
        {
            // a small window (TextInfo): the loan has few lines; long texts are wrapped by words
            g.ui.CloseUI(UIType.TextInfo);
            var ui = new UICustom2(GameTool.LS("loan_title"));
            var col = ui.MidCol;
            var row = ui.FirstRow;
            foreach (var line in WrapWords(string.Format(GameTool.LS("loan_terms"), INTEREST * 100, TERM_MONTHS, PENALTY * 100), WRAP_CHARS))
                ui.AddText(col, row++, line).Format(null, 13);
            foreach (var line in WrapWords(GameTool.LS("loan_warning"), WRAP_CHARS))
                ui.AddText(col, row++, line).Format(Color.red, 12);
            if (Blacklisted)
            {
                ui.AddText(col, row++, string.Format(GameTool.LS("loan_state_blacklist"), g.world.build.GetBuild(BlacklistTownId)?.name)).Format(Color.red, 16);
                return;
            }
            if (HasLoan)
            {
                var due = DueMonth - GameHelper.GetGameTotalMonth();
                ui.AddText(col, row++, string.Format(GameTool.LS("loan_state"), Town?.name, Borrowed, Debt, due)).Format(due < 0 ? Color.red : Color.black, 16);
                row++;
                var pay = ui.AddCompositeSlider(col - 3, row++, GameTool.LS("loan_repay_amount"), 1, Math.Max(1, (float)Math.Min(Debt, Player.GetUnitMoney())), Math.Max(1, (float)Math.Min(Debt, Player.GetUnitMoney())), "{0}");
                pay.Postfix.SetWork(new UIItemWork { Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { pay.MainComponent.Get().Parse<int>() }) });
                ui.AddButton(col, row++, () =>
                {
                    Repay(pay.MainComponent.Get().Parse<int>());
                    OpenLoan(town);
                }, GameTool.LS("loan_repay")).Size(200, 40);
                ui.IsAutoUpdate = true;
                return;
            }
            var limit = Limit();
            ui.AddText(col, row++, string.Format(GameTool.LS("loan_limit"), limit, MapBuildPropertyEvent.GetBuildProperty(town))).Format(null, 16);
            row++;
            var amount = ui.AddCompositeSlider(col - 3, row++, GameTool.LS("loan_amount"), Math.Min(100, limit), limit, Math.Min(100, limit), "{0}");
            amount.Postfix.SetWork(new UIItemWork { Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { amount.MainComponent.Get().Parse<int>() }) });
            ui.AddButton(col, row++, () =>
            {
                var err = Borrow(town, amount.MainComponent.Get().Parse<int>());
                if (err != null)
                {
                    g.ui.MsgBox(GameTool.LS("loan_title"), GameTool.LS(err));
                    return;
                }
                OpenLoan(town);
            }, GameTool.LS("loan_borrow")).Size(200, 40);
            ui.IsAutoUpdate = true;
        }
    }
}
