using System.Collections.Generic;
using System.Linq;
using Godot;
using wizardballz.spells;
using wizardballz.spells.SpellDeck;

namespace wizardballz.world;

public partial class GameDeck : Node
{
    [Export] public SpellDeck StartingDeck;

    private List<SpellRecord> UsedSpells = new();

    public GameDeck()
    { }

    public GameDeck(SpellDeck startingDeck)
    {
        StartingDeck = startingDeck;
    }

    public IEnumerable<SpellRecord> RemainingSpells =>
        StartingDeck.Spells.Except(UsedSpells);

    public SpellRecord TakeSpell()
    {
        var spells = RemainingSpells.ToList();
        var randi = GD.RandRange(0, spells.Count - 1);
        var spell = spells[randi];
        UsedSpells.Add(spell);

        if (UsedSpells.Count == StartingDeck.Spells.Count) {
            UsedSpells.Clear();
        }
        
        return spell;
    }

    public bool HasSpell()
    {
        return true;
    }
}