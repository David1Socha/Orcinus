using Godot;

namespace Orcinus.Scripts.Orcas
{
    public class CutsceneOrca : Orca
    {
        private bool _canTakeInput = false;
        [Export]
        public bool CanTakeInput { get { return _canTakeInput; } set { _canTakeInput = value; } }
        private bool _fishBoostTriggered = false;
        [Export]
        public bool FishBoostTriggered { get { return _fishBoostTriggered; } set { _fishBoostTriggered = value; } }
        private bool _isAnimating = false;
        [Export]
        public bool IsAnimating
        {
            get { return _isAnimating; }
            set
            {
                _isAnimating = value;
            }
        }

        public override void _Ready()
        {
            base._Ready();
            UpdateAnimationStates();
        }

        public override void _Process(float dt)
        {
            if (CanTakeInput)
            {
                base._Process(dt);
            }

            UpdateAnimationStates();
        }

        public override void _PhysicsProcess(float delta)
        {
            if (CanTakeInput)
            {
                base._PhysicsProcess(delta);
            }
        }

        private void UpdateAnimationStates()
        {
            if (_isAnimating)
            {
                _orcaBody.Playing = true;
            }
            else
            {
                _orcaBody.Playing = false;
            }

            if (FishBoostTriggered)
            {
                FishBoostTriggered = false;
                StartFishBoost();
            }
        }

        // TODO at end of tutorial, set Settings.TutorialEnabled = false;
        // TODO add dialogue triggers
        // TODO add tutorial segments that loop until you learn them
        // TODO add skip button to pause menu (replacing home button)
    }
}
