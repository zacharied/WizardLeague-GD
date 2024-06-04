using Godot;
using System;
using System.Linq;
using wizardballz;
using wizardballz.world;

public partial class NetworkHud : MarginContainer
{
    [Export] private NetworkGame NetworkGame = null!;
    
    private NetworkScreenState State = NetworkScreenState.Init;
    
    private LiteTabContainer ContentTabs = null!;
	
    public override void _Ready()
    {
        ContentTabs = GetNode<LiteTabContainer>("%ContentTabs");
        GetNode<Button>("%ConnectToServerButton").Pressed += () =>
        {
            State = NetworkScreenState.Connecting;
            GetNode<RichTextLabel>("%Status").Text = "Connecting...";
            ContentTabs.SelectChild("Status");
			
            var address = "127.0.0.1";
            NetworkGame.ConnectToServer(address);
        };

        NetworkGame.Multiplayer.ConnectedToServer += UpdateByPlayerCount;

        NetworkGame.Multiplayer.ConnectionFailed += () =>
        {
            State = NetworkScreenState.Init;
            ContentTabs.SelectChild("ConnectPrompt");
        };

        NetworkGame.Multiplayer.PeerConnected += id =>
        {
            if (id == 1)
                return;

            UpdateByPlayerCount();
        };

        NetworkGame.Multiplayer.PeerDisconnected += _ =>
        {
            State = NetworkScreenState.Init;
            GetNode<RichTextLabel>("%Status").Text = "Connecting...";
            ContentTabs.SelectChild("ConnectPrompt");
        };

        NetworkGame.PlayerAdded += (_, _) =>
        {
            GD.Print($"PlayerAdded: {NetworkGame.GetPlayers().Count} players");
            UpdateByPlayerCount();
        };
        
        NetworkGame.ReceivedOtherPlayerInfo += () =>
        {
            GD.Print($"Received other player(s) info");      
            UpdateByPlayerCount();
        };

        GetNode<Button>("%ReadyUpButton").Pressed += () =>
        {
            NetworkGame.TellServerClientReady();
            GetNode<RichTextLabel>("%ReadyUpStatus").Text = "You have readied up.";
        };
    }
    
    private void UpdateByPlayerCount()
    {
        GD.Print($"{NetworkGame.GetPlayers().Count} players");
        if (NetworkGame.GetPlayers().Count == WizardBallz.PlayerCount) {
            State = NetworkScreenState.OpponentConnected;
            ContentTabs.SelectChild("ReadyUpPrompt"); 
        }
        else {
            State = NetworkScreenState.WaitingForOpponentConnection;
            ContentTabs.SelectChild("Status");
            GetNode<RichTextLabel>("%Status").Text = $"Waiting for players ({NetworkGame.GetPlayers().Count} / {WizardBallz.PlayerCount})";
        }
    }

    private enum NetworkScreenState
    {
        Init,
        Connecting,
        WaitingForOpponentConnection,
        OpponentConnected,
        WaitingForOpponentReady
    }
}