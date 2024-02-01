using APU.WebApp.Services.SignalRHub;

namespace APU.WebApp.Utils.EventAggregatorMessages.SignalRHubMessages;

public class SignalRApuHubStatusMessage
{
    public SignalRApuHubStatusMessage(HubState hubState)
    {
        HubState = hubState;
    }

    public HubState HubState { get; }
}