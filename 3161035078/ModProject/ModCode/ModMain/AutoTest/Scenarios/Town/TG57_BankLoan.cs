using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-57 (BankLoanEvent): a bank loan from start to blacklist. The storage of a town with an account shows "Bank
    /// loan", the loan window opens; borrowing pays the player from the town budget (a second loan is refused);
    /// interest each month; a partial repayment goes to the budget; overdue: penalty interest and a reminder letter;
    /// then a debt collector: handing over takes money and items (-reputation); beating 2 collectors writes the debt off
    /// and blacklists the player (no loan, account closed, taxes x3 there). The loan state is restored at the end.
    /// </summary>
    public class TG57_BankLoan : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-57";

        private static BankLoanEvent Loan => BankLoanEvent.Instance;
        private static bool DramaOpen => g.ui.HasUI(UIType.DramaDialogue) || g.ui.HasUI(UIType.DramaDialogueBigTexture) || g.ui.HasUI(UIType.DramaBigTexture) || g.ui.HasUI(UIType.DramaFortuitous);

        private static void Reset()
        {
            Loan.Debt = 0; Loan.Borrowed = 0; Loan.TownId = null; Loan.OverdueMonths = 0; Loan.CollectorsBeaten = 0;
            Loan.LastCollectorMonth = -99; Loan.Blacklisted = false; Loan.BlacklistTownId = null;
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildTown town = null;
            var hadAccount = false;
            var amount = 0;
            return new Scenario { Id = "TG-57", Feature = "BankLoanEvent", Title = "Bank loan: borrow, interest, overdue, collector, blacklist", Overlay = false }
                .Step("travel", c =>
                {
                    if (!c.Assert(Loan != null, "BankLoanEvent loaded")) return;
                    Reset();
                    if (!TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownStorage)))) return;
                    town = BagTown(c);
                    hadAccount = BankAccountEvent.Instance.RegisterdTown.Contains(town.buildData.id);
                    if (!hadAccount) BankAccountEvent.Instance.RegisterdTown.Add(town.buildData.id);
                    amount = BankLoanEvent.Limit() / 2;
                    if (MapBuildPropertyEvent.GetBuildProperty(town) < amount * 2) MapBuildPropertyEvent.AddBuildProperty(town, amount * 2);
                    OpenTownUI<UITownStorage>(UIType.TownStorage, ui => ui.InitData(town));
                }, 1500)
                .Step("loan button", c =>
                {
                    var btn = FindButtons(UIType.TownStorage, GameTool.LS("loan_title")).LastOrDefault();
                    if (!c.Assert(btn != null, "'Bank loan' button in the storage")) return;
                    btn.Item.onClick.Invoke();
                }, 1500)
                .Step("loan window", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.TextInfo), "loan window opened");
                    c.Assert(FindTexts(g.ui.GetUI<UIBase>(UIType.TextInfo), BankLoanEvent.Limit().ToString("#,##0")).Any(), $"limit {BankLoanEvent.Limit():#,##0} shown");
                    CloseAll(UIType.TextInfo, UIType.TownStorage);
                })
                .Step("borrow", c =>
                {
                    if (town == null) return;
                    var m = Player.GetUnitMoney(); var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.AssertEqual(null, Loan.Borrow(town, amount), $"borrow {amount:#,##0}");
                    c.AssertEqual(m + amount, Player.GetUnitMoney(), "the player got the money");
                    c.AssertEqual(budget - amount, MapBuildPropertyEvent.GetBuildProperty(town), "lent from the town budget");
                    c.Assert(Loan.Debt == amount && Loan.DueMonth == GameHelper.GetGameTotalMonth() + BankLoanEvent.TERM_MONTHS, $"debt {Loan.Debt}, due in {BankLoanEvent.TERM_MONTHS} months");
                    c.AssertEqual("loan_err_loan", Loan.Borrow(town, 10), "a second loan is refused");
                })
                .Step("interest", c =>
                {
                    if (!Loan.HasLoan) return;
                    var debt = Loan.Debt;
                    Loan.OnMonthly();
                    c.AssertEqual(debt + (long)Math.Ceiling(debt * BankLoanEvent.INTEREST), Loan.Debt, "3 % interest a month");
                })
                .Step("repay part", c =>
                {
                    if (!Loan.HasLoan) return;
                    var debt = Loan.Debt; var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    if (Player.GetUnitMoney() < 1000) Player.AddUnitMoney(1000);
                    c.AssertEqual(1000L, Loan.Repay(1000), "repay 1,000");
                    c.Assert(Loan.Debt == debt - 1000 && MapBuildPropertyEvent.GetBuildProperty(town) == budget + 1000, "debt down, budget up");
                })
                .Step("overdue letter", c =>
                {
                    if (!Loan.HasLoan) return;
                    Loan.DueMonth = GameHelper.GetGameTotalMonth() - 1;
                    var debt = Loan.Debt;
                    Loan.OnMonthly();
                    c.AssertEqual(1, Loan.OverdueMonths, "1 month overdue");
                    c.AssertEqual(debt + (long)Math.Ceiling(debt * (BankLoanEvent.INTEREST + BankLoanEvent.PENALTY)), Loan.Debt, "3 % + 5 % penalty");
                }, 1500)
                .Step("letter shown", c =>
                {
                    c.Assert(DramaOpen, "reminder letter");
                    DismissDramas();
                })
                .Step("collector", c =>
                {
                    if (!Loan.HasLoan) return;
                    Loan.OverdueMonths = BankLoanEvent.REMINDER_MONTHS;
                    Loan.OnMonthly();
                    c.AssertEqual(GameHelper.GetGameTotalMonth(), Loan.LastCollectorMonth, "a debt collector came");
                }, 1500)
                .Step("hand over", c =>
                {
                    c.Assert(DramaOpen, "collector drama (pay / hand over / fight)");
                    DismissDramas();
                    if (!Loan.HasLoan) return;
                    // little money: the collector also takes items
                    Player.AddUnitMoney(-(Player.GetUnitMoney() - (int)(Loan.Debt / 4)));
                    Player.AddUnitProp(g.conf.itemProps._allConfList.ToArray().First(x => x.IsHerbItem() != null && x.sale > 0).id, 5);
                    var debt = Loan.Debt; var rep = Player.GetProperty<int>(UnitPropertyEnum.Reputation);
                    Loan.OnCollectorChoice(1, Loan.FindCollector());
                    c.Assert(Loan.Debt < debt - debt / 4 + 1, $"money and items taken: debt {debt:#,##0} -> {Loan.Debt:#,##0}");
                    c.AssertEqual(0, Player.GetUnitMoney(), "all the money taken");
                    c.AssertEqual(rep - 100, Player.GetProperty<int>(UnitPropertyEnum.Reputation), "-100 reputation");
                }, 1500)
                .Step("beat collectors", c =>
                {
                    DismissDramas();
                    if (town == null) return;
                    if (!Loan.HasLoan) { Loan.Borrow(town, 100); }
                    var tax = MapBuildPropertyEvent.GetTax(town, Player);
                    Loan.OnCollectorBattleEnd(true);
                    c.Assert(Loan.CollectorsBeaten == 1 && !Loan.Blacklisted && Loan.HasLoan, "1st collector beaten: the debt stays");
                    Loan.OnCollectorBattleEnd(true);
                    c.Assert(Loan.Blacklisted && !Loan.HasLoan, "2nd collector beaten: debt written off, blacklisted");
                    c.Assert(!BankAccountEvent.Instance.RegisterdTown.Contains(town.buildData.id), "account of the town closed");
                    c.AssertEqual(tax * BankLoanEvent.WANTED_TAX_RATE, MapBuildPropertyEvent.GetTax(town, Player), "taxes x3 in that town");
                    c.AssertEqual("loan_err_blacklist", Loan.Borrow(town, 100), "no more loans");
                    var keys = TaoistLogEvent.Instance.Entries.Select(x => x.Key).ToList();
                    c.Assert(new[] { "log_loan_borrow", "log_loan_repay", "log_loan_overdue", "log_loan_collector", "log_loan_seized", "log_loan_blacklisted" }.All(keys.Contains), "loan steps in the Taoist log");
                }, 1500)
                .Step("restore", c =>
                {
                    DismissDramas();
                    Reset();
                    if (town != null && hadAccount && !BankAccountEvent.Instance.RegisterdTown.Contains(town.buildData.id)) BankAccountEvent.Instance.RegisterdTown.Add(town.buildData.id);
                    if (town != null && !hadAccount) BankAccountEvent.Instance.RegisterdTown.Remove(town.buildData.id);
                    c.Assert(!Loan.HasLoan && !Loan.Blacklisted, "loan state restored");
                });
        }
    }
}
