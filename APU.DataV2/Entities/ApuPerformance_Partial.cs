namespace APU.DataV2.Entities;

public partial class ApuPerformance
{
    [NotMapped]
    public decimal HoursPerUnits { get; set; }

    [NotMapped]
    public decimal UnitDays { get; set; }
    [NotMapped]
    public decimal TotalHours { get; set; }
    [NotMapped]
    public decimal TotalDays { get; set; }

    public void Calculate(decimal apuQuantity)
    {
	    try
	    {
		    HoursPerUnits = Hours / Value;
		    HoursPerUnits = HoursPerUnits.AsRound(4);

		    UnitDays = 8 / HoursPerUnits;
		    UnitDays = UnitDays.AsRound();

		    TotalHours = HoursPerUnits * apuQuantity;
		    //TotalHours = TotalHours.AsRound(4);
		    TotalHours = TotalHours.AsRound(2);

		    if (TotalHours == 0)
			    TotalDays = 0;
		    else
		    {
			    TotalDays = TotalHours / 8;
			    TotalDays = TotalDays.AsRound();
		    }
	    }
	    catch (Exception e)
	    {
		    
	    }

    }
}