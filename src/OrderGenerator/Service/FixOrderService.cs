using System.Collections.Concurrent;
using OrderGenerator.Interfaces;
using OrderGenerator.Models;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderGenerator.Services;

public class FixOrderService : IFixOrderService
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<OrderResponse>> _pendingOrders = new();
    private readonly ILogger<FixOrderService> _logger;
    private SessionID? _sessionId;

    public FixOrderService(ILogger<FixOrderService> logger)
    {
        _logger = logger;
    }

    public bool IsSessionConnected => _sessionId != null;

    public void SetSessionId(SessionID? sessionId)
    {
        _sessionId = sessionId;
    }

    public async Task<OrderResponse> SendOrderAsync(OrderRequest request, CancellationToken ct = default)
    {
        if (_sessionId == null)
            throw new InvalidOperationException("FIX session is not connected");

        var clOrdId = Guid.NewGuid().ToString();
        var side = request.Side.Equals("buy", StringComparison.OrdinalIgnoreCase)
            ? new Side(Side.BUY)
            : new Side(Side.SELL);

        var order = new NewOrderSingle(
            new ClOrdID(clOrdId),
            new Symbol(request.Symbol),
            side,
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.LIMIT)
        )
        {
            OrderQty = new OrderQty(request.Quantity),
            Price = new Price(request.Price)
        };

        var tcs = new TaskCompletionSource<OrderResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingOrders[clOrdId] = tcs;

        try
        {
            Session.SendToTarget(order, _sessionId);

            _logger.LogInformation(
                "Sent NewOrderSingle {ClOrdId}: {Side} {Qty} {Symbol} @ {Price}",
                clOrdId,
                request.Side.ToUpper(),
                request.Quantity,
                request.Symbol,
                request.Price
            );

            return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
        }
        finally
        {
            _pendingOrders.TryRemove(clOrdId, out _);
        }
    }

    public void CompleteOrder(string clOrdId, OrderResponse response)
    {
        if (_pendingOrders.TryRemove(clOrdId, out var tcs))
        {
            tcs.TrySetResult(response);
            _logger.LogInformation(
                "Completed order {ClOrdId}: {Status}",
                clOrdId,
                response.Status
            );
        }
        else
        {
            _logger.LogWarning("Received ExecutionReport for unknown ClOrdId: {ClOrdId}", clOrdId);
        }
    }
}
