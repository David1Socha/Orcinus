using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts
{
    class CurrentLevelLabel : Label
    {
        public override void _Ready()
        {
            GlobalSignalBus.RegisterHandler(Signals.ProgressLoaded, this, nameof(OnLevelChanged));
            GlobalSignalBus.RegisterHandler(Signals.GameOver, this, nameof(OnLevelChanged));
        }

        public void OnLevelChanged()
        {
            Text = $"Level {ProgressTracker.Progress.CurrentPlayerLevel}";
        }
    }
}
