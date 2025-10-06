
namespace Bw.Sync.BwCliClient;

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
}