using Godot;
using System;
using System.Collections.Generic;

public partial class DelayDrawer : Node
{
    public const float DelaySec = 0.52f;
    
    [Export] private Node3D PhysicalBall = null!;
    [Export] private PackedScene BallScene = null!;

    private Dictionary<Node3D, (List<Vector3>, Node3D)> PositionHistories = new();
    private List<ZombieVisual> ZombieVisuals = new();

    public override void _Ready()
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var (node, (list, visualNode)) in PositionHistories) {
            list.Add(node.GlobalPosition);
        }
    }

    public override void _Process(double delta)
    {
        foreach (var node in PositionHistories.Keys) {
            DrawVisual(node);
        }
        
        DrawZombieVisuals(delta);
    }

    private void DrawZombieVisuals(double delta)
    {
        var removals = new List<ZombieVisual>();
        var rate = (float)1 / Engine.PhysicsTicksPerSecond;
        foreach (var zombie in ZombieVisuals) {
            var displayIndex = zombie.PositionHistory.Count * rate - DelaySec + zombie.Lifetime;
            var i = (int)Math.Floor(displayIndex * Engine.PhysicsTicksPerSecond);
            
            if (i >= zombie.PositionHistory.Count) {
                removals.Add(zombie);
                zombie.VisualNode.GetParent().RemoveChild(zombie.VisualNode);
                zombie.VisualNode.QueueFree();
            }
            else if (i >= 0) {
                zombie.VisualNode.Visible = true;
                zombie.VisualNode.GlobalPosition = zombie.PositionHistory[i];
            }
            zombie.Lifetime += (float)delta;
        }
        
        removals.ForEach(tup => ZombieVisuals.Remove(tup));
    }

    private void DrawVisual(Node3D obj)
    {
        if (!PositionHistories.ContainsKey(obj)) {
            GD.PushWarning($"Cannot draw visual for {obj} because it has no entry in the history table");
            return;
        }

        var rate = (float)1 / Engine.PhysicsTicksPerSecond;
        var (history, visualNode) = PositionHistories[obj];
        
        var displayIndex = history.Count * rate - DelaySec;
        if (displayIndex >= 0) {
            visualNode.Visible = true;
            var i = (int)Math.Floor(displayIndex * Engine.PhysicsTicksPerSecond);
            visualNode.GlobalPosition = history[i];
        }
        else {
            visualNode.Visible = false;
        }
    }

    public void RegisterNode(Node3D node, Node3D visualTempHolder)
    {
        if (node.GetNodeOrNull<Node3D>("Visual") is { } visualChild) {
            PositionHistories[node] = (new(), visualChild);
            visualChild.Visible = false;
        }
        else {
            GD.PushWarning($"Unable to add node {node.Name} because it lacks a Visual child");
        }
    }

    public void MarkForDeletion(Node3D node, Node3D visualOrphanParent)
    {
        var (history, visualNode) = PositionHistories[node];
        PositionHistories.Remove(node);
        
        ZombieVisuals.Add(new ZombieVisual() { PositionHistory = history, VisualNode = visualNode });
        node.RemoveChild(visualNode);
        visualOrphanParent.AddChild(visualNode);
        visualNode.Owner = visualOrphanParent;
    }

    private class ZombieVisual
    {
        public Node3D VisualNode;
        public List<Vector3> PositionHistory;
        public float Lifetime;
    }
}