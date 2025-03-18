using Godot;

namespace wizardballz.ui.NetworkStartScreen;

public partial class NetworkLobbyPlayerCard : Control
{
    public NetworkLobbyPlayer Player;

    public override void _Ready()
    {
        GetNode<RichTextLabel>("%PlayerNameLabel").Text = Player.GamePlayerProfile.Name;
    }

    public override void _Process(double delta)
    {
        if (IsInstanceValid(Player)) {
            GetNode<RichTextLabel>("%PlayerStateLabel").Text = Player.State.ToString();
        }
        else {
            Visible = false;
        }
    }
}