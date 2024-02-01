namespace APU.DataV2.Utils.Extensions;

public static class DecimalExtensions
{
    public static decimal AsRound(this decimal value, int places = 2)
    {
        return Math.Round(value, places);
    }

    //public static decimal? AsRound(this decimal? value, int places = 2)
    //{
    //    if (value is null)
    //        return null;

    //    return Math.Round(value.Value, places);
    //}
}