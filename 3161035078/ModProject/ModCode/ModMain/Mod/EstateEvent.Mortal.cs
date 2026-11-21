using MOD_nE7UL2.Helper;
using ModLib.Helper;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// A mortal house bought back by the player (only the houses of the mortals: not a shop of the market, not the house
    /// of a cultivator): the land and the whole house (a house bought to make room for a bigger one pays half of it, it
    /// is pulled down); the house keeps its level, the mortals move out. The windows of the plots tell which plot it is.
    /// </summary>
    public partial class EstateEvent
    {
        /// <summary>Price of a mortal house bought back: its land and its whole house.</summary>
        public static long MortalPrice(MapBuildTown town, Plot p) => Payable(LandPrice(town, p) + BuildCost(town, Math.Max(1, p.Level)));

        /// <summary>A house of the mortals that can be bought back (not a shop of the market).</summary>
        public static bool IsMortalHome(Plot p) => p != null && p.OwnerId == null && p.MainIndex == null && p.Level > 0 && DistrictOf(p.District).Code != 'M';

        /// <summary>Buys back a mortal house: the unit's house in this town, of the level of the mortal house.</summary>
        public static BuildResult BuyMortal(MapBuildTown town, Plot p, WorldUnitBase wunit)
        {
            if (!IsMortalHome(p))
                return BuildResult.NotFree;
            if (HasHouse(town, wunit))
                return BuildResult.HasHouse;
            var level = Math.Min(MAX_LEVEL, p.Level);
            if (wunit.GetGradeLvl() < RequiredGrade(level))
                return BuildResult.GradeTooLow;
            var cost = PayHouse(wunit, MortalPrice(town, p));
            if (cost <= 0)
                return BuildResult.NoMoney;
            MapBuildPropertyEvent.AddBuildProperty(town, cost);
            p.OwnerId = wunit.GetUnitId();
            p.Level = level;
            p.Unpaid = 0;
            p.MainIndex = null;
            p.Rooms = new Dictionary<int, int>();
            p.Herbs = new List<int>();
            Touch();
            if (wunit.IsPlayer())
                TaoistLogEvent.Log("log_estate_buy_mortal", town.name, HouseName(level), cost);
            return BuildResult.Ok;
        }

        /// <summary>Which plot: its number and place on the town map.</summary>
        public static string PlotWhere(Plot p) => string.Format(GameTool.LS("estate_plot_where"), p.Index + 1, p.X + 1, p.Y + 1);

        /// <summary>The window of a plot of the mortals: what it is, and the house bought back (not a shop).</summary>
        public static void MortalWindow(MapBuildTown town, Plot p, string title, string text)
        {
            var where = PlotWhere(p) + "\n" + text;
            if (!IsMortalHome(p))
            {
                InfoDialog.Show(title, where);
                return;
            }
            var price = MortalPrice(town, p);
            var level = Math.Min(MAX_LEVEL, p.Level);
            var info = where + "\n" + string.Format(GameTool.LS("estate_mortal_buy_info"), HouseName(level), SpiritNoteEvent.NotesFor(price), price,
                SpiritNoteEvent.Notes(Player), HouseTax(town, p, level), RequiredGrade(level));
            if (HasHouse(town, Player))
            {
                InfoDialog.Show(title, info + "\n" + GameTool.LS("estate_one_house"));
                return;
            }
            InfoDialog.Show(title, info,
                (string.Format(GameTool.LS("estate_mortal_buy"), SpiritNoteEvent.NotesFor(price)), () =>
                {
                    var rs = BuyMortal(town, p, Player);
                    if (rs == BuildResult.Ok)
                    {
                        OpenHouse(town, p);
                        InfoDialog.Msg(title, string.Format(GameTool.LS("estate_done"), HouseName(p.Level)));
                    }
                    else if (rs == BuildResult.NoMoney) InfoDialog.Msg(title, NoNotesText(price));
                    else if (rs == BuildResult.GradeTooLow) InfoDialog.Msg(title, string.Format(GameTool.LS("estate_grade"), HouseName(level), RequiredGrade(level)));
                    else InfoDialog.Msg(title, GameTool.LS("estate_not_free"));
                }),
                (GameTool.LS("dlg_close"), null));
        }
    }
}
