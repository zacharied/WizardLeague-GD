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
    private bool PlayerSetup = false;

    public override void _Ready()
    {
        CountdownLabel = GetNode<RichTextLabel>("%CountdownLabel");
    }

    public override void _Process(double delta)
    {
        if (!GameMatch.HasAllPlayers()) {
            return;
        } 
        
        if (!PlayerSetup) {
            SetupLocalPlayer();
        }

        if (GameMatch.CountdownTimer > 0) {
            CountdownLabel.Visible = true;
            CountdownLabel.Text = ((int)Math.Ceiling(GameMatch.CountdownTimer)).ToString();
        }
        else {
            CountdownLabel.Visible = false;
        }

        // Spell slot
        foreach (var (spellSlot, i) in GetNode("%SpellSlots").GetChildrenOfType<SpellSlot>().Select((s, i) => (s, i))) {
            spellSlot.SetSelected(false);

            if (GameMatch.GetLocalPlayer().SpellSlots.TryGetValue((GamePlayer.SpellSlot)(i + 1), out var spell)) {
                spellSlot.PopulateForSpell(spell);
            }
        }
        
        if (GameInput.SelectedSpellSlot != GamePlayer.SpellSlot.Primary) {
            GetNode("%SpellSlots").GetChild<SpellSlot>((int)GameInput.SelectedSpellSlot - 1).SetSelected(true);
        }

        var localPlayer = GameMatch.GetLocalPlayer();
        if (localPlayer.PrimarySpellCooldown > 0) {
            GetNode<SpellSlot>("%PrimarySpellSlot").SetCooldownTimer(
                (GamePlayer.PrimarySpellCooldownDuration - localPlayer.PrimarySpellCooldown) /
                GamePlayer.PrimarySpellCooldownDuration);
        }
    }

    public void SetupLocalPlayer()
    {
        GetNode<SpellSlot>("%PrimarySpellSlot").PopulateForSpell(GameMatch.GetLocalPlayer().SpellSlots[GamePlayer.SpellSlot.Primary]!);
        PlayerSetup = true;
    }
}