using APU.WebApp.Services.Authentication;
using APU.WebApp.Services.SignalRHub.Messages;
using APU.WebApp.Utils.EventAggregatorMessages.SignalRHubMessages;
using APU.WebApp.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using APU.WebApp.Services.SignalRHub.HubClients.ProjectScheduleHub.Messages;

namespace APU.WebApp.Services.SignalRHub.HubClients.ProjectScheduleHub;

public class ProjectScheduleHubClient : IDisposable
{
    private readonly ILogger<ProjectScheduleHubClient> _logger;
    private readonly IEventAggregator _eventAggregator;

    private readonly string hubName;
    private string hubId;

    private User liu;
    private string _component;

    #region Lifecycle

    public ProjectScheduleHubClient(ILogger<ProjectScheduleHubClient> logger, IEventAggregator eventAggregator, JwtTokenService jwt)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

        InitializeLiu(jwt);

        hubName = nameof(ProjectScheduleHubClient);
    }

    private async void InitializeLiu(JwtTokenService jwt)
    {
        var (liuResult, userId, email, name, _, _) = await jwt.GetLoggedInUser();
        if (!liuResult.IsSuccess())
        {
            liu = new User();
            return;
        }

        liu = new User
        {
            Id = userId.Value,
            Email = email,
            Name = name,
        };
    }

    #endregion

    #region Hub Control

    private HubConnection hubConnection;

    public async void StartHub(NavigationManager navigation, string component)
    {
        _component = component;

        try
        {
            var url = navigation.BaseUri.TrimEnd('/') + MainHub.Url;
            hubConnection = new HubConnectionBuilder()
                .WithUrl(url, _ =>
                {

                })
                .WithAutomaticReconnect(new CustomSignalRHubRetryPolicy())
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions = Json.Options;
                })
                .Build();

            hubConnection.Closed += HubConnectionOnClosed;
            hubConnection.Reconnecting += HubConnectionOnReconnecting;
            hubConnection.Reconnected += HubConnectionOnReconnected;

            hubConnection.On<ProjectScheduleHubScheduleUpdatedMessage>(nameof(MainHub.ProjectScheduleUpdated), ReceiveScheduleUpdated);

            await hubConnection.StartAsync();

            hubId = hubConnection.ConnectionId;

            await hubConnection.SendAsync(nameof(MainHub.ProjectScheduleHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Connected, _component, liu.Id, liu.Name));

            await _eventAggregator.PublishAsync(new SignalRProjectScheduleHubStatusMessage(HubState.Connected));
            _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Connected);
        }
        catch (Exception e)
        {
            _logger.LogSignalRHubError(liu, hubName, hubId, HubState.ConnectionFailed, e);
        }
    }

    public async void StopHub()
    {
	    try
	    {
            if (hubConnection is null)
                return;

		    await _eventAggregator.PublishAsync(new SignalRProjectScheduleHubStatusMessage(HubState.Stopped));
            _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Stopped);

            await hubConnection.StopAsync();
            await hubConnection.DisposeAsync();
            hubConnection = null;
	    }
	    catch (Exception e)
	    {
            Console.WriteLine(e);
	    }
    }

    #endregion

    #region Hub Connection State Events

    private Task HubConnectionOnClosed(Exception arg)
    {
        hubConnection.SendAsync(nameof(MainHub.ProjectScheduleHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Closed, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Closed);

        _eventAggregator.PublishAsync(new SignalRProjectScheduleHubStatusMessage(HubState.Closed));
        return Task.CompletedTask;
    }
    private Task HubConnectionOnReconnecting(Exception arg)
    {
        hubConnection.SendAsync(nameof(MainHub.ProjectScheduleHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Reconnecting, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Reconnecting);

        _eventAggregator.PublishAsync(new SignalRProjectScheduleHubStatusMessage(HubState.Reconnecting));
        return Task.CompletedTask;
    }
    private Task HubConnectionOnReconnected(string arg)
    {
        hubConnection.SendAsync(nameof(MainHub.ProjectScheduleHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Reconnected, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Reconnected);

        _eventAggregator.PublishAsync(new SignalRProjectScheduleHubStatusMessage(HubState.Reconnected));
        return Task.CompletedTask;
    }

    #endregion

    #region Hub Event - Schedule Updated

    public async void SendScheduleUpdate(Guid projectId)
    {
        var message = new ProjectScheduleHubScheduleUpdatedMessage(projectId);
        await hubConnection.SendAsync(nameof(MainHub.ProjectScheduleUpdated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendScheduleUpdate), JsonSerializer.Serialize(message));
    }

    private void ReceiveScheduleUpdated(ProjectScheduleHubScheduleUpdatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveScheduleUpdated), JsonSerializer.Serialize(message));
        ScheduleUpdated?.Invoke(message);
    }

    public Action<ProjectScheduleHubScheduleUpdatedMessage> ScheduleUpdated { get; set; }

    #endregion




    #region IDisposable

    private bool isDisposed;

    public void Dispose()
    {
        if (isDisposed)
            return;
        isDisposed = true;

        StopHub();
    }

    #endregion
}