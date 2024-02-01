namespace APU.DataV2.Entities;

public partial class Project : EntityBase, ICommon
{
    public Project()
    {
        Apus = new List<Apu>();
    }

    [Key]
    public Guid Id { get; set; }

    public bool IsBlocked { get; set; }

    [Required(AllowEmptyStrings = false)]
    //[StringLength(300)]
    public string ProjectName { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Owner { get; set; }

    [StringLength(30)]
    public string Phone { get; set; }

    [StringLength(50)]
    public string Email { get; set; }

    [StringLength(200)]
    public string Address { get; set; }



    [StringLength(2)]
    public string State { get; set; }

    public int? Zip { get; set; }

    [StringLength(5)]
    public string Estimator { get; set; }

    [StringLength(600)]
    public string Link { get; set; }


    #region Schedule

    public bool HasSchedule { get; set; }    

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    #endregion


    #region Default Values

    [Precision(18,2)]
    public decimal Gross { get; set; }
    
    [Precision(18,2)]
    public decimal Supervision { get; set; }

    [Precision(18,2)]
    public decimal Tools { get; set; }

    [Precision(18,2)]
    public decimal Bond { get; set; }

    [Precision(18,2)]
    public decimal SalesTax { get; set; }

    [Precision(18,2)]
    public decimal GrossLabor { get; set; }

    [Precision(18,2)]
    public decimal GrossMaterials { get; set; }

    [Precision(18,2)]
    public decimal GrossEquipment { get; set; }
  
    [Precision(18,2)]
    public decimal GrossContracts { get; set; }

    #endregion

    #region Navigation - City

    public int? CityId { get; set; }
    
    [ForeignKey(nameof(CityId))]
    public virtual City City { get; set; }

    #endregion

    #region Navigation - Apus

    [InverseProperty(nameof(Apu.Project))]
    public virtual IList<Apu> Apus { get; set; }

    #endregion

    #region Nav - ProjectSchedules

    [InverseProperty(nameof(ProjectSchedule.Project))]
    public virtual ICollection<ProjectSchedule> ProjectSchedules { get; set; }

    #endregion

    #region Temp - Migration Values

    public int? OldId { get; set; }

    #endregion
}