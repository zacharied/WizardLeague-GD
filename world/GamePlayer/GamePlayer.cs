using System.Linq;
using Godot;
using Godot.Collections;
using wizardballz.spells;
using wizardballz.spells.SpellDeck;
using wizardballz.world.Network;

namespace wizardballz.world;

public partial class GamePlayer : Node
{
    private const float ManaTickDuration = 3f;
    public const float PrimarySpellCooldownDuration = 3f;
    public const float SpellRestockCountdownDuration = 0.2f;

    [Signal]
    public delegate void ManaTickedEventHandler(uint newMana);
    
    public ulong ClientId;
    public string PlayerName;
    
    public uint Score = 0;
    
    public uint Mana { get; private set; }
    public float ManaTickTimer { get; private set; }

    public float PrimarySpellCooldown { get; private set; } = 0;
    public float SpellRestockCountdown { get; private set; } = 0;

    private NetworkSpellManager SpellManager;
    public Node3D SpellCircle;
    public Vector3 SpawnPointTurretPosition;

    public GameDeck Deck;
    public Dictionary<SpellSlot, SpellRecord?> SpellSlots = new();
    public Color IndicatorColor;
    public uint PlayerIndex;

    public GamePlayer(ulong clientId, string playerName, uint playerIndex, SpellRecord primarySpell, SpellDeck deck, Node3D spellCircle, Vector3 turretPosition,NetworkSpellManager spellManager, Color indicatorColor)
    {
        ClientId = clientId;
        PlayerName = playerName;
        PlayerIndex = playerIndex;
        SpellSlots[SpellSlot.Primary] = primarySpell;
        SpellCircle = spellCircle;
        SpellManager = spellManager;
        IndicatorColor = indicatorColor;
        SpawnPointTurretPosition = turretPosition;
        
        SpellRestockCountdown = SpellRestockCountdownDuration;

        Deck = new GameDeck(deck);
        foreach (var slotIdx in new[] { SpellSlot.Slot1, SpellSlot.Slot2, SpellSlot.Slot3 }) {
            SpellSlots[slotIdx] = null;
        }
    }

    public override void _Ready()
    {
        AddChild(Deck);
    }

    public override void _Process(double delta)
    {
        if (PrimarySpellCooldown > 0) {
            PrimarySpellCooldown -= (float)delta;
        }
        
        if (SpellRestockCountdown > 0 && SpellSlots.Values.Any(v => v is null)) {
            SpellRestockCountdown -= (float)delta;
        }
        else {
            foreach (var (slot, spellRecord) in SpellSlots) {
                if (spellRecord is null) {
                    if (Deck.HasSpell()) {
                        SpellSlots[slot] = Deck.TakeSpell();
                        SpellRestockCountdown = SpellRestockCountdownDuration;
                    }
                    break;
                }
            }
        }
    }

    public void CastSpell(Vector3 position, SpellSlot slot)
    {
        if (slot is SpellSlot.Primary && PrimarySpellCooldown > 0) return;

        if (SpellSlots[slot] == null) {
            GD.PushWarning($"slot {slot} is empty!");
            return;
        }

        SpellManager.TellSpellCast(SpellSlots[slot]!, position);

        if (slot is not SpellSlot.Primary) {
            SpellSlots[slot] = null;
        }
        else {
            PrimarySpellCooldown = PrimarySpellCooldownDuration;
        }
    }

    public enum SpellSlot
    {
        Primary = 0,
        Slot1,
        Slot2,
        Slot3
    }
}