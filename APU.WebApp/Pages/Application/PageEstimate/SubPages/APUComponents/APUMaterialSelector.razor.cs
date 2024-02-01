using APU.DataV2.Repositories;
using APU.WebApp.Services.Components;
using Syncfusion.Blazor.Inputs;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.APUComponents;

public class APUMaterialSelectorVM : ComponentBase, IPopup, IDisposable
{
    #region BaseItem Hub

    [Inject]
    protected BaseItemHubClient BaseItemHub { get; set; }

    private void InitializeBaseItemHub()
    {
        BaseItemHub.StartHub(NavM, "Apu - Material Sel.");

        BaseItemHub.MaterialCreated = BaseItemHub_MaterialCreated;
        BaseItemHub.MaterialUpdated = BaseItemHub_MaterialUpdated;
        BaseItemHub.MaterialDeleted = BaseItemHub_MaterialDeleted;
    }

    private async void BaseItemHub_MaterialCreated(BaseItemHubMaterialCreatedMessage message)
    {
        if (baseMaterials.Count == 0)
            return;

        var result = await BaseItemRepo.MaterialGetAsync(message.baseMaterialId);
        if (!result.IsSuccess())
            return;

        baseMaterials.Add(result.Data);
        baseMaterials = baseMaterials.OrderByDescending(q => q.LastUpdatedAt).ToList();
        GetFilteredItems();
    }

    private async void BaseItemHub_MaterialUpdated(BaseItemHubMaterialUpdatedMessage message)
    {
	    if (baseMaterials.Count == 0)
		    return;

	    var result = await BaseItemRepo.MaterialGetAsync(message.baseMaterialId);
	    if (!result.IsSuccess())
		    return;

	    baseMaterials = baseMaterials.ReplaceItem(result.Data, true);
	    GetFilteredItems();
    }

    private void BaseItemHub_MaterialDeleted(BaseItemHubMaterialDeletedMessage message)
    {
	    if (baseMaterials.Count == 0)
		    return;

	    var baseMaterial = baseMaterials.FirstOrDefault(q => q.Id == message.baseMaterialId);
	    if (baseMaterial is null)
		    return;

	    baseMaterials.Remove(baseMaterial);
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

    [Inject]
    public IEventAggregator EventAggregator { get; set; }    

    #endregion

    #region ElementRefs

    internal SfTextBox TbSearch { get; set; }    

    internal SfGrid<BaseMaterial> SfGrid { get; set; }

    internal async void GetSelectedRecords(RowSelectEventArgs<BaseMaterial> args)
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
	        baseMaterials = new List<BaseMaterial>();
            FilteredMaterials = new List<BaseMaterial>();
            PopupService.Add(this);

            InitializeBaseItemHub();
        }

        base.OnAfterRender(firstRender);
    }

    #endregion

    #region Items

    private List<BaseMaterial> baseMaterials;

    #endregion
    #region FilteredItems

    internal List<BaseMaterial> FilteredMaterials { get; set; }

    internal void GetFilteredItems()
    {
        FilteredMaterials = baseMaterials
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

            if (baseMaterials.Count == 0)
            {
	            var result = await BaseItemRepo.MaterialGetAllAsync();
                if (!result.IsSuccess())
                    return;

                baseMaterials = result.Data.OrderByDescending(q => q.LastUpdatedAt).ToList();

                GetFilteredItems();
            }

            //Top = 25 + no * 29;
            Top = 25;

            PopupCss = "popup-materialselector-show";
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

    public Action<BaseMaterial> ItemSelected { get; set; }    

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