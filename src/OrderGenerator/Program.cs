using OrderGenerator.Fix;
using OrderGenerator.Interfaces;
using OrderGenerator.Models;
using OrderGenerator.Services;
using OrderGenerator.Validators;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5050");

builder.Services.AddSingleton<FixOrderService>();
builder.Services.AddSingleton<IFixOrderService>(sp => sp.GetRequiredService<FixOrderService>());
builder.Services.AddSingleton<FixInitiatorApp>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

var initiatorApp = app.Services.GetRequiredService<FixInitiatorApp>();
var cfgPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Fix", "initiator.cfg");
var settings = new SessionSettings(cfgPath);
var storeFactory = new FileStoreFactory(settings);
var logFactory = new FileLogFactory(settings);
var initiator = new SocketInitiator(initiatorApp, storeFactory, settings, logFactory);

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    initiator.Start();
    app.Logger.LogInformation("OrderGenerator started. FIX Initiator connecting to port 5001");
});
lifetime.ApplicationStopping.Register(() =>
{
    initiator.Stop();
    app.Logger.LogInformation("FIX Initiator stopped");
});

app.MapPost("/api/orders", async (OrderRequest request, IFixOrderService orderService) =>
{
    var (isValid, errors) = OrderRequestValidator.Validate(request);
    if (!isValid)
    {
        return Results.BadRequest(new ApiErrorResponse
        {
            Error = "Validation failed",
            Details = errors
        });
    }

    if (!orderService.IsSessionConnected)
    {
        return Results.Json(
            new ApiErrorResponse { Error = "FIX session is not connected. Please try again later." },
            statusCode: 503
        );
    }

    try
    {
        var response = await orderService.SendOrderAsync(request);
        return Results.Ok(response);
    }
    catch (TimeoutException)
    {
        return Results.Json(
            new ApiErrorResponse { Error = "Order timed out waiting for execution report." },
            statusCode: 504
        );
    }
});

app.Run();
