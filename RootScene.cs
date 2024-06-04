using Godot;
using System;
using wizardballz;
using wizardballz.world;

public partial class RootScene : Node
{
	[Export] private NetworkGame NetworkGame;
	
	public override void _Ready()
	{
		GodotUtil.ThrowIfNotPopulated(NetworkGame);

		NetworkGame.PlayerReadied += (id) =>
		{
			if (!NetworkGame.AllPlayersReady) {
				return;
			}

			// Move on to next scene.
			var canvas = this.GetChildOfType<CanvasLayer>();
			var oldScene = canvas.GetChildOfType<Control>();
			canvas.RemoveChild(oldScene);
			oldScene.QueueFree();
			canvas.AddChild(GD.Load<PackedScene>("res://ui/PregameDeckEditScreen/PregameDeckEditScreen.tscn").Instantiate());
		};
	}
}
