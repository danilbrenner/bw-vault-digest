namespace Bw.VaultDigest.Common;

public static class Extensions
{
    public static IDictionary<TKey, TValue> FAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value)
    {
        dictionary.Add(key, value);
        return dictionary;
    }

    public static T ToEnum<T>(this int i) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException($"{typeof(T).Name} must be an enumerated type");
        if (!Enum.IsDefined(typeof(T), i))
            throw new ArgumentException($"The value {i} is an invalid enum value for the type {typeof(T).Name}");
        return (T)Enum.ToObject(typeof(T), i);
    }

    public static IReadOnlyList<T> Add<T>(this IReadOnlyList<T> list, T item)
    {
        var wList = list.ToList();
        wList.Add(item);
        return wList;
    }
}