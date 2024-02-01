using System.Text.Json.Serialization;

namespace APU.DataV2.Entities;

public partial class ApuPerformance : EntityBase, ICommon, IPerformance
{
    [Key]
    public Guid Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    //[StringLength(200)]
    public string Description { get; set; }

    [Precision(18,2)]
    public decimal Value { get; set; }

    [Precision(18,4)]
    public decimal Hours { get; set; }

    #region Navigation - Apu

    public Guid ApuId { get; set; }

    [ForeignKey(nameof(ApuId))]
    public virtual Apu Apu { get; set; }

    #endregion

    #region Navigation - Base Performance

    public Guid? BasePerformanceId { get; set; }

    [ForeignKey(nameof(BasePerformanceId))]
    public virtual BasePerformance BasePerformance { get; set; }

    #endregion


    #region Temp - Migration Values

    public int? OldId { get; set; }

    #endregion
}