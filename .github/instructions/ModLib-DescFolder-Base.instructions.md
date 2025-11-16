---
applyTo: '**/3385996759/**/ModLib/Mod/Base/*.cs'
---
Folder này chứa source code của các lớp base (cơ sở) trong thư viện ModLib. Đây là nơi chứa các abstract classes và base classes cung cấp foundation và infrastructure cho toàn bộ hệ thống mod. Các lớp trong folder này định nghĩa các interfaces, abstract methods, và common functionalities mà các mod con sẽ kế thừa và implement, bao gồm:

- **ModMaster**: Lớp abstract chính quản lý lifecycle và core functionalities của mod
- **ModEvent**: Base class cho việc xử lý các events trong game  
- **BattleEvent**: Specialized event class cho các events liên quan đến battle system
- **SkillEvent**: Event class cho skill-related events
- **ModChild**: Base class cho các module con của mod
- **ModSkill**: Base class cho custom skills
- **ModMaster_xxx**: Các partial classes mở rộng ModMaster cho specific event types (GameEvent, CommonEvent, MapEvent, BattleEvent, TimerEvent)

Các file trong folder này cung cấp structure và pattern cho việc phát triển mod, đảm bảo consistency và reusability across different mod implementations.