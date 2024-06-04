using Godot;
using wizardballz.world;

namespace wizardballz.spells;

[GlobalClass]
public partial class SpawnPrefabSpellCastEffect : SpellCastEffect
{
    [Export] public PackedScene Prefab = null!;
    [Export(PropertyHint.Range, "5,60,0.5")] public float Lifetime = 5;

    public override void DoEffect(SpellInstance spellInstance, GameField gameField)
    {
        var prefabInstance = Prefab.Instantiate<Node3D>();
        if (prefabInstance is ISpellPrefabRoot root) {
            // Give the prefab a reference to the spell instance so they can manipulate game state and
            //  finish the spell.
            // This is optional because the spell will destroy from lifetime anyways.
            root.SpellInstance = spellInstance;
        }
        spellInstance.AddChild(prefabInstance);
        prefabInstance.GlobalPosition = spellInstance.TargetPosition;

        var prefabHelper = new SpellPrefabHelper(Lifetime);
        prefabInstance.AddChild(prefabHelper);
    }
}