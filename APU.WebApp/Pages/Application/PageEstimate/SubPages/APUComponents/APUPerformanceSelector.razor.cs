using APU.DataV2.Repositories;
using APU.WebApp.Services.Components;
using Syncfusion.Blazor.Inputs;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.APUComponents;

public class APUPerformanceSelectorVM : ComponentBase, IPopup, IDisposable
{
    #region BaseItem Hub

    [Inject]
    protected BaseItemHubClient BaseItemHub { get; set; }

    private void InitializeBaseItemHub()
    {
        BaseItemHub.StartHub(NavM, "Apu - Performance Sel.");

        BaseItemHub.PerformanceCreated = BaseItemHub_PerformanceCreated;
        BaseItemHub.PerformanceUpdated = BaseItemHub_PerformanceUpdated;
        BaseItemHub.PerformanceDeleted = BaseItemHub_PerformanceDeleted;
    }

    private async void BaseItemHub_PerformanceCreated(BaseItemHubPerformanceCreatedMessage message)
    {
        if (basePerformances.Count == 0)
            return;

        var result = await BaseItemRepo.PerformanceGetAsync(message.basePerformanceId);
        if (!result.IsSuccess())
            return;

        basePerformances.Add(result.Data);
        basePerformances = basePerformances.OrderByDescending(q => q.LastUpdatedAt).ToList();
        GetFilteredItems();
    }

    private async void BaseItemHub_PerformanceUpdated(BaseItemHubPerformanceUpdatedMessage message)
    {
        if (basePerformances.Count == 0)
            return;

        var result = await BaseItemRepo.PerformanceGetAsync(message.basePerformanceId);
        if (!result.IsSuccess())
            return;

        basePerformances = basePerformances.ReplaceItem(result.Data, true);
        GetFilteredItems();
    }

    private void BaseItemHub_PerformanceDeleted(BaseItemHubPerformanceDeletedMessage message)
    {
        if (basePerformances.Count == 0)
            return;

        var basePerformance = basePerformances.FirstOrDefault(q => q.Id == message.basePerformanceId);
        if (basePerformance is null)
            return;

        basePerformances.Remove(basePerformance);
        GetFilteredItems();
    }

    #endregion

    #region Services

    [Inject] 
    protected NavigationManager NavM { get; set; }

    [Inject] 
    protected PopupService PopupService { get; set; }

    [Inject] 
    protected BaseItemRepository BaseItemRepo { get; set; }  

    #endregion

    #region ElementRefs

    internal SfTextBox TbSearch { get; set; }    

    internal SfGrid<BasePerformance> SfGrid { get; set; }

    internal async void GetSelectedRecords(RowSelectEventArgs<BasePerformance> args)
    {
        await SfGrid.ClearSelectionAsync();
        PerformanceSelected?.Invoke(args.Data);
        ClosePopup();
    }

    #endregion

    #region Lifecycle

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            basePerformances = new List<BasePerformance>();
            FilteredPerformances = new List<BasePerformance>();
            PopupService.Add(this);

            InitializeBaseItemHub();
        }

        base.OnAfterRender(firstRender);
    }

    #endregion

    #region Items

    private List<BasePerformance> basePerformances;

    #endregion
    #region FilteredItems

    internal List<BasePerformance> FilteredPerformances { get; set; }

    internal void GetFilteredItems()
    {
        FilteredPerformances = basePerformances
            .If(!string.IsNullOrWhiteSpace(filterText), q =>
                q.Where(o =>
                    o.Description.ToLower().Contains(filterText.ToLower()) ||
                    TeguStringComparer.CompareToFilterBool(o.Description, filterText))
            ).ToList();
    }        

    #endregion

    #region Popup Control

    internal string PopupCss { get; set; } 

    internal async void TogglePopup()
    {
        if (string.IsNullOrWhiteSpace(PopupCss))
        {
            PopupService.ClosePopups(this);

            if (basePerformances.Count == 0)
            {
                var result = await BaseItemRepo.PerformanceGetAllAsync();
                if (!result.IsSuccess())
                    return;

                basePerformances = result.Data.OrderByDescending(q => q.LastUpdatedAt).ToList();

                GetFilteredItems();
            }

            PopupCss = "popup-performanceselector-show";
            StateHasChanged();
        }
        else
        {
            ClosePopup();
        }
    }

    public void ClosePopup()
    {
        PopupCss = "";
        StateHasChanged();

        filterText = "";
        TbSearch.Value = "";
        GetFilteredItems();
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

    #region Selected Item



    public Action<BasePerformance> PerformanceSelected { get; set; }    

    #endregion

    #region IDisposable

    public void Dispose()
    {
        PopupService.Remove(this);

        BaseItemHub?.Dispose();
        BaseItemHub = null;
    }    

    #endregion
}