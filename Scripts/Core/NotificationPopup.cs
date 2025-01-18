using Godot;

namespace Orcinus.Scripts.Core
{
    public class NotificationPopup : PopupDialog
    {
        public override void _GuiInput(InputEvent @event)
        {
            if (@event.IsClicked() && Visible)
            {
                Hide();
            }

            base._GuiInput(@event);
        }
    }
}
