namespace APU.DataV2.Entities;


public partial class User
{
    // ReSharper disable once VirtualMemberCallInConstructor
    public User()
    {
        RefreshTokens = new HashSet<UserRefreshToken>();
        Sessions = new HashSet<UserSession>();
    }

    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Email { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [StringLength(5)]
    public string Initials { get; set; }

    public bool IsBlocked { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    #region Navigation - RefreshTokens

    [InverseProperty(nameof(UserRefreshToken.User))]
    public ICollection<UserRefreshToken> RefreshTokens { get; set; }

    #endregion

    #region Navigation - UserSessions

    [InverseProperty(nameof(UserSession.User))]
    public ICollection<UserSession> Sessions { get; set; }

    #endregion


    #region Navigation - Role

    [InverseProperty(nameof(UserRole.User))]
    public virtual ICollection<UserRole> UserRoles { get; set; }

    #endregion

    [NotMapped]
    public string Monogram
    {
        get
        {
            // Todo: Parse name
            return Initials;
        }
    }


    #region LastUpdated

    public DateTime LastUpdatedAt { get; set; }

    public Guid? LastUpdatedById { get; set; }

    //[ForeignKey(nameof(LastUpdatedById))]
    //public virtual User LastUpdatedBy { get; set; } 

    #endregion
}