using APU.WebApp.Services.SignalRHub;

namespace APU.WebApp.Utils.EventAggregatorMessages.SignalRHubMessages;

public class SignalRBaseItemHubStatusMessage
{
    public SignalRBaseItemHubStatusMessage(HubState hubState)
    {
        HubState = hubState;
    }

    public HubState HubState { get; }
}