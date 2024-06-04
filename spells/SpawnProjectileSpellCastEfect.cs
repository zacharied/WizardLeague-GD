using System;
using Godot;
using wizardballz.world;

namespace wizardballz.spells;

[GlobalClass]
public partial class SpawnProjectileSpellCastEfect : SpellCastEffect
{
    [Export] public PackedScene Prefab = null!;
    [Export] public float Speed = 12f;
    [Export] public SpawnPoint SpawnLocation;
    [Export] public float Lifetime = 10f;
    [Export] public bool DestroyOnBallCollision = false;
    [Export] public bool DestroyOnWallCollision = false;

    public override void DoEffect(SpellInstance spellInstance, GameField gameField)
    {
        var prefab = Prefab.Instantiate<RigidBody3D>();
        spellInstance.AddChild(prefab);
        
        // Setup collisions for auto-kill zone, ball, and wall
        prefab.SetCollisionMaskValue(CollisionLayers.KillProjectile, true);
        if (DestroyOnWallCollision) prefab.SetCollisionMaskValue(CollisionLayers.Wall, true);
        if (DestroyOnBallCollision) prefab.SetCollisionMaskValue(CollisionLayers.Ball, true);
        prefab.BodyEntered += AnyEntered;

        prefab.GlobalPosition = SpawnLocation == SpawnPoint.Circle
            ? gameField.SpellCastSpawns[spellInstance.Caster.GetPlayerIndex()].GlobalPosition
            : gameField.TurretPositions[spellInstance.Caster.GetPlayerIndex()];

        var velocity = prefab.GlobalPosition.DirectionTo(spellInstance.TargetPosition) * Speed;
        if (SpawnLocation == SpawnPoint.Circle)
            velocity.Y = 0;
        prefab.LinearVelocity = velocity;

        var prefabHelper = new SpellPrefabHelper(Lifetime);
        prefab.AddChild(prefabHelper);
        
        return;

        void AnyEntered(Node other)
        {
            if (other is not PhysicsBody3D body3D) {
                return;
            }

            if (body3D.GetCollisionLayerValue(CollisionLayers.KillProjectile) 
                || (body3D.GetCollisionLayerValue(CollisionLayers.Wall) && DestroyOnWallCollision) 
                || (body3D.GetCollisionLayerValue(CollisionLayers.Ball) && DestroyOnBallCollision)) 
            {
                spellInstance.GetTree().Connect(
                    SceneTree.SignalName.ProcessFrame,
                    Callable.From(spellInstance.FinishEffect),
                    flags: (uint)ConnectFlags.OneShot);
            }
        }
    }

    public enum SpawnPoint
    {
        Circle,
        Turret
    }

    public Vector3 GetSourcePosition(GameField gameField, GamePlayer caster)
        => SpawnLocation switch
        {
            SpawnPoint.Circle => gameField.SpellCastSpawns[caster.GetPlayerIndex()].GlobalPosition,
            SpawnPoint.Turret => gameField.TurretPositions[caster.GetPlayerIndex()],
        };
}