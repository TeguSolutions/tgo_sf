using System.Collections.ObjectModel;
using APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;
using APU.WebApp.Shared.Dialogs;
using APU.WebApp.Utils;
using Syncfusion.Blazor.Inputs;

#pragma warning disable BL0005

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class BasePerformancePageVM : PageVMBase, IDisposable
{
    #region BaseItem Hub

    [Inject]
    protected BaseItemHubClient BaseItemHub { get; set; }

    private void InitializeBaseItemHub()
    {
        if (BaseItemHub is null)
            return;

        BaseItemHub.StartHub(NavM, "Performances");

        BaseItemHub.PerformanceCreated = BaseItemHub_PerformanceCreated;
        BaseItemHub.PerformanceUpdated = BaseItemHub_PerformanceUpdated;
        BaseItemHub.PerformanceDeleted = BaseItemHub_PerformanceDeleted;
    }

    private async void BaseItemHub_PerformanceCreated(BaseItemHubPerformanceCreatedMessage message)
    {
        if (allPerformances.Count == 0)
            return;

        var result = await BaseItemRepo.PerformanceGetAsync(message.basePerformanceId);
        if (!result.IsSuccess())
            return;

        allPerformances.Add(result.Data);
        GetFilteredItems();

        await InvokeAsync(StateHasChanged);
    }

    private async void BaseItemHub_PerformanceUpdated(BaseItemHubPerformanceUpdatedMessage message)
    {
        if (allPerformances.Count == 0)
            return;

        var result = await BaseItemRepo.PerformanceGetAsync(message.basePerformanceId);
        if (!result.IsSuccess())
            return;

        allPerformances = allPerformances.ReplaceItem(result.Data);
        GetFilteredItems();

        await InvokeAsync(StateHasChanged);
    }

    private async void BaseItemHub_PerformanceDeleted(BaseItemHubPerformanceDeletedMessage message)
    {
        if (allPerformances.Count == 0)
            return;

        var performance = allPerformances.FirstOrDefault(q => q.Id == message.basePerformanceId);
        if (performance is null)
            return;

        allPerformances.Remove(performance);
        GetFilteredItems();

        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderEstimate));

            ConfirmationDialogPerformance.Submit = DeletePerformance;
            PerformanceManager.Submit = PerformanceManagerSubmit;

            await GetLIU();

            await GetAllPerformances();
            GetFilteredItems();
            StateHasChanged();

            InitializeBaseItemHub();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region ElementRefs - Dialogs

    internal DlgConfirmation<BasePerformance> ConfirmationDialogPerformance { get; set; }   

    #endregion
    #region Dialog Performance Manager

    internal DlgPerformanceManager PerformanceManager { get; set; }

    internal void PerformanceManagerOpen()
    {
        if (PerformanceManager.IsVisible)
            return;

        if (Grid.SelectedRecords.Count > 0)
        {
            var performance = Grid.SelectedRecords[0];
            PerformanceManager.Open(ManagerOpenMode.Duplicate, performance);
        }
        else
        {
            PerformanceManager.Open(ManagerOpenMode.Create, null);
        }
    }

    internal async void PerformanceManagerSubmit((ManagerSubmitMode mode, IPerformance performance) p)
    {
        if (p.performance is not BasePerformance basePerformance)
            return;

        if (p.mode is ManagerSubmitMode.Create or ManagerSubmitMode.Duplicate)
            await CreatePerformance(basePerformance);
    }

    #endregion

    #region ElementRef - SfGrid

    internal SfGrid<BasePerformance> Grid { get; set; }

    internal async void DataGridOnActionBegin(ActionEventArgs<BasePerformance> args)
    {
        if (args.RequestType == SfGridAction.Save)
        {
            var result = await UpdatePerformance(args.Data);
            if (!result.IsSuccess())
                args.Cancel = true;
        }
    }

    internal void DataGridCommandClick(CommandClickEventArgs<BasePerformance> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            ConfirmationDialogPerformance.Open("Are you sure to delete the following Base Performance?", args.RowData.Description, args.RowData);
            args.Cancel = true;
        }
    }

    #endregion

    #region Filter

    private string filterText = "";

    internal void TbFilterInputChanged(InputEventArgs args)
    {
        filterText = args.Value;
    }
    internal void TbFilterKeyPressed(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            GetFilteredItems();
        }
    }

    #endregion

    #region Items / FilteredItems

    private List<BasePerformance> allPerformances;
    private async Task GetAllPerformances()
    {
        ProgressStart();

        var result = await BaseItemRepo.PerformanceGetAllAsync(true);
        if (!result.IsSuccess())
        {
            allPerformances = new List<BasePerformance>();
            ShowError("Failed to collect Performances!");
            return;
        }

        allPerformances = result.Data;

        ProgressStop(false);
    }

    public ObservableCollection<BasePerformance> FilteredPerformances { get; set; }

    internal void GetFilteredItems()
    {
        FilteredPerformances = allPerformances
            .If(!string.IsNullOrWhiteSpace(filterText), q =>
                q.Where(o =>
                    o.Description.ToLower().Contains(filterText.ToLower()) ||
                    TeguStringComparer.CompareToFilterBool(o.Description, filterText))
            )
            .OrderBy(q => q.Description)
            .ToObservableCollection();
    }

    #endregion

    #region CRUD

    private async Task CreatePerformance(BasePerformance performance)
    {
        ProgressStart();

        var result = await BaseItemRepo.PerformanceAddAsync(performance, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add new Performance!");
            return;
        }

        allPerformances.Add(result.Data);
        GetFilteredItems();

        BaseItemHub.SendPerformanceCreate(new BaseItemHubPerformanceCreatedMessage(result.Data.Id));

        ProgressStop();
    }

    private async Task<Result> UpdatePerformance(BasePerformance newPerformance)
    {
        ProgressStart();

        var result = await BaseItemRepo.PerformanceUpdateFromIPerformanceAsync(newPerformance.Id, newPerformance, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Performance!");
            return Result.Fail();
        }

        GetFilteredItems();

        BaseItemHub.SendPerformanceUpdate(new BaseItemHubPerformanceUpdatedMessage(newPerformance.Id));

        ProgressStop();
        return Result.Ok();
    }

    private async void DeletePerformance(BasePerformance performance)
    {
        ProgressStart();

        var result = await BaseItemRepo.PerformanceDeleteAsync(performance.Id);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Performance!");
            return;
        }

        allPerformances.Remove(performance);
        FilteredPerformances.Remove(performance);
        
        BaseItemHub.SendPerformanceDelete(new BaseItemHubPerformanceDeletedMessage(performance.Id));

        ProgressStop();
    }

    #endregion

    #region Export

    internal async void ExportAsExcel()
    {
        var properties = new ExcelExportProperties();
        properties.Columns = new List<GridColumn>
        {
            new() { Field = nameof(BasePerformance.Description), HeaderText = "Description", Width = "500" },
            new() { Field = nameof(BasePerformance.Value), HeaderText = "Performance", Width = "100" },
            new() { Field = nameof(BasePerformance.Hours), HeaderText = "Hours", Width = "100" },
        };
        
        properties.Header = ExportTemplate.GetExcelHeader(colspan: 3, "Performance Report");
        properties.Theme = ExportTemplate.GetExcelTheme();
        properties.FileName = "TechGroupOne Performances.xlsx";

        await Grid.ExportToExcelAsync(properties);
    }

    #endregion


    #region IDisposable

    public void Dispose()
    {
        BaseItemHub?.Dispose();
        BaseItemHub = null;
    }

    #endregion
}