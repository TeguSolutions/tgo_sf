namespace APU.WebApp.Utils;

public static class ExportTemplate
{
    public static ExcelHeader GetExcelHeader(int colspan, string value)
    {
        var headerContent = new List<ExcelRow>
        {
            new()
            {
                Cells = new List<ExcelCell>
                {
                    new()
                    {
                        ColSpan = colspan,
                        RowSpan = 2,
                        Style = new ExcelStyle
                        {
                            //FontName = "Segoe UI",
                            FontColor = "#101010",
                            FontSize = 20,
                            Bold = true,
                            HAlign = ExcelHorizontalAlign.Right 
                        },
                        Value = value
                    }
                }
            }
        };
        var header = new ExcelHeader
        {
            HeaderRows = 2,
            Rows = headerContent
        };

        return header;
    }

    public static ExcelTheme GetExcelTheme()
    {
        var theme = new ExcelTheme
        {
            Header = new ExcelStyle
            {
                FontSize = 12,
                Bold = true,
                HAlign = ExcelHorizontalAlign.Center,
                VAlign = ExcelVerticalAlign.Center
            },

            Record = new ExcelStyle
            {
                FontSize = 11,
                VAlign = ExcelVerticalAlign.Center, 
                WrapText = true
            }
        };

        return theme;
    }
}