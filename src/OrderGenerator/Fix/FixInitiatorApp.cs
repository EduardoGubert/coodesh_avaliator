using OrderGenerator.Models;
using OrderGenerator.Services;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderGenerator.Fix;

public class FixInitiatorApp : MessageCracker, IApplication
{
    private readonly FixOrderService _orderService;
    private readonly ILogger<FixInitiatorApp> _logger;

    public FixInitiatorApp(FixOrderService orderService, ILogger<FixInitiatorApp> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public void OnCreate(SessionID sessionId)
    {
        _logger.LogInformation("Session created: {SessionId}", sessionId);
    }

    public void OnLogon(SessionID sessionId)
    {
        _orderService.SetSessionId(sessionId);
        _logger.LogInformation("FIX session established with {TargetCompID}", sessionId.TargetCompID);
    }

    public void OnLogout(SessionID sessionId)
    {
        _orderService.SetSessionId(null);
        _logger.LogWarning("FIX session disconnected from {TargetCompID}", sessionId.TargetCompID);
    }

    public void ToAdmin(QuickFix.Message message, SessionID sessionId) { }

    public void FromAdmin(QuickFix.Message message, SessionID sessionId) { }

    public void ToApp(QuickFix.Message message, SessionID sessionId) { }

    public void FromApp(QuickFix.Message message, SessionID sessionId)
    {
        Crack(message, sessionId);
    }

    public void OnMessage(ExecutionReport report, SessionID sessionId)
    {
        var clOrdId = report.ClOrdID.Value;
        var ordStatus = report.OrdStatus.Value;

        var response = new OrderResponse
        {
            OrderId = report.OrderID.Value,
            ExecId = report.ExecID.Value,
            ClOrdId = clOrdId,
            Symbol = report.Symbol.Value,
            Side = report.Side.Value == Side.BUY ? "BUY" : "SELL",
            Quantity = (int)report.CumQty.Value,
            Price = report.AvgPx.Value,
            Status = ordStatus switch
            {
                OrdStatus.FILLED => "Filled",
                OrdStatus.REJECTED => "Rejected",
                _ => ordStatus.ToString()
            },
            Message = report.IsSetText() ? report.Text.Value : null
        };

        _logger.LogInformation(
            "Received ExecutionReport {ExecId}: {Status} for {ClOrdId}",
            response.ExecId,
            response.Status,
            clOrdId
        );

        _orderService.CompleteOrder(clOrdId, response);
    }
}
