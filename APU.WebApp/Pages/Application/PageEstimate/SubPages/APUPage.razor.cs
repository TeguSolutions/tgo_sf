using System.Collections.ObjectModel;
using APU.DataV2.EntityModels;
using APU.WebApp.Pages.Application.PageEstimate.SubPages.APUComponents;
using APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class APUPageVM : PageVMBase, IDisposable
{
    #region Security

	internal bool CanEdit
	{
		get
		{
			if (Apu is null)
				return false;

			if (SelectedProject?.IsBlocked == true || Apu.Project?.IsBlocked == true || Apu.IsBlocked)
			{
				if ((Liu.IsAdministrator || Liu.IsSupervisor) == false)
					return false;
			}

			return true;
		}
	}

	#endregion

	#region BaseItem Hub

	[Inject]
	protected BaseItemHubClient BaseItemHub { get; set; }

	private void InitializeBaseItemHub()
	{
        if (BaseItemHub is null)
            return;

		BaseItemHub?.StartHub(NavM, "Apu");
	}

	#endregion
	#region Project Hub

	[Inject]
	protected ProjectHubClient ProjectHub { get; set; }

	private void InitializeProjectHub()
	{
        if (ProjectHub is null)
            return;

        ProjectHub.StartHub(NavM, "Apu");

        ProjectHub.ProjectUpdated = ProjectHub_ProjectUpdated;
	}

	private async void ProjectHub_ProjectUpdated(ProjectHubProjectUpdatedMessage message)
	{
        if (message is null)
            return;

        var result = await ProjectRepo.GetModelAsync(message.ProjectId);
        if (!result.IsSuccess())
	        return;

        var dbProjectModel = result.Data;
        allProjectModels = allProjectModels.ReplaceItem(dbProjectModel);
        GetFilteredProjects(statechange: false);

        if (SelectedProject is null)
			return;

		if (SelectedProject.Id != message.ProjectId)
			return;

		SelectedProject.IsBlocked = dbProjectModel.IsBlocked;
		SelectedProject.ProjectName = dbProjectModel.ProjectName;
        SelectedProject.Bond = dbProjectModel.Bond;
        SelectedProject.SalesTax = dbProjectModel.SalesTax;
        SelectedProject.Tools = dbProjectModel.Tools;

        if ((IsBlocked == true && SelectedProject.IsBlocked == false) ||
            (IsBlocked == false && SelectedProject.IsBlocked))
		{
			SelectedProject = null;
			Apu = null;
		}
        else
        {
            var calculationResult = SelectedProject.Calculate(defaultValue);
            if (!calculationResult.IsSuccess())
                ShowError(calculationResult.Message);
        }

        await InvokeAsync(SfCbProjectFilter.RefreshDataAsync);
		await InvokeAsync(SfCbApus.RefreshDataAsync);
		await InvokeAsync(StateHasChanged);
	}

	#endregion
	#region Apu Hub

	[Inject]
	protected ApuHubClient ApuHub { get; set; }

    private void InitializeApuHub()
    {
        if (ApuHub is null)
            return;

        ApuHub.StartHub(NavM, "Apu");

        ApuHub.ApuCreated = ApuHub_ApuCreated;
        ApuHub.ApuCreatedMultipleLineItems = ApuHub_ApuHubCreatedMultipleLineItems;
        ApuHub.ApuUpdated = ApuHub_ApuUpdated;
        ApuHub.ApuUpdatedMultiple = ApuHub_ApuUpdatedMultiple;
        ApuHub.ApuDeleted = ApuHub_ApuDeleted;
    }

    private async void ApuHub_ApuCreated(ApuHubApuCreatedMessage message)
    {
	    if (SelectedProject is null)
		    return;

	    if (SelectedProject.Id != message.ProjectId)
		    return;

	    if (!message.IsLineItem)
		    return;

	    var result = await ApuRepo.GetAsync(message.ApuId, includeApuItems: true);
	    if (!result.IsSuccess())
		    return;
	    var apu = result.Data;

	    // Step 1: Line item order management
	    var orderedGroupItems = SelectedProject.Apus.Where(q => q.GroupId == apu.GroupId && q.ItemId is >= 1 and <= 999).OrderBy(q => q.ItemId).ToList();
	    var reorderedGroupItems = orderedGroupItems.ApuInsert(apu.ItemId, apu);

	    // Step 2: Local Update.
	    SelectedProject.Apus.Add(apu);
	    SelectedProject.Apus.ApuReorderGroup(reorderedGroupItems);
	    SelectedProject.Apus = SelectedProject.Apus.OrderBy(q => q.GroupId).ThenBy(q => q.ItemId).ToList();

        // Step 3: Calculation
        apu.OrderItems();
        apu.CalculateAll(defaultValue, SelectedProject);

        await InvokeAsync(SfCbApus.RefreshDataAsync);
        await InvokeAsync(StateHasChanged);
    }

    private async void ApuHub_ApuHubCreatedMultipleLineItems(ApuHubApuCreatedMultipleLineItemsMessage message)
    {
	    if (SelectedProject is null)
		    return;

	    if (SelectedProject.Id != message.ProjectId)
		    return;

	    var result = await ApuRepo.GetMultipleAsync(message.ApuIds, includeApuItems: true);
	    if (!result.IsSuccess())
		    return;

	    // Step 1: Local Update.
	    foreach (var apu in result.Data)
	    {
		    // Step 2: Calculation
            apu.OrderItems();
		    apu.CalculateAll(defaultValue, SelectedProject);
		    SelectedProject.Apus.Add(apu);
	    }
	    // Step 3: Line item order management
	    SelectedProject.Apus = SelectedProject.Apus.OrderBy(q => q.GroupId).ThenBy(q => q.ItemId).ToList();

	    await InvokeAsync(SfCbApus.RefreshDataAsync);
	    await InvokeAsync(StateHasChanged);
    }

    private async void ApuHub_ApuUpdated(ApuHubApuUpdatedMessage message)
    {
        if (SelectedProject is null)
            return;

        if (SelectedProject.Id != message.ProjectId)
            return;

        if (!message.IsLineItem)
            return;

        var result = await ApuRepo.GetAsync(message.ApuId, includeApuItems: true);
        if (!result.IsSuccess())
            return;

        var newApu = result.Data;
        newApu.OrderItems();
        newApu.CalculateAll(defaultValue, SelectedProject);
        
        if (SelectedProject.Apus.FirstOrDefault(q => q.Id == newApu.Id) is null)
        {
            ApuHub_ApuCreated(new ApuHubApuCreatedMessage(message.ProjectId, message.ApuId, message.IsLineItem));
        }
        else
        {
	        SelectedProject.Apus = SelectedProject.Apus.ReplaceItem(newApu);

	        if (message.OrderChanged)
            {
	            // Step 1: Line item order management
	            var orderedGroupItems = SelectedProject.Apus.Where(q => q.GroupId == newApu.GroupId && q.ItemId is >= 1 and <= 999).OrderBy(q => q.ItemId).ToList();
	            var reorderedGroupItems = orderedGroupItems.ApuMove(newApu.ItemId, newApu);

	            // Step 2: Update Local
	            SelectedProject.Apus.ApuReorderGroup(reorderedGroupItems);
            }

            SelectedProject.Apus = SelectedProject.Apus.OrderBy(q => q.GroupId).ThenBy(q => q.ItemId).ToList();

            await InvokeAsync(SfCbApus.RefreshDataAsync);
            await InvokeAsync(StateHasChanged);
        }

        if (Apu?.Id == newApu.Id)
        {
            Apu = newApu;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void ApuHub_ApuUpdatedMultiple(ApuHubApuUpdatedMultipleMessage message)
    {
	    if (SelectedProject is null)
		    return;

	    if (SelectedProject.Id != message.ProjectId)
		    return;

        if (message.ApuIds.Count == 0)
            return;

	    var result = await ApuRepo.GetMultipleAsync(message.ApuIds, includeApuItems: true);
	    if (!result.IsSuccess())
	    {
            ShowError("SignalR: Couldn't collect Apus for local update!");
		    return;
	    }

	    foreach (var dbApu in result.Data)
	    {
            dbApu.OrderItems();
            dbApu.CalculateAll(defaultValue, SelectedProject);

            SelectedProject.Apus = SelectedProject.Apus.ReplaceItem(dbApu);

            if (Apu?.Id == dbApu.Id)
	            Apu = dbApu;
	    }

	    await InvokeAsync(SfCbApus.RefreshDataAsync);
	    await InvokeAsync(StateHasChanged);
    }

    private async void ApuHub_ApuDeleted(ApuHubApuDeletedMessage message)
    {
	    if (SelectedProject is null)
		    return;

	    if (SelectedProject.Id != message?.ProjectId)
		    return;

	    var apu = SelectedProject.Apus.FirstOrDefault(q => q.Id == message.ApuId);
	    if (apu is null) 
		    return;
	    
	    if (Apu.Id == message.ApuId)
		    TrySelectNextApu(false);

	    SelectedProject.Apus.Remove(apu);
	    if (message.ReorderedGroupItems is not null)
		    SelectedProject.Apus.ApuReorderGroup(message.ReorderedGroupItems);

	    SelectedProject.Apus = SelectedProject.Apus.OrderBy(q => q.GroupId).ThenBy(q => q.ItemId).ToList();

	    await InvokeAsync(SfCbApus.RefreshDataAsync);
	    await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Element References

	public SfComboBox<Guid?, Apu> SfCbApus { get; set; }

	#endregion

    #region Lifecycle

    private int? navGroupId;
    private int? navItemId;

    protected override void OnInitialized()
    {
        #region Query Param

        if (NavM.TryGetQueryString<string>("nav", out var queryNav))
        {
            try
            {
                var def = queryNav.ToLower().Split("-");
                navGroupId = int.Parse(def[0]);
                navItemId = int.Parse(def[1]);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        #endregion

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderEstimate));
            await EventAggregator.PublishAsync(new HeaderLinkMessage());

            if (!await GetLIU())
                return;

            #region Load UserSession

            IsBlocked = USS.GetProjectBlock(Liu.Id);
            var selectedProjectId = USS.GetSelectedProjectId(Liu.Id);

            #endregion

            // Load the definitions
            LaborWorkTypes = LaborWorkTypeDefinitions.Collection.ToList();
            ItemTypes = ItemTypeDefinitions.Collection.ToList();
            ApuStatuses = ApuStatusDefinitions.Collection.ToList();

            // Load the base items
            var pageDataResult = await GetPageData();
            if (!pageDataResult.IsSuccess())
                return;

            var allProjectResult = await GetAllProjectModelsAsync();
            if (!allProjectResult.IsSuccess())
                return;

            // Selector Callback Events
            ApuSelectorRef.ApuSelected = ApuCopy;

            PerformanceSelectorRef.PerformanceSelected = ApuPerformanceAdd;
            LaborSelectorRef.ItemSelected = ApuLaborAdd;
            MaterialSelectorRef.ItemSelected = ApuMaterialAdd;
            EquipmentSelectorRef.ItemSelected = ApuEquipmentAdd;
            ContractSelectorRef.ItemSelected = ApuContractAdd;

            // Dialogs
            PerformanceManager.Submit = PerformanceManagerSubmit;
            LaborManager.Submit = LaborManagerSubmit;
            MaterialManager.Submit = MaterialManagerSubmit;
            EquipmentManager.Submit = EquipmentManagerSubmit;
            ContractManager.Submit = ContractManagerSubmit;

            // Selected Project / Apu
            var selectedProjectModel = allProjectModels.FirstOrDefault(q => q.Id == selectedProjectId);
            if (selectedProjectModel is not null)
            {
	            if (IsBlocked == true && selectedProjectModel.IsBlocked == false)
	            { }
                else if (IsBlocked == false && selectedProjectModel.IsBlocked)
	            { }
                else
		            await LoadSelectedProjectAsync(selectedProjectId, navGroupId, navItemId);
            }

            navGroupId = null;
            navItemId = null;

            InitializeBaseItemHub();
            InitializeProjectHub();
            InitializeApuHub();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task<Result> GetPageData()
    {
        ProgressStart();

        #region Default Values

        var defaultResult = await DefaultRepo.GetAsync();
        if (!defaultResult.IsSuccess())
        {
            ShowError("Failed to collect the Default Values!");
            return Result.Fail();
        }

        defaultValue = defaultResult.Data;
        LaborSelectorRef.SetDefaultValue(defaultValue);

        #endregion

        ProgressStop();
        return Result.Ok();
    }

    #endregion

    #region Definitions

    internal List<LaborWorkType> LaborWorkTypes { get; set; }
    internal List<ItemType> ItemTypes { get; set; }
    internal List<ApuStatus> ApuStatuses { get; set; }

    #endregion
    #region DefaultValues

    private DefaultValue defaultValue;

    #endregion

    #region BaseItem - Performances

    private async Task BasePerformanceCreateAsync(BasePerformance performance)
    {
        if (performance is null)
            return;

        ProgressStart();

        var createResult = await BaseItemRepo.PerformanceAddAsync(performance, Liu);
        if (!createResult.IsSuccess())
        {
            ShowError("Failed to add new Base Performance");
            return;
        }

        BaseItemHub.SendPerformanceCreate(new BaseItemHubPerformanceCreatedMessage(createResult.Data.Id));

        ProgressStop();

        await ApuPerformanceRemoveAsync();
        ApuPerformanceAdd(createResult.Data);
    }
    private async Task BasePerformanceUpdateFromApuPerformanceAsync(ApuPerformance apuPerformance)
    {
        if (apuPerformance?.BasePerformanceId is null)
            return;

        ProgressStart();

        var updateResult = await BaseItemRepo.PerformanceUpdateFromIPerformanceAsync(apuPerformance.BasePerformanceId.Value, apuPerformance, Liu);
        if (!updateResult.IsSuccess())
        {
            ShowError("Failed to Update Base Performance");
            return;
        }

        BaseItemHub.SendPerformanceUpdate(new BaseItemHubPerformanceUpdatedMessage(apuPerformance.BasePerformanceId.Value));

        ProgressStop();
    }

    #endregion
    #region BaseItem - Labors

    private async Task BaseLaborCreateAsync(BaseLabor labor)
    {
        if (labor is null)
            return;

        ProgressStart();

        var createResult = await BaseItemRepo.LaborAddAsync(labor, Liu);
        if (!createResult.IsSuccess())
        {
            ShowError("Failed to add new Base Labor");
            return;
        }

        createResult.Data.CalculateCost(defaultValue.TopFica, defaultValue.Fica, defaultValue.FutaSuta, defaultValue.WorkersComp);
        
        BaseItemHub.SendLaborCreate(new BaseItemHubLaborCreatedMessage(createResult.Data.Id));

        ProgressStop();

        ApuLaborAdd(createResult.Data);
    }
    private async Task BaseLaborUpdateFromApuLaborAsync(ApuLabor apuLabor)
    {
        if (apuLabor?.BaseLaborId is null)
            return;

        ProgressStart();

        var updateResult = await BaseItemRepo.LaborUpdateFromILaborAsync(apuLabor.BaseLaborId.Value, apuLabor, Liu);
        if (!updateResult.IsSuccess())
        {
            ShowError("Failed to Update Base Labor!");
            return;
        }

        BaseItemHub.SendLaborUpdate(new BaseItemHubLaborUpdatedMessage(updateResult.Data.Id));
        
        ProgressStop();
    }

    #endregion
    #region BaseItem - Materials

    private async Task BaseMaterialCreateAsync(BaseMaterial material)
    {
        if (material is null)
            return;

        ProgressStart();

        var createResult = await BaseItemRepo.MaterialAddAsync(material, Liu);
        if (!createResult.IsSuccess())
        {
            ShowError("Failed to add new Base Material");
            return;
        }

        BaseItemHub.SendMaterialCreate(new BaseItemHubMaterialCreatedMessage(createResult.Data.Id));

        ProgressStop();
     
        ApuMaterialAdd(createResult.Data);
    }
    private async Task BaseMaterialUpdateFromApuMaterialAsync(ApuMaterial apuMaterial)
    {
        if (apuMaterial?.BaseMaterialId is null)
            return;

        ProgressStart();

        var updateResult = await BaseItemRepo.MaterialUpdateFromIMaterialAsync(apuMaterial.BaseMaterialId.Value, apuMaterial, Liu);
        if (!updateResult.IsSuccess())
        {
            ShowError("Failed to Update Base Material");
            return;
        }

        BaseItemHub.SendMaterialUpdate(new BaseItemHubMaterialUpdatedMessage(apuMaterial.BaseMaterialId.Value));

        ProgressStop();
    }

    #endregion
    #region BaseItem - Equipments

    private async Task BaseEquipmentCreateAsync(BaseEquipment equipment)
    {
        if (equipment is null)
            return;

        ProgressStart();

        var createResult = await BaseItemRepo.EquipmentAddAsync(equipment, Liu);
        if (!createResult.IsSuccess())
        {
            ShowError("Failed to add new Base Equipment");
            return;
        }

        BaseItemHub.SendEquipmentCreate(new BaseItemHubEquipmentCreatedMessage(createResult.Data.Id));

        ProgressStop();
       
        ApuEquipmentAdd(createResult.Data);
    }
    private async Task BaseEquipmentUpdateFromApuEquipmentAsync(ApuEquipment apuEquipment)
    {
        if (apuEquipment?.BaseEquipmentId is null)
            return;
        
        ProgressStart();

        var updateResult = await BaseItemRepo.EquipmentUpdateFromIEquipmentAsync(apuEquipment.BaseEquipmentId.Value, apuEquipment, Liu);
        if (!updateResult.IsSuccess())
        {
            ShowError("Failed to Update Base Equipment");
            return;
        }

        BaseItemHub.SendEquipmentUpdate(new BaseItemHubEquipmentUpdatedMessage(apuEquipment.BaseEquipmentId.Value));

        ProgressStop();
    }

    #endregion
    #region BaseItem - Contracts

    private async Task BaseContractCreateAsync(BaseContract contract)
    {
        if (contract is null)
            return;

        ProgressStart();

        var createResult = await BaseItemRepo.ContractAddAsync(contract, Liu);
        if (!createResult.IsSuccess())
        {
            ShowError("Failed to add new Base Contract");
            return;
        }

        BaseItemHub.SendContractCreate(new BaseItemHubContractCreatedMessage(createResult.Data.Id));

        ProgressStop();
 
        ApuContractAdd(createResult.Data);
    }
    private async Task BaseContractUpdateFromApuContractAsync(ApuContract apuContract)
    {
        if (apuContract?.BaseContractId is null)
            return;

        ProgressStart();

        var updateResult = await BaseItemRepo.ContractUpdateFromIContractAsync(apuContract.BaseContractId.Value, apuContract, Liu);
        if (!updateResult.IsSuccess())
        {
            ShowError("Failed to Update Base Contract");
            return;
        }

        BaseItemHub.SendContractUpdate(new BaseItemHubContractUpdatedMessage(apuContract.BaseContractId.Value));

        ProgressStop();
    }

    #endregion

    #region ProjectModels - All

    private List<ProjectModel> allProjectModels = new();

    private async Task<Result> GetAllProjectModelsAsync()
    {
        ProgressStart();

        var result = await ProjectRepo.GetAllModelAsync();
        if (!result.IsSuccess())
        {
            ShowError("Failed to collect Projects!");
            return Result.Fail();
        }

        allProjectModels = result.Data.OrderBy(p => p.ProjectName).ToList();
        GetFilteredProjects();

        ApuSelectorRef.ProjectModels = allProjectModels;

        ProgressStop();
        return Result.Ok();
    }    

    #endregion
    #region ProjectModels - Filtered

    internal ObservableCollection<ProjectModel> FilteredProjectModels { get; set; } = new();
    private void GetFilteredProjects(string projectFilter = null, bool statechange = true)
    {
        FilteredProjectModels = allProjectModels
            .If(IsBlocked == true, p => p.Where(o => o.IsBlocked))
            .If(IsBlocked == false, p => p.Where(o => o.IsBlocked == false))
            .If(!string.IsNullOrWhiteSpace(projectFilter), q => 
                q.Where(o =>
                    o.ProjectName.ToLower().Contains(projectFilter?.ToLower() ?? string.Empty) ||
                    TeguStringComparer.CompareToFilterBool(o.ProjectName, projectFilter))
            )
            .ToObservableCollection();

        if (statechange)
			InvokeAsync(StateHasChanged);
    }    

    #endregion

    #region Project - Selected

    internal Project SelectedProject { get; set; }
    internal void SelectedProjectCBChanged(ChangeEventArgs<Guid?, ProjectModel> args)
    {
        _ = LoadSelectedProjectAsync(args?.ItemData?.Id);
    }

    private async Task LoadSelectedProjectAsync(Guid? projectId, int? apuGroupId = null, int? apuItemId = null)
    {
        if (SelectedProject?.Id == projectId)
            return;

        if (projectId == null)
        {
            SelectedProject = null;
            USS.SetSelectedProject(Liu.Id, null);
            SelectedApuChanged(null);
        }
        else
        {
	        ProgressStart();

            var result = await ProjectRepo.GetLineItemsAsync(projectId.Value);
            if (!result.IsSuccess())
            {
                SelectedProject = null;
                SelectedApuChanged(null);
                ShowError("Failed to load Project!");
                return;
            }

            SelectedProject = result.Data;
            SelectedProject.Apus = SelectedProject.Apus.OrderBy(q => q.GroupId).ThenBy(q => q.ItemId).ToList();
            USS.SetSelectedProject(Liu.Id, SelectedProject.Id);

            if (apuGroupId is not null && apuItemId is not null)
            {
                var apu = SelectedProject.Apus.FirstOrDefault(q => q.GroupId == apuGroupId && q.ItemId == apuItemId) ??
                          SelectedProject.Apus.FirstOrDefault();
                SelectedApuChanged(apu);
            }
            else
            {
                SelectedApuChanged(SelectedProject.Apus.FirstOrDefault());
            }

            ProgressStop();
        }
    }

    #endregion
    #region Projects - Filter

    internal bool? IsBlocked { get; set; }
    internal void IsBlockedChanged()
    {
        if (IsBlocked == null)
            IsBlocked = true;
        else if (IsBlocked == true)
            IsBlocked = false;
        else if (IsBlocked == false)
            IsBlocked = null;

        SelectedProjectCBChanged(null);
        USS.SetProjectBlock(Liu.Id, IsBlocked);

        GetFilteredProjects();
    }

    // Project Filter
    internal SfComboBox<Guid?, ProjectModel> SfCbProjectFilter { get; set; }
    internal async void ProjectFilterChanged(SfDropDownFilteringEventArgs args)
    {
        args.PreventDefaultAction = true;
        GetFilteredProjects(args.Text);

        var query = new Query();
        await SfCbProjectFilter.FilterAsync(FilteredProjectModels, query);
    }

    #endregion

    #region Apus

    private async void ApuCopy(Guid sourceApuId)
    {
        if (Apu is null)
            return;

        if (Apu.Id == sourceApuId)
        {
            ShowError("Apu Copy Error - Apu can't be copied to itself!");
            return;
        }

        ProgressStart();

        // Step 1: clean up the current Apu
        var resetResult = await ApuRepo.RemoveItemsAsync(Apu.Id);
        if (!resetResult.IsSuccess())
        {
            ShowError("Apu Copy Error - Failed to reset Apu Items!");
            return;
        }

        // Step 2: Get the source Apu
        var sourceApuResult = await ApuRepo.GetAsync(sourceApuId, includeApuItems: true, includeProject: true);
        if (!sourceApuResult.IsSuccess())
        {
            ShowError("Apu Copy Error - Failed to collect the source Apu!");
            return;
        }

        var sourceApu = sourceApuResult.Data;

        // Step 3: Update the current Apu
        var cccResult = await ApuRepo.AddItemsAsync(sourceApu, Apu.Id);
        if (!cccResult.IsSuccess())
        {
            ShowError("Apu Copy Error - Failed to copy the source Apu Items!");
            return;
        }

        // Step 4: Final assignment
        SelectedProject.Apus = SelectedProject.Apus.ReplaceItem(cccResult.Data);

        Apu = cccResult.Data;
        Apu.Status = ApuStatusDefinitions.Progress;
      
        ApuCalculate();

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    #endregion
    #region Selected Apu

    /// <summary>
    /// Selected Apu
    /// </summary>
    internal Apu Apu { get; set; }

    internal void SelectedApuCBChanged(ChangeEventArgs<Guid?, Apu> args)
    {
        SelectedApuChanged(args.ItemData);
    }

    internal void SelectedApuChanged(Apu selectedApu)
    {
        if (selectedApu?.Id == Apu?.Id)
            return;

        if (selectedApu is null)
            Apu = null;
        else
        {
            Apu = selectedApu;
            Apu.SetPreviousValues();
            Apu.OrderItems();
        }

        ApuCalculate();
        ApuSelectorRef?.ApuChanged(Apu?.Id);
    }

    internal async void TrySelectPreviousApu()
    {
        if (SelectedProject is null || SelectedProject.Apus.Count == 0)
        {
	        Apu = null;
            await InvokeAsync(StateHasChanged);
	        return;
        }
    
        if (SelectedProject.Apus.Count == 1)
	        return;

        var index = SelectedProject.Apus.IndexOf(Apu);
        if (index == -1)
            Apu = SelectedProject.Apus.Last();
        else
        {
            if (index == 0)
                Apu = SelectedProject.Apus.Last();
            else
                Apu = SelectedProject.Apus.ElementAt(index - 1);
        }

        Apu?.OrderItems();
        ApuCalculate();
        ApuSelectorRef?.ApuChanged(Apu?.Id);
    }
    internal async void TrySelectNextApu(bool stateChange)
    {
        if (SelectedProject is null || SelectedProject.Apus.Count == 0)
        {
	        Apu = null;
	        await InvokeAsync(StateHasChanged);
	        return;
        }

        if (SelectedProject.Apus.Count == 1)
            return;

        var index = SelectedProject.Apus.IndexOf(Apu);
        if (index == -1)
            Apu = SelectedProject.Apus.First();
        else
        {
            if (index < SelectedProject.Apus.Count - 1)
                Apu = SelectedProject.Apus.ElementAt(index + 1);
            else
                Apu = SelectedProject.Apus.ElementAt(0);
        }

        Apu?.OrderItems();
        ApuCalculate(stateChange);
        ApuSelectorRef?.ApuChanged(Apu?.Id);
    }

    private async void ApuCalculate(bool stateChange = true)
    {
        Apu?.CalculateAll(defaultValue, SelectedProject);
        if (stateChange)
            await InvokeAsync(StateHasChanged);
    }

    #endregion
    #region Apu - Update

    internal async void SfDropDownApuStatus_OnValueSelect(SelectEventArgs<ApuStatus> args)
    {
        var result = await ApuUpdateStatusAsync(Apu, args.ItemData);
        if (!result.IsSuccess())
            args.Cancel = true;
    }
    private async Task<Result> ApuUpdateStatusAsync(Apu apu, ApuStatus status)
    {
        ProgressStart();

        var result = await ApuRepo.UpdateStatusAsync(apu.Id, status.Id, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu Status!");
            ProgressStop();
            return result;
        }

        apu.StatusId = status.Id;
        apu.Status = status;
        apu.SetLastUpdated(Liu);

        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));

        return result;
    }

    // Pcts - Local change
    internal void ApuLaborSuperPctChanged(decimal laborSuperPct)
    {
        Apu.SuperPercentage = laborSuperPct;
        ApuCalculate(false);
    }
    internal void ApuLaborGrossPctChanged(decimal laborGrossPct)
    {
        Apu.LaborGrossPercentage = laborGrossPct;
        ApuCalculate(false);
    }
    internal void ApuMaterialGrossPctChanged(decimal materialGrossPct)
    {
        Apu.MaterialGrossPercentage = materialGrossPct;
        ApuCalculate(false);
    }
    internal void ApuEquipmentGrossPctChanged(decimal equipmentGrossPct)
    {
        Apu.EquipmentGrossPercentage = equipmentGrossPct;
        ApuCalculate(false);
    }
    internal void ApuContractGrossPctChanged(decimal contractGrossPct)
    {
        Apu.SubcontractorGrossPercentage = contractGrossPct;
        ApuCalculate(false);
    }

    // Pcts - Update
    internal async void ApuUpdateLaborSuperPct()
    {
        if (Apu is null)
            return;

        ProgressStart();

        var result = await ApuRepo.UpdateLaborSuperPercentageAsync(Apu.Id, Apu.SuperPercentage, Liu);
        if (!result.IsSuccess())
        {
            Apu.RestorePreviousValues();
            ApuCalculate(false);
            ShowError("Apu update failed!");
            return;
        }

        Apu.SetPreviousValues();
        Apu.SetLastUpdated(Liu);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    internal async void ApuUpdateLaborGrossPct()
    {
        if (Apu is null)
            return;

        ProgressStart();

        var result = await ApuRepo.UpdateLaborGrossPercentageAsync(Apu.Id, Apu.LaborGrossPercentage, Liu);
        if (!result.IsSuccess())
        {
            Apu.RestorePreviousValues();
            ApuCalculate(false);
            ShowError("Apu update failed!");
            return;
        }

        Apu.SetPreviousValues();
        Apu.SetLastUpdated(Liu);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    internal async void ApuUpdateMaterialGrossPct()
    {
        if (Apu is null)
            return;

        ProgressStart();

        var result = await ApuRepo.UpdateMaterialGrossPercentageAsync(Apu.Id, Apu.MaterialGrossPercentage, Liu);
        if (!result.IsSuccess())
        {
            Apu.RestorePreviousValues();
            ApuCalculate(false);
            ShowError("Apu update failed!");
            return;
        }

        Apu.SetPreviousValues();
        Apu.SetLastUpdated(Liu);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    internal async void ApuUpdateEquipmentGrossPct()
    {
        if (Apu is null)
            return;

        ProgressStart();

        var result = await ApuRepo.UpdateEquipmentGrossPercentageAsync(Apu.Id, Apu.EquipmentGrossPercentage, Liu);
        if (!result.IsSuccess())
        {
            Apu.RestorePreviousValues();
            ApuCalculate(false);
            ShowError("Apu update failed!");
            return;
        }

        Apu.SetPreviousValues();
        Apu.SetLastUpdated(Liu);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    internal async void ApuUpdateContractGrossPct()
    {
        if (Apu is null)
            return;

        ProgressStart();

        var result = await ApuRepo.UpdateContractGrossPercentageAsync(Apu.Id, Apu.SubcontractorGrossPercentage, Liu);
        if (!result.IsSuccess())
        {
            Apu.RestorePreviousValues();
            ApuCalculate(false);
            ShowError("Apu update failed!");
            return;
        }

        Apu.SetPreviousValues();
        Apu.SetLastUpdated(Liu);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    internal async void ApuUpdateLaborNotes()
    {
        if (Apu is null)
            return;

        ProgressStart();

        var result = await ApuRepo.UpdateLaborNotesAsync(Apu.Id, Apu.LaborNotes, Liu);
        if (!result.IsSuccess())
        {
            Apu.RestorePreviousValues();
            ApuCalculate(false);
            ShowError("Apu update failed!");
            return;
        }

        Apu.SetPreviousValues();
        Apu.SetLastUpdated(Liu);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    internal async void ApuUpdateMaterialNotes()
    {
        if (Apu is null)
            return;

        ProgressStart();

        var result = await ApuRepo.UpdateMaterialNotesAsync(Apu.Id, Apu.MaterialNotes, Liu);
        if (!result.IsSuccess())
        {
            Apu.RestorePreviousValues();
            ApuCalculate(false);
            ShowError("Apu update failed!");
            return;
        }

        Apu.SetPreviousValues();
        Apu.SetLastUpdated(Liu);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    internal async void ApuUpdateEquipmentNotes()
    {
        if (Apu is null)
            return;

        ProgressStart();

        var result = await ApuRepo.UpdateEquipmentNotesAsync(Apu.Id, Apu.EquipmentNotes, Liu);
        if (!result.IsSuccess())
        {
            Apu.RestorePreviousValues();
            ApuCalculate(false);
            ShowError("Apu update failed!");
            return;
        }

        Apu.SetPreviousValues();
        Apu.SetLastUpdated(Liu);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    internal async void ApuUpdateContractNotes()
    {
        if (Apu is null)
            return;

        ProgressStart();

        var result = await ApuRepo.UpdateContractNotesAsync(Apu.Id, Apu.ContractNotes, Liu);
        if (!result.IsSuccess())
        {
            Apu.RestorePreviousValues();
            ApuCalculate(false);
            ShowError("Apu update failed!");
            return;
        }

        Apu.SetPreviousValues();
        Apu.SetLastUpdated(Liu);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    #endregion

    #region ApuPerformance

    internal async void ApuPerformanceAdd(BasePerformance basePerformance)
    {
        // Todo: Manage to only have 1 Performance max, replace it!!!
        if (Apu.Performance is not null)
        {
            ShowWarning("Remove the existing Performance first!");
            return;
        }

        ProgressStart();

        var result = await ApuRepo.ApuPerformanceAddFromBasePerformanceAsync(basePerformance, Apu, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add Apu Performance");
            return;
        }

        Apu.ApuPerformances.Add(result.Data);
        ApuCalculate();

        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    private async Task ApuPerformanceUpdateAsync(ApuPerformance performance)
    {
        if (performance is null)
            return;

        ProgressStart();

        var result = await ApuRepo.ApuPerformanceUpdateAsync(performance, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu Performance");
            return;
        }

        Apu.ApuPerformances = Apu.ApuPerformances.ReplaceItem(result.Data);

        ApuCalculate();

        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    internal async Task ApuPerformanceRemoveAsync()
    {
        if (Apu.Performance is null)
            return;

        ProgressStart();

        var result = await ApuRepo.ApuPerformanceRemoveAsync(Apu.Performance);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Apu Performance!");
            return;
        }

        Apu.ApuPerformances.Remove(Apu.Performance);
        ApuCalculate();
        
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    #endregion
    #region ApuLabors

    internal void ApuLaborQuantityChanged(ApuLabor labor, decimal quantity)
    {
        labor.Quantity = quantity;
        ApuCalculate(false);
    }
    internal async void ApuLaborUpdateQuantity(ApuLabor labor)
    {
        ProgressStart();

        var result = await ApuRepo.ApuLaborUpdateQuantityAsync(labor.Id, labor.Quantity, Liu);
        if (!result.IsSuccess())
        {
            // Todo: restore..
            ApuCalculate(false);
            ShowError("Apu Labor quantity update failed!");
            return;
        }

        // Todo: set values
        labor.SetLastUpdated(Liu);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    internal async void SfDropDownApuLaborWorkType_OnValueSelect(SelectEventArgs<LaborWorkType> args, ApuLabor labor)
    {
        var result = await ApuLaborUpdateWorkTypeAsync(labor, args.ItemData);
        if (!result.IsSuccess())
            args.Cancel = true;
    }
    private async Task<Result> ApuLaborUpdateWorkTypeAsync(ApuLabor labor, LaborWorkType workType)
    {
        ProgressStart();

        var result = await ApuRepo.ApuLaborUpdateWorkTypeAsync(labor.Id, workType.Id, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Labor Work Type!");
            ProgressStop();
            return result;
        }

        labor.WorkTypeId = workType.Id;
        labor.WorkType = workType;

        labor.LastUpdatedAt = DateTime.Now;
        labor.LastUpdatedById = Liu.Id;
        labor.LastUpdatedBy = Liu;

        ApuCalculate();
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
        return result;
    }

    private async void ApuLaborAdd(BaseLabor baseLab)
    {
        ProgressStart();

        var result = await ApuRepo.ApuLaborAddFromBaseLaborAsync(baseLab, Apu, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add Apu Labor!");
            return;
        }

        Apu.ApuLabors.Add(result.Data);

        Apu.OrderItems();
        ApuCalculate(false);

        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    internal async void ApuLaborRemove(ApuLabor apuLabor)
    {
        ProgressStart();

        var result = await ApuRepo.ApuLaborRemoveAsync(apuLabor);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Apu Labor!");
            return;
        }

        Apu.ApuLabors.Remove(apuLabor);
        ApuCalculate();
        
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    private async Task<bool> ApuLaborUpdateAsync(ApuLabor labor)
    {
        ProgressStart();

        var result = await ApuRepo.ApuLaborUpdateAsync(labor, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu Labor");
            return false;
        }

        Apu.ApuLabors = Apu.ApuLabors.ReplaceItem(result.Data);

        ApuCalculate();

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
        return true;
    }

    #endregion
    #region ApuMaterials

    internal void ApuMaterialQuantityChanged(decimal qty, ApuMaterial material)
    {
        material.Quantity = qty;
        ApuCalculate(false);
    }
    internal async void ApuMaterialUpdateQuantity(ApuMaterial material)
    {
        ProgressStart();

        var result = await ApuRepo.ApuMaterialUpdateQuantityAsync(material.Id, material.Quantity, Liu);
        if (!result.IsSuccess())
        {
            // Todo: restore..
            ApuCalculate(false);
            ShowError("Apu Material quantity update failed!");
            return;
        }

        // Todo: set values
        material.SetLastUpdated(Liu);

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    internal void ApuMaterialWasteChanged(decimal waste, ApuMaterial material)
    {
        material.Waste = waste;
        ApuCalculate(false);
    }
    internal async void ApuMaterialUpdateWaste(ApuMaterial material)
    {
        ProgressStart();

        var result = await ApuRepo.ApuMaterialUpdateWasteAsync(material.Id, material.Waste, Liu);
        if (!result.IsSuccess())
        {
            // Todo: restore..
            ApuCalculate(false);
            ShowError("Apu Material waste update failed!");
            return;
        }

        // Todo: set values
        material.SetLastUpdated(Liu);

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    internal async void SfDropDownApuMaterialItemType_OnValueSelect(SelectEventArgs<ItemType> args, ApuMaterial material)
    {
        var result = await ApuMaterialUpdateItemTypeAsync(material, args.ItemData);
        if (!result.IsSuccess())
            args.Cancel = true;
    }
    private async Task<Result> ApuMaterialUpdateItemTypeAsync(ApuMaterial material, ItemType itemType)
    {
        ProgressStart();

        var result = await ApuRepo.ApuMaterialUpdateItemTypeAsync(material.Id, itemType.Id, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Material Item Type!");
            ProgressStop();
            return result;
        }

        material.ItemTypeId = itemType.Id;
        material.ItemType = itemType;

        material.SetLastUpdated(Liu);

        ApuCalculate(false);
        ProgressStop();

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
        return result;
    }

    private async void ApuMaterialAdd(BaseMaterial baseMaterial)
    {
        ProgressStart();

        var result = await ApuRepo.ApuMaterialAddFromBaseMaterialAsync(baseMaterial, Apu, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add Apu Material!");
            return;
        }

        Apu.ApuMaterials.Add(result.Data);

        Apu.OrderItems();
        ApuCalculate(false);

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    private async Task<bool> ApuMaterialUpdateAsync(ApuMaterial material)
    {
        ProgressStart();

        var result = await ApuRepo.ApuMaterialUpdateAsync(material, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu Material!");
            return false;
        }

        Apu.ApuMaterials = Apu.ApuMaterials.ReplaceItem(result.Data);

        ApuCalculate(false);

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));

        return true;
    }
    internal async void ApuMaterialRemove(ApuMaterial apuMaterial)
    {
        ProgressStart();

        var result = await ApuRepo.ApuMaterialRemoveAsync(apuMaterial);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Apu Material!");
            return;
        }

        Apu.ApuMaterials.Remove(apuMaterial);
        ApuCalculate(false);
        
        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }    

    #endregion
    #region ApuEquipments

    internal void ApuEquipmentQuantityChanged(decimal quantity, ApuEquipment equipment)
    {
        equipment.Quantity = quantity;
        ApuCalculate(false);
    }
    internal async void ApuEquipmentUpdateQuantity(ApuEquipment equipment)
    {
        ProgressStart();

        var result = await ApuRepo.ApuEquipmentUpdateQuantityAsync(equipment.Id, equipment.Quantity, Liu);
        if (!result.IsSuccess())
        {
            // Todo: restore..
            ApuCalculate(false);
            ShowError("Apu Equipment quantity update failed!");
            return;
        }

        // Todo: set values
        equipment.SetLastUpdated(Liu);
        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    internal async void SfDropDownApuEquipmentItemType_OnValueSelect(SelectEventArgs<ItemType> args, ApuEquipment equipment)
    {
        var result = await ApuEquipmentUpdateItemTypeAsync(equipment, args.ItemData);
        if (!result.IsSuccess())
            args.Cancel = true;
    }
    private async Task<Result> ApuEquipmentUpdateItemTypeAsync(ApuEquipment equipment, ItemType itemType)
    {
        ProgressStart();

        var result = await ApuRepo.ApuEquipmentUpdateItemTypeAsync(equipment.Id, itemType.Id, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Equipment Item Type!");
            ProgressStop();
            return result;
        }

        equipment.ItemTypeId = itemType.Id;
        equipment.ItemType = itemType;

        equipment.LastUpdatedAt = DateTime.Now;
        equipment.LastUpdatedById = Liu.Id;
        equipment.LastUpdatedBy = Liu;

        ApuCalculate();
        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));

        return result;
    }

    private async void ApuEquipmentAdd(BaseEquipment baseEquipment)
    {
        ProgressStart();

        var result = await ApuRepo.ApuEquipmentAddFromBaseEquipmentAsync(baseEquipment, Apu, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add Apu Equipment!");
            return;
        }

        Apu.ApuEquipments.Add(result.Data);

        Apu.OrderItems();
        ApuCalculate(false);

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    private async Task<bool> ApuEquipmentUpdateAsync(ApuEquipment equipment)
    {
        ProgressStart();

        var result = await ApuRepo.ApuEquipmentUpdateAsync(equipment, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu Equipment!");
            return false;
        }

        Apu.ApuEquipments = Apu.ApuEquipments.ReplaceItem(result.Data);
        ApuCalculate();

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
        return true;
    }
    internal async void ApuEquipmentRemove(ApuEquipment apuEquipment)
    {
        ProgressStart();

        var result = await ApuRepo.ApuEquipmentRemoveAsync(apuEquipment);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Apu Equipment!");
            return;
        }

        Apu.ApuEquipments.Remove(apuEquipment);
        ApuCalculate();
        
        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    #endregion
    #region ApuContracts

    internal void ApuContractQuantityChanged(decimal qty, ApuContract contract)
    {
        contract.Quantity = qty;
        ApuCalculate(false);
    }
    internal async void ApuContractUpdateQuantity(ApuContract contract)
    {
        ProgressStart();

        var result = await ApuRepo.ApuContractUpdateQuantityAsync(contract.Id, contract.Quantity, Liu);
        if (!result.IsSuccess())
        {
            // Todo: restore..
            ApuCalculate(false);
            ShowError("Apu Contract quantity update failed!");
            return;
        }

        // Todo: set values
        contract.SetLastUpdated(Liu);
        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    internal async void SfDropDownApuContractItemType_OnValueSelect(SelectEventArgs<ItemType> args, ApuContract contract)
    {
        var result = await ApuContractUpdateItemTypeAsync(contract, args.ItemData);
        if (!result.IsSuccess())
            args.Cancel = true;
    }
    private async Task<Result> ApuContractUpdateItemTypeAsync(ApuContract contract, ItemType itemType)
    {
        ProgressStart();

        var result = await ApuRepo.ApuContractUpdateItemTypeAsync(contract.Id, itemType.Id, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Contract Item Type!");
            ProgressStop();
            return result;
        }

        contract.ItemTypeId = itemType.Id;
        contract.ItemType = itemType;

        contract.LastUpdatedAt = DateTime.Now;
        contract.LastUpdatedById = Liu.Id;
        contract.LastUpdatedBy = Liu;

        ApuCalculate();
        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));

        return result;
    }

    private async void ApuContractAdd(BaseContract baseContract)
    {
        ProgressStart();

        var result = await ApuRepo.ApuContractAddFromBaseContractAsync(baseContract, Apu, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add Apu Contract!");
            return;
        }

        Apu.ApuContracts.Add(result.Data);

        Apu.OrderItems();
        ApuCalculate(false);

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }
    private async Task<bool> ApuContractUpdateAsync(ApuContract contract)
    {
        ProgressStart();

        var result = await ApuRepo.ApuContractUpdateAsync(contract, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu Contract!");
            return false;
        }

        Apu.ApuContracts = Apu.ApuContracts.ReplaceItem(result.Data);
        ApuCalculate();

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));

        return true;
    }
    internal async void ApuContractRemove(ApuContract apuContract)
    {
        ProgressStart();

        var result = await ApuRepo.ApuContractRemoveAsync(apuContract);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Apu Contract!");
            return;
        }

        Apu.ApuContracts.Remove(apuContract);
        ApuCalculate();
        
        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(Apu.ProjectId, Apu.Id, Apu.IsLineItem, false));
    }

    #endregion

    #region Dlg - Performance Manager

    internal DlgPerformanceManager PerformanceManager { get; set; }

    internal void OpenPerformanceManagerForCreate()
    {
        if (!CanEdit)
            return;

	    PerformanceManager.Open(ManagerOpenMode.Create, null);
    }
    internal void OpenPerformanceManagerForUpdateMix()
    {
        if (!CanEdit)
            return;

        if (Apu.Performance is null)
            return;

        PerformanceManager.Open(ManagerOpenMode.UpdateMix, Apu.Performance);
    }

    internal async void PerformanceManagerSubmit((ManagerSubmitMode mode, IPerformance performance) p)
    {
        if (!CanEdit)
            return;

        var (mode, performance) = p;
        if (performance is null)
            return;

        if (mode is ManagerSubmitMode.Create)
            await BasePerformanceCreateAsync(performance as BasePerformance);
        if (mode is ManagerSubmitMode.Update)
            await ApuPerformanceUpdateAsync(performance as ApuPerformance);
        if (mode is ManagerSubmitMode.UpdateProjectAndBase)
        {
            await ApuPerformanceUpdateAsync(performance as ApuPerformance);
            await BasePerformanceUpdateFromApuPerformanceAsync(performance as ApuPerformance);
        }
    }

    #endregion
    #region Dlg - Labor Manager

    internal DlgLaborManager LaborManager { get; set; }

    internal void LaborManagerOpenCreate()
    {
        if (!CanEdit) 
            return;

        LaborManager.Open(ManagerOpenMode.Create, null);
    }
    internal void LaborManagerOpenUpdateMix(ApuLabor labor)
    {
        if (!CanEdit) 
            return;

        LaborManager.Open(ManagerOpenMode.UpdateMix, labor);
    }

    private async void LaborManagerSubmit((ManagerSubmitMode mode, ILabor labor) p)
    {
        if (!CanEdit)
            return;

        var (mode, labor) = p;
        if (labor is null)
            return;

        if (mode is ManagerSubmitMode.Create)
            await BaseLaborCreateAsync(labor as BaseLabor);
        if (mode is ManagerSubmitMode.Copy)
            ApuLaborAdd(labor as BaseLabor);
        if (mode is ManagerSubmitMode.Update)
            await ApuLaborUpdateAsync(labor as ApuLabor);
        if (mode is ManagerSubmitMode.UpdateProjectAndBase)
        {
            await ApuLaborUpdateAsync(labor as ApuLabor);
            await BaseLaborUpdateFromApuLaborAsync(labor as ApuLabor);
        }
    }

    #endregion
    #region Dlg - Material Manager

    internal DlgMaterialManager MaterialManager { get; set; }

    private async void MaterialManagerSubmit((ManagerSubmitMode mode, IMaterial material) p)
    {
        if (!CanEdit)
            return;

        var (mode, material) = p;
        if (material is null)
            return;

        if (mode is ManagerSubmitMode.Create)
            await BaseMaterialCreateAsync(material as BaseMaterial);
        if (mode is ManagerSubmitMode.Copy)
            ApuMaterialAdd(material as BaseMaterial);
        if (mode is ManagerSubmitMode.Update)
            await ApuMaterialUpdateAsync(material as ApuMaterial);
        if (mode is ManagerSubmitMode.UpdateProjectAndBase)
        {
            await ApuMaterialUpdateAsync(material as ApuMaterial);
            await BaseMaterialUpdateFromApuMaterialAsync(material as ApuMaterial);
        }
    }

    #endregion
    #region Dlg - Equipment Manager

    internal DlgEquipmentManager EquipmentManager { get; set; }

    //internal void EquipmentManagerOpenCreate()
    //{
    //    if (!CanEdit) 
    //        return;

    //    EquipmentManager.Open(ManagerOpenMode.Create, null);
    //}
    //internal void EquipmentManagerOpenUpdateMix(ApuEquipment equipment)
    //{
    //    if (!CanEdit) 
    //        return;

    //    EquipmentManager.Open(ManagerOpenMode.UpdateMix, equipment);
    //}

    private async void EquipmentManagerSubmit((ManagerSubmitMode mode, IEquipment equipment) p)
    {
        if (!CanEdit) 
            return;

        var (mode, equipment) = p;
        if (equipment is null)
            return;

        if (mode is ManagerSubmitMode.Create)
            await BaseEquipmentCreateAsync(equipment as BaseEquipment);
        if (mode is ManagerSubmitMode.Copy)
            ApuEquipmentAdd(equipment as BaseEquipment);
        if (mode is ManagerSubmitMode.Update)
            await ApuEquipmentUpdateAsync(equipment as ApuEquipment);
        if (mode is ManagerSubmitMode.UpdateProjectAndBase)
        {
            await ApuEquipmentUpdateAsync(equipment as ApuEquipment);
            await BaseEquipmentUpdateFromApuEquipmentAsync(equipment as ApuEquipment);
        }
    }

    #endregion
    #region Dlg - Contract Manager

    internal DlgContractManager ContractManager { get; set; }

    //internal void ContractManagerOpenCreate()
    //{
    //    if (!CanEdit) 
    //        return;

    //    ContractManager.Open(ManagerOpenMode.Create, null);
    //}
    //internal void ContractManagerOpenUpdateMix(ApuContract contract)
    //{
    //    if (!CanEdit) 
    //        return;

    //    ContractManager.Open(ManagerOpenMode.UpdateMix, contract);
    //}

    private async void ContractManagerSubmit((ManagerSubmitMode mode, IContract contract) p)
    {
        if (!CanEdit) 
            return;

        var (mode, contract) = p;
        if (contract is null)
            return;

        if (mode is ManagerSubmitMode.Create)
            await BaseContractCreateAsync(contract as BaseContract);
        if (mode is ManagerSubmitMode.Copy)
            ApuContractAdd(contract as BaseContract);
        if (mode is ManagerSubmitMode.Update)
            await ApuContractUpdateAsync(contract as ApuContract);
        if (mode is ManagerSubmitMode.UpdateProjectAndBase)
        {
            await ApuContractUpdateAsync(contract as ApuContract);
            await BaseContractUpdateFromApuContractAsync(contract as ApuContract);
        }
    }

    #endregion

    #region Selector - Apu

    internal APUSelector ApuSelectorRef { get; set; }

    internal void ApuSelectorToggle()
    {
        if (!CanEdit)
            return;

        ApuSelectorRef.TogglePopup(Apu?.Id);
    }

    #endregion

    #region Selector - Performance

    internal APUPerformanceSelector PerformanceSelectorRef { get; set; }

    internal void PerformanceSelectorToggle()
    {
        if (!CanEdit)
            return;

        PerformanceSelectorRef.TogglePopup();
    }
    internal void PerformanceSelectorClose()
    {
        PerformanceSelectorRef?.ClosePopup();
    }

    #endregion
    #region Selector - Labor

    internal APULaborSelector LaborSelectorRef { get; set; }

    internal void LaborSelectorToggle()
    {
        if (!CanEdit)
            return;

        LaborSelectorRef?.TogglePopup();
    }
    internal void LaborSelectorClose()
    {
        LaborSelectorRef?.ClosePopup();
    }    

    #endregion
    #region Selector - Material

    internal APUMaterialSelector MaterialSelectorRef { get; set; }

    internal void MaterialSelectorToggle()
    {
        if (!CanEdit)
            return;

        MaterialSelectorRef?.TogglePopup();
    }
    internal void MaterialSelectorClose()
    {
        MaterialSelectorRef.ClosePopup();
    }

    #endregion
    #region Selector - Equipment

    internal APUEquipmentSelector EquipmentSelectorRef { get; set; }    

    internal void EquipmentSelectorToggle()
    {
        if (!CanEdit)
            return;

        EquipmentSelectorRef?.TogglePopup();
    }
    internal void EquipmentSelectorClose()
    {
        EquipmentSelectorRef.ClosePopup();
    }

    #endregion
    #region Selector - Contract

    internal APUContractSelector ContractSelectorRef { get; set; }    

    internal void ContractSelectorToggle()
    {
        if (!CanEdit)
            return;

        ContractSelectorRef?.TogglePopup();
    }
    internal void ContractSelectorClose()
    {
        ContractSelectorRef.ClosePopup();
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        BaseItemHub?.Dispose();
        BaseItemHub = null;

        ProjectHub?.Dispose();
        ProjectHub = null;

        ApuHub?.Dispose();
        ApuHub = null;
    }

    #endregion


    #region Project Export

    internal async void ExportProject()
    {
        if (SelectedProject is null)
        {
            ShowError("Select a Project!");
            return;
        }

        ProgressStart();

        await Task.Delay(200);

        try 
        {
            // https://support.syncfusion.com/kb/article/11875/how-to-convert-html-to-pdf-in-azure-app-service-linux-using-net-core
            // https://help.syncfusion.com/file-formats/pdf/convert-html-to-pdf/blink
            // -> used one -> https://help.syncfusion.com/file-formats/pdf/convert-html-to-pdf/azure#azure-app-service-linux
            var htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);

            //Set the BlinkBinaries folder path
            var blinkConverterSettings = new BlinkConverterSettings();

            //Set command line arguments to run without the sandbox.
            blinkConverterSettings.CommandLineArguments.Add("--no-sandbox");
            blinkConverterSettings.CommandLineArguments.Add("--disable-setuid-sandbox");
            htmlConverter.ConverterSettings = blinkConverterSettings;

            //https://techgrouponedev.azurewebsites.net/projectexport?projectid=e51b92f0-9a69-4849-81ff-001d66015bea
            var document = htmlConverter.Convert(NavM.BaseUri + $"projectexport?projectid={SelectedProject.Id}");
            var ms = new MemoryStream();

            document.Save(ms);
            document.Close(true);

            var base64 = Convert.ToBase64String(ms.ToArray());

            await FileSaver.SaveAsBase64("TechGroupOne - Project Export.pdf", base64);
        }
        catch (PdfException pe)
        {
            Console.WriteLine(pe);
            var msg = pe.Message + "\r\n";
            if (pe.InnerException is not null)
                msg += pe.InnerException.Message;
            ShowError(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            var msg = e.Message + "\r\n";
            if (e.InnerException is not null)
                msg += e.InnerException.Message;
            ShowError(msg);
        }

        ProgressStop();
    }

    #endregion
}