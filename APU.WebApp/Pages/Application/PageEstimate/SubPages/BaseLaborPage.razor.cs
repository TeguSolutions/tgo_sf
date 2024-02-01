using System.Collections.ObjectModel;
using APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;
using APU.WebApp.Shared.Dialogs;
using APU.WebApp.Utils;
using Syncfusion.Blazor.Inputs;

#pragma warning disable BL0005

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class BaseLaborPageVM : PageVMBase, IDisposable
{
    #region BaseItem Hub

    [Inject]
    protected BaseItemHubClient BaseItemHub { get; set; }

    private void InitializeBaseItemHub()
    {
        if (BaseItemHub is null)
            return;

        BaseItemHub.StartHub(NavM, "Labors");

        BaseItemHub.LaborCreated = BaseItemHub_LaborCreated;
        BaseItemHub.LaborUpdated = BaseItemHub_LaborUpdated;
        BaseItemHub.LaborDeleted = BaseItemHub_LaborDeleted;
    }

    private async void BaseItemHub_LaborCreated(BaseItemHubLaborCreatedMessage message)
    {
        if (allLabors.Count == 0)
            return;

        var result = await BaseItemRepo.LaborGetAsync(message.baseLaborId);
        if (!result.IsSuccess())
            return;

        result.Data.CalculateCost(defaultValue.TopFica, defaultValue.Fica, defaultValue.FutaSuta, defaultValue.WorkersComp);
        allLabors.Add(result.Data);
        GetFilteredItems();

        await InvokeAsync(StateHasChanged);
    }

    private async void BaseItemHub_LaborUpdated(BaseItemHubLaborUpdatedMessage message)
    {
        if (allLabors.Count == 0)
            return;

        var result = await BaseItemRepo.LaborGetAsync(message.baseLaborId);
        if (!result.IsSuccess())
            return;

        result.Data.CalculateCost(defaultValue.TopFica, defaultValue.Fica, defaultValue.FutaSuta, defaultValue.WorkersComp);
        allLabors = allLabors.ReplaceItem(result.Data);
        GetFilteredItems();

        await InvokeAsync(StateHasChanged);
    }

    private async void BaseItemHub_LaborDeleted(BaseItemHubLaborDeletedMessage message)
    {
        if (allLabors.Count == 0)
            return;

        var labor = allLabors.FirstOrDefault(q => q.Id == message.baseLaborId);
        if (labor is null)
            return;

        allLabors.Remove(labor);
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

            ConfirmationDialogLabor.Submit = DeleteLabor;
            LaborManager.Submit = LaborManagerSubmit;

            await GetLIU();

            await GetDefaultValues();
            await GetAllLabors();
            GetFilteredItems();
            StateHasChanged();

            InitializeBaseItemHub();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region ElementRefs - Dialogs

    internal DlgConfirmation<BaseLabor> ConfirmationDialogLabor { get; set; }   

    #endregion
    #region Dialog Labor Manager

    internal DlgLaborManager LaborManager { get; set; }

    internal void LaborManagerOpen()
    {
        if (LaborManager.IsVisible)
            return;

        if (Grid.SelectedRecords.Count > 0)
        {
            var labor = Grid.SelectedRecords[0];
            LaborManager.Open(ManagerOpenMode.Duplicate, labor);
        }
        else
        {
            LaborManager.Open(ManagerOpenMode.Create, null);
        }
    }

    internal async void LaborManagerSubmit((ManagerSubmitMode mode, ILabor labor) p)
    {
        if (p.labor is not BaseLabor baseLabor)
            return;

        if (p.mode is ManagerSubmitMode.Create or ManagerSubmitMode.Duplicate)
            await CreateLabor(baseLabor);
    }

    #endregion

    #region ElementRef - SfGrid

    internal SfGrid<BaseLabor> Grid { get; set; }

    internal async void DataGridOnActionBegin(ActionEventArgs<BaseLabor> args)
    {
        if (args.RequestType == SfGridAction.Save)
        {
            var result = await UpdateLabor(args.Data);
            if (!result.IsSuccess())
                args.Cancel = true;
        }
    }

    internal void DataGridCommandClick(CommandClickEventArgs<BaseLabor> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            ConfirmationDialogLabor.Open("Are you sure to delete the following Base Labor?", args.RowData.Description, args.RowData);
            args.Cancel = true;
        }
    }

    #endregion

    #region Default Values

    private DefaultValue defaultValue;

    private async Task GetDefaultValues()
    {
        var result = await DefaultRepo.GetAsync();
        if (!result.IsSuccess())
        {
            defaultValue = new DefaultValue();
            ShowError("Collecting Default Value failed!");
            return;
        }

        defaultValue = result.Data;
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
            GetFilteredItems();
        }
    }

    #endregion

    #region Items / FilteredItems

    private List<BaseLabor> allLabors;
    private async Task GetAllLabors()
    {
        ProgressStart();

        var result = await BaseItemRepo.LaborGetAllAsync(true);
        if (!result.IsSuccess())
        {
            allLabors = new List<BaseLabor>();
            ShowError("Failed to collect Labors!");
            return;
        }

        allLabors = result.Data;
        foreach (var labor in allLabors)
            labor.CalculateCost(defaultValue.TopFica, defaultValue.Fica, defaultValue.FutaSuta, defaultValue.WorkersComp);

        ProgressStop(false);
    }

    public ObservableCollection<BaseLabor> FilteredLabors { get; set; }

    internal void GetFilteredItems()
    {
        FilteredLabors = allLabors
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

    private async Task CreateLabor(BaseLabor labor)
    {
        ProgressStart();

        var result = await BaseItemRepo.LaborAddAsync(labor, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add new Labor");
            return;
        }

        result.Data.CalculateCost(defaultValue.TopFica, defaultValue.Fica, defaultValue.FutaSuta, defaultValue.WorkersComp);

        allLabors.Add(result.Data);
        GetFilteredItems();

        BaseItemHub.SendLaborCreate(new BaseItemHubLaborCreatedMessage(result.Data.Id));

        ProgressStop();
    }

    private async Task<Result> UpdateLabor(BaseLabor newLabor)
    {
        ProgressStart();

        var result = await BaseItemRepo.LaborUpdateFromILaborAsync(newLabor.Id, newLabor, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Labor!");
            return Result.Fail();
        }

        newLabor.CalculateCost(defaultValue.TopFica, defaultValue.Fica, defaultValue.FutaSuta, defaultValue.WorkersComp);

        GetFilteredItems();

        BaseItemHub.SendLaborUpdate(new BaseItemHubLaborUpdatedMessage(newLabor.Id));

        ProgressStop();
        return Result.Ok();
    }

    private async void DeleteLabor(BaseLabor labor)
    {
        ProgressStart();

        var result = await BaseItemRepo.LaborDeleteAsync(labor.Id);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Labor!");
            return;
        }

        allLabors.Remove(labor);
        FilteredLabors.Remove(labor);
        
        BaseItemHub.SendLaborDelete(new BaseItemHubLaborDeletedMessage(labor.Id));

        ProgressStop();
    }

    #endregion

    #region Export

    internal async void ExportAsExcel()
    {
        var properties = new ExcelExportProperties();
        properties.Columns = new List<GridColumn>
        {
            new() { Field = nameof(BaseLabor.Description), HeaderText = "Description", Width = "300" },
            new() { Field = nameof(BaseLabor.Salary), HeaderText = "Salary", Width = "70", Format = "C2"},
            new() { Field = nameof(BaseLabor.Cost), HeaderText = "Cost", Width = "70", Format = "C2"},

            new() { Field = nameof(BaseLabor.HrsYear), HeaderText = "Hrs Year", Width = "70" },
            new() { Field = nameof(BaseLabor.HrsStandardYear), HeaderText = "Hrs Standard Year", Width = "70" },
            new() { Field = nameof(BaseLabor.HrsOvertimeYear), HeaderText = "Hrs Overtime Year", Width = "70" },

            new() { Field = nameof(BaseLabor.VacationsDays), HeaderText = "Vacation Days", Width = "70" },
            new() { Field = nameof(BaseLabor.HolydaysYear), HeaderText = "Holidays Year", Width = "70" },
            new() { Field = nameof(BaseLabor.SickDaysYear), HeaderText = "Sick Days Year", Width = "70" },
            new() { Field = nameof(BaseLabor.BonusYear), HeaderText = "Bonus Year", Width = "70" },
            new() { Field = nameof(BaseLabor.HealthYear), HeaderText = "Health Year", Width = "70" },
            new() { Field = nameof(BaseLabor.HealthYear), HeaderText = "Life Ins Year", Width = "70" },

            new() { Field = nameof(BaseLabor.Percentage401), HeaderText = "401 %", Width = "70", Format = "P2"},

            new() { Field = nameof(BaseLabor.MeetingsHrsYear), HeaderText = "Meeting Hrs Year", Width = "70" },
            new() { Field = nameof(BaseLabor.OfficeHrsYear), HeaderText = "Office Hrs Year", Width = "70" },
            new() { Field = nameof(BaseLabor.TrainingHrsYear), HeaderText = "Training Hrs Year", Width = "70" },
            new() { Field = nameof(BaseLabor.UniformsYear), HeaderText = "Uniform Year", Width = "70" },
            new() { Field = nameof(BaseLabor.SafetyYear), HeaderText = "Safety Year", Width = "70" },
        };
        
        properties.Header = ExportTemplate.GetExcelHeader(colspan: 18, "Labors Report");
        properties.Theme = ExportTemplate.GetExcelTheme();
        properties.FileName = "TechGroupOne Labors.xlsx";

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