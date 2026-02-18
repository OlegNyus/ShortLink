using ShortLink.Domain.Entities;
using ShortLink.Infrastructure.Repositories;
using Xunit;

namespace ShortLink.Tests.Repositories;

public class InMemoryUrlRepositoryTests
{
    private readonly InMemoryUrlRepository _repository = new();

    [Fact]
    public void Add_ValidMapping_ReturnsTrue()
    {
        var mapping = new UrlMapping("abc123", "https://example.com");

        var result = _repository.Add(mapping);

        Assert.True(result);
    }

    [Fact]
    public void Add_DuplicateShortCode_ReturnsFalse()
    {
        _repository.Add(new UrlMapping("abc123", "https://example.com"));

        var result = _repository.Add(new UrlMapping("abc123", "https://other.com"));

        Assert.False(result);
    }

    [Fact]
    public void GetByShortCode_Existing_ReturnsMapping()
    {
        var mapping = new UrlMapping("abc123", "https://example.com");
        _repository.Add(mapping);

        var result = _repository.GetByShortCode("abc123");

        Assert.NotNull(result);
        Assert.Equal("abc123", result.ShortCode);
        Assert.Equal("https://example.com", result.LongUrl);
    }

    [Fact]
    public void GetByShortCode_NonExistent_ReturnsNull()
    {
        var result = _repository.GetByShortCode("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public void Delete_Existing_ReturnsTrue()
    {
        _repository.Add(new UrlMapping("abc123", "https://example.com"));

        var result = _repository.Delete("abc123");

        Assert.True(result);
    }

    [Fact]
    public void Delete_NonExistent_ReturnsFalse()
    {
        var result = _repository.Delete("nonexistent");

        Assert.False(result);
    }

    [Fact]
    public void Delete_ThenGet_ReturnsNull()
    {
        _repository.Add(new UrlMapping("abc123", "https://example.com"));
        _repository.Delete("abc123");

        var result = _repository.GetByShortCode("abc123");

        Assert.Null(result);
    }

    [Fact]
    public void Exists_Existing_ReturnsTrue()
    {
        _repository.Add(new UrlMapping("abc123", "https://example.com"));

        Assert.True(_repository.Exists("abc123"));
    }

    [Fact]
    public void Exists_NonExistent_ReturnsFalse()
    {
        Assert.False(_repository.Exists("nonexistent"));
    }

    [Fact]
    public void GetAll_ReturnsAllMappings()
    {
        _repository.Add(new UrlMapping("code1", "https://example1.com"));
        _repository.Add(new UrlMapping("code2", "https://example2.com"));
        _repository.Add(new UrlMapping("code3", "https://example3.com"));

        var all = _repository.GetAll().ToList();

        Assert.Equal(3, all.Count);
    }

    [Fact]
    public void Count_ReflectsAdditionsAndDeletions()
    {
        Assert.Equal(0, _repository.Count);

        _repository.Add(new UrlMapping("code1", "https://example1.com"));
        Assert.Equal(1, _repository.Count);

        _repository.Add(new UrlMapping("code2", "https://example2.com"));
        Assert.Equal(2, _repository.Count);

        _repository.Delete("code1");
        Assert.Equal(1, _repository.Count);
    }
}
