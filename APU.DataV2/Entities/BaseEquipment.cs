namespace APU.DataV2.Entities;

public class BaseEquipment : EntityBase, ICommon, IEquipment
{        
    [Key]
    public Guid Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(300)]
    public string Description { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string Unit { get; set; }

    [Precision(18,2)]
    public decimal Quantity { get; set; }

    [Precision(18,2)]
    public decimal Price { get; set; }

    [StringLength(100)]
    public string Vendor { get; set; }

    [StringLength(30)]
    public string Phone { get; set; }

    [StringLength(600)]
    public string Link { get; set; }

    #region Navigation - ApuEquipments

    [InverseProperty(nameof(ApuEquipment.BaseEquipment))]
    public virtual ICollection<ApuEquipment> ApuEquipments { get; set; }

    #endregion


    #region Temp - Migration Values

    public int? OldId { get; set; }

    #endregion
}