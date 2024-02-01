namespace APU.DataV2.Entities;

public partial class ApuMaterial : EntityBase, ICommon, IMaterial
{
    [Key]
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required(AllowEmptyStrings = false)]
    //[StringLength(300)]
    public string Description { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string Unit { get; set; }

    [Precision(18,2)]
    public decimal Quantity { get; set; }

    [Precision(18,2)]
    public decimal Waste { get; set; }

    [Precision(18,2)]
    public decimal Price { get; set; }

    [StringLength(200)]
    public string Vendor { get; set; }

    [StringLength(30)]
    public string Phone { get; set; }
    
    [StringLength(600)]
    public string Link { get; set; }


    #region Navigation - ItemType

    public int ItemTypeId { get; set; }

    [ForeignKey(nameof(ItemTypeId))]
    public virtual ItemType ItemType { get; set; }

    #endregion


    #region Navigation - Apu

    public Guid ApuId { get; set; }

    [ForeignKey(nameof(ApuId))]
    public virtual Apu Apu { get; set; }

    #endregion

    #region Navigation - Base Material

    public Guid? BaseMaterialId { get; set; }

    [ForeignKey(nameof(BaseMaterialId))]
    public virtual BaseMaterial BaseMaterial { get; set; }

    #endregion


    #region Temp - Migration Values

    public int? OldId { get; set; }

    #endregion
}