using APU.WebApp.Services.SignalRHub;

namespace APU.WebApp.Utils.Extensions;

public static class LoggerExtensions
{
    public static void LogLatency(this ILogger logger, User liu, long actualLatency, double avgLatency)
    {
        logger.LogInformation("UserId: {userId}, Name: {userName}, Actual Latency: {actualLatency}, Average Latency: {avgLatency}", liu?.Id.ToString(), liu?.Name, actualLatency.ToString("0") + "ms", avgLatency.ToString("0") + "ms");
    }


    public static void LogSignalRHubStatus(this ILogger logger, User liu, string hubClientName, string hubId, HubState hubState)
    {
        logger.LogInformation("UserId: {userId}, Name: {userName}, {hubClientName} ({hubId}) - {hubState}", liu?.Id.ToString(), liu?.Name, hubClientName, hubId, hubState);
    }

    public static void LogSignalRHubError(this ILogger logger, User liu, string hubClientName, string hubId, HubState hubState, Exception e)
    {
        logger.LogError(e, "UserId: {userId}, Name: {userName}, {hubClientName} ({hubId}) - {hubState}", liu?.Id.ToString(), liu?.Name, hubClientName, hubId, hubState);
    }

    public static void LogSignalRHubMessage(this ILogger logger, User liu, string hubClientName, string hubId, string method, string message)
    {
        logger.LogInformation("UserId: {userId}, Name: {userName}, {hubClientName} ({hubId}) - {method} - {message}", liu?.Id.ToString(), liu?.Name, hubClientName, hubId, method, message);
    }


}

