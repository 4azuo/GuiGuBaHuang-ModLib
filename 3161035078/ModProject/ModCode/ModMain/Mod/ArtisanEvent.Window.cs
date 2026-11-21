using MOD_nE7UL2.Helper;
using ModLib.Helper;
using ModLib.Object;
using System;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// The window of the Artisan Dao: a card for every craft (2 columns, 3 rows): its title (icon and name, big), its
    /// rank, value and masterpieces, its progress (craft exp, the exam, the essence of a Saint), its action (the exam, an
    /// apprentice) or why there is none yet (apprentices from Master, essence once Saint). Under the cards: the bonuses of
    /// the essence, the crafts returned to simplicity, the sell price; the commissions and the apprentices (a section
    /// each, the text and the button on the same lines); the masterpieces of the world.
    /// </summary>
    public partial class ArtisanEvent
    {
        public const float CARD_TITLE_SIZE = 20f;
        // properties, not static fields: a Unity type in the static constructor breaks the unit tests (no Unity there)
        private static Color TITLE_ON => new Color(0.1f, 0.3f, 0.5f);
        private static Color TITLE_OFF => new Color(0.25f, 0.25f, 0.25f);
        private static Color HINT => new Color(0.45f, 0.45f, 0.45f);
        private static Color SECTION => new Color(0.1f, 0.35f, 0.3f);

        public static void OpenWindow()
        {
            var ui = new UICustom1(GameTool.LS("artisan_title_window"));
            var x0 = ui.Columns[ui.FirstCol];
            var width = ui.Columns[ui.LastCol] - x0;
            var row = ui.FirstRow + 1;
            for (var i = 0; i < CRAFTS.Length; i++)
            {
                var c = CRAFTS[i];
                var cx = x0 + width * (i % 2 == 0 ? 0.27f : 0.73f);
                var r0 = row + (i / 2) * 5;
                Card(ui, c, cx, r0);
            }
            row += 15;

            // the bonuses
            var steps = TotalEssenceSteps(Player);
            if (steps > 0)
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("artisan_essence_bonus"), Math.Round(StatRate(steps) * 100, 1))).Format(new Color(0.45f, 0.2f, 0.55f), 13);
            var returned = ReturnedCrafts(Player);
            if (returned.Count > 0)
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("artisan_returned"), string.Join(", ", returned.Select(CraftName)))).Format(new Color(0.55f, 0.4f, 0f), 14);
            if (BestRank(Player) > 0)
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("artisan_sell"), SellRate(Player))).Format(new Color(0.1f, 0.4f, 0.1f), 13);

            // commissions and apprentices: the text left of the middle, the button right of it, on the same line
            var textX = ui.Columns[ui.MidCol] - 140f;
            var buttonX = ui.Columns[ui.MidCol] + 300f;
            if (Instance.Commissions.Count > 0)
            {
                ui.AddText(ui.MidCol, row++, GameTool.LS("artisan_section_commissions")).Format(SECTION, 15, FontStyle.Bold);
                foreach (var cm in Instance.Commissions.ToList())
                {
                    var com = cm;
                    var client = Unit(cm.ClientId);
                    ui.AddText(textX, ui.Rows[row], string.Format(GameTool.LS("artisan_commission_line"), client?.GetName() ?? "?", CraftName((Craft)cm.Craft), cm.Level, cm.Reward,
                        Math.Max(0, cm.Deadline - GameHelper.GetGameTotalMonth()))).Format(null, 13);
                    ui.AddButton(buttonX, ui.Rows[row], () =>
                    {
                        if (!Deliver(Player, com)) InfoDialog.Msg(GameTool.LS("artisan_title_window"), GameTool.LS("artisan_no_item"));
                        else OpenWindow();
                    }, GameTool.LS("artisan_deliver")).Format(Color.black, 12).Size(200, 28);
                    row++;
                }
            }
            if (Instance.Apprentices.Count > 0)
            {
                ui.AddText(ui.MidCol, row++, GameTool.LS("artisan_section_apprentices")).Format(SECTION, 15, FontStyle.Bold);
                foreach (var a in Instance.Apprentices.ToList())
                {
                    var ap = a;
                    var npc = Unit(a.UnitId);
                    ui.AddText(textX, ui.Rows[row], CraftIcon((Craft)a.Craft) + string.Format(GameTool.LS("artisan_apprentice_line"), npc?.GetName() ?? "?",
                        CraftName((Craft)a.Craft), RankName(Rank(npc, (Craft)a.Craft)))).Format(null, 13);
                    ui.AddButton(buttonX, ui.Rows[row], () => { Instance.Apprentices.Remove(ap); OpenWindow(); }, GameTool.LS("artisan_dismiss")).Format(Color.black, 12).Size(200, 28);
                    row++;
                }
            }
            if (Instance.Masterpieces.Count > 0)
            {
                ui.AddText(ui.MidCol, row++, GameTool.LS("artisan_masterpieces")).Format(new Color(0.45f, 0.2f, 0.55f), 14);
                foreach (var m in Instance.Masterpieces.Skip(Math.Max(0, Instance.Masterpieces.Count - 3)).Reverse())
                    ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("artisan_masterpiece_line"), GameTool.LS(g.conf.itemProps.GetItem(m.PropsId)?.name ?? "?"), m.Maker, CraftName((Craft)m.Craft)))
                        .Format(new Color(0.35f, 0.35f, 0.35f), 12);
            }
            ui.AddText(ui.MidCol, ui.LastRow - 2, string.Format(GameTool.LS("artisan_usual"), UsualLevel(Player.GetGradeLvl()))).Format(Color.gray, 12);
            ui.AddText(ui.MidCol, ui.LastRow - 1, GameTool.LS("artisan_help")).Format(null, 13);
        }

        /// <summary>The card of a craft at (cx, row): 4 lines.</summary>
        private static void Card(UICustom1 ui, Craft c, float cx, int row)
        {
            var craft = c;
            var d = Get(Player, c, true);
            Sync(Player, c);
            // the title: icon and name, big
            ui.AddText(cx, ui.Rows[row], CraftIcon(c) + CraftName(c)).Format(d.Rank > 0 ? TITLE_ON : TITLE_OFF, (int)CARD_TITLE_SIZE, FontStyle.Bold);
            // rank, value, masterpieces
            ui.AddText(cx, ui.Rows[row + 1], string.Format(GameTool.LS("artisan_card_rank"), RankName(d.Rank), CraftValue(Player, c), d.Given, d.Masterpieces)).Format(null, 14);
            // the progress: the essence of a Saint, the exam, the exp to the next rank
            ui.AddText(cx, ui.Rows[row + 2], d.Rank >= MAX_RANK
                ? string.Format(GameTool.LS("artisan_essence"), d.Essence, EssenceSteps(d.Essence), RETURN_STEPS)
                : ExamReady(d) ? string.Format(GameTool.LS("artisan_exam_ready"), RankName(d.Rank + 1), ExamChance(CraftValue(Player, c), d.Rank, d.Masterpieces > 0))
                : string.Format(GameTool.LS("artisan_exp"), d.Exp, NeedExp(d.Rank))).Format(null, 13);
            // the action, or why there is none yet
            if (ExamReady(d))
                ui.AddButton(cx, ui.Rows[row + 3], () =>
                {
                    var rs = Exam(Player, craft, CommonTool.Random(0f, 100f));
                    InfoDialog.Msg(GameTool.LS("artisan_title_window"), string.Format(GameTool.LS($"artisan_exam_{rs}"), CraftName(craft), RankName(Rank(Player, craft)), ExamFee(Rank(Player, craft))));
                    if (rs == ExamResult.Passed || rs == ExamResult.Failed) OpenWindow();
                }, string.Format(GameTool.LS("artisan_exam_button"), RankName(d.Rank + 1), ExamFee(d.Rank))).Format(Color.black, 12).Size(400, 28);
            else if (CanTakeApprentice(Player, c))
                ui.AddButton(cx, ui.Rows[row + 3], () =>
                {
                    var npc = ApprenticeCandidate(Player);
                    if (TakeApprentice(Player, craft, npc) == null)
                        InfoDialog.Msg(GameTool.LS("artisan_title_window"), GameTool.LS("artisan_no_apprentice"));
                    else OpenWindow();
                }, string.Format(GameTool.LS("artisan_take_apprentice_n"), Instance.Apprentices.Count(a => a.Craft == (int)c), MaxApprentices(d.Rank))).Format(Color.black, 12).Size(300, 28);
            else
                ui.AddText(cx, ui.Rows[row + 3], Hint(d, c)).Format(HINT, 12);
        }

        /// <summary>Why a craft has no essence or no apprentice yet.</summary>
        private static string Hint(CraftData d, Craft c)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (d.Rank < APPRENTICE_RANK) parts.Add(string.Format(GameTool.LS("artisan_hint_apprentice"), RankName(APPRENTICE_RANK)));
            else parts.Add(string.Format(GameTool.LS("artisan_hint_apprentice_full"), Instance.Apprentices.Count(a => a.Craft == (int)c), MaxApprentices(d.Rank)));
            if (d.Rank < MAX_RANK) parts.Add(string.Format(GameTool.LS("artisan_hint_essence"), RankName(MAX_RANK)));
            return string.Join(" · ", parts);
        }
    }
}
