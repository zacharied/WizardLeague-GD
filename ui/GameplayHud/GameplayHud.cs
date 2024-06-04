using Godot;
using System;
using System.Linq;
using wizardballz;
using wizardballz.world;

public partial class GameplayHud : Control
{
    [Export] public GameMatch GameMatch = null!;
    [Export] public GameInput GameInput = null!;
    
    private RichTextLabel CountdownLabel = null!;

    public override void _Ready()
    {
        CountdownLabel = GetNode<RichTextLabel>("%CountdownLabel");
        
        GameInput.SpellSelected += (_, uSpellSlot) =>
        {
            var spellSlot = (GamePlayer.SpellSlot)uSpellSlot;
            RefreshSpellSlots(spellSlot);
        };
    }

    public override void _Process(double delta)
    {
        if (GameMatch.CountdownTimer > 0) {
            CountdownLabel.Visible = true;
            CountdownLabel.Text = ((int)Math.Ceiling(GameMatch.CountdownTimer)).ToString();
        }
        else {
            CountdownLabel.Visible = false;
        }

        var localPlayer = GameMatch.GetLocalPlayer();
        if (localPlayer.PrimarySpellCooldown > 0) {
            GetNode<SpellSlot>("%PrimarySpellSlot").SetCooldownTimer(
                (GamePlayer.PrimarySpellCooldownDuration - localPlayer.PrimarySpellCooldown) /
                GamePlayer.PrimarySpellCooldownDuration);
        }
    }

    private void RefreshPrimarySpell()
    {
    }
    
    private void RefreshSpellSlots(GamePlayer.SpellSlot? selectedSlot = null)
    {
        // Spell slot
        foreach (var (spellSlot, i) in GetNode("%SpellSlots").GetChildrenOfType<SpellSlot>().Select((s, i) => (s, i))) {
            spellSlot.SetSelected(false);

            if (GameMatch.GetLocalPlayer().SpellSlots.TryGetValue((GamePlayer.SpellSlot)(i + 1), out var spell)) {
                spellSlot.PopulateForSpell(spell);
            }

            if (selectedSlot.HasValue && (uint)selectedSlot == i + 1) {
                spellSlot.SetSelected(true);
            }
        }
    }

    private void SetupLocalPlayer()
    {
        GetNode<SpellSlot>("%PrimarySpellSlot").PopulateForSpell(GameMatch.GetLocalPlayer().SpellSlots[GamePlayer.SpellSlot.Primary]!);
    }
}