using APU.WebApp.Services.Authentication;
using APU.WebApp.Services.SignalRHub.Messages;
using APU.WebApp.Utils;
using APU.WebApp.Utils.EventAggregatorMessages.SignalRHubMessages;
using Microsoft.AspNetCore.SignalR.Client;

namespace APU.WebApp.Services.SignalRHub.HubClients.BaseItemHub;

public class BaseItemHubClient : IDisposable
{
    private readonly ILogger<BaseItemHubClient> _logger;
    private readonly IEventAggregator _eventAggregator;

    private readonly string hubName;
    private string hubId;

    private User liu;
    private string _component;

    #region Lifecycle

    public BaseItemHubClient(ILogger<BaseItemHubClient> logger, IEventAggregator eventAggregator, JwtTokenService jwt)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

        InitializeLiu(jwt);

        hubName = nameof(BaseItemHubClient);
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

            hubConnection.On<BaseItemHubPerformanceCreatedMessage>(nameof(MainHub.BaseItemPerformanceCreated), ReceivePerformanceCreated);
            hubConnection.On<BaseItemHubPerformanceUpdatedMessage>(nameof(MainHub.BaseItemPerformanceUpdated), ReceivePerformanceUpdated);
            hubConnection.On<BaseItemHubPerformanceDeletedMessage>(nameof(MainHub.BaseItemPerformanceDeleted), ReceivePerformanceDeleted);

            hubConnection.On<BaseItemHubLaborCreatedMessage>(nameof(MainHub.BaseItemLaborCreated), ReceiveLaborCreated);
            hubConnection.On<BaseItemHubLaborUpdatedMessage>(nameof(MainHub.BaseItemLaborUpdated), ReceiveLaborUpdated);
            hubConnection.On<BaseItemHubLaborDeletedMessage>(nameof(MainHub.BaseItemLaborDeleted), ReceiveLaborDeleted);

            hubConnection.On<BaseItemHubMaterialCreatedMessage>(nameof(MainHub.BaseItemMaterialCreated), ReceiveMaterialCreated);
            hubConnection.On<BaseItemHubMaterialUpdatedMessage>(nameof(MainHub.BaseItemMaterialUpdated), ReceiveMaterialUpdated);
            hubConnection.On<BaseItemHubMaterialDeletedMessage>(nameof(MainHub.BaseItemMaterialDeleted), ReceiveMaterialDeleted);

            hubConnection.On<BaseItemHubEquipmentCreatedMessage>(nameof(MainHub.BaseItemEquipmentCreated), ReceiveEquipmentCreated);
            hubConnection.On<BaseItemHubEquipmentUpdatedMessage>(nameof(MainHub.BaseItemEquipmentUpdated), ReceiveEquipmentUpdated);
            hubConnection.On<BaseItemHubEquipmentDeletedMessage>(nameof(MainHub.BaseItemEquipmentDeleted), ReceiveEquipmentDeleted);

            hubConnection.On<BaseItemHubContractCreatedMessage>(nameof(MainHub.BaseItemContractCreated), ReceiveContractCreated);
            hubConnection.On<BaseItemHubContractUpdatedMessage>(nameof(MainHub.BaseItemContractUpdated), ReceiveContractUpdated);
            hubConnection.On<BaseItemHubContractDeletedMessage>(nameof(MainHub.BaseItemContractDeleted), ReceiveContractDeleted);

            await hubConnection.StartAsync();

            hubId = hubConnection.ConnectionId;

            await hubConnection.SendAsync(nameof(MainHub.BaseItemHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Connected, _component, liu.Id, liu.Name));

            await _eventAggregator.PublishAsync(new SignalRBaseItemHubStatusMessage(HubState.Connected));
            _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Connected);
        }
        catch (Exception e)
        {
            _logger.LogSignalRHubError(liu, hubName, hubId, HubState.ConnectionFailed, e);
        }
    }

    private async void StopHub()
    {
	    try
	    {
            if (hubConnection is null)
                return;

		    await _eventAggregator.PublishAsync(new SignalRBaseItemHubStatusMessage(HubState.Stopped));
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
        hubConnection.SendAsync(nameof(MainHub.BaseItemHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Closed, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Closed);

        _eventAggregator.PublishAsync(new SignalRBaseItemHubStatusMessage(HubState.Closed));
        return Task.CompletedTask;
    }
    private Task HubConnectionOnReconnecting(Exception arg)
    {
        hubConnection.SendAsync(nameof(MainHub.BaseItemHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Reconnecting, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Reconnecting);

        _eventAggregator.PublishAsync(new SignalRBaseItemHubStatusMessage(HubState.Reconnecting));
        return Task.CompletedTask;
    }
    private Task HubConnectionOnReconnected(string arg)
    {
        hubConnection.SendAsync(nameof(MainHub.BaseItemHubUserConnectionChanged), new MainHubUserConnectionStateMessage(HubState.Reconnected, _component, liu.Id, liu.Name));
        _logger.LogSignalRHubStatus(liu, hubName, hubId, HubState.Reconnected);

        _eventAggregator.PublishAsync(new SignalRBaseItemHubStatusMessage(HubState.Reconnected));
        return Task.CompletedTask;
    }

    #endregion

    #region Hub Event - Base Performance Created

    public async void SendPerformanceCreate(BaseItemHubPerformanceCreatedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.BaseItemPerformanceCreated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendPerformanceCreate), JsonSerializer.Serialize(message));
    }

    private void ReceivePerformanceCreated(BaseItemHubPerformanceCreatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceivePerformanceCreated), JsonSerializer.Serialize(message));
        PerformanceCreated?.Invoke(message);
    }

    public Action<BaseItemHubPerformanceCreatedMessage> PerformanceCreated { get; set; }

    #endregion
    #region Hub Event - Base Performance Updated

    public async void SendPerformanceUpdate(BaseItemHubPerformanceUpdatedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.BaseItemPerformanceUpdated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendPerformanceUpdate), JsonSerializer.Serialize(message));
    }

    private void ReceivePerformanceUpdated(BaseItemHubPerformanceUpdatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceivePerformanceUpdated), JsonSerializer.Serialize(message));
        PerformanceUpdated?.Invoke(message);
    }

    public Action<BaseItemHubPerformanceUpdatedMessage> PerformanceUpdated { get; set; }

    #endregion
    #region Hub Event - Base Performance Deleted

    public async void SendPerformanceDelete(BaseItemHubPerformanceDeletedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.BaseItemPerformanceDeleted), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendPerformanceDelete), JsonSerializer.Serialize(message));
    }

    private void ReceivePerformanceDeleted(BaseItemHubPerformanceDeletedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceivePerformanceDeleted), JsonSerializer.Serialize(message));
        PerformanceDeleted?.Invoke(message);
    }

    public Action<BaseItemHubPerformanceDeletedMessage> PerformanceDeleted { get; set; }

    #endregion

    #region Hub Event - Base Labor Created

    public async void SendLaborCreate(BaseItemHubLaborCreatedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.BaseItemLaborCreated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendLaborCreate), JsonSerializer.Serialize(message));
    }

    private void ReceiveLaborCreated(BaseItemHubLaborCreatedMessage message)
	{
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveLaborCreated), JsonSerializer.Serialize(message));
        LaborCreated?.Invoke(message);
    }

    public Action<BaseItemHubLaborCreatedMessage> LaborCreated { get; set; }

    #endregion
    #region Hub Event - Base Labor Updated

    public async void SendLaborUpdate(BaseItemHubLaborUpdatedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.BaseItemLaborUpdated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendLaborUpdate), JsonSerializer.Serialize(message));
    }

    private void ReceiveLaborUpdated(BaseItemHubLaborUpdatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveLaborUpdated), JsonSerializer.Serialize(message));
        LaborUpdated?.Invoke(message);
    }

    public Action<BaseItemHubLaborUpdatedMessage> LaborUpdated { get; set; }

    #endregion
    #region Hub Event - Base Labor Deleted

    public async void SendLaborDelete(BaseItemHubLaborDeletedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.BaseItemLaborDeleted), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendLaborDelete), JsonSerializer.Serialize(message));
    }

    private void ReceiveLaborDeleted(BaseItemHubLaborDeletedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveLaborDeleted), JsonSerializer.Serialize(message));
        LaborDeleted?.Invoke(message);
    }

    public Action<BaseItemHubLaborDeletedMessage> LaborDeleted { get; set; }

    #endregion

    #region Hub Event - Base Material Created

    public async void SendMaterialCreate(BaseItemHubMaterialCreatedMessage message)
    {
        await hubConnection.SendAsync(nameof(MainHub.BaseItemMaterialCreated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendMaterialCreate), JsonSerializer.Serialize(message));
    }

    private void ReceiveMaterialCreated(BaseItemHubMaterialCreatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveMaterialCreated), JsonSerializer.Serialize(message));
        MaterialCreated?.Invoke(message);
    }

    public Action<BaseItemHubMaterialCreatedMessage> MaterialCreated { get; set; }

    #endregion
    #region Hub Event - Base Material Updated

    public async void SendMaterialUpdate(BaseItemHubMaterialUpdatedMessage message)
    {
	    await hubConnection.SendAsync(nameof(MainHub.BaseItemMaterialUpdated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendMaterialUpdate), JsonSerializer.Serialize(message));
    }

    private void ReceiveMaterialUpdated(BaseItemHubMaterialUpdatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveMaterialUpdated), JsonSerializer.Serialize(message));
        MaterialUpdated?.Invoke(message);
    }

    public Action<BaseItemHubMaterialUpdatedMessage> MaterialUpdated { get; set; }

    #endregion
    #region Hub Event - Base Material Deleted

    public async void SendMaterialDelete(BaseItemHubMaterialDeletedMessage message)
    {
	    await hubConnection.SendAsync(nameof(MainHub.BaseItemMaterialDeleted), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendMaterialDelete), JsonSerializer.Serialize(message));
    }

    private void ReceiveMaterialDeleted(BaseItemHubMaterialDeletedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveMaterialDeleted), JsonSerializer.Serialize(message));
        MaterialDeleted?.Invoke(message);
    }

    public Action<BaseItemHubMaterialDeletedMessage> MaterialDeleted { get; set; }

    #endregion

    #region Hub Event - Base Equipment Created

    public async void SendEquipmentCreate(BaseItemHubEquipmentCreatedMessage message)
    {
	    await hubConnection.SendAsync(nameof(MainHub.BaseItemEquipmentCreated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendEquipmentCreate), JsonSerializer.Serialize(message));
    }

    private void ReceiveEquipmentCreated(BaseItemHubEquipmentCreatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveEquipmentCreated), JsonSerializer.Serialize(message));
        EquipmentCreated?.Invoke(message);
    }

    public Action<BaseItemHubEquipmentCreatedMessage> EquipmentCreated { get; set; }

    #endregion
    #region Hub Event - Base Equipment Updated

    public async void SendEquipmentUpdate(BaseItemHubEquipmentUpdatedMessage message)
    {
	    await hubConnection.SendAsync(nameof(MainHub.BaseItemEquipmentUpdated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendEquipmentUpdate), JsonSerializer.Serialize(message));
    }

    private void ReceiveEquipmentUpdated(BaseItemHubEquipmentUpdatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveEquipmentUpdated), JsonSerializer.Serialize(message));
        EquipmentUpdated?.Invoke(message);
    }

    public Action<BaseItemHubEquipmentUpdatedMessage> EquipmentUpdated { get; set; }

    #endregion
    #region Hub Event - Base Equipment Deleted

    public async void SendEquipmentDelete(BaseItemHubEquipmentDeletedMessage message)
    {
	    await hubConnection.SendAsync(nameof(MainHub.BaseItemEquipmentDeleted), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendEquipmentDelete), JsonSerializer.Serialize(message));
    }

    private void ReceiveEquipmentDeleted(BaseItemHubEquipmentDeletedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveEquipmentDeleted), JsonSerializer.Serialize(message));
        EquipmentDeleted?.Invoke(message);
    }

    public Action<BaseItemHubEquipmentDeletedMessage> EquipmentDeleted { get; set; }

    #endregion

    #region Hub Event - Base Contract Created

    public async void SendContractCreate(BaseItemHubContractCreatedMessage message)
    {
	    await hubConnection.SendAsync(nameof(MainHub.BaseItemContractCreated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendContractCreate), JsonSerializer.Serialize(message));
    }

    private void ReceiveContractCreated(BaseItemHubContractCreatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveContractCreated), JsonSerializer.Serialize(message));
        ContractCreated?.Invoke(message);
    }

    public Action<BaseItemHubContractCreatedMessage> ContractCreated { get; set; }

    #endregion
    #region Hub Event - Base Contract Updated

    public async void SendContractUpdate(BaseItemHubContractUpdatedMessage message)
    {
	    await hubConnection.SendAsync(nameof(MainHub.BaseItemContractUpdated), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendContractUpdate), JsonSerializer.Serialize(message));
    }

    private void ReceiveContractUpdated(BaseItemHubContractUpdatedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveContractUpdated), JsonSerializer.Serialize(message));
        ContractUpdated?.Invoke(message);
    }

    public Action<BaseItemHubContractUpdatedMessage> ContractUpdated { get; set; }

    #endregion
    #region Hub Event - Base Contract Deleted

    public async void SendContractDelete(BaseItemHubContractDeletedMessage message)
    {
	    await hubConnection.SendAsync(nameof(MainHub.BaseItemContractDeleted), message);
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(SendContractDelete), JsonSerializer.Serialize(message));
    }

    private void ReceiveContractDeleted(BaseItemHubContractDeletedMessage message)
    {
        _logger.LogSignalRHubMessage(liu, hubName, hubId, nameof(ReceiveContractDeleted), JsonSerializer.Serialize(message));
        ContractDeleted?.Invoke(message);
    }

    public Action<BaseItemHubContractDeletedMessage> ContractDeleted { get; set; }

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