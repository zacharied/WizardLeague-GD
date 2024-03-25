using Godot;
using wizardballz.spells;

namespace wizardballz.world;

public partial class SpellInstance : Node3D
{
    public uint PlayerIndex { get; private set; } = 0;
    public Node3D PrefabRoot { get; private set; } = null!;
    public Node3D Ball { get; private set; } = null!;
    public SpellRecord Record { get; private set; } = null!;
    public Vector3 CastPosition { get; private set; }
    public Vector3 TargetPosition { get; private set; }

    private SpellCastEffect Effect = null!;
    
    private float Lifetime = 0;
    private SpellState State = SpellState.Charging;

    public SpellInstance Init(uint playerIndex, SpellRecord spellRecord, Node3D ball, Node3D prefabRoot, Vector3 castPosition, Vector3 targetPosition)
    {
        PlayerIndex = playerIndex;
        Record = spellRecord;
        Ball = ball;
        PrefabRoot = prefabRoot;
        CastPosition = castPosition;
        TargetPosition = targetPosition;
        return this;
    }

    public override void _Ready()
    {
        Effect = (SpellCastEffect)Record.Effect.Duplicate();
        Effect.Init(this);
    }

    public override void _Process(double delta)
    {
        Lifetime += (float)delta;

        if (State == SpellState.Charging) {
            if (Lifetime > Record.ChargeDuration) {
                GD.Print($"Finished charge for spell {Record.Name}, casting!");
                State = SpellState.PerformingEffect;
                Effect.DoEffect();
            }
        }
    }

    public enum SpellState
    {
        Charging,
        PerformingEffect
    }
}