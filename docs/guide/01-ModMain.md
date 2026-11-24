# Create ModMain.cs

Entry point inheriting from `ModChild` (ModLib.dll).

**Important:** Each mod project must have exactly ONE class inheriting from `ModChild`. This class is executed first when the mod loads, runs only once during mod initialization.

## Key Components

- **ModChild:** Base class for mod lifecycle
- **Cache Attribute:** `[Cache("modId")]` required for mod registration
  - `modId` is the unique identifier for your mod
  - Example: Project 3161035078 has namespace `MOD_nE7UL2` and uses `[Cache("nE7UL2")]`
  - Format: `[Cache("nE7UL2", CacheType = CacheAttribute.CType.Global, OrderIndex = 0)]`
- **Constants:** MOD_ID, MOD_NAME, VERSION
- **OnLoadClass():** Override to initialize mod (call `base.OnLoadClass()` first)

## Implementation

**Location:** `ModProject/ModCode/ModMain/ModMain.cs`
**Namespace:** `MOD_xxxxx` (e.g., `MOD_nE7UL2`)
**Class Name:** Typically `ModMain`
**Cache ID:** Extract from namespace (e.g., namespace `MOD_nE7UL2` → Cache ID `nE7UL2`)
**Using:** `ModLib.Mod`, `ModLib.Helper`
