using APU.WebApp.Shared.Dialogs;
using System.Collections.ObjectModel;
using APU.WebApp.Shared.FormClasses;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

public class VendorsVM : PageVMBase
{
    #region Dialogs

    internal DlgConfirmation<Vendor> DlgConfirmation { get; set; }

    internal DlgVendorRegistration DlgVendorRegistration { get; set; }
    internal void DlgVendorRegistrationOpen()
    {
        DlgVendorRegistration.Open(VendorTypes, Trades, Counties);
    }

    #endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderEstimate));
            await EventAggregator.PublishAsync(new HeaderLinkMessage());

            DlgConfirmation.Submit = DeleteVendor;
            DlgVendorRegistration.Submit = RegisterVendor;

            ProgressStart();

            await GetVendorTypesAsync();
            await GetTradesAsync();
            await GetCountiesAsync();

            await GetVendorsAsync();

            ProgressStop();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region Grid

    internal async void DataGridActionBegin(ActionEventArgs<Vendor> args)
    {
        if (args.RequestType == SfGridAction.Save)
        {
            var result = await UpdateVendorAsync(args.Data);
            if (!result.IsSuccess())
                args.Cancel = true;
        }
    }

    internal void DataGridCommandClick(CommandClickEventArgs<Vendor> args)
    {
        var result = false;

        if (args.CommandColumn.Type == CommandButtonType.Delete)
            DlgConfirmation.Open("Delete Vendor?", args.RowData.CompanyName, args.RowData);

        else if (args.CommandColumn.Type == CommandButtonType.Save)
            result = true;

        else if (args.CommandColumn.Type == CommandButtonType.Edit)
            result = true;

        else if (args.CommandColumn.Type == CommandButtonType.Cancel)
            result = true;

        if (!result)
            args.Cancel = true;
    }


    internal void DataGridDropDownVendorTypeOnValueChange(ChangeEventArgs<Guid?, VendorType> args, Vendor vendor)
    {
        vendor.TypeId = args.Value;
        vendor.Type = VendorTypes.FirstOrDefault(q => q.Id == args.Value);
    }
    internal void DataGridDropDownTradeOnValueChange(ChangeEventArgs<Guid?, Trade> args, Vendor vendor)
    {
        vendor.TradeId = args.Value;
        vendor.Trade = Trades.FirstOrDefault(q => q.Id == args.Value);
    }
    internal void DataGridDropDownCountyOnValueChange(ChangeEventArgs<int?, County> args, Vendor vendor)
    {
        vendor.CountyId = args.Value;
        vendor.County = Counties.FirstOrDefault(q => q.Id == args.Value);
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
            GetFilteredVendors();
        }
    }

    #endregion
    #region Filters - Vendor Types

    internal ObservableCollection<VendorType> VendorTypes { get; set; }

    private async Task GetVendorTypesAsync()
    {
        var result = await DefinitionsRepo.VendorTypesGetAllAsync();
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        VendorTypes = result.Data.OrderBy(q => q.Name).ToObservableCollection();
    }


    internal List<VendorType> SelectedVendorTypes { get; set; }

    internal void VendorTypeChangeHandler(MultiSelectChangeEventArgs<List<VendorType>> args)
    {
        SelectedVendorTypes = args.Value;
        GetFilteredVendors();
    }    

    #endregion
    #region Filters - Trades

    internal ObservableCollection<Trade> Trades { get; set; }

    private async Task GetTradesAsync()
    {
        var result = await DefinitionsRepo.TradeGetAllAsync();
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        Trades = result.Data.OrderBy(q => q.Name).ToObservableCollection();
    }


    internal List<Trade> SelectedTrades { get; set; }

    internal void TradeChangeHandler(MultiSelectChangeEventArgs<List<Trade>> args)
    {
        SelectedTrades = args.Value;
        GetFilteredVendors();
    }  

    #endregion
    #region Filters - Counties

    internal ObservableCollection<County> Counties { get; set; }

    private async Task GetCountiesAsync()
    {
        var result = await DefinitionsRepo.CountyGetAllAsync();
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        Counties = result.Data.OrderBy(q => q.Name).ToObservableCollection();
    }


    internal List<County> SelectedCounties { get; set; }

    internal void CountyChangeHandler(MultiSelectChangeEventArgs<List<County>> args)
    {
        SelectedCounties = args.Value;
        GetFilteredVendors();
    }  

    #endregion

    #region All Vendors

    private List<Vendor> allVendors;

    private async Task GetVendorsAsync()
    {
        var result = await VendorRepo.GetAllAsync();
        if (!result.IsSuccess())
        {
            ShowError("Failed to load Vendors!");
            return;
        }

        allVendors = result.Data.OrderBy(p => p.ContactPerson).ToList();

        GetFilteredVendors();
    }    

    #endregion

    #region Filtered Vendors

    internal ObservableCollection<Vendor> FilteredVendors { get; set; }

    internal async void GetFilteredVendors()
    {
        FilteredVendors = allVendors
            .If(!string.IsNullOrWhiteSpace(filterText), q =>
                q.Where(o =>
                    TeguStringComparer.Contains(o.CompanyName, filterText) || TeguStringComparer.CompareToFilterBool(o.CompanyName, filterText) ||
                    TeguStringComparer.Contains(o.Address, filterText) || TeguStringComparer.CompareToFilterBool(o.Address, filterText) ||
                    TeguStringComparer.Contains(o.ContactPerson, filterText) || TeguStringComparer.CompareToFilterBool(o.ContactPerson, filterText) ||
                    TeguStringComparer.Contains(o.Phone, filterText) || TeguStringComparer.CompareToFilterBool(o.Phone, filterText) ||
                    TeguStringComparer.Contains(o.CEL, filterText) || TeguStringComparer.CompareToFilterBool(o.CEL, filterText) ||
                    TeguStringComparer.Contains(o.Email, filterText) || TeguStringComparer.CompareToFilterBool(o.Email, filterText) ||
                    TeguStringComparer.Contains(o.Email2, filterText) || TeguStringComparer.CompareToFilterBool(o.Email2, filterText) ||
                    TeguStringComparer.Contains(o.Comments, filterText) || TeguStringComparer.CompareToFilterBool(o.Comments, filterText)
                )
            )
            .If(SelectedVendorTypes?.Count > 0, q =>
                q.Where(o => SelectedVendorTypes.FirstOrDefault(p => p.Id == o.TypeId) is not null)
            )
            .If(SelectedTrades?.Count > 0, q =>
                q.Where(o => SelectedTrades.FirstOrDefault(p => p.Id == o.TradeId) is not null)
            )
            .If(SelectedCounties?.Count > 0, q =>
                q.Where(o => SelectedCounties.FirstOrDefault(p => p.Id == o.CountyId) is not null)
            )
            .ToObservableCollection();

        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Crud

    internal async void RegisterVendor(VendorFormClass formClass)
    {
        if (formClass is null)
        {
            ShowError("Form is null!?");
            return;
        }

        var vendor = new Vendor();
        vendor.Id = Guid.NewGuid();
        vendor.CompanyName = formClass.CompanyName;
        vendor.Address = formClass.Address;
        vendor.ContactPerson = formClass.ContactPerson;
        vendor.Phone = formClass.Phone;
        vendor.CEL = formClass.CEL;
        vendor.Email = formClass.Email;
        vendor.Email2 = formClass.Email2;
        vendor.Url = formClass.Url;
        vendor.Comments = formClass.Comments;

        vendor.TypeId = formClass.Type?.Id;
        vendor.TradeId = formClass.Trade?.Id;
        vendor.CountyId = formClass.County?.Id;

        ProgressStart();

        var result = await VendorRepo.AddAsync(vendor);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add Vendor");
            return;
        }

        // Assign the values
        vendor.Type = formClass.Type;
        vendor.Trade = formClass.Trade;
        vendor.County = formClass.County;

        allVendors.Add(vendor);
        GetFilteredVendors();

        ProgressStop();
    }

    private async Task<Result> UpdateVendorAsync(Vendor vendor)
    {
        ProgressStart();

        var result = await VendorRepo.UpdateAsync(vendor);
        if (!result.IsSuccess())
        {
            ShowError("Vendor update failed!");
            return Result.Fail();
        }

        GetFilteredVendors();

        ProgressStop();
        return Result.Ok();
    }

    private async void DeleteVendor(Vendor vendor)
    {
        ProgressStart();

        var result = await VendorRepo.DeleteAsync(vendor.Id);
        if (!result.IsSuccess())
        {
            ShowError("Failed to delete Vendor!");
            return;
        }

        allVendors.Remove(vendor);
        GetFilteredVendors();

        ProgressStop();
    }    

    #endregion
}