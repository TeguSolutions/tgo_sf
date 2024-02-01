namespace APU.DataV2.Entities;

public partial class BaseLabor : EntityBase, ICommon, ILabor
{        
    [Key]
    public Guid Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Description { get; set; }

    [Precision(18,2)]
    public decimal Salary { get; set; }

    [Precision(18,2)]
    public decimal HrsYear { get; set; }

    [Precision(18,2)]
    public decimal HrsStandardYear { get; set; }

    [Precision(18,2)]
    public decimal HrsOvertimeYear { get; set; }
 
    [Precision(18,2)]
    public decimal VacationsDays { get; set; }
  
    [Precision(18,2)]
    public decimal HolydaysYear { get; set; }

    [Precision(18,2)]
    public decimal SickDaysYear { get; set; }
 
    [Precision(18,2)]
    public decimal BonusYear { get; set; }

    [Precision(18,2)]
    public decimal HealthYear { get; set; }
 
    [Precision(18,2)]
    public decimal LifeInsYear { get; set; }
  
    [Precision(18,2)]
    public decimal Percentage401 { get; set; }

    [Precision(18,2)]
    public decimal MeetingsHrsYear { get; set; }
 
    [Precision(18,2)]
    public decimal OfficeHrsYear { get; set; }

    [Precision(18,2)]
    public decimal TrainingHrsYear { get; set; }

    [Precision(18,2)]
    public decimal UniformsYear { get; set; }

    [Precision(18,2)]
    public decimal SafetyYear { get; set; }

    #region Navigation - ApuLabors

    [InverseProperty(nameof(ApuLabor.BaseLabor))]
    public virtual ICollection<ApuLabor> ApuLabors { get; set; }

    #endregion


    #region Temp - Migration Values

    public int? OldId { get; set; }

    #endregion
}