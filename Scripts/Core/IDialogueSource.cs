using Godot;

namespace Orcinus.Scripts.Core
{
    public interface IDialogueSource
    {
        Vector2 DialogueScreenPosition { get; }
        string DialogueName { get; }
        float PitchScale { get; }
        Color? DialogueColor { get; }
    }

    public class GenericDialogueSource : IDialogueSource
    {
        public Vector2 DialogueScreenPosition { get; set; }

        public string DialogueName { get; set; }

        public float PitchScale { get; set; }

        public Color? DialogueColor { get; set; }
    }
}
