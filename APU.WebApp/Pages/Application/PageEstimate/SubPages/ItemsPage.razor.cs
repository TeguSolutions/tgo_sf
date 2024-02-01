using System.Collections.ObjectModel;
using APU.WebApp.Services.Navigation;
using Syncfusion.Blazor.Inputs;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class ItemsPageVM : PageVMBase
{
    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderEstimate));

            ProgressStart();

            if (!await GetDefaultValue())
            {
                ProgressStop();
                return;
            }

            await GetAllApus();
            GetFilteredItems();

            //await Grid.Refresh();

            ProgressStop();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region DefaultValues

    private DefaultValue defaultValue;

    private async Task<bool> GetDefaultValue()
    {
        var result = await DefaultRepo.GetAsync();
        if (!result.IsSuccess())
        {
            ShowError("Failed to collect the Default Values!");
            return false;
        }

        defaultValue = result.Data;

        return true;
    }

    #endregion

    #region Element References

    internal SfGrid<Apu> Grid { get; set; }

    #endregion

    #region Filters

    internal string filterText = "";

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

    private List<Apu> allApus;
    private async Task GetAllApus()
    {
        var result = await ApuRepo.GetLineItemsAsync(includeApuItems: true, includeProject: true);
        if (!result.IsSuccess())
        {
            ShowError("Failed to collect Line Item Apus!");
            return;
        }

        allApus = result.Data.OrderBy(q => q.Description).ToList();
        foreach (var apu in allApus)
            apu.CalculateAll(defaultValue, apu.Project);
    }

    public ObservableCollection<Apu> FilteredItems { get; set; }

    internal async void GetFilteredItems()
    {
        FilteredItems = allApus
            .Where(q => !string.IsNullOrWhiteSpace(q.Description))
            .If(!string.IsNullOrWhiteSpace(filterText), q => 
                q.Where(o =>
                    o.Description.ToLower().Contains(filterText.ToLower()) ||
                    TeguStringComparer.CompareToFilterBool(o.Description, filterText))
            )
            .ToObservableCollection();

        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Apu Navigation

    internal void NavigateToApu(Apu apu)
    {
        if (apu is null)
            return;

        JS.OpenUrlInNewTab(NavM.BaseUri + NavS.Estimates.APUView + "?view=" + apu.Id);
    }

    #endregion
}