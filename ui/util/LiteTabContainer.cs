using Godot;
using System;
using System.Linq;
using wizardballz;

[GlobalClass]
public partial class LiteTabContainer : MarginContainer
{
    public override void _Ready()
    {
        if (this.GetChildrenOfType<Control>().Count(c => c.Visible) > 1) {
            GD.PushWarning("more than one visible child"); 
            SelectChild(0);
        }
    }

    public void SelectChild(int childIndex)
    {
        AssertHasChildren();

        bool matched = false;
        foreach (var (child, i) in this.GetChildrenOfType<Control>().Select((c, i) => (c, i))) {
            if (childIndex == i) {
                child.Visible = true;
                matched = true;
            }
            else {
                child.Visible = false;
            }
        }
        
        if (!matched) {
            GD.PushWarning($"failed to find child node at index {childIndex}"); 
        }
    }

    public void SelectChild(string nodeName)
    {
        AssertHasChildren();
        
        bool matched = false;
        foreach (var child in this.GetChildrenOfType<Control>()) {
            if (child.Name == nodeName) {
                child.Visible = true;
                matched = true;
            }
            else {
                child.Visible = false;
            }
        }

        if (!matched) {
            GD.PushWarning($"failed to find child node \"{nodeName}\""); 
        }
    }

    private void AssertHasChildren()
    {
        if (!this.GetChildrenOfType<Control>().Any()) {
            GD.PushError("there are no children; cannot select node");
            throw new Exception();
        }
    }
}