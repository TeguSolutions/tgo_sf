namespace APU.WebApp.Pages.Application.PageEstimate;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class EstimateVM : PageVMBase
{
    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderEstimate));
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion
}