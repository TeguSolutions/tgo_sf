using System.ComponentModel.DataAnnotations;

namespace APU.WebApp.Shared.FormClasses;

public class VendorFormClass
{
    //public Guid? Id { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Company Name is required!")]
    public string CompanyName { get; set; }
    
    public VendorType Type { get; set; }

    public Trade Trade { get; set; }

    public County County { get; set; }

    public string Address { get; set; }

    public string ContactPerson { get; set; }

    [StringLength(30)]
    public string Phone { get; set; }

    public string CEL { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    [EmailAddress]
    public string Email2 { get; set; }

    public string Url { get; set; }

    public string Comments { get; set; }



}