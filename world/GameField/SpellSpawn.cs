using Godot;
using System;

public partial class SpellSpawn : Marker3D
{
	private Sprite3D Sprite;
	
	public override void _Ready()
	{
		Sprite = GetNode<Sprite3D>("Sprite3D");
	}

	public override void _Process(double delta)
	{
		Sprite.RotationDegrees += new Vector3(0, 90 * (float)delta, 0);
	}
}
