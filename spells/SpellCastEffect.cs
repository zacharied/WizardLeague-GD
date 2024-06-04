using Godot;
using wizardballz.world;

namespace wizardballz.spells;

[GlobalClass]
public partial class SpellCastEffect : Resource
{
    public virtual void DoEffect(SpellInstance spellInstance, GameField gameField)
    {
    }
}