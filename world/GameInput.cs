using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;

namespace wizardballz.world;

public partial class GameInput : Node3D
{
    public const float MouseRaycastLength = 1000;
    
    [Export] public Camera3D Camera = null!;
    [Export] public SpellManager SpellManager = null!;
    [Export] public SpellRecord DefaultSpell = null!;
    [Export] public PackedScene WorldCursorPrefab = null!;

    private Node3D WorldCursor = null!;

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
        if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent is { ButtonIndex: MouseButton.Left, Pressed: true }) {
            if (TryProjectMouseToWorld(out var worldPosition)) {
                SpellManager.BeginSpellCharge(0, worldPosition, DefaultSpell);
            }
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