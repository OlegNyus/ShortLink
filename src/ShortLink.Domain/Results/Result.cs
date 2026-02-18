using ShortLink.Domain.Enums;

namespace ShortLink.Domain.Results;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public ErrorCode Code { get; }

    private Result(bool isSuccess, T? value, string? errorMessage, ErrorCode code)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Code = code;
    }

    public static Result<T> Success(T value)
        => new(true, value, null, ErrorCode.None);

    public static Result<T> Failure(string errorMessage, ErrorCode code)
        => new(false, default, errorMessage, code);
}
