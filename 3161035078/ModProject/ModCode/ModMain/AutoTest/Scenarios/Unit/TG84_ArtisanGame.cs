using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System;
using System.Linq;
using UnityEngine;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-84 (ArtisanEvent with the game's own actions and windows): the player stands on a map cell outside towns and
    /// gathers with the game's actions (UnitUnitActionGridHerb, UnitActionGridMine): the herbs and ores count for Herbs and
    /// Mining; the alchemy window open, the game's alchemy action (UnitActionRefineElixir: herbs in, pills out) counts for
    /// Alchemy; the craft windows of forging, talismans and feng shui opened by their UI types are seen as the windows of
    /// their crafts (an item that comes into the bag while one is open counts for its craft). Artisan data, exp and the
    /// cell restored.
    /// </summary>
    public class TG84_ArtisanGame : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-84";

        private static long Exp(ArtisanEvent.Craft c) { var d = ArtisanEvent.Get(Player, c); return d == null ? 0 : d.Exp + ArtisanEvent.NeedExp(0) * (d.Rank > 0 ? 1 : 0); }

        /// <summary>A cell near the player outside buildings (towns, sects).</summary>
        private static Vector2Int? FreeCell()
        {
            var p0 = Player.GetUnitPos();
            for (var r = 1; r <= 6; r++)
                for (var dx = -r; dx <= r; dx++)
                    for (var dy = -r; dy <= r; dy++)
                    {
                        var p = p0 + new Vector2Int(dx, dy);
                        DataGrid.GridData gd = null;
                        try { gd = g.data.grid.GetGridData(p); } catch { }
                        if (gd != null && !gd.IsBuild() && !gd.isBlock && g.world.build.GetBuild(p) == null)
                            return p;
                    }
            return null;
        }

        private static void Gather(AutoTestContext c, ArtisanEvent.Craft craft, Func<Vector2Int, UnitActionBase> action)
        {
            var p = (Vector2Int)c.Bag["cell"];
            var gd = g.data.grid.GetGridData(p);
            if (craft == ArtisanEvent.Craft.Herbal) gd.SetMedicinalValue(Math.Max(100, gd.medicinalMax));
            else gd.SetMineralValue(Math.Max(100, gd.mineralMax));
            Player.SetProperty<int>(UnitPropertyEnum.Energy, Player.GetDynProperty(UnitDynPropertyEnum.EnergyMax).value);
            ArtisanEvent.RefreshBag();
            c.Bag["exp"] = Exp(craft);
            c.Bag["bag"] = ArtisanEvent.Bag(Player);
            // the game's gathering may find nothing: up to 3 tries
            for (var i = 0; i < 3; i++)
            {
                Player.SetProperty<int>(UnitPropertyEnum.Energy, Player.GetDynProperty(UnitDynPropertyEnum.EnergyMax).value);
                var rs = Player.CreateAction(action(p), false);
                var got = ArtisanEvent.Gained((System.Collections.Generic.Dictionary<int, int>)c.Bag["bag"], ArtisanEvent.Bag(Player))
                    .Any(x => ArtisanEvent.GatherCraft(g.conf.itemProps.GetItem(x.Key)) == craft);
                c.Info($"{craft}: game action {i + 1}, result {rs}, found {got}");
                if (got) break;
                if (craft == ArtisanEvent.Craft.Herbal) gd.SetMedicinalValue(Math.Max(100, gd.medicinalMax));
                else gd.SetMineralValue(Math.Max(100, gd.mineralMax));
            }
        }

        private static void Gathered(AutoTestContext c, ArtisanEvent.Craft craft)
        {
            DramaHelper.DismissDramas();
            var gained = ArtisanEvent.Gained((System.Collections.Generic.Dictionary<int, int>)c.Bag["bag"], ArtisanEvent.Bag(Player));
            c.Info($"{craft}: bag +{string.Join(", ", gained.Select(x => $"{GameTool.LS(g.conf.itemProps.GetItem(x.Key)?.name ?? "?")} x{x.Value}"))}; {ArtisanEvent.LastCrafted}");
            if (!c.Assert(gained.Any(x => ArtisanEvent.GatherCraft(g.conf.itemProps.GetItem(x.Key)) == craft), $"the game's action put a {craft} item in the bag")) return;
            c.Assert(Exp(craft) > (long)c.Bag["exp"], $"{craft} exp {(long)c.Bag["exp"]} -> {Exp(craft)}");
            CloseAll(UIType.GetReward);
        }

        /// <summary>Opens a craft window of the game by its UI type (the game's OpenUIEnd comes a frame later).</summary>
        private static void OpenWindow(AutoTestContext c, string uiName)
        {
            try { g.ui.OpenUI(UIType.GetUIType(uiName)); }
            catch (Exception ex) { c.Info($"{uiName}: {ex.GetType().Name} {ex.Message}"); }
        }

        /// <summary>The window is open and seen as a window of its craft; an item comes into the bag (as the game's craft does).</summary>
        private static void InWindow(AutoTestContext c, string uiName, ArtisanEvent.Craft craft, Func<ConfItemPropsItem, bool> item)
        {
            c.Assert(g.ui.HasUI(UIType.GetUIType(uiName)), $"the game's window {uiName} opened");
            c.AssertEqual((ArtisanEvent.Craft?)craft, ArtisanEvent.OpenCraft, $"{uiName} is a window of {craft}");
            var conf = g.conf.itemProps._allConfList.ToArray().Where(item).OrderBy(x => x.level).FirstOrDefault();
            c.Bag["exp"] = Exp(craft);
            c.Bag["item"] = conf?.name;
            if (c.Assert(conf != null, $"an item of {craft}")) Player.AddUnitProp(conf.id, 1);
        }

        /// <summary>The window closed: the item made counts for its craft.</summary>
        private static void Closed(AutoTestContext c, string uiName, ArtisanEvent.Craft craft)
        {
            CloseAll(UIType.GetUIType(uiName), UIType.GetUIType("GeomancyGameTip"));
        }

        private static void Counted(AutoTestContext c, string uiName, ArtisanEvent.Craft craft)
        {
            c.Assert(ArtisanEvent.OpenCraft == null, $"{uiName} closed");
            c.Assert(Exp(craft) > (long)c.Bag["exp"], $"{craft}: {GameTool.LS((string)c.Bag["item"] ?? "?")} counted ({ArtisanEvent.LastCrafted})");
        }

        private static Scenario Craft(Scenario sc, string uiName, ArtisanEvent.Craft craft, Func<ConfItemPropsItem, bool> item) => sc
            .Step($"{uiName}: open", c => OpenWindow(c, uiName), 1000)
            .Step($"{uiName}: an item made", c => InWindow(c, uiName, craft, item), 1000)
            .Step($"{uiName}: close", c => Closed(c, uiName, craft), 1000)
            .Step($"{uiName}: counted", c => Counted(c, uiName, craft));

        private static readonly Lazy<System.Collections.Generic.HashSet<int>> RUNES = new Lazy<System.Collections.Generic.HashSet<int>>(() =>
            new System.Collections.Generic.HashSet<int>(g.conf.runeFormula._allConfList.ToArray().SelectMany(x => new[] { x.rune1, x.rune2 }).Where(x => x > 0)));

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var sc = new Scenario { Id = "TG-84", Feature = "ArtisanEvent", Title = "Artisan Dao with the game's own gathering and craft windows", Overlay = false }
                .Step("a cell outside towns", c =>
                {
                    if (!c.Assert(ArtisanEvent.Enabled, "ArtisanEvent loaded")) return;
                    var id = Player.GetUnitId();
                    c.Bag["data"] = ArtisanEvent.Instance.Artisans.TryGetValue(id, out var old) ? Newtonsoft.Json.JsonConvert.SerializeObject(old) : null;
                    c.Bag["values"] = ArtisanEvent.CRAFTS.ToDictionary(x => x, x => Player.GetProperty<int>(ArtisanEvent.ValueOf(x)) - (old != null && old.TryGetValue((int)x, out var a) ? a.Given : 0));
                    if (old != null) { foreach (var x in old.Values) { x.Rank = 0; x.Essence = 0; } foreach (var cr in ArtisanEvent.CRAFTS) ArtisanEvent.Sync(Player, cr); }
                    ArtisanEvent.Instance.Artisans.Remove(id);
                    c.Bag["pexp"] = Player.GetExp();
                    c.Bag["pos"] = Player.GetUnitPos();
                    CloseAll(UIType.TextInfoLong, UIType.TextInfo, UIType.FateFeature);
                    var cell = FreeCell();
                    if (!c.Assert(cell != null, "a free cell near the player")) return;
                    c.Bag["cell"] = cell.Value;
                    var gd = g.data.grid.GetGridData(cell.Value);
                    c.Bag["grid"] = new[] { gd.medicinal, gd.mineral };
                    Player.SetUnitPos(cell.Value);
                    c.Assert(Player.GetMapBuild<MapBuildTown>() == null, $"the player stands outside towns at {cell.Value}");
                }, 1500)
                .Step("gather herbs (the game's action)", c => Gather(c, ArtisanEvent.Craft.Herbal, p => new UnitUnitActionGridHerb(p)), 3000)
                .Step("herbs counted", c => Gathered(c, ArtisanEvent.Craft.Herbal), 500)
                .Step("mine ores (the game's action)", c => Gather(c, ArtisanEvent.Craft.Mining, p => new UnitActionGridMine(p)), 3000)
                .Step("ores counted", c => Gathered(c, ArtisanEvent.Craft.Mining), 500)
                .Step("craft windows", c => { DramaHelper.DismissDramas(); CloseAll(UIType.GetReward); });
            sc = Craft(sc, "RefineElixir", ArtisanEvent.Craft.Alchemy, x => x.IsPill() != null);
            sc = Craft(sc, "ArtifactShapeMade", ArtisanEvent.Craft.Forge, x => x.IsArtifact() != null);
            sc = Craft(sc, "RuneSelectDraw", ArtisanEvent.Craft.Talisman, x => RUNES.Value.Contains(x.id));
            sc = Craft(sc, "RefineRuneFormula", ArtisanEvent.Craft.Talisman, x => RUNES.Value.Contains(x.id));
            sc = Craft(sc, "Geomancy", ArtisanEvent.Craft.Geomancy, x => x.IsMineItem() != null && x.IsMineItem().Length > 0);
            return sc
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfoLong, UIType.TextInfo);
                    var id = Player.GetUnitId();
                    foreach (var cr in ArtisanEvent.CRAFTS) { var d = ArtisanEvent.Get(Player, cr); if (d != null) { d.Rank = 0; d.Essence = 0; } ArtisanEvent.Sync(Player, cr); }
                    ArtisanEvent.Instance.Artisans.Remove(id);
                    if (c.Bag["data"] != null)
                    {
                        var old = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<int, ArtisanEvent.CraftData>>((string)c.Bag["data"]);
                        foreach (var x in old.Values) x.Given = 0;
                        ArtisanEvent.Instance.Artisans[id] = old;
                        foreach (var cr in ArtisanEvent.CRAFTS) ArtisanEvent.Sync(Player, cr);
                    }
                    if (c.Bag.ContainsKey("cell"))
                    {
                        var gd = g.data.grid.GetGridData((Vector2Int)c.Bag["cell"]);
                        var v = (int[])c.Bag["grid"];
                        gd.SetMedicinalValue(v[0]); gd.SetMineralValue(v[1]);
                    }
                    if (c.Bag.ContainsKey("pos")) Player.SetUnitPos((Vector2Int)c.Bag["pos"]);
                    Player.ClearExp();
                    Player.AddExp((int)c.Bag["pexp"]);
                    var values = (System.Collections.Generic.Dictionary<ArtisanEvent.Craft, int>)c.Bag["values"];
                    foreach (var cr in ArtisanEvent.CRAFTS)
                        c.AssertEqual(values[cr] + (ArtisanEvent.Get(Player, cr)?.Given ?? 0), Player.GetProperty<int>(ArtisanEvent.ValueOf(cr)), $"{cr} value restored");
                });
        }
    }
}
