using System;
using System.ComponentModel.Design;
using System.Linq;
using Godot;
using wizardballz.spells;
using wizardballz.spells.SpellDeck;
using wizardballz.world.Network;

namespace wizardballz.world;

public partial class NetworkGame : Node
{
    [Export] private GameMatch GameMatch = null!;
    [Export] private MultiplayerSynchronizer Synchronizer = null!;
    [Export] private NetworkSpellManager NetworkSpellManager = null;
    [Export] private SpellManager SpellManager = null!;
    
    [Export] private SpellRecord DefaultPrimarySpell = null!;
    [Export] private SpellDeck DefaultSpellDeck = null!;

    public override void _Ready()
    {
        var peer = new ENetMultiplayerPeer();
        var args = OS.GetCmdlineArgs().ToList();

        string playerName;
        if (args.Contains("--name")) {
            var idx = args.IndexOf("--name") + 1;
            if (idx >= args.Count || args[idx].StartsWith("-")) {
                throw new InvalidOperationException("no name provided");
            }

            playerName = args[idx];
        }
        else {
            playerName = $"Player {GD.RandRange(0, 9999):0000}";
        }
        
        if (DisplayServer.GetName() == "headless") {
            RunServerMode();
        }
        else {
            RunClientMode();
        }

        Multiplayer.MultiplayerPeer = peer;
        
        return;

        void RunServerMode()
        {
            peer.CreateServer(13237);
            GD.Print("Running in server mode.");

            DisableNodesWithGroup("client_side");

            Multiplayer.PeerConnected += id =>
            {
                if (id == 1)
                    return;
                GD.Print($"Client {id} connected.");
            };
        

            GameMatch.Ball.FreezeMode = RigidBody3D.FreezeModeEnum.Static;
            GameMatch.Ball.Freeze = true;
        }

        void RunClientMode()
        {
            Multiplayer.ConnectedToServer += () => { RpcId(1, MethodName.ServerSetClientPlayerInfo, playerName); };

            var err = peer.CreateClient("71.178.235.20", 13237);
            if (err != Error.Ok) {
                GD.PrintErr($"Unable to connect (error code {err})");
            }

            DisableNodesWithGroup("server_side");

            foreach (var node in GetTree().GetNodesInGroup("client_side_no_process_physics")) {
                if (node is RigidBody3D rigidBody) {
                    rigidBody.FreezeMode = RigidBody3D.FreezeModeEnum.Static;
                    rigidBody.Freeze = true;
                    rigidBody.CollisionLayer = 0;
                    rigidBody.CollisionMask = 0;
                }
                else {
                    GD.PushWarning($"cannot freeze node of type {node.GetType()}");
                }
            }
        }

        void DisableNodesWithGroup(StringName groupName)
        {
            GetTree().NodeAdded += (node) =>
            {
                if (node.IsInGroup(groupName)) {
                    node.GetParent().RemoveChild(node);
                    node.QueueFree();
                }
            };

            foreach (var node in GetTree().GetNodesInGroup(groupName)) {
                node.GetParent().CallDeferred(Node.MethodName.RemoveChild, node);
                node.QueueFree();
            }
        }
    }

    private void TellClientsPlayerData()
    {
        foreach (var player in GameMatch.Players) {
            Rpc(MethodName.ClientAddPlayer, player.ClientId, player.PlayerName, new string[] { });
        }
    }

    /// <summary>
    /// RPC method. Called by the server to copy the player data over to the clients.
    /// </summary>
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = false)]
    private void ClientAddPlayer(ulong clientId, string playerName, string[] deckSpellPaths)
    {
        if (Multiplayer.IsServer())
            return;
        
        if (GameMatch.Players.Count() >= WizardBallz.PlayerCount) {
            throw new InvalidOperationException("already at max players");
        }

        GameMatch.AddChild(CreatePlayer(clientId, playerName));

        if (clientId == (ulong)Multiplayer.GetUniqueId()) {
            // This is the local player
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ServerSetClientPlayerInfo(string playerName)
    {
        if (!Multiplayer.IsServer())
            return;
        
        if (GameMatch.Players.Count() >= WizardBallz.PlayerCount) {
            throw new InvalidOperationException("already at max players");
        }
        
        GD.Print($"Adding player [{playerName}]");
        
        // On server
        GameMatch.AddChild(CreatePlayer((uint)Multiplayer.GetRemoteSenderId(), playerName));

        if (GameMatch.Players.Count() == WizardBallz.PlayerCount) {
            // All players have joined the game, share player data with clients
            TellClientsPlayerData();
            GameMatch.SetState(GameMatch.MatchPlayState.Countdown);
        }
    }

    private Node3D GetSpellCircle()
    {
        return GameMatch.Field.SpellCastSpawns[GameMatch.Players.Count()];
    }

    private GamePlayer CreatePlayer(ulong clientId, string playerName)
    {
        var indicatorColor = clientId == (ulong)Multiplayer.GetUniqueId() ? Colors.White : Colors.Red;
        return new GamePlayer(clientId, playerName, (uint)GameMatch.Players.Count(), DefaultPrimarySpell, DefaultSpellDeck, GetSpellCircle(),
            GameMatch.Field.TurretPositions[GameMatch.Players.Count()], NetworkSpellManager, indicatorColor
            );
    }
}