using Syncfusion.XlsIO;

namespace APU.WebApp.Utils.DocumentHelper;

/// <summary>
/// Excel Helper
/// </summary>
public static class EH
{
    static EH()
    {
        Columns = new List<string>()
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U","V", "W", "X", "Y", "Z", 
            "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU","AV", "AW", "AX", "AY", "AZ", 
            "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU","BV", "BW", "BX", "BY", "BZ", 
            //"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U","V", "W", "X", "Y", "Z", 
        };
    }

    public static List<string> Columns { get; }

    public static void AddCell(IWorksheet ws, string col, int row, string text = "", bool wrapText = false, 
        ExcelHAlign hAlign = ExcelHAlign.HAlignCenter, ExcelVAlign vAlign = ExcelVAlign.VAlignCenter,
        ExcelKnownColors bgColor = ExcelKnownColors.White, ExcelKnownColors fontColor = ExcelKnownColors.Black,
        double? columnWidth = null,
        bool bold = false)
    {
        ws.Range[$"{col}{row}"].Text = text;
        ws.Range[$"{col}{row}"].WrapText = wrapText;

        ws.Range[$"{col}{row}"].CellStyle.HorizontalAlignment = hAlign;
        ws.Range[$"{col}{row}"].CellStyle.VerticalAlignment = vAlign;
        ws.Range[$"{col}{row}"].CellStyle.ColorIndex = bgColor;
        ws.Range[$"{col}{row}"].CellStyle.Font.Color = fontColor;
        ws.Range[$"{col}{row}"].CellStyle.Font.Bold = bold;

        //if (borderInline)
        //    ws.Range[$"{col}{row}"].BorderInside(ExcelLineStyle.Thin, ExcelKnownColors.White);

        if (columnWidth is not null)
            ws.Range[$"{col}{row}"].ColumnWidth = columnWidth.Value;
    }
}