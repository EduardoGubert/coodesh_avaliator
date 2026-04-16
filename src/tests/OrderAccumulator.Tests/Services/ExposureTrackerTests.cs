using Microsoft.Extensions.Logging;
using Moq;
using OrderAccumulator.Enums;
using OrderAccumulator.Services;

namespace OrderAccumulator.Tests.Services;

public class ExposureTrackerTests
{
    private readonly ExposureTracker _tracker;

    public ExposureTrackerTests()
    {
        var logger = new Mock<ILogger<ExposureTracker>>();
        _tracker = new ExposureTracker(logger.Object);
    }

    [Fact]
    public void Record_Buy_IncreasesExposure()
    {
        _tracker.Record("PETR4", OrderSide.Buy, 100m, 28.50m);

        var exposures = _tracker.GetExposures();
        Assert.Equal(2850.00m, exposures["PETR4"]);
    }

    [Fact]
    public void Record_Sell_DecreasesExposure()
    {
        _tracker.Record("VALE3", OrderSide.Sell, 50m, 65.00m);

        var exposures = _tracker.GetExposures();
        Assert.Equal(-3250.00m, exposures["VALE3"]);
    }

    [Fact]
    public void Record_MultipleTrades_AccumulatesCorrectly()
    {
        _tracker.Record("PETR4", OrderSide.Buy, 100m, 28.50m); // +2850
        _tracker.Record("PETR4", OrderSide.Sell, 50m, 29.00m);  // -1450
        _tracker.Record("PETR4", OrderSide.Buy, 200m, 27.00m); // +5400

        var exposures = _tracker.GetExposures();
        Assert.Equal(6800.00m, exposures["PETR4"]); // 2850 - 1450 + 5400
    }

    [Fact]
    public void Record_DifferentSymbols_TrackedIndependently()
    {
        _tracker.Record("PETR4", OrderSide.Buy, 100m, 28.50m);
        _tracker.Record("VALE3", OrderSide.Sell, 50m, 65.00m);
        _tracker.Record("VIIA4", OrderSide.Buy, 200m, 3.50m);

        var exposures = _tracker.GetExposures();
        Assert.Equal(2850.00m, exposures["PETR4"]);
        Assert.Equal(-3250.00m, exposures["VALE3"]);
        Assert.Equal(700.00m, exposures["VIIA4"]);
    }

    [Fact]
    public void GetExposures_ReturnsSnapshot_NotReference()
    {
        _tracker.Record("PETR4", OrderSide.Buy, 100m, 10.00m);

        var snapshot = _tracker.GetExposures();
        _tracker.Record("PETR4", OrderSide.Buy, 100m, 10.00m);

        Assert.Equal(1000.00m, snapshot["PETR4"]);
    }

    [Fact]
    public void GetExposures_EmptyWhenNoTrades()
    {
        var exposures = _tracker.GetExposures();
        Assert.Empty(exposures);
    }
}
