using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using wizardballz.spells;
using wizardballz.spells.SpellDeck;
using wizardballz.world.Network;

namespace wizardballz.world;

/// <summary>
/// Persistent object that stores network players. Serves as communication point between client and server.
/// </summary>
public partial class NetworkGame : Node
{
    [Signal]
    public delegate void PlayerReadiedEventHandler(ulong id);

    [Signal]
    public delegate void PlayerAddedEventHandler(ulong id, string name);

    [Signal]
    public delegate void ReceivedOtherPlayerInfoEventHandler();

    [Export] private SpellRecord DefaultPrimarySpell = null!;
    [Export] private SpellDeck DefaultSpellDeck = null!;

    private List<GamePlayerProfile> Players = new();
    private List<ulong> ReadyPlayers = new();

    public IReadOnlyList<GamePlayerProfile> GetPlayers() => Players.AsReadOnly();
    public bool AllPlayersReady => Players.Count >= WizardBallz.PlayerCount 
                                   && Players.All(p => ReadyPlayers.Contains(p.ClientId));
    
    public override void _Ready()
    {
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

        return;

        void RunServerMode()
        {
            var peer = new ENetMultiplayerPeer();
            peer.CreateServer(13237);
            Multiplayer.MultiplayerPeer = peer;

            GD.Print("Running in server mode.");

            this.DisableNodesWithGroup("client_side");

            Multiplayer.PeerConnected += id =>
            {
                if (id == 1)
                    return;
                
                GD.Print($"Client {id} connected.");
                
                // Add existing players to the new client
                foreach (var player in Players) {
                    RpcId(id, MethodName.ClientAddPlayer, player.ClientId, player.Name, ReadyPlayers.Contains(player.ClientId));
                }
                
                // Tell existing players about the new client
                foreach (var player in Players) {
                    RpcId((uint)player.ClientId, MethodName.ClientAddPlayer, id, playerName, false);
                }
            };
        }

        void RunClientMode()
        {
            Multiplayer.ConnectedToServer += () => { Rpc(MethodName.ServerSetClientPlayerInfo, playerName); };

            this.DisableNodesWithGroup("server_side");

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
    }

    /// <summary>
    /// Called by the server to copy existing player data to a new client.
    /// </summary>
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = false)]
    private void ClientAddPlayer(ulong clientId, string playerName, bool isReady)
    {
        AssertIsClient();
        AssertLobbyNotFull();
        
        Players.Add(new GamePlayerProfile(clientId, playerName));
        if (isReady)
            ReadyPlayers.Add(clientId);

        EmitSignal(SignalName.ReceivedOtherPlayerInfo);
    }

    /// <summary>
    /// Called by a client with name <b>playerName</b> to inform the server of its player information. Also called
    /// locally, to add a new GamePlayer node.
    /// </summary>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ServerSetClientPlayerInfo(string playerName)
    {
        AssertLobbyNotFull();
        GD.Print(nameof(ServerSetClientPlayerInfo));

        var clientId = (uint)Multiplayer.GetRemoteSenderId();
        var gamePlayerProfile = new GamePlayerProfile(clientId, playerName);
        Players.Add(gamePlayerProfile);
        EmitSignal(SignalName.PlayerAdded, clientId, playerName);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ServerSetClientReady()
    {
        ReadyPlayers.Add((ulong)Multiplayer.GetRemoteSenderId());

        EmitSignal(SignalName.PlayerReadied, (ulong)Multiplayer.GetRemoteSenderId());

        if (Multiplayer.IsServer() && Players.All(p => ReadyPlayers.Contains(p.ClientId))) {
//            GD.Print("All players have readied up!");
        }
    }

    public void ConnectToServer(string address)
    {
//        AssertIsClient();

        var peer = new ENetMultiplayerPeer();
        peer.CreateClient(address, 13237);
        Multiplayer.MultiplayerPeer = peer;
    }

    public void TellServerClientReady()
    {
        AssertIsClient();
        Rpc(MethodName.ServerSetClientReady);
    }
    
    #region Assertions

    private void AssertIsServer()
    {
        if (!Multiplayer.IsServer()) {
            throw new InvalidOperationException($"function must be called on a server");
        }
    }

    private void AssertIsClient()
    {
        if (Multiplayer.IsServer()) {
            throw new InvalidOperationException($"function must be called on a client");
        }
    }

    private void AssertLobbyNotFull()
    {
        if (Players.Count() >= WizardBallz.PlayerCount) {
            throw new InvalidOperationException("too many players");
        }
    }
    
    #endregion
}