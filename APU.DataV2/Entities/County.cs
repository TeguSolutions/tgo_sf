namespace APU.DataV2.Entities;

public class County : EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Name { get; set; }

    #region Navigation - Cities

    [InverseProperty(nameof(City.County))]
    public virtual ICollection<City> Cities { get; set; }

    #endregion

    //#region Navigation - Projects

    //[InverseProperty(nameof(Project.County))]
    //public virtual ICollection<Project> Projects { get; set; }

    //#endregion


    #region Temp - Migration Values

    public int? OldId { get; set; }

    #endregion
}