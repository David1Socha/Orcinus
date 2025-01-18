using Godot;
using Orcinus.Scripts.CollectibleObjects;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using System.Linq;

namespace Orcinus.Scripts.Orcas
{
    public class AbstractOrca : Node2D
    {
        private Orca _placeholderOrca;

        [Export]
        private PackedScene[] OrcaScenes;

        [Export]
        private PackedScene[] FishScenes;

        [Export]
        private NodePath PositionFollower;

        [Export]
        private bool IsNpc = false;

        [Export]
        private bool DelaySpawn = false;

        public BiomeEnum AssociatedBiome { get; set; } = BiomeEnum.Coral;

        public override void _Ready()
        {
            base._Ready();
            _placeholderOrca = GetNode<Orca>("PlaceholderOrca");
            _placeholderOrca?.QueueFree();


            if (!DelaySpawn)
            {
                SpawnOrca();
            }
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (DelaySpawn)
            {
                SpawnOrca();
                DelaySpawn = false;
            }
        }

        private void SpawnOrca()
        {
            var random = new RandomNumberGenerator();
            random.Randomize();

            int activeOrcaIndex = (int)Settings.ActiveOrca - 1;
            var scenesToChooseFrom = OrcaScenes.Where((_, i) => i != activeOrcaIndex).ToArray();
            PackedScene scene;
            string script;
            bool isFish = false;

            if (IsNpc)
            {
                var unlockableOrcas = ProgressTracker.Progress.UnlockableOrcas;
                if (ProgressTracker.Progress.UnlockableOrca != null
                    && AssociatedBiome == unlockableOrcas.First(uo => (OrcaEnum)uo.EnumValue == ProgressTracker.Progress.UnlockableOrca).BiomeThreshold
                    && ProgressTracker.Progress.RescuedOrca == null)
                {
                    scene = OrcaScenes[(int)ProgressTracker.Progress.UnlockableOrca.Value - 1];
                    script = "res://Scripts/Orcas/NpcOrca.cs";
                    ProgressTracker.Progress.RescuedOrca = ProgressTracker.Progress.UnlockableOrca.Value;
                }
                else
                {
                    isFish = true;
                    scene = FishScenes.PickRandomElement(random);
                    script = null;
                }
            }
            else
            {
                scene = OrcaScenes[activeOrcaIndex];
                script = "res://Scripts/Orcas/Orca.cs";
            }

            var node = scene.Instance<Node2D>();
            if (script != null)
            {
                node = node.SetScriptPreservingInstance<KinematicBody2D>(GD.Load<Script>(script));
            }

            node.Position = Position;
            if (!IsNpc)
            {
                (node as Orca).PositionFollower = PositionFollower;
                (node as Orca).ActiveHat = Settings.ActiveHat;
            }
            else if (isFish)
            {
                (node as CollectiblePrize).Velocity = 0f;
            }

            this.AddSiblingDeferred(node);
        }
    }
}
