using UserClass = APU.DataV2.Entities.User;

namespace APU.DataV2.Entities;

public class UserSession
{        
    [Key]
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool? BlockProject { get; set; }

    public Guid? SelectedProjectId { get; set; }

    //[Required]
    public string EstimatePageGridColumns { get; set; }


    #region Navigation - User

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    [InverseProperty(nameof(UserClass.Sessions))]
    public virtual User User { get; set; }    

    #endregion
}