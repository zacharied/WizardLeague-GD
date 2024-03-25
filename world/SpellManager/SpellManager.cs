using System.Collections.Generic;
using Godot;

namespace wizardballz.world;

public partial class SpellManager : Node3D
{
    [Export] public Node3D SpellPrefabRoot;
    [Export] public GameField GameField;
    [Export] public Node3D Ball;
    
    public override void _Process(double delta)
    {
    }

    public void BeginSpellCharge(uint playerIndex, Vector3 position, SpellRecord spellRecord)
    {
        GD.Print($"Starting charge for spell {spellRecord.Name} (player {playerIndex})");
        
        var spellInstance = new SpellInstance().Init(playerIndex, (SpellRecord)spellRecord.Duplicate(), Ball, SpellPrefabRoot, GameField.SpellCastSpawns[playerIndex].GlobalPosition, position);
        AddChild(spellInstance);
    }
}