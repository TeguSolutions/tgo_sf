using System.Diagnostics;
using System.Text.Encodings.Web;
using APU.WebApp.Data;
using APU.WebApp.Services.Email;
using APU.WebApp.Services.Navigation;

namespace APU.WebApp.Pages.Public;

public class ForgotPasswordVM : PageVMBase
{
    [Inject]
    public IEmailSender EmailSender { get; set; }

    public ForgotPasswordUser Model { get; set; } = new();

    internal string ErrorMessage { get; set; }
    internal string ValidMessage { get; set; }

    internal async void CreatePasswordRecoveryLink()
    {
        ProgressStart();

        // Step 1: User check
        var userResult = await UserRepo.GetByEmailAsync(Model.Email);
        if (!userResult.IsSuccess())
        {
            ErrorMessage = "Server error!";
            ProgressStop();
            return;
        }

        if (userResult.Data is null)
        {
            ErrorMessage = "Email was not found!";
            ProgressStop();
            return;
        }

        // Step 2: Generate the Password Recovery Link
        var passwordRecoveryLink = new UserPasswordRecoveryLink
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow + TimeSpan.FromDays(7),
            UserId = userResult.Data.Id,
            EmailAddress = userResult.Data.Email,
            IsUsed = false
        };

        var linkResult = await AuthRepo.PasswordRecoveryLinkAddAsync(passwordRecoveryLink);
        if (!linkResult.IsSuccess())
        {
            ErrorMessage = "Unexpected error happened! Please try again.";
            ProgressStop();
            return;
        }

        // Step 3: Send the Email
        var emailResult = await Send(userResult.Data, passwordRecoveryLink);
        if (!emailResult)
        {
            ErrorMessage = "Email sending failed! Please try again.";
            ProgressStop();
            return;
        }

        ValidMessage = "Password recovery link created and sent successfully to the given Email Address.";
        ProgressStop();

        await Task.Delay(4000);
        NavM.NavigateTo(NavS.Index);
    }

    private async Task<bool> Send(User user, UserPasswordRecoveryLink pwlink)
    {
        try
        {
            var passwordRecoveryUrl = NavM.BaseUri.TrimEnd('/') + NavS.PasswordRecovery + "/" + pwlink.Id;
            var htmlContent = "<h3>Password reset</h3><p>Dear " + user.Name +
                              ", We have received a request to recover your lost password, associated with this email address.  </p>" +
                              "<br/>" +
                              "<p>Please visit the following link to set new password: </p>" +
                              "<br/>" +
                              "<a href=" + HtmlEncoder.Default.Encode(passwordRecoveryUrl) + ">Click to this link to recover your password!</a>";

            var sendResult = await EmailSender.SendEmailAsync(user.Email, "TechGroupOne password recovery", htmlContent);
            return sendResult;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }
}