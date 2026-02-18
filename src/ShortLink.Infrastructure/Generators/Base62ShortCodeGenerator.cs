using System.Security.Cryptography;
using ShortLink.Domain.Interfaces;

namespace ShortLink.Infrastructure.Generators;

public class Base62ShortCodeGenerator : IShortCodeGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate(int length = 7)
    {
        if (length < 1 || length > 30)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 30.");

        var chars = new char[length];
        var bytes = RandomNumberGenerator.GetBytes(length);

        for (int i = 0; i < length; i++)
        {
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        }

        return new string(chars);
    }
}
