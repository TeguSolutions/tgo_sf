using APU.WebApp.Services.Authentication;
using APU.WebApp.Services.SignalRHub.Messages;
using APU.WebApp.Utils;
using APU.WebApp.Utils.EventAggregatorMessages.SignalRHubMessages;
using Microsoft.AspNetCore.SignalR.Client;

namespace APU.WebApp.Services.SignalRHub.HubClients.ApuHub;

public class ApuHubClient : IDisposable
{
    private readonly ILogger<ApuHubClient> _logger;
    private readonly IEventAggregator _eventAggregator;

    private readonly string hubName;
    private string hubId;

    private User liu;
    private string _component;

    #region Lifecycle

    public ApuHubClient(ILogger<ApuHubClient> logger, IEventAggregator eventAggregator, JwtTokenService jwt)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

        InitializeLiu(jwt);

        hubName = nameof(ApuHubClient);
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

    public async void StartHub(NavigationManager Navigation, string component)
    {
        _component = component;

        try
        {
            var url = Navigation.BaseUri.TrimEnd('/') + MainHub.Url;
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

            hubConnection.On<ApuHubApuCreatedMessage>(nameof(MainHub.ApuCreated), ReceiveApuCreated);
            hubConnection.On<ApuHubApuCreatedMultipleLineItemsMessage>(nameof(MainHub.ApuCreatedMultipleLineItems), ReceiveApuCreatedMultipleLineItems);
            hubConnection.On<ApuHubApuUpdatedMessage>(nameof(MainHub.ApuUpdated), ReceiveApuUpdated);
            hubConnection.On<ApuHubApuUpdatedMultipleMessage>(nameof(MainHub.ApuUpdatedMultiple), ReceiveApuUpdatedMultiple);
            hubConnection.On<ApuHubApuDeletedMessage>(nameof(MainHub.ApuDeleted), ReceiveApuDeleted);

            await hubConnection.StartAsync();

            hubId = hubConnection.ConnectionId;

            await hubConnection.SendAsync(nameof(MainHub.ApuHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Connected, _component, liu.Id, liu.Name));

            await _eventAggregator.PublishAsync(new SignalRApuHubStatusMessage(HubState.Connected));
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

		    await _eventAggregator.PublishAsync(new SignalRApuHubStatusMessage(HubState.Stopped));
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
        hubConnection.SendAsync(nameof(MainHub.ApuHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Closed, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Closed);

        _eventAggregator.PublishAsync(new SignalRApuHubStatusMessage(HubState.Closed));
        return Task.CompletedTask;
    }
    private Task HubConnectionOnReconnecting(Exception arg)
    {
        hubConnection.SendAsync(nameof(MainHub.ApuHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Reconnecting, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Reconnecting);

        _eventAggregator.PublishAsync(new SignalRApuHubStatusMessage(HubState.Reconnecting));
        return Task.CompletedTask;
    }
    private Task HubConnectionOnReconnected(string arg)
    {
        hubConnection.SendAsync(nameof(MainHub.ApuHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Reconnected, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Reconnected);

        _eventAggregator.PublishAsync(new SignalRApuHubStatusMessage(HubState.Reconnected));
        return Task.CompletedTask;
    }

    #endregion

    #region Hub Event - ApuCreated

    public async void SendApuCreate(ApuHubApuCreatedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.ApuCreated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendApuCreate), JsonSerializer.Serialize(message));
    }

    private void ReceiveApuCreated(ApuHubApuCreatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveApuCreated), JsonSerializer.Serialize(message));
        ApuCreated?.Invoke(message);
    }

    public Action<ApuHubApuCreatedMessage> ApuCreated { get; set; }

    #endregion
    #region Hub Event - ApuCreatedMultipleLineItems

    public async void SendApuCreatedMultipleLineItems(ApuHubApuCreatedMultipleLineItemsMessage message)
    {
	    await hubConnection.SendAsync(nameof(MainHub.ApuCreatedMultipleLineItems), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendApuCreatedMultipleLineItems), JsonSerializer.Serialize(message));
    }

    private void ReceiveApuCreatedMultipleLineItems(ApuHubApuCreatedMultipleLineItemsMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveApuCreatedMultipleLineItems), JsonSerializer.Serialize(message));
        ApuCreatedMultipleLineItems.Invoke(message);
    }

    public Action<ApuHubApuCreatedMultipleLineItemsMessage> ApuCreatedMultipleLineItems { get; set; }

    #endregion
    #region Hub Event - ApuUpdate

    public async void SendApuUpdate(ApuHubApuUpdatedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.ApuUpdated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendApuUpdate), JsonSerializer.Serialize(message));
    }

    private void ReceiveApuUpdated(ApuHubApuUpdatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveApuUpdated), JsonSerializer.Serialize(message));
        ApuUpdated?.Invoke(message);
    }

    public Action<ApuHubApuUpdatedMessage> ApuUpdated { get; set; }

    #endregion
    #region Hub Event - ApuUpdateMultiple

    public async void SendApuUpdateMultiple(ApuHubApuUpdatedMultipleMessage message)
    {
	    await hubConnection.SendAsync(nameof(MainHub.ApuUpdatedMultiple), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendApuUpdateMultiple), JsonSerializer.Serialize(message));
    }

    private void ReceiveApuUpdatedMultiple(ApuHubApuUpdatedMultipleMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveApuUpdatedMultiple), JsonSerializer.Serialize(message));
        ApuUpdatedMultiple?.Invoke(message);
    }

    public Action<ApuHubApuUpdatedMultipleMessage> ApuUpdatedMultiple { get; set; }

    #endregion
    #region Hub Event - ApuDeleted

    public async void SendApuDelete(ApuHubApuDeletedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.ApuDeleted), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendApuDelete), JsonSerializer.Serialize(message));
    }

    private void ReceiveApuDeleted(ApuHubApuDeletedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveApuDeleted), JsonSerializer.Serialize(message));
        ApuDeleted?.Invoke(message);
    }

    public Action<ApuHubApuDeletedMessage> ApuDeleted { get; set; }

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