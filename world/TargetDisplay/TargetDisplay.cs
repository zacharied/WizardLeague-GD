using System;
using System.Linq;
using System.Reflection.Metadata;
using Godot;
using wizardballz.exceptions;
using wizardballz.spells;

namespace wizardballz.world;

public partial class TargetDisplay : Node3D
{
    [Export] public GameInput GameInput = null!;
    [Export] public GameMatch GameMatch = null!;
    [Export] public GameField GameField = null!;
    [Export] public CanvasLayer CanvasLayer = null!;
    [Export] public Camera3D Camera = null!;
    
    [Export] private Material TransparentMaterial;
    
    private Decal TargetLineDecal = null!;
    private Decal TargetLineRemainder = null!;
    
    private MeshInstance3D FloatingTargetLine = null!;
    private bool BallFocus = false;

    private Node3D? PreviewPrefab = null!;

    public override void _Ready()
    {
        GodotUtil.ThrowIfNotPopulated(GameInput, GameMatch, GameField, CanvasLayer, Camera);

        TargetLineDecal = GetNode<Decal>("TargetLineDecal");
        TargetLineRemainder = TargetLineDecal.GetNode<Decal>("Remainder");
        FloatingTargetLine = GetNode<MeshInstance3D>("FloatingTargetLine");

        GameInput.SpellSelected += (spell, _) => PopulateForSpell(spell);
        GameMatch.Connect(GameMatch.SignalName.PlayStateChanged, Callable.From<uint>(newStateU =>
        {
            if (newStateU == (uint)GameMatch.MatchPlayState.Active && IsNodeReady()) {
                GD.Print("Populating spell for match state change");
                PopulateForSpell(GameMatch.GetLocalPlayer().SpellSlots[GamePlayer.SpellSlot.Primary]);
            }
        }));
    }

    public override void _Process(double delta)
    {
        if (GameMatch is not { PlayState: GameMatch.MatchPlayState.Active }) {
            Visible = false;
            return;
        }
        else {
            Visible = true;
        }
        
        UpdateTargetLineDecal();
        UpdateFloatingTargetLine();
        UpdatePrefabPreview();
        return;

        void UpdateTargetLineDecal()
        {
            if (!TargetLineDecal.Visible)
                return;

            var point1 = GameField.SpellCastSpawns[GameMatch.GetLocalPlayer().GetPlayerIndex()].GlobalPosition;
            var point2 = GameInput.WorldCursorCoordinates;
            var midpoint = (point1 + point2) / 2;
            var rotation = -new Vector2(point2.X, point2.Z).AngleToPoint(new(point1.X, point1.Z)) + Mathf.Pi / 2;
            var length = point1.DistanceTo(point2);
            TargetLineDecal.Position = midpoint;
            TargetLineDecal.Rotation = TargetLineDecal.Rotation with { Y = rotation };
            TargetLineDecal.Size = TargetLineDecal.Size with { Z = length };

            TargetLineRemainder.Position = new(0, 0, -TargetLineDecal.Size.Z / 2 - TargetLineRemainder.Size.Z / 2);
        }

        void UpdateFloatingTargetLine()
        {
            if (!FloatingTargetLine.Visible)
                return;

            var point1 = GameField.TurretPositions[GameMatch.GetLocalPlayer().GetPlayerIndex()];
            var point2 = BallFocus ? GameMatch.Ball.GlobalPosition : GameInput.WorldCursorCoordinates;
            var midpoint = (point1 + point2) / 2;
            FloatingTargetLine.GlobalPosition = midpoint;
            FloatingTargetLine.Scale = new(1, 1, point1.DistanceTo(point2));
            FloatingTargetLine.LookAt(point2);
        }

        void UpdatePrefabPreview()
        {
            if (PreviewPrefab is null) {
                return;
            }
            
            PreviewPrefab.GlobalPosition = GameInput.WorldCursorCoordinates;
        }
    }

    public void PopulateForSpell(SpellRecord? spellRecord)
    {
        HideAllChildren();
        
        if (spellRecord is null) {
            return;
        }
        
        if (spellRecord.Effect is SpawnProjectileSpellCastEfect projectileEffect) {
            if (projectileEffect.SpawnLocation == SpawnProjectileSpellCastEfect.SpawnPoint.Circle) {
                // Target line
                TargetLineDecal.Visible = true;
            } else if (projectileEffect.SpawnLocation == SpawnProjectileSpellCastEfect.SpawnPoint.Turret) {
                FloatingTargetLine.Visible = true;
            }
            else {
                GD.PushWarning($"invalid projectile effect");
            }

            if (spellRecord.Target == SpellRecord.TargetType.Ball) {
                BallFocus = true;
            }
        }
        else if (spellRecord.Effect is SpawnPrefabSpellCastEffect prefabEffect) {
            var prefab = prefabEffect.Prefab.Instantiate<Node3D>();
            var mesh = prefab.GetChildOfType<MeshInstance3D>();
            mesh.MaterialOverride = TransparentMaterial;
            mesh.Reparent(this);
            AddChild(mesh);
            prefab.Free();
            
            PreviewPrefab = mesh;
        }
        else {
            GD.PushWarning($"{nameof(TargetDisplay)} cannot handle effects of type {spellRecord.Effect.GetType()}");
            return;
        }

        return;

        void HideAllChildren()
        {
            foreach (var child in GetChildren().OfType<Node3D>()) {
                child.Visible = false;
            }

            if (PreviewPrefab is not null) {
                RemoveChild(PreviewPrefab);
                PreviewPrefab.QueueFree();
                PreviewPrefab = null;
            }
        }
    }
}