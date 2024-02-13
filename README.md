# GuiGuBaHuang-ModLib
Simple ModLib for game 鬼谷八荒 (guigubahuang)

# How to create a mod
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/91921f05-251a-4f1e-a2e9-d1e5bdd853d3)

# Configuration
**Add ModLib to your project**
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/49b156ba-74db-45e0-a210-42e26c5f7a9a)
**Add ModLib-BuildEvent**
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/3b2f1d21-177d-4605-8e97-b969dc4ae61b)
`xcopy "$(ProjectDir)\bin\Release\*.dll" "$(ProjectDir)\..\ModMain\bin\Release\" /y`
Add BuildEvent for copying dll to your project.
**Add ModMain-BuildEvent**
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/4b21eb8f-44ab-45cf-bebf-4894f0862553)
`rd "$(ProjectDir)obj" /s /q
rd "$(ProjectDir)\..\..\..\debug\Mod_JhUKQ7\ModCode\dll" /s /q
rd "$(ProjectDir)\..\..\..\debug\Mod_JhUKQ7\ModConf" /s /q
rd "$(ProjectDir)\..\..\..\debug\Mod_JhUKQ7\ModExcel" /s /q
xcopy "$(ProjectDir)\bin\Release\*.dll" "$(ProjectDir)\..\..\..\debug\Mod_JhUKQ7\ModCode\dll\" /y /i
xcopy "$(ProjectDir)\..\..\ModConf\*.json" "$(ProjectDir)\..\..\..\debug\Mod_JhUKQ7\ModConf\" /y /i
xcopy "$(ProjectDir)\..\..\ModExcel\*.xlsx" "$(ProjectDir)\..\..\..\debug\Mod_JhUKQ7\ModExcel\" /y /i`
Add BuildEvent for copying dll to "debug" folder.

# How to use
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/33b579c2-5d91-4e97-86ea-ce964edf4379)
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/84f35501-d6f2-4b8d-9cd6-2606bf397e59)
Just inherit **ModMaster** then you can use declared on-events.
But you should use **ModEvent**↓↓↓ to process your mod. And use **ModMain**↑↑↑ to config your mod.
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/e8da9fd1-89d0-4870-ace4-b7153dace9f2)
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/84af2edc-fa41-4e27-a4fa-01f3caaf1865)
About **ModEvent**, you should declare `Cache(string cacheId, bool isGlobal = false)`
If **IsGlobal**=true then, the event will be created when start game (application).
If **IsGlobal**=false then, the event will be created when load save.
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/3c404677-54fe-4e5f-af2e-8ec0618480f0)
Can declare same ModEvent by adding **multi** CacheAttribute to the ModEvent.
※I wrote some events which i need. You can add more to ModMaster.

# Debug/Log
You can use DebugHelper to write a log which will be saved to ↓ folder.
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/7cf4688f-8890-41e7-bf0a-aa6519bbf325)
`xxx\AppData\LocalLow\guigugame\guigubahuang\mod\{MOD_NAME}`
**InGame** filelog: `{g.world.playerUnit.GetUnitId()}_debug-{DateTime.Now:yyyyMMdd}.log`
**Global** filelog: `debug-{DateTime.Now:yyyyMMdd}.log`
Also, you can use **TraceAttribute**/**TraceIgnoreAttribute** which catch calling method.

# Cache/Save
`xxx\AppData\LocalLow\guigugame\guigubahuang\mod\{MOD_NAME}`
Same folder with logs.

# ModConf
I hate to use **.cache**. It could not be read/write directly.
So, i wrote a snipet which i can use **.json** to edit game's conf.
Create a folder **ModConf** in **ModProject**, copy samples from below folder to **ModConf**
`Steam\steamapps\common\鬼谷八荒\Mod\modFQA\配置修改教程\配置（只读）Json格式`
## ModConf - Notes
1. About filename, split by **_**. The last part is the name of conf.
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/a1ff96de-6850-466d-a3fd-682faf38a7f3)
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/81548d72-7c44-4fe7-abed-094ff507a38e)
2. You dont need rewrite all properties of conf. Just edit which you want.
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/7f9de987-89a8-441d-addc-a894d37ba746)
※**"__name"** is my comment.
3. Use **|** to set same value to the properties.
![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/cb95e58a-daf2-45f4-88d1-6ee1cb6cca1b)

※Sorry for my english.
