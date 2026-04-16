using OrderAccumulator.Enums;
using OrderAccumulator.Interfaces;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderAccumulator.Fix;

public class FixAcceptorApp : MessageCracker, IApplication
{
    private readonly IExposureTracker _exposureTracker;
    private readonly ILogger<FixAcceptorApp> _logger;

    public FixAcceptorApp(IExposureTracker exposureTracker, ILogger<FixAcceptorApp> logger)
    {
        _exposureTracker = exposureTracker;
        _logger = logger;
    }

    public void OnCreate(SessionID sessionId)
    {
        _logger.LogInformation("Session created: {SessionId}", sessionId);
    }

    public void OnLogon(SessionID sessionId)
    {
        _logger.LogInformation("Logon: {SessionId}", sessionId);
    }

    public void OnLogout(SessionID sessionId)
    {
        _logger.LogWarning("Logout: {SessionId}", sessionId);
    }

    public void ToAdmin(QuickFix.Message message, SessionID sessionId) { }

    public void FromAdmin(QuickFix.Message message, SessionID sessionId) { }

    public void ToApp(QuickFix.Message message, SessionID sessionId) { }

    public void FromApp(QuickFix.Message message, SessionID sessionId)
    {
        Crack(message, sessionId);
    }

    public void OnMessage(NewOrderSingle order, SessionID sessionId)
    {
        try
        {
            var clOrdId = order.ClOrdID.Value;
            var symbol = order.Symbol.Value;
            var side = order.Side;
            var orderQty = order.OrderQty.Value;
            var price = order.Price.Value;

            _logger.LogInformation(
                "Received NewOrderSingle {ClOrdId}: {Side} {Qty} {Symbol} @ {Price}",
                clOrdId,
                side.Value == Side.BUY ? "BUY" : "SELL",
                orderQty,
                symbol,
                price
            );

            _exposureTracker.Record(symbol, side, (decimal)orderQty, (decimal)price);

            var orderId = Guid.NewGuid().ToString();
            var execId = Guid.NewGuid().ToString();

            var execReport = new ExecutionReport(
                new OrderID(orderId),
                new ExecID(execId),
                new ExecType(ExecType.TRADE),
                new OrdStatus(OrdStatus.FILLED),
                order.Symbol,
                order.Side,
                new LeavesQty(0m),
                new CumQty(orderQty),
                new AvgPx(price)
            )
            {
                ClOrdID = new ClOrdID(clOrdId),
                LastQty = new LastQty(orderQty),
                LastPx = new LastPx(price)
            };

            Session.SendToTarget(execReport, sessionId);

            _logger.LogInformation(
                "Sent ExecutionReport {ExecId} for order {ClOrdId}: FILLED",
                execId,
                clOrdId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing NewOrderSingle");

            var rejectReport = new ExecutionReport(
                new OrderID("NONE"),
                new ExecID(Guid.NewGuid().ToString()),
                new ExecType(ExecType.REJECTED),
                new OrdStatus(OrdStatus.REJECTED),
                order.Symbol,
                order.Side,
                new LeavesQty(0m),
                new CumQty(0m),
                new AvgPx(0m)
            )
            {
                ClOrdID = order.ClOrdID,
                Text = new Text(ex.Message)
            };

            Session.SendToTarget(rejectReport, sessionId);
        }
    }
}
