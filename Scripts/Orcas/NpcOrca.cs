using Godot;
using Orcinus.Scripts.Core;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.Orcas
{
    [Tool]
    public class NpcOrca : KinematicBody2D
    {
        public bool IsFree { get; set; } = false;

        public override void _Ready()
        {
            GlobalSignalBus.RegisterHandler(Signals.OrcaFreed, this, nameof(OnOrcaFreed));
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            var bubbleEmitter = GetNode<Particles2D>("TransformInheritanceBlocker/OrcaPositionReceiver/OrcaBubbleEmitter");
            bubbleEmitter.Visible = false;

            var finlet1 = GetNode<Node2D>("Finlet1");
            finlet1.Visible = false;
            var finlet2 = GetNode<Node2D>("Finlet2");
            finlet2.Visible = false;

            var orcaAnimation = GetNode<AnimatedSprite>("OrcaSprites/Viewport/OrcaAnimation");
            orcaAnimation.FlipH = false;
            var orcaMouthAnimation = GetNode<AnimatedSprite>("OrcaSprites/Viewport/OrcaMouthAnimation");
            orcaMouthAnimation.FlipH = false;

            var orcaBodyCollision = GetNode<CollisionPolygon2D>("OrcaBodyCollision");
            orcaBodyCollision.Disabled = true;
            var orcaMouthShape = GetNode<CollisionShape2D>("OrcaMouthCollision/OrcaMouthCollisionShape");
            orcaMouthShape.Disabled = true;

            var remoteTransform = GetNode<RemoteTransform2D>("OrcaBubblesRemoteTransform");
            remoteTransform.UpdatePosition = false;

            var powerupAura = GetNode<Node2D>("ActivePowerup");
            powerupAura.Visible = false;
        }

        public void OnOrcaFreed()
        {
            IsFree = true;
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            if (IsFree)
            {
                MoveAndSlide(new Vector2(-250f, 0f));
            }
        }
    }
}
