using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts
{
    [Tool]
    public class Settings : Node
    {
        public static Settings Instance;

        private EnhancedConfigFile _config;

        private bool _musicEnabled, _soundEnabled, _vibrateEnabled, _tutorialEnabled;
        private OrcaEnum _activeOrca;
        private HatEnum _activeHat;
        private BiomeEnum _startingBiome;


        public static BiomeEnum StartingBiome
        {
            get
            {
                return Instance._startingBiome;
            }
            set
            {
                Instance._setStartingBiome(value);
            }
        }

        public static bool ForceQuip { get; set; } = false;

        public static OrcaEnum ActiveOrca
        {
            get
            {
                return Instance._activeOrca;
            }
            set
            {
                Instance.SetActiveOrca(value);
            }
        }

        public static HatEnum ActiveHat
        {
            get
            {
                return Instance._activeHat;
            }
            set
            {
                Instance.SetActiveHat(value);
            }
        }

        public override void _Ready()
        {
            Instance = this;
            InitializeSettingsFromConfig();

            AudioServer.SetBusMute(AudioBuses.MusicBus, !_musicEnabled);
            AudioServer.SetBusMute(AudioBuses.SoundBus, !_soundEnabled);

            base._Ready();
        }

        private void InitializeSettingsFromConfig()
        {
            _config = new EnhancedConfigFile(Config.FilePath);

            _musicEnabled = _config.GetValue<bool>(Config.SettingsSection, Config.MusicSetting, false);
            _soundEnabled = _config.GetValue<bool>(Config.SettingsSection, Config.SoundSetting, true);
            _vibrateEnabled = _config.GetValue<bool>(Config.SettingsSection, Config.VibrateSetting, true);
            _tutorialEnabled = _config.GetValue<bool>(Config.SettingsSection, Config.TutorialSetting, true);

            _activeOrca = _config.GetValue<OrcaEnum>(Config.UnlocksSection, Config.ActiveOrcaSetting, OrcaEnum.CoralOrca);
            _activeHat = _config.GetValue<HatEnum>(Config.UnlocksSection, Config.ActiveHatSetting, HatEnum.None);
            _startingBiome = _config.GetValue<BiomeEnum>(Config.UnlocksSection, Config.StartingBiomeSetting, BiomeEnum.Coral);
        }

        public static bool MusicEnabled { get { return Instance._musicEnabled; } }
        public static bool SoundEnabled { get { return Instance._soundEnabled; } }
        public static bool VibrateEnabled { get { return Instance._vibrateEnabled; } }
        public static bool TutorialEnabled { get { return Instance._tutorialEnabled; } }

        // using explicit methods instead of setters, so these plays well with Godot signal linking

        public void SetMusicEnabled(bool isMusicEnabled)
        {
            _musicEnabled = isMusicEnabled;
            _config.SetValue(Config.SettingsSection, Config.MusicSetting, isMusicEnabled);
            _config.Save(Config.FilePath);

            AudioServer.SetBusMute(AudioBuses.MusicBus, !isMusicEnabled);
        }

        public void SetSoundEnabled(bool isSoundEnabled)
        {
            _soundEnabled = isSoundEnabled;
            _config.SetValue(Config.SettingsSection, Config.SoundSetting, isSoundEnabled);
            _config.Save(Config.FilePath);

            AudioServer.SetBusMute(AudioBuses.SoundBus, !isSoundEnabled);
        }

        public void SetVibrateEnabled(bool isVibrateEnabled)
        {
            _vibrateEnabled = isVibrateEnabled;
            _config.SetValue(Config.SettingsSection, Config.VibrateSetting, isVibrateEnabled);
            _config.Save(Config.FilePath);
        }

        public void SetTutorialEnabled(bool isTutorialEnabled)
        {
            _tutorialEnabled = isTutorialEnabled;
            _config.SetValue(Config.SettingsSection, Config.TutorialSetting, isTutorialEnabled);
            _config.Save(Config.FilePath);
            if (!isTutorialEnabled)
            {
                SteamWrapper.UnlockAchievement(AchievementEnum.Escapee);
            }
        }

        public void SetActiveOrca(OrcaEnum activeOrca)
        {
            _activeOrca = activeOrca;
            _config.SetValue(Config.UnlocksSection, Config.ActiveOrcaSetting, activeOrca);
            _config.Save(Config.FilePath);
        }

        public void SetActiveHat(HatEnum activeHat)
        {
            _activeHat = activeHat;
            _config.SetValue(Config.UnlocksSection, Config.ActiveHatSetting, activeHat);
            _config.Save(Config.FilePath);
        }

        private void _setStartingBiome(BiomeEnum biome)
        {
            _startingBiome = biome;

            _config.SetValue(Config.UnlocksSection, Config.StartingBiomeSetting, biome);
            _config.Save(Config.FilePath);
        }

    }
}
