using System.Collections.ObjectModel;
using APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;
using APU.WebApp.Shared.Dialogs;
using APU.WebApp.Utils;
using Syncfusion.Blazor.Inputs;

#pragma warning disable BL0005

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class BaseMaterialsPageVM : PageVMBase, IDisposable
{
    #region BaseItem Hub

    [Inject]
    protected BaseItemHubClient BaseItemHub { get; set; }

    private void InitializeBaseItemHub()
    {
        if (BaseItemHub is null)
            return;

        BaseItemHub.StartHub(NavM, "Materials");

        BaseItemHub.MaterialCreated = BaseItemHub_MaterialCreated;
        BaseItemHub.MaterialUpdated = BaseItemHub_MaterialUpdated;
        BaseItemHub.MaterialDeleted = BaseItemHub_MaterialDeleted;
    }

    private async void BaseItemHub_MaterialCreated(BaseItemHubMaterialCreatedMessage message)
    {
	    if (allMaterials.Count == 0)
		    return;

	    var result = await BaseItemRepo.MaterialGetAsync(message.baseMaterialId);
	    if (!result.IsSuccess())
		    return;

	    allMaterials.Add(result.Data);
        GetFilteredItems();

        await InvokeAsync(StateHasChanged);
    }

    private async void BaseItemHub_MaterialUpdated(BaseItemHubMaterialUpdatedMessage message)
    {
	    if (allMaterials.Count == 0)
		    return;

	    var result = await BaseItemRepo.MaterialGetAsync(message.baseMaterialId);
	    if (!result.IsSuccess())
		    return;

	    allMaterials = allMaterials.ReplaceItem(result.Data);
	    GetFilteredItems();

        await InvokeAsync(StateHasChanged);
    }

    private async void BaseItemHub_MaterialDeleted(BaseItemHubMaterialDeletedMessage message)
    {
	    if (allMaterials.Count == 0)
			return;

	    var baseMaterial = allMaterials.FirstOrDefault(q => q.Id == message.baseMaterialId);
	    if (baseMaterial is null)
			return;

	    allMaterials.Remove(baseMaterial);
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

            ConfirmationDialogMaterial.Submit = DeleteMaterial;
            MaterialManager.Submit = MaterialManagerSubmit;

            await GetLIU();

            await GetAllMaterials();
            GetFilteredItems();
            StateHasChanged();

            InitializeBaseItemHub();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region ElementRefs - Dialogs

    internal DlgConfirmation<BaseMaterial> ConfirmationDialogMaterial { get; set; }   

    #endregion
    #region Dialog - Material Manager

    internal DlgMaterialManager MaterialManager { get; set; }

    internal void MaterialManagerOpen()
    {
        if (MaterialManager.IsVisible)
            return;

        if (Grid.SelectedRecords.Count > 0)
        {
            var material = Grid.SelectedRecords[0];
            MaterialManager.Open(ManagerOpenMode.Duplicate, material);
        }
        else
        {
            MaterialManager.Open(ManagerOpenMode.Create, null);
        }
    }

    internal async void MaterialManagerSubmit((ManagerSubmitMode mode, IMaterial material) p)
    {
        if (p.material is not BaseMaterial baseMaterial)
            return;

        if (p.mode is ManagerSubmitMode.Create or ManagerSubmitMode.Duplicate)
            await CreateMaterial(baseMaterial);
    }

    #endregion

    #region ElementRef - SfGrid

    internal SfGrid<BaseMaterial> Grid { get; set; }

    internal async void DataGridOnActionBegin(ActionEventArgs<BaseMaterial> args)
    {
        if (args.RequestType == SfGridAction.Save)
        {
            var result = await UpdateMaterial(args.Data);
            if (!result.IsSuccess())
                args.Cancel = true;
        }
    }

    internal void DataGridCommandClick(CommandClickEventArgs<BaseMaterial> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            ConfirmationDialogMaterial.Open("Are you sure to delete the following Base Material?", args.RowData.Description, args.RowData);
            args.Cancel = true;
        }
    }

    #endregion

    #region Filters

    private string filterText { get; set; } = "";

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

    private List<BaseMaterial> allMaterials;
    private async Task GetAllMaterials()
    {
        ProgressStart();

        var result = await BaseItemRepo.MaterialGetAllAsync(true);
        if (!result.IsSuccess())
        {
            allMaterials = new List<BaseMaterial>();
            ShowError("Failed to collect Materials!");
            return;
        }

        allMaterials = result.Data;

        ProgressStop(false);
    }

    public ObservableCollection<BaseMaterial> FilteredMaterials { get; set; }

    internal void GetFilteredItems()
    {
        FilteredMaterials = allMaterials
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

    private async Task CreateMaterial(BaseMaterial material)
    {
        ProgressStart();

        var result = await BaseItemRepo.MaterialAddAsync(material, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add new Material!");
            return;
        }

        allMaterials.Add(result.Data);
        GetFilteredItems();

        BaseItemHub.SendMaterialCreate(new BaseItemHubMaterialCreatedMessage(result.Data.Id));

        ProgressStop();
    }

    private async Task<Result> UpdateMaterial(BaseMaterial newMaterial)
    {
        ProgressStart();

        var result = await BaseItemRepo.MaterialUpdateFromIMaterialAsync(newMaterial.Id, newMaterial, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Material!");
            return Result.Fail();
        }

        GetFilteredItems();

        BaseItemHub.SendMaterialUpdate(new BaseItemHubMaterialUpdatedMessage(newMaterial.Id));

        ProgressStop();
        return Result.Ok();
    }

    private async void DeleteMaterial(BaseMaterial material)
    {
        ProgressStart();

        var result = await BaseItemRepo.MaterialDeleteAsync(material.Id);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Material!");
            return;
        }

        allMaterials.Remove(material);
        FilteredMaterials.Remove(material);
        
        BaseItemHub.SendMaterialDelete(new BaseItemHubMaterialDeletedMessage(material.Id));

        ProgressStop();
    }

    #endregion

    #region Export

    internal async void ExportAsExcel()
    {
        var properties = new ExcelExportProperties();
        properties.Columns = new List<GridColumn>
        {
            new() { Field = nameof(BaseMaterial.Description), HeaderText = "Description", Width = "500" },
            new() { Field = nameof(BaseMaterial.Unit), HeaderText = "Unit", Width = "50" },
            new() { Field = nameof(BaseMaterial.Quantity), HeaderText = "Qty", Width = "50"},
            new() { Field = nameof(BaseMaterial.Price), HeaderText = "Price", Width = "100", Format = "C2" },
            new() { Field = nameof(BaseMaterial.Vendor), HeaderText = "Vendor", Width = "130" },
            new() { Field = nameof(BaseMaterial.Phone), HeaderText = "Phone", Width = "80" },
            new() { Field = nameof(BaseMaterial.Link), HeaderText = "Link", Width = "300" }
        };
        
        properties.Header = ExportTemplate.GetExcelHeader(colspan: 7, "Materials Report");
        properties.Theme = ExportTemplate.GetExcelTheme();
        properties.FileName = "TechGroupOne Materials.xlsx";

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