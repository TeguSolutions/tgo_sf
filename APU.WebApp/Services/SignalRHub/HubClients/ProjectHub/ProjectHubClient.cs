using APU.WebApp.Services.Authentication;
using APU.WebApp.Services.SignalRHub.Messages;
using APU.WebApp.Utils;
using APU.WebApp.Utils.EventAggregatorMessages.SignalRHubMessages;
using Microsoft.AspNetCore.SignalR.Client;

namespace APU.WebApp.Services.SignalRHub.HubClients.ProjectHub;

public class ProjectHubClient : IDisposable
{
    private readonly ILogger<ProjectHubClient> _logger;
    private readonly IEventAggregator _eventAggregator;

    private readonly string hubName;
    private string hubId;

    private User liu;
    private string _component;

    #region Lifecycle

    public ProjectHubClient(ILogger<ProjectHubClient> logger, IEventAggregator eventAggregator, JwtTokenService jwt)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

        InitializeLiu(jwt);

        hubName = nameof(ProjectHubClient);
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

            hubConnection.On<ProjectHubProjectUpdatedMessage>(nameof(MainHub.ProjectUpdated), ReceiveProjectUpdated);
            hubConnection.On<ProjectHubProjectHasScheduleUpdatedMessage>(nameof(MainHub.ProjectHasScheduleUpdated), ReceiveProjectHasScheduleUpdated);

            await hubConnection.StartAsync();

            hubId = hubConnection.ConnectionId;

            await hubConnection.SendAsync(nameof(MainHub.ProjectHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Connected, _component, liu.Id, liu.Name));

            await _eventAggregator.PublishAsync(new SignalRProjectHubStatusMessage(HubState.Connected));
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

		    await _eventAggregator.PublishAsync(new SignalRProjectHubStatusMessage(HubState.Stopped));
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
        hubConnection.SendAsync(nameof(MainHub.ProjectHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Closed, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Closed);

        _eventAggregator.PublishAsync(new SignalRProjectHubStatusMessage(HubState.Closed));
        return Task.CompletedTask;
    }
    private Task HubConnectionOnReconnecting(Exception arg)
    {
        hubConnection.SendAsync(nameof(MainHub.ProjectHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Reconnecting, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Reconnecting);

        _eventAggregator.PublishAsync(new SignalRProjectHubStatusMessage(HubState.Reconnecting));
        return Task.CompletedTask;
    }
    private Task HubConnectionOnReconnected(string arg)
    {
        hubConnection.SendAsync(nameof(MainHub.ProjectHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Reconnected, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Reconnected);

        _eventAggregator.PublishAsync(new SignalRProjectHubStatusMessage(HubState.Reconnected));
        return Task.CompletedTask;
    }

    #endregion

    #region Hub Event - Project Updated

    public async void SendProjectUpdate(ProjectHubProjectUpdatedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.ProjectUpdated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendProjectUpdate), JsonSerializer.Serialize(message));
    }

    private void ReceiveProjectUpdated(ProjectHubProjectUpdatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveProjectUpdated), JsonSerializer.Serialize(message));
        ProjectUpdated?.Invoke(message);
    }

    public Action<ProjectHubProjectUpdatedMessage> ProjectUpdated { get; set; }

    #endregion

    #region Hub Event - Project HasSchedule Updated

    public async void SendProjectHasScheduleUpdate(Guid projectId, bool hasSchedule)
    {
        var message = new ProjectHubProjectHasScheduleUpdatedMessage(projectId, hasSchedule);
        await hubConnection.SendAsync(nameof(MainHub.ProjectHasScheduleUpdated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendProjectHasScheduleUpdate), JsonSerializer.Serialize(message));
    }

    private void ReceiveProjectHasScheduleUpdated(ProjectHubProjectHasScheduleUpdatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveProjectHasScheduleUpdated), JsonSerializer.Serialize(message));
        ProjectHasScheduleUpdated?.Invoke(message);
    }

    public Action<ProjectHubProjectHasScheduleUpdatedMessage> ProjectHasScheduleUpdated { get; set; }

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