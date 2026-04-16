using QuickFix.Fields;

namespace OrderAccumulator.Interfaces;

public interface IExposureTracker
{
    void Record(string symbol, Side side, decimal quantity, decimal price);
    IReadOnlyDictionary<string, decimal> GetExposures();
}