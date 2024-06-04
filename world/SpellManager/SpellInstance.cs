using System;
using Godot;
using wizardballz.spells;

namespace wizardballz.world;

/// <summary>
/// A cast of a spell. <see cref="Lifetime"/> begins ticking as soon as the object is added to tree.
/// </summary>
public partial class SpellInstance : Node3D
{
    [Signal]
    public delegate void SpellChargeFinishedEventHandler();

    [Signal]
    public delegate void SpellEffectEndedEventHandler();
    
    public ulong SpellId { get; private set; } = 0;
    public RigidBody3D Ball { get; private set; } = null!;
    public SpellRecord Record { get; private set; } = null!;
    public GamePlayer Caster { get; private set; }
    public GameField GameField { get; private set; } = null!;
    public Vector3 TargetPosition { get; private set; }

    private SpellCastEffect Effect = null!;
    
    private float Lifetime = 0;
    public SpellState State = SpellState.Charging;
    public bool AnimationLock = false;

    public SpellInstance Init(
        ulong spellId,
        SpellRecord spellRecord,
        RigidBody3D ball,
        GamePlayer caster,
        GameField field,
        Vector3 targetPosition)
    {
        SpellId = spellId;
        Record = spellRecord;
        Ball = ball;
        Caster = caster;
        GameField = field;
        TargetPosition = targetPosition;
        return this;
    }

    public override void _Ready()
    {
        Effect = (SpellCastEffect)Record.Effect.Duplicate();
    }

    public override void _Process(double delta)
    {
        Lifetime += (float)delta;

        if (State == SpellState.Charging) {
            if (Lifetime > Record.ChargeDuration) {
                State = SpellState.PerformingEffect;
                EmitSignal(SignalName.SpellChargeFinished);
                
                GD.Print($"CAST SPELL {SpellId} \"{Record.Name}\"");
                Effect.DoEffect(this, GameField);
            }
        }
    }

    public void FinishEffect()
    {
        State = SpellState.Finished;
    }

    public enum SpellState
    {
        Charging,
        PerformingEffect,
        Finished
    }
}