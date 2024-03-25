using Godot;

namespace wizardballz.spells;

[GlobalClass]
public partial class SpawnPrefabSpellCastEffect : SpellCastEffect
{
    [Export] public PackedScene Prefab = null!;
    [Export(PropertyHint.Range, "5,60,0.5")] public float Lifetime;
    
    public override void DoEffect()
    {
        var prefabInstance = Prefab.Instantiate<Node3D>();
        SpellInstance.PrefabRoot.AddChild(prefabInstance);
        prefabInstance.GlobalPosition = SpellInstance.TargetPosition;
    }

    public override bool IsFinished(float lifetime)
    {
        return lifetime > Lifetime;
    }
}