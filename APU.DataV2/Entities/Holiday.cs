namespace APU.DataV2.Entities;

public class Holiday
{
    [Key]
    public Guid Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; }

    public int? Year { get; set; }

    public int Month { get; set; }

    public int Day { get; set; }

    #region Helper

    public string YearText => Year?.ToString("0000");

    public string MonthTextNumber => Month.ToString("00");

    public string DayText => Day.ToString("00");

    public string DateText
    {
        get
        {
            var text = Day.ToString();
            text += "/" + Month.ToString("00");

            if (Year is not null)
                text += "/" + Year.Value;

            return text;
        }
    }

    #endregion
}