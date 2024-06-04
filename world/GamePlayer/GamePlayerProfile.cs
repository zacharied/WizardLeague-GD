using Godot;

namespace wizardballz.world;

/// <summary>
/// Lightweight wrapper for game player metadata.
/// </summary>
public partial class GamePlayerProfile : Resource
{
    [Export] public string Name;
    
    public ulong ClientId;

    public GamePlayerProfile()
    {
    }

    public GamePlayerProfile(ulong clientId, string name)
    {
        ClientId = clientId;
        Name = name;
    }
}