namespace Sqlx.Core.Cache;

/// <summary>
/// Simple LRU (Last Recently Used) cache for key value pairs. When adding a new entry to the cache,
/// the last recently used element is removed to add the new item if the capacity of the cache would
/// otherwise be exceeded.
/// </summary>
/// <typeparam name="TKey">key type of the cache entries</typeparam>
/// <typeparam name="TValue">value type of the cache entries</typeparam>
public class LruCache<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<(TKey key, TValue value)>> _keyValuePairs = [];
    private readonly LinkedList<(TKey key, TValue value)> _linkedList = [];

    /// <summary>
    /// Create an empty cache with a set capacity
    /// </summary>
    /// <param name="capacity">
    /// max capacity of the cache before items are ejected to add new items
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">if capacity &lt;=0</exception>
    public LruCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        Capacity = capacity;
    }

    /// <summary>
    /// Create a cache with initial entries and a max capacity. If the number of initial entries
    /// exceeds the capacity, only the latest entry in the dictionary will be kept (using the
    /// default iteration ordering for the dictionary).
    /// </summary>
    /// <param name="capacity">
    /// max capacity of the cache before items are ejected to add new items
    /// </param>
    /// <param name="initialEntries">initial entries in the cache</param>
    /// <exception cref="ArgumentOutOfRangeException">if capacity &lt;=0</exception>
    public LruCache(int capacity, Dictionary<TKey, TValue> initialEntries) : this(capacity)
    {
        foreach (var entry in initialEntries)
        {
            Put(entry.Key, entry.Value);
        }
    }
    
    /// <summary>
    /// Max capacity of the cache. This does not reflect how many items are currently in the cache.
    /// For that use <see cref="Count"/>.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Total number of items in the cache
    /// </summary>
    public int Count => _linkedList.Count;

    /// <summary>
    /// Get the current item associated with this key (if any)
    /// </summary>
    /// <param name="key">key to lookup in the cache</param>
    /// <returns>
    /// The value associated with this key or the default value of <typeparamref name="TValue"/> if
    /// the key is not found in the cache.
    /// </returns>
    public TValue? Get(TKey key)
    {
        if (!_keyValuePairs.TryGetValue(key, out var node)) return default;
        _linkedList.Remove(node);
        _linkedList.AddLast(node);
        return node.Value.value;
    }

    /// <summary>
    /// Add the key value pair into the cache, removing the last recently used entry if the cache
    /// is currently full.
    /// </summary>
    /// <param name="key">key to add to the cache</param>
    /// <param name="value">value associated with the key</param>
    /// <returns>
    /// ejected entry from the cache, or null if the cache capacity has not been exceeded
    /// </returns>
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
