# ArtisanEvent

`3161035078/ModProject/ModCode/ModMain/Mod/ArtisanEvent.cs` - cache key `ModConst.ARTISAN_EVENT` - group: World and NPCs

Artisan Dao (nghệ đạo): the 6 crafts of the game as a path of cultivation: **alchemy** (luyện đan), **artifact forging**
(luyện khí), **talismans** (chế phù), **feng shui** (phong thủy), **herbs** (thảo dược) and **mining** (khoáng mạch). Off with the S&M switch `NoArtisan` (+500 points).

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `Artisans` | `Dictionary<unitId, Dictionary<craft, CraftData>>` | `Rank` (0-5), `Exp`, `Given` (craft value given by the rank and the essence, kept in step), `LastExamYear`, `Masterpieces`, `Essence`, `CraftedIds`, `Returned` (this craft returned to simplicity) |
| `Commissions` | `List<Commission>` | the player's open commissions: `ClientId`, `Craft`, `Level`, `Reward`, `Deadline` (total month) |
| `Apprentices` | `List<Apprentice>` | the player's apprentices: `UnitId`, `Craft` |
| `Masterpieces` | `List<Masterpiece>` | the last 50 masterpieces of the world: `Maker` (name), `Craft`, `PropsId`, `Month` |

## How a craft is counted

No patch of the game: when a craft window of the game opens (`RefineElixir`, `TownFactoryRefineElixir`,
`ArtifactShapeMade`, `RefineRuneFormula`, `RuneSelectDraw`, `Geomancy`, `GeomancyMap`) the bag is noted; when the last one closes, every item that
came into the bag counts as crafted (`Gained`; the materials used go down and are not counted).

**Gathering** (herbs, ores): the game gathers on the map by actions (`UnitActionGridMine`, `UnitUnitActionGridHerb`), with no
window but the reward window `GetReward`, and they lower the herbs (`medicinal`) / ores (`mineral`) of the cell. Every
second on the map the bag is compared with the last one; herbs (`IsHerbItem`: Herbs) and ores (`IsMineItem`: Mining) that
came in count as gathered only when the cell and the cells next to it lost herbs / ores since the last check
(`GatheredFromCell`), no window that brings items another way is open (`BLOCKING`: markets, shops, dramas, storage, the
mod's windows...; not `GetReward`), not during a time skip or a seclusion. A quest reward in `GetReward` does not lower
the cells: not counted. The mod's own gifts
(`EncounterEvent.GiveItem`, apprentices) call `RefreshBag`, so they are not counted. No furnace accident in gathering.

## Mine veins and herb valleys

`ArtisanEvent.Spots.cs`. Every area with towns gets 2 mine veins (mỏ khoáng) and 2 herb valleys (thung lũng dược liệu) at
load (`MakeSpots`, saved in `Spots`, fixed for the save): free cells of the area, 4-12 cells from one of its towns. Their
cells are made rich every year (`Enrich`: mineral / medicinal 500), so the game's own gathering finds more there. When the
player stands on one (or next to it), not offered in the last 12 months, a drama offers to stay 1, 3, 5 or 10 years: a
seclusion (`SeclusionEvent.Kind.MineVein / HerbValley`, the time passes). At the end (`SpotReward`): ores / herbs of the
area's grade, 2 a month x (1 + 0.25 x the rank of Mining / Herbs), and their craft exp. The spirit sense's scan tells the
nearest one, its distance and direction (`SpiritSenseEvent.Scan`).

## Formulas

```
craft exp            = 10 x item level² x √count
rank exp             = 100 x 4^rank (100, 400, 1,600, 6,400, 25,600)
usual item level     = 1 + (grade - 1) / 2   (1 for grades 1-2 ... 5 for grades 9-10)
masterpiece          = item level above the usual one: cultivation exp = phase exp x 3 % x levels above x (1 + 0.25 x rank)
furnace accident     = 2 levels or more above the usual one: 25 % -> HP -20 %, lifespan -3 months
exam chance          = 30 + craft value / 20 + 10 (a masterpiece made) - 5 x (rank - 1), 10-90 %
rank bonus           = craft value of the game (RefineElixir / RefineWeapon / Symbol / Geomancy) +15 a rank
essence (Saint)      = the exp of a Saint; a step every 25,600: craft value +2, attack / defense / max HP +0.1 % (all crafts)
return to simplicity = a craft with 40 essence steps (1,024,000 exp), once a craft, the crafts stack: the destiny of
                       that craft (ReturnDestiny = 420201000 + craft, ModConf artisan_return_roleCreateFeature.json /
                       _roleEffect.json): +50 to that craft only (basPil / basEqp / basSym / basGeo / basHerb / basMine),
                       attack / defense / max HP +15 %, Luck +3 (phản phác quy chân; drama, Taoist log; NPCs too)
market               = sell rate +2 % a rank of the best craft (RealMarketEvent.GetSellRate)
commission           = 2 % a month a rank of the best craft (at most 3 open): an NPC around asks for an item of a craft
                       the player made, of the usual level or one above, in 12 months; reward 4 x level x Cost();
                       delivered (an item of that craft made by the player, of that level or above): reward, +50
                       intimacy, craft exp x2; late: the client hates the player a little
apprentice           = Master 1, Grandmaster 2, Saint 3 a craft: the friendliest NPC around (not hired); every month
                       +50 craft exp, 25 % an item the player made in the craft, of the apprentice's level at most
```

| Rank | Name | Exp to the next |
|---|---|---|
| 0 | Novice | 100 (the first rank comes by itself) |
| 1 | Apprentice (Học đồ) | 400 |
| 2 | Craftsman (Thợ lành nghề) | 1,600 |
| 3 | Master (Đại sư) | 6,400 |
| 4 | Grandmaster (Tông sư) | 25,600 |
| 5 | Saint (Thánh thủ) | - |

From rank 1 the exp stops at the next rank: an **exam** in a city (`Exam`), once a year, fee `Cost(3 x rank)` to the
town budget. NPCs with a craft value of 100 or more raise their best craft every month (10 % of the value) and take its
exam in the first month of the year. Taoist log: `log_artisan_rank`, `log_artisan_masterpiece`, `log_artisan_accident`,
friends and family `log_npc_artisan_rank`, `log_npc_artisan_masterpiece`.

## UI

"Artisan Dao" button on the cultivation screen (left of the figure, under "Merit"; the best title under it) -> window
(`ArtisanEvent.Window.cs`): a card for every craft, 2 columns x 3 rows - its title (icon and name, size 20, coloured once
it has a rank), its rank / value / masterpieces (icons), its progress (craft exp, the exam, the essence of a Saint), its
action (the exam button, "Take an apprentice (n / max)") or why there is none yet in grey ("Apprentices: from Master",
"Apprentices: n / max", "Essence of the craft: once Saint": the essence grows only at the top rank, apprentices come
from Master, one more a rank above). Under the cards: the essence bonus, the crafts returned to simplicity, the sell
price; the commissions and the apprentices in sections of their own (text and button on the same lines); the last 3
masterpieces of the world. NPC info: "Craft: Master of Alchemy".

## Tests

TG-83 (autotest: ranks, masterpieces, commissions, apprentices, returns), TG-84 (the game's own gathering actions and craft
windows), TG-86 (mine veins and herb valleys), `FormulaTests.Artisan_Formulas`, `FormulaTests.Mind_Formulas`.
