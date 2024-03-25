using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace wizardballz;

public static class GodotUtil
{
    public static IEnumerable<Node> RecurseChildren(this Node @this)
    {
        return  @this.GetChildren().Concat(@this.GetChildren().SelectMany(c => c.RecurseChildren()));
    }

    public static T GetChildOfType<T>(this Node @this)
        where T : Node
    {
        return (T)@this.GetChildren().Single(n => n is T);
    }

    public static T? GetChildOfTypeOrNull<T>(this Node @this)
        where T : Node
    {
        return (T?)@this.GetChildren().SingleOrDefault(n => n is T);
    }

    public static IEnumerable<T> GetChildrenOfType<T>(this Node @this)
        where T : Node
    {
        return @this.GetChildren().OfType<T>();
    }
}