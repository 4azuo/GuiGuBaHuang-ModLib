# DebugEvent (disabled)

`3161035078/ModProject/ModCode/ModMain/Mod/DebugEvent.cs` - cache key `ModConst.DEBUG_EVENT` - group: Core

**The whole file is commented out**: the class is not compiled and does nothing. It keeps old debugging handlers (`OnMonthly`, `OnMonoUpdate`, `OnSave`, `OnOpenUIEnd`, `OnLoadGame`) as a scratch pad. Its order slot is 9999 in [00-Overview](../00-Overview.md).

To debug today, use the logs (`docs/06-Debug.md`), `DebugHelper.WriteLine`, the unit tests and the autotest ([AutoTestEvent](AutoTestEvent.md)).
