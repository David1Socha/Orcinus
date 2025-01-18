using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using System.Linq;
using static Orcinus.Scripts.Models.Constants;
using ProgressBar = Godot.ProgressBar;

namespace Orcinus.Scripts
{
    [Tool]
    public class ProgressRow : Control
    {
        [Export]
        private readonly Texture Icon;
        [Export]
        private readonly string Title;
        [Export(PropertyHint.MultilineText)]
        private readonly string Description;
        [Export]
        private readonly OrcaEnum Orca = OrcaEnum.Default;
        [Export]
        private readonly PowerupEnum Powerup = PowerupEnum.Default;
        [Export]
        private readonly BiomeEnum Biome = BiomeEnum.Test;
        [Export]
        private readonly HatEnum Hat = HatEnum.Default;
        [Export]
        private readonly float IconAnchorPadding = .05f;
        [Export]
        private readonly int SpeedStat;
        [Export]
        private readonly int PowerStat;
        [Export]
        private readonly int AgilityStat;
        [Export]
        private readonly int SizeStat;

        private Button _ToggledButton;
        private TextureRect _Icon;
        private Label _TitleLabel;
        private Label _DescriptionLabel;
        private HBoxContainer _StatsHbox;
        private ProgressBar _SpeedBar, _PowerBar, _AgilityBar, _SizeBar;
        private readonly Color GrayedOutColor = new Color(1f, 1f, 1f, .5f);
        private const string InactiveText = "";

        [Signal]
        public delegate void ProgressRowGainedFocus(ProgressRow focusedRow);

        public override void _EnterTree()
        {
            base._EnterTree();
            RequestReady();
        }

        public void GrabFocusForRow()
        {
            _ToggledButton.GrabFocus();
            EmitSignal(Signals.ProgressRowGainedFocus, this);
        }

        public override void _Ready()
        {
            base._Ready();
            ProgressTracker.LoadProgress();

            _ToggledButton = GetNode<Button>("Button");
            _Icon = GetNode<TextureRect>("IconControl/Icon");
            _Icon.AnchorLeft = IconAnchorPadding;
            _Icon.AnchorRight = 1f - IconAnchorPadding;
            _TitleLabel = GetNode<Label>("VBoxContainer/Title");
            _DescriptionLabel = GetNode<Label>("VBoxContainer/Description");
            _StatsHbox = GetNodeOrNull<HBoxContainer>("StatsHbox");
            _SpeedBar = GetNodeOrNull<ProgressBar>("StatsHbox/Speed/ProgressBar");
            _PowerBar = GetNodeOrNull<ProgressBar>("StatsHbox/Power/ProgressBar");
            _AgilityBar = GetNodeOrNull<ProgressBar>("StatsHbox/Agility/ProgressBar");
            _SizeBar = GetNodeOrNull<ProgressBar>("StatsHbox/Size/ProgressBar");

            GlobalSignalBus.RegisterEmitter(Signals.ProgressRowGainedFocus, this);

            PullProgressAndSettingsThenRedraw();
        }

        private void PullProgressAndSettingsThenRedraw()
        {
            bool isLocked = false;

            if (Orca != OrcaEnum.Default)
            {
                if (Settings.Instance != null && Settings.ActiveOrca == Orca)
                {
                    _ToggledButton.Disabled = false;
                    _ToggledButton.Pressed = true;
                    _DescriptionLabel.Text = Description;
                }
                else
                {
                    UpdateRowBasedOnProgress(Orca, "orca");
                }

                if (_ToggledButton.Disabled)
                {
                    isLocked = true;
                    _ToggledButton.Text = "Locked";
                }
                else if (!_ToggledButton.Pressed)
                {
                    _ToggledButton.Text = InactiveText;
                }
            }
            else if (Hat != HatEnum.Default)
            {
                if (Settings.Instance != null && Settings.ActiveHat == Hat)
                {
                    _ToggledButton.Disabled = false;
                    _ToggledButton.Pressed = true;
                    _DescriptionLabel.Text = Description;
                }
                else
                {
                    UpdateRowBasedOnProgress(Hat, "hat");
                }

                if (_ToggledButton.Disabled)
                {
                    isLocked = true;
                    _ToggledButton.Text = "Locked";
                }
                else if (!_ToggledButton.Pressed)
                {
                    _ToggledButton.Text = InactiveText;
                }
            }
            else if (Biome != BiomeEnum.Test)
            {
                UpdateRowBasedOnProgress(Biome, "biome");
                isLocked = _ToggledButton.Disabled;
                _ToggledButton.Text = "";
                _ToggledButton.Disabled = true;
            }
            else if (Powerup != PowerupEnum.Default)
            {
                isLocked = _ToggledButton.Disabled;
                UpdateRowBasedOnProgress(Powerup, "powerup");
                _ToggledButton.Text = "";
                _ToggledButton.Disabled = true;
            }

            _Icon.Texture = Icon;
            _TitleLabel.Text = Title;

            if (_StatsHbox != null)
            {
                _SpeedBar.Value = SpeedStat;
                _AgilityBar.Value = AgilityStat;
                _PowerBar.Value = PowerStat;
                _SizeBar.Value = SizeStat;

                if (isLocked)
                {
                    _StatsHbox.Modulate = GrayedOutColor;
                }
            }

            if (isLocked)
            {
                _Icon.Modulate = GrayedOutColor;
                _TitleLabel.Modulate = GrayedOutColor;
                _DescriptionLabel.Modulate = GrayedOutColor;
            }
        }

        private bool UpdateRowBasedOnProgress<TEnum>(TEnum type, string typeName) where TEnum : struct
        {
            bool isUnlocked;
            var unlockable = ProgressTracker.Progress.Unlockables
                    .Where(u => u.Type == typeof(TEnum) && u.EnumValue.Equals(type))
                    .FirstOrDefault();
            if (unlockable == null)
            {
                return true;
            }

            bool isBiomeGated = unlockable.BiomeThreshold != null;
            if (isBiomeGated)
            {
                isUnlocked = ProgressTracker.Progress.UnlockedBiomes.Contains(unlockable.BiomeThreshold.Value) && ProgressTracker.Progress.UnlockedOrcas.Contains((OrcaEnum)unlockable.EnumValue);
                if (!isUnlocked)
                {
                    _DescriptionLabel.Text = $"Locked. Reach {unlockable.BiomeThreshold.GetDescription()} biome to discover this {typeName}.";
                }
            }
            else
            {
                isUnlocked = ProgressTracker.Progress.CurrentPlayerLevel >= unlockable.LevelThreshold.Value;
                if (!isUnlocked)
                {
                    _DescriptionLabel.Text = $"Locked. Reach level {unlockable.LevelThreshold} to unlock this {typeName}.";
                }
            }

            if (isUnlocked)
            {
                _ToggledButton.Disabled = false;
                _ToggledButton.Pressed = false;
                _DescriptionLabel.Text = Description;
                return true;
            }
            else
            {
                _ToggledButton.Disabled = true;
                _ToggledButton.Pressed = false;
                return false;
            }
        }

        public void OnToggled(bool isToggled)
        {
            if (isToggled)
            {
                _ToggledButton.Text = "Active";
                if (Orca != OrcaEnum.Default)
                {
                    if (Settings.ActiveOrca != Orca)
                    {
                        Settings.ForceQuip = true;
                    }

                    Settings.ActiveOrca = Orca;
                }
                else if (Hat != HatEnum.Default)
                {
                    _ToggledButton.Text = "Active";
                    Settings.ActiveHat = Hat;
                    if (Hat != HatEnum.None)
                    {
                        SteamWrapper.UnlockAchievement(AchievementEnum.Fashionista);
                    }
                }
            }
            else
            {
                _ToggledButton.Text = InactiveText;
            }
        }

    }
}
