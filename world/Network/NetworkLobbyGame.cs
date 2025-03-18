using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using wizardballz.ui.NetworkStartScreen;

namespace wizardballz.world.Network;

public partial class NetworkLobbyGame : Node
{
    [Signal]
    public delegate void JoinedLobbyAsClientEventHandler();

    [Signal]
    public delegate void ReceivedLobbyPlayerInfoAsClientEventHandler(long id);

    public MultiplayerSynchronizer MultiplayerSynchronizer => GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
    public IReadOnlyList<NetworkLobbyPlayer> Players => PlayersNode.GetChildrenOfType<NetworkLobbyPlayer>().ToList();
    private Node PlayersNode => GetNode("Players");

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

        if (args.Contains("--localhost")) {
        }

        // Set up shared client/server event handlers
        Multiplayer.PeerDisconnected += id =>
        {
            var peerNode = PlayersNode
                .GetChildrenOfType<NetworkLobbyPlayer>()
                .SingleOrDefault(p => p.GamePlayerProfile.ClientId == id);
            if (peerNode is not null) {
                MultiplayerSynchronizer.ReplicationConfig.RemoveProperty($"Players/Player_{id}:State");
                PlayersNode.RemoveChild(peerNode);
                peerNode.QueueFree();
            }
        };

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
        }

        void RunClientMode()
        {
            Multiplayer.ConnectedToServer += () =>
            {
                Rpc(MethodName.RpcGlobal_ClientConnected, Multiplayer.GetUniqueId(), playerName);
                EmitSignalJoinedLobbyAsClient();
            };

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

    public void ConnectToServer(string address = "127.0.0.1")
    {
        if (Multiplayer.MultiplayerPeer is not OfflineMultiplayerPeer && Multiplayer.IsServer()) {
            throw new InvalidOperationException("cannot connect as a server");
        }

        var peer = new ENetMultiplayerPeer();
        peer.CreateClient(address, 13237);
        Multiplayer.MultiplayerPeer = peer;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RpcGlobal_ClientConnected(long clientId, string playerName)
    {
        OnClientConnected(clientId, playerName);

        if (Multiplayer.IsServer()) {
            // Update current client with the lobby and send to other clients
            foreach (var other in Players.Where(p => p.GamePlayerProfile.ClientId != Multiplayer.GetRemoteSenderId())) {
                RpcId(clientId, MethodName.OnClientConnected, other.GamePlayerProfile.ClientId, other.GamePlayerProfile.Name);
                RpcId(other.GamePlayerProfile.ClientId, MethodName.OnClientConnected, clientId, playerName);
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void OnClientConnected(long clientId, string playerName)
    {
        var profile = new GamePlayerProfile(clientId, playerName);
        var player = new NetworkLobbyPlayer() { GamePlayerProfile = profile, Name = $"Player_{clientId}" };
        GetNode("Players").AddChild(player);
        MultiplayerSynchronizer.ReplicationConfig.AddProperty($"Players/Player_{clientId}:State");

        EmitSignalReceivedLobbyPlayerInfoAsClient(clientId);
    }
}