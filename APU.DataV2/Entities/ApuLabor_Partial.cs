using APU.DataV2.Definitions;

namespace APU.DataV2.Entities;

public partial class ApuLabor
{
    [NotMapped]
    public decimal Total { get; set; }

    [NotMapped]
    public decimal TotalExtend { get; set; }

    public void Calculate(decimal apuQuantity, ApuPerformance performance)
    {
        if (apuQuantity == 0)
        {
            Reset();
            return;
        }

        TotalExtend = Cost * Quantity;

        if (WorkTypeId == LaborWorkTypeDefinitions.Workers.Id)
        {
            if (performance is null)
            {
                Reset();
                return;
            }

            TotalExtend *= performance.TotalHours;
        }

        TotalExtend = TotalExtend.AsRound();

        Total = TotalExtend / apuQuantity;
        Total = Total.AsRound();
    }

    private void Reset()
    {
        Total = 0;
        TotalExtend = 0;
    }


    [NotMapped]
    public decimal Cost { get; set; }
}