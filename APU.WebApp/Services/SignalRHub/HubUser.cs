namespace APU.WebApp.Services.SignalRHub;

public class HubUser
{
    public HubUser(HubState hubState, string component, Guid userId, string userName)
    {
        HubState = hubState;
        Component = component;
        UserId = userId;
        UserName = userName;
    }

    public HubState HubState { get; set; }

    public Guid UserId { get; set; }

    public string UserName { get; set; }

    public string Component { get; set; }

    public double Latency { get; set; }
}