using EComm.Shared.ValueObjects;

namespace EComm.Shared.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        //Arrange
        var money1 = new Money(100m, Currency.Try);
        var money2 = new Money(100m, Currency.Try);

        //Act&Assert
        Assert.Equal(money1, money2);
        Assert.True(money1 == money2);
    }

    [Fact]
    public void Equality_DifferentCurrency_AreNotEqual()
    {
        var tryMoney = new Money(100m, Currency.Try);
        var usdMoney = new Money(100m, Currency.Usd);

        Assert.NotEqual(tryMoney, usdMoney);
    }

    [Fact]
    public void Immutability_Add_DoesNotModifyOriginalInstances()
    {
        var money1 = new Money(100m, Currency.Try);
        var money2 = new Money(50m, Currency.Try);
        
        var result = money1.Add(money2);

        Assert.Equal(150m, result.Amount);
        Assert.Equal(100m, money1.Amount);
        Assert.Equal(50m, money2.Amount);
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        // Arrange
        var money1 = new Money(120.50m, Currency.Try);
        var money2 = new Money(79.50m, Currency.Try);

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(new Money(200m, Currency.Try), result);
    }
    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        // Arrange
        var money1 = new Money(200m, Currency.Usd);
        var money2 = new Money(75.25m, Currency.Usd);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.Equal(new Money(124.75m, Currency.Usd), result);
    }

    [Theory]
    [InlineData(100,0,0)]
    [InlineData(100,1,100)]
    [InlineData(25.50,3,76.50)]
    public void Multiply_ValidQuantity_ReturnsExpectedAmount(decimal unitAmount,int quantity,decimal expectedAmount)
    {
        //Arrange
        var unitPrice = new Money(unitAmount, Currency.Try);

        var result = unitPrice.Multiply(quantity);

        Assert.Equal(new Money(expectedAmount,Currency.Try), result);
    }

    [Fact]
    public void Constructor_NegativeAmount_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(-10m, Currency.Try));
    }

    [Fact]
    public void Add_DifferentCurrencies_ThrowsInvalidOperationException()
    {
        // Arrange
        var tryMoney = new Money(100m, Currency.Try);
        var usdMoney = new Money(50m, Currency.Usd);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => tryMoney.Add(usdMoney));
    }
    [Fact]
    public void Subtract_DifferentCurrencies_ThrowsInvalidOperationException()
    {
        // Arrange
        var tryMoney = new Money(100m, Currency.Try);
        var eurMoney = new Money(50m, Currency.Eur);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => tryMoney.Subtract(eurMoney));
    }

    [Fact]
    public void Subtract_ResultWouldBeNegative_ThrowsInvalidOperationException()
    {
        // Arrange
        var small = new Money(50m, Currency.Try);
        var large = new Money(100m, Currency.Try);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => small.Subtract(large));
    }

    [Fact]
    public void Multiply_NegativeQuantity_ThrowsArgumentException()
    {
        // Arrange
        var money = new Money(100m, Currency.Try);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => money.Multiply(-2));
    }
    [Fact]
    public void Zero_ReturnsZeroAmountInGivenCurrency()
    {
        var result = Money.Zero(Currency.Try);

        Assert.Equal(0m, result.Amount);
        Assert.Equal(Currency.Try, result.Currency);
        Assert.Equal(new Money(0m, Currency.Try), result);
    }

    [Theory]
    [InlineData(3.335, 3.34)]
    [InlineData(3.334, 3.33)]
    [InlineData(0.005, 0.01)]
    [InlineData(10, 10)]
    public void Constructor_AmountWithExtraDecimals_RoundsToTwoDecimalsAwayFromZero(decimal amount, decimal expected)
    {
        var money = new Money(amount, Currency.Try);

        Assert.Equal(expected, money.Amount);
    }

    [Fact]
    public void Constructor_NullCurrency_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Money(10m, null!));
    }

    [Fact]
    public void Add_Null_ThrowsArgumentNullException()
    {
        var money = new Money(100m, Currency.Try);

        Assert.Throws<ArgumentNullException>(() => money.Add(null!));
    }

    [Fact]
    public void Subtract_Null_ThrowsArgumentNullException()
    {
        var money = new Money(100m, Currency.Try);

        Assert.Throws<ArgumentNullException>(() => money.Subtract(null!));
    }
}
