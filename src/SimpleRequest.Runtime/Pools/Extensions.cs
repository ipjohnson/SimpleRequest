namespace SimpleRequest.Runtime.Pools;

public static class Extensions {


    public static TValue GetOrDefault<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue defaultValue = default(TValue)!) {

        if (dictionary.TryGetValue(key, out var value)) {
            return value;
        }
        
        return defaultValue;
    }
}