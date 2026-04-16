namespace OrderGenerator.Models;

public class OrderResponse
{
    public string OrderId { get; set; } = string.Empty;
    public string ExecId { get; set; } = string.Empty;
    public string ClOrdId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}
