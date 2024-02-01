namespace APU.DataV2.Entities;

public class Certificate : EntityBase
{
	[Key]
	public Guid Id { get; set; }

	[Required(AllowEmptyStrings = false)]
	public string Name { get; set; }

	public string Initials { get; set; }

	public string IssuedBy { get; set; }

	public DateTime IssuedAt { get; set; }

	public DateTime ExpiresAt { get; set; }

	public string Link { get; set; }
}