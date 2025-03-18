using Godot;
using System;
using wizardballz;
using wizardballz.world;
using wizardballz.world.Network;

public partial class RootScene : Node
{
	[Export] private NetworkLobbyGame NetworkGame;
	
	public override void _Ready()
	{
		GodotUtil.ThrowIfNotPopulated(NetworkGame);
	}
}
