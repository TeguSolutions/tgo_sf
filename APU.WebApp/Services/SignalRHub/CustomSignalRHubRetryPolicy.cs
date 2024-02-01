using Microsoft.AspNetCore.SignalR.Client;

namespace APU.WebApp.Services.SignalRHub;

public class CustomSignalRHubRetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return TimeSpan.FromSeconds(5);
    }
}