namespace Sqlx.Core.Cache;

public class LruCacheTest
{
    [Test]
    public async Task Get_Should_ReturnValueForKey_WhenPresentInCache()
    {
        var cache = new LruCache<string, int>(
            1,
            new Dictionary<string, int>
            {
                { "Test", 1 },
            });
        await Assert.That(cache.Get("Test")).IsEqualTo(1);
    }

    [Test]
    public async Task Capacity_Should_MatchTheSpecificInitializer()
    {
        var cache = new LruCache<string, int>(10);
        
        await Assert.That(cache.Capacity).IsEqualTo(10);
    }

    [Test]
    public async Task Count_Should_MatchTheCurrentNumberOfItems()
    {
        var cache = new LruCache<string, int>(10);
        
        await Assert.That(cache.Count).IsEqualTo(0);

        cache.Put("Test", 1);
        await Assert.That(cache.Count).IsEqualTo(1);
    }

    [Test]
    public void Put_Should_ReturnNull_When_CapacityIsNotExceeded()
    {
        var cache = new LruCache<string, int>(10);
        
        var ejected = cache.Put("Test", 1);
        
        Assert.Null(ejected);
    }

    public record EjectedTuple(string Key, int Value)
    {
        public static implicit operator (string, int)(EjectedTuple tuple) =>
            (tuple.Key, tuple.Value);
    }

    [Test]
    [MethodDataSource(nameof(PutCapacityExceeded))]
    public async Task Put_Should_ReturnOldestEntry_When_CapacityIsExceeded(
        Dictionary<string, int> initialEntries,
        Dictionary<string, int> otherEntries,
        EjectedTuple expectedEjected)
    {
        var cache = new LruCache<string, int>(initialEntries.Count, initialEntries);
        foreach (var entry in otherEntries)
        {
            cache.Put(entry.Key, entry.Value);
        }
        
        var ejected = cache.Put("TestKey", int.MaxValue);
        
        Assert.NotNull(ejected);
        await Assert.That(ejected).IsEqualTo(expectedEjected);
    }

    public static IEnumerable<Func<(Dictionary<string, int>, Dictionary<string, int>, EjectedTuple)>> PutCapacityExceeded()
    {
        yield return () => (
            new Dictionary<string, int>
            {
                { "Test", 1 },
                { "Test2", 2 },
            },
            new Dictionary<string, int>(),
            new EjectedTuple("Test", 1)
        );
        yield return () => (
            new Dictionary<string, int>
            {
                { "Test", 1 },
                { "Test2", 2 },
            },
            new Dictionary<string, int>
            {
                { "Test3", 3 },
            },
            new EjectedTuple("Test2", 2)
        );
        yield return () => (
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
            new EjectedTuple("Test3", 3)
        );
    }
}