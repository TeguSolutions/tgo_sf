namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

public class ProjectExportVM : PageVMBase
{
    private Guid? projectId;

    #region Lifecycle

    protected override void OnInitialized()
    {
        #region Query Param

        if (NavM.TryGetQueryString<string>("projectid", out var queryPrev))
        {
            try
            {
                projectId = Guid.Parse(queryPrev);
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
            if (projectId is null)
                return;

            var defaultResult = await DefaultRepo.GetAsync();
            if (!defaultResult.IsSuccess())
            {
                return;
            }

            //var result = await ProjectRepo.GetAsync(projectId.Value, includeApu: true);
            var result = await ProjectRepo.GetLineItemsAsync(projectId.Value);
            if (!result.IsSuccess())
            {
                return;
            }

            Project = result.Data;
            var calculationResult = Project.Calculate(defaultResult.Data);
            if (!calculationResult.IsSuccess())
                ShowError(calculationResult.Message);

            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    internal Project Project { get; set; }
}