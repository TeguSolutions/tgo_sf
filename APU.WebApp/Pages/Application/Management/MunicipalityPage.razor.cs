using APU.WebApp.Pages.Application.Management.Dialogs;
using APU.WebApp.Shared.Dialogs;
using Syncfusion.Blazor.Inputs;
using System.Collections.ObjectModel;

namespace APU.WebApp.Pages.Application.Management;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.SupervisorText}")]
public class MunicipalityVM : PageVMBase
{
    #region ElementRefs - Dialogs

    internal DlgConfirmation<Municipality> ConfirmationDialog { get; set; }   

    #endregion

    #region ElementRef - Certificate Manager

    internal DlgMunicipalityManager MunicipalityManager { get; set; }
    internal void MunicipalityManagerOpen()
    {
        MunicipalityManager.Open(Counties);
    }

    #endregion

    #region ElementRefs - Grid

    internal SfGrid<Municipality> Grid { get; set; }

    internal async void DataGridOnActionBegin(ActionEventArgs<Municipality> args)
    {
        if (args.RequestType == SfGridAction.Save)
        {
            var result = await UpdateMunicipalityAsync(args.Data);
            if (!result.IsSuccess())
                args.Cancel = true;
        }
    }

    internal void DataGridCommandClick(CommandClickEventArgs<Municipality> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            ConfirmationDialog.Open("Are you sure to delete the following Municipality?", args.RowData.Name, args.RowData);
            args.Cancel = true;
        }
    }


    internal void CountyChanged(int countyId, Municipality municipality)
    {
        municipality.CountyId = countyId;
        municipality.County = Counties.FirstOrDefault(q => q.Id == countyId);
    }

    #endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderHome));
            await EventAggregator.PublishAsync(new HeaderLinkMessage());

            ConfirmationDialog.Submit = DeleteMunicipality;
            MunicipalityManager.Submit = CreateMunicipality;

            await GetLIU();

            await GetCounties();
            
            await GetAllMunicipalities();
            GetFilteredItems();
            await Grid.Refresh();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region Counties

    internal List<County> Counties { get; set; }

    private async Task GetCounties()
    {
        ProgressStart();

        var result = await DefinitionsRepo.CountyGetAllAsync();
        if (!result.IsSuccess())
        {
            Counties = new List<County>();
            ShowError("Failed to load Counties!");
            return;
        }

        Counties = result.Data.OrderBy(q => q.Name).ToList();

        ProgressStop(false);
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

    private List<Municipality> allMunicipalities;

    private async Task GetAllMunicipalities()
    {
        ProgressStart();

        var result = await MunicipalityRepo.GetAllAsync();
        if (!result.IsSuccess())
        {
            allMunicipalities = new List<Municipality>();
            ShowError("Failed to collect Municipalities!");
            return;
        }

        allMunicipalities = result.Data.OrderBy(q => q.County?.Name).ToList();

        ProgressStop();
    }

    public ObservableCollection<Municipality> FilteredMunicipalities { get; set; }

    internal async void GetFilteredItems()
    {
        FilteredMunicipalities = allMunicipalities
            .If(!string.IsNullOrWhiteSpace(filterText), q => 
                q.Where(o =>
                    o.Name.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.Name, filterText) ||
                    o.Address.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.Address, filterText) ||
                    o.Phone.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.Phone, filterText) ||
                    o.Fax.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.Fax, filterText) ||
                    o.Building.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.Building, filterText) ||
                    o.Bid.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.Bid, filterText) ||
                    o.BidSync.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.BidSync, filterText)
                )
            )
            .ToObservableCollection();

        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region CRUD

    private async void CreateMunicipality(Municipality municipality)
    {
        ProgressStart();

        var result = await MunicipalityRepo.AddAsync(municipality, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add new Municipality");
            return;
        }

        allMunicipalities.Add(municipality);
        GetFilteredItems();

        ProgressStop();
    }

    private async Task<Result> UpdateMunicipalityAsync(Municipality municipality)
    {
        ProgressStart();

        var result = await MunicipalityRepo.UpdateAsync(municipality, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Municipality");
            return Result.Fail();
        }

        GetFilteredItems();
        ProgressStop();

        return Result.Ok();
    }

    private async void DeleteMunicipality(Municipality municipality)
    {
        ProgressStart();

        var result = await MunicipalityRepo.DeleteAsync(municipality.Id);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Municipality");
            return;
        }

        allMunicipalities.Remove(municipality);
        GetFilteredItems();

        ProgressStop();
    }

    #endregion
}