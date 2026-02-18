namespace ShortLink.Domain.Entities;

public class UrlMapping
{
    private int _clickCount;

    public string ShortCode { get; }
    public string LongUrl { get; }
    public DateTime CreatedAtUtc { get; }
    public int ClickCount => _clickCount;

    public UrlMapping(string shortCode, string longUrl)
    {
        ShortCode = shortCode ?? throw new ArgumentNullException(nameof(shortCode));
        LongUrl = longUrl ?? throw new ArgumentNullException(nameof(longUrl));
        CreatedAtUtc = DateTime.UtcNow;
        _clickCount = 0;
    }

    public void IncrementClick()
    {
        Interlocked.Increment(ref _clickCount);
    }
}
