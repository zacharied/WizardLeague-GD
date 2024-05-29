using Godot;

namespace wizardballz.spells;

[GlobalClass]
public partial class SpellRecord : Resource
{
    [Export] public Texture2D Icon;
    [Export] public string Name;
    [Export] public uint Cost;
    [Export] public float ChargeDuration;
    [Export] public TargetType Target;
    [Export] public SpellCastEffect Effect;
    
    public enum TargetType
    {
        /// <summary>
        /// A player-targeted position on the field
        /// </summary>
        FieldPosition,
        
        /// <summary>
        /// The current position of the ball
        /// </summary>
        Ball,
        
        /// <summary>
        /// A fixed location on the field
        /// </summary>
        Fixed
    }
}