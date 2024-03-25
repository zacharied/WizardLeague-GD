using Godot;

namespace wizardballz.spells;

[GlobalClass]
public partial class SpawnProjectileSpellCastEfect : SpellCastEffect
{
    [Export] public PackedScene Prefab = null!;
    [Export] public float Speed = 5f;

    private bool PrefabDestroyed = false;

    private Node3D? PrefabInstance = null;
    
    public override void DoEffect()
    {
        PrefabInstance = Prefab.Instantiate<Node3D>();
        SpellInstance.PrefabRoot.AddChild(PrefabInstance);
        PrefabInstance.GlobalPosition = SpellInstance.CastPosition;
        PrefabInstance.TreeExiting += () =>
        {
            PrefabDestroyed = true;
        };
        
        SpellInstance.PrefabRoot.GetNode<DelayDrawer>("/root/DelayDrawer").RegisterNode(PrefabInstance, SpellInstance.PrefabRoot);
        
        ApplySpeed();
    }

    public override bool IsFinished(float lifetime)
    {
        return PrefabDestroyed;
    }

    private void ApplySpeed()
    {
        var velocity = SpellInstance.CastPosition.DirectionTo(SpellInstance.TargetPosition) * Speed;
        velocity.Y = 0;
        if (PrefabInstance! is RigidBody3D body) {
            body.LinearVelocity = velocity;
            body.BodyEntered += node =>
            {
                if (node is PhysicsBody3D body3D) {
                    if ((body3D.CollisionLayer & ((1 << 2) | (1 << 0))) > 0) {
                        CallDeferred(MethodName.RemovePrefabInstance);
                    }
                }
            };
        }
        else if (PrefabInstance is CharacterBody3D charBody) {
            charBody.Velocity = velocity;
        }
        else if (PrefabInstance is StaticBody3D staticBody) {
            staticBody.ConstantLinearVelocity = velocity;
        }
        else {
            GD.PushWarning($"Unable to apply velocity to projectile for spell {SpellInstance}");
        }
    }

    private void RemovePrefabInstance()
    {
        SpellInstance.PrefabRoot.GetNode<DelayDrawer>("/root/DelayDrawer").MarkForDeletion(PrefabInstance!, SpellInstance.PrefabRoot);
        SpellInstance.PrefabRoot.RemoveChild(PrefabInstance);
        PrefabInstance!.QueueFree();
    }
}