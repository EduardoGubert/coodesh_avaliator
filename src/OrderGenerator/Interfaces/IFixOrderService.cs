using OrderGenerator.Models;

namespace OrderGenerator.Interfaces;

public interface IFixOrderService
{
    Task<OrderResponse> SendOrderAsync(OrderRequest request, CancellationToken ct = default);
    bool IsSessionConnected { get; }
}
