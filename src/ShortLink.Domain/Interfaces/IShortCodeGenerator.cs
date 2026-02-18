namespace ShortLink.Domain.Interfaces;

public interface IShortCodeGenerator
{
    string Generate(int length = 7);
}
