using APU.WebApp.Data;
using APU.WebApp.Services.Navigation;

namespace APU.WebApp.Pages.Public;

public class LoginVM : PageVMBase
{
    public LoginUser User { get; set; } = new();

    internal string ErrorMessage { get; set; }

    internal async void Login()
    {
        ErrorMessage = "";

        var authResult = await CASP.Authenticate(User.Email, User.Password);
        if (!authResult.success)
        {
            if (string.IsNullOrWhiteSpace(authResult.message))
				ErrorMessage = "Email or password are not correct!";
            else
				ErrorMessage = authResult.message;

            StateHasChanged();
            return;
        }
        
        NavM.NavigateTo(NavS.Index, true);
    }

    internal void NavigateToForgotPassword()
    {
        NavM.NavigateTo(NavS.ForgotPassword);
    }
}