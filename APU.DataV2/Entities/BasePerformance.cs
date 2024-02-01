using System.Text.Json.Serialization;

namespace APU.DataV2.Entities;

public class BasePerformance : EntityBase, ICommon, IPerformance
{
    #region Lifecycle

    public BasePerformance()
    {
        
    }

    #endregion

    [Key]
    public Guid Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(200)]
    public string Description { get; set; }

    [Precision(18,2)]
    public decimal Value { get; set; }

    [Precision(18,4)]
    public decimal Hours { get; set; }

    #region Navigation - ApuPerformances

    [JsonIgnore]
    [InverseProperty(nameof(ApuPerformance.BasePerformance))]
    public virtual ICollection<ApuPerformance> ApuPerformances { get; set; }

    #endregion


    #region Temp - Migration Values

    [JsonIgnore]
    public int? OldId { get; set; }

    #endregion
}