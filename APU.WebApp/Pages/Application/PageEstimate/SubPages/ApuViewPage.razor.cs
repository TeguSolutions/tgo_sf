namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

public class ApuViewVM : PageVMBase
{
    private Guid? apuId;

    #region Lifecycle

    protected override void OnInitialized()
    {
        #region Query Param

        if (NavM.TryGetQueryString<string>("view", out var queryPrev))
        {
            try
            {
                apuId = Guid.Parse(queryPrev);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        #endregion

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderEstimate));

            if (apuId is null)
                return;

            ProgressStart();

            var defaultResult = await DefaultRepo.GetAsync();
            if (!defaultResult.IsSuccess())
            {
                ShowError("Default Values loading failed!", false);
            }

            var result = await ApuRepo.GetAsync(apuId.Value, includeApuItems: true, includeProject: true);
            if (!result.IsSuccess())
            {
                ShowError("Apu loading failed!");
                return;
            }

            Apu = result.Data;
            Project = result.Data.Project;

            Apu.CalculateAll(defaultResult.Data, Project);

            ProgressStop();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    internal Project Project { get; set; }

    internal Apu Apu { get; set; }
}