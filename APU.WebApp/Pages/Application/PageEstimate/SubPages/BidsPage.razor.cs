using System.Collections.ObjectModel;
using APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;
using APU.WebApp.Services.Navigation;
using APU.WebApp.Shared.Dialogs;
using APU.WebApp.Utils;
using Syncfusion.Blazor.Inputs;

#pragma warning disable BL0005

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class BidsPageVM : PageVMBase, IDisposable
{
    #region Security

    private bool CanModifyBlockedProject(Project project)
    {
        if (project is null)
            return false;

        if (project.IsBlocked)
        {
            if ((Liu.IsAdministrator || Liu.IsSupervisor) == false)
                return false;
        }

        return true;
    }

    #endregion

    #region Project Hub

    [Inject]
    protected ProjectHubClient ProjectHub { get; set; }

    private void InitializeProjectHub()
    {
        if (ProjectHub is null)
            return;

        ProjectHub.StartHub(NavM, "Bids");

        ProjectHub.ProjectUpdated = ProjectHub_ProjectUpdated;
    }

    private async void ProjectHub_ProjectUpdated(ProjectHubProjectUpdatedMessage message)
    {
	    var result = await ProjectRepo.GetAsync(message.ProjectId, includeCity: true);
	    if (!result.IsSuccess())
	    {
            ShowError(result.Message);
            return;
	    }
	    var dbProject = result.Data;

        var calculationResult = dbProject.Calculate(defaultValues);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        var project = allProjects.FirstOrDefault(q => q.Id == dbProject.Id);
        if (project is null)
        {
            allProjects.Add(dbProject);
            GetFilteredProjects();
            return;
        }

        allProjects = allProjects.ReplaceItem(dbProject);
        GetFilteredProjects();

        await InvokeAsync(Grid.Refresh);
    }

    #endregion
    #region Apu Hub

    [Inject]
    protected ApuHubClient ApuHub { get; set; }

    private void InitializeApuHub()
    {
        if (ApuHub is null)
            return;

	    ApuHub.StartHub(NavM, "Bids");
    }

    #endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderEstimate));

            ConfirmationDialogProject.Submit = DeleteProject;

            if (!await GetLIU())
                return;

            IsBlocked = USS.GetProjectBlock(Liu.Id);

            await GetCities();
            await GetDefaultValuesAsync();
            await GetAllProjects();
            GetFilteredProjects();
            await Grid.Refresh();

            ProjectManager.Submit = ProjectManagerSubmit;

            InitializeProjectHub();
            InitializeApuHub();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region ElementRefs - Dialogs

    internal DlgConfirmation<Project> ConfirmationDialogProject { get; set; }    

    #endregion
    #region ElementRefs - DlgProjectManager

    internal DlgProjectManager ProjectManager { get; set; }

    internal void ProjectManagerOpen()
    {
        if (ProjectManager.IsVisible)
            return;

        if (Grid.SelectedRecords.Count > 0)
        {
            var project = Grid.SelectedRecords[0];
            ProjectManager.Open(ManagerMode.Duplicate, project, defaultValues, Cities);
        }
        else
        {
            ProjectManager.Open(ManagerMode.Add, null, defaultValues, Cities);
        }
    }

    internal async void ProjectManagerSubmit((ManagerMode mode, Project project) p)
    {
        if (p.mode is ManagerMode.Add or ManagerMode.Duplicate)
            await CreateProject(p.project);
    }

    #endregion
    #region ElementRef - SfGrid

    internal SfGrid<Project> Grid { get; set; }

    internal async void DataGridOnActionBegin(ActionEventArgs<Project> args)
    {
        if (args.RequestType == SfGridAction.BeginEdit)
        {
            if (!CanModifyBlockedProject(args.Data))
            {
                args.Cancel = true;
                return;
            }
        }

        if (args.RequestType == SfGridAction.Save)
        {
            if (!CanModifyBlockedProject(args.Data))
            {
                args.Cancel = true;
                return;
            }

            var result = await UpdateProject(args.Data, args.PreviousData.Supervision, args.PreviousData.GrossLabor, args.PreviousData.GrossMaterials, args.PreviousData.GrossEquipment, args.PreviousData.GrossContracts);
            if (!result.IsSuccess())
                args.Cancel = true;
        }
    }

    internal void DataGridCommandClick(CommandClickEventArgs<Project> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            if (!CanModifyBlockedProject(args.RowData))
            {
                args.Cancel = true;
                return;
            }

            ConfirmationDialogProject.Open("Are you sure to delete the following Project?", args.RowData.ProjectName, args.RowData);
            args.Cancel = true;
        }
    }

    #endregion

    #region Filters

    private string filterText = "";

    internal void TbFilterInputChanged(InputEventArgs args)
    {
        filterText = args.Value;
    }

    internal void TbFilterKeyPressed(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            GetFilteredProjects();
        }
    }

    internal bool? IsBlocked { get; set; }

    internal void IsBlockedChanged()
    {
        if (IsBlocked == null)
            IsBlocked = true;
        else if (IsBlocked == true)
            IsBlocked = false;
        else if (IsBlocked == false)
            IsBlocked = null;

        USS.SetProjectBlock(Liu.Id, IsBlocked);

        GetFilteredProjects();
    }

    #endregion

    #region Resources - Cities

    internal ObservableCollection<City> Cities { get; set; } = new();

    private async Task GetCities()
    {
        var result = await CityRepo.GetAllAsync();
        if (!result.IsSuccess())
        {
            Cities = new ObservableCollection<City>();
            ShowError("Failed to collect cities!");
            return;
        }

        Cities = result.Data.OrderBy(c => c.Name).ToObservableCollection();
    }

    #endregion
    #region Resources - Default Values

    private DefaultValue defaultValues;

    private async Task GetDefaultValuesAsync()
    {
        var result = await DefaultRepo.GetAsync();
        if (!result.IsSuccess())
        {
            defaultValues = new DefaultValue();
            ShowError("Failed to collect Default Values!");
            return;
        }

        defaultValues = result.Data;
    }

    #endregion

    #region Projects / FilteredProjects

    private List<Project> allProjects;
    private async Task GetAllProjects()
    {
        var result = await ProjectRepo.GetAllAsync(includeCity: true);
        if (!result.IsSuccess())
        {
            allProjects = new List<Project>();
            ShowError("Failed to collect Projects!");
            return;
        }

        allProjects = result.Data;
    }

    public ObservableCollection<Project> FilteredProjects { get; set; }

    internal void GetFilteredProjects()
    {
        FilteredProjects = allProjects
            .If(!string.IsNullOrWhiteSpace(filterText), q => 
                q.Where(o =>
                    o.ProjectName.ToLower().Contains(filterText.ToLower()) ||
                    TeguStringComparer.CompareToFilterBool(o.ProjectName, filterText))
            )
            .If(IsBlocked == true, p => p.Where(o => o.IsBlocked))
            .If(IsBlocked == false, p => p.Where(o => o.IsBlocked == false))
            .OrderBy(p => p.ProjectName)
            .ToObservableCollection();

        InvokeAsync(StateHasChanged);
    }

    #endregion
     

    #region CRUD

    private async Task CreateProject(Project project)
    {
        ProgressStart();

        var result = await ProjectRepo.AddAsync(project, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add new Project");
            return;
        }

        result.Data.City = Cities.FirstOrDefault(q => q.Id == project.CityId);

        allProjects.Add(result.Data);
        GetFilteredProjects();

        ProgressStop();
    }

    private async void DeleteProject(Project project)
    {
        ProgressStart();

        var deleteResult = await ProjectRepo.DeleteAsync(project.Id);
        if (!deleteResult.IsSuccess())
        {
            ShowError("Delete Project Failed!");
            return;
        }

        allProjects.Remove(project);
        FilteredProjects.Remove(project);

        ProgressStop();
    }

    #endregion

    #region Update

    internal void CityChanged(int? cityId, Project project)
    {
        project.CityId = cityId;
        project.City = Cities.FirstOrDefault(q => q.Id == cityId);
    }

    private async Task<Result> UpdateProject(Project newProject, decimal prevSupervision, decimal prevGrossLabor, decimal prevGrossMaterial, decimal prevGrossEquipment, decimal prevGrossContract)
    {
        ProgressStart();

        var apuUpdateResult = await TryUpdateApuPercentagesAsync(newProject, prevSupervision, prevGrossLabor, prevGrossMaterial, prevGrossEquipment, prevGrossContract);
        if (!apuUpdateResult.IsSuccess())
        {
            ShowError("Failed to update Line Items!");
            return Result.Fail();
        }

        var result = await ProjectRepo.UpdateAsync(newProject, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Project!");
            return Result.Fail();
        }

        GetFilteredProjects();

        newProject.City.Projects = null;
        ProjectHub.SendProjectUpdate(new ProjectHubProjectUpdatedMessage(result.Data.Id));

        ProgressStop();
        return Result.Ok();
    }

    private async Task<Result> TryUpdateApuPercentagesAsync(Project newProject, decimal prevSupervision, decimal prevGrossLabor, decimal prevGrossMaterial, decimal prevGrossEquipment, decimal prevGrossContract)
    {
	    if (prevGrossLabor != newProject.GrossLabor ||
	        prevGrossMaterial != newProject.GrossMaterials ||
	        prevGrossEquipment != newProject.GrossEquipment ||
	        prevGrossContract != newProject.GrossContracts ||
	        prevSupervision != newProject.Supervision)
	    {
		    var lineItemsResult = await ProjectRepo.GetLineItemsAsync(newProject.Id);
		    if (!lineItemsResult.IsSuccess())
			    return lineItemsResult;

		    var apus = lineItemsResult.Data.Apus;
		    var updatedApus = new List<Apu>();
		    foreach (var apu in apus)
		    {
			    var updated = false;

			    if (apu.SuperPercentage == prevSupervision)
			    {
				    apu.SuperPercentage = newProject.Supervision;
				    updated = true;
			    }
			    if (apu.LaborGrossPercentage == prevGrossLabor)
			    {
				    apu.LaborGrossPercentage = newProject.GrossLabor;
				    updated = true;
			    }
			    if (apu.MaterialGrossPercentage == prevGrossMaterial)
			    {
				    apu.MaterialGrossPercentage = newProject.GrossMaterials;
				    updated = true;
			    }
			    if (apu.EquipmentGrossPercentage == prevGrossEquipment)
			    {
				    apu.EquipmentGrossPercentage = newProject.GrossEquipment;
				    updated = true;
			    }
			    if (apu.SubcontractorGrossPercentage == prevGrossContract)
			    {
				    apu.SubcontractorGrossPercentage = newProject.GrossContracts;
				    updated = true;
			    }

                if (updated)
                    updatedApus.Add(apu);
		    }

		    if (updatedApus.Count == 0) 
			    return Result.Ok();
		    
		    var updateResult = await ApuRepo.UpdateRangeAsync(updatedApus, Liu);
		    if (!updateResult.IsSuccess())
			    return updateResult;

            ApuHub.SendApuUpdateMultiple(new ApuHubApuUpdatedMultipleMessage(newProject.Id, updatedApus.Select(q => q.Id).ToList()));
	    }

	    return Result.Ok();
    }

    #endregion


    #region Duplicate

    internal async void DuplicateSelectedProject()
    {
        if (Grid.SelectedRecords.Count < 1)
            return;

        ProgressStart();

        var selectedProject = Grid.SelectedRecords[0];
        var result = await ProjectRepo.DuplicateAsync(selectedProject.Id, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Project Duplication Failed");
            return;
        }

        allProjects.Add(result.Data);
        GetFilteredProjects();

        ProgressStop();
    }

    #endregion

    #region Functions - Navigate

    internal void NavigateToEstimate()
    {
        if (Grid.SelectedRecords.Count == 0)
            return;

        USS.SetSelectedProject(Liu.Id, Grid.SelectedRecords[0].Id);
        NavM.NavigateTo(NavS.Estimates.Estimate);
    }

    #endregion

    #region Export

    internal async void ExportAsExcel()
    {
        var properties = new ExcelExportProperties();
        properties.Columns = new List<GridColumn>
        {
            new() { Field = nameof(Project.ProjectName), HeaderText = "Project Name", Width = "400" },
            new() { Field = nameof(Project.Owner), HeaderText = "Owner", Width = "200" },
            new() { Field = nameof(Project.Phone), HeaderText = "Phone", Width = "110" },
            new() { Field = nameof(Project.Email), HeaderText = "Email", Width = "200" },
            new() { Field = nameof(Project.Address), HeaderText = "Address", Width = "300" },
            new() { Field = "City.Name", HeaderText = "City", Width = "130" },
            new() { Field = nameof(Project.State), HeaderText = "State", Width = "40" },
            new() { Field = nameof(Project.Zip), HeaderText = "Zip", Width = "50" },
            new() { Field = nameof(Project.Gross), HeaderText = "Gross (%)", Width = "60"},
            new() { Field = nameof(Project.Supervision), HeaderText = "Supervision (%)", Width = "60"},
            new() { Field = nameof(Project.Tools), HeaderText = "Tools (%)", Width = "60"},
            new() { Field = nameof(Project.Bond), HeaderText = "Bond (%)", Width = "60"},
            new() { Field = nameof(Project.SalesTax), HeaderText = "SalesTax (%)", Width = "60"},
        };
        
        properties.Header = ExportTemplate.GetExcelHeader(colspan: 13, "Bids Report");
        properties.Theme = ExportTemplate.GetExcelTheme();
        properties.FileName = "TechGroupOne Bids.xlsx";

        await Grid.ExportToExcelAsync(properties);
    }

    #endregion


    #region IDisposable

    public void Dispose()
    {
        ProjectHub?.Dispose();
        ProjectHub = null;

        ApuHub?.Dispose();
        ApuHub = null;
    }

    #endregion
}