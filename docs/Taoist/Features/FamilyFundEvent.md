# FamilyFundEvent

`3161035078/ModProject/ModCode/ModMain/Mod/FamilyFundEvent.cs` - cache key `ModConst.FAMILY_FUND_EVENT` - group: Economy

The household of the player ([FamilyEvent](FamilyEvent.md)): the companion, the living grown-up descendants and the children not grown up yet. They cost a monthly upkeep, bring gifts to the bank storage now and then, and sometimes take a gifted item back. A map button "Family" opens the accounts.

## Saved data

| Property | Meaning |
|---|---|
| `Ledger` | `List<Entry>` (max 200): Month, Kind (Upkeep, Unpaid, GiftStones, GiftItem, Used), UnitId, Name, Amount, ItemId |
| `GiftedItems` | item id -> count of gifted items still owed to the family (the only ones it may take back) |

## Switch

`NoFamily` (S&M).

## Constants

| Name | Value |
|---|---|
| `UPKEEP_PER_GRADE2` | 30 (member upkeep = grade^2 x 30) |
| `MINOR_UPKEEP` | 20 (a child not grown up) |
| `GIFT_RATE` | 1.5 % a month per grown-up member |
| `GIFT_ITEM_SHARE` | 35 % of the gifts are items |
| `USE_RATE` | 2 % a month |
| `UNPAID_INTIM` | -5 |

All amounts go through `CalculateInflationary`.

## Members

`Members()`: the living companion + `FamilyEvent.Heirs()` (living grown-up children and grandchildren). `Minors()`: unborn / not grown child records of the player or of a member.

## Monthly (`OnMonthly`, x months passed)

1. **Upkeep** (`PayUpkeep(months)`): `MonthlyUpkeep x months` taken from the player's money, then from the spirit stones in the bank storage (`Split`). Unpaid rest: ledger `Unpaid`, every member and the player -5 intimacy both ways, log `log_family_unpaid`.
2. **Gifts** (each month): every member, 1.5 %: 35 % an item (pill / herb / mineral of the member's grade, added to the storage and to `GiftedItems`, `log_family_gift_item`), else spirit stones `upkeep x (2 + roll / 34)` = 2 to 4 months of its upkeep to the storage (`log_family_gift_stones`).
3. **Use** (each month): 2 % a random member takes one gifted item still in the storage (`log_family_used`).

## UI

- `OnOpenUIEnd(MapMain)`: button "Family" (`fund_button`) bottom left, shown only when the player has a companion (alive or not); refreshed every second (marriage / inheritance).
- `OpenAccounts()`: the game's NPC search window with the members' portraits, and the texts: title (player, generation), members and minors, monthly upkeep (members, minors), totals of the current year (paid, unpaid, gifted stones, gifted items, items used), gifted items in storage, the last 8 entries other than upkeep (`fund_entry_*`, dated `fund_when`); a help tooltip (`fund_help`) and a big "Family tree" button (`fund_tree`) -> `FamilyEvent.OpenFamily(player)`.

## Tests

TX-10.
