using System.Collections.Concurrent;
using OrderAccumulator.Enums;
using OrderAccumulator.Interfaces;

namespace OrderAccumulator.Services;

public class ExposureTracker : IExposureTracker
{
    private readonly ConcurrentDictionary<string, decimal> _exposures = new();
    private readonly ILogger<ExposureTracker> _logger;

    public ExposureTracker(ILogger<ExposureTracker> logger)
    {
        _logger = logger;
    }

    public void Record(string symbol, OrderSide side, decimal quantity, decimal price)
    {
        var amount = quantity * price;

        var exposure = _exposures.AddOrUpdate(
            symbol,
            side == OrderSide.Buy ? amount : -amount,
            (_, current) => side == OrderSide.Buy ? current + amount : current - amount
        );

        _logger.LogInformation(
            "Recorded {Side} {Quantity} {Symbol} @ {Price} | Amount: {Amount} | Exposure: {Exposure}",
            side == OrderSide.Buy ? "BUY" : "SELL",
            quantity,
            symbol,
            price,
            amount,
            exposure
        );
    }

    public IReadOnlyDictionary<string, decimal> GetExposures()
    {
        return new Dictionary<string, decimal>(_exposures);
    }
}
