using APU.WebApp.Services.SignalRHub;

namespace APU.WebApp.Utils.EventAggregatorMessages.SignalRHubMessages;

public class SignalRProjectScheduleHubStatusMessage
{
    public SignalRProjectScheduleHubStatusMessage(HubState hubState)
    {
        HubState = hubState;
    }

    public HubState HubState { get; }
}