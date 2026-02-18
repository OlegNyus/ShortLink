namespace ShortLink.Domain.Enums;

public enum ErrorCode
{
    None = 0,
    NotFound,
    DuplicateShortCode,
    InvalidUrl,
    InvalidShortCode,
    GenerationFailed
}
