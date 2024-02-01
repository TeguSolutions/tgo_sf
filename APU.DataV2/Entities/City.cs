namespace APU.DataV2.Entities;

public class City : EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(100)]
    public string Address { get; set; }

    [StringLength(50)]
    public string Phone { get; set; }

    [StringLength(50)]
    public string Fax { get; set; }

    [StringLength(300)]
    public string Web { get; set; }

    [StringLength(300)]
    public string Building { get; set; }

    [StringLength(300)]
    public string Bids { get; set; }

    [StringLength(300)]
    public string BidSync { get; set; }


    #region Navigation - County

    public int CountyId { get; set; }

    [ForeignKey(nameof(CountyId))]
    public virtual County County { get; set; }

    #endregion

    #region Navigation - Projects

    [InverseProperty(nameof(Project.City))]
    public virtual ICollection<Project> Projects { get; set; }

    #endregion


    #region Temp - Migration Values

    public int? OldId { get; set; }

    #endregion
}