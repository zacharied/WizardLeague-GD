using Godot;
using wizardballz.world;

namespace wizardballz.spells;

public abstract partial class SpellCastEffect : Resource
{
    protected SpellInstance SpellInstance;

    public SpellCastEffect Init(SpellInstance instance)
    {
        SpellInstance = instance;
        return this;
    }

    public abstract void DoEffect();

    public abstract bool IsFinished(float lifetime);
}