using System.ComponentModel.DataAnnotations;

namespace APU.WebApp.Shared.FormClasses;

public class FC_UserRegistration
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    [StringLength(50)]
    [EmailAddress]
    public string Email { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string Name { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Initials is required")]
    [StringLength(5)]
    public string Initials { get; set; }
}