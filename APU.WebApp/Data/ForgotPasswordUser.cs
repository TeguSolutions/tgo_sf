using System.ComponentModel.DataAnnotations;

namespace APU.WebApp.Data;

public class ForgotPasswordUser
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is required")]
    public string Email { get; set; }
}