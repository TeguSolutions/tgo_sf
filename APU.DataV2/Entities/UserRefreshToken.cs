using UserClass = APU.DataV2.Entities.User;

namespace APU.DataV2.Entities;

public class UserRefreshToken
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [Required]
    public DateTimeOffset ExpiresAt { get; set; }

    #region Navigation - User

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    [InverseProperty(nameof(UserClass.RefreshTokens))]
    public virtual User User { get; set; }    

    #endregion

    #region Helper

    public bool IsExpired => ExpiresAt < DateTimeOffset.Now;

    #endregion
}