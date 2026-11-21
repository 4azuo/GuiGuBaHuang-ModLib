# ClanEvent

`3161035078/ModProject/ModCode/ModMain/Mod/ClanEvent.cs` - cache key `ModConst.CLAN_EVENT` - group: Economy (order 3022)

Cultivator clans (世家) of the towns. Builds on [EstateEvent](EstateEvent.md) (NPC cultivators buy houses) and
[FamilyEvent](FamilyEvent.md) (marriages, children).

## Clans

- A clan: a cultivator with a house in a town (an NPC or the player) and its living family: spouse, children,
  grandchildren (`Members`), at least `MinMembers` (2). Name: "the <head> clan".
- Relation of two clans of a town, -100..100 (`Relation`):
  `base + MemberWeight (0.5) x mean intimacy between their members (both ways, 8 x 8 at most) + MarriageBonus (25) x marriages between them`.
  The base starts at random in `StartRelation` (-40..40) and falls with their fights (`BrawlRelation` -15, a death in a
  brawl `DeathRelation` -40, a war `WarRelation` -50). States: blood feud (`WarAt` -70 or less), hostile (`HostileAt`
  -30 or less), allied (`AllyAt` 50 or more), neutral.
- The town map has a "Clans (n)" button (top left): the clans, their members, their houses, the relation of every pair
  (red hostile, green allied).

## Fights (yearly, very rare)

- Hostile clans brawl with `BrawlChance` (1 %) a year: the youngest member of each (not the player) fights; win chance =
  power / both powers (`UnitPowerCalHelper`). The loser is hurt (HP at most 1/5), dies with `BrawlDeath` (5 %); the two
  fighters' intimacy -30 both ways.
- Clans in a blood feud go to war with `WarChance` (0.2 %): all members' power; each loser dies with `WarDeath` (25 %),
  the others are hurt; each winner dies with a quarter of it; the losers' house goes to the mortals. A clan left without
  members is wiped out.
- The player never dies in a simulated fight.
- The player's side: its own clan (the player has a house in the town and a family) and its allies there. When one side
  is the player's (and not in battle, not skipping time), a drama asks: step in (a real battle against the enemy's
  fighter, the player not killed; the result decides the fight) or stay out (decided at once). `Pending` keeps the fight
  until the battle ends (`OnBattleEndHandler`).
- Taoist log (icon zhandouxiaoguo): brawls when the player's side is in it or the player is in the town, deaths, wars,
  wiped-out clans (always).

## Settings (`estate_configs.json`, `Clans`)

`Enabled`, `MinMembers`, `StartRelation`, `MemberWeight`, `MarriageBonus`, `HostileAt`, `WarAt`, `AllyAt`,
`BrawlChance`, `WarChance`, `BrawlDeath`, `WarDeath`, `BrawlRelation`, `DeathRelation`, `WarRelation`.

## Tests

TG-72 (autotest: two clans, the window, a brawl, the question, a war that wipes a clan out); TG-74 (in depth: the members'
intimacy and a marriage move the relation, a year of the town by pair, the player's clan and an ally, "step in" and the
real battle won, the player never dies in a lost war); `FormulaTests.Clan_Formulas`. Promotion pictures: TR-03 (`clans`,
`clan-war`).
