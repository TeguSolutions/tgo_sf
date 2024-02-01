using System.Collections.ObjectModel;
using APU.DataV2.Context;
using APU.DataV2.EntityModels;
using APU.WebApp.Utils;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;

namespace APU.WebApp.Pages.DevTest;

[Authorize(Roles = RD.AdministratorText)]
public class DevToolsPageVM : PageVMBase
{
    [Inject]
    public IDbContextFactory<ApuDbContext> DbContextFactory { get; set; }

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetLIU();

            await GetAllProjectModels();
            GetFilteredProjectModels();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion


    internal string Message { get; set; }


    #region Projects

    private List<ProjectModel> allProjectModels;
    private async Task GetAllProjectModels()
    {
        ProgressStart();

        var result = await ProjectRepo.GetAllModelAsync();
        if (!result.IsSuccess())
        {
            allProjectModels = new List<ProjectModel>();
            ShowError("Failed to collect Projects!");
            return;
        }

        allProjectModels = result.Data.OrderBy(p => p.ProjectName).ToList();

        ProgressStop();
    }


    internal ObservableCollection<ProjectModel> FilteredProjectModels { get; set; } = new();
    private async void GetFilteredProjectModels(string projectFilter = null)
    {
        FilteredProjectModels = allProjectModels
            .If(!string.IsNullOrWhiteSpace(projectFilter), q => 
                q.Where(o =>
                    o.ProjectName.ToLower().Contains(projectFilter?.ToLower() ?? string.Empty) ||
                    TeguStringComparer.CompareToFilterBool(o.ProjectName, projectFilter))
            )
            //.OrderBy(p => p.ProjectName)
            .ToObservableCollection();

        await InvokeAsync(SfCbProjectFilter.RefreshDataAsync);
        await InvokeAsync(StateHasChanged);
    }

    internal SfComboBox<Guid?, ProjectModel> SfCbProjectFilter { get; set; }
    internal async void ProjectFilterChanged(SfDropDownFilteringEventArgs args)
    {
        args.PreventDefaultAction = true;
        GetFilteredProjectModels(args.Text);

        var query = new Query();
        await SfCbProjectFilter.FilterAsync(FilteredProjectModels, query);
    }

    internal Guid? SelectedProjectId { get; set; }

    public Project SelectedProject { get; set; }

    internal void SelectedProjectCBChanged(ChangeEventArgs<Guid?, ProjectModel> args)
    {
        SelectedProjectId = args.ItemData?.Id;
    }


    public string SerializedProject { get; set; }


    internal async void LoadAndSerializeProject()
    {
        SerializedProject = "";
        if (SelectedProjectId is null)
        {
            StateHasChanged();
            return;
        }

        ProgressStart();

        try
        {
            var dbContext = await DbContextFactory.CreateDbContextAsync();

            var project = await dbContext.Projects
                .Include(p => p.City).ThenInclude(p => p.County)
                .Include(p => p.Apus)

                .Include(p => p.Apus).ThenInclude(o => o.Status)
                .Include(p => p.Apus).ThenInclude(o => o.ApuPerformances)
                .Include(p => p.Apus).ThenInclude(o => o.ApuLabors)
                .Include(p => p.Apus).ThenInclude(o => o.ApuMaterials)
                .Include(p => p.Apus).ThenInclude(o => o.ApuEquipments)
                .Include(p => p.Apus).ThenInclude(o => o.ApuContracts)

                .Include(q => q.LastUpdatedBy)

                .SingleAsync(q => q.Id == SelectedProjectId);


            SerializedProject = JsonSerializer.Serialize(project, Json.Options);

            ProgressStop();
        }
        catch (Exception e)
        {
            ShowError(e.Message);
        }
    }

    internal async void Import(InputFileChangeEventArgs arg)
    {
        var reader = await new StreamReader(arg.File.OpenReadStream()).ReadToEndAsync();

        try
        {
            var projectId = Guid.NewGuid();
            
            var project = JsonSerializer.Deserialize<Project>(reader, Json.Options);
            
            project.Id = projectId;
            project.CityId = null;
            project.City = null;

            foreach (var apu in project.Apus)
            {
                apu.ProjectId = projectId;

                var apuId = Guid.NewGuid();
                apu.Id = apuId;

                apu.Status = null;

                apu.LastUpdatedBy = null;
                apu.SetLastUpdatedDb(Liu);

                foreach (var apuPerformance in apu.ApuPerformances)
                {
                    apuPerformance.Id = Guid.NewGuid();
                    apuPerformance.ApuId = apuId;
                    apuPerformance.BasePerformanceId = null;

                    apuPerformance.LastUpdatedBy = null;
                    apuPerformance.SetLastUpdatedDb(Liu);
                }
                foreach (var apuLabor in apu.ApuLabors)
                {
                    apuLabor.Id = Guid.NewGuid();
                    apuLabor.ApuId = apuId;
                    apuLabor.BaseLaborId = null;

                    apuLabor.LastUpdatedBy = null;
                    apuLabor.SetLastUpdatedDb(Liu);
                }
                foreach (var apuMaterial in apu.ApuMaterials)
                {
                    apuMaterial.Id = Guid.NewGuid();
                    apuMaterial.ApuId = apuId;
                    apuMaterial.BaseMaterialId = null;

                    apuMaterial.LastUpdatedBy = null;
                    apuMaterial.SetLastUpdatedDb(Liu);
                }
                foreach (var apuEquipment in apu.ApuEquipments)
                {
                    apuEquipment.Id = Guid.NewGuid();
                    apuEquipment.ApuId = apuId;
                    apuEquipment.BaseEquipmentId = null;

                    apuEquipment.LastUpdatedBy = null;
                    apuEquipment.SetLastUpdatedDb(Liu);
                }
                foreach (var apuContract in apu.ApuContracts)
                {
                    apuContract.Id = Guid.NewGuid();
                    apuContract.ApuId = apuId;
                    apuContract.BaseContractId = null;

                    apuContract.LastUpdatedBy = null;
                    apuContract.SetLastUpdatedDb(Liu);
                }
            }

            project.LastUpdatedBy = null;
            project.SetLastUpdatedDb(Liu);

            var dbContext = await DbContextFactory.CreateDbContextAsync();
            await dbContext.Projects.AddAsync(project);
            await dbContext.SaveChangesAsync();

            ShowSuccess("Import ready!");
        }
        catch (Exception e)
        {
            ShowError(e.Message);
        }

    }

    #endregion



}