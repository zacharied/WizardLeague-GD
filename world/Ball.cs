using Godot;
using System;

public partial class Ball : RigidBody3D
{
    public Area3D Area => GetNode<Area3D>("Area3D");

    public override void _Ready()
    {
    }
}
