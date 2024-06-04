using System;
using System.Runtime.Serialization;
using Godot;
using wizardballz.spells;

namespace wizardballz.world;

public partial class IndicatorDisplay : Node3D
{
    private const float FadeTime = 5f;
    private const float FadeDepleteQuicklyMultiplier = 4f;

    private const string Meta_DepleteQuickly = "deplete_quickly";

    [Export] public SpellManager SpellManager = null!;
    [Export] public GameField GameField = null!;
    [Export] public Texture2D DefaultIndicator = null!;
    [Export] public Texture2D DefaultLine = null!;

    public override void _Ready()
    {
        GodotUtil.ThrowIfNotPopulated(SpellManager, GameField, DefaultIndicator, DefaultLine);
        
        var node = new Node();
        AddChild(node);
        GD.Print(node);

        SpellManager.Connect(SpellManager.SignalName.SpellChargeStarted, Callable.From<ulong>(spellId =>
        {
            GD.Print("Indicator spell!");
            if (SpellManager.TryGetSpellInstance(spellId, out var spellInstance)) {
                GD.Print(spellInstance);
                if (spellInstance.Record.Effect is SpawnProjectileSpellCastEfect effect) {
                    DrawIndicatorLine(spellInstance.TargetPosition, effect.GetSourcePosition(GameField, spellInstance.Caster), spellId);
                }
                else if (spellInstance.Record.Effect is SpawnPrefabSpellCastEffect prefabEffect) {
                    var tempPrefab = prefabEffect.Prefab.Instantiate<Node3D>();
                    var mesh = tempPrefab.GetChildOfType<MeshInstance3D>();
                    tempPrefab.RemoveChild(mesh);
                    mesh.Ready += () =>
                    {
                        DrawIndicatorCircle(spellInstance.TargetPosition, mesh.GetAabb().GetLongestAxisSize(),
                            spellId);
                    };
                    AddChild(mesh);
                    tempPrefab.Free();
                }
            }
        }));
    }

    public override void _Process(double delta)
    {
        foreach (var child in this.GetChildrenOfType<Decal>()) {
            child.Modulate = child.Modulate with
            {
                A = child.Modulate.A - (1f / FadeTime) * (float)delta *
                ((bool)child.GetMeta(Meta_DepleteQuickly, false) ? FadeDepleteQuicklyMultiplier : 1)
            };
            if (child.Modulate.A <= 0) {
                RemoveChild(child);
                child.QueueFree();
            }
        }
    }

    public void DrawIndicatorCircle(Vector3 castPosition, float scale, ulong spellId)
    {
        if (!SpellManager.TryGetSpellInstance(spellId, out var spellInstance)) {
            GD.PushWarning($"attempted to create indicator for nonexistent spell {spellId}");
            return;
        }

        var decal = new Decal()
        {
            TextureAlbedo = DefaultIndicator,
            TextureEmission = DefaultIndicator,
            Scale = new Vector3(scale, scale, scale)
        };

        decal.Ready += () => { decal.GlobalPosition = castPosition; };

        AddChild(decal);
    }

    public void DrawIndicatorLine(Vector3 targetPosition, Vector3 source, ulong spellId)
    {
        if (!SpellManager.TryGetSpellInstance(spellId, out var spellInstance)) {
            GD.PushWarning($"attempted to create indicator for nonexistent spell {spellId}");
            return;
        }

        var decal = new Decal()
        {
            TextureAlbedo = DefaultLine,
            Size = new(0.5f, 2, 40),
            Modulate = spellInstance.Caster.IndicatorColor with { A = 1f }
        };

        decal.Ready += () =>
        {
            decal.LookAtFromPosition(targetPosition, source);
        };

        spellInstance.TreeExiting += () => { decal.SetMeta(Meta_DepleteQuickly, true); };

        AddChild(decal);
    }
}