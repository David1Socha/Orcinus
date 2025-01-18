using Godot;

namespace Orcinus.Scripts.Models
{
    public class PowerupEffect : Godot.Object
    {
        public float Duration { get; set; }
        public float DurationElapsed { get; set; }
        public PowerupEnum Type { get; set; }
        public Color ModulateColor { get; set; }
        public Texture PowerupIcon { get; set; }
        public bool IsActive { get { return DurationElapsed < Duration; } }
        public float Scale { get; set; }
    }
}
