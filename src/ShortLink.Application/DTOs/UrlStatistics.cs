namespace ShortLink.Application.DTOs;

public class UrlStatistics
{
    public string ShortCode { get; init; } = string.Empty;
    public string LongUrl { get; init; } = string.Empty;
    public int ClickCount { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
