using APU.WebApp.Services.Navigation;

namespace APU.WebApp.Shared.AppHeader;

[Authorize]
public class HeaderEstimateVM : PageVMBase, IHandle<HeaderLinkMessage>
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

        if (NavM.Uri.EndsWith(NavS.Estimates.Base))
            Dict[NavS.Estimates.Base] = "clr-orange";

        if (NavM.Uri.EndsWith(NavS.Estimates.APU))
            Dict[NavS.Estimates.APU] = "clr-orange";
        if (NavM.Uri.EndsWith(NavS.Estimates.Bids))
            Dict[NavS.Estimates.Bids] = "clr-orange";
        if (NavM.Uri.EndsWith(NavS.Estimates.Contracts))
            Dict[NavS.Estimates.Contracts] = "clr-orange";
        if (NavM.Uri.EndsWith(NavS.Estimates.Defaults))
            Dict[NavS.Estimates.Defaults] = "clr-orange";
        if (NavM.Uri.EndsWith(NavS.Estimates.Equipments))
            Dict[NavS.Estimates.Equipments] = "clr-orange";
        if (NavM.Uri.EndsWith(NavS.Estimates.Estimate))
            Dict[NavS.Estimates.Estimate] = "clr-orange";
        if (NavM.Uri.EndsWith(NavS.Estimates.Items))
            Dict[NavS.Estimates.Items] = "clr-orange";
        if (NavM.Uri.EndsWith(NavS.Estimates.Labor))
            Dict[NavS.Estimates.Labor] = "clr-orange";
        if (NavM.Uri.EndsWith(NavS.Estimates.Materials))
            Dict[NavS.Estimates.Materials] = "clr-orange";
        if (NavM.Uri.EndsWith(NavS.Estimates.Performance))
            Dict[NavS.Estimates.Performance] = "clr-orange";

        if (NavM.Uri.EndsWith(NavS.Estimates.Vendors))
            Dict[NavS.Estimates.Vendors] = "clr-orange";

        if (NavM.Uri.EndsWith(NavS.Estimates.Definitions))
            Dict[NavS.Estimates.Definitions] = "clr-orange";

        if (NavM.Uri.EndsWith(NavS.Estimates.Schedule))
            Dict[NavS.Estimates.Schedule] = "clr-orange";

        StateHasChanged();
    }

    private void ResetAllLink()
    {
        Dict[NavS.Estimates.Base] = "";

        Dict[NavS.Estimates.APU] = "";
        Dict[NavS.Estimates.Bids] = "";
        Dict[NavS.Estimates.Contracts] = "";
        Dict[NavS.Estimates.Defaults] = "";
        Dict[NavS.Estimates.Equipments] = "";
        Dict[NavS.Estimates.Estimate] = "";
        Dict[NavS.Estimates.Items] = "";
        Dict[NavS.Estimates.Labor] = "";
        Dict[NavS.Estimates.Materials] = "";
        Dict[NavS.Estimates.Performance] = "";
        Dict[NavS.Estimates.Vendors] = "";
        Dict[NavS.Estimates.Definitions] = "";
        Dict[NavS.Estimates.Schedule] = "";
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