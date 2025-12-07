using EGameTypeData;
using ModLib.Attributes;
using System.Linq;
using UnityEngine;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for game state queries and operations.
    /// Provides utilities for checking game state, saving, and accessing game data.
    /// </summary>
    [ActionCat("Game")]
    public static class GameHelper
    {
        /// <summary>
        /// Saves the current game progress.
        /// </summary>
        public static void SaveGame()
        {
            EventHelper.CallGameEvent<SaveData>(EGameType.SaveData);
            g.data.SaveData(ActionHelper.TracedIl2Action<bool>((b) => { }));
        }

        /// <summary>
        /// Gets the current game difficulty level.
        /// </summary>
        /// <returns>The game difficulty level</returns>
        public static GameLevelType GetGameLevel()
        {
            return g.data.dataWorld.data.gameLevel;
        }

        /// <summary>
        /// Gets a code representing the current in-game date (YYYYMMDD format).
        /// </summary>
        /// <returns>Date code string</returns>
        public static string GetDayCode()
        {
            return $"{(g.world?.run?.roundMonth / 12) + 1:0000}{(g.world?.run?.roundMonth % 12) + 1:00}{g.world?.run?.roundDay + 1:00}";
        }

        /// <summary>
        /// Checks if a mask screen overlay is currently displayed.
        /// </summary>
        /// <returns>True if mask screen is active</returns>
        public static bool IsMaskScreen()
        {
            return g.ui.HasUI(UIType.Mask) || g.ui.HasUI(UIType.MaskNotClick);
        }

        /// <summary>
        /// Checks if any loading screen is currently displayed.
        /// </summary>
        /// <returns>True if loading screen is active</returns>
        public static bool IsLoadingScreen()
        {
            return g.ui.HasUI(UIType.Loading) ||
                g.ui.HasUI(UIType.LoadingBar) ||
                g.ui.HasUI(UIType.LoadingBig) ||
                g.ui.HasUI(UIType.LoadingBigCacheFix) ||
                g.ui.HasUI(UIType.LoadingCreateGame);
        }

        /// <summary>
        /// Checks if the mod menu screen is currently displayed.
        /// </summary>
        /// <returns>True if mod menu is active</returns>
        public static bool IsModScreen()
        {
            return g.ui.HasUI(UIType.ModMain);
        }

        /// <summary>
        /// Checks if the login screen is currently displayed.
        /// </summary>
        /// <returns>True if login screen is active</returns>
        public static bool IsLoginScreen()
        {
            return g.ui.HasUI(UIType.Login);
        }

        /// <summary>
        /// Checks if the player is in an active game (world loaded with valid player).
        /// </summary>
        /// <returns>True if in game</returns>
        public static bool IsInGame()
        {
            return (g.world?.isIntoWorld).Is(true) == 1 && g.world?.playerUnit?.GetUnitId() != null;
        }

        /// <summary>
        /// Checks if the player is currently in battle.
        /// </summary>
        /// <returns>True if in battle</returns>
        public static bool IsInBattlle()
        {
            return IsInGame() && (g.world?.battle?.isBattle).Is(true) == 1;
        }

        /// <summary>
        /// Checks if the game world is currently running (not paused).
        /// </summary>
        /// <returns>True if world is running</returns>
        public static bool IsWorldRunning()
        {
            return IsInGame() && (g.world?.run?.isRunning).Is(true) == 1;
        }

        /// <summary>
        /// Gets the current in-game year.
        /// </summary>
        /// <returns>Current year</returns>
        public static int GetGameYear()
        {
            return (g.world.run.roundMonth / 12) + 1;
        }

        /// <summary>
        /// Gets the current in-game month (1-12).
        /// </summary>
        /// <returns>Current month</returns>
        public static int GetGameMonth()
        {
            return (g.world.run.roundMonth % 12) + 1;
        }

        /// <summary>
        /// Gets the current in-game day.
        /// </summary>
        /// <returns>Current day</returns>
        public static int GetGameDay()
        {
            return g.world.run.roundDay + 1;
        }

        /// <summary>
        /// Gets the current in-game time as an integer (YYYYMMDD format).
        /// </summary>
        /// <returns>Time integer</returns>
        public static int GetGameTime()
        {
            return GetGameDay() + GetGameMonth() * 100 + GetGameYear() * 10000;
        }

        /// <summary>
        /// Gets the total number of months elapsed in the game.
        /// </summary>
        /// <returns>Total months</returns>
        public static int GetGameTotalMonth()
        {
            return g.world.run.roundMonth;
        }

        /// <summary>
        /// Gets the player's main storage building.
        /// </summary>
        /// <returns>The storage building, or null if not found</returns>
        public static MapBuildTownStorage GetStorage()
        {
            var storageTown = g.world.build.GetBuilds(MapTerrainType.Town).ToArray().FirstOrDefault(x => x.gridData.areaBaseID == 1);
            return storageTown?.GetBuildSub<MapBuildTownStorage>();
        }

        /// <summary>
        /// Gets all items stored in the player's main storage.
        /// </summary>
        /// <returns>List of stored items</returns>
        public static Il2CppSystem.Collections.Generic.List<DataProps.PropsData> GetStorageItems()
        {
            var storageTown = g.world.build.GetBuilds(MapTerrainType.Town).ToArray().FirstOrDefault(x => x.gridData.areaBaseID == 1);
            return storageTown?.GetBuildSub<MapBuildTownStorage>().data.propData.allProps;
        }

        /// <summary>
        /// Sets the game speed multiplier.
        /// </summary>
        /// <param name="multiplier">Speed multiplier (0 = paused, 1 = normal, >1 = faster)</param>
        public static void SpeedGame(float multiplier)
        {
            Time.timeScale = multiplier;
        }

        private static float oldSpeed = 1;
        
        /// <summary>
        /// Pauses the game by setting time scale to 0.
        /// Saves the current speed for restoring later.
        /// </summary>
        public static void PauseGame()
        {
            oldSpeed = Time.timeScale;
            SpeedGame(0);
        }

        /// <summary>
        /// Resumes the game by restoring the previous speed.
        /// </summary>
        public static void UnPauseGame()
        {
            SpeedGame(oldSpeed);
        }

        /// <summary>
        /// Toggles between paused and running states.
        /// </summary>
        public static void ChangeGameSpeed()
        {
            if (Time.timeScale == 0)
            {
                UnPauseGame();
            }
            else
            {
                oldSpeed = Time.timeScale;
                SpeedGame(0);
            }
        }
    }
}