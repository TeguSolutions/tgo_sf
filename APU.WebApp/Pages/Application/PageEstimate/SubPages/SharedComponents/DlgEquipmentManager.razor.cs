using APU.WebApp.Services.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;

public class DlgEquipmentManagerVM : ComponentBase, IPopup, IDisposable
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        Equipment = new BaseEquipment();

        base.OnInitialized();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            PopupService.Add(this);
        }

        base.OnAfterRender(firstRender);
    }

    #endregion

    #region Services

    [Inject] 
    protected PopupService PopupService { get; set; }

    #endregion

    #region Parameters

    [Parameter]
    public string Target { get; set; }   

    #endregion

    internal EditForm Form { get; set; }

    public bool IsVisible { get; set; }

    internal ManagerOpenMode Mode { get; set; }

    internal IEquipment Equipment { get; set; }

    public void Open(ManagerOpenMode mode, IEquipment equipment)
    {
        PopupService.ClosePopups(this);

        Mode = mode;

        if (Mode == ManagerOpenMode.Create)
            Equipment = new BaseEquipment();

        else if (Mode == ManagerOpenMode.Update || Mode == ManagerOpenMode.UpdateMix)
            Equipment = equipment;

        else if (Mode == ManagerOpenMode.Duplicate || Mode == ManagerOpenMode.Copy)
            Equipment = new BaseEquipment()
            {
                Description = equipment.Description,
                Unit = equipment.Unit,
                Quantity = equipment.Quantity,
                Price = equipment.Price,
                Vendor = equipment.Vendor,
                Phone = equipment.Phone,
                Link = equipment.Link
            };

        IsVisible = true;
        StateHasChanged();
    }

    public void DialogSubmit(ManagerSubmitMode mode)
    {
        if (Form.EditContext?.Validate() != true)
            return;

        Submit?.Invoke((mode, Equipment));
        IsVisible = false;
        Equipment = new BaseEquipment();
    }

    public void DialogCancel()
    {
        Equipment = new BaseEquipment();
        IsVisible = false;
    }
    
    public Action<(ManagerSubmitMode mode, IEquipment equipment)> Submit { get; set; }

    #region IPopup

    public void ClosePopup()
    {
        DialogCancel();
    }    

    #endregion

    #region IDisposable

    public void Dispose()
    {
        PopupService.Remove(this);
    }    

    #endregion
}