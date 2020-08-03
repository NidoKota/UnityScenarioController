using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public static class Utility
{
    public static bool Within<T>(this IEnumerable<T> i, int index)
    {
        return index >= 0 && i.Count() > index;
    }
}
