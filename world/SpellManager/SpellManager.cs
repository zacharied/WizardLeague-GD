using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using wizardballz.spells;

namespace wizardballz.world;

/// <summary>
/// Serves as a container for the <see cref="SpellInstance"/>s created by the players. A new spell can be created via <see cref="BeginSpellCharge"/>.
/// Automatically deletes spells that have entered the <code>Finished</code> state.
/// </summary>
public partial class SpellManager : Node3D
{
    public const string Meta_ResourcePath = "spell_record_resource_path";
    
    [Signal]
    public delegate void SpellChargeStartedEventHandler(ulong spellId);
    
    [Signal]
    public delegate void SpellChargeFinishedEventHandler(ulong spellId);
        
    [Export] public GameField GameField;
    [Export] public RigidBody3D Ball;

    public override void _Process(double delta)
    {
        foreach (var spellInstance in this.GetChildrenOfType<SpellInstance>()) {
            if (spellInstance is { State: SpellInstance.SpellState.Finished, AnimationLock: false }) {
                RemoveChild(spellInstance);
                spellInstance.QueueFree();
            }
        }
    }

    public SpellInstance BeginSpellCharge(SpellRecord spellRecord, GamePlayer caster, Vector3 inputPosition, ulong spellId)
    {
        var resourcePath = spellRecord.ResourcePath;
        GD.Print($"START SPELL {spellId} \"{spellRecord.Name}\" [ target: {inputPosition}, source: {caster} ]");

        var spellTarget = spellRecord.Target switch
        {
            SpellRecord.TargetType.FieldPosition => inputPosition,
            SpellRecord.TargetType.Ball => Ball.GlobalPosition,
            SpellRecord.TargetType.Fixed => GameField.SpellCastSpawns[(caster.PlayerIndex + 1) % 2].GlobalPosition,
            _ => throw new IndexOutOfRangeException()
        };
        var spellInstance = new SpellInstance().Init(spellId, (SpellRecord)spellRecord.Duplicate(), Ball, caster, spellTarget);
        spellInstance.SetMeta(Meta_ResourcePath, resourcePath);
        spellInstance.SpellChargeFinished += () => {
            EmitSignal(SpellInstance.SignalName.SpellChargeFinished, spellInstance.SpellId);
        };
        
        spellInstance.TreeExiting += () =>
        {
            GD.Print($"END SPELL {spellId} \"{spellRecord.Name}\"");
        };
        
        AddChild(spellInstance);
        
        CallDeferred(MethodName.EmitSignal, SignalName.SpellChargeStarted, spellInstance.SpellId);

        return spellInstance;
    }

    public SpellInstance GetSpellInstance(ulong spellId)
    {
        if (TryGetSpellInstance(spellId, out var spellInstance)) {
            return spellInstance;
        }

        throw new IndexOutOfRangeException($"spell ID {spellId} not found");
    }

    public bool TryGetSpellInstance(ulong spellId, [MaybeNullWhen(false)] out SpellInstance spellInstance)
        => GetChildren().OfType<SpellInstance>().TrySingle(s => s.SpellId == spellId, out spellInstance);
}