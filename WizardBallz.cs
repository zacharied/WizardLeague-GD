using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using wizardballz.spells;

namespace wizardballz;

public static class WizardBallz
{
    public const uint PlayerCount = 2;

    public static IEnumerable<SpellRecord> GetAllSpells()
    {
        foreach (var directory in DirAccess.Open("res://data/spells").GetDirectories()) {
            var spellPath = $"{directory}/{directory.Split("/").Last()}.tres";
            if (FileAccess.FileExists(spellPath)) {
                yield return GD.Load<SpellRecord>(spellPath);
            }
        }
    }
}

public static class Util
{
    public static bool TrySingle<T>(this IEnumerable<T> @this, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
    {
        var result = @this.SingleOrDefault(predicate);
        if (result == null) {
            value = default;
            return false;
        }

        value = result;
        return true;
    }
    
    public static bool TrySingle<T>(this IEnumerable<T> @this, [MaybeNullWhen(false)] out T value)
    {
        var result = @this.SingleOrDefault();
        if (result == null) {
            value = default;
            return false;
        }

        value = result;
        return true;
    }
}