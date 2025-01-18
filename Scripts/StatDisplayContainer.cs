using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts
{
    public abstract class StatDisplayContainer : HBoxContainer
    {
        private const int ThousandsAbbreviationThreshold = 10000;
        private Label _messageLabel;
        [Export]
        protected readonly string MessagePrefix = "";
        [Export]
        protected readonly string MessagePostfix = "";
        [Export]
        protected readonly bool ShouldAbbreviateLargeNumbers = true;
        [Export]
        protected readonly StatEnum StatToDisplay;
        [Export]
        protected readonly bool HideHighScoreIndicator;
        private int _statValue;
        private TextureRect _highScoreIcon;
        private bool _isPsuedoTooltipOn = false;
        private string _tooltipPrefix = "";

        public override void _Ready()
        {
            if (!HideHighScoreIndicator)
            {
                var highScoreRect = GetNode<ColorRect>("IconPlaceholder");
                highScoreRect.Visible = true;
            }

            _highScoreIcon = GetNode<TextureRect>("IconPlaceholder/HighScoreIcon");
            _highScoreIcon.Visible = false;

            _statValue = -999; // setting to unreachable value, so we detect a change when the level actually initializes stats
            base._Ready();
            GlobalSignalBus.RegisterHandler(Signals.DisplayedStatChanged, this, nameof(OnDisplayedStatChanged));

            _messageLabel = GetNode<Label>("MessageLabel");
        }
        public void OnDisplayedStatChanged()
        {
            var currentStatValue = ProgressTracker.Progress.CurrentSessionStats[StatToDisplay];
            var recordStatValue = ProgressTracker.Progress.SingleSessionRecordStats[StatToDisplay];
            UpdateStatDisplay(currentStatValue);
            if (currentStatValue > recordStatValue)
            {
                _highScoreIcon.Visible = true;
            }
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (GDUtils.IsReleased(@event) && GetGlobalRect().HasPoint(GetViewport().GetMousePosition()) && !HideHighScoreIndicator)
            {
                _isPsuedoTooltipOn = !_isPsuedoTooltipOn;
                if (_isPsuedoTooltipOn)
                {
                    _tooltipPrefix = " " + HintTooltip;
                }
                else
                {
                    _tooltipPrefix = "";
                }

                UpdateMessageLabelText();
            }
            base._GuiInput(@event);
        }

        protected void UpdateStatDisplay(int newStatValue)
        {
            if (newStatValue == _statValue)
            {
                // in this case, no change to this stat, so we can return early
                return;
            }

            if (newStatValue != 0)
            {
                var tween = CreateTween();
                tween.TweenProperty(_messageLabel, "rect_scale", new Vector2(1.1f, 1.1f), .2f);
                tween.TweenProperty(_messageLabel, "rect_scale", new Vector2(1f, 1f), .2f);
            }

            _statValue = newStatValue;
            UpdateMessageLabelText();
        }

        public void UpdateMessageLabelText()
        {
            string msg;

            // Make sure this starts with highest threshold and goes down, to ensure correct display mode is used

            if (_statValue > ThousandsAbbreviationThreshold && ShouldAbbreviateLargeNumbers)
            {
                var statInThousands = _statValue / 1000;
                var statInHundreds = _statValue % 1000;
                var statDecimalValue = statInHundreds != 0 ? $".{statInHundreds / 10}" : "";
                msg = $"{statInThousands}{statDecimalValue}k";
            }
            else
            {
                msg = _statValue.ToString();
            }

            _messageLabel.Text = $"{MessagePrefix}{msg}{MessagePostfix}{_tooltipPrefix}";
        }
    }
}