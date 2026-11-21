# EstateEvent

`3161035078/ModProject/ModCode/ModMain/Mod/EstateEvent.cs` (data, formulas, monthly), `EstateEvent.Layout.cs` (map
generator), `EstateEvent.Map.cs` (town map window), `EstateEvent.Rooms.cs` (rooms), `EstateEvent.Config.cs` (settings) - cache key `ModConst.ESTATE_EVENT` - group: Economy

Town estates (steps B1 and B2 of the estates / mortals / clan plan, see [05-Backlog](../05-Backlog.md)): the mortals of
every town, its map of plots in districts, the town master's palace, the player's house and its rooms. Next steps: the
private storage, the mortals of an estate (B3), the clan (B4).

## Settings: `ModConf/estate_configs.json`

`EstateConfig` (`EstateEvent.Config.cs`), read in `OnLoadClass` (missing file or values: the defaults of the code, which
are the same as the file; `FormulaTests.Estate_Config` checks both match). Pictures are `"Atlas/Sprite"` of the game
(list: `docs/GameAssets/index.html`, Buildings tab), colors `"#RRGGBBAA"`.

| Section | What |
|---|---|
| `Population` | base by size and area, budget bonus, monthly move, mortal tax divisor, + per house level / palace level |
| `Prices` | land, house build, palace, room (in plain base taxes), share of the mortals' building paid to buy them out |
| `MaxUnpaidMonths` | months of unpaid house tax before the house is taken back |
| `Houses` (6) | picture, `Width` x `Height` plots, monthly `Tax` (base taxes x district price), cultivation `Grade` |
| `Palace` (5) | pictures of the town master's palace |
| `Districts` (M N R C O) | prosperity, price factor, level of the mortals' houses, ground color, pictures of the mortals' houses |
| `Map` | town / city size (cells), palace size (town 4 x 3, city 5 x 3), `RoadReach`, `LabelBlock`, `MainRoadWidth` / `AlleyWidth` (drawn width, share of a cell), `StyleWeights` (5 styles), colors |
| `Rooms` | max level, effects a level |

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `Towns` | `Dictionary<townId, TownEstate>` | `Population`, `LastMortalTax`, `PalaceLevel` (1-5), `Map` (rows of the map), `Style`, `MapVersion`, `Plots`: `Index`, `X`/`Y` (cell), `District` (M/N/R/C/O), `Prosperity` 5-100, `Level` 0 = free / 1-6, `OwnerId` (null = mortals), `MainIndex` (a plot taken by a bigger house: its main plot), `Unpaid` months, `Rooms` (kind -> level 1-3), `Herbs` (item ids waiting in the garden) |

An estate is made the first time it is needed (`Get`): its map, the plots, the population at its target, the mortals'
houses on the busiest plots. An estate of an older map version or of another size (a town grown into a city, a new size
in the config) is made again (`MakeEstate`): the player's houses move to a plot of the same district with room for their
level (a lower level when none has room), with their rooms.

## Town map (`EstateEvent.Layout.cs`)

`GenerateMap(city, seed)`: one map per town, from `StableSeed(town id)` (the same map every time), saved in the estate.
Town 24 x 16, city 30 x 19 cells (config). A char a cell: `M N R C O` plots, `P` palace, `#` main road, `B` bridge, `=`
alley, `~` water, `^` woods.

1. Style (weights of the config): crossroads, riverside (a winding river, bridges), lakeside (a lake in a corner),
   ring road (a loop around the middle, four exits), hillside (woods along an edge).
2. Main roads: random walks across the map that keep a drift and turn now and then (a town sometimes has a T crossing;
   a city a second street).
3. The palace (town 4 x 3, city 5 x 3 cells: bigger than the biggest house) on free cells near the middle, next to a main road when possible.
4. Alleys: while a plot is farther than `RoadReach` (3) from a road, an alley is cut from the farthest one to the nearest
   road (breadth search with shuffled directions: the alleys bend). Cells no road reaches become woods.
5. Districts by the distance to the palace: noble around it, market along the main roads near the middle, outskirts far
   away, craftsmen in a random third of the town and along the water, residential the rest; a missing district takes
   the best cells of the biggest one.

| District | Prosperity | Land / house tax factor | Mortal houses |
|---|---|---|---|
| Market (M) | 90 | x2.0 | shops `Build2001`, `Build2009`, `Build2013` (+ `_1`) |
| Noble quarter (N) | 78 | x1.6 | `Build2004`, `Build2004_1` |
| Residential (R) | 52 | x1.0 | tiled houses `Build2005`, `chuzhuangmaowu2` |
| Craftsmen (C) | 38 | x0.8 | workshops `Build2008`, `Build2008_1` |
| Outskirts (O) | 15 | x0.5 | huts `cunzhuangmaowu1`, `chuzhuangmaowu2` |

Prosperity of a plot = its district +/- 6, +4 next to a main road.

## Formulas (defaults of the config)

Balance (2026-11-20): cultivation takes centuries, so a house costs years of income, a town building more (it serves the
game and draws mortals), the palace decades of the town budget. Income yardstick: a town guardian's yearly salary,
3^grade x 1,000; a player of grade g usually lives in towns of area g. Price tax P = 5 x 2^max(area, grade).

```
base population   = town 10,000 + 2,000 x (area - 1) (max 30,000); city 40,000 + 6,000 x (area - 1) (max 100,000)
budget bonus      = budget / plain base tax x 5 (max the base population)
tax factor        = 1.3 - 0.3 x tax rate of the town (0.03 - 1.15): the mortal tax (rate x factor) is best at a rate
                    about 2 (x1.4 of rate 1), rate 4 gives x0.4; NPC masters set a rate of 0.5 - 3.5 every year
target            = (base + budget bonus) x tax factor x (1 + 0.2 x (palace level - 1) + 0.08 x town buildings)
                    + 300 x levels of the players' houses
population        = moves 5 % + 1 % a town building of the gap to the target a month (max 50 %)
mortal tax        = population x base tax / 1,000 a month, to the town budget
mortal plots      = plots x population / (2 x base) (15 % - 95 %); one plot built or left a month
land              = P x 400 x district factor x (1 + prosperity / 100)
build level L     = P x 1,200 x 2^(L-1) + the plots it takes (land + 50 % of the mortals' building)
house tax         = base tax x 1 / 1.5 / 2 / 3 / 4.5 / 6 (level 1-6) x district factor a month; 6 unpaid months: taken back
room level L      = P x 1,500 x 2^(L-1); one room a house level, 3 levels a room
palace level L    = plain base tax x 7,000 x 1.35^(L-2), from the town budget (the master; an NPC master when the
                    budget > 1.5 x the cost); each level above 1: +20 % mortals, +25 % yearly town budget
payable           = every price is paid with the spirit banknotes of the bag first, the rest in stones
                    ([SpiritNoteEvent](SpiritNoteEvent.md): the stones are an int; inflation up to x100); at most 10^12
town buildings    = their table of BuildingCostEnum x 10 (BuildingArrangeEvent.GetBuildingCost; not the sects')
```

| (rate 1, no inflation) | cost | years |
|---|---|---|
| hut + residential land, grade 1 | ~1,800 P = 18k | 6 years of a grade 1 income |
| courtyard house (level 3, 2 plots), grade 2 | ~9,600 P = 192k | 21 years of grade 2 |
| mansion (level 4), grade 3 | ~19,000 P = 760k | 28 years of grade 3 |
| manor (level 5, 4 plots), grade 4 | ~40,000 P = 3.2M | 39 years of grade 4 |
| immortal palace (level 6, 6 plots), grade 5 | ~82,000 P = 13M | 54 years of grade 5 |
| 5 rooms at level 3 | 52,500 P | 35 years of grade 5 |
| town factory / portal, area 1 | 100k / 200k | 24 / 48 years of the town budget (~4.2k a year) |
| palace level 2 / levels 2-5, area 1 | 70k / 464k | 17 / ~110 years of the town budget |
`plain base tax` = the base tax of the town without its tax rate (inflation applied, x2 for a city); prices and taxes go
to the town budget ([MapBuildPropertyEvent](MapBuildPropertyEvent.md)).

## Houses and palace (pictures of the game)

| Level | House | Plots | Grade | Sprite | Palace | Sprite |
|---|---|---|---|---|---|---|
| 1 | Thatched Hut | 1 x 1 | 1 | `CommonMapGrid/cunzhuangmaowu1` | Town Hall | `TownCommon/Build2016` |
| 2 | Tiled House | 1 x 1 | 1 | `TownCommon/Build2005` | Grand Town Hall | `SchoolCommon/BuildUp1001_1` |
| 3 | Courtyard House | 2 x 1 | 2 | `TownCommon/Build2004` | Lord's Palace | `SchoolCommon/BuildUp1001_2` |
| 4 | Mansion | 2 x 1 | 3 | `TownCommon/Build2002` | Jade Palace | `SchoolCommon/BuildUp1001_3` |
| 5 | Manor | 2 x 2 | 4 | `TownCommon/Build2015` | Heavenly Pavilion | `SchoolCommon/BuildUp1001_4` |
| 6 | Immortal Palace | 3 x 2 | 5 | `CommonMapGrid/wurenfangshi` | | |

A bigger house (`Expansion`): the cheapest block of its size (either way round) that holds the house, made of plots
that are free, of mortals (bought out) or already the house's, never a road, water, the palace or another owner's plot.
No such block: the upgrade is refused (`NoRoom`), the house window says so; the buy dialog of a free plot tells the
highest level it has room for (`MaxLevelAt`). The plots taken keep `OwnerId` with `Level` 0 and `MainIndex`; the house
is drawn once over all of them. Taken back / the owner dead: every plot is released (`Release`).

Loaded with the game's `SpriteTool.GetSprite(atlas, name)` (`GameSprite`; `g.res.Load<SpriteAtlas>("SpriteAtlas/<atlas>")`
as a fallback: it finds `CommonMapGrid` but not `TownCommon`). Pictures of the atlases: `python
scripts/extract-game-assets.py --buildings` -> `docs/GameAssets/Buildings/<atlas>/` (TownCommon, CommonMapGrid, VillageCommon, the `Build*` halls of SchoolCommon).

Decoration (`Map` of the config): a wall around the town with gates where the main roads leave it, ripples on the
water, rails on the bridges, trees on the woods (`Trees`), people on some main road cells (`People`, `PeopleChance`).
The price legend is made from the districts of the config (`LegendText`). District names: the biggest blocks first, at
most `MaxLabels` (3) a district, on the top, middle or bottom row of the block, never over another name or the palace.

Roads are drawn thin: a band through the middle of the cell and an arm to every road next to it (main roads half a cell
wide, alleys a quarter), the rest of the cell is ground.
## Rooms (B2)

| Room | Effect a level (1-3) | When |
|---|---|---|
| Quiet room | cultivation exp +2 % of the phase | a month spent in the town |
| Training ground | body refining exp +40 ([BodyCulEvent](BodyCulEvent.md)) | a month spent in the town |
| Herb garden | 1 herb of the owner's grade a month (up to 12 a level), collected in the house to the bag | every month |
| Spirit gathering array | Qi gathered +15 % ([QiCulEvent](QiCulEvent.md)) | while in the town |
| Ancestral hall | household upkeep -10 % ([FamilyFundEvent](FamilyFundEvent.md)), the best hall of all houses | always |

## Flow

| Hook | What happens |
|---|---|
| `OnMonthly` | every town (`MonthTown`): population, mortal tax, mortal houses, an NPC master expanding the palace, gardens, house taxes (paid -> budget + tax statistics; unpaid -> log, taken back after 6 with the rooms) |
| `OnMonthlyForEachWUnit` | the player in a town where it has a house: full HP / MP / SP (no inn), quiet room, training ground |
| [MapBuildPropertyEvent](MapBuildPropertyEvent.md) `OnOpenUIEnd(Town)` | no entry tax (`CheckIn`) in a town where the player has a house |
| [UIEvent](UIEvent.md) town window | the tax button reads "Tax: free (your house)" |

## Windows

- "Plots" button of the town window and of the town management window (which also shows "Mortals: n -> target, mortal
  tax"). `OpenPlots(town)`: population, target, mortal tax, built plots; the player's house; the map (ground, roads,
  plots tinted by district, gold for the player's house, the pictures, the palace with its level, the name of every
  district block); the price legend. A click (a `Button` added to the ground image): free plot -> buy (district,
  prosperity, land + hut, tax); the player's house -> `OpenHouse`; the palace -> expand (the player master: cost, budget,
  mortals) or information; others -> information (district, prosperity, land value).
- `OpenHouse(town, plot)`: the house picture, district / prosperity / level / tax, rooms used / slots; "Upgrade to ..."
  (cost, grade), "Collect the herbs (n)", "Town map"; the rooms (level, effect, "Raise" with its cost) and the empty
  slots ("Build a room" -> `OpenRoomChoice`: the rooms not built, their effect and cost).

Taoist log: `log_estate_buy`, `log_estate_upgrade`, `log_estate_unpaid`, `log_estate_seized`, `log_estate_palace`,
`log_estate_room`. Texts: `estate_localText.json`.

## Mortal houses bought back, plot windows (`EstateEvent.Mortal.cs`, 2026-11-20)

- Every plot window says which plot it is: `PlotWhere(p)` = "Plot no. n (column x, row y of the town map)".
- A click on a house of the mortals (`IsMortalHome`: no owner, not part of another house, level > 0, not a shop of the
  market district) opens a text window (`InfoDialog.Show`, UITextInfo) with what it is and, if the player has no house in
  the town, "Buy this house (n banknotes)": `MortalPrice` = land + the whole house of its level (a plot taken for a bigger
  house pays only half the house: it is pulled down). `BuyMortal` makes it the player's house of the same level (the
  grade that level needs), the price goes to the town budget, Taoist log `log_estate_buy_mortal`. Shops cannot be bought.
- A bigger house still takes the plots around it: they are bought with the upgrade (`Expansion`/`AcquirePrice`: free land,
  or mortal houses at land + half the house); the window says so.
- Buy confirmation of a free plot, NPC houses: text windows too (long texts, see `Helper/InfoDialog.cs`).

## Tests

TG-87 (buy back a mortal house from its window); TG-63 (map, plots, house, taxes), TG-64 (bigger houses on more plots, rooms, palace) (autotests); `FormulaTests.Estate_Formulas`, `Estate_Maps` (60 seeds: size, palace, districts, roads one network, plots near a road), `Estate_Footprints`, `Estate_Config` (unit tests).

## Map kinds, walls, a town grown into a city (2026-11-20)

- Seed: the town id + the player's unit id (a new save gets new maps), scrambled (`Mix`).
- Landscape, any mix (`RiverChance` 0.4, `LakeChance` 0.35, `WoodsChance` 0.5): a winding river (bridges; a main road
  never runs along it), a lake in the corner with the fewest roads, made after the roads and around them (an islet of
  trees when it is big enough), woods on one or two edges, outside the town. Main roads: crossroads, T crossing, ring
  road, two streets (`RoadWeights`); the palace in the middle or off it: 64 kinds of maps; no 2 x 2 square of main road
  (`ThinRoads`). Style shown as "River · Woods · T crossing".
- City wall: cities only, level 1-5 (`WallLevel`), along the border of the streets (woods outside, none on the water),
  thicker by level, gates on the main roads. The master strengthens it from the town budget (`Prices.Wall` 3,000 x
  1.5^(L-2)); an NPC master too. Defense of the city: +10 % at level 1, +5 % a level above (`WallDefense`, `WallDefensePerLevel`; +30 % at level 5), in the town wars and monster waves
  (MapBuildBattleEvent): the simulated ones, and the real ones for every defender (the player too when he defends).
- A town grown into a city keeps its map in the middle of a bigger one (`ExpandMap` / `ExpandEstate`): every plot kept
  (owner, house, rooms, prosperity, district), the main roads and the river go on to the new edge, the old woods are
  cleared, new alleys and districts in the ring.
- People on the main roads move every time the map opens.

Tests: TG-67 (a town of every landscape, a town grown into a city), TG-69 (city master: the wall, a house paid with banknotes), `FormulaTests.Estate_Maps` (no road square, every
landscape and road layout), `Estate_TownToCity`.

## The town map lives (2026-11-20)

Settings: `Growth` of `estate_configs.json`.

- **Mortal houses grow** (`GrowMortals`, monthly): with the chance `MortalChance` (5 %) a month, one mortal house below
  its cap grows a level (the most prosperous first); one above it (the population fell) shrinks back. Cap
  (`MortalCap`): the district's level, +1 when the population is at least `MortalPopulation` x the base, +1 on a plot of
  prosperity `MortalProsperity` (70) or more, at most `MortalMaxBonus` (2) above. A grown house is drawn with the house
  picture of its level; its plot says "the mortals here prospered". Every grown level adds `PerMortalLevel` (30)
  residents to the target population; a bigger mortal house costs more to buy out.
- **NPC cultivators buy houses** (`NpcHouse`, monthly for every NPC staying in a town; `Npc` false: off): without a
  house anywhere, with the chance `NpcBuyChance` (2 %) a month, it buys the most prosperous free plot it can pay with
  `NpcSpend` (half) of its spirit stones; with a house, the chance `NpcUpgradeChance` (3 %) to upgrade it one level
  (grade and room as for the player; the plots around bought). The same prices as the player's (by area), paid
  in spirit stones (`PayHouse`: the player pays in banknotes), to the town budget. NPCs own at most `NpcShare` (25 %) of
  the plots of a town. They pay the house tax every month (6 months unpaid: the house is taken back). A cultivator who
  dies leaves the house to the mortals.
- The map shows the NPCs' houses on a blue ground (`Colors.Owned`, legend: "blue: a cultivator's house"); a click tells
  whose house it is, its district and its tax.

Tests: TG-70 (autotest, screenshot of the map), TG-73 (NPC houses in depth: the monthly hook, the rules, the house tax,
unpaid months and seizure, a dead owner, mortal growth by months); `FormulaTests.Estate_MortalGrowth` (unit test).
Promotion pictures: TR-03 (`estate-town-map`, `estate-house`, `town-buildings`, `spirit-banknotes`).

## House prices by area (2026-11-20)

Houses, land, upgrades and rooms are counted in "price taxes" (`PriceTax`): `Prices.HouseTax` (100) x area x
`Prices.AreaWeight` (0.5), x2 in a city, inflation applied: area 1 = 50, area 10 = 500 (x5). The same for every buyer:
the grade floor of the player (`GradeFloor`, 5 x 2^grade) is gone, so a high-grade NPC can build up to an immortal
palace. The palace and the wall (town budget) still use the plain base tax of the town (5 x 2^area).

## Where NPCs live (2026-11-20)

- **The town** (`ChooseTowns`): an NPC without a home buys in a town of the area where it is (it need not stand in it):
  from `Growth.NpcCityGrade` (5) a city first, below a small town first; the town where it stands first among its kind,
  then at random (3 tried). `NpcHouse(..., at)` forces the town (tests).
- **The family lives together** (`FamilyHome`: the house of the spouse, else of the parents, else of the grandparents):
  the spouse and the unmarried children / grandchildren of a house owner buy nothing; a married one moves out, in the
  family's town.
- **Going home** (`NpcHome`, monthly): an NPC with a home (its own house or its family's) goes back to that town with
  `Growth.NpcHomeChance` (3 %) a month; not a hired NPC, not a guardian of another town.
- `HomeOf` reads an index of the owners (made again when a house changes hands, `Touch`, or a month passes); the monthly
  logic of every NPC rolls first and looks for the house only when the roll hits.

Test: TG-75.

