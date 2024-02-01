namespace APU.DataV2.Entities;

public class VendorType
{
    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; }

    #region Nav - Contractors

    //[InverseProperty(nameof(Contractor.Type))]
    //public virtual ICollection<Contractor> Contractors { get; set; }

    #endregion
}