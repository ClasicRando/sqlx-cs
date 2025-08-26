namespace Sqlx.Core.Cache;

public class LruCache<TKey, TValue>(int capacity) where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<(TKey key, TValue value)>> _keyValuePairs = [];
    private readonly LinkedList<(TKey key, TValue value)> _linkedList = [];
    
    public int Capacity { get; } = capacity;

    public TValue? Get(TKey key)
    {
        if (!_keyValuePairs.TryGetValue(key, out var node)) return default;
        _linkedList.Remove(node);
        _linkedList.AddLast(node);
        return node.Value.value;
    }

    public (TKey, TValue)? Put(TKey key, TValue value)
    {
        if (!_keyValuePairs.TryGetValue(key, out var node))
        {
            (TKey, TValue)? result = null;
            if (_keyValuePairs.Count == Capacity && _linkedList.First is not null)
            {
                _keyValuePairs.Remove(_linkedList.First.Value.key);
                result = _linkedList.First.Value;
                _linkedList.RemoveFirst();
            }

            _keyValuePairs[key] = _linkedList.AddLast((key, value));
            return result;
        }

        _linkedList.Remove(node);
        node.Value = (key, value);
        _linkedList.AddLast(node);
        return null;
    }
}
