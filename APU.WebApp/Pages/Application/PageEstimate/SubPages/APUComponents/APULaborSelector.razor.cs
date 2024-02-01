using APU.DataV2.Repositories;
using APU.WebApp.Services.Components;
using Syncfusion.Blazor.Inputs;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.APUComponents;

public class APULaborSelectorVM : ComponentBase, IPopup, IDisposable
{
    #region BaseItem Hub

    [Inject]
    protected BaseItemHubClient BaseItemHub { get; set; }

    private void InitializeBaseItemHub()
    {
        BaseItemHub.StartHub(NavM, "Apu - Labor Sel.");

        BaseItemHub.LaborCreated = BaseItemHub_LaborCreated;
        BaseItemHub.LaborUpdated = BaseItemHub_LaborUpdated;
        BaseItemHub.LaborDeleted = BaseItemHub_LaborDeleted;
    }

    private async void BaseItemHub_LaborCreated(BaseItemHubLaborCreatedMessage message)
    {
        if (baseLabors.Count == 0)
            return;

        var result = await BaseItemRepo.LaborGetAsync(message.baseLaborId);
        if (!result.IsSuccess())
            return;

        result.Data.CalculateCost(defaultValue.TopFica, defaultValue.Fica, defaultValue.FutaSuta, defaultValue.WorkersComp);
        baseLabors.Add(result.Data);
        baseLabors = baseLabors.OrderByDescending(q => q.LastUpdatedAt).ToList();
        GetFilteredItems();
    }

    private async void BaseItemHub_LaborUpdated(BaseItemHubLaborUpdatedMessage message)
    {
        if (baseLabors.Count == 0)
            return;

        var result = await BaseItemRepo.LaborGetAsync(message.baseLaborId);
        if (!result.IsSuccess())
            return;

        result.Data.CalculateCost(defaultValue.TopFica, defaultValue.Fica, defaultValue.FutaSuta, defaultValue.WorkersComp);
        baseLabors = baseLabors.ReplaceItem(result.Data, true);
        GetFilteredItems();
    }

    private void BaseItemHub_LaborDeleted(BaseItemHubLaborDeletedMessage message)
    {
        if (baseLabors.Count == 0)
            return;

        var baseLabor = baseLabors.FirstOrDefault(q => q.Id == message.baseLaborId);
        if (baseLabor is null)
            return;

        baseLabors.Remove(baseLabor);
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

    internal SfGrid<BaseLabor> SfGrid { get; set; }

    internal async void GetSelectedRecords(RowSelectEventArgs<BaseLabor> args)
    {
        await SfGrid.ClearSelectionAsync();
        ItemSelected?.Invoke(args.Data);
        ClosePopup();
    }

    #endregion

    #region Lifecycle

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
	        baseLabors = new List<BaseLabor>();
	        FilteredLabors = new List<BaseLabor>();
	        PopupService.Add(this);

            InitializeBaseItemHub();
        }

        base.OnAfterRender(firstRender);
    }

    #endregion

    #region DefaultValues

    private DefaultValue defaultValue;

    internal void SetDefaultValue(DefaultValue defaultvalue)
    {
        defaultValue = defaultvalue;
    }

    #endregion

    #region Items

    private List<BaseLabor> baseLabors;

    #endregion

    #region FilteredItems

    internal List<BaseLabor> FilteredLabors { get; set; }

    internal void GetFilteredItems()
    {
        FilteredLabors = baseLabors
            .If(!string.IsNullOrWhiteSpace(filterText), q => 
                q.Where(o =>
                    o.Description.ToLower().Contains(filterText.ToLower()) ||
                    TeguStringComparer.CompareToFilterBool(o.Description, filterText))
            ).ToList();
    }     

    #endregion


    #region Popup Control

    internal int Top { get; set; }
    internal string TopPx => Top + "px";

    internal string PopupCss { get; set; } 

    internal async void TogglePopup()
    {
        if (string.IsNullOrWhiteSpace(PopupCss))
        {
            PopupService.ClosePopups(this);

            if (baseLabors.Count == 0)
            {
	            var result = await BaseItemRepo.LaborGetAllAsync();
	            if (!result.IsSuccess())
		            return;

	            baseLabors = result.Data.OrderBy(q => q.LastUpdatedAt).ToList();
	            if (defaultValue is not null)
	            {
		            foreach (var baseLabor in baseLabors)
			            baseLabor.CalculateCost(defaultValue.TopFica, defaultValue.Fica, defaultValue.FutaSuta, defaultValue.WorkersComp);
	            }

                GetFilteredItems();
            }

            //Top = 25 + no * 29;
            Top = 25;

            PopupCss = "popup-laborselector-show";
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

    #region Selected Item

    public Action<BaseLabor> ItemSelected { get; set; }    

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