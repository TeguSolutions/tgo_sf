using System.Text.Json.Serialization;

namespace APU.DataV2.Entities;

public partial class Apu : EntityBase, ICommon
{
    [Key]
    public Guid Id { get; set; }

    public bool IsBlocked { get; set; }

    [StringLength(5)]
    public string Code { get; set; }

    //[Required(AllowEmptyStrings = false)]
    //[StringLength(200)]
    public string Description { get; set; }

    [StringLength(20)]
    public string Unit { get; set; }

    [Precision(18,2)]
    public decimal Quantity { get; set; }

    public int GroupId { get; set; }

    public int ItemId { get; set; }


    #region Notes

    //[StringLength(250)]
    public string LaborNotes { get; set; }    

    //[StringLength(250)]
    public string MaterialNotes { get; set; }

    //[StringLength(250)]
    public string EquipmentNotes { get; set; }

    //[StringLength(250)]
    public string ContractNotes { get; set; }

    #endregion

    #region Percentages

    [Precision(18,2)]
    public decimal SuperPercentage { get; set; }

    [Precision(18,2)]
    public decimal LaborGrossPercentage { get; set; }

    [Precision(18,2)]
    public decimal MaterialGrossPercentage { get; set; }

    [Precision(18,2)]
    public decimal EquipmentGrossPercentage { get; set; }

    [Precision(18,2)]
    public decimal SubcontractorGrossPercentage { get; set; }

    #endregion


    #region Navigation - Project

    public Guid ProjectId { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; }

    #endregion

    #region Navigation - Status

    public int StatusId { get; set; }

    [ForeignKey(nameof(StatusId))]
    public virtual ApuStatus Status { get; set; }

    #endregion

    #region Navigation - ApuPerformances

    [InverseProperty(nameof(ApuPerformance.Apu))]
    public virtual IList<ApuPerformance> ApuPerformances { get; set; }

    #endregion
    #region Navigation - ApuLabors

    [InverseProperty(nameof(ApuLabor.Apu))]
    public virtual IList<ApuLabor> ApuLabors { get; set; }

    #endregion
    #region Navigation - ApuMaterials

    [InverseProperty(nameof(ApuMaterial.Apu))]
    public virtual IList<ApuMaterial> ApuMaterials { get; set; }

    #endregion
    #region Navigation - ApuEquipments

    [InverseProperty(nameof(ApuEquipment.Apu))]
    public virtual IList<ApuEquipment> ApuEquipments { get; set; }

    #endregion
    #region Navigation - ApuContracts

    [InverseProperty(nameof(ApuContract.Apu))]
    public virtual IList<ApuContract> ApuContracts { get; set; }

    #endregion

    #region Nav - ProjectSchedules

    [InverseProperty(nameof(ProjectSchedule.Apu))]
    public virtual ICollection<ProjectSchedule> ProjectSchedules { get; set; }

    #endregion

    #region Temp - Migration Values

    [JsonIgnore]
    public int? OldId { get; set; }

    #endregion

    //#region Development

    //[NotMapped]
    //public ProjectSchedule Schedule { get; set; }

    //#endregion
}