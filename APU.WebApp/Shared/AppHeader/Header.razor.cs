using System.Collections.ObjectModel;
using System.Timers;
using APU.WebApp.Services.Latency;
using APU.WebApp.Services.SignalRHub;
using APU.WebApp.Utils.EventAggregatorMessages.SignalRHubMessages;
using Timer = System.Timers.Timer;

namespace APU.WebApp.Shared.AppHeader;

[Authorize]
public class HeaderVM : PageVMBase, IHandle<HeaderMessage>, IHandle<ProgressMessage>, 
	IHandle<SignalRProjectHubStatusMessage>, IHandle<SignalRApuHubStatusMessage>, 
    IHandle<SignalRBaseItemHubStatusMessage>, IHandle<SignalRProjectScheduleHubStatusMessage>,

IDisposable

{
    [Inject]
    public PingService PingService { get; set; }

    [Inject]
    public LatencyService LatencyService { get; set; }


    #region Lifecycle

    protected override void OnInitialized()
    {
        ProjectHubStatus = new HubStatus(StateChanged);
        MainHub.ProjectHubClientCallbacks.Add(ProjectHubStatus.UpdateHubUsers);

        ApuHubStatus = new HubStatus(StateChanged);
        MainHub.ApuHubClientCallbacks.Add(ApuHubStatus.UpdateHubUsers);

        BaseItemHubStatus = new HubStatus(StateChanged);
        MainHub.BaseItemHubClientCallbacks.Add(BaseItemHubStatus.UpdateHubUsers);

        ProjectScheduleHubStatus = new HubStatus(StateChanged);
        MainHub.ProjectScheduleHubClientCallbacks.Add(ProjectScheduleHubStatus.UpdateHubUsers);

	    base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (!IsFirstParameterSet)
        {
            EventAggregator.Subscribe(this);
            IsFirstParameterSet = true;
        }

        base.OnParametersSet();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetLIU();
            PingService.Start(LatencyService, Liu);

            _latencyTimer = new Timer(10000);
            _latencyTimer.Elapsed += LatencyTimerOnElapsed;
            _latencyTimer.Start();
            LatencyTimerOnElapsed(null, null);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region Header Management

    internal Type HeaderType { get; set; }

    public Task HandleAsync(HeaderMessage message)
    {
        HeaderType = message.HeaderType;
        StateHasChanged();
        return Task.CompletedTask;
    }    

    #endregion

    #region SignalR Hub Statuses

    internal HubStatus ProjectHubStatus { get; set; }

    internal HubStatus ApuHubStatus { get; set; }

    internal HubStatus BaseItemHubStatus { get; set; }

    internal HubStatus ProjectScheduleHubStatus { get; set; }

    public Task HandleAsync(SignalRProjectHubStatusMessage message)
    {
        ProjectHubStatus.SetHubState(message.HubState);
        return Task.CompletedTask;
    }

    public Task HandleAsync(SignalRApuHubStatusMessage message)
    {
        ApuHubStatus.SetHubState(message.HubState);
        return Task.CompletedTask;
    }

    public Task HandleAsync(SignalRBaseItemHubStatusMessage message)
    {
        BaseItemHubStatus.SetHubState(message.HubState);
        return Task.CompletedTask;
    }

    public Task HandleAsync(SignalRProjectScheduleHubStatusMessage message)
    {
        ProjectScheduleHubStatus.SetHubState(message.HubState);
        return Task.CompletedTask;
    }

    #endregion

    #region Latency

    private Timer _latencyTimer;

    internal ObservableCollection<LatencyService.LatencyClass> UserLatencies { get; set; } = new();

    private void LatencyTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        UserLatencies = LatencyService.GetUserLatencies();
        StateChanged();
    }

    #endregion

    #region Progress Management

    internal string ProgressCss { get; set; }

    public async Task HandleAsync(ProgressMessage message)
    {
        ProgressCss = message.IsRunning ? "h-progress-show" : "";
        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Logout

    internal async void Logout()
    {
        await CASP.MarkUserAsLoggedOut();
        StateHasChanged();
    }    

    #endregion


    #region Helpers

    private async void StateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        EventAggregator.Unsubscribe(this);

        MainHub.ProjectHubClientCallbacks.Remove(ProjectHubStatus.UpdateHubUsers);
        MainHub.ApuHubClientCallbacks.Remove(ApuHubStatus.UpdateHubUsers);
        MainHub.BaseItemHubClientCallbacks.Remove(BaseItemHubStatus.UpdateHubUsers);
        MainHub.ProjectScheduleHubClientCallbacks.Remove(ProjectScheduleHubStatus.UpdateHubUsers);

        _latencyTimer?.Stop();
        _latencyTimer?.Dispose();
        _latencyTimer = null;
    }    

    #endregion
}