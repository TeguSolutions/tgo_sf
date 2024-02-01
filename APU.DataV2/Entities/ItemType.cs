namespace APU.DataV2.Entities;

/// <summary>
/// TotUnds
/// </summary>
public class ItemType
{
    [Key]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string Name { get; set; }

    #region Naviation - ApuContracts

    [InverseProperty(nameof(ApuContract.ItemType))]
    public virtual ICollection<ApuContract> ApuContracts { get; set; }

    #endregion
    #region Naviation - ApuEquipments

    [InverseProperty(nameof(ApuEquipment.ItemType))]
    public virtual ICollection<ApuEquipment> ApuEquipments { get; set; }

    #endregion
    #region Naviation - ApuMaterials

    [InverseProperty(nameof(ApuMaterial.ItemType))]
    public virtual ICollection<ApuMaterial> ApuMaterials { get; set; }

    #endregion
}