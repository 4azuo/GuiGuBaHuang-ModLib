# RealMarketEvent3 (player market)

`3161035078/ModProject/ModCode/ModMain/Mod/RealMarketEvent3.cs` + `RealMarketEvent3.Extra.cs` (partial class) - cache key `ModConst.REAL_MARKET_EVENT3` - group: Economy

A market between cultivators in the towns with a market: NPCs and the player list items; buyers (NPCs and the player) make offers in spirit stones and / or items (barter); every month the best offer of a listing may be accepted. Plus: price history, notifications of new offers, buy orders, an anonymous (VIP) part. Deals can attract stalkers. Open work: `docs/Taoist/05-Backlog.md`.

Related objects: `Object/MarketItem.cs` (listing), `NegotiatingDeal.cs` (an offer), `NegotiatingItem.cs` (stones or an item in an offer).

## Saved data

| Property | Meaning |
|---|---|
| `MarketStack` | `List<MarketItem>`: SellerId, TownId, PropId, SoleId, Count, SellerPrice, CreateMonth, IsPartialItem (stackable), IsHidden (anonymous) |
| `NegotiatingDeals` | `List<NegotiatingDeal>`: BuyerId, target listing, items (stones / items), TotalValue |
| `BoughtItems` | soleID -> month bought (an NPC does not resell it for 12 months) |
| `PriceHistory` | item id -> last 10 unit prices of deals |
| `BuyOrders` | `List<BuyOrder>`: TownId, PropId, Count, Price (per item), CreateMonth |

## Switches

S&M: `NoMarketItem` (all off), `MarketItemNpcJoinRate`, `OnlyActiveOnCurOrNearArea` (only towns within 2 areas of the player), `MaxSellersEachTown`, `MaxItemsOnSeller`, `MaxBuyersOnSellingItem`, `MarketItemGetAttackedRate`, `NoStalker`, `NoBuyOrder`. Taoist Settings: `NotifyMarketOffers`.

`market_item_configs.json` (`MarketItemConfig`):

| Key | Value | Use |
|---|---|---|
| `JoinRange` | 3 | units within 3 of a town take part |
| `SellerJoinMarketRate` / `BuyerJoinMarketRate` | 8 / 12 % a month | NPC seller / buyer joins |
| `SellRateDependOnGrade` / `BuyRateDependOnGrade` | 3 / 4 | item chance `(7 - level) x 3` % to be listed; `level x 4` % to be bid on |
| `GetDealRate` | 24 | accept chance `24 x bestOffer / sellerPrice` % a month |
| `CancelDealOnMonth` | 3 | cancel chance `3 x months listed` % a month |
| `CancelDealLastMinuteRate` | 8 % | seller backs out at the deal |
| `HiddenRate` | 20 % | NPC listing anonymous |
| `GetStalkedRate` | 10 % | a rival buyer stalks the player |

`MIN_PRICE` 10, `MAX_PRICE` 100,000,000.

## Monthly (`OnMonthly`)

1. Each listing: invalid, or the cancel roll -> cancelled (buyers informed). Otherwise, with an offer: accept roll -> `AcceptDeal` with the best offer.
2. `BoughtItems` older than 12 months forgotten.
3. **NPC sellers**: per market town (below `MaxSellersEachTown`), units in range roll `SellerJoinMarketRate`; each lists up to `MaxItemsOnSeller` unequipped sellable items (not money, contribution, mayor degree; not bought in the last 12 months), each with chance `(7 - level) x 3` %: random count, price `sale x Random(0.6, 3.0)`, 20 % anonymous.
4. Snapshot of the offers on the player's listings.
5. **NPC buyers**: per listing (below `MaxBuyersOnSellingItem` buyers), each unit of the town rolls `BuyerJoinMarketRate` and `level x 4` %: first offer `sale x Random(0.5 + 0.1 x level, 1.0 + 0.1 x level)` (max half its money); otherwise outbids the best by `x Random(1.01, 1.20)` if under half its money; else, for level 5-6 items of grade >= its own, adds lower-level items of its bag (barter) up to the best offer.
6. `NotifyNewOffers`: new or raised offers on the player's listings -> log `market_offer_line`, one message `market_offer_title` (setting on).
7. `ProcessBuyOrders`.

## A deal (`AcceptDeal`)

Checks in order: seller / listing valid (else cancelled with messages); 8 % the seller backs out (`NG_Seller1`, buyer hates seller `grade x 10`); item gone (`NG_Seller2`); buyer cannot pay or offered items gone (`NG_Buyer1`, seller hates buyer `grade x 10`); buyer offers the same item twice / more than owned (`NG_Buyer2`, `grade x 15`). OK: +`grade x 5` intimacy both ways, stones to the seller, the item to the buyer (refine slots copied for equipment, `CustomRefineEvent.CopyAdj`), barter items to the seller, `BoughtItems`, `RecordPrice`, log `log_market_sold` / `log_market_bought`. Dramas (`other500020078..093`) only when the player is involved; a deal the player lost to an NPC: `other500020082`.

## UI

Town market (`OnOpenUIEnd(TownMarket)`):

| Button | Window |
|---|---|
| VIP (`other500020085`, tooltip `other500020041`) | pay `VIPCost = CalculateInflationary(100 x area)` to the town budget -> the anonymous listings and those of grade >= 4 |
| Market (`other500020040`) | the public listings, by seller (NPC search) -> a seller's listings |
| My listings (`other500020060`) | the player's listings: raise / lower price (+-5..20 %), cancel, see the offers (accept) |
| Register (`other500020061`) | list an item: one item, quantity, price slider (max `2 x sale x count`), anonymous toggle (fee 10 % of the price, 100 .. 10M, paid to the town), price hint `market_history` (avg, min, max, count) / `market_nohistory`; must be in a town, one listing per item |
| Buy orders (`market_buyorders`) | shown unless `NoBuyOrder` (or orders still running) |

On a seller's listing the player can: make / change an offer in stones (+-5..20 %), add items to the offer (barter), give up (cancel). Raising the offer or adding items may make rival buyers (valid, `IsStalker`, `GetStalkedRate x (1 + MarketItemGetAttackedRate)` %) stalk the player when the offer is more than twice their money / they could rob the item ([BattleAfterEvent](BattleAfterEvent.md), reasons `MarketBuyerShowUp_SpiritStones` / `_Items`).

Window flags (`IsOpenMarket`, `IsOpenSellingList`, `IsOpenNegotiatingList`, `IsOpenRegister`, `IsOpenMySellingList`, `IsOpenMyDeals`) are reset when `NPCSearch` / `PropSelect` close; other features read them (e.g. no bounty button in the market NPC info).

## Buy orders (`Extra.cs`)

`MAX_BUY_ORDERS` 8, `BUY_ORDER_MONTHS` 12, `BUY_ORDER_RATE` 25.

- **New** (`AddBuyOrder`): an item (`OrderableItems`: seen in deals / listings, or herb / ore / pill up to grade + 1), count, unit price >= 10; the stones are held at once; log `log_buyorder_new`.
- **Monthly** (`ProcessBuyOrders`): expired -> refunded (`log_buyorder_expired`); each NPC of the order town owning the item sells with chance `min(90, 25 x price / reference)` %, reference = average recent price or the sale value; paid from the held stones, +5 intimacy, `RecordPrice`, log / message `market_buyorder_filled`.
- **Cancel**: refund of the remaining items (`log_buyorder_cancel`).
- Window: a table per town (cancel, icon, item, quantity, price each, town, time left), 8 per page; new order in 2 steps (item, then count and price).

## Tests

TG-14, TG-46.
