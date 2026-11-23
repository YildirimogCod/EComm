using EComm.Shared.ValueObjects;

namespace EComm.Shared.Tests.ValueObjects;

public class CurrencyTests
{
    [Theory]
    [InlineData("TRY", "TRY")]
    [InlineData("try", "TRY")]
    [InlineData(" usd ", "USD")]
    [InlineData("Eur", "EUR")]
    public void FromCode_ValidCode_ReturnsCurrencyWithNormalizedCode(string input, string expectedCode)
    {
        var currency = Currency.FromCode(input);

        Assert.Equal(expectedCode, currency.Code);
    }

    [Fact]
    public void FromCode_KnownCode_ReturnsSameInstanceAsStaticField()
    {
        var currency = Currency.FromCode("try");

        Assert.Same(Currency.Try, currency);
    }

    [Fact]
    public void FromCode_UnsupportedCode_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Currency.FromCode("XYZ"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FromCode_EmptyOrWhitespace_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(() => Currency.FromCode(input));
    }

    [Fact]
    public void FromCode_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Currency.FromCode(null!));
    }
}
