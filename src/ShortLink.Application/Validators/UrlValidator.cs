namespace ShortLink.Application.Validators;

public static class UrlValidator
{
    private const int MaxLongUrlLength = 2048;
    private const int MinShortCodeLength = 3;
    private const int MaxShortCodeLength = 30;

    public static (bool IsValid, string? ErrorMessage) ValidateLongUrl(string? longUrl)
    {
        if (string.IsNullOrWhiteSpace(longUrl))
            return (false, "Long URL cannot be empty.");

        if (longUrl.Length > MaxLongUrlLength)
            return (false, $"Long URL cannot exceed {MaxLongUrlLength} characters.");

        if (!Uri.TryCreate(longUrl, UriKind.Absolute, out var uri))
            return (false, "Long URL must start with http:// or https:// (e.g. https://example.com).");

        if (uri.Scheme != "http" && uri.Scheme != "https")
            return (false, "Long URL must use http or https scheme.");

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidateCustomShortCode(string? shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
            return (false, "Short code cannot be empty.");

        if (shortCode.Length < MinShortCodeLength || shortCode.Length > MaxShortCodeLength)
            return (false, $"Short code must be between {MinShortCodeLength} and {MaxShortCodeLength} characters.");

        if (!shortCode.All(c => char.IsLetterOrDigit(c) || c == '-'))
            return (false, "Short code may only contain letters, digits, and hyphens.");

        if (shortCode.StartsWith('-') || shortCode.EndsWith('-'))
            return (false, "Short code cannot start or end with a hyphen.");

        return (true, null);
    }
}
