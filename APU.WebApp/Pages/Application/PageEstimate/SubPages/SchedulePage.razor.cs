using System.Collections.ObjectModel;
using APU.DataV2.EntityModels;
using APU.WebApp.Services.SignalRHub.HubClients.ProjectScheduleHub;
using APU.WebApp.Services.SignalRHub.HubClients.ProjectScheduleHub.Messages;
using APU.WebApp.Shared.Dialogs;
using APU.WebApp.Utils.Modules;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
// ReSharper disable UnusedMethodReturnValue.Local

#pragma warning disable BL0005

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class SchedulePageVM : PageVMBase, IDisposable
{
    #region Security

    internal bool CanEdit
    {
        get
        {
            if (SelectedProjectModel is null)
                return false;

            if (SelectedProjectModel.IsBlocked)
            {
                if ((Liu.IsAdministrator || Liu.IsSupervisor) == false)
                    return false;
            }

            return true;
        }
    }

    #endregion

    #region Apu Hub

    [Inject]
    protected ApuHubClient ApuHub { get; set; }

    private void InitializeApuHub()
    {
        if (ApuHub is null)
            return;

        ApuHub.StartHub(NavM, "Project Schedule");

        ApuHub.ApuCreated = ApuHub_ApuCreated;
        ApuHub.ApuUpdated = ApuHub_ApuUpdated;
        ApuHub.ApuDeleted = ApuHub_ApuDeleted;
    }

    private void ApuHub_ApuCreated(ApuHubApuCreatedMessage message)
    {
        if (SelectedProjectModel is null)
            return;

        if (SelectedProjectModel.Id != message.ProjectId)
            return;

        if (!message.IsLineItem)
            return;

        _ = ProjectScheduleLoadAsync();
    }

    private void ApuHub_ApuUpdated(ApuHubApuUpdatedMessage message)
    {
        if (SelectedProjectModel is null)
            return;

        if (SelectedProjectModel.Id != message.ProjectId)
            return;

        if (!message.IsLineItem)
            return;

        _ = ProjectScheduleLoadAsync();
    }

    private void ApuHub_ApuDeleted(ApuHubApuDeletedMessage message)
    {
        if (SelectedProjectModel is null)
            return;

        if (SelectedProjectModel.Id != message.ProjectId)
            return;

        _ = ProjectScheduleLoadAsync();
    }

    #endregion

    #region Project Hub

    [Inject]
    protected ProjectHubClient ProjectHub { get; set; }

    private void InitializeProjectHub()
    {
        if (ProjectHub is null)
            return;

        ProjectHub.StartHub(NavM, "Project Schedule");

        ProjectHub.ProjectHasScheduleUpdated = ProjectHub_ProjectHasScheduleUpdated;
    }

    private async void ProjectHub_ProjectHasScheduleUpdated(ProjectHubProjectHasScheduleUpdatedMessage message)
    {
        var projectModel = allProjectModels.FirstOrDefault(q => q.Id == message.ProjectId);
        if (projectModel is null)
            return;

        projectModel.HasSchedule = message.HasSchedule;

        if (projectModel.Id == SelectedProjectModel.Id)
        {
            SelectedProjectModel.HasSchedule = message.HasSchedule;
            await ProjectScheduleLoadAsync();
        }
    }

    #endregion

    #region Project Schedule Hub

    [Inject]
    protected ProjectScheduleHubClient ScheduleHub { get; set; }

    private void InitializeScheduleHub()
    {
        if (ScheduleHub is null)
            return;

        ScheduleHub.StartHub(NavM, "Project Schedule");

        ScheduleHub.ScheduleUpdated = ScheduleHub_ScheduleUpdated;
    }

    private void ScheduleHub_ScheduleUpdated(ProjectScheduleHubScheduleUpdatedMessage message)
    {
        if (SelectedProjectModel is null)
            return;

        if (SelectedProjectModel.Id != message.ProjectId)
            return;

        _ = ProjectScheduleLoadAsync();
    }

    #endregion

    #region ElementRefs - Dialogs

    internal DlgConfirmation<ProjectModel> ConfirmationDialogDeleteProjectSchedule { get; set; }

    internal void ConfirmationDialogDeleteProjectScheduleOpen()
    {
        if (SelectedProjectModel is null)
            return;

        if (!SelectedProjectModel.HasSchedule)
            return;

        ConfirmationDialogDeleteProjectSchedule.Open("Are you sure to delete the complete Project Schedule?", "", SelectedProjectModel, SelectedProjectModel.ProjectName);
    }

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        SM = new ScheduleManager();

        SM.ShowError = ShowError;

        SM.ItemAdd = ScheduleItemAddAsync;
        SM.ItemUpdate = ScheduleItem_UpdateSingle;
        SM.ItemUpdateOrderNos = ScheduleItem_UpdateOrderNosAndParentIdsAsync;
        SM.ItemDeletePost = ScheduleItem_DeletePostDb;

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await EventAggregator.PublishAsync(new HeaderLinkMessage());
            SendHeaderMessage(typeof(HeaderEstimate));

            ConfirmationDialogDeleteProjectSchedule.Submit = ProjectScheduleDeleteComplete;

            await GetLIU();

            var holidayResult = await GetHolidaysAsync();
            if (!holidayResult.IsSuccess())
            {
                ShowError("Failed to Load Holidays!");
                return;
            }

            SM.Initialize(holidayResult.Data);

            #region Load UserSession

            IsBlocked = USS.GetProjectBlock(Liu.Id);
            var selectedProjectId = USS.GetSelectedProjectId(Liu.Id);

            #endregion

            await GetAllProjectModels();
            await GetFilteredProjectModels();

            SelectedProjectModel = FilteredProjectModels.FirstOrDefault(q => q.Id == selectedProjectId);

            await ProjectScheduleLoadAsync();

            InitializeApuHub();
            InitializeProjectHub();
            InitializeScheduleHub();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region Schedule Manager

    internal ScheduleManager SM { get; set; }    

    public DateTime? GanttStartDate
    {
        get
        {
            if (SelectedProject?.StartDate is null)
                return null;

            return SelectedProject.StartDate.Value - TimeSpan.FromDays(0);
        }
    }
    public DateTime? GanttEndDate
    {
        get
        {
            if (SelectedProject?.EndDate is null)
                return null;

            return SelectedProject.EndDate.Value + TimeSpan.FromDays(2);
        }
    }

    #endregion

    #region Items - AllProjectModels / FilteredProjectModels

    private List<ProjectModel> allProjectModels;
    private async Task GetAllProjectModels()
    {
        ProgressStart();

        var result = await ProjectRepo.GetAllModelWithScheduleAsync();
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
    private async Task GetFilteredProjectModels(string projectFilter = null)
    {
        FilteredProjectModels = allProjectModels
            .If(IsBlocked == true, p => p.Where(o => o.IsBlocked))
            .If(IsBlocked == false, p => p.Where(o => o.IsBlocked == false))
            .If(!string.IsNullOrWhiteSpace(projectFilter), q => 
                q.Where(o =>
	                o.ProjectName.ToLower().Contains(projectFilter?.ToLower() ?? string.Empty) ||
	                TeguStringComparer.CompareToFilterBool(o.ProjectName, projectFilter))
            )
            .ToObservableCollection();

        await InvokeAsync(SfCbProjectFilter.RefreshDataAsync);
        await InvokeAsync(StateHasChanged);
    }

    // Project Filter
    internal SfComboBox<ProjectModel, ProjectModel> SfCbProjectFilter { get; set; }
    internal async void ProjectFilterChanged(SfDropDownFilteringEventArgs args)
    {
        args.PreventDefaultAction = true;
        await GetFilteredProjectModels(args.Text);

        var query = new Query();
        await SfCbProjectFilter.FilterAsync(FilteredProjectModels, query);
    }

    internal bool? IsBlocked { get; set; }
    internal async void IsBlockedChanged()
    {
        if (IsBlocked == null)
            IsBlocked = true;
        else if (IsBlocked == true)
            IsBlocked = false;
        else if (IsBlocked == false)
            IsBlocked = null;

        if ((IsBlocked == true && SelectedProjectModel?.IsBlocked == false) ||
            (IsBlocked == false && SelectedProjectModel?.IsBlocked == true))
        {
            // Reset the items
            SelectedProjectModel = null;

            StateHasChanged();
            await Task.Delay(1000);

            await ProjectScheduleLoadAsync();
        }

        USS.SetProjectBlock(Liu.Id, IsBlocked);

        await GetFilteredProjectModels();
    }

    #endregion

    #region Selected Project

    internal string InitializationError { get; set; }

    public ProjectModel SelectedProjectModel { get; set; }

    internal async void SelectedProjectCBChanged(ChangeEventArgs<ProjectModel, ProjectModel> args)
    {
	    if (SelectedProjectModel?.Id != args.ItemData?.Id)
        {
            SelectedProjectModel = args.ItemData;
            USS.SetSelectedProject(Liu.Id, SelectedProjectModel?.Id);
            await ProjectScheduleLoadAsync();
        }
    }

    internal Project SelectedProject { get; set; }

    internal async void ProjectStartDateChanged(ChangedEventArgs<DateTime?> arg)
    {
        if (!CanEdit)
        {
            ShowError("Not authorized to change this value!");
            return;
        }

        if (SelectedProject is null || !SelectedProject.HasSchedule)
        {
            ShowError("Select a Project with Schedule!");
            return;
        }

        var result = await ProjectRepo.UpdateStartDateAsync(SelectedProject.Id, arg.Value);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Project Start Date - " + result.Message);
            return;
        }

        SelectedProject.StartDate = arg.Value;
        SM.SetGanttHolidays(SelectedProject.StartDate?.Year, SelectedProject.EndDate?.Year);
        StateHasChanged();

        ScheduleHub.SendScheduleUpdate(SelectedProject.Id);
    }

    internal async void ProjectEndDateChanged(ChangedEventArgs<DateTime?> arg)
    {
        if (!CanEdit)
        {
            ShowError("Not authorized to change this value!");
            return;
        }

        if (SelectedProject is null || !SelectedProject.HasSchedule)
        {
            ShowError("Select a Project with Schedule!");
            return;
        }

        var result = await ProjectRepo.UpdateEndDateAsync(SelectedProject.Id, arg.Value);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Project Start Date - " + result.Message);
            return;
        }

        SelectedProject.EndDate = arg.Value;
        SM.SetGanttHolidays(SelectedProject.StartDate?.Year, SelectedProject.EndDate?.Year);
        StateHasChanged();

        ScheduleHub.SendScheduleUpdate(SelectedProject.Id);
    }

    #endregion

    #region Gantt Data - Holidays

    private async Task<Result<List<Holiday>>> GetHolidaysAsync()
    {
        var holidaysResult = await DefinitionsRepo.HolidayGetAllAsync();
        if (!holidaysResult.IsSuccess())
            return holidaysResult;

        return Result<List<Holiday>>.OkData(holidaysResult.Data);
    }

    #endregion


    #region Project Schedule - CRUD

    private async Task ProjectScheduleLoadAsync()
    {
        if (SelectedProjectModel is null || !SelectedProjectModel.HasSchedule)
        {
            SelectedProject = null;
            SM.ResetGanttData();
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            ProgressStart();
            // Wait to put the Gantt into the view!
            await Task.Delay(100);

            // Step 1: Get the LineItems
            var projectResult = await ProjectRepo.GetAsync(SelectedProjectModel.Id, false, true);
            if (!projectResult.IsSuccess())
            {
                SelectedProject = null;
                SM.ResetGanttData();
                ShowError("Failed to load Project!");
                return;
            }

            SelectedProject = projectResult.Data;

            var lineItems = SelectedProject.Apus
                .Where(q => q.IsLineItem)
                .ToList();

            // Step 2: Get the Schedule Items
            var scheduleResult = await ProjectScheduleRepo.GetAllAsync(SelectedProjectModel.Id);
            if (!scheduleResult.IsSuccess())
            {
                SM.ResetGanttData();
                ShowError("Failed to collect Schedule Items!");
                return;
            }

            SM.SetGanttHolidays(SelectedProject.StartDate?.Year, SelectedProject.EndDate?.Year);

            var dataResult = SM.SetData(lineItems, scheduleResult.Data);
            if (!dataResult.IsSuccess())
                ShowError(dataResult.Message);
            else
                ProgressStop();
        }
    }

    internal async void ProjectScheduleCreate()
    {
        #region Validation

        if (!CanEdit)
        {
            ShowError("Not authorized to create Project Schedule!");
            return;
        }

        if (SelectedProjectModel is null)
        {
            ShowError("Select a Project!");
            return;
        }

        if (SelectedProjectModel.HasSchedule)
        {
            ShowError("Selected Project already has a Schedule!");
            return;
        }

        #endregion

        ProgressStart();

        var projectResult = await ProjectRepo.GetAsync(SelectedProjectModel.Id, false, true);
        if (!projectResult.IsSuccess())
        {
            ShowError("Failed to load Project!");
            return;
        }

        var lineItems = projectResult.Data.Apus
            .Where(q => q.IsLineItem)
            .OrderBy(q => q.GroupId)
            .ThenBy(q => q.ItemId)
            .ToList();

        // Step 2: Generate the Schedules
        var schedules = new List<ProjectSchedule>();

        var orderNo = 1;
        foreach (var apu in lineItems)
        {
            schedules.Add(new ProjectSchedule(id: Guid.NewGuid(), orderNo, projectId: SelectedProjectModel.Id, apuId: apu.Id));
            orderNo++;
        }

        var scheduleUpdateResult = await ProjectScheduleRepo.AddRangeAsync(schedules);
        if (!scheduleUpdateResult.IsSuccess())
        {
            ShowError(scheduleUpdateResult.Message);
            return;
        }

        var projectUpdateResult = await ProjectRepo.UpdateHasScheduleAsync(SelectedProjectModel.Id, true);
        if (!projectUpdateResult.IsSuccess())
        {
            ShowError(projectUpdateResult.Message);
            return;
        }

        SelectedProjectModel.HasSchedule = true;
        ProjectHub.SendProjectHasScheduleUpdate(SelectedProjectModel.Id, true);

        await ProjectScheduleLoadAsync();

        //ProgressStop();

        ScheduleHub.SendScheduleUpdate(SelectedProject.Id);
    }

    internal async void ProjectScheduleSynchronizeItems()
    {
        if (!CanEdit)
        {
            ShowError("Not authorized to create Project Schedule!");
            return;
        }

        if (SelectedProjectModel is null)
        {
            ShowError("Select a Project!");
            return;
        }

        if (!SelectedProjectModel.HasSchedule)
        {
            ShowError("Selected Project doesn't to have a Schedule!");
            return;
        }

        ProgressStart();

        // Step 1: Get the LineItems
        var projectResult = await ProjectRepo.GetAsync(SelectedProjectModel.Id, false, true);
        if (!projectResult.IsSuccess())
        {
            ShowError("Failed to load Project!");
            return;
        }

        var lineItems = projectResult.Data.Apus
            .Where(q => q.IsLineItem)
            .ToList();

        // Step 2: Get the Schedule Items
        var scheduleResult = await ProjectScheduleRepo.GetAllAsync(SelectedProjectModel.Id);
        if (!scheduleResult.IsSuccess())
        {
            SM.ResetGanttData();
            ShowError("Failed to collect Schedule Items!");
            return;
        }

        var scheduleItems = scheduleResult.Data;

        // Step 3: Compare
        var missingApuSchedules = new List<Apu>();

        foreach (var apu in lineItems)
        {
            var schedule = scheduleItems.FirstOrDefault(q => q.ApuId == apu.Id);
            if (schedule is null)
                missingApuSchedules.Add(apu);
        }

        if (missingApuSchedules.Count == 0)
        {
            ShowSuccess("Project Schedule Items matches, no action needed!");
            ProgressStop(false);
            return;
        }

        // Step 4: Add the missing schedule items
        var missingSchedules = new List<ProjectSchedule>();

        var orderNo = scheduleItems.Max(q => q.OrderNo) + 1;
        foreach (var apu in missingApuSchedules)
        {
            missingSchedules.Add(new ProjectSchedule(id: Guid.NewGuid(), orderNo, projectId: SelectedProjectModel.Id, apuId: apu.Id));
            orderNo++;
        }

        var scheduleUpdateResult = await ProjectScheduleRepo.AddRangeAsync(missingSchedules);
        if (!scheduleUpdateResult.IsSuccess())
        {
            ShowError(scheduleUpdateResult.Message);
            return;
        }

        await ProjectScheduleLoadAsync();

        ScheduleHub.SendScheduleUpdate(SelectedProject.Id);
    }

    internal async void ProjectScheduleDeleteComplete(ProjectModel projectModel)
    {
        if (projectModel is null)
            return;

        if (!projectModel.HasSchedule)
            return;

        ProgressStart();

        var psDeleteResult = await ProjectScheduleRepo.DeleteCompleteAsync(projectModel.Id);
        if (!psDeleteResult.IsSuccess())
        {
            ShowError(psDeleteResult);
            return;
        }

        var projectUpdateResult = await ProjectRepo.UpdateHasScheduleAsync(projectModel.Id, false);
        if (!projectUpdateResult.IsSuccess())
        {
            ShowError(projectUpdateResult.Message);
            return;
        }

        projectModel.HasSchedule = false;
        ProjectHub.SendProjectHasScheduleUpdate(projectModel.Id, false);

        ProgressStop();

        await ProjectScheduleLoadAsync();

        ScheduleHub.SendScheduleUpdate(SelectedProjectModel.Id);
    }    

    #endregion

    #region Project Schedule Items - CRUD

    private async Task ScheduleItemAddAsync(ProjectSchedule ps, List<ProjectSchedule> updatedPss)
    {
        if (ps is null)
        {
            ShowError("Project Schedule is null!");
            return;
        }

        if (SelectedProject is null)
        {
            ShowError("Select a Project!?");
            return;
        }

        ProgressStart();

        ps.ProjectId = SelectedProject.Id;

        var addResult = await ProjectScheduleRepo.AddAsync(ps, updatedPss);
        if (!addResult.IsSuccess())
        {
            ShowError(addResult);
            return;
        }

        ProgressStop();

        ScheduleHub.SendScheduleUpdate(SelectedProjectModel.Id);
    }

    internal async void ScheduleItem_UpdateIsHidden(ProjectSchedule schedule)
    {
        ProgressStart();
        
        // Update in the db
        var updateResult = await ProjectScheduleRepo.UpdateIsHiddenAsync(schedule.Id, !schedule.IsHidden);
        if (!updateResult.IsSuccess())
        {
            ShowError(updateResult.Message);
            return;
        }

        schedule.IsHidden = !schedule.IsHidden;

        SM.GanttFilterAsync(SM.IsHidden);

        ProgressStop();

        ScheduleHub.SendScheduleUpdate(SelectedProjectModel.Id);
    }

    private async void ScheduleItem_UpdateSingle(ProjectSchedule ps)
    {
        ProgressStart();

        if (ps is null)
        {
            ShowError("Project Schedule is null, update failed!");
            return;
        }

        // Assign the Description for the custom items
        if (ps.ApuId is null)
            ps.Description = ps.GanttCustomDescription;

        var updateResult = await ProjectScheduleRepo.UpdateAsync(
            id: ps.Id,
            parentId: ps.ParentId,
            description: ps.Description,
            startDate: ps.StartDate,
            endDate: ps.EndDate,
            duration: ps.Duration,
            predecessor: ps.Predecessor
        );

        if (!updateResult.IsSuccess())
        {
            ShowError(updateResult.Message);
            return;
        }

        ProgressStop();

        ScheduleHub.SendScheduleUpdate(SelectedProjectModel.Id);
    }

    private async Task ScheduleItem_UpdateOrderNosAndParentIdsAsync(List<ProjectSchedule> pss)
    {
        ProgressStart();

        var result = await ProjectScheduleRepo.UpdateOrderNosAndParentIdAsync(SelectedProject.Id, pss);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Schedule Item OrderNos! " + result.Message);
            return;
        }

        ProgressStop();

        ScheduleHub.SendScheduleUpdate(SelectedProjectModel.Id);
    }

    private async Task ScheduleItem_DeletePostDb(ProjectSchedule ps, List<ProjectSchedule> updatedPss)
    {
        ProgressStart();

        var result = await ProjectScheduleRepo.DeleteAsync(ps.Id, updatedPss);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        ProgressStop();
 
        ScheduleHub.SendScheduleUpdate(SelectedProjectModel.Id);
    }

    #endregion


    #region IDisposable

    public void Dispose()
    {
        ProjectHub?.Dispose();
        ProjectHub = null;

        ScheduleHub?.Dispose();
        ScheduleHub = null;
    }

    #endregion
}