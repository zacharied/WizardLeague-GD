using Godot;
using System;
using System.Linq;
using wizardballz;
using wizardballz.ui.NetworkStartScreen;
using wizardballz.world;
using wizardballz.world.Network;

public partial class NetworkHud : MarginContainer
{
    [Export] private NetworkLobbyGame NetworkGame = null!;
    
    private LiteTabContainer ContentTabs = null!;
	
    public override void _Ready()
    {
        ContentTabs = GetNode<LiteTabContainer>("%ContentTabs");

        GetNode<Button>("%ConnectToServerButton").Pressed += () =>
        {
            NetworkGame.ConnectToServer();
        };

        NetworkGame.JoinedLobbyAsClient += () =>
        {
            ContentTabs.SelectChild("Lobby");
        };

        NetworkGame.ReceivedLobbyPlayerInfoAsClient += id =>
        {
            var player = NetworkGame.Players.Single(p => p.GamePlayerProfile.ClientId == id);
            GetNode<NetworkLobbyPlayerList>("%Lobby/PlayerList").AddPlayerCard(player);
        };

        /*
        NetworkGame.Multiplayer.ConnectedToServer += UpdateState;
        NetworkGame.Multiplayer.ConnectionFailed += UpdateState;
        NetworkGame.Multiplayer.PeerConnected += id =>
        {
            if (id == 1)
                return;
            UpdateState();
        };

        NetworkGame.Multiplayer.PeerDisconnected += _ => UpdateState();
        */
    }
}