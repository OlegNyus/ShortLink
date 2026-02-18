using System.Collections.Concurrent;
using ShortLink.Domain.Entities;
using ShortLink.Domain.Interfaces;

namespace ShortLink.Infrastructure.Repositories;

public class InMemoryUrlRepository : IUrlRepository
{
    private readonly ConcurrentDictionary<string, UrlMapping> _store = new();

    public bool Add(UrlMapping mapping)
    {
        return _store.TryAdd(mapping.ShortCode, mapping);
    }

    public bool Delete(string shortCode)
    {
        return _store.TryRemove(shortCode, out _);
    }

    public UrlMapping? GetByShortCode(string shortCode)
    {
        _store.TryGetValue(shortCode, out var mapping);
        return mapping;
    }

    public bool Exists(string shortCode)
    {
        return _store.ContainsKey(shortCode);
    }

    public IEnumerable<UrlMapping> GetAll()
    {
        return _store.Values.ToList();
    }

    public int Count => _store.Count;
}
