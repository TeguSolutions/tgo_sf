namespace APU.DataV2.Entities;

public class Trade
{
    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; }
}