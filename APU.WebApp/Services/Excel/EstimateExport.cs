using APU.WebApp.Utils;
using APU.WebApp.Utils.DocumentHelper;
using APU.WebApp.Utils.Extensions;
using Syncfusion.XlsIO;

namespace APU.WebApp.Services.Excel;

public static partial class ExcelService
{
    public static async Task<Result<string>> EstimateExportAsync(NavigationManager navM, Project project, SfGrid<Apu> grid)
    {
        try
        {
            using var excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;  
            application.DefaultVersion = ExcelVersion.Xlsx;
         
            //Create a workbook
            var workbook = application.Workbooks.Create(1);
            var ws = workbook.Worksheets[0];

            //Disable gridlines in the worksheet
            ws.IsGridLinesVisible = false;

            ws.Range["A1"].ColumnWidth = 2;
            // Row adjustments
            ws.Range["A1"].RowHeight = 9;
            ws.Range["A2:A10"].RowHeight = 15;
            //ws.Range["A11"].FreezePanes();

            #region Logo

            //Adding a Picture
            //var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //var filePath = Path.Combine(buildDir!, """wwwroot\imgs\logo.png""");

            var httpClient = new HttpClient();
            var logoStream = await httpClient.GetStreamAsync(navM.BaseUri + "/imgs/logo.png").ConfigureAwait(false);
            var logoMemoryStream = new MemoryStream();
            await logoStream.CopyToAsync(logoMemoryStream);

            var shape = ws.Pictures.AddPicture(2, 2, logoMemoryStream);
            shape.Height = 85*20/31;
            shape.Width = 346*20/31;

            // Project Details
            ws.Range["B6"].Text = project.ProjectName;
            ws.Range["B6"].CellStyle.Font.Bold = true;
            ws.Range["B7"].Text = project.Address;
            ws.Range["B7"].WrapText = false;
            ws.Range["B8"].Text = "Phone: " + project.Phone;
            ws.Range["B9"].Text = "Email: " + project.Email;

            // Techgroupone Details
            ws.Range["M2"].Text = "TECHGROUPONE INC";
            ws.Range["M3"].Text = "8504 NW 66th Street";
            ws.Range["M4"].Text = "Miami FL 33166";
            ws.Range["M5"].Text = "Phone: (305) 517-3040";
            ws.Range["M6"].Text = "License & Insured CGC-1523588";
            ws.Range["M7"].Text = "Email: contractor@techgroupone.com";

            // Date
            ws.Range["M9"].Text = DateTimeHelper.GetFullDateText();

            #endregion
            
            #region Table Header

            // Top separator row
            ws.Range[$"A{11}"].RowHeight = 9;

            // Start col
            var hr = 12;
            var hc = 1;

            ws.Range[$"A{hr}"].RowHeight = 45;

            foreach (var gc in grid.Columns)
            {
                if (!gc.Visible)
                    continue;

                if (hc == 1)
                    EH.AddCell(ws, EH.Columns[hc], hr, "Item", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 6.5);

                else if (gc.Field == "Code")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Code", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 6.5);
                else if (gc.Field == "Description")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Description", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 30);
                else if (gc.Field == "Unit")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 5);
                else if (gc.Field == "Quantity")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Qty", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 6);

                // 01
                else if (gc.Field == "LaborSum.Total")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Labor", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "LaborSum.TotalExtend")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Labor Subtotal", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "MaterialSum.Total")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Material", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "MaterialSum.TotalExtend")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Material Subtotal", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "EquipmentSum.Total")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Equipment", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "EquipmentSum.TotalExtend")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Equipment Subtotal", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "ContractSum.Total")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Contract", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "ContractSum.TotalExtend")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Contract Subtotal", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "Sum.Total")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Price", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "Sum.TotalExtend")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Total Price", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);

                // 02
                else if (gc.Field == "LaborSum.TotalProRate")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Labor", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "LaborSum.TotalExtendProRate")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Labor Subtotal", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "MaterialSum.TotalProRate")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Material", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "MaterialSum.TotalExtendProRate")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Material Subtotal", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "EquipmentSum.TotalProRate")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Equipment", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "EquipmentSum.TotalExtendProRate")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Equipment Subtotal", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "ContractSum.TotalProRate")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Contract", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "ContractSum.TotalExtendProRate")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Contract Subtotal", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "Sum.TotalProRate")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Pro Rate Price", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "Sum.TotalExtendProRate")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Total Pro Rate", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);

                // 03
                else if (gc.Field == "LaborSum.SubTotalSuperVision")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Labor", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "LaborSum.SubTotalSuperVisionExtend")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Labor Cost Sub-Total", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "MaterialSum.SubTotalSalesTotal")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Cost Material", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "MaterialSum.SubTotalSalesTotalExtend")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Material Cost Sub-Total", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "EquipmentSum.SubTotal")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Cost Equipment", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "EquipmentSum.SubTotalExtend")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Equip. Cost Sub-Total", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "ContractSum.SubTotal")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Cost Contract", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "ContractSum.SubTotalExtend")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Contract Cost Sub-Total", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "Sum.SubTotal")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Unit Cost", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "Sum.SubTotalExtend")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Cost Sub-Total", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else if (gc.Field == "Sum.GrossTotalPct")
                    EH.AddCell(ws, EH.Columns[hc], hr, "Gross", bgColor: ExcelKnownColors.Blue, fontColor: ExcelKnownColors.White, wrapText: true, bold: true, columnWidth: 8);
                else
                    continue;

                hc++;
            }

            ws.Range[$"B{hr}:{hc}{hr}"].BorderInside(ExcelLineStyle.Thin, ExcelKnownColors.White);

            #endregion

            #region Table Data

            // Start line
            var r = 13;
            var c = 1;

            foreach (var apu in project.Apus.OrderBy(q => q.GroupId).ThenBy(q => q.ItemId))
            {
                if ((apu.GroupId == 998 && apu.ItemId == 1003) ||
                    (apu.GroupId == 999 && apu.ItemId == 1003))
                {}

                // Start col is the ItemText
                c = 1;
                if (apu.DisplayItemText)
                {
                    if (apu.IsGroupSubTotalHeader || apu.IsGroupSubTotalFooter)
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignLeft, bold: apu.DisplayAsBold, text: apu.GroupId + "-");
                    else
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignLeft, bold: apu.DisplayAsBold, text: apu.GroupId + "-" + apu.ItemId);
                }
                c++;

                foreach (var gc in grid.Columns)
                {
                    if (!gc.Visible)
                        continue;

                    if (gc.Field == "Code")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignCenter, bold: apu.DisplayAsBold, text: apu.DisplayCode ? apu.Code : "");
                    else if (gc.Field == "Description")
                        EH.AddCell(ws, EH.Columns[c], r, wrapText: true, hAlign: ExcelHAlign.HAlignLeft, bold: apu.DisplayAsBold, text: apu.DisplayDescription ? apu.Description : "");
                    else if (gc.Field == "Unit")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignCenter, bold: apu.DisplayAsBold, text: apu.DisplayUnit ? apu.Unit : "");
                    else if (gc.Field == "Quantity")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignCenter, bold: apu.DisplayAsBold, text: apu.DisplayQuantity ? apu.Quantity.ToString("0") : "");

                    // 01
                    else if (gc.Field == "LaborSum.Total")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayLaborSumTotal ? apu.LaborSum.Total.ToFormattedNumber() : "");
                    else if (gc.Field == "LaborSum.TotalExtend")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayLaborSumTotalExtend ? apu.LaborSum.TotalExtend.ToFormattedNumber() : "");
                    else if (gc.Field == "MaterialSum.Total")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayMaterialSumTotal ? apu.MaterialSum.Total.ToFormattedNumber() : "");
                    else if (gc.Field == "MaterialSum.TotalExtend")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayMaterialSumTotalExtend ? apu.MaterialSum.TotalExtend.ToFormattedNumber() : "");
                    else if (gc.Field == "EquipmentSum.Total")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayEquipmentSumTotal ? apu.EquipmentSum.Total.ToFormattedNumber() : "");
                    else if (gc.Field == "EquipmentSum.TotalExtend")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayEquipmentSumTotalExtend ? apu.EquipmentSum.TotalExtend.ToFormattedNumber() : "");
                    else if (gc.Field == "ContractSum.Total")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayContractSumTotal ? apu.ContractSum.Total.ToFormattedNumber() : "");
                    else if (gc.Field == "ContractSum.TotalExtend")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayContractSumTotalExtend ? apu.ContractSum.TotalExtend.ToFormattedNumber() : "");
                    else if (gc.Field == "Sum.Total")
                    {
                        if (apu.IsGroup1000Item1004)
                            EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: project.Bond.ToString("0.00") + " %");
                        else
                            EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplaySumTotal ? apu.Sum.Total.ToFormattedNumber() : "");
                    }
                    else if (gc.Field == "Sum.TotalExtend")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplaySumTotalExtend ? apu.Sum.TotalExtend.ToFormattedNumber() : "");

                    // 02
                    else if (gc.Field == "LaborSum.TotalProRate")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayLaborSumTotalProRate ? apu.LaborSum.TotalProRate.ToFormattedNumber() : "");
                    else if (gc.Field == "LaborSum.TotalExtendProRate")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayLaborSumTotalExtendProRate ? apu.LaborSum.TotalExtendProRate.ToFormattedNumber() : "");
                    else if (gc.Field == "MaterialSum.TotalProRate")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayMaterialSumTotalProRate ? apu.MaterialSum.TotalProRate.ToFormattedNumber() : "");
                    else if (gc.Field == "MaterialSum.TotalExtendProRate")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayMaterialSumTotalExtendProRate ? apu.MaterialSum.TotalExtendProRate.ToFormattedNumber() : "");
                    else if (gc.Field == "EquipmentSum.TotalProRate")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayEquipmentSumTotalProRate ? apu.EquipmentSum.TotalProRate.ToFormattedNumber() : "");
                    else if (gc.Field == "EquipmentSum.TotalExtendProRate")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayEquipmentSumTotalExtendProRate ? apu.EquipmentSum.TotalExtendProRate.ToFormattedNumber() : "");
                    else if (gc.Field == "ContractSum.TotalProRate")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayContractSumTotalProRate ? apu.ContractSum.TotalProRate.ToFormattedNumber() : "");
                    else if (gc.Field == "ContractSum.TotalExtendProRate")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayContractSumTotalExtendProRate ? apu.ContractSum.TotalExtendProRate.ToFormattedNumber() : "");
                    else if (gc.Field == "Sum.TotalProRate")
                    {
                        if (apu.IsGroup1000Item1004)
                            EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: project.Bond.ToString("0.00") + " %");
                        else
                            EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold,
                                text: apu.DisplaySumTotalProRate ? apu.Sum.TotalProRate.ToFormattedNumber() : "");
                    }
                    else if (gc.Field == "Sum.TotalExtendProRate")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplaySumTotalExtendProRate ? apu.Sum.TotalExtendProRate.ToFormattedNumber() : "");

                    // 03
                    else if (gc.Field == "LaborSum.SubTotalSuperVision")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayLaborSumSubTotalSuperVision ? apu.LaborSum.SubTotalSuperVision.ToFormattedNumber() : "");
                    else if (gc.Field == "LaborSum.SubTotalSuperVisionExtend")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayLaborSumSubTotalSuperVisionExtend ? apu.LaborSum.SubTotalSuperVisionExtend.ToFormattedNumber() : "");
                    else if (gc.Field == "MaterialSum.SubTotalSalesTotal")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayMaterialSumSubTotalSalesTotal ? apu.MaterialSum.SubTotalSalesTotal.ToFormattedNumber() : "");
                    else if (gc.Field == "MaterialSum.SubTotalSalesTotalExtend")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayMaterialSumSubTotalSalesTotalExtend ? apu.MaterialSum.SubTotalSalesTotalExtend.ToFormattedNumber() : "");
                    else if (gc.Field == "EquipmentSum.SubTotal")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayEquipmentSumSubTotal ? apu.EquipmentSum.SubTotal.ToFormattedNumber() : "");
                    else if (gc.Field == "EquipmentSum.SubTotalExtend")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayEquipmentSumSubTotalExtend ? apu.EquipmentSum.SubTotalExtend.ToFormattedNumber() : "");
                    else if (gc.Field == "ContractSum.SubTotal")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayContractSumSubTotal ? apu.ContractSum.SubTotal.ToFormattedNumber() : "");
                    else if (gc.Field == "ContractSum.SubTotalExtend")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplayContractSumSubTotalExtend ? apu.ContractSum.SubTotalExtend.ToFormattedNumber() : "");
                    else if (gc.Field == "Sum.SubTotal")
                    {
                        if (apu.IsGroup1000Item1004)
                            EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: project.Bond.ToString("0.00") + " %");
                        else
                            EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold,
                                text: apu.DisplaySumSubTotal ? apu.Sum.SubTotal.ToFormattedNumber() : "");
                    }
                    else if (gc.Field == "Sum.SubTotalExtend")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplaySumSubTotalExtend ? apu.Sum.SubTotalExtend.ToFormattedNumber() : "");
                    else if (gc.Field == "Sum.GrossTotalPct")
                        EH.AddCell(ws, EH.Columns[c], r, hAlign: ExcelHAlign.HAlignRight, bold: apu.DisplayAsBold, text: apu.DisplaySumGrossTotalPct ? apu.Sum.GrossTotalPct.ToString("0.00") + " %" : "");
                    
                    else
                        continue;

                    c++;
                }

                r++;
            }            

            // Adjust the Row Heights (first row!)
            for (var i = 13; i < r; i++)
            {
                ws.Range[$"{EH.Columns[0]}{i}"].CellStyle.Font.Size = 13;
                ws.Range[$"{EH.Columns[0]}{i}"].AutofitRows();
            }

            #endregion


            #region Save

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return Result<string>.OkData( Convert.ToBase64String(stream.ToArray()));

            #endregion
        }
        catch (Exception e)
        {
            return Result<string>.FailMessage(e.Message);
        }
    }
}