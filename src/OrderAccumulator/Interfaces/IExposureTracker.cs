using OrderAccumulator.Enums;

namespace OrderAccumulator.Interfaces;

public interface IExposureTracker
{
    void Record(string symbol, OrderSide side, decimal quantity, decimal price);
    IReadOnlyDictionary<string, decimal> GetExposures();
}