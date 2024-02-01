namespace APU.WebApp.Utils.Extensions;

public static class DecimalExtensions
{
    public static string ToN2(this decimal number)
    {
        return number.ToString("N2");
    }
    public static string ToN2(this decimal? number)
    {
        if (number is null)
            return string.Empty;

        return number.Value.ToString("N2");
    }

    public static string ToN4(this decimal number)
    {
        return number.ToString("N4");
    }
    public static string ToN4(this decimal? number)
    {
        if (number is null)
            return string.Empty;

        return number.Value.ToString("N4");
    }

    public static string ToDollar(this decimal number)
    {
        return "$" + number.ToString("N2");
    }
    public static string ToDollar(this decimal? number)
    {
        if (number is null)
            return string.Empty;

        return "$" + number.Value.ToString("N2");
    }

    public static string ToPercentage(this decimal number)
    {
        return (number / 100).ToString("P2");
    }
    public static string ToPercentage(this decimal? number)
    {
        if (number is null)
            return string.Empty;

        return (number.Value / 100).ToString("P2");
    }

    public static string ToFormattedNumber(this decimal text)
    {
        return text.ToString("###,###,###,###,###,##0.00");
    }
}