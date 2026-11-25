@everyone
# Fouru's ModLib v2.3.00 is out!
The shared library behind my mods. **Taoist 6.2 needs this version**: update it, keep it at the **top of the mod list**, then restart the game.

## For players
- **Title screen:** the mod buttons are in one row at the bottom center
- **Windows take turns:** custom mod windows wait for each other instead of opening over one another
- **Error reports:** a clearer window shows what is sent and what is never sent, with *Allow* / *Do not send*
- **Recent additions (2.2.38–2.2.39):** mod wallpapers for the title screen, and mod options kept outside the Workshop folder

## For modders
- `TitleModHelper`: your button in the shared title screen row
- `HitHelper`: deal hits from code, with the 12 attributes of the game
- `ModApiHelper`: call another mod's API safely, with no hard dependency
- `BattleEffectHelper`: remove the parts of an effect that stop the game's timers
- `CUnitHelper`: grade level of a battle unit, timed % bonuses
- `UIGlowHelper`: pulsing gold frame around the chosen item
- `TextIconHelper`: new `Icon`, `Paint`, `AttributeIcon`
- `UICustom1/2.WhenFree`: queue your window until the UI is free
- **Fixed:** `UnitDynPropertyEnum` lookups by value found nothing, so every basis read 0

## Links
- **Steam:** <https://steamcommunity.com/sharedfiles/filedetails/?id=3385996759>
- **Source, docs, how to create a mod:** <https://github.com/4azuo/GuiGuBaHuang-ModLib/>
- **Example mod:** <https://github.com/4azuo/GuiGuBaHuang-Taoist/>

Questions and bugs: **#modlib-discussion** and **#modlib-bug-reports**.
ModLib is free and open source (MIT). A 👍 on the Workshop helps a lot, or a coffee on [Ko-fi](<https://ko-fi.com/fouru>). **Building a mod with it? Show us!** 👇
