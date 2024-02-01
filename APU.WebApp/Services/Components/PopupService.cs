namespace APU.WebApp.Services.Components;

public class PopupService
{
    private readonly List<IPopup> popups;

    public PopupService()
    {
        popups = new List<IPopup>();
    }

    public void Add(IPopup popup)
    {
        popups.Add(popup);
    }

    public void Remove(IPopup popup)
    {
        popups.Remove(popup);
    }

    public void ClosePopups(IPopup sourcePopup)
    {
        foreach (var popup in popups)
        {
            if (popup != sourcePopup)
                popup.ClosePopup();
        }
    }
}