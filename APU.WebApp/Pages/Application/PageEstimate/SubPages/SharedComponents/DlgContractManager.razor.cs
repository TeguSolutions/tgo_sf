using APU.WebApp.Services.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;

public class DlgContractManagerVM : ComponentBase, IPopup, IDisposable
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        Contract = new BaseContract();

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

    internal IContract Contract { get; set; }

    public void Open(ManagerOpenMode mode, IContract contract)
    {
        PopupService.ClosePopups(this);

        Mode = mode;

        if (Mode == ManagerOpenMode.Create)
            Contract = new BaseContract();

        else if (Mode == ManagerOpenMode.Update || Mode == ManagerOpenMode.UpdateMix)
            Contract = contract;

        else if (Mode == ManagerOpenMode.Duplicate || Mode == ManagerOpenMode.Copy)
            Contract = new BaseContract
            {
                Description = contract.Description,
                Unit = contract.Unit,
                Quantity = contract.Quantity,
                Price = contract.Price,
                Vendor = contract.Vendor,
                Phone = contract.Phone,
                Link = contract.Link
            };

        IsVisible = true;
        StateHasChanged();
    }

    public void DialogSubmit(ManagerSubmitMode mode)
    {
        if (Form.EditContext?.Validate() != true)
            return;

        Submit?.Invoke((mode, Contract));
        IsVisible = false;
        Contract = new BaseContract();
    }

    public void DialogCancel()
    {
        Contract = new BaseContract();
        IsVisible = false;
    }

    public Action<(ManagerSubmitMode mode, IContract contract)> Submit { get; set; }

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