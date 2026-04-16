using OrderAccumulator.Fix;
using OrderAccumulator.Interfaces;
using OrderAccumulator.Services;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.UseUrls("http://0.0.0.0:5002");
builder.Services.AddSingleton<IExposureTracker, ExposureTracker>();
builder.Services.AddSingleton<FixAcceptorApp>();

var app = builder.Build();

var acceptorApp = app.Services.GetRequiredService<FixAcceptorApp>();
var cfgPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Fix", "acceptor.cfg");
var settings = new SessionSettings(cfgPath);
var storeFactory = new FileStoreFactory(settings);
var logFactory = new FileLogFactory(settings);
var acceptor = new ThreadedSocketAcceptor(acceptorApp, storeFactory, settings, logFactory);

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    acceptor.Start();
    app.Logger.LogInformation("OrderAccumulator started. FIX Acceptor listening on port 5001");
});
lifetime.ApplicationStopping.Register(() =>
{
    acceptor.Stop();
    app.Logger.LogInformation("FIX Acceptor stopped");
});

app.MapGet("/api/exposure", (IExposureTracker tracker) => Results.Ok(tracker.GetExposures()));

app.Run();
