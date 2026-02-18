using ShortLink.Domain.Entities;

namespace ShortLink.Domain.Interfaces;

public interface IUrlRepository
{
    bool Add(UrlMapping mapping);
    bool Delete(string shortCode);
    UrlMapping? GetByShortCode(string shortCode);
    bool Exists(string shortCode);
    IEnumerable<UrlMapping> GetAll();
    int Count { get; }
}
