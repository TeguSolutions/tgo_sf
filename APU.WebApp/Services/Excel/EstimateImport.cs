using Microsoft.AspNetCore.Components.Forms;
using Microsoft.VisualBasic.FileIO;
using Syncfusion.XlsIO;

namespace APU.WebApp.Services.Excel;

public static partial class ExcelService
{
    public static async Task<Result<List<Apu>>> EstimateImportAsync(IBrowserFile file, Guid projectId, Guid liuId, decimal grossLabor, decimal grossMaterials, decimal grossEquipment, decimal grossContracts, decimal supervision)
    {
        var apus = new List<Apu>();

        try
        {
	        using var excelEngine = new ExcelEngine();
	        var application = excelEngine.Excel;
	        application.DefaultVersion = ExcelVersion.Xlsx;
	        using var ms1 = new MemoryStream();
	        await file.OpenReadStream().CopyToAsync(ms1);
	        ms1.Position = 0;

	        var workbook = excelEngine.Excel.Workbooks.Open(ms1);
	        var ws = workbook.Worksheets[0];
	        foreach (var c in ws.Range["A1:E200"].Cells)
	        {
		        // Text Format Fix
		        if (!string.IsNullOrWhiteSpace(c.Text))
		        {
			        c.Text = c.Text.Trim();
		        }

		        // Number Format Fix
		        if (c.NumberFormat != "General")
		        {
			        c.NumberFormat = "General";
		        }
	        }

	        using var ms = new MemoryStream();
	        workbook.SaveAs(ms, ",");
	        ms.Position = 0;

	        var lines = await ms.ReadAllLinesAsync();

            var lastUpdatedAt = DateTime.Now;

            var parsedLines = new List<string[]>();
            foreach (var line in lines)
            {
	            using var textFieldParser = new TextFieldParser(new StringReader(line));
	            textFieldParser.SetDelimiters(",");
	            var lineCells = textFieldParser.ReadFields();
	            if (lineCells is not null)
		            parsedLines.Add(lineCells);
	            else
	            {
		            //ignore
	            }
            }


	        foreach (var v in parsedLines)
	        {
		        //var v = line.Split(",");
                if (v.Length < 5)
                    continue;

                if (v[0].ToLower() == "group" || v[1].ToLower() == "item" || string.IsNullOrWhiteSpace(v[0]))
                    continue;

                try
                {
	                var groupId = int.Parse(v[0]);
	                var itemId = int.Parse(v[1]);
	                var description = v[2];
	                var unit = v[3];
	                decimal quantity = 0;
	                if (!string.IsNullOrWhiteSpace(v[4]))
		                quantity = decimal.Parse(v[4]);
                
	                var apu = new Apu();
	                apu.Id = Guid.NewGuid();
                    apu.ProjectId = projectId;
	                apu.StatusId = ApuStatusDefinitions.Progress.Id;

	                apu.GroupId = groupId;
	                apu.ItemId = itemId;
	                apu.Description = description;

                    if (apu.IsLineItem)
                    {
	                    apu.LaborGrossPercentage = grossLabor;
	                    apu.MaterialGrossPercentage = grossMaterials;
	                    apu.EquipmentGrossPercentage = grossEquipment;
	                    apu.SubcontractorGrossPercentage = grossContracts;
	                    apu.SuperPercentage = supervision;

	                    apu.Unit = unit;
	                    apu.Quantity = quantity;
                    }

                    apu.LastUpdatedAt = lastUpdatedAt;
	                apu.LastUpdatedById = liuId;

	                apus.Add(apu);
                }
                catch (Exception e)
                {
	                Console.WriteLine(e);
                }
            }
        }
        catch (Exception e)
        {
			return Result<List<Apu>>.FailMessage(e.Message);
        }

		return Result<List<Apu>>.OkData(apus);
    }
}