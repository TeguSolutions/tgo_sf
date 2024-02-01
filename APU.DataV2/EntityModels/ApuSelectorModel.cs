namespace APU.DataV2.EntityModels;

public class ApuSelectorModel
{
    public Guid Id { get; set; }
    public string Description { get; set; }

    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; }

    public DateTime LastUpdatedAt { get; set; }
}