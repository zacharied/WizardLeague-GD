using Godot;
using System;
using System.Linq;
using wizardballz;

public partial class PregameDeckEditScreen : MarginContainer
{
    public override void _Ready()
    {
        SetupItemList();
    }

    public void SetupItemList()
    {
        foreach (var spellRecord in WizardBallz.GetAllSpells()) {
            GetNode<ItemList>("%AllSpellsList").AddItem(spellRecord.Name, spellRecord.Icon);
        }
    }
}
