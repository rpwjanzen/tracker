namespace System.Collections.Generic;

public static class EnumerableEx
{
    public static IEnumerable<T> FromSingle<T>(T item)
    {
        yield return item;
    }
}