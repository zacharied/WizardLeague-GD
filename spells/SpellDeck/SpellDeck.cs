using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace wizardballz.spells.SpellDeck;

[GlobalClass]
public partial class SpellDeck : Resource
{
    [Export] public Godot.Collections.Array<SpellRecord> Spells;

    public SpellDeck()
    {
    }

    public SpellDeck(IEnumerable<SpellRecord> spells)
    {
        Spells = new Array<SpellRecord>(spells);
    }
}