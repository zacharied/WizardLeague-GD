using System.Linq;
using Godot;
using wizardballz.spells;

namespace wizardballz.world;

/// <summary>
/// Helper node added to the spell prefab by the spell manager that handles any "runaway" prefabs that weren't destroyed
/// by their creator.
/// <p />
/// We add it as a child of the prefab because the prefab itself might already have a script, and adding it as a child
/// allows easy access to position. It also reduces cleanup overhead.
/// </summary>
public partial class SpellPrefabHelper : Node
{
    private float MaxLifetime;
    private float Lifetime = 0;

    private SpellInstance SpellInstance = null!;
    private Node Parent = null!;

    public SpellPrefabHelper(float maxLifetime)
    {
        MaxLifetime = maxLifetime;
    }

    public override void _Ready()
    {
        Parent = GetParent();
    }

    public override void _Process(double delta)
    {
        if (MaxLifetime == 0) {
            // Not initialized at all, lol
            return;
        }
        
        Lifetime += (float)delta;

        if (Lifetime >= MaxLifetime) {
            GD.Print("Prefab destroy reason: lifetime");
            Cleanup();
            return;
        }

        if (Parent.GetChildren().Count <= 1) {
            GD.Print("Prefab destroy reason: no children");
            Cleanup();
            return;
        }
        
        foreach (var child in Parent.GetChildrenOfType<Node3D>()) {
            if (child.GlobalPosition.Length() > 1000) {
                GD.Print("Prefab destroy reason: too far");
                Cleanup();
                return;
            }
        }
    }

    private void Cleanup()
    {
        foreach (var area in this.GetChildrenOfType<Area3D>()) {
            area.Gravity = 0;
        }

        // Delay destroying the prefab so that the game can maybe reprocess gravity.
        // This is because if we delete a prefab with a gravity area, sometimes that area gets "stuck" with that gravity.
        GetTree().Connect(
            SceneTree.SignalName.ProcessFrame,
            Callable.From(SpellInstance.FinishEffect),
            flags: (uint)ConnectFlags.OneShot);
    }
}