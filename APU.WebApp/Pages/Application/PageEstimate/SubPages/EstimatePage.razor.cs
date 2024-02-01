using System.Collections.ObjectModel;
using APU.DataV2.EntityModels;
using APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;
using APU.WebApp.Services.Excel;
using APU.WebApp.Services.Navigation;
using APU.WebApp.Services.RSMeans;
using APU.WebApp.Shared.Dialogs;
using APU.WebApp.Shared.FormClasses;
using APU.WebApp.Utils;
using Microsoft.AspNetCore.Components.Forms;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;

#pragma warning disable BL0005

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.EstimatorText}, {RD.SupervisorText}")]
public class EstimatePageVM : PageVMBase, IDisposable
{
    #region Security

    internal bool CanModifyBlockedApu(Apu apu)
    {
        if (apu is null)
            return false;

        if (apu.Project?.IsBlocked == true || apu.IsBlocked)
        {
            if ((Liu.IsAdministrator || Liu.IsSupervisor) == false)
                return false;
        }

        return true;
    }

    #endregion

    #region Project Hub

    [Inject]
    protected ProjectHubClient ProjectHub { get; set; }

    private void InitializeProjectHub()
    {
        if (ProjectHub is null)
            return;

        ProjectHub.StartHub(NavM, "Estimate");

        ProjectHub.ProjectUpdated = ProjectHub_ProjectUpdated;
        ProjectHub.ProjectHasScheduleUpdated = ProjectHub_ProjectHasScheduleUpdated;
    }

    private async void ProjectHub_ProjectUpdated(ProjectHubProjectUpdatedMessage message)
    {
        // Step 1: Get the Project Model
        var pmResult = await ProjectRepo.GetModelAsync(message.ProjectId);
        if (!pmResult.IsSuccess())
        {
            ShowError(pmResult.Message);
            return;
        }

        // Step 2: Replace the Project Model
        allProjectModels = allProjectModels.ReplaceItem(pmResult.Data);

        if (SelectedProjectId == message.ProjectId)
            await LoadProject(message.ProjectId);

        GetFilteredProjectModels(filterText);
    }

    private void ProjectHub_ProjectHasScheduleUpdated(ProjectHubProjectHasScheduleUpdatedMessage message)
    {
        var projectModel = allProjectModels.FirstOrDefault(q => q.Id == message.ProjectId);
        if (projectModel is null)
            return;

        projectModel.HasSchedule = message.HasSchedule;

        if (projectModel.Id == SelectedProject.Id)
            SelectedProject.HasSchedule = message.HasSchedule;
    }

    #endregion
    #region Apu Hub

    [Inject]
    protected ApuHubClient ApuHub { get; set; }

    private void InitializeApuHub()
    {
        if (ApuHub is null)
            return;

        ApuHub.StartHub(NavM, "Estimate");

        ApuHub.ApuCreated = ApuHub_ApuCreated;
        ApuHub.ApuCreatedMultipleLineItems = ApuHub_ApuHubCreatedMultipleLineItems;
        ApuHub.ApuUpdated = ApuHub_ApuUpdated;
        ApuHub.ApuDeleted = ApuHub_ApuDeleted;
    }

    private async void ApuHub_ApuCreated(ApuHubApuCreatedMessage message)
    {
        if (SelectedProject is null)
            return;

        if (SelectedProject.Id != message.ProjectId)
            return;

        var result = await ApuRepo.GetAsync(message.ApuId, includeApuItems: true);
        if (!result.IsSuccess())
            return;
        var apu = result.Data;

        // Step 1: Line item order management
        if (message.IsLineItem)
        {
            var orderedGroupItems = SelectedProject.Apus
                .Where(q => q.GroupId == apu.GroupId && q.ItemId is >= 1 and <= 999).OrderBy(q => q.ItemId).ToList();
            var reorderedGroupItems = orderedGroupItems.ApuInsert(apu.ItemId, apu);
            SelectedProject.Apus.ApuReorderGroup(reorderedGroupItems);
        }

        // Step 2: Local Update.
        SelectedProject.Apus.Add(apu);

        // Step 3: Calculation
        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        SelectedProject.GetFilteredApus(filterText);

       // await InvokeAsync(Grid.Refresh);
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

        // Step 2: Local Update.
        foreach (var apu in result.Data)
            SelectedProject.Apus.Add(apu);
        SelectedProject.Apus = SelectedProject.Apus.OrderBy(q => q.GroupId).ThenBy(q => q.ItemId).ToList();

        // Step 3: Calculation
        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        SelectedProject.GetFilteredApus(filterText);

        //await InvokeAsync(Grid.Refresh);
    }

    private async void ApuHub_ApuUpdated(ApuHubApuUpdatedMessage message)
    {
        if (SelectedProject is null)
            return;

        if (SelectedProject.Id != message.ProjectId)
            return;

        var result = await ApuRepo.GetAsync(message.ApuId, includeApuItems: true);
        if (!result.IsSuccess())
            return;

        var apu = result.Data;
        apu.CalculateAll(defaultValue, SelectedProject);

        if (SelectedProject.Apus.FirstOrDefault(q => q.Id == apu.Id) is null)
        {
            ApuHub_ApuCreated(new ApuHubApuCreatedMessage(message.ProjectId, message.ApuId, message.IsLineItem));
        }
        else
        {
            SelectedProject.Apus = SelectedProject.Apus.ReplaceItem(apu);

            if (message.IsLineItem && message.OrderChanged)
            {
                // Step 1: Line item order management
                var orderedGroupItems = SelectedProject.Apus
                    .Where(q => q.GroupId == apu.GroupId && q.ItemId is >= 1 and <= 999).OrderBy(q => q.ItemId)
                    .ToList();
                var reorderedGroupItems = orderedGroupItems.ApuMove(apu.ItemId, apu);

                // Step 2: Update Local
                SelectedProject.Apus.ApuReorderGroup(reorderedGroupItems);
            }

            var calculationResult = SelectedProject.Calculate(defaultValue);
            if (!calculationResult.IsSuccess())
                ShowError(calculationResult.Message);

            SelectedProject.GetFilteredApus(filterText);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async void ApuHub_ApuDeleted(ApuHubApuDeletedMessage message)
    {
        if (SelectedProject is null)
            return;

        if (SelectedProject.Id != message.ProjectId)
            return;

        var apu = SelectedProject.Apus.FirstOrDefault(q => q.Id == message.ApuId);
        if (apu is not null)
        {
            SelectedProject.Apus.Remove(apu);
            SelectedProject.Apus.ApuReorderGroup(message.ReorderedGroupItems);

            var calculationResult = SelectedProject.Calculate(defaultValue);
            if (!calculationResult.IsSuccess())
                ShowError(calculationResult.Message);

            SelectedProject.GetFilteredApus(filterText);

            //await InvokeAsync(Grid.Refresh);
        }
    }

    #endregion

    #region ElementRefs - Dialogs

    internal DlgConfirmation<Apu> ConfirmationDialogApu { get; set; }

    internal void ConfirmationDialogApuOpen(Apu apu)
    {
        ConfirmationDialogApu.Open("Are you sure to delete the following Apu?", apu.Description, apu);
    }

    internal DlgApuManager DlgApuManager { get; set; }
    internal void DlgApuManagerOpen(Apu apu = null)
    {
	    if (apu is not null && !CanModifyBlockedApu(apu))
            return;

        if (SelectedProject is null)
        {
            ShowWarning("Select a project first!");
            return;
        }

        DlgApuManager.Open(apu);
    }
    internal void DlgApuManagerSubmit(ApuFormClass formClass)
    {
        if (formClass.Id is null)
            AddApu(formClass);
        else
            UpdateApu(formClass);
    }

    #endregion
    #region ElementRef - SfGrid

    internal DataGrid<Apu> Grid { get; set; }

    //internal async void DataGridOnActionBegin(ActionEventArgs<Apu> args)
    //{
    //    if (args.RequestType == SfGridAction.BeginEdit)
    //    {
    //        if (!CanModifyBlockedApu(args.Data))
    //        {
    //            args.Cancel = true;
    //            return;
    //        }
    //    }

    //    if (args.RequestType == SfGridAction.Save)
    //    {
    //        if (!CanModifyBlockedApu(args.Data))
    //        {
    //            args.Cancel = true;
    //            return;
    //        }

    //        //var result = await UpdateApu(args.Data);
    //        //if (!result.IsSuccess())
    //        //    args.Cancel = true;
    //    }
    //}

    //internal void DataGridCommandClick(CommandClickEventArgs<Apu> args)
    //{
    //    if (args.CommandColumn.Type == CommandButtonType.Delete)
    //    {
    //        if (!CanModifyBlockedApu(args.RowData))
    //        {
    //            args.Cancel = true;
    //            return;
    //        }

    //        ConfirmationDialogApu.Open("Are you sure to delete the following Apu?", args.RowData.Description, args.RowData);
    //        args.Cancel = true;
    //    }
    //}

    //private int gridQueryCellInfoCount;
    //private int gridRowDataBoundCount;

    internal void GridQueryCellInfoHandler(QueryCellInfoEventArgs<Apu> args)
    {
        //gridQueryCellInfoCount++;
        //JS.JSRuntime.InvokeVoidAsync("console.log", "Grid Query Cell Info Count: " + gridQueryCellInfoCount);
        //return;

        // Blue columns
        if (args.Column.Field is nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalExtend) 
                              or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalExtendProRate) 
                              or nameof(Apu.Sum) + "." + nameof(Apu.Sum.SubTotalExtend))
        {
            args.Cell.AddClass(new[] { "grid-estimate-cell-blue" });
        }

        // Group Totals
        if (args.Data.IsGroupSubTotalHeader || args.Data.IsGroupSubTotalFooter)
        {
            if (args.Column.Field 
                is nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.Total) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.Total) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.Total) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.Total) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.Total) 

                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalProRate) 

                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVision) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotal) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotal) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotal) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.SubTotal) 

                //or nameof(Apu.Sum) + "." + nameof(Apu.Sum.GrossTotalPct)
                )
            {
                args.Cell.AddStyle(new[] { "color: transparent !important;" });
            }
        }

        // Group 998 Item 1003 (Total of SubTotals)
        else if (args.Data.IsGroup998Item1003)
        {
            if (args.Column.Field 
                is nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.Total) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.Total) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.Total) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.Total) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.Total) 
                
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalProRate) 
                
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVision) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotal) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotal) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotal)
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.SubTotal) 

                //or nameof(Apu.Sum) + "." + nameof(Apu.Sum.GrossTotalPct)
                )
            {
                args.Cell.AddStyle(new[] { "color: transparent !important;" });
            }
        }

        // Group 999 (Allowance) SubTotals
        else if (args.Data.IsGroup999SubTotalHeader || args.Data.IsGroup999SubTotalFooter)
        {
            if (args.Column.Field 
                is nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.Total) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.Total) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.Total) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.Total) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.Total) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalProRate) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVision) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotal) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotal) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotal) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.SubTotal)

                //args.Column.Field == nameof(Apu.Sum) + "." + nameof(Apu.Sum.GrossTotalPct)
                )
            {
                args.Cell.AddStyle(new[] { "color: transparent !important;" });
            }
        }
        
        // Group 999 (Allowance) Total
        else if (args.Data.IsGroup999Item1003)
        {
            if (args.Column.Field 
                is nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.Total) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.Total) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.Total) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.Total) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.Total) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalProRate) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVision) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotal) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotal) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotal) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.SubTotal)

                //args.Column.Field == nameof(Apu.Sum) + "." + nameof(Apu.Sum.GrossTotalPct)
                )
            {
                args.Cell.AddStyle(new[] { "color: transparent !important;" });
            }
        }

        // Group 1000 (Pro Rate) SubTotals
        else if (args.Data.IsGroup1000SubTotalHeader || args.Data.IsGroup1000SubTotalFooter)
        {
            if (args.Column.Field 
                is nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.Total) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.Total) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.Total) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.Total) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.Total) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalProRate) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalExtendProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalExtendProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalExtendProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalExtendProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalExtendProRate) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVision) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotal) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotal) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotal) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.SubTotal)

                //|| args.Column.Field == nameof(Apu.Sum) + "." + nameof(Apu.Sum.GrossTotalPct)
                )
            {
                args.Cell.AddStyle(new[] { "color: transparent !important;" });
            }
        }
        // Group 1000 (Pro Rate) Items
        else if (args.Data.IsGroup1000Item)
        {
            if (args.Column.Field 
                is nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalProRate) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalExtendProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalExtendProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalExtendProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalExtendProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalExtendProRate)
               )
            {
                args.Cell.AddStyle(new[] { "color: transparent !important;" });
            }
        }

        // Group 1000 (Project Grand Total)
        else if (args.Data.IsGroup1000Item1003)
        {
            if (args.Column.Field 
                is nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.Total) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.Total) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.Total) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.Total) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.Total) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalProRate) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVision) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotal) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotal) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotal) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.SubTotal)
                )
            {
                args.Cell.AddStyle(new[] { "color: transparent !important;" });
            }
        }

        // Group 1000 (Payment & Performance Bond)
        else if (args.Data.IsGroup1000Item1004)
        {
            if (args.Column.Field 
                is nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.Total) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalExtend) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.Total) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalExtend) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.Total) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalExtend) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.Total) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalExtend) 

                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalProRate) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalExtendProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalExtendProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalExtendProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalExtendProRate) 

                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVision) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVisionExtend) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotal) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotalExtend) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotal) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotalExtend) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotal) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotalExtend) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.GrossTotalPct))
            {
                args.Cell.AddStyle(new[] { "color: transparent !important;" });
            }
        }

        // Group 1000 (Gross Profit)
        else if (args.Data.IsGroup1000Item1005)
        {
            if (args.Column.Field 
                is nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.Total) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalExtend) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.Total) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalExtend) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.Total) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalExtend) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.Total) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalExtend) 

                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.Total) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalExtend) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalProRate) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalExtendProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalExtendProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalExtendProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalProRate)
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalExtendProRate) 

                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalExtendProRate) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVision) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVisionExtend) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotal) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotalExtend)
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotal) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotalExtend) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotal) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotalExtend)

                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.SubTotal) 
                //or nameof(Apu.Sum) + "." + nameof(Apu.Sum.GrossTotalPct) 
                //args.Column.Field == nameof(Apu.Sum) + "." + nameof(Apu.Sum.GrossTotalPct))
                )
            {
                args.Cell.AddStyle(new[] { "color: transparent !important;" });
            }
        }

        // Group 1000 (Grand Total)
        else if (args.Data.IsGroup1000Item1006)
        {
            if (args.Column.Field is nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.Total) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalExtend) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.Total) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalExtend) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.Total) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalExtend) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.Total) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalExtend)
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.Total) 

                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalProRate) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.TotalExtendProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalProRate) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.TotalExtendProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalProRate) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.TotalExtendProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalProRate) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.TotalExtendProRate) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.TotalProRate) 

                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVision) 
                or nameof(Apu.LaborSum) + "." + nameof(Apu.LaborSum.SubTotalSuperVisionExtend) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotal) 
                or nameof(Apu.MaterialSum) + "." + nameof(Apu.MaterialSum.SubTotalSalesTotalExtend) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotal) 
                or nameof(Apu.EquipmentSum) + "." + nameof(Apu.EquipmentSum.SubTotalExtend) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotal) 
                or nameof(Apu.ContractSum) + "." + nameof(Apu.ContractSum.SubTotalExtend) 
                or nameof(Apu.Sum) + "." + nameof(Apu.Sum.SubTotal)
           
                //args.Column.Field == nameof(Apu.Sum) + "." + nameof(Apu.Sum.GrossTotalPct)
                )
            {
                args.Cell.AddStyle(new[] { "color: transparent !important;" });
            }
        }

        // Separator - display the Description
        if (args.Data.ItemId is -2 or 1002 or 0 or 1000)
        {
            if (args.Column.Field == nameof(Apu.Description))
                args.Cell.AddStyle(new[] { "color: black !important;" });
        }
    }

    internal void GridRowDataBound(RowDataBoundEventArgs<Apu> args)
    {
        //gridRowDataBoundCount++;
        //JS.JSRuntime.InvokeVoidAsync("console.log", "Grid Row Data Bound Count: " + gridRowDataBoundCount);

        // If the grid is grouped - hide the separators
        //if (Grid.GroupSettings?.Columns?.Length > 0 && args.Data.ItemId == 1002)
        //{
        //    args.Row.AddStyle(new[] { "visibility: collapse;" });
        //}

        // Separators
        if (args.Data.ItemId is -2 or 1002 or 0 or 1000)
        {
            args.Row.AddClass(new[] { "e-row-transparent" });
        }

        // Bold text
        if (args.Data.ItemId is -2 or -1 or 1000 or 1001 or 1003 or 1004 or 1005 or 1006)
        {
            args.Row.AddStyle(new[] { "font-weight: 600;" });
        }
    }    

    #endregion
    #region Elements - Grid Column Chooser

    //internal EstimatePageGridColumnChooser GCS { get; set; }

    #endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await EventAggregator.PublishAsync(new HeaderLinkMessage());
            SendHeaderMessage(typeof(HeaderEstimate));

            ConfirmationDialogApu.Submit = DeleteApu;
            DlgApuManager.Submit = DlgApuManagerSubmit;

            await GetLIU();

            if (!await GetDefaultValue())
                return;

            //GCS.Initialize(Liu.Id, USS, Grid);

            #region Load UserSession

            IsBlocked = USS.GetProjectBlock(Liu.Id);
            var selectedProjectId = USS.GetSelectedProjectId(Liu.Id);

            #endregion

            await GetAllProjectModels();
            GetFilteredProjectModels();

            var fpm = FilteredProjectModels.FirstOrDefault(q => q.Id == selectedProjectId);
            if (fpm is not null)
                await LoadProject(selectedProjectId);

            InitializeApuHub();
            InitializeProjectHub();
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
            defaultValue = new DefaultValue();
            ShowError("Failed to collect Default Values!");
            return false;
        }

        defaultValue = result.Data;

        return true;
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
            SelectedProject?.GetFilteredApus(filterText);
        }
    }

    internal void Filter()
    {
        SelectedProject?.GetFilteredApus(filterText);
    }

    #endregion

    #region Items - AllProjectModels / FilteredProjectModels

    private List<ProjectModel> allProjectModels;
    private async Task GetAllProjectModels()
    {
        ProgressStart();

        var result = await ProjectRepo.GetAllModelAsync();
        if (!result.IsSuccess())
        {
            allProjectModels = new List<ProjectModel>();
            ShowError("Failed to collect Projects!");
            return;
        }

        allProjectModels = result.Data.OrderBy(p => p.ProjectName).ToList();
        //GetFilteredProjectModels();

        ProgressStop();
    }


    internal ObservableCollection<ProjectModel> FilteredProjectModels { get; set; } = new();
    private async void GetFilteredProjectModels(string projectFilter = null)
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

        await InvokeAsync(SfCbProjectFilter.RefreshDataAsync);
        await InvokeAsync(StateHasChanged);
    }

    // Project Filter
    internal SfComboBox<Guid?, ProjectModel> SfCbProjectFilter { get; set; }
    internal async void ProjectFilterChanged(SfDropDownFilteringEventArgs args)
    {
        args.PreventDefaultAction = true;
        GetFilteredProjectModels(args.Text);

        var query = new Query();
        await SfCbProjectFilter.FilterAsync(FilteredProjectModels, query);
    }

    internal bool? IsBlocked { get; set; }
    internal async void IsBlockedChanged()
    {
        if (IsBlocked == null)
            IsBlocked = true;
        else if (IsBlocked == true)
            IsBlocked = false;
        else if (IsBlocked == false)
            IsBlocked = null;

        if (SelectedProject is not null)
        {
            if ((IsBlocked == true && SelectedProject.IsBlocked == false) ||
                (IsBlocked == false && SelectedProject.IsBlocked))
            {
                await LoadProject(null);
            }
        }

        //await LoadProject(SelectedProjectId);
        USS.SetProjectBlock(Liu.Id, IsBlocked);

        GetFilteredProjectModels();
    }

    #endregion
    #region Selected Project

    internal Guid? SelectedProjectId { get; set; }

    public Project SelectedProject { get; set; }

    internal async void SelectedProjectCBChanged(ChangeEventArgs<Guid?, ProjectModel> args)
    {
	    if (SelectedProjectId != args.ItemData?.Id)
		    await LoadProject(args.ItemData?.Id);
    }

    internal async Task LoadProject(Guid? selectedProjectId)
    {
        SelectedProjectId = selectedProjectId;

        if (SelectedProjectId is null)
        {
            SelectedProject = null;
        }
        else
        {
            ProgressStart();

            var result = await ProjectRepo.GetAsync(SelectedProjectId.Value, false, true);
            if (!result.IsSuccess())
            {
                SelectedProjectId = null;
                SelectedProject = null;
                ShowError("Failed to load Project!");
                return;
            }

            if ((IsBlocked == true && result.Data.IsBlocked == false) ||
                (IsBlocked == false && result.Data.IsBlocked))
            {
                await LoadProject(null);
                ProgressStop();
                return;
            }

            SelectedProject = result.Data;

            SelectedProject.GetFilteredApus(filterText);

            var calculationResult = SelectedProject.Calculate(defaultValue);
            if (!calculationResult.IsSuccess())
                ShowError(calculationResult.Message);

            ProgressStop();
        }

        USS.SetSelectedProject(Liu.Id, SelectedProject?.Id);

        //await InvokeAsync(Grid.Refresh);
        //await InvokeAsync(StateHasChanged);
    }    

    #endregion

    #region Apu Navigation

    internal void NavigateToApu(Apu apu)
    {
        if (apu is null)
            return;

        if (!apu.IsLineItem && !apu.IsGroup999Item && !apu.IsGroup1000Item)
            return;

        NavM.NavigateTo(NavS.Estimates.APU + "?nav="  + apu.GroupId + "-" + apu.ItemId);
    }

    #endregion

    #region CRUD - Add

    private async void AddApu(ApuFormClass formClass)
    {
        if (SelectedProject is null)
        {
            ShowWarning("Select a project first!");
            return;
        }

        ProgressStart();

        var newapu = new Apu
        {
            Id = Guid.NewGuid(),
            GroupId = formClass.GroupId,
            ItemId = formClass.ItemId,
            Description = formClass.Description,

            LaborGrossPercentage = SelectedProject.GrossLabor,
            MaterialGrossPercentage = SelectedProject.GrossMaterials,
            EquipmentGrossPercentage = SelectedProject.GrossMaterials,
            SubcontractorGrossPercentage = SelectedProject.GrossContracts,
            SuperPercentage = SelectedProject.Supervision,

            Unit = formClass.Unit,
            Quantity = formClass.Quantity,
        };

        if (newapu.IsAnyLineItem)
            await AddApuLineItemAsync(newapu);
        else
            await AddApuNonLineItemAsync(newapu);

        ProgressStop();
    }

    private async Task AddApuNonLineItemAsync(Apu apu)
    {
        // Step 1: Add the Apu as NonLineItem
        var result = await ApuRepo.AddNonLineItemAsync(apu, SelectedProject, ApuStatusDefinitions.Progress, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add Apu!");
            return;
        }

        SelectedProject.Apus.Add(result.Data);

        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        SelectedProject.GetFilteredApus(filterText);

        ApuHub.SendApuCreate(new ApuHubApuCreatedMessage(result.Data.ProjectId, result.Data.Id, false));
    }

    private async Task AddApuLineItemAsync(Apu apu)
    {
        // Step 1: Line item order management
        var orderedGroupItems = SelectedProject.Apus.Where(q => q.GroupId == apu.GroupId && q.ItemId is >= 1 and <= 999).OrderBy(q => q.ItemId).ToList();
        var reorderedGroupItems = orderedGroupItems.ApuInsert(apu.ItemId, apu);

        // Step 2: Add the Apu as LineItem
        var result = await ApuRepo.AddLineItemAsync(apu, reorderedGroupItems, SelectedProject, ApuStatusDefinitions.Progress, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add Apu!");
            return;
        }

        // Step 3: Local Update.
        SelectedProject.Apus.ApuReorderGroup(reorderedGroupItems);

        SelectedProject.Apus.Add(result.Data);

        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        SelectedProject.GetFilteredApus(filterText);

        // Step 4: Project schedule
        if (SelectedProject.HasSchedule && apu.IsLineItem)
        {
            var count = await ProjectScheduleRepo.GetCountAsync(SelectedProject.Id);
            if (count == -1)
                count = 999;
            else
                count = count + 1;

            var schedule = new ProjectSchedule(id: Guid.NewGuid(), count, projectId: SelectedProject.Id, apuId: apu.Id);
            var projectScheduleResult = await ProjectScheduleRepo.AddAsync(schedule);
            if (!projectScheduleResult.IsSuccess())
                ShowError("Failed to create Project Schedule: " + projectScheduleResult.Message);
        }

        // Step 5: Broadcast the changes
        ApuHub.SendApuCreate(new ApuHubApuCreatedMessage(result.Data.ProjectId, result.Data.Id, true));
    }

    #endregion
    #region CRUD - Update

    private async void UpdateApu(ApuFormClass formClass)
    {
        if (SelectedProject is null)
        {
            ShowWarning("Select a project first!");
            return;
        }

        ProgressStart();

        var oldApu = SelectedProject.Apus.FirstOrDefault(q => q.Id == formClass.Id);
        if (oldApu is null)
        {
            ShowWarning("Original Apu is not found!?");
            return;
        }

        if (!CanModifyBlockedApu(oldApu))
        {
	        ProgressStop();
	        return;
        }

        // -- Case a: There is no Group/Items change
        if (oldApu.GroupId == formClass.GroupId && oldApu.ItemId == formClass.ItemId)
        {
            await UpdateApuAsync(oldApu, formClass);
            return;
        }


        // -- Case b: There is --

        // Case b-1: From LineItem to Non-LineItem
        if (oldApu.IsAnyLineItem != formClass.IsAnyLineItem)
        {
            ShowWarning("Can't change Line Item to Non-Line Item!");
            return;
        }

        // Case b-2: Non-LineItem
        if (!formClass.IsAnyLineItem)
        {
	        await UpdateApuAsync(oldApu, formClass);
            return;
        }

        // Case b-3: LineItem - Same Group
        if (oldApu.GroupId == formClass.GroupId)
        {
	        await UpdateApuLineItemSameGroupAsync(oldApu, formClass);
            return;
        }

        // Case b-4: LineItem - Different Group
        await UpdateApuLineItemDifferentGroupAsync(oldApu, formClass);
    }

    private async Task<Result> UpdateApuAsync(Apu apu, ApuFormClass formClass)
    {
        var result = await ApuRepo.UpdateBaseAsync(apuId: formClass.Id!.Value, groupId: formClass.GroupId, itemId: formClass.ItemId,
            code: formClass.Code, description: formClass.Description, unit: formClass.Unit, quantity: formClass.Quantity, Liu);

        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu!");
            return Result.Fail();
        }

        apu.GroupId = formClass.GroupId;
        apu.ItemId = formClass.ItemId;
        apu.Code = formClass.Code;
        apu.Description = formClass.Description;
        apu.Unit = formClass.Unit;
        apu.Quantity = formClass.Quantity;
        apu.SetLastUpdated(Liu);

        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        SelectedProject.GetFilteredApus(filterText);

        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(apu.ProjectId, apu.Id, apu.IsLineItem, false));
        ProgressStop();
        return Result.Ok();
    }

    private async Task<Result> UpdateApuLineItemSameGroupAsync(Apu apu, ApuFormClass formClass)
    {
	    apu.GroupId = formClass.GroupId;
	    apu.ItemId = formClass.ItemId;
	    apu.Code = formClass.Code;
	    apu.Description = formClass.Description;
	    apu.Unit = formClass.Unit;
	    apu.Quantity = formClass.Quantity;
	    apu.SetLastUpdated(Liu);

        // Step 1: Line item order management
        var orderedGroupItems = SelectedProject.Apus.Where(q => q.GroupId == apu.GroupId && q.ItemId is >= 1 and <= 999).OrderBy(q => q.ItemId).ToList();
        var reorderedGroupItems = orderedGroupItems.ApuMove(apu.ItemId, apu);
        //var reorderedGroupItems = orderedGroupItems.Select(q => (q.Id, q.ItemId)).ToList();

        // Step 2: Update database
        var result = await ApuRepo.UpdateBaseAndReorderGroupAsync(apuId: apu.Id, groupId: apu.GroupId, itemId: apu.ItemId, 
            code: apu.Code, description: apu.Description, unit: apu.Unit, quantity: apu.Quantity, Liu, reorderedGroupItems);

        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu!");
            return Result.Fail();
        }

        // Step 3: Update Local
        SelectedProject.Apus.ApuReorderGroup(reorderedGroupItems);

        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        SelectedProject.GetFilteredApus(filterText);
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(apu.ProjectId, apu.Id, apu.IsLineItem, true));

        ProgressStop();
        return Result.Ok();
    }

    private async Task<Result> UpdateApuLineItemDifferentGroupAsync(Apu apu, ApuFormClass formClass)
    {
	    // Step 1: Remove from the old group
        var orderedOldGroupItems = SelectedProject.Apus.Where(q => q.GroupId == apu.GroupId && q.ItemId is >= 1 and <= 999).OrderBy(q => q.ItemId).ToList();
        var reorderedOldGroupItems = orderedOldGroupItems.ApuRemove(apu);

        // Step 2: Add to the new group
        var orderedNewGroupItems = SelectedProject.Apus.Where(q => q.GroupId == formClass.GroupId && q.ItemId is >= 1 and <= 999).OrderBy(q => q.ItemId).ToList();
        //var reorderedNewGroupItems = orderedNewGroupItems.ApuInsert(formClass.ItemId, formClass);
        var reorderedNewGroupItems = orderedNewGroupItems.ApuInsert(formClass.ItemId, apu);

        // Step 3: Update database
        var result = await ApuRepo.UpdateBaseAndReorderGroupAsync(apuId: apu.Id, groupId: formClass.GroupId, itemId: formClass.ItemId, 
            code: formClass.Code, description: formClass.Description, unit: formClass.Unit, quantity: formClass.Quantity, Liu, reorderedOldGroupItems, reorderedNewGroupItems);

        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu");
            return Result.Fail();
        }

	    apu.GroupId = formClass.GroupId;
	    apu.ItemId = formClass.ItemId;
	    apu.Code = formClass.Code;
	    apu.Description = formClass.Description;
	    apu.Unit = formClass.Unit;
	    apu.Quantity = formClass.Quantity;
	    apu.SetLastUpdated(Liu);


        // Step 4: Update local
        SelectedProject.Apus.ApuReorderGroup(reorderedOldGroupItems);
        SelectedProject.Apus.ApuReorderGroup(reorderedNewGroupItems);

        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        SelectedProject.GetFilteredApus(filterText);
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(apu.ProjectId, apu.Id, apu.IsLineItem, true));

        ProgressStop();
        return Result.Ok();
    }


    internal async void UpdateApuBlock(Apu apu)
    {
        var isBlocked = !apu.IsBlocked;

        ProgressStart();

        var result = await ApuRepo.UpdateIsBlockedAsync(apu.Id, isBlocked, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu Blocked!");
            ProgressStop();
            return;
        }

        apu.IsBlocked = isBlocked;
        apu.SetLastUpdated(Liu);

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(apu.ProjectId, apu.Id, apu.IsLineItem, false));
    }    

    internal void SfDropDownApuStatus_OnValueSelect(SelectEventArgs<ApuStatus> args, Apu apu)
    {
        args.Cancel = true;
        UpdateApuStatus(apu, args.ItemData);
    }
    private async void UpdateApuStatus(Apu apu, ApuStatus status)
    {
        ProgressStart();

        var result = await ApuRepo.UpdateStatusAsync(apu.Id, status.Id, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Apu Status!");
            ProgressStop();
            return;
        }

        apu.StatusId = status.Id;
        apu.Status = status;
        apu.SetLastUpdated(Liu);

        ProgressStop();
        ApuHub.SendApuUpdate(new ApuHubApuUpdatedMessage(apu.ProjectId, apu.Id, apu.IsLineItem, false));
    }

    #endregion
    #region CRUD - Delete

    private async void DeleteApu(Apu apu)
    {
        if (SelectedProject is null)
        {
            ShowWarning("Select a project first!");
            return;
        }

        ProgressStart();

        if (apu.IsAnyLineItem)
            await DeleteApuLineItemAsync(apu);
        else
            await DeleteApuNonLineItemAsync(apu);

        ProgressStop();
    }

    private async Task DeleteApuLineItemAsync(Apu apu)
    {
        ProgressStart();

        // Step 1: Line item order management
        var orderedGroupItems = SelectedProject.Apus.Where(q => q.GroupId == apu.GroupId && q.ItemId is >= 1 and <= 999).OrderBy(q => q.ItemId).ToList();
        var reorderedGroupItems = orderedGroupItems.ApuRemove(apu);

        var result = await ApuRepo.DeleteLineItemAsync(apu.Id, reorderedGroupItems);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Apu!");
            return;
        }

        SelectedProject.Apus.Remove(apu);
        SelectedProject.Apus.ApuReorderGroup(reorderedGroupItems);

        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        SelectedProject.GetFilteredApus(filterText);

        await DeleteProjectScheduleItemAsync(apu.Id);

        ApuHub.SendApuDelete(new ApuHubApuDeletedMessage(SelectedProject.Id, apu.Id, reorderedGroupItems));

        ProgressStop();
    }

    private async Task DeleteProjectScheduleItemAsync(Guid apuId)
    {
        if (SelectedProject is null)
        {
            ShowError("Selected Project is null!");
            return;
        }

        if (!SelectedProject.HasSchedule)
            return;

        var scheduleGetAllResult = await ProjectScheduleRepo.GetAllAsync(SelectedProject.Id);
        if (!scheduleGetAllResult.IsSuccess())
        {
            ShowError("Project Schedule Item removal failed!");
            return;
        }

        var schedules = scheduleGetAllResult.Data;

        var deletableSchedule = schedules.FirstOrDefault(q => q.ApuId == apuId);
        if (deletableSchedule is null)
            return;

        var deleteResult = await ProjectScheduleRepo.DeleteAsync(deletableSchedule.Id);
        if (!deleteResult.IsSuccess())
        {
            ShowError("Failed to delete Project Schedule Item!");
            return;
        }

        schedules.Remove(deletableSchedule);
        schedules = schedules.OrderBy(q => q.OrderNo).ToList();

        var orderedSchedules = new List<Tuple<Guid, int, Guid?>>();

        var orderNo = 1;
        foreach (var schedule in schedules)
        {
            orderedSchedules.Add(new Tuple<Guid, int, Guid?>(schedule.Id, orderNo, schedule.ParentId));
            orderNo++;
        }

        await ProjectScheduleRepo.UpdateOrderNosAndParentIdAsync(SelectedProject.Id, orderedSchedules);
    }

    private async Task DeleteApuNonLineItemAsync(Apu apu)
    {
        var result = await ApuRepo.DeleteNonLineItemAsync(apu.Id);
        if (!result.IsSuccess())
        {
            ShowError("Apu deletion failed!");
            return;
        }

        SelectedProject.Apus.Remove(apu);
        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        SelectedProject.GetFilteredApus(filterText);

        ApuHub.SendApuDelete(new ApuHubApuDeletedMessage(SelectedProject.Id, apu.Id, null));
    }
    
    #endregion


    #region Export

    internal async void ExportAsExcel()
    {
        //if (SelectedProject is null)
        //    return;

        //ProgressStart();

        //var result = await ExcelService.EstimateExportAsync(NavM, SelectedProject, Grid);
        //if (!result.IsSuccess())
        //{
        //    ShowError("Failed to create Excel report!");
        //    return;
        //}

        //await FileSaver.SaveAsBase64($"TechgroupOne - Estimate - {DateTimeHelper.GetDTText()}.xlsx", result.Data);    

        //ProgressStop();
    }

    #endregion
    #region Excel Import

    internal void OpenExcelImportFileSelector()
    {
        if (SelectedProject is null)
        {
            ShowError("Select a project!");
            return;
        }

        JS.OpenFileDialog("fileinput-excelimport");
    }
    public async void ImportExcel(InputFileChangeEventArgs args)
    {
        #region Validation

        if (SelectedProject is null)
        {
            ShowError("Select a project!");
            return;
        }

        if (args.FileCount == 0)
        {
            ShowError("Select a file!");
            return;
        }

        if (args.FileCount > 1)
        {
            ShowError("Select only 1 file!");
            return;
        }

        if (args.File.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            ShowError("Invalid Content Type: " + args.File.ContentType);
            return;
        }        

        #endregion

        ProgressStart();

        var importResult = await ExcelService.EstimateImportAsync(args.File, SelectedProject.Id, Liu.Id,
            SelectedProject.GrossLabor, SelectedProject.GrossMaterials, SelectedProject.GrossEquipment,
            SelectedProject.GrossContracts, SelectedProject.Supervision);

        if (!importResult.IsSuccess())
        {
            ShowError(importResult.Message);
            return;
        }

        foreach (var apu in importResult.Data)
        {
            if (apu.IsLineItem)
            {
                // Step 1: Line item order management
                var orderedGroupItems = SelectedProject.Apus.Where(q => q.GroupId == apu.GroupId && q.ItemId is >= 1 and <= 999).OrderBy(q => q.ItemId).ToList();
                var reorderedGroupItems = orderedGroupItems.ApuInsert(apu.ItemId, apu);

                // Step 2: Local Update.
                SelectedProject.Apus.ApuReorderGroup(reorderedGroupItems);
                SelectedProject.Apus.Add(apu);
            }
            else
            {
                var exApu = SelectedProject.Apus.FirstOrDefault(q => q.GroupId == apu.GroupId && q.ItemId == apu.ItemId);
                if (exApu is null)
                {
                    SelectedProject.Apus.Add(apu);
                }
                else
                {
                    exApu.Description = apu.Description;
                }
            }
        }

        var updateResult = await ApuRepo.UpdateOrAddRangeFromExcelImportAsync(SelectedProject.Apus.ToList(), Liu);
        if (!updateResult.IsSuccess())
        {
            ShowError("Excel Import - Database update failed: " + updateResult.Message);
            await LoadProject(SelectedProject.Id);
            return;
        }

        // Local update
        foreach (var apu in SelectedProject.Apus)
            apu.SetLastUpdated(Liu);

        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

	    SelectedProject.GetFilteredApus(filterText);

        ApuHub.SendApuCreatedMultipleLineItems(
            new ApuHubApuCreatedMultipleLineItemsMessage(SelectedProject.Id, SelectedProject.Apus.Where(q => q.IsLineItem).Select(q => q.Id).ToList()));

        ProgressStop();
    }

    #endregion
    #region RSMeans

    internal void OpenRSMeanFileSelector()
    {
	    if (SelectedProject is null)
	    {
            ShowError("Select a project!");
            return;
	    }

        JS.OpenFileDialog("fileinput-rsmeans");
    }
    public async void ParseRSMeansFile(InputFileChangeEventArgs args)
    {
	    if (args.FileCount == 0)
        {
            ShowError("Select a file!");
            return;
        }

        if (args.FileCount > 1)
        {
            ShowError("Select only 1 file!");
            return;
        }

        var processResult = await RsMeanProcessor.Process(SelectedProject, Liu, args.File);
        if (!processResult.IsSuccess())
        {
            ShowError(processResult);
            return;
        }

        var result = await ApuRepo.AddRangeAsync(processResult.Data.unitCostApus, processResult.Data.assemblyCostApus);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        foreach (var apu in processResult.Data.unitCostApus)
        {
	        apu.Status = ApuStatusDefinitions.Progress;
	        apu.LastUpdatedBy = Liu;
            
	        SelectedProject.Apus.Add(apu);
        }

        foreach (var apu in processResult.Data.assemblyCostApus)
        {
	        apu.Status = ApuStatusDefinitions.Progress;
	        apu.LastUpdatedBy = Liu;
            
	        SelectedProject.Apus.Add(apu);
        }
        
        var calculationResult = SelectedProject.Calculate(defaultValue);
        if (!calculationResult.IsSuccess())
            ShowError(calculationResult.Message);

        SelectedProject.GetFilteredApus(filterText);

        StateHasChanged();

        ApuHub.SendApuCreatedMultipleLineItems(
	        new ApuHubApuCreatedMultipleLineItemsMessage(SelectedProject.Id, SelectedProject.Apus.Where(q => q.IsLineItem).Select(q => q.Id).ToList()));
    }

    #endregion


    #region IDisposable

    public void Dispose()
    {
        ProjectHub?.Dispose();
        ProjectHub = null;

        ApuHub?.Dispose();
        ApuHub = null;
    }

    #endregion
}