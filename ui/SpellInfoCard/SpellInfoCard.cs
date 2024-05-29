using Godot;
using System;
using wizardballz;
using wizardballz.spells;
using wizardballz.world;

public partial class SpellInfoCard : Control
{
    public static SpellInfoCard Instantiate(SpellRecord record)
    {
        var instance = GD.Load<PackedScene>("res://ui/SpellInfoCard/SpellInfoCard.tscn").Instantiate<SpellInfoCard>();
        instance.Ready += () =>
        {
            instance.PopulateForSpell(record);
        };
        return instance;
    }
    
    public override void _Ready()
    {
        GetNode("%InfoItemLines").RemoveAndQueueFreeChildren();
    }

    private void PopulateForSpell(SpellRecord spellRecord)
    {
        GetNode<RichTextLabel>("%SpellNameLabel").Text = spellRecord.Name;
        GetNode<RichTextLabel>("%CostNumberLabel").Text = CreateCostNumberText(spellRecord.Cost);
        AddInfoItem("Charge Time", $"{spellRecord.ChargeDuration:F2}s");
        AddInfoItem("Target", $"{spellRecord.Target.ToString()}");
        AddInfoItem("Spell Type", GetSpellTypeLabel(spellRecord.Effect));
    }

    private void AddInfoItem(string key, string value)
    {
        var item = CardInfoItem.Instantiate(key, value);
        GetNode("%InfoItemLines").AddChild(item);
    }

    private static string GetSpellTypeLabel(SpellCastEffect effect)
    {
        return effect switch
        {
            SpawnPrefabSpellCastEffect => "Conjure object",
            SpawnProjectileSpellCastEfect => "Projectile"
        };
    }
    
    private static string CreateCostNumberText(uint number)
    {
        return number is > 10 or <= 0 
            ? string.Empty 
            : ((char)((uint)'❶' + number - 1)).ToString();
    }
}
