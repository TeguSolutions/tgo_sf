namespace APU.DataV2.Entities;

public class Municipality : EntityBase
{
	[Key]
	public Guid Id { get; set; }

	[Required(AllowEmptyStrings = false)]
	public string Name { get; set; }

	public string Address { get; set; }

	public string Phone { get; set; }

	public string Fax { get; set; }

	public string WebLink { get; set; }

	public string Building { get; set; }

	public string Bid { get; set; }

	public string BidSync { get; set; }

	public int CountyId { get; set; }

	[Required]
	[ForeignKey(nameof(CountyId))]
	public virtual County County { get; set; }
}