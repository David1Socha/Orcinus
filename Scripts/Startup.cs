using Godot;

namespace Orcinus.Scripts
{
    public class Startup : Node
    {
        public override void _Ready()
        {
            base._Ready();

            OS.MinWindowSize = new Vector2(600, 400);

            SteamWrapper.InitIfEnabled();
        }

        public override void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                HandleQuit();
                GetTree().Quit(); // default behavior
            }
        }

        public void HandleQuit()
        {
            SteamWrapper.ShutdownIfActive();
        }
    }
}
