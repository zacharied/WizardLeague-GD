using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using wizardballz;
using wizardballz.ui.NetworkStartScreen;

public partial class NetworkLobbyPlayerList : Control
{
    public void AddPlayerCard(NetworkLobbyPlayer player)
    {
        var playerCard = GD.Load<PackedScene>("res://ui/NetworkStartScreen/NetworkLobbyPlayerCard.tscn")
            .Instantiate<NetworkLobbyPlayerCard>();
        playerCard.Player = player;
        PlayerCardsNode.AddChild(playerCard);
    }

    public override void _Process(double delta)
    {
        foreach (var playerCard in PlayerCardsNode.GetChildrenOfType<NetworkLobbyPlayerCard>()) {
            if (!IsInstanceValid(playerCard.Player)) {
                PlayerCardsNode.RemoveChild(playerCard);
                playerCard.QueueFree();
            }
        }
    }

    private Node PlayerCardsNode => GetNode("%PlayerCards");
}