namespace APU.DataV2.EntityModels;

public class ProjectModel : ICommon
{
    public Guid Id { get; set; }

    public bool IsBlocked { get; set; }
    public string ProjectName { get; set; }

    public bool HasSchedule { get; set; }

    public decimal Tools { get; set; }
    public decimal Bond { get; set; }
    public decimal SalesTax { get; set; }


    public DateTime LastUpdatedAt { get; set; }
}