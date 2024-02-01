using APU.DataV2.Repositories;
using APU.WebApp.Services.Components;
using Syncfusion.Blazor.Inputs;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.APUComponents;

public class APUEquipmentSelectorVM : ComponentBase, IPopup, IDisposable
{
	#region BaseItem Hub

    [Inject]
    protected BaseItemHubClient BaseItemHub { get; set; }

	private void InitializeBaseItemHub()
	{
        BaseItemHub.StartHub(NavM, "Apu - Equipment Sel.");

        BaseItemHub.EquipmentCreated = BaseItemHub_EquipmentCreated;
        BaseItemHub.EquipmentUpdated = BaseItemHub_EquipmentUpdated;
        BaseItemHub.EquipmentDeleted = BaseItemHub_EquipmentDeleted;
	}

	private async void BaseItemHub_EquipmentCreated(BaseItemHubEquipmentCreatedMessage message)
	{
		if (baseEquipments.Count == 0)
			return;

		var result = await BaseItemRepo.EquipmentGetAsync(message.baseEquipmentId);
		if (!result.IsSuccess())
			return;

		baseEquipments.Add(result.Data);
		baseEquipments = baseEquipments.OrderByDescending(q => q.LastUpdatedAt).ToList();
		GetFilteredItems();
	}

	private async void BaseItemHub_EquipmentUpdated(BaseItemHubEquipmentUpdatedMessage message)
	{
		if (baseEquipments.Count == 0)
			return;

		var result = await BaseItemRepo.EquipmentGetAsync(message.baseEquipmentId);
		if (!result.IsSuccess())
			return;

		baseEquipments = baseEquipments.ReplaceItem(result.Data, true);
		GetFilteredItems();
	}

	private void BaseItemHub_EquipmentDeleted(BaseItemHubEquipmentDeletedMessage message)
	{
		if (baseEquipments.Count == 0)
			return;

		var baseEquipment = baseEquipments.FirstOrDefault(q => q.Id == message.baseEquipmentId);
		if (baseEquipment is null)
			return;

		baseEquipments.Remove(baseEquipment);
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

    internal SfGrid<BaseEquipment> SfGrid { get; set; }

    internal async void GetSelectedRecords(RowSelectEventArgs<BaseEquipment> args)
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
	        baseEquipments = new List<BaseEquipment>();
	        FilteredEquipments = new List<BaseEquipment>();
            PopupService.Add(this);

            InitializeBaseItemHub();
        }

        base.OnAfterRender(firstRender);
    }

    #endregion

    #region Items

    private List<BaseEquipment> baseEquipments;

    #endregion
    #region FilteredItems

    internal List<BaseEquipment> FilteredEquipments { get; set; }

    internal void GetFilteredItems()
    {
        FilteredEquipments = baseEquipments
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

            if (baseEquipments.Count == 0)
            {
	            var result = await BaseItemRepo.EquipmentGetAllAsync();
                if (!result.IsSuccess())
                    return;

                baseEquipments = result.Data.OrderByDescending(q => q.LastUpdatedAt).ToList();

                GetFilteredItems();
            }

            //if (no == 0)
            //    Top = 25;
            //else
            //    Top = 25 + 26 + no * 29;
            Top = 25;

            PopupCss = "popup-equipmentselector-show";
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

    public Action<BaseEquipment> ItemSelected { get; set; }    

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