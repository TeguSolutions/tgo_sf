namespace APU.DataV2.Entities.Common;

public class EntityBase
{
    #region LastUpdated

    public DateTime LastUpdatedAt { get; set; }

    public Guid? LastUpdatedById { get; set; }

    [ForeignKey(nameof(LastUpdatedById))]
    public virtual User LastUpdatedBy { get; set; } 

    #endregion

    public void SetLastUpdatedDb(User user)
    {
        LastUpdatedAt = DateTime.Now;
        LastUpdatedById = user.Id;
    }

    public void SetLastUpdated(User user)
    {
        LastUpdatedAt = DateTime.Now;
        LastUpdatedById = user.Id;
        LastUpdatedBy = user;
    }
}