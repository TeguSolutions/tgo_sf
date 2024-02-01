using APU.WebApp.Services.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;

public class DlgMaterialManagerVM : ComponentBase, IPopup, IDisposable
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        Material = new BaseMaterial();

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

    internal IMaterial Material { get; set; }

    public void Open(ManagerOpenMode mode, IMaterial material)
    {
        PopupService.ClosePopups(this);

        Mode = mode;

        if (Mode == ManagerOpenMode.Create)
            Material = new BaseMaterial();

        else if (Mode == ManagerOpenMode.Update || Mode == ManagerOpenMode.UpdateMix)
            Material = material;

        else if (Mode == ManagerOpenMode.Duplicate || Mode == ManagerOpenMode.Copy)
            Material = new BaseMaterial
            {
                Description = material.Description,
                Unit = material.Unit,
                Quantity = material.Quantity,
                Price = material.Price,
                Vendor = material.Vendor,
                Phone = material.Phone,
                Link = material.Link
            };

        IsVisible = true;
        StateHasChanged();
    }

    public void DialogSubmit(ManagerSubmitMode mode)
    {
        if (Form.EditContext?.Validate() != true)
            return;

        Submit?.Invoke((mode, Material));
        IsVisible = false;
        Material = new BaseMaterial();
    }

    public void DialogCancel()
    {
        Material = new BaseMaterial();
        IsVisible = false;
    }


    public Action<(ManagerSubmitMode mode, IMaterial material)> Submit { get; set; }

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