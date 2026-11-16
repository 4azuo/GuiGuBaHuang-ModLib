# Create Mod Events

Inherit from `ModEvent` to handle game logic according to lifecycle events. You can create multiple ModEvents to handle different features.

## Event Execution Order

ModEvents execute sequentially. Control execution order using:

**Option 1: ModOrder Attribute on ModMain**
```csharp
[ModOrder("_EventOrderIndex.json")]
public class ModMain : ModChild
```

**Option 2: OrderIndex in Cache Attribute**
```csharp
[Cache("EventName", OrderIndex = 1)]
public class MyEvent : ModEvent
```

Lower OrderIndex values execute first.

## CacheType

Defines when the ModEvent is initialized/loaded:

- **Global:** Initialized/loaded from game menu
- **Local:** Initialized/loaded when entering save game

Example:
```csharp
[Cache("EventName", CacheType = CacheAttribute.CType.Global)]
public class MyEvent : ModEvent
```

## WorkOn

Defines the operational scope of the ModEvent:

- **All:** Operates in both game menu and save game
- **Global:** Only operates in game menu
- **Local:** Only operates in save game

Example:
```csharp
[Cache("EventName", WorkOn = CacheAttribute.WType.Local)]
public class MySaveGameEvent : ModEvent
```
