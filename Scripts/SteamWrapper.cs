using Godot;
using Orcinus.Scripts.Models;
using Steamworks;
using Steamworks.Data;
using System;

namespace Orcinus.Scripts
{
    public static class SteamWrapper
    {
        private static bool IsInitialized { get; set; }
        private static bool IsEnabled => OS.HasFeature("steam");
        private static bool IsActive => IsEnabled && SteamClient.IsValid && IsInitialized;
        public static bool InitIfEnabled()
        {
            if (IsEnabled)
            {
                GD.Print("Attempting to connect to steam");
                try
                {
                    SteamClient.Init(Constants.SteamAppId, asyncCallbacks: true);

                    if (SteamClient.IsValid)
                    {
                        GD.Print($"Successfully connected {SteamClient.Name} to steam");
                        IsInitialized = true;
                        return true;
                    }
                    else
                    {
                        GD.Print("Failed to connect to steam");
                    }

                }
                catch (System.Exception e)
                {
                    GD.PrintErr(e.Message);
                }
            }

            return false;
        }

        public static void ShutdownIfActive()
        {
            if (IsActive)
            {
                SteamClient.Shutdown();
            }
        }

        public static void UnlockAchievement(AchievementEnum achievement)
        {
            if (IsActive)
            {
                string achievementId = achievement.GetDescription();
                var achievementObj = new Achievement(achievementId);
                achievementObj.Trigger();
            }
        }

        public static void ClearAchievement(AchievementEnum achievement)
        {
            if (IsActive)
            {
                string achievementId = achievement.GetDescription();
                var achievementObj = new Achievement(achievementId);
                achievementObj.Clear();
            }
        }

        public static void AddOverlayHandler(Action<bool> action)
        {
            if (IsActive)
            {
                SteamFriends.OnGameOverlayActivated += action;
            }
        }

        public static void RemoveOverlayHandler(Action<bool> action)
        {
            if (IsActive)
            {
                SteamFriends.OnGameOverlayActivated -= action;
            }
        }
    }
}
