using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace wizardballz;

public static class WizardBallz
{
    public const uint PlayerCount = 2;
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