namespace APU.WebApp.Services.SignalRHub;

public enum HubState
{
    Stopped = 0,
    Connected = 1,
    Reconnecting = 2,
    Reconnected = 3,
    Closed = 4,
    ConnectionFailed = 5
}