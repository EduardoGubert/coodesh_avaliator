using System.ComponentModel.DataAnnotations;

namespace OrderGenerator.Models;

public class OrderRequest
{
    [Required]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public string Side { get; set; } = string.Empty;

    [Range(1, 99999)]
    public int Quantity { get; set; }

    [Range(0.01, 999.99)]
    public decimal Price { get; set; }
}
