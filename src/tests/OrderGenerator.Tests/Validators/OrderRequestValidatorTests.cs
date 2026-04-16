using OrderGenerator.Models;
using OrderGenerator.Validators;

namespace OrderGenerator.Tests.Validators;

public class OrderRequestValidatorTests
{
    private static OrderRequest ValidRequest() => new()
    {
        Symbol = "PETR4",
        Side = "buy",
        Quantity = 100,
        Price = 28.50m
    };

    [Fact]
    public void Valid_Request_ReturnsNoErrors()
    {
        var (isValid, errors) = OrderRequestValidator.Validate(ValidRequest());
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("PETR4")]
    [InlineData("VALE3")]
    [InlineData("VIIA4")]
    public void Valid_Symbols_Accepted(string symbol)
    {
        var request = ValidRequest();
        request.Symbol = symbol;
        var (isValid, _) = OrderRequestValidator.Validate(request);
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("PETR3")]
    [InlineData("")]
    public void Invalid_Symbol_ReturnsError(string symbol)
    {
        var request = ValidRequest();
        request.Symbol = symbol;
        var (isValid, errors) = OrderRequestValidator.Validate(request);
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Symbol"));
    }

    [Theory]
    [InlineData("buy")]
    [InlineData("sell")]
    [InlineData("BUY")]
    [InlineData("SELL")]
    public void Valid_Sides_Accepted(string side)
    {
        var request = ValidRequest();
        request.Side = side;
        var (isValid, _) = OrderRequestValidator.Validate(request);
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("hold")]
    [InlineData("")]
    [InlineData("compra")]
    public void Invalid_Side_ReturnsError(string side)
    {
        var request = ValidRequest();
        request.Side = side;
        var (isValid, errors) = OrderRequestValidator.Validate(request);
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Side"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100000)]
    [InlineData(100001)]
    public void Invalid_Quantity_ReturnsError(int quantity)
    {
        var request = ValidRequest();
        request.Quantity = quantity;
        var (isValid, errors) = OrderRequestValidator.Validate(request);
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Quantity"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99999)]
    [InlineData(50000)]
    public void Valid_Quantity_Accepted(int quantity)
    {
        var request = ValidRequest();
        request.Quantity = quantity;
        var (isValid, _) = OrderRequestValidator.Validate(request);
        Assert.True(isValid);
    }

    [Fact]
    public void Price_Zero_ReturnsError()
    {
        var request = ValidRequest();
        request.Price = 0m;
        var (isValid, errors) = OrderRequestValidator.Validate(request);
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Price"));
    }

    [Fact]
    public void Price_TooLarge_ReturnsError()
    {
        var request = ValidRequest();
        request.Price = 1000m;
        var (isValid, errors) = OrderRequestValidator.Validate(request);
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Price"));
    }

    [Fact]
    public void Price_NotMultipleOf001_ReturnsError()
    {
        var request = ValidRequest();
        request.Price = 10.005m;
        var (isValid, errors) = OrderRequestValidator.Validate(request);
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("multiple"));
    }

    [Fact]
    public void Multiple_Errors_ReturnsAll()
    {
        var request = new OrderRequest
        {
            Symbol = "INVALID",
            Side = "hold",
            Quantity = 0,
            Price = 0m
        };

        var (isValid, errors) = OrderRequestValidator.Validate(request);
        Assert.False(isValid);
        Assert.True(errors.Length >= 3);
    }
}
