using Moq;
using ShortLink.Application.Services;
using ShortLink.Domain.Entities;
using ShortLink.Domain.Enums;
using ShortLink.Domain.Interfaces;
using ShortLink.Infrastructure.Generators;
using ShortLink.Infrastructure.Repositories;
using Xunit;

namespace ShortLink.Tests.Services;

public class UrlServiceTests
{
    private readonly InMemoryUrlRepository _repository;
    private readonly Base62ShortCodeGenerator _generator;
    private readonly UrlService _sut;

    public UrlServiceTests()
    {
        _repository = new InMemoryUrlRepository();
        _generator = new Base62ShortCodeGenerator();
        _sut = new UrlService(_repository, _generator);
    }

    // ========== Create ==========

    [Fact]
    public void Create_ValidUrl_AutoGenerate_ReturnsSuccess()
    {
        var result = _sut.CreateShortUrl("https://example.com/some/long/path");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(7, result.Value!.ShortCode.Length);
        Assert.Equal("https://example.com/some/long/path", result.Value.LongUrl);
        Assert.Equal(0, result.Value.ClickCount);
    }

    [Fact]
    public void Create_ValidUrl_CustomCode_ReturnsSuccess()
    {
        var result = _sut.CreateShortUrl("https://example.com", "mycode");

        Assert.True(result.IsSuccess);
        Assert.Equal("mycode", result.Value!.ShortCode);
        Assert.Equal("https://example.com", result.Value.LongUrl);
    }

    [Fact]
    public void Create_DuplicateCustomCode_ReturnsDuplicateError()
    {
        _sut.CreateShortUrl("https://example.com", "mycode");

        var result = _sut.CreateShortUrl("https://other.com", "mycode");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.DuplicateShortCode, result.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyLongUrl_ReturnsInvalidUrlError(string? longUrl)
    {
        var result = _sut.CreateShortUrl(longUrl!);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidUrl, result.Code);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    [InlineData("mailto:test@example.com")]
    [InlineData("qahub.ai")]              // missing scheme
    [InlineData("example.com/path")]       // missing scheme with path
    public void Create_InvalidUrlScheme_ReturnsInvalidUrlError(string longUrl)
    {
        var result = _sut.CreateShortUrl(longUrl);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidUrl, result.Code);
    }

    [Theory]
    [InlineData("ab")]           // too short
    [InlineData("a")]            // too short
    [InlineData("co de")]        // has space
    [InlineData("co!de")]        // has special char
    [InlineData("-mycode")]      // starts with hyphen
    [InlineData("mycode-")]      // ends with hyphen
    public void Create_InvalidCustomCode_ReturnsInvalidShortCodeError(string customCode)
    {
        var result = _sut.CreateShortUrl("https://example.com", customCode);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidShortCode, result.Code);
    }

    [Fact]
    public void Create_UrlExceedingMaxLength_ReturnsInvalidUrlError()
    {
        var longUrl = "https://example.com/" + new string('a', 2048);

        var result = _sut.CreateShortUrl(longUrl);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidUrl, result.Code);
    }

    [Fact]
    public void Create_CustomCodeTooLong_ReturnsInvalidShortCodeError()
    {
        var longCode = new string('a', 31);

        var result = _sut.CreateShortUrl("https://example.com", longCode);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidShortCode, result.Code);
    }

    [Fact]
    public void Create_SameLongUrl_DifferentCodes_BothSucceed()
    {
        var result1 = _sut.CreateShortUrl("https://example.com", "code1");
        var result2 = _sut.CreateShortUrl("https://example.com", "code2");

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(result1.Value!.ShortCode, result2.Value!.ShortCode);
        Assert.Equal(result1.Value.LongUrl, result2.Value.LongUrl);
    }

    [Fact]
    public void Create_GeneratorCollision_RetriesAndSucceeds()
    {
        // Pre-seed the repository with a known code
        _repository.Add(new UrlMapping("collision", "https://existing.com"));

        var callCount = 0;
        var mockGenerator = new Mock<IShortCodeGenerator>();
        mockGenerator
            .Setup(g => g.Generate(It.IsAny<int>()))
            .Returns(() =>
            {
                callCount++;
                // First call returns the colliding code, second returns a unique one
                return callCount == 1 ? "collision" : "unique1";
            });

        var service = new UrlService(_repository, mockGenerator.Object);

        var result = service.CreateShortUrl("https://newsite.com");

        Assert.True(result.IsSuccess);
        Assert.Equal("unique1", result.Value!.ShortCode);
        Assert.True(callCount >= 2);
    }

    [Fact]
    public void Create_GeneratorAllCollisions_ReturnsGenerationFailed()
    {
        // Pre-seed the repository with the code that the generator always returns
        _repository.Add(new UrlMapping("always", "https://existing.com"));

        var mockGenerator = new Mock<IShortCodeGenerator>();
        mockGenerator
            .Setup(g => g.Generate(It.IsAny<int>()))
            .Returns("always");

        var service = new UrlService(_repository, mockGenerator.Object);

        var result = service.CreateShortUrl("https://newsite.com");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.GenerationFailed, result.Code);
    }

    [Fact]
    public void Create_ValidCustomCodeWithHyphens_ReturnsSuccess()
    {
        var result = _sut.CreateShortUrl("https://example.com", "my-custom-code");

        Assert.True(result.IsSuccess);
        Assert.Equal("my-custom-code", result.Value!.ShortCode);
    }

    // ========== Resolve ==========

    [Fact]
    public void Resolve_ExistingCode_ReturnsLongUrl()
    {
        _sut.CreateShortUrl("https://example.com/page", "test");

        var result = _sut.ResolveShortUrl("test");

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/page", result.Value);
    }

    [Fact]
    public void Resolve_ExistingCode_IncrementsClickCount()
    {
        _sut.CreateShortUrl("https://example.com", "test");

        _sut.ResolveShortUrl("test");

        var stats = _sut.GetStatistics("test");
        Assert.Equal(1, stats.Value!.ClickCount);
    }

    [Fact]
    public void Resolve_CalledThreeTimes_ClickCountIsThree()
    {
        _sut.CreateShortUrl("https://example.com", "test");

        _sut.ResolveShortUrl("test");
        _sut.ResolveShortUrl("test");
        _sut.ResolveShortUrl("test");

        var stats = _sut.GetStatistics("test");
        Assert.Equal(3, stats.Value!.ClickCount);
    }

    [Fact]
    public void Resolve_NonExistentCode_ReturnsNotFoundError()
    {
        var result = _sut.ResolveShortUrl("nonexistent");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_EmptyCode_ReturnsInvalidShortCodeError(string? shortCode)
    {
        var result = _sut.ResolveShortUrl(shortCode!);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidShortCode, result.Code);
    }

    // ========== Delete ==========

    [Fact]
    public void Delete_ExistingCode_ReturnsSuccess()
    {
        _sut.CreateShortUrl("https://example.com", "test");

        var result = _sut.DeleteShortUrl("test");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Delete_NonExistentCode_ReturnsNotFoundError()
    {
        var result = _sut.DeleteShortUrl("nonexistent");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Code);
    }

    [Fact]
    public void Delete_ThenResolve_ReturnsNotFound()
    {
        _sut.CreateShortUrl("https://example.com", "test");
        _sut.DeleteShortUrl("test");

        var result = _sut.ResolveShortUrl("test");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Delete_EmptyCode_ReturnsInvalidShortCodeError(string? shortCode)
    {
        var result = _sut.DeleteShortUrl(shortCode!);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidShortCode, result.Code);
    }

    // ========== Statistics ==========

    [Fact]
    public void GetStatistics_ExistingCode_ReturnsCorrectDto()
    {
        _sut.CreateShortUrl("https://example.com/page", "test");

        var result = _sut.GetStatistics("test");

        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value!.ShortCode);
        Assert.Equal("https://example.com/page", result.Value.LongUrl);
        Assert.Equal(0, result.Value.ClickCount);
        Assert.True(result.Value.CreatedAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void GetStatistics_AfterClicks_ReflectsClickCount()
    {
        _sut.CreateShortUrl("https://example.com", "test");
        _sut.ResolveShortUrl("test");
        _sut.ResolveShortUrl("test");

        var result = _sut.GetStatistics("test");

        Assert.Equal(2, result.Value!.ClickCount);
    }

    [Fact]
    public void GetStatistics_NonExistentCode_ReturnsNotFoundError()
    {
        var result = _sut.GetStatistics("nonexistent");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Code);
    }

    [Fact]
    public void GetStatistics_DoesNotIncrementClickCount()
    {
        _sut.CreateShortUrl("https://example.com", "test");

        // Call GetStatistics multiple times
        _sut.GetStatistics("test");
        _sut.GetStatistics("test");
        _sut.GetStatistics("test");

        var result = _sut.GetStatistics("test");
        Assert.Equal(0, result.Value!.ClickCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetStatistics_EmptyCode_ReturnsInvalidShortCodeError(string? shortCode)
    {
        var result = _sut.GetStatistics(shortCode!);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidShortCode, result.Code);
    }

    // ========== ListAll ==========

    [Fact]
    public void ListAll_Empty_ReturnsEmptyCollection()
    {
        var result = _sut.ListAll().ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void ListAll_WithMappings_ReturnsAll()
    {
        _sut.CreateShortUrl("https://example1.com", "code1");
        _sut.CreateShortUrl("https://example2.com", "code2");

        var result = _sut.ListAll().ToList();

        Assert.Equal(2, result.Count);
    }
}
