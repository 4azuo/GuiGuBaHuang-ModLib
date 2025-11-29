# ModEvent Editor

## Overview

Tab 5 in Project Editor. Most powerful tool - create event handlers visually or with code.

## Interface Modes

**GUI Mode:**
- Visual editor with dropdowns
- Select event, conditions, actions from lists
- Drag-and-drop reordering

**Code Mode:**
- C# editor with syntax highlighting
- Direct code editing
- Find/Replace support

## Event Types

**Timer:** OnTimeUpdate10ms, 100ms, 200ms, 500ms, 1000ms
**Map:** OnPlayerOpenTreeVault, OnMapLoaded
**Game:** OnGameStart, OnGameSave, OnGameEnd
**Battle:** OnBattleStart, OnBattleEnd, OnUnitDeath
**Custom:** User-defined events

## Properties

**CacheType:**
- Local: Separate cache per instance
- Global: Shared cache for all

**WorkOn:**
- All: All objects
- Town: Town only
- Avatar: Player only

## Conditions

**Categories:**
- Basic: IsNull, IsNotNull, IsTrue, IsFalse
- Player: IsPlayer, IsPlayerInTown, PlayerLevelGreaterThan
- Game State: IsGameRunning, IsGamePaused
- Random: RandomChance, RandomBetween

## Workflow

**Create Event:**
1. Click New
2. Enter class name (e.g., `PlayerLevelUpReward`)
3. Set OrderIndex, CacheType, WorkOn
4. Select Event
5. Add Conditions
6. Add Actions
7. Save

**Clone:** Duplicate event with new name

**Rename:** Change class name and filename

**Delete:** Remove event permanently
