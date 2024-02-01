using System.Collections.ObjectModel;
using APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;
using APU.WebApp.Shared.Dialogs;
using APU.WebApp.Utils;
using Syncfusion.Blazor.Inputs;

#pragma warning disable BL0005

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class BaseContractsPageVM : PageVMBase, IDisposable
{
	#region BaseItem Hub

	[Inject]
	protected BaseItemHubClient BaseItemHub { get; set; }

	private void InitializeBaseItemHub()
	{
        if (BaseItemHub is null)
            return;

		BaseItemHub.StartHub(NavM, "Contracts");

		BaseItemHub.ContractCreated = BaseItemHub_ContractCreated;
		BaseItemHub.ContractUpdated = BaseItemHub_ContractUpdated;
		BaseItemHub.ContractDeleted = BaseItemHub_ContractDeleted;
	}

	private async void BaseItemHub_ContractCreated(BaseItemHubContractCreatedMessage message)
	{
        if (allContracts.Count == 0)
            return;

        var result = await BaseItemRepo.ContractGetAsync(message.baseContractId);
        if (!result.IsSuccess())
	        return;

        allContracts.Add(result.Data);
        GetFilteredContracts();

        await InvokeAsync(StateHasChanged);
	}

	private async void BaseItemHub_ContractUpdated(BaseItemHubContractUpdatedMessage message)
	{
        if (allContracts.Count == 0)
            return;

        var result = await BaseItemRepo.ContractGetAsync(message.baseContractId);
        if (!result.IsSuccess())
	        return;

        allContracts = allContracts.ReplaceItem(result.Data);
        GetFilteredContracts();

        await InvokeAsync(StateHasChanged);
	}

	private async void BaseItemHub_ContractDeleted(BaseItemHubContractDeletedMessage message)
	{
        if (allContracts.Count == 0)
            return;

        var baseContract = allContracts.FirstOrDefault(q => q.Id == message.baseContractId);
        if (baseContract is null)
            return;

        allContracts.Remove(baseContract);
        GetFilteredContracts();

        await InvokeAsync(StateHasChanged);
	}

	#endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderEstimate));

            ConfirmationDialogContract.Submit = DeleteContract;
            ContractManager.Submit = ContractManagerSubmit;

            await GetLIU();

            await GetAllContracts();
            GetFilteredContracts();
            StateHasChanged();

            InitializeBaseItemHub();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region ElementRefs - Dialogs

    internal DlgConfirmation<BaseContract> ConfirmationDialogContract { get; set; }   

    #endregion
    #region Dialog Contract Manager

    internal DlgContractManager ContractManager { get; set; }

    internal void ContractManagerOpen()
    {
        if (ContractManager.IsVisible)
            return;

        if (Grid.SelectedRecords.Count > 0)
        {
            var contract = Grid.SelectedRecords[0];
            ContractManager.Open(ManagerOpenMode.Duplicate, contract);
        }
        else
        {
            ContractManager.Open(ManagerOpenMode.Create, null);
        }
    }

    internal async void ContractManagerSubmit((ManagerSubmitMode mode, IContract contract) p)
    {
        if (p.contract is not BaseContract baseContract)
            return;

        if (p.mode is ManagerSubmitMode.Create or ManagerSubmitMode.Duplicate)
            await CreateContract(baseContract);
    }

    #endregion

    #region ElementRef - SfGrid

    internal SfGrid<BaseContract> Grid { get; set; }

    internal async void DataGridOnActionBegin(ActionEventArgs<BaseContract> args)
    {
        if (args.RequestType == SfGridAction.Save)
        {
            var result = await UpdateContract(args.Data);
            if (!result.IsSuccess())
                args.Cancel = true;
        }
    }

    internal void DataGridCommandClick(CommandClickEventArgs<BaseContract> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            ConfirmationDialogContract.Open("Are you sure to delete the following Base Contract?", args.RowData.Description, args.RowData);
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
            GetFilteredContracts();
        }
    }

    #endregion

    #region Items / FilteredItems

    private List<BaseContract> allContracts;

    private async Task GetAllContracts()
    {
        ProgressStart();

        var result = await BaseItemRepo.ContractGetAllAsync(true);
        if (!result.IsSuccess())
        {
            allContracts = new List<BaseContract>();
            ShowError("Failed to collect Contracts!");
            return;
        }

        allContracts = result.Data;

        ProgressStop(false);
    }

    public ObservableCollection<BaseContract> FilteredContracts { get; set; }

    internal void GetFilteredContracts()
    {
        FilteredContracts = allContracts
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

    private async Task CreateContract(BaseContract contract)
    {
        ProgressStart();

        var result = await BaseItemRepo.ContractAddAsync(contract, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add new Contract!");
            return;
        }

        allContracts.Add(result.Data);
        GetFilteredContracts();

        BaseItemHub.SendContractCreate(new BaseItemHubContractCreatedMessage(result.Data.Id));

        ProgressStop();
    }

    private async Task<Result> UpdateContract(BaseContract newContract)
    {
        ProgressStart();

        var result = await BaseItemRepo.ContractUpdateFromIContractAsync(newContract.Id, newContract, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Contract!");
            return Result.Fail();
        }

        GetFilteredContracts();

        BaseItemHub.SendContractUpdate(new BaseItemHubContractUpdatedMessage(newContract.Id));

        ProgressStop();
        return Result.Ok();
    }

    private async void DeleteContract(BaseContract contract)
    {
        ProgressStart();

        var result = await BaseItemRepo.ContractDeleteAsync(contract.Id);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Contract!");
            return;
        }

        allContracts.Remove(contract);
        FilteredContracts.Remove(contract);
        
        BaseItemHub.SendContractDelete(new BaseItemHubContractDeletedMessage(contract.Id));

        ProgressStop();
    }

    #endregion

    #region Export

    internal async void ExportAsExcel()
    {
        var properties = new ExcelExportProperties();
        properties.Columns = new List<GridColumn>
        {
            new() { Field = nameof(BaseContract.Description), HeaderText = "Description", Width = "500" },
            new() { Field = nameof(BaseContract.Unit), HeaderText = "Unit", Width = "50" },
            new() { Field = nameof(BaseContract.Quantity), HeaderText = "Qty", Width = "50"},
            new() { Field = nameof(BaseContract.Price), HeaderText = "Price", Width = "100", Format = "C2" },
            new() { Field = nameof(BaseContract.Vendor), HeaderText = "Vendor", Width = "130" },
            new() { Field = nameof(BaseContract.Phone), HeaderText = "Phone", Width = "80" },
            new() { Field = nameof(BaseContract.Link), HeaderText = "Link", Width = "300" }
        };
        
        properties.Header = ExportTemplate.GetExcelHeader(colspan: 7, "Contracts Report");
        properties.Theme = ExportTemplate.GetExcelTheme();
        properties.FileName = "TechGroupOne Contracts.xlsx";

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