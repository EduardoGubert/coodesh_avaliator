using Microsoft.Extensions.Logging;
using Moq;
using OrderGenerator.Models;
using OrderGenerator.Services;
using QuickFix;

namespace OrderGenerator.Tests.Services;

public class FixOrderServiceTests
{
    private readonly FixOrderService _service;

    public FixOrderServiceTests()
    {
        var logger = new Mock<ILogger<FixOrderService>>();
        _service = new FixOrderService(logger.Object);
    }

    [Fact]
    public async Task SendOrder_WhenSessionDisconnected_ThrowsInvalidOperation()
    {
        var request = new OrderRequest
        {
            Symbol = "PETR4",
            Side = "buy",
            Quantity = 100,
            Price = 28.50m
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SendOrderAsync(request)
        );
    }

    [Fact]
    public void IsSessionConnected_FalseByDefault()
    {
        Assert.False(_service.IsSessionConnected);
    }

    [Fact]
    public void SetSessionId_MakesConnectedTrue()
    {
        _service.SetSessionId(new SessionID("FIX.4.4", "SENDER", "TARGET"));
        Assert.True(_service.IsSessionConnected);
    }

    [Fact]
    public void SetSessionId_Null_MakesConnectedFalse()
    {
        _service.SetSessionId(new SessionID("FIX.4.4", "SENDER", "TARGET"));
        _service.SetSessionId(null);
        Assert.False(_service.IsSessionConnected);
    }

    [Fact]
    public void CompleteOrder_UnknownClOrdId_DoesNotThrow()
    {
        var response = new OrderResponse
        {
            OrderId = "test",
            ExecId = "test",
            ClOrdId = "unknown",
            Symbol = "PETR4",
            Side = "BUY",
            Quantity = 100,
            Price = 28.50m,
            Status = "Filled"
        };

        var exception = Record.Exception(() => _service.CompleteOrder("unknown", response));
        Assert.Null(exception);
    }
}
