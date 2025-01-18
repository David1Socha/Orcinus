using Godot;
using Godot.Collections;
using Orcinus.Scripts.Models;
using System;
using System.Collections.Generic;

namespace Orcinus.Scripts
{
    [Tool]
    public class Hat : Node2D
    {
        private HatEnum _activeHat;
        private Sprite _sprite;

        [Export]
        public HatEnum ActiveHat 
        {
            get { return _activeHat; }
            set 
            {
                _activeHat = value;
                SetSpriteBasedOnActiveHat();
            }
        }

        [Export]
        private readonly PackedScene[] Hats;

        public override void _Ready()
        {
            _sprite = GetNodeOrNull<Sprite>("HatSprite");
            SetSpriteBasedOnActiveHat();
        }

        private void SetSpriteBasedOnActiveHat()
        {
            var index = (int)_activeHat;
            var hatScene = Hats[index];
            _sprite?.QueueFree();
            _sprite = null;
            if (hatScene != null)
            {
                var newHatSprite = hatScene.Instance<Sprite>();
                _sprite = newHatSprite;
                newHatSprite.Name = "HatSprite";
                AddChild(newHatSprite);
            }
        }
    }

}
