using Godot;
using System.Collections.Generic;

namespace Orcinus.Scripts.Core
{
    public class NotificationHandler : Node
    {
        private static NotificationHandler Instance;
        private PopupDialog _popup;
        private RichTextLabel _popupText;
        private AudioStreamPlayer _sound;
        private Notification? _activeNotification = null;
        private Queue<Notification> _notificationQueue = new Queue<Notification>();
        public override void _Ready()
        {
            base._Ready();
            _popup = GetNode<PopupDialog>("PopupDialog");
            _popupText = GetNode<RichTextLabel>("PopupDialog/PopupText");
            _sound = GetNode<AudioStreamPlayer>("NotificationSound");
            Instance = this;
        }

        public static void QueueNotification(string bbMessage, float secondsToDisplay = 2f, PauseModeEnum pauseMode = PauseModeEnum.Process, float secondsBeforeDisplaying = .5f)
        {
            Instance._notificationQueue.Enqueue(new Notification(bbMessage, secondsToDisplay, pauseMode, secondsBeforeDisplaying));
            Instance.PlayTopNotificationAndCheckForNext();
        }

        public async void PlayTopNotificationAndCheckForNext()
        {
            if (_notificationQueue.Count == 0 || _activeNotification != null)
            {
                return;
            }


            _activeNotification = _notificationQueue.Dequeue();
            var notif = _activeNotification.Value;
            if (notif.SecondsBeforeDisplaying > 0f)
            {
                await GDUtils.Wait(this, seconds: notif.SecondsBeforeDisplaying, pauseMode: notif.PauseMode);
            }

            _sound.PlayIfAble();
            _popupText.BbcodeText = $"[center]{notif.BbMessage}[/center]";
            _popup.Popup_();
            await GDUtils.Wait(this, seconds: notif.SecondsToDisplay, pauseMode: notif.PauseMode);

            if (_activeNotification == notif)
            {
                _popup.Hide();
            }

        }

        public void OnNotificationPopupHidden()
        {
            _activeNotification = null;
            PlayTopNotificationAndCheckForNext();
        }
    }
}
