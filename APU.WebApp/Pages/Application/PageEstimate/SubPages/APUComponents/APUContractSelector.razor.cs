using APU.DataV2.Repositories;
using APU.WebApp.Services.Components;
using Syncfusion.Blazor.Inputs;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.APUComponents;

public class APUContractSelectorVM : ComponentBase, IPopup, IDisposable
{
	#region BaseItem Hub

    [Inject]
    protected BaseItemHubClient BaseItemHub { get; set; }

	private void InitializeBaseItemHub()
	{
        BaseItemHub.StartHub(NavM, "Apu - Contract Sel.");

        BaseItemHub.ContractCreated = BaseItemHub_ContractCreated;
        BaseItemHub.ContractUpdated = BaseItemHub_ContractUpdated;
        BaseItemHub.ContractDeleted = BaseItemHub_ContractDeleted;
	}

	private async void BaseItemHub_ContractCreated(BaseItemHubContractCreatedMessage message)
	{
        if (baseContracts.Count == 0)
            return;

        var result = await BaseItemRepo.ContractGetAsync(message.baseContractId);
        if (!result.IsSuccess())
            return;

        baseContracts.Add(result.Data);
        baseContracts = baseContracts.OrderByDescending(q => q.LastUpdatedAt).ToList();
        GetFilteredItems();
	}

	private async void BaseItemHub_ContractUpdated(BaseItemHubContractUpdatedMessage message)
	{
		if (baseContracts.Count == 0)
			return;

		var result = await BaseItemRepo.ContractGetAsync(message.baseContractId);
		if (!result.IsSuccess())
			return;

		baseContracts = baseContracts.ReplaceItem(result.Data, true);
        GetFilteredItems();
	}

	private void BaseItemHub_ContractDeleted(BaseItemHubContractDeletedMessage message)
	{
		if (baseContracts.Count == 0)
			return;

		var baseContract = baseContracts.FirstOrDefault(q => q.Id == message.baseContractId);
        if (baseContract is null)
            return;

        baseContracts.Remove(baseContract);
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

    internal SfGrid<BaseContract> SfGrid { get; set; }

    internal async void GetSelectedRecords(RowSelectEventArgs<BaseContract> args)
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
	        baseContracts = new List<BaseContract>();
	        FilteredContracts = new List<BaseContract>();
            PopupService.Add(this);

            InitializeBaseItemHub();
        }

        base.OnAfterRender(firstRender);
    }

    #endregion

    #region Items

    private List<BaseContract> baseContracts;

    #endregion
    #region FilteredItems

    internal List<BaseContract> FilteredContracts { get; set; }

    internal void GetFilteredItems()
    {
        FilteredContracts = baseContracts
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

            if (baseContracts.Count == 0)
            {
	            var result = await BaseItemRepo.ContractGetAllAsync();
	            if (!result.IsSuccess())
		            return;

	            baseContracts = result.Data.OrderByDescending(q => q.LastUpdatedAt).ToList();

                GetFilteredItems();
            }

            //Top = 25 + no * 29;
            Top = 25;

            PopupCss = "popup-contractselector-show";
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

    public Action<BaseContract> ItemSelected { get; set; }    

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