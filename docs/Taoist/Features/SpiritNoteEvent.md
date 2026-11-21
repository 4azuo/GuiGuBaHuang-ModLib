# SpiritNoteEvent

`3161035078/ModProject/ModCode/ModMain/Mod/SpiritNoteEvent.cs` - cache key `ModConst.SPIRIT_NOTE_EVENT` - group: Economy

Spirit banknotes (灵票, "Linh phiếu"): the spirit stones of a unit are an int (at most 2,147,483,647), too few for the
biggest prices of the town estates ([EstateEvent](EstateEvent.md)) once the inflation grows. A banknote is an item
worth 10,000 stones, kept in the bag or the storage, changed at the bank for a fee.

## Item

`ModConf/note_itemProps.json`: id `484410200` (`item_name484410200`, `item_desc484410200`), type 6 / class 610, icon
`jiashu`, worth 10,000 (counted by the storage fee of [RealStorageEvent](RealStorageEvent.md)), not sold in shops nor
dropped (`sale` -1, `drop` 0). Texts: `note_localText.json` (ids `-1000012000`...).

## Settings (`estate_configs.json`, `Notes`)

| Key | Default | Meaning |
|---|---|---|
| `Value` | 10,000 | spirit stones a banknote is worth |
| `ExchangeFee` | 0.01 | share kept by the bank on every exchange, both ways (+ the S&M bank fee `AddBankFee`); to the town budget |

## Payments

- Houses, land, upgrades and rooms are priced in whole banknotes (`NotesFor`: the price in stones / 10,000, rounded up)
  and paid with the banknotes of the bag only (`PayNotes`): spirit stones never pay them, so no price passes the int
  limit. The windows show the banknotes needed; short: "Change spirit stones at the bank of a town" (`estate_no_notes`).
- The monthly house tax (a few stones): `Pay(unit, cost)`, the banknotes of the bag first, the rest in stones; when the
  stones are short, one more note and the change back in stones (`Split`, pure).
- Only the player pays in banknotes. The town budget is a long (no int limit): the buildings, the palace, the wall and
  "Upgrade to City" are priced and paid in spirit stones from it, NPC masters included; NPCs keep spirit stones.
- The game's text icons have no banknote: a button priced in banknotes ends with the number and `WithIcon` puts the
  banknote picture right after it.

## Currency reform

[InflationaryEvent](InflationaryEvent.md) divides the spirit stones by 10,000 when the price index passes its limit. A
banknote is 10,000 stones, so it follows them (`Reform`): every banknote of a bag (player and NPCs) becomes 1 spirit
stone, the banknotes of the player's storage are lost like its stones. Without this, banknotes bought before the reform
would keep their full value (a way around the reform). The prices in banknotes follow the index like every price
(`CalculateInflationary`), and stay far under the int limit (1e12 stones = 1e8 banknotes). Test: TX-02.

## The bank

The town storage (`UITownStorage`) of a town where the player has an account ([BankAccountEvent](BankAccountEvent.md))
or is a guardian gets a "Spirit banknotes" button: `OpenBank` shows the stones, the notes, the fee, and buttons to buy
1 / 10 / 100 / as many as possible and change back 1 / 10 / 100 / all (never past the int limit of the stones).

```
buy n   = n x 10,000 + ceil(n x 10,000 x fee) stones
sell n  = n x 10,000 - ceil(n x 10,000 x fee) stones
```

## Tests

TG-66 (autotest); `FormulaTests.SpiritNote_Formulas` (unit test).
