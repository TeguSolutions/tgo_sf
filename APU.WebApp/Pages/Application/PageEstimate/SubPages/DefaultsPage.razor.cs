namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class DefaultsPageVM : PageVMBase
{
    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {           
            SendHeaderMessage(typeof(HeaderEstimate));

            await GetLIU();

            await GetDefaultValues();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region Default Values

    internal DefaultValue Default { get; set; }

    private async Task GetDefaultValues()
    {
        ProgressStart();

        var result = await DefaultRepo.GetAsync();
        if (!result.IsSuccess())
        {
            ShowError("Failed to collect Default Value!");
            return;
        }

        Default = result.Data;

        ProgressStop();
    }

    internal async void UpdateDefaults()
    {
        ProgressStart();

        var result = await DefaultRepo.UpdateAsync(Default, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Default Value!");
            return;
        }

        Default = result.Data;

        ProgressStop();
    }

    #endregion
}