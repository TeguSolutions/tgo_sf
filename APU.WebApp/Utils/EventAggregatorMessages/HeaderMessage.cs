namespace APU.WebApp.Utils.EventAggregatorMessages;

public class HeaderMessage
{
    public HeaderMessage(Type headerType)
    {
        HeaderType = headerType;
    }

    public Type HeaderType { get; }
}