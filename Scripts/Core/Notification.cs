using static Godot.Node;

namespace Orcinus.Scripts.Core
{
    public struct Notification
    {
        public string BbMessage { get; set; }
        public float SecondsToDisplay { get; set; }
        public PauseModeEnum PauseMode { get; set; }
        public float SecondsBeforeDisplaying { get; set; }

        public Notification(string bbMessage, float secondsToDisplay, PauseModeEnum pauseMode, float secondsBeforeDisplaying)
        {
            BbMessage = bbMessage;
            SecondsToDisplay = secondsToDisplay;
            PauseMode = pauseMode;
            SecondsBeforeDisplaying = secondsBeforeDisplaying;
        }

        public static bool operator ==(Notification a, Notification b)
        {
            return a.BbMessage == b.BbMessage && a.SecondsToDisplay == b.SecondsToDisplay && a.PauseMode == b.PauseMode && a.SecondsBeforeDisplaying == b.SecondsBeforeDisplaying;
        }
        public static bool operator !=(Notification a, Notification b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
