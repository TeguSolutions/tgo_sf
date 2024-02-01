using System.Collections.ObjectModel;
using APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;
using APU.WebApp.Shared.Dialogs;
using APU.WebApp.Utils;
using Syncfusion.Blazor.Inputs;

#pragma warning disable BL0005

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class BaseEquipmentsPageVM : PageVMBase, IDisposable
{
	#region BaseItem Hub

	[Inject]
	protected BaseItemHubClient BaseItemHub { get; set; }

	private void InitializeBaseItemHub()
	{
        if (BaseItemHub is null)
            return;

		BaseItemHub.StartHub(NavM, "Equipments");

        BaseItemHub.EquipmentCreated = BaseItemHub_EquipmentCreated;
        BaseItemHub.EquipmentUpdated = BaseItemHub_EquipmentUpdated;
        BaseItemHub.EquipmentDeleted = BaseItemHub_EquipmentDeleted;
	}

	private async void BaseItemHub_EquipmentCreated(BaseItemHubEquipmentCreatedMessage message)
	{
		if (allEquipments.Count == 0)
			return;

		var result = await BaseItemRepo.EquipmentGetAsync(message.baseEquipmentId);
		if (!result.IsSuccess())
			return;

        allEquipments.Add(result.Data);
        GetFilteredItems();

        await InvokeAsync(StateHasChanged);
	}

	private async void BaseItemHub_EquipmentUpdated(BaseItemHubEquipmentUpdatedMessage message)
	{
		if (allEquipments.Count == 0)
			return;

		var result = await BaseItemRepo.EquipmentGetAsync(message.baseEquipmentId);
		if (!result.IsSuccess())
			return;

		allEquipments = allEquipments.ReplaceItem(result.Data);
		GetFilteredItems();

        await InvokeAsync(StateHasChanged);
	}

	private async void BaseItemHub_EquipmentDeleted(BaseItemHubEquipmentDeletedMessage message)
	{
        if (allEquipments.Count == 0)
            return;

        var baseEquipment = allEquipments.FirstOrDefault(q => q.Id == message.baseEquipmentId);
        if (baseEquipment is null)
            return;

        allEquipments.Remove(baseEquipment);
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

            ConfirmationDialogEquipment.Submit = DeleteEquipment;
            EquipmentManager.Submit = EquipmentManagerSubmit;

            await GetLIU();

            await GetAllEquipments();
            GetFilteredItems();
            StateHasChanged();

            InitializeBaseItemHub();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region ElementRefs - Dialogs

    internal DlgConfirmation<BaseEquipment> ConfirmationDialogEquipment { get; set; }   

    #endregion
    #region Dialog Equipment Manager

    internal DlgEquipmentManager EquipmentManager { get; set; }

    internal void EquipmentManagerOpen()
    {
        if (EquipmentManager.IsVisible)
            return;

        if (Grid.SelectedRecords.Count > 0)
        {
            var equipment = Grid.SelectedRecords[0];
            EquipmentManager.Open(ManagerOpenMode.Duplicate, equipment);
        }
        else
        {
            EquipmentManager.Open(ManagerOpenMode.Create, null);
        }
    }

    internal async void EquipmentManagerSubmit((ManagerSubmitMode mode, IEquipment equipment) p)
    {
        if (p.equipment is not BaseEquipment baseEquipment)
            return;

        if (p.mode is ManagerSubmitMode.Create or ManagerSubmitMode.Duplicate)
            await CreateEquipment(baseEquipment);
    }

    #endregion

    #region ElementRef - SfGrid

    internal SfGrid<BaseEquipment> Grid { get; set; }

    internal async void DataGridOnActionBegin(ActionEventArgs<BaseEquipment> args)
    {
        if (args.RequestType == SfGridAction.Save)
        {
            var result = await UpdateEquipment(args.Data);
            if (!result.IsSuccess())
                args.Cancel = true;
        }
    }

    internal void DataGridCommandClick(CommandClickEventArgs<BaseEquipment> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            ConfirmationDialogEquipment.Open("Are you sure to delete the following Base Equipment?", args.RowData.Description, args.RowData);
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
            GetFilteredItems();
        }
    }

    #endregion

    #region Items / FilteredItems

    private List<BaseEquipment> allEquipments;
    private async Task GetAllEquipments()
    {
        ProgressStart();

        var result = await BaseItemRepo.EquipmentGetAllAsync(true);
        if (!result.IsSuccess())
        {
            allEquipments = new List<BaseEquipment>();
            ShowError("Failed to collect Equipments!");
            return;
        }

        allEquipments = result.Data;

        ProgressStop(false);
    }

    public ObservableCollection<BaseEquipment> FilteredEquipments { get; set; }

    internal async void GetFilteredItems()
    {
        FilteredEquipments = allEquipments
            .If(!string.IsNullOrWhiteSpace(filterText), q => 
                q.Where(o =>
                    o.Description.ToLower().Contains(filterText.ToLower()) ||
                    TeguStringComparer.CompareToFilterBool(o.Description, filterText))
            )
            .OrderBy(q => q.Description)
            .ToObservableCollection();

        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region CRUD

    private async Task CreateEquipment(BaseEquipment equipment)
    {
        ProgressStart();

        var result = await BaseItemRepo.EquipmentAddAsync(equipment, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add new Equipment!");
            return;
        }

        allEquipments.Add(result.Data);
        GetFilteredItems();

        BaseItemHub.SendEquipmentCreate(new BaseItemHubEquipmentCreatedMessage(result.Data.Id));

        ProgressStop();
    }

    private async Task<Result> UpdateEquipment(BaseEquipment newEquipment)
    {
        ProgressStart();

        var result = await BaseItemRepo.EquipmentUpdateFromIEquipmentAsync(newEquipment.Id, newEquipment, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Equipment!");
            return Result.Fail();
        }

        GetFilteredItems();

        BaseItemHub.SendEquipmentUpdate(new BaseItemHubEquipmentUpdatedMessage(newEquipment.Id));

        ProgressStop();
        return Result.Ok();
    }

    private async void DeleteEquipment(BaseEquipment equipment)
    {
        ProgressStart();

        var result = await BaseItemRepo.EquipmentDeleteAsync(equipment.Id);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Equipment");
            return;
        }

        allEquipments.Remove(equipment);
        FilteredEquipments.Remove(equipment);
   
        BaseItemHub.SendEquipmentDelete(new BaseItemHubEquipmentDeletedMessage(equipment.Id));

        ProgressStop();
    }

    #endregion

    #region Export

    internal async void ExportAsExcel()
    {
        var properties = new ExcelExportProperties();
        properties.Columns = new List<GridColumn>
        {
            new() { Field = nameof(BaseEquipment.Description), HeaderText = "Description", Width = "500" },
            new() { Field = nameof(BaseEquipment.Unit), HeaderText = "Unit", Width = "50" },
            new() { Field = nameof(BaseEquipment.Quantity), HeaderText = "Qty", Width = "50"},
            new() { Field = nameof(BaseEquipment.Price), HeaderText = "Price", Width = "100", Format = "C2" },
            new() { Field = nameof(BaseEquipment.Vendor), HeaderText = "Vendor", Width = "130" },
            new() { Field = nameof(BaseEquipment.Phone), HeaderText = "Phone", Width = "80" },
            new() { Field = nameof(BaseEquipment.Link), HeaderText = "Link", Width = "300" }
        };
        
        properties.Header = ExportTemplate.GetExcelHeader(colspan: 7, "Equipments Report");
        properties.Theme = ExportTemplate.GetExcelTheme();
        properties.FileName = "TechGroupOne Equipments.xlsx";

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