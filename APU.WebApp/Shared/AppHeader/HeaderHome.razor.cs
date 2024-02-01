using APU.WebApp.Services.Navigation;
using EventAggregator.Blazor;

namespace APU.WebApp.Shared.AppHeader;

[Authorize]
public class HeaderHomeVM : PageVMBase, IHandle<HeaderLinkMessage>
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        HighlightLink();

        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (!IsFirstParameterSet)
        {
            EventAggregator.Subscribe(this);
            IsFirstParameterSet = true;
        }

        base.OnParametersSet();
    }

    #endregion

    internal void Navigate(string url)
    {
        ResetAllLink();
        Dict[url] = "clr-orange";
    }

    #region Highlight logic

    internal Dictionary<string, string> Dict { get; } = new();

    private void HighlightLink()
    {
        ResetAllLink();

        if (NavM.Uri.EndsWith(NavS.Home.Municipalities))
            Dict[NavS.Home.Municipalities] = "clr-orange";

        if (NavM.Uri.EndsWith(NavS.Home.Certificates))
            Dict[NavS.Home.Certificates] = "clr-orange";

        if (NavM.Uri.EndsWith(NavS.Home.UserManager))
            Dict[NavS.Home.UserManager] = "clr-orange";

        StateHasChanged();
    }

    private void ResetAllLink()
    {
        Dict[NavS.Home.Municipalities] = "";
        Dict[NavS.Home.Certificates] = "";
        Dict[NavS.Home.UserManager] = "";
    }    

    #endregion

    #region Eventaggregator Message

    public Task HandleAsync(HeaderLinkMessage message)
    {
        HighlightLink();
        return Task.CompletedTask;
    }

    #endregion
}