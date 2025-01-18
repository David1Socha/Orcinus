using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using System.Collections.Generic;
using System.Linq;

namespace Orcinus.Scripts.CollectibleObjects
{
    public class AbstractCollectiblePowerup : Node2D
    {
        [Export]
        public PackedScene[] Powerups { get; set; }

        [Export(PropertyHint.Range, "0,1,.01")]
        public float SpawnProbability { get; set; } = 1f;

        public int OrcaHealth { get; set; } // not yet impled

        public override void _Ready()
        {
            base._Ready();

            var placeholder = GetNode<Sprite>("PlaceholderSprite");
            placeholder.Visible = false;

            var random = new RandomNumberGenerator();
            random.Randomize();

            var eligiblePowerupScenes = new List<PackedScene>();
            for (int i = 0; i < Powerups.Length; i++)
            {
                var powerupScene = Powerups[i];
                var powerupEnum = (PowerupEnum)i;
                if (powerupEnum == PowerupEnum.HealthPack)
                {
                    if (OrcaHealth < 3)
                    {
                        eligiblePowerupScenes.Add(powerupScene);
                    }
                }
                else
                {
                    if (ProgressTracker.Progress.UnlockedPowerups.Contains(powerupEnum))
                    {
                        eligiblePowerupScenes.Add(powerupScene);
                    }
                }
            }

            var selectedPowerupScene = eligiblePowerupScenes.PickRandomElement(random);

            var powerup = selectedPowerupScene.Instance<CollectiblePowerup>();
            powerup.SpawnProbability = SpawnProbability;
            powerup.Position = Position;
            this.AddSiblingDeferred(powerup);
        }
    }
}
