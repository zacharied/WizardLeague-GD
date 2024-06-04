using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using wizardballz.spells;
using wizardballz.world.Network;

namespace wizardballz.world;

public partial class GameInput : Node3D
{
    [Signal]
    public delegate void SpellSelectedEventHandler(SpellRecord spellRecord, uint uSpellSlotEnum);
    
    public const float MouseRaycastLength = 1000;
    
    [Export] public Camera3D Camera = null!;
    [Export] public NetworkSpellManager SpellManager = null!;
    [Export] public GameMatch? GameMatch = null!;
    [Export] public PackedScene WorldCursorPrefab = null!;

    private Node3D WorldCursor = null!;
    public Vector3 WorldCursorCoordinates => WorldCursor.GlobalPosition;
    
    #region Spell slot
    private GamePlayer.SpellSlot _selectedSpellSlot;
    public GamePlayer.SpellSlot SelectedSpellSlot
    {
        get => _selectedSpellSlot;
        private set
        {
            _selectedSpellSlot = value;
            
            var spell = GameMatch?.GetLocalPlayer().SpellSlots[SelectedSpellSlot];
            if (spell != null) {
                EmitSignal(SignalName.SpellSelected, spell, (uint)SelectedSpellSlot);
            }
        }
    }
    #endregion

    public override void _Ready()
    {
        WorldCursor = WorldCursorPrefab.Instantiate<Node3D>();
        AddChild(WorldCursor);
    }

    public override void _Process(double delta)
    {
        
        if (TryProjectMouseToWorld(out var worldPosition)) {
            WorldCursor.GlobalPosition = worldPosition;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }) {
            if (GameMatch.PlayState != GameMatch.MatchPlayState.Active)
                return;
            
            if (TryProjectMouseToWorld(out var worldPosition)) {
                GameMatch.GetLocalPlayer().CastSpell(worldPosition, SelectedSpellSlot);
            }
        } else if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: true }) {
            // Deselect selected spell
            SelectedSpellSlot = GamePlayer.SpellSlot.Primary;
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        // Select spell slot when number key pressed
        if (@event is InputEventKey { KeyLabel: >= Key.Key1 and <= Key.Key3, Pressed: true } keyInput) {
            var spellSlot = (GamePlayer.SpellSlot)((int)keyInput.KeyLabel - (int)Key.Key1) + 1;
            SelectedSpellSlot = spellSlot;
        }
    }

    private bool TryProjectMouseToWorld([NotNullWhen(true)] out Vector3 worldPosition)
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePosition = GetViewport().GetMousePosition();

        var origin = Camera.ProjectRayOrigin(mousePosition);
        var end = origin + Camera.ProjectRayNormal(mousePosition) * MouseRaycastLength;
        var query = PhysicsRayQueryParameters3D.Create(origin, end);
        query.CollisionMask = 1 << 1;
        var result = spaceState.IntersectRay(query);

        if (result.Any()) {
            worldPosition = (Vector3)result["position"];
            return true;
        }

        worldPosition = Vector3.Zero;
        return false;
    }
}