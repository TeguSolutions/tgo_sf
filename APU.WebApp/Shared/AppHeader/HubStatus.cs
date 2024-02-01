using APU.WebApp.Services.SignalRHub;
using Syncfusion.Blazor.Buttons;
using Action = System.Action;

namespace APU.WebApp.Shared.AppHeader;

public class HubStatus
{
    private readonly Action _stateChanged;

    public HubStatus(Action stateChanged)
    {
        _stateChanged = stateChanged;
        HubUsers = new List<HubUser>();
        SetHubState(HubState.Closed);
    }

    public HubState HubState { get; set; }

    public IconName IconName { get; set; }
    public string ColorCss { get; set; }

    public List<HubUser> HubUsers { get; set; }

    public void UpdateHubUsers(List<HubUser> hubUsers)
    {
        HubUsers = hubUsers;
        _stateChanged?.Invoke();
    }

    internal void SetHubState(HubState state)
    {
        HubState = state;

        if (HubState is HubState.Connected or HubState.Reconnected)
        {
            IconName = IconName.CircleCheck;
            ColorCss = "clr-green";
        }
        else if (HubState is HubState.Closed or HubState.Stopped)
        {
            IconName = IconName.CircleClose;
            ColorCss = "clr-gray";
        }
        else if (HubState is HubState.Reconnecting)
        {
            IconName = IconName.CircleRemove;
            ColorCss = "clr-orange";
        }

        _stateChanged?.Invoke();
    }
}