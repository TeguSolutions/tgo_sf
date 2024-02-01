using System.Timers;
using APU.WebApp.Services.JS;
using Timer = System.Timers.Timer;

namespace APU.WebApp.Services.Latency;

public static class JSInteropMethods
{
    [JSInvokable]
    public static Task Ping()
    {
        // Your logic here
        return Task.CompletedTask;
    }
}

public class PingService : IDisposable
{
    private readonly ILogger<PingService> _logger;
    private readonly AppJS _js;

    private readonly Guid id;

    #region Lifecycle

    public PingService(ILogger<PingService> logger, AppJS js)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _js = js ?? throw new ArgumentNullException(nameof(js));

        id = Guid.NewGuid();
    }    

    #endregion

    private LatencyService _latencyService;

    private Timer _latencyTimer;
    private User _liu;

    public void Start(LatencyService latencyService, User liu)
    {
        if (liu is null)
            return;

        // already started
        if (_liu is not null)
            return;

        _latencyService = latencyService;
        _liu = liu;

        values = new Queue<long>();

        _latencyTimer = new Timer(30000);
        _latencyTimer.Elapsed += LatencyTimerOnElapsed;
        _latencyTimer.Start();
        LatencyTimerOnElapsed(null, null);
    }

    private DateTimeOffset referenceTime;
    private long latency;
    private double avgLatency;
    private Queue<long> values;

    private async void LatencyTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            referenceTime = DateTimeOffset.UtcNow;
            await _js.JSRuntime.InvokeVoidAsync("pingServer");
            latency = (DateTimeOffset.UtcNow - referenceTime).Milliseconds / 2;

            values.Enqueue(latency);
            if (values.Count > 10)
                values.Dequeue();

            avgLatency = values.Average();

            //_logger.LogLatency(_liu, latency, avgLatency);
            _latencyService.AddValue(id, _liu.Id, _liu.Name, avgLatency);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }


    #region IDisposable

    public void Dispose()
    {
        _latencyTimer?.Stop();
        _latencyTimer?.Dispose();
        _latencyTimer = null;
    }    

    #endregion
}