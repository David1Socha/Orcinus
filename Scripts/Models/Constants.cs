namespace Orcinus.Scripts.Models
{
    public static class Constants
    {
        public static class Groups
        {
            // markers
            public const string LevelSegmentEnd = "level_segment_end";
            public const string BiomeTransitionMarker = "biome_transition_marker";
            public const string TransitionSegment = "transition_segment";
            public const string TutorialChattyFish = "tutorial_chatty_fish";
        }

        public static class BuiltInSignals
        {
            public const string Toggled = "toggled";
            public const string Pressed = "pressed";
            public const string AreaEntered = "area_entered";
            public const string PopupHide = "popup_hide";
            public const string MouseEntered = "mouse_entered";
            public const string MouseExited = "mouse_exited";
        }

        public static class CollisionLayers
        {
            // NOTE: these are bits, not layers, so they will be off by one from the layer numbers
            public const int ObjectLayer = 0;
            public const int ProximityLayer = 1;
            public const int ActivatorLayer = 2;
            public const int HazardLayer = 4;
        }

        public static class Signals
        {
            public const string ProgressLoaded = nameof(ProgressLoaded);
            public const string DisplayedStatChanged = nameof(DisplayedStatChanged);
            public const string BiomeChanged = nameof(BiomeChanged);

            public const string OrcaHealthChanged = nameof(OrcaHealthChanged);
            public const string OrcaChanged = nameof(OrcaChanged);
            public const string OrcaFreed = nameof(OrcaFreed);

            public const string MusicEnabled = nameof(MusicEnabled);
            public const string SoundEnabled = nameof(SoundEnabled);

            public const string ObjectCollected = nameof(ObjectCollected);
            public const string HazardTriggered = nameof(HazardTriggered);
            public const string PowerupTriggered = nameof(PowerupTriggered);
            public const string PrizeTriggered = nameof(PrizeTriggered);

            public const string OrcaExitedProximity = nameof(OrcaExitedProximity);
            public const string OrcaEnteredProximity = nameof(OrcaEnteredProximity);

            public const string GameOver = nameof(GameOver);
            public const string PauseTriggered = nameof(PauseTriggered);
            public const string UnpauseTriggered = nameof(UnpauseTriggered);

            public const string DialogueClosed = nameof(DialogueClosed);
            public const string SessionSaved = nameof(SessionSaved);

            public const string ProgressDisplayClosed = nameof(ProgressDisplayClosed);
            public const string ProgressRowGainedFocus = nameof(ProgressRowGainedFocus);
        }

        public static class Dialogues
        {
            public const int TestDialogue = 1;
            public const int NewMissionsReady = 20;
            public const int NewMissionsReadyReminder = 21;
        }

        public static class Animations
        {
            public const string OrcaDamaged = "Damaged";
            public const string Retreat = "retreat";
            public const string Default = "default";
            public const string Powerup = "powerup_active";
        }

        public static class Config
        {
            public const string FilePath = "user://player_data.orcacfg";

            public const string SettingsSection = "Settings";
            public const string MusicSetting = "Music";
            public const string SoundSetting = "Sound";
            public const string VibrateSetting = "Vibrate";

            public const string TutorialSetting = "Tutorial";

            public const string UnlocksSection = "Unlocks";
            public const string ActiveOrcaSetting = "ActiveOrca";
            public const string StartingBiomeSetting = "StartingBiome";
            public const string ActiveHatSetting = "ActiveHat";
        }

        public static class AudioBuses
        {
            public const int MasterBus = 0;
            public const int SoundBus = 1;
            public const int MusicBus = 2;
        }

        public static class ShaderParams
        {
            public static string GetParamNodePath(string paramName) => $"shader_param/{paramName}";
            public const string PercentVisible = "percentVisible";
            public const string CircleSize = "circle_size";
        }

        public static class FeatureTags
        {
            public const string Steam = "steam";
        }

        public const uint SteamAppId = 2723270;

    }
}
