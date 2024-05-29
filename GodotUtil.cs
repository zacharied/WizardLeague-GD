using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using wizardballz.exceptions;

namespace wizardballz;

public static class GodotUtil
{
    public static IEnumerable<Node> RecurseChildren(this Node @this)
    {
        return @this.GetChildren().Concat(@this.GetChildren().SelectMany(c => c.RecurseChildren()));
    }

    public static T GetChildOfType<T>(this Node @this, bool includeSelf = false)
        where T : Node
    {
        return (includeSelf ? (@this is T ? (T)@this : null) : null)
               ?? (T)@this.GetChildren().Single(n => n is T);
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

    public static void ThrowIfNotPopulated(params GodotObject[] items)
    {
        foreach (var item in items)
            if (item == null)
                throw new ExportFieldUnsetException("a field is null");
    }

    public static void RemoveAndQueueFreeChildren(this Node @this)
    {
        foreach (var child in @this.GetChildren()) {
            @this.RemoveChild(child);
            child.QueueFree();
        }
    }
}

public static class CollisionLayers
{
    public const int Ball = 1;
    public const int Field = 2;
    public const int Wall = 3;
    public const int Projectile = 4;
    public const int KillProjectile = 5;
    public const int SpellWall = 6;
}

public static class GodotExceptions
{
}