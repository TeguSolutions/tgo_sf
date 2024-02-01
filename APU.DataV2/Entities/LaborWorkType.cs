namespace APU.DataV2.Entities;

/// <summary>
/// UndHours
/// </summary>
public class LaborWorkType
{
    [Key]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string Name { get; set; }

    #region Navigation - ApuLabor

    [InverseProperty(nameof(ApuLabor.WorkType))]
    public virtual ICollection<ApuLabor> ApuLabors { get; set; }

    #endregion
}