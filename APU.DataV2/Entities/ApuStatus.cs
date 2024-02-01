namespace APU.DataV2.Entities;

public class ApuStatus
{
    [Key]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string Name { get; set; }

    //#region Navigation - Apus

    //[InverseProperty(nameof(Apu.Status))]
    //public virtual ICollection<Apu> Apus { get; set; }

    //#endregion
}
