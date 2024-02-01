using APU.DataV2.EntityModels;
using APU.DataV2.Repositories;
using APU.WebApp.Services.Components;
using Syncfusion.Blazor.Inputs;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.APUComponents;

public class APUSelectorVM : ComponentBase, IPopup, IDisposable
{
    #region Apu Hub

    [Inject]
    protected ApuHubClient ApuHub { get; set; }

    private void InitializeApuHub()
    {
        ApuHub.StartHub(NavM, "Apu - Apu Sel.");

        ApuHub.ApuCreated = ApuHub_ApuCreated;
        ApuHub.ApuCreatedMultipleLineItems = ApuHub_ApuHubCreatedMultipleLineItems;
        ApuHub.ApuUpdated = ApuHub_ApuUpdated;
        //_apuHub.ApuUpdatedMultiple = ApuHub_ApuUpdatedMultiple;
        ApuHub.ApuDeleted = ApuHub_ApuDeleted;
    }

    private async void ApuHub_ApuCreated(ApuHubApuCreatedMessage message)
    {
        if (!message.IsLineItem)
            return;

        var result = await ApuRepo.GetLineItemApuSelectorModelAsync(message.ApuId);
        if (!result.IsSuccess())
            return;

        var apu = result.Data;

        // Assign Project Name
        var pm = ProjectModels.FirstOrDefault(q => q.Id == apu.ProjectId);
        apu.ProjectName = pm?.ProjectName;

        apus.Add(apu);
        apus = apus.OrderByDescending(p => p.LastUpdatedAt).ThenBy(q => q.Description).ToList();
        GetFilteredApus();
    }

    private async void ApuHub_ApuHubCreatedMultipleLineItems(ApuHubApuCreatedMultipleLineItemsMessage message)
    {
        var result = await ApuRepo.GetLineItemApuSelectorModelsAsync(message.ApuIds);
        if (!result.IsSuccess())
            return;

        var apuModels = result.Data;

        // Assign Project Name
        foreach (var apu in apuModels)
        {
            var pm = ProjectModels.FirstOrDefault(q => q.Id == apu.ProjectId);
            apu.ProjectName = pm?.ProjectName;
        }

        apus.AddRange(apuModels);
        apus = apus.OrderByDescending(p => p.LastUpdatedAt).ThenBy(q => q.Description).ToList();
        GetFilteredApus();
    }

    private async void ApuHub_ApuUpdated(ApuHubApuUpdatedMessage message)
    {
        if (!message.IsLineItem)
            return;

        var result = await ApuRepo.GetLineItemApuSelectorModelAsync(message.ApuId);
        if (!result.IsSuccess())
            return;

        var apu = result.Data;

        // Assign Project Name
        var pm = ProjectModels.FirstOrDefault(q => q.Id == apu.ProjectId);
        apu.ProjectName = pm?.ProjectName;

        var index = apus.FindIndex(q => q.Id == message.ApuId);
        if (index == -1)
            return;

        apus[index] = apu;
        apus = apus.OrderByDescending(p => p.LastUpdatedAt).ThenBy(q => q.Description).ToList();
        GetFilteredApus();
    }

    private void ApuHub_ApuDeleted(ApuHubApuDeletedMessage message)
    {
        var apu = apus.FirstOrDefault(q => q.Id == message.ApuId);
        if (apu is not null)
        {
            apus.Remove(apu);
            GetFilteredApus();
        }
    }

    #endregion

    #region Services

    [Inject]
    protected NavigationManager NavM { get; set; }

    [Inject] 
    protected PopupService PopupService { get; set; }

    [Inject]
    protected ApuRepository ApuRepo { get; set; }

    #endregion

    #region ElementRefs

    internal SfTextBox TbSearch { get; set; }    

    #endregion

    #region Lifecycle

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            apus = new List<ApuSelectorModel>();
            FilteredApus = new List<ApuSelectorModel>();
            ProjectModels = new List<ProjectModel>();

            PopupService.Add(this);

            InitializeApuHub();
        }

        base.OnAfterRender(firstRender);
    }

    #endregion

    #region ProjectModels

    internal List<ProjectModel> ProjectModels { get; set; }

    #endregion
    #region Items

    private List<ApuSelectorModel> apus;
    internal List<ApuSelectorModel> FilteredApus { get; set; }

    private void GetFilteredApus()
    {
        FilteredApus = apus
            .If(_apuId is not null, q => q.Where(o => o.Id != _apuId))
            .If(!string.IsNullOrWhiteSpace(filterText), q =>
                q.Where(o =>
                    o.Description.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.Description, filterText) ||
                    o.ProjectName.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.ProjectName, filterText)
                    )
            )
            .ToList();
    }

    internal void ApuChanged(Guid? apuId)
    {
        _apuId = apuId;
        GetFilteredApus();
    }

    #endregion

    private Guid? _apuId;

    #region Popup Control

    internal string PopupCss { get; set; } 

    internal async void TogglePopup(Guid? apuId = null)
    {
        if (string.IsNullOrWhiteSpace(PopupCss))
        {
            PopupService.ClosePopups(this);

            if (apus.Count == 0)
            {
                var apuResult = await ApuRepo.GetLineItemApuSelectorModelsAsync();
                if (!apuResult.IsSuccess())
                    return;

                apus = apuResult.Data.OrderByDescending(p => p.LastUpdatedAt).ThenBy(q => q.Description).ToList();
                foreach (var apu in apus)
                {
                    var pm = ProjectModels.FirstOrDefault(q => q.Id == apu.ProjectId);
                    apu.ProjectName = pm?.ProjectName;
                }

                GetFilteredApus();
            }

            _apuId = apuId;

            PopupCss = "popup-apuselector-show";
            StateHasChanged();
        }
        else
        {
            ClosePopup();
        }
    }

    public void ClosePopup()
    {
        _apuId = null;
        PopupCss = "";
        StateHasChanged();

        filterText = "";
        TbSearch.Value = "";
        GetFilteredApus();
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
            GetFilteredApus();
        }
    }

    #endregion

    #region Selected Item

    internal void SelectApu(ApuSelectorModel apu)
    {
        ApuSelected?.Invoke(apu.Id);
        ClosePopup();
    }

    public Action<Guid> ApuSelected { get; set; }    

    #endregion

    #region IDisposable

    public void Dispose()
    {
        ApuHub?.Dispose();
        ApuHub = null;

        PopupService.Remove(this);
    }    

    #endregion
}