using Godot;

namespace Orcinus.Scripts.Core
{
    public class SafeAreaControl : Control
    {
        public override void _Ready()
        {
            base._Ready();

            this.SetMarginsToScreenSafeArea();
        }

        // I would love to subscribe to a callback or listen to a notification for orientation change here, but it seems like no such things exist
    }
}
