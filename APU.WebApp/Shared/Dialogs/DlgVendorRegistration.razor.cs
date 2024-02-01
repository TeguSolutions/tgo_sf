using System.Collections.ObjectModel;
using APU.WebApp.Shared.FormClasses;

namespace APU.WebApp.Shared.Dialogs;

public class DlgVendorRegistrationVM : ComponentBase
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        FormClass = new VendorFormClass();
        base.OnInitialized();
    }

    #endregion

    #region Parameters

    [Parameter]
    public string Target { get; set; }   

    #endregion

    internal bool IsVisible { get; set; }

    internal ObservableCollection<VendorType> VendorTypes { get; set; }
    internal ObservableCollection<Trade> Trades { get; set; }
    internal ObservableCollection<County> Counties { get; set; }

    internal VendorFormClass FormClass { get; set; }

    public void Open(ObservableCollection<VendorType> vendorTypes, ObservableCollection<Trade> trades, ObservableCollection<County> counties)
    {
        VendorTypes = vendorTypes;
        Trades = trades;
        Counties = counties;

        FormClass = new VendorFormClass();
        IsVisible = true;
        StateHasChanged();
    }

    internal void Cancel()
    {
        IsVisible = false;
        StateHasChanged();
        FormClass = new VendorFormClass();
    }

    internal void ValidSubmit()
    {
        Submit?.Invoke(FormClass);

        Cancel();
    }

    public Action<VendorFormClass> Submit { get; set; }
}