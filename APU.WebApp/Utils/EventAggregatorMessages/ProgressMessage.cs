namespace APU.WebApp.Utils.EventAggregatorMessages;

public class ProgressMessage
{
    public ProgressMessage(bool isRunning)
    {
        IsRunning = isRunning;
    }

    public bool IsRunning { get; }
}