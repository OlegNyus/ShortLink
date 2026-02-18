using ShortLink.Infrastructure.Generators;
using Xunit;

namespace ShortLink.Tests.Generators;

public class Base62ShortCodeGeneratorTests
{
    private readonly Base62ShortCodeGenerator _generator = new();

    [Fact]
    public void Generate_DefaultLength_Returns7Characters()
    {
        var code = _generator.Generate();

        Assert.Equal(7, code.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(30)]
    public void Generate_ReturnsStringOfRequestedLength(int length)
    {
        var code = _generator.Generate(length);

        Assert.Equal(length, code.Length);
    }

    [Fact]
    public void Generate_OnlyContainsBase62Characters()
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        for (int i = 0; i < 100; i++)
        {
            var code = _generator.Generate();
            Assert.All(code, c => Assert.Contains(c, validChars));
        }
    }

    [Fact]
    public void Generate_MultipleCallsProduceDifferentResults()
    {
        var codes = new HashSet<string>();
        for (int i = 0; i < 50; i++)
        {
            codes.Add(_generator.Generate());
        }

        // With 62^7 possible combinations, 50 codes should all be unique
        Assert.Equal(50, codes.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(31)]
    public void Generate_InvalidLength_ThrowsArgumentOutOfRange(int length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Generate(length));
    }
}
