using Godot;
using wizardballz.spells;

namespace wizardballz.world;

[GlobalClass]
public partial class SpellRecord : Resource
{
    [Export] public string Name = string.Empty;
    [Export] public uint Cost = 0;
    [Export] public float ChargeDuration = 0.5f;
    [Export] public SpellCastEffect Effect = null!;
}