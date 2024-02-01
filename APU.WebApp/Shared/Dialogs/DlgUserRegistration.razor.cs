using APU.WebApp.Shared.FormClasses;
using Microsoft.AspNetCore.Components.Forms;

namespace APU.WebApp.Shared.Dialogs;

public class DlgUserRegistrationVM : ComponentBase
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        FormUser = new FC_UserRegistration();

        base.OnInitialized();
    }    

    #endregion

    #region Parameters

    [Parameter]
    public string Target { get; set; }   

    #endregion

    internal EditForm Form { get; set; }

    internal bool IsVisible { get; set; }

    internal FC_UserRegistration FormUser { get; set; }


    public void Open()
    {
        FormUser = new FC_UserRegistration();
        IsVisible = true;
        StateHasChanged();
    }
    internal void DialogCancel()
    {
        IsVisible = false;
        StateHasChanged();
        FormUser = new FC_UserRegistration();
    }

    internal void DialogSubmit()
    {
        if (Form.EditContext?.Validate() != true)
            return;

        Submit?.Invoke(FormUser);

        DialogCancel();
    }

    public Action<FC_UserRegistration> Submit { get; set; }
}