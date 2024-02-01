using System.Diagnostics;

namespace APU.DataV2.Utils.Helpers;

public static class TeguStringComparer
{


    public static decimal Compare(string text, string filter)
    {
        try
        {
            text = text.TrimEnd(' ');

            text = text.ToLower();
            filter = filter.ToLower();

            var texts = text.Split(' ');
            var filters = filter.Split(' ');

            decimal totcount = 0;
            decimal matchcount1 = 0;
            foreach (var t in texts)
            {
                totcount++;
                if (filters.Contains(t))
                    matchcount1++;
            }

            decimal matchcount2 = 0;
            foreach (var f in filters)
            {
                totcount++;
                if (texts.Contains(f))
                    matchcount2++;
            }

            return (matchcount1 + matchcount2) / totcount;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return 0;
        }
    }


    public static bool CompareToFilterBool(string text, string filter, decimal threshold = (decimal)0.6)
    {
        var result = CompareToFilter(text, filter);
        if (result >= threshold) return 
            true;
        return false;
    }

    public static decimal CompareToFilter(string text, string filter)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        text = text.TrimEnd(' ');
        text = text.ToLower();
        filter = filter.ToLower();

        //filter = filter.Replace(":", "");
        //filter = filter.Replace("-", "");

        var texts = text.ToLower().Split(' ');
        var filters = filter.ToLower().Split(' ');

        decimal totcount = 0;

        decimal matchcount = 0;
        foreach (var f in filters)
        {
            if (string.IsNullOrWhiteSpace(f))
                continue;

            totcount++;
            if (texts.Contains(f))
                matchcount++;
        }

        return (matchcount) / totcount;
    }


    public static bool Contains(string text, string filter)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text.ToLower().Contains(filter.ToLower());
    }
}