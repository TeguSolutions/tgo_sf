using System.Text.Json.Serialization;

namespace APU.DataV2.Entities;

public partial class Apu
{
    public void SetPreviousValues()
    {
        PreviousSuperPercentage = SuperPercentage;
        PreviousLaborGrossPercentage = LaborGrossPercentage;
        PreviousMaterialGrossPercentage = MaterialGrossPercentage;
        PreviousEquipmentGrossPercentage = EquipmentGrossPercentage;
        PreviousSubcontractorGrossPercentage = SubcontractorGrossPercentage;

        PreviousLaborNotes = LaborNotes;
        PreviousMaterialNotes = MaterialNotes;
        PreviousEquipmentNotes = EquipmentNotes;
        PreviousContractNotes = ContractNotes;
    }

    public void RestorePreviousValues()
    {
        SuperPercentage = PreviousSuperPercentage;
        LaborGrossPercentage = PreviousLaborGrossPercentage;
        MaterialGrossPercentage = PreviousMaterialGrossPercentage;
        EquipmentGrossPercentage = PreviousEquipmentGrossPercentage;
        SubcontractorGrossPercentage = PreviousSubcontractorGrossPercentage;

        LaborNotes = PreviousLaborNotes;
        MaterialNotes = PreviousMaterialNotes;
        EquipmentNotes = PreviousEquipmentNotes;
        ContractNotes = PreviousContractNotes;
    }

    [NotMapped]
    [JsonIgnore]
    public decimal PreviousSuperPercentage { get; set; }
    [NotMapped]
    [JsonIgnore]
    public decimal PreviousLaborGrossPercentage { get; set; }
    [NotMapped]
    [JsonIgnore]
    public decimal PreviousMaterialGrossPercentage { get; set; }
    [NotMapped]
    [JsonIgnore]
    public decimal PreviousEquipmentGrossPercentage { get; set; }
    [NotMapped]
    [JsonIgnore]
    public decimal PreviousSubcontractorGrossPercentage { get; set; }


    [NotMapped]
    [JsonIgnore]
    public string PreviousLaborNotes { get; set; }
    [NotMapped]
    [JsonIgnore]
    public string PreviousMaterialNotes { get; set; }
    [NotMapped]
    [JsonIgnore]
    public string PreviousEquipmentNotes { get; set; }
    [NotMapped]
    [JsonIgnore]
    public string PreviousContractNotes { get; set; }
}