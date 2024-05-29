using Godot;
using System;

[Tool]
public partial class CardInfoItem : MarginContainer
{
    public static CardInfoItem Instantiate(string keyText, string valueText)
    {
        var instance = GD.Load<PackedScene>("res://ui/SpellInfoCard/CardInfoItem.tscn").Instantiate<CardInfoItem>();
        instance.KeyText = keyText;
        instance.ValueText = valueText;
        return instance;
    } 
    
    [Export] public string KeyText = null!;
    [Export] public string ValueText = null!;

    public override void _Ready()
    {
        GetNode<RichTextLabel>("%KeyLabel").Text = KeyText;
        GetNode<RichTextLabel>("%ValueLabel").Text = ValueText;
    }
}