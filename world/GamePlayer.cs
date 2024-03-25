using Godot;

namespace wizardballz.world;

public struct GamePlayer
{
    public uint PlayerNumber => PlayerIndex + 1;

    public ulong ClientId;
    public uint PlayerIndex;
    public uint Score;
}