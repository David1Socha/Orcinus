using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orcinus.Scripts.Core
{
    public static class GDUtils
    {
        public static List<Timer> ActiveTimers { get; set; } = new List<Timer>();

        public static async Task WaitAsync(Node self, float seconds, Node.PauseModeEnum pauseMode = Node.PauseModeEnum.Inherit)
        {
            Func<Task> waitFn = async () => await Wait(self, seconds, pauseMode);
            await waitFn();
        }

        public static SignalAwaiter Wait(Node self, float seconds, Node.PauseModeEnum pauseMode = Node.PauseModeEnum.Inherit)
        {
            return Wait(self, seconds, out var _, pauseMode);
        }

        public static SignalAwaiter Wait(Node self, float seconds, out Timer createdTimer, Node.PauseModeEnum pauseMode = Node.PauseModeEnum.Inherit)
        {
            var timer = new Timer()
            {
                Autostart = true,
                WaitTime = seconds,
                PauseMode = pauseMode
            };
            self.AddChildDeferred(timer);
            ActiveTimers.Add(timer);
            createdTimer = timer;
            return self.ToSignal(timer, "timeout");
        }

        public static void CancelAllActiveTimers()
        {
            foreach (var timer in ActiveTimers)
            {
                timer?.Stop();
                timer?.QueueFree();
            }

            ActiveTimers = new List<Timer>();
        }

        // Some devices (mostly Android/iPhone) have notches on screens that can display some pixels, but have others obscured by a camera or notch. This sets margins to the "safe area" so that no controls are unreachable
        public static void SetMarginsToScreenSafeArea(this Control ctrl)
        {
            var safeArea = OS.GetWindowSafeArea();
            ctrl.MarginLeft = safeArea.Position.x;
            ctrl.MarginTop = safeArea.Position.y;
            ctrl.MarginRight = -(OS.WindowSize.x - safeArea.End.x);
            ctrl.MarginBottom = -(OS.WindowSize.y - safeArea.End.y);
        }

        public static void ForceUIRedraw(this CanvasItem ci)
        {
            // ugly workaround to force label to recalc size -- see https://github.com/godotengine/godot/issues/20623
            // may have issues or limitations, could get broken when updating Godot version
            // seems unreliable, but works for my needs as of 3.5
            ci.PropagateNotification(CanvasItem.NotificationVisibilityChanged);
        }

        public static bool IsClicked(this InputEvent @event)
        {
            var mouseEvent = @event as InputEventMouseButton;
            if (mouseEvent != null)
            {
                return mouseEvent.ButtonIndex == (int)ButtonList.Left && mouseEvent.IsPressed();
            }
            else
            {
                return false;
            }
        }

        public static bool IsReleasedWithinControl(this InputEvent @event, Control ctrl)
        {
            return (@event.IsReleased() && ctrl.GetGlobalRect().HasPoint(ctrl.GetViewport().GetMousePosition()));
        }

        public static bool IsReleased(this InputEvent @event)
        {
            var mouseEvent = @event as InputEventMouseButton;
            if (mouseEvent != null)
            {
                return mouseEvent.ButtonIndex == (int)ButtonList.Left && !mouseEvent.Pressed;
            }
            else
            {
                return false;
            }
        }

        public static SignalAwaiter WaitForTweenCompletion(this Node self, SceneTreeTween tween)
        {
            return self.ToSignal(tween, "finished");
        }

        public static async Task ToTask(SignalAwaiter signalAwaiter)
        {
            await signalAwaiter;
        }

        public static NotificationHandler GetNotificationHandler(this Node node)
        {
            return node.GetNode<NotificationHandler>("/root/NotificationHandler/NotificationHandler");
        }

        public static T PickRandomElement<T>(this List<T> list, RandomNumberGenerator random)
        {
            return list == null ? default : list[random.RandiRange(0, list.Count - 1)];
        }

        public static T PickRandomElement<T>(this T[] list, RandomNumberGenerator random)
        {
            return list == null ? default : list[random.RandiRange(0, list.Length - 1)];
        }

        public static void Quit(this Node self)
        {
            self.GetTree().Quit();
        }

        // works around weird bug(?) with AnimationPlayer. See https://github.com/godotengine/godot/issues/27499
        public static void ActuallyStop(this AnimationPlayer anim, bool reset = true)
        {
            anim.Seek(0, true);
            anim.Stop(reset);
        }

        public static Godot.Collections.Array<Node> GetNestedChildren(this Node self)
        {
            var nestedChildren = new Godot.Collections.Array<Node>();
            nestedChildren.Add(self);

            if (self.GetChildCount() > 0)
            {
                var directChildren = self.GetChildren().Cast<Node>();
                foreach (var child in directChildren)
                {
                    var nestedDescendants = child.GetNestedChildren();
                    foreach (var descendant in nestedDescendants)
                    {
                        nestedChildren.Add(descendant);
                    }
                }
            }

            return nestedChildren;
        }

        public static void PlayIfAble(this AudioStreamPlayer player)
        {
            if (player.Stream != null)
            {
                player.Play();
            }
        }

        public static void PrintTs(string text)
        {
            GD.Print($"{Time.GetDatetimeStringFromSystem()} -- {text}");
        }


        public static void AddChildDeferred(this Node obj, Node child)
        {
            obj.CallDeferred("add_child", child);
        }

        public static void AddSiblingDeferred(this Node obj, Node child)
        {
            var parent = obj.GetParent();
            parent.CallDeferred("add_child_below_node", obj, child);
        }

        public static void VibrateIfAble(int durationMs)
        {
            bool canVibrate = IsVibratingPlatform();
            if (canVibrate)
            {
                if (Settings.VibrateEnabled)
                {
                    Input.VibrateHandheld(durationMs);
                }
            }
        }

        public static bool IsVibratingPlatform()
        {
            var platformName = OS.GetName();
            return platformName == "iOS" || platformName == "Android" || platformName == "HTML5";
        }

        public static ShaderMaterial AsShaderMaterial(this Material material)
        {
            return material as ShaderMaterial;
        }

        /// <summary>
        /// Randomly sets the active animation of the animated sprite to one of the named animations defined on the animated sprite's animated frames
        /// </summary>
        /// <param name="anim">animated sprite to be updated</param>
        /// <returns>string name of the animation that the AnimatedSprite was set to</returns>
        public static string SetAnimationToRandomValue(this AnimatedSprite anim)
        {
            var possibleAnimations = anim?.Frames?.GetAnimationNames();
            if (possibleAnimations == null)
            {
                return null;
            }

            var rng = new RandomNumberGenerator(); // not completely sure if this is valid in all cases, if called multiple times in same millisecond i think this gives dupe (not random) results
            rng.Randomize();
            int animationIndexToSet = rng.RandiRange(0, possibleAnimations.Length - 1);
            rng.Dispose();
            string animationToSet = possibleAnimations[animationIndexToSet];
            anim.Animation = animationToSet;
            return animationToSet;
        }

        public static void SetFilter(this Viewport viewport, bool isFilterEnabled)
        {
            uint filterFlag = (uint)Texture.FlagsEnum.Filter;
            if (isFilterEnabled)
            {
                viewport.GetTexture().Flags = viewport.GetTexture().Flags | filterFlag;
            }
            else
            {
                viewport.GetTexture().Flags = viewport.GetTexture().Flags & ~filterFlag;
            }
        }

        public static async Task WaitForAudioCompletion(this Godot.Object obj, AudioStreamPlayer player)
        {
            await obj.ToSignal(player, "finished");
        }

        public static async Task WaitForAnimationCompletion(this Godot.Object obj, AnimatedSprite anim)
        {
            await obj.ToSignal(anim, "animation_finished");
        }

        public static List<T> GetNodesInGroup<T>(this Node self, string group) where T : Node
        {
            var nodes = self
                .GetTree()
                .GetNodesInGroup(group)
                .Cast<T>()
                .ToList();
            return nodes;
        }

        /// <summary>
        /// Transitions from current root scene to the scene provided.
        /// </summary>
        /// <param name="self">A node under the current scene</param>
        /// <param name="newRootScene">The scene to transition to</param>
        public static void TransitionToScene(this Node self, Node newRootScene)
        {
            self.GetTree().Root.AddChild(newRootScene);
            self.GetTree().CurrentScene.QueueFree();
            self.GetTree().CurrentScene = newRootScene;
        }

        public static T SetScriptPreservingInstance<T>(this Godot.Object obj, Script script) where T : Godot.Object
        {
            var objId = obj.GetInstanceId();
            obj.SetScript(script);
            return GD.InstanceFromId(objId) as T;
        }

        public static void FireAndForget(Action action)
        {
            _ = Task.Run(action);
        }
    }
}