using System.Text.Json.Serialization;

namespace APU.DataV2.Entities;

public class ProjectSchedule
{
    #region Lifecycle

    [JsonConstructor]
    public ProjectSchedule()
    {
        Description = "";
        Duration = "";
        Predecessor = "";
    }

    /// <summary>
    /// For new Schedule creation
    /// </summary>
    public ProjectSchedule(Guid id, int orderNo, Guid projectId, Guid apuId)
    {
        Id = id;
        OrderNo = orderNo;

        ProjectId = projectId;
        ApuId = apuId;

        ParentId = null;
        Predecessor = "";

        StartDate = DateTime.Today;
        Duration = "1";
        EndDate = DateTime.Today;

        Description = "";
    }

    #endregion

    [Key]
    public Guid Id { get; set; }

    public string Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Duration { get; set; }

    //public string DurationUnit { get; set; } = "day";
    //public int? Progress { get; set; }


    // Relations
    public Guid? ParentId { get; set; }

    public string Predecessor { get; set; }

    public int OrderNo { get; set; }

    // Custom Fields

    public bool IsHidden { get; set; }

    #region Nav - Project

    public Guid ProjectId { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; }

    #endregion

    #region Nav - Apu

    public Guid? ApuId { get; set; }   

    [JsonIgnore]
    [ForeignKey(nameof(ApuId))]
    public virtual Apu Apu { get; set; }

    #endregion

    [NotMapped]
    public string GanttDescription => Description;

    [NotMapped]
    public string GanttCustomDescription { get; set; }

    //[NotMapped]
    //public int Progress { get; set; }
}