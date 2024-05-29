using Godot;
using System;
using System.Net.Mime;
using Microsoft.Win32.SafeHandles;
using wizardballz.spells;
using wizardballz.world;

[Tool]
public partial class SpellSlot : VBoxContainer
{
    [Export]
    public string CostText
    {
        get => _costText;
        set
        {
            _costText = value;
            if (IsInsideTree()) GetNode<RichTextLabel>("%SpellCostLabel").Text = CostText;
        }
    }

    [Export]
    public string SlotText
    {
        get => _slotText;
        set
        {
            _slotText = value;
            if (IsInsideTree()) GetNode<RichTextLabel>("%SlotNumberLabel").Text = SlotText;
        }
    }

    [Export]
    public Texture2D Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            if (IsInsideTree()) GetNode<TextureRect>("%SpellIcon").Texture = Texture;
        }
    }

    private StyleBox? SelectedStylebox;
    private StyleBox? OriginalStylebox;
    private SpellRecord? SpellRecord;
	
    private string _costText = null!;
    private string _slotText = null!;
    private Texture2D _texture = null!;

    public override void _Ready()
    {
        GetNode<RichTextLabel>("%SpellCostLabel").Text = CostText;
        GetNode<RichTextLabel>("%SlotNumberLabel").Text = SlotText;
        GetNode<TextureRect>("%SpellIcon").Texture = _texture;
    }

    public void PopulateForSpell(SpellRecord? spell)
    {
        if (spell is null) {
            CostText = "0";
            Texture = null!;
        }
        else {
            CostText = spell.Cost.ToString();
            Texture = spell.Icon;
        }
        
        SpellRecord = spell;
    }

    public override GodotObject _MakeCustomTooltip(string forText)
    {
        if (SpellRecord is not null) {
            return SpellInfoCard.Instantiate(SpellRecord);
        }

        return null!;
    }

    public void SetCooldownTimer(float cooldownRatio)
    {
        ((ShaderMaterial)GetNode<TextureRect>("%SpellIcon").Material).SetShaderParameter("fill_ratio", cooldownRatio);
    }

    public void SetSelected(bool selected)
    {
        if (OriginalStylebox == null || SelectedStylebox == null) {
            SetupStyleboxes();
        }
        
        if (selected) {
            GetNode<Container>("IconContainer").AddThemeStyleboxOverride("panel", SelectedStylebox);
        }
        else {
            GetNode<Container>("IconContainer").AddThemeStyleboxOverride("panel", OriginalStylebox);
        }
    }

    private void SetupStyleboxes()
    {
        OriginalStylebox = (StyleBoxFlat)GetNode<Container>("IconContainer").GetThemeStylebox("panel");
        SelectedStylebox = (StyleBoxFlat)OriginalStylebox.Duplicate();
        ((StyleBoxFlat)SelectedStylebox).SetBorderWidthAll(4);
        ((StyleBoxFlat)SelectedStylebox).SetExpandMarginAll(4);
    }
}