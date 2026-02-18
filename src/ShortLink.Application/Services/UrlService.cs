using ShortLink.Application.DTOs;
using ShortLink.Application.Validators;
using ShortLink.Domain.Entities;
using ShortLink.Domain.Enums;
using ShortLink.Domain.Interfaces;
using ShortLink.Domain.Results;

namespace ShortLink.Application.Services;

public class UrlService
{
    private readonly IUrlRepository _repository;
    private readonly IShortCodeGenerator _generator;
    private const int MaxGenerationAttempts = 10;

    public UrlService(IUrlRepository repository, IShortCodeGenerator generator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
    }

    public Result<UrlMapping> CreateShortUrl(string longUrl, string? customShortCode = null)
    {
        var (isValidUrl, urlError) = UrlValidator.ValidateLongUrl(longUrl);
        if (!isValidUrl)
            return Result<UrlMapping>.Failure(urlError!, ErrorCode.InvalidUrl);

        string shortCode;
        if (!string.IsNullOrWhiteSpace(customShortCode))
        {
            var (isValidCode, codeError) = UrlValidator.ValidateCustomShortCode(customShortCode);
            if (!isValidCode)
                return Result<UrlMapping>.Failure(codeError!, ErrorCode.InvalidShortCode);

            shortCode = customShortCode;
        }
        else
        {
            var generated = GenerateUniqueCode();
            if (generated is null)
                return Result<UrlMapping>.Failure(
                    "Failed to generate a unique short code after multiple attempts.",
                    ErrorCode.GenerationFailed);

            shortCode = generated;
        }

        var mapping = new UrlMapping(shortCode, longUrl);
        if (!_repository.Add(mapping))
            return Result<UrlMapping>.Failure(
                $"Short code '{shortCode}' is already in use.",
                ErrorCode.DuplicateShortCode);

        return Result<UrlMapping>.Success(mapping);
    }

    public Result<bool> DeleteShortUrl(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
            return Result<bool>.Failure("Short code cannot be empty.", ErrorCode.InvalidShortCode);

        if (!_repository.Delete(shortCode))
            return Result<bool>.Failure(
                $"Short code '{shortCode}' was not found.",
                ErrorCode.NotFound);

        return Result<bool>.Success(true);
    }

    public Result<string> ResolveShortUrl(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
            return Result<string>.Failure("Short code cannot be empty.", ErrorCode.InvalidShortCode);

        var mapping = _repository.GetByShortCode(shortCode);
        if (mapping is null)
            return Result<string>.Failure(
                $"Short code '{shortCode}' was not found.",
                ErrorCode.NotFound);

        mapping.IncrementClick();
        return Result<string>.Success(mapping.LongUrl);
    }

    public Result<UrlStatistics> GetStatistics(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
            return Result<UrlStatistics>.Failure("Short code cannot be empty.", ErrorCode.InvalidShortCode);

        var mapping = _repository.GetByShortCode(shortCode);
        if (mapping is null)
            return Result<UrlStatistics>.Failure(
                $"Short code '{shortCode}' was not found.",
                ErrorCode.NotFound);

        var stats = new UrlStatistics
        {
            ShortCode = mapping.ShortCode,
            LongUrl = mapping.LongUrl,
            ClickCount = mapping.ClickCount,
            CreatedAtUtc = mapping.CreatedAtUtc
        };

        return Result<UrlStatistics>.Success(stats);
    }

    public IEnumerable<UrlMapping> ListAll() => _repository.GetAll();

    private string? GenerateUniqueCode()
    {
        for (int attempt = 0; attempt < MaxGenerationAttempts; attempt++)
        {
            var code = _generator.Generate();
            if (!_repository.Exists(code))
                return code;
        }
        return null;
    }
}
