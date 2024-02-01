using APU.DataV2.Definitions;

namespace APU.DataV2.Entities;

public partial class ApuContract
{
    [NotMapped]
    public decimal Total { get; set; }

    [NotMapped]
    public decimal TotalExtend { get; set; }

    public void Calculate(decimal apuQuantity)
    {
        if (apuQuantity == 0)
        {
            Reset();
            return;
        }

        TotalExtend = Quantity * Price;
        if (ItemTypeId == ItemTypeDefinitions.ByUnit.Id)
            TotalExtend *= apuQuantity;

        TotalExtend = TotalExtend.AsRound();

        Total = TotalExtend / apuQuantity;
        Total = Total.AsRound();
    }

    private void Reset()
    {
        Total = 0;
        TotalExtend = 0;
    }
}