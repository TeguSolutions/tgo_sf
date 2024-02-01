namespace APU.DataV2.Entities;

public class UserPasswordRecoveryLink
{
    [Key]
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid UserId { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string EmailAddress { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsUsed { get; set; }
}