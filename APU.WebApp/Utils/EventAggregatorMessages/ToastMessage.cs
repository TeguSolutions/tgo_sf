using APU.WebApp.Utils.Definitions;

namespace APU.WebApp.Utils.EventAggregatorMessages;

public class ToastMessage
{
    public ToastMessage(NotificationType type, string title, string content)
    {
        Type = type;
        Title = title;
        Content = content;
    }

    public NotificationType Type { get; }
    public string Title { get; }
    public string Content { get; }
}