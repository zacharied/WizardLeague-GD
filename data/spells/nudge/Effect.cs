using Godot;
using wizardballz.world;

public partial class Effect : GodotObject
{
    public void OneShot(SpellInstance spellInstance)
    {
        GD.Print("Nudge OneShot()");
		spellInstance.Ball.ApplyImpulse(spellInstance.Ball.LinearVelocity);
    }
}