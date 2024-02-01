namespace APU.DataV2.Entities;

public class Vendor
{
    [Key]
    public Guid Id { get; set; }

    public string CompanyName { get; set; }

    public string Address { get; set; }

    public string ContactPerson { get; set; }

    [StringLength(30)]
    public string Phone { get; set; }

    public string CEL { get; set; }

    public string Email { get; set; }

    public string Email2 { get; set; }

    public string Url { get; set; }

    public string Comments { get; set; }


    #region Nav - Type

    public Guid? TypeId { get; set; }

    [ForeignKey(nameof(TypeId))]
    public virtual VendorType Type { get; set; }

    #endregion

    #region Nav - Trade

    public Guid? TradeId { get; set; }

    [ForeignKey(nameof(TradeId))]
    public virtual Trade Trade { get; set; }

    #endregion

    #region Nav - County

    public int? CountyId { get; set; }

    [ForeignKey(nameof(CountyId))]
    public virtual County County { get; set; }

    #endregion
}