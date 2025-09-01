namespace Sqlx.Core.Cache;

public class LruCacheTest
{

    [Fact]
    public void Get_Should_ReturnValueForKey_WhenPresentInCache()
    {
        var cache = new LruCache<string, int>(
            1,
            new Dictionary<string, int>
            {
                { "Test", 1 },
            });
        Assert.Equal(1, cache.Get("Test"));
    }

    [Fact]
    public void Capacity_Should_MatchTheSpecificInitializer()
    {
        var cache = new LruCache<string, int>(10);
        
        Assert.Equal(10, cache.Capacity);
    }

    [Fact]
    public void Count_Should_MatchTheCurrentNumberOfItems()
    {
        var cache = new LruCache<string, int>(10);
        
        Assert.Equal(0, cache.Count);

        cache.Put("Test", 1);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void Put_Should_ReturnNull_When_CapacityIsNotExceeded()
    {
        var cache = new LruCache<string, int>(10);
        
        var ejected = cache.Put("Test", 1);
        
        Assert.Null(ejected);
    }

    [Theory]
    [MemberData(nameof(PutCapacityExceeded))]
    public void Put_Should_ReturnOldestEntry_When_CapacityIsExceeded(
        Dictionary<string, int> initialEntries,
        Dictionary<string, int> otherEntries,
        (string, int) expectedEjected)
    {
        var cache = new LruCache<string, int>(initialEntries.Count, initialEntries);
        foreach (var entry in otherEntries)
        {
            cache.Put(entry.Key, entry.Value);
        }
        
        var ejected = cache.Put("TestKey", int.MaxValue);
        
        Assert.NotNull(ejected);
        Assert.Equal(expectedEjected, ejected);
    }

    public static IEnumerable<object[]> PutCapacityExceeded()
    {
        yield return [
            new Dictionary<string, int>
            {
                { "Test", 1 },
                { "Test2", 2 },
            },
            new Dictionary<string, int>(),
            ("Test", 1),
        ];
        yield return [
            new Dictionary<string, int>
            {
                { "Test", 1 },
                { "Test2", 2 },
            },
            new Dictionary<string, int>
            {
                { "Test3", 3 },
            },
            ("Test2", 2),
        ];
        yield return [
            new Dictionary<string, int>
            {
                { "Test", 1 },
                { "Test2", 2 },
            },
            new Dictionary<string, int>
            {
                { "Test3", 3 },
                { "Test", 10 },
            },
            ("Test3", 3),
        ];
    }
}
