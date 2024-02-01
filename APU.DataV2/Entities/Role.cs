namespace APU.DataV2.Entities;

public class Role
{
    [Key]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(30)]
    public string Name { get; set; }
}