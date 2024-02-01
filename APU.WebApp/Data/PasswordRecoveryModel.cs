using System.ComponentModel.DataAnnotations;

namespace APU.WebApp.Data;

public class PasswordRecoveryModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please type a correct password!")]
    public string Password { get; set; }

    [Compare(nameof(Password), ErrorMessage = "Password and Confirm Password don't match!")]
    public string ConfirmPassword { get; set; }
}