using System.Text.Json.Serialization;
using Godot;

namespace wizardballz.world;

/// <summary>
/// Lightweight wrapper for game player metadata.
/// </summary>
public partial class GamePlayerProfile : Resource
{
    [Export] public string Name;
    
    public long ClientId;

    public GamePlayerProfile()
    {
    }

    public GamePlayerProfile(long clientId, string name)
    {
        ClientId = clientId;
        Name = name;
    }
}