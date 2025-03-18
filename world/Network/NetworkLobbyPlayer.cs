using Godot;
using wizardballz.world;

namespace wizardballz.ui.NetworkStartScreen;

public partial class NetworkLobbyPlayer : Node
{
    [Export] public GamePlayerProfile GamePlayerProfile = null!;
    [Export] public ReadyState State = ReadyState.Unready;

    public enum ReadyState
    {
        Unready,
        EditDeck,
        Ready
    }
}