using APU.WebApp.Services.SignalRHub;

namespace APU.WebApp.Utils.EventAggregatorMessages.SignalRHubMessages;

public class SignalRProjectHubStatusMessage
{
    public SignalRProjectHubStatusMessage(HubState hubState)
    {
        HubState = hubState;
    }

    public HubState HubState { get; }
}