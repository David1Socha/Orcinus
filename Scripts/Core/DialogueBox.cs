using Godot;
using System.Threading.Tasks;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.Core
{
    public class DialogueBox : VBoxContainer
    {
        private Label _dialogueLabel;
        private Control _control;
        private Polygon2D _dialogueTriangle;
        private TextureRect _nextIndicator;
        private AudioStreamPlayer _audio;

        public DialogueLine Line { get; set; }
        public Vector2 DialoguePosition { get; set; }
        public string SpeakerName { get; set; }
        public float PitchScale { get; set; }
        public Color DialogueColor { get; set; }
        private SceneTreeTween _tween;

        [Signal]
        public delegate void DialogueClosed();

        [Export]
        public float TextScrollDurationSeconds = 1f;

        [Export]
        public float TextWaitDurationSeconds = 2f;

        public override async void _Ready()
        {
            base._Ready();
            _dialogueLabel = GetNode<Label>("Control/DialogueLabel");
            _control = GetNode<Control>("Control");
            _dialogueTriangle = GetNode<Polygon2D>("Control/Triangle");
            _nextIndicator = GetNode<TextureRect>("Control/DialogueLabel/DialogueBackgroundPanel/NextIndicator");
            _audio = GetNode<AudioStreamPlayer>("OrcaSpeechAudio");

            await ShowDialogue();
        }

        private float HalfwayPointX => GetViewportRect().Size.x / 2f;

        public override void _GuiInput(InputEvent @event)
        {
            base._GuiInput(@event);

            bool isClicked = @event.IsClicked();
            if (isClicked)
            {
                AdvanceDialogueOneStep();
            }
        }

        private void AdvanceDialogueOneStep()
        {
            if (_tween != null)
            {
                if (_tween.GetTotalElapsedTime() < TextScrollDurationSeconds)
                {
                    _tween.CustomStep(TextScrollDurationSeconds - _tween.GetTotalElapsedTime());
                }
                else
                {
                    _tween.CustomStep(TextScrollDurationSeconds + TextWaitDurationSeconds - _tween.GetTotalElapsedTime());
                }
            }
        }

        public void StopAudio()
        {
            if (_audio.Playing)
            {
                _audio.Stop();
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            var mouseEvent = @event as InputEventMouseButton;
            if (@event.IsActionReleased("ui_accept") || @event.IsActionReleased("ui_cancel"))
            {
                AdvanceDialogueOneStep();
                GetTree().SetInputAsHandled();
            }
            base._UnhandledInput(@event);
        }

        public async Task ShowDialogue()
        {
            var replacedLine = Line.Line.Replace("XX", SpeakerName);
            GDUtils.PrintTs($"Dialogue triggered: {replacedLine}. Source: {SpeakerName}, Position: {DialoguePosition}");
            _dialogueLabel.Text = replacedLine;
            _dialogueLabel.SelfModulate = DialogueColor;
            _nextIndicator.SelfModulate = DialogueColor;
            _nextIndicator.Visible = false;

            this.ForceUIRedraw();

            bool isDialogueFlipped = DialoguePosition.x > HalfwayPointX;
            if (isDialogueFlipped)
            {
                RectScale = new Vector2(-1f, 1f);
                _dialogueLabel.RectScale = new Vector2(-1f, 1f);
                // using some weird constants which isn't ideal, but probably good enough for my use cases
                _dialogueLabel.RectPosition = new Vector2(_dialogueLabel.RectSize.x + 20, _dialogueLabel.RectPosition.y);
                _dialogueTriangle.Position = new Vector2(-50, _dialogueTriangle.Position.y);
            }

            float dialogBoxOffset = _dialogueLabel.RectSize.y / 2f;
            RectPosition = DialoguePosition - new Vector2(0f, dialogBoxOffset);
            _dialogueTriangle.Offset = new Vector2(0f, dialogBoxOffset);

            if (RectPosition.y + _dialogueLabel.RectSize.y >= GetViewportRect().Size.y)
            {
                var offsetY = RectPosition.y + _dialogueLabel.RectSize.y - GetViewportRect().Size.y;
                _dialogueLabel.RectPosition -= new Vector2(0, offsetY);
            }
            else if (RectPosition.y <= 0)
            {
                var offsetY = Mathf.Abs(RectPosition.y);
                _dialogueLabel.RectPosition += new Vector2(0, offsetY);
            }

            if (PitchScale > 0)
            {
                var rng = new RandomNumberGenerator();
                rng.Randomize();
                var playOffset = rng.RandfRange(0f, _audio.Stream.GetLength());
                _audio.PitchScale = PitchScale;
                _audio.Play(playOffset);
            }

            // TODO adjust dialogue background panel as needed to fit within screen height (lowpri, can work around this for now)

            _tween = CreateTween();
            _dialogueLabel.PercentVisible = 0f;
            _tween.TweenProperty(_dialogueLabel, "percent_visible", 1f, TextScrollDurationSeconds);
            _tween.TweenProperty(_nextIndicator, "visible", true, 0f);
            _tween.TweenCallback(this, nameof(StopAudio));
            _tween.TweenInterval(TextWaitDurationSeconds);
            await this.WaitForTweenCompletion(_tween);

            if (Visible)
            {
                Visible = false;
            }

            EmitSignal(Signals.DialogueClosed);
        }
    }
}
