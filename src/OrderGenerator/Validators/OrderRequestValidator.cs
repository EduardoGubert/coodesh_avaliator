using OrderGenerator.Models;

namespace OrderGenerator.Validators;

public static class OrderRequestValidator
{
    private static readonly HashSet<string> ValidSymbols = ["PETR4", "VALE3", "VIIA4"];
    private static readonly HashSet<string> ValidSides = new(StringComparer.OrdinalIgnoreCase) { "buy", "sell" };

    public static (bool IsValid, string[] Errors) Validate(OrderRequest request)
    {
        var errors = new List<string>();

        if (!ValidSymbols.Contains(request.Symbol))
            errors.Add($"Symbol must be one of: {string.Join(", ", ValidSymbols)}");

        if (!ValidSides.Contains(request.Side))
            errors.Add("Side must be 'buy' or 'sell'");

        if (request.Quantity <= 0 || request.Quantity >= 100_000)
            errors.Add("Quantity must be a positive integer less than 100,000");

        if (request.Price <= 0 || request.Price >= 1_000)
            errors.Add("Price must be a positive value less than 1,000");

        if (request.Price > 0 && request.Price % 0.01m != 0)
            errors.Add("Price must be a multiple of 0.01");

        return (errors.Count == 0, errors.ToArray());
    }
}
