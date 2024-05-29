using Godot;
using wizardballz.world;

namespace wizardballz.spells;

[GlobalClass]
public partial class OneShotSpellCastEffect : SpellCastEffect
{
    /// <summary>
    /// Must be a C# class inheriting from GodotObject that implements a method called <c>OneShot(SpellInstance)</c>.
    /// </summary>
    [Export] private CSharpScript Script;
    
    /// <summary>
    /// Optional, a prefab consisting at least of a child of type AnimationPlayer.
    /// </summary>
    [Export] private PackedScene? Prefab;

    public override void DoEffect(SpellInstance spellInstance)
    {
        var obj = Script.New();
        if (obj.VariantType != Variant.Type.Object) {
            GD.PushWarning("Script field is not set to a script");
            return;
        }
        
        if (!obj.AsGodotObject().HasMethod("OneShot")) {
            GD.PushWarning("OneShot script lacks a function called OneShot");
            return;
        }
        
        obj.AsGodotObject().Call("OneShot", spellInstance);
        
        if (Prefab is not null) {
            var prefab = Prefab.Instantiate<Node3D>();
            spellInstance.Ball.AddChild(prefab);

            if (prefab.GetChildOfTypeOrNull<AnimationPlayer>() is { } animation) {
                spellInstance.AnimationLock = true;
                animation.AnimationFinished += _ =>
                {
                    spellInstance.AnimationLock = false;
                    spellInstance.Ball.RemoveChild(prefab);
                    prefab.QueueFree();
                };
            }
        }
        
        spellInstance.FinishEffect();
    }
}