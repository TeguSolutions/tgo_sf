namespace APU.DataV2.Entities;

public class DefaultValue : EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Precision(18,2)]
    public decimal Gross { get; set; }

    [Required]
    [Precision(18,2)]
    public decimal Supervision { get; set; }

    [Required]
    [Precision(18,2)]
    public decimal Tools { get; set; }
 
    [Required]
    [Precision(18,2)]
    public decimal WorkersComp { get; set; }

    [Required]
    [Precision(18,2)]
    public decimal Fica { get; set; }
 
    [Required]
    [Precision(18,2)]
    public decimal TopFica { get; set; }

    [Required]
    [Precision(18,2)]
    public decimal FutaSuta { get; set; }

    [Required]
    [Precision(18,2)]
    public decimal SalesTax { get; set; }

    [Required]
    [Precision(18,2)]
    public decimal Bond { get; set; }
  
    [Required]
    [Precision(18,2)]
    public decimal HrsDay { get; set; }
}