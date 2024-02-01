namespace APU.WebApp.Utils;

public static class DateTimeHelper
{
    public static string GetFullDateText()
    {
        var n = DateTime.Now;

        var text = $"{n.DayOfWeek}, {GetMonth(n.Month)} {n.Day}, {n.Year}";
        return text;
    }

    private static string GetMonth(int month)
    {
        if (month == 1)
            return "January";
        if (month == 2)
            return "February";
        if (month == 3)
            return "March";
        if (month == 4)
            return "April";
        if (month == 5)
            return "May";
        if (month == 6)
            return "June";
        if (month == 7)
            return "July";
        if (month == 8)
            return "August";
        if (month == 9)
            return "September";
        if (month == 10)
            return "October";
        if (month == 11)
            return "November";
        if (month == 12)
            return "December";
        return "";
    }

    public static string GetDTText()
    {
        var n = DateTime.Now;
        return $"{n.Year}.{n.Month:00}.{n.Day:00}.{n.Hour:00}.{n.Minute:00}.{n.Second:00}";
    }
}