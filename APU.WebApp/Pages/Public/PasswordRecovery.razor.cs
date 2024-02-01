using APU.WebApp.Data;
using APU.WebApp.Services.Navigation;
using APU.WebApp.Utils.Security;

namespace APU.WebApp.Pages.Public;

public class PasswordRecoveryVM : PageVMBase
{
    // https://localhost:44304/passwordrecovery/6c39d223-5795-4c79-8d58-e51b55924eb1

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (PasswordRecoveryId == Guid.Empty)
            {
                IsLoading = false;
                LinkErrorMessage = "Invalid Password Recovery Link!";
                StateHasChanged();
                return;
            }

            await GetPasswordRecovery();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region Parameters

    [Parameter]
    public Guid PasswordRecoveryId { get; set; }

    #endregion

    #region Status / Messages

    internal bool IsLoading { get; set; } = true;

    internal string LinkErrorMessage { get; set; }

    internal string ErrorMessage { get; set; }
    internal string ValidMessage { get; set; }    

    #endregion

    internal PasswordRecoveryModel Model { get; set; } = new();

    internal User User { get; set; } = new();

    private async Task GetPasswordRecovery()
    {
        if (PasswordRecoveryId == Guid.Empty)
            return;

        // Step 1 - Get the link
        var linkResult = await AuthRepo.PasswordRecoveryLinkGetAsync(PasswordRecoveryId);
        if (!linkResult.IsSuccess())
        {
            IsLoading = false;
            LinkErrorMessage = "Invalid Password Recovery Link!";
            StateHasChanged();
            return;
        }

        var link = linkResult.Data;

        // Step 2 - Link validation
        if (link.IsUsed)
        {
            IsLoading = false;
            LinkErrorMessage = "Password Recovery Link is already used!";
            StateHasChanged();
            return;
        }

        if (link.ExpiresAt < DateTimeOffset.UtcNow)
        {
            IsLoading = false;
            LinkErrorMessage = "Password Recovery Link is expired!";
            StateHasChanged();
            return;
        }

        // Step 3: Get the user
        var userResult = await UserRepo.GetAsync(link.UserId);
        if (!userResult.IsSuccess())
        {
            IsLoading = false;
            LinkErrorMessage = "User validation failed!";
            StateHasChanged();
            return;
        }

        User = userResult.Data;

        IsLoading = false;
        StateHasChanged();
    }

    internal async void UpdatePassword()
    {
        if (Model.Password != Model.ConfirmPassword)
        {
            ErrorMessage = "Password and Confirm Password don't match!";
            StateHasChanged();
            return;
        }

        IsLoading = true;
        StateHasChanged();

        var passwordHash = PasswordHash.ComputeSha256Hash(Model.Password);

        var updateResult = await UserRepo.UpdatePasswordHashAsync(User.Id, passwordHash);
        if (!updateResult.IsSuccess())
        {
            IsLoading = false;
            ErrorMessage = "Failed to update password!";
            StateHasChanged();
            return;
        }

        await AuthRepo.PasswordRecoveryLinkUpdateIsUsedAsync(PasswordRecoveryId, false);

        ValidMessage = "Password is successfully updated!";
        IsLoading = false;
        StateHasChanged();

        await Task.Delay(3000);
        NavM.NavigateTo(NavS.Login);
    }
}