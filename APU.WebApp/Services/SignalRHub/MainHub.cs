using APU.WebApp.Services.SignalRHub.HubClients.ProjectScheduleHub.Messages;
using APU.WebApp.Services.SignalRHub.Messages;
using Microsoft.AspNetCore.SignalR;

namespace APU.WebApp.Services.SignalRHub;

public class MainHub : Hub
{
    public const string Url = "/mainhub";

    private static readonly List<string> connectedClientIds = new();

    private readonly ILogger<MainHub> _logger;

    #region Lifecycle

    public MainHub(ILogger<MainHub> logger)
    {
	    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }    

    #endregion

    #region Hub Status

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("MainHub connected, ConnectionId: {ConnectionId}", Context.ConnectionId);
        
        connectedClientIds.Add(Context.ConnectionId);
        _logger.LogInformation("MainHub active connection ids: {activeConnectionIds}", JsonSerializer.Serialize(connectedClientIds));
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception e)
    {
	    _logger.LogInformation("MainHub disconnected, ConnectionId: {ConnectionId}, Exception: {exceptionMessage}", Context.ConnectionId, e?.ToString());
        
        connectedClientIds.Remove(Context.ConnectionId);

		ProjectHubUserConnectionChanged(new MainHubUserConnectionStateMessage(HubState.Closed, "", Guid.Empty, ""));
		ApuHubUserConnectionChanged(new MainHubUserConnectionStateMessage(HubState.Closed, "", Guid.Empty, ""));
		BaseItemHubUserConnectionChanged(new MainHubUserConnectionStateMessage(HubState.Closed, "", Guid.Empty, ""));
        ProjectScheduleHubUserConnectionChanged(new MainHubUserConnectionStateMessage(HubState.Closed, "", Guid.Empty, ""));

		await base.OnDisconnectedAsync(e);
    }

    #endregion

    #region User Connections
    
    private static readonly Dictionary<string, HubUser> projectHubUsers = new();
    public static List<Action<List<HubUser>>> ProjectHubClientCallbacks { get; set; } = new();
    public void ProjectHubUserConnectionChanged(MainHubUserConnectionStateMessage message)
    {
        if (message is null)
            return;

        if (message.HubState == HubState.Connected)
            projectHubUsers[Context.ConnectionId] = new HubUser(HubState.Connected, message.Component, message.UserId, message.UserName);
        else if (message.HubState == HubState.Closed)
            projectHubUsers.Remove(Context.ConnectionId);
        else if (message.HubState == HubState.Reconnecting)
            projectHubUsers[Context.ConnectionId] = new HubUser(HubState.Reconnecting, message.Component, message.UserId, message.UserName);
        else if (message.HubState == HubState.Reconnected)
            projectHubUsers[Context.ConnectionId] = new HubUser(HubState.Reconnected, message.Component, message.UserId, message.UserName);

        var hubUsers = projectHubUsers.Values.OrderBy(q => q.UserName).ThenBy(q => q.Component).ToList();
        foreach (var callback in ProjectHubClientCallbacks)
            callback.Invoke(hubUsers);
    }

    private static readonly Dictionary<string, HubUser> apuHubUsers = new();
    public static List<Action<List<HubUser>>> ApuHubClientCallbacks { get; set; } = new();
    public void ApuHubUserConnectionChanged(MainHubUserConnectionStateMessage message)
    {
        if (message is null)
            return;

        if (message.HubState == HubState.Connected)
            apuHubUsers[Context.ConnectionId] = new HubUser(HubState.Connected, message.Component, message.UserId, message.UserName);
        else if (message.HubState == HubState.Closed)
            apuHubUsers.Remove(Context.ConnectionId);
        else if (message.HubState == HubState.Reconnecting)
            apuHubUsers[Context.ConnectionId] = new HubUser(HubState.Reconnecting, message.Component, message.UserId, message.UserName);
        else if (message.HubState == HubState.Reconnected)
            apuHubUsers[Context.ConnectionId] = new HubUser(HubState.Reconnected, message.Component, message.UserId, message.UserName);

        var hubUsers = apuHubUsers.Values.OrderBy(q => q.UserName).ThenBy(q => q.Component).ToList();
        foreach (var callback in ApuHubClientCallbacks)
            callback.Invoke(hubUsers);
    }

    private static readonly Dictionary<string, HubUser> baseItemHubUsers = new();
    public static List<Action<List<HubUser>>> BaseItemHubClientCallbacks { get; set; } = new();
    public void BaseItemHubUserConnectionChanged(MainHubUserConnectionStateMessage message)
    {
        if (message is null)
            return;

        if (message.HubState == HubState.Connected)
            baseItemHubUsers[Context.ConnectionId] = new HubUser(HubState.Connected, message.Component, message.UserId, message.UserName);
        else if (message.HubState == HubState.Closed)
            baseItemHubUsers.Remove(Context.ConnectionId);
        else if (message.HubState == HubState.Reconnecting)
            baseItemHubUsers[Context.ConnectionId] = new HubUser(HubState.Reconnecting, message.Component, message.UserId, message.UserName);
        else if (message.HubState == HubState.Reconnected)
            baseItemHubUsers[Context.ConnectionId] = new HubUser(HubState.Reconnected, message.Component, message.UserId, message.UserName);

        var hubUsers = baseItemHubUsers.Values.OrderBy(q => q.UserName).ThenBy(q => q.Component).ToList();
        foreach (var callback in BaseItemHubClientCallbacks)
            callback.Invoke(hubUsers);
    }
    //public void BaseItemHubUserLatency(ClientHubLatencyMessage message)
    //{
	   // var diff = DateTimeOffset.UtcNow - message.start;
	   // baseItemHubUsers[Context.ConnectionId].Latency = diff.TotalMilliseconds;

	   // var hubUsers = baseItemHubUsers.Values.OrderBy(q => q.UserName).ThenBy(q => q.Component).ToList();
	   // foreach (var callback in BaseItemHubClientCallbacks)
		  //  callback.Invoke(hubUsers);
    //}

    private static readonly Dictionary<string, HubUser> projectScheduleHubUsers = new();
    public static List<Action<List<HubUser>>> ProjectScheduleHubClientCallbacks { get; set; } = new();
    public void ProjectScheduleHubUserConnectionChanged(MainHubUserConnectionStateMessage message)
    {
        if (message is null)
            return;

        if (message.HubState == HubState.Connected)
            projectScheduleHubUsers[Context.ConnectionId] = new HubUser(HubState.Connected, message.Component, message.UserId, message.UserName);
        else if (message.HubState == HubState.Closed)
            projectScheduleHubUsers.Remove(Context.ConnectionId);
        else if (message.HubState == HubState.Reconnecting)
            projectScheduleHubUsers[Context.ConnectionId] = new HubUser(HubState.Reconnecting, message.Component, message.UserId, message.UserName);
        else if (message.HubState == HubState.Reconnected)
            projectScheduleHubUsers[Context.ConnectionId] = new HubUser(HubState.Reconnected, message.Component, message.UserId, message.UserName);

        var hubUsers = projectScheduleHubUsers.Values.OrderBy(q => q.UserName).ThenBy(q => q.Component).ToList();
        foreach (var callback in ProjectScheduleHubClientCallbacks)
            callback.Invoke(hubUsers);
    }

    #endregion

    #region Apu Hub Client Methods

    public async Task ApuCreated(ApuHubApuCreatedMessage message)
    {
        await Clients.Others.SendAsync(nameof(ApuCreated), message);
    }
    public async Task ApuCreatedMultipleLineItems(ApuHubApuCreatedMultipleLineItemsMessage message)
    {
	    await Clients.Others.SendAsync(nameof(ApuCreatedMultipleLineItems), message);
    }
    public async Task ApuUpdated(ApuHubApuUpdatedMessage message)
    {
        await Clients.Others.SendAsync(nameof(ApuUpdated), message);
    }
    public async Task ApuUpdatedMultiple(ApuHubApuUpdatedMultipleMessage message)
    {
	    await Clients.Others.SendAsync(nameof(ApuUpdatedMultiple), message);
    }
    public async Task ApuDeleted(ApuHubApuDeletedMessage message)
    {
        await Clients.Others.SendAsync(nameof(ApuDeleted), message);
    }

    #endregion

    #region Project Hub Client Methods

    public async Task ProjectUpdated(ProjectHubProjectUpdatedMessage message)
    {
        await Clients.Others.SendAsync(nameof(ProjectUpdated), message);
    }

    public async Task ProjectHasScheduleUpdated(ProjectHubProjectHasScheduleUpdatedMessage message)
    {
        await Clients.Others.SendAsync(nameof(ProjectHasScheduleUpdated), message);
    }

    #endregion

    #region Project Schedule Hub Client Methods

    public async Task ProjectScheduleUpdated(ProjectScheduleHubScheduleUpdatedMessage message)
    {
        await Clients.Others.SendAsync(nameof(ProjectScheduleUpdated), message);
    }

    #endregion

    #region BaseItem Hub Client Methods

    public async Task BaseItemPerformanceCreated(BaseItemHubPerformanceCreatedMessage message)
    {
        await Clients.Others.SendAsync(nameof(BaseItemPerformanceCreated), message);
    }
    public async Task BaseItemPerformanceUpdated(BaseItemHubPerformanceUpdatedMessage message)
    {
        await Clients.Others.SendAsync(nameof(BaseItemPerformanceUpdated), message);
    }
    public async Task BaseItemPerformanceDeleted(BaseItemHubPerformanceDeletedMessage message)
    {
        await Clients.Others.SendAsync(nameof(BaseItemPerformanceDeleted), message);
    }

    public async Task BaseItemLaborCreated(BaseItemHubLaborCreatedMessage message)
    {
        await Clients.Others.SendAsync(nameof(BaseItemLaborCreated), message);
    }
    public async Task BaseItemLaborUpdated(BaseItemHubLaborUpdatedMessage message)
    {
        await Clients.Others.SendAsync(nameof(BaseItemLaborUpdated), message);
    }
    public async Task BaseItemLaborDeleted(BaseItemHubLaborDeletedMessage message)
    {
        await Clients.Others.SendAsync(nameof(BaseItemLaborDeleted), message);
    }

    public async Task BaseItemMaterialCreated(BaseItemHubMaterialCreatedMessage message)
    {
        await Clients.Others.SendAsync(nameof(BaseItemMaterialCreated), message);
    }
    public async Task BaseItemMaterialUpdated(BaseItemHubMaterialUpdatedMessage message)
    {
	    await Clients.Others.SendAsync(nameof(BaseItemMaterialUpdated), message);
    }
    public async Task BaseItemMaterialDeleted(BaseItemHubMaterialDeletedMessage message)
    {
	    await Clients.Others.SendAsync(nameof(BaseItemMaterialDeleted), message);
    }

    public async Task BaseItemEquipmentCreated(BaseItemHubEquipmentCreatedMessage message)
    {
	    await Clients.Others.SendAsync(nameof(BaseItemEquipmentCreated), message);
    }
    public async Task BaseItemEquipmentUpdated(BaseItemHubEquipmentUpdatedMessage message)
    {
	    await Clients.Others.SendAsync(nameof(BaseItemEquipmentUpdated), message);
    }
    public async Task BaseItemEquipmentDeleted(BaseItemHubEquipmentDeletedMessage message)
    {
	    await Clients.Others.SendAsync(nameof(BaseItemEquipmentDeleted), message);
    }

    public async Task BaseItemContractCreated(BaseItemHubContractCreatedMessage message)
    {
	    await Clients.Others.SendAsync(nameof(BaseItemContractCreated), message);
    }
    public async Task BaseItemContractUpdated(BaseItemHubContractUpdatedMessage message)
    {
	    await Clients.Others.SendAsync(nameof(BaseItemContractUpdated), message);
    }
    public async Task BaseItemContractDeleted(BaseItemHubContractDeletedMessage message)
    {
	    await Clients.Others.SendAsync(nameof(BaseItemContractDeleted), message);
    }

    #endregion
}