using Microsoft.AspNetCore.Components.Forms;
using Syncfusion.XlsIO;

namespace APU.WebApp.Services.RSMeans;

public static class RsMeanProcessor
{
    public static async Task<Result<(List<Apu> unitCostApus, List<Apu> assemblyCostApus)>> Process(Project project, User liu, IBrowserFile file)
    {
        List<string> lines;

        try
        {
            if (file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {            
                using var excelEngine = new ExcelEngine();
                var application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
                using var ms1 = new MemoryStream();
                await file.OpenReadStream().CopyToAsync(ms1);
                ms1.Position = 0;

                var workbook = excelEngine.Excel.Workbooks.Open(ms1);
                var ws = workbook.Worksheets[0];
                foreach (var c in ws.Range["A1:Z100"].Cells)
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

                lines = await ms.ReadAllLinesAsync();
            }
            else if (file.ContentType == "text/csv")
            {
                lines = await file.OpenReadStream().ReadAllLinesAsync();
            }
            else
            {
				return Result<(List<Apu> unitCostApus, List<Apu> assemblyCostApus)>.FailMessage("Unrecognized file type!");
            }
        }
        catch (Exception e)
        {
            return Result<(List<Apu> unitCostApus, List<Apu> assemblyCostApus)>.FailMessage("Parse failed: " + e.Message);
        }

        var parseResult = RSMeansParser.Parse(lines);
        if (!parseResult.IsSuccess())
        {
            return Result<(List<Apu> unitCostApus, List<Apu> assemblyCostApus)>.FailMessage(parseResult.Message);
        }

        var unitCostLineItems = parseResult.Data.UnitCostLineItems;
        var unitCostGroupId = project.GetNextAvailableLineItemGroup();
        if (unitCostGroupId is null)
        {
            return Result<(List<Apu> unitCostApus, List<Apu> assemblyCostApus)>.FailMessage("There is no available GroupId in Project!");
        }

        var assemblyCostLineItems = parseResult.Data.AssemblyCostLineItems;
        var assemblyCostGroupId = project.GetNextAvailableLineItemGroup(unitCostGroupId.Value + 1);
        if (assemblyCostGroupId is null)
        {
            return Result<(List<Apu> unitCostApus, List<Apu> assemblyCostApus)>.FailMessage("There is no available GroupId in Project!");
        }

        var lastUpdatedAt = DateTime.Now;

        var unitCostApus = new List<Apu>();
        foreach (var item in unitCostLineItems)
        {
	        var apu = new Apu();
	        apu.Id = Guid.NewGuid();
	        apu.ProjectId = project.Id;
	        apu.StatusId = ApuStatusDefinitions.Progress.Id;

	        apu.GroupId = unitCostGroupId.Value;
	        apu.ItemId = unitCostApus.Count + 1;
	        apu.Description = item.Description;

	        apu.LaborGrossPercentage = project.GrossLabor;
	        apu.MaterialGrossPercentage = project.GrossMaterials;
	        apu.EquipmentGrossPercentage = project.GrossEquipment;
	        apu.SubcontractorGrossPercentage = project.GrossContracts;
	        apu.SuperPercentage = project.Supervision;

	        apu.Unit = item.Unit;
	        apu.Quantity = item.Qty;

	        apu.LastUpdatedAt = lastUpdatedAt;
	        apu.LastUpdatedById = liu.Id;

	        if (item.PerformanceValue != 0)
	        {
		        apu.ApuPerformances.Add(new ApuPerformance
		        {
			        Id = Guid.NewGuid(),
			        ApuId = apu.Id,
			        BasePerformanceId = null,
			        Description = item.PerformanceDescription,
			        Value = item.PerformanceValue,
			        Hours = item.PerformanceHours,
			        LastUpdatedAt = lastUpdatedAt,
			        LastUpdatedById = liu.Id
		        });
	        }

	        if (item.MaterialPrice != 0)
	        {
		        apu.ApuMaterials.Add(new ApuMaterial
		        {
			        Id = Guid.NewGuid(),
			        ApuId = apu.Id,
			        BaseMaterialId = null,
			        Description = item.MaterialDescription,
			        Unit = item.MaterialUnit,
			        ItemTypeId = item.MaterialItemType.Id,
			        Quantity = item.MaterialQty,
			        Price = (item.MaterialPrice) / (1 + project.SalesTax / 100),
			        LastUpdatedAt = lastUpdatedAt,
			        LastUpdatedById = liu.Id
		        });
	        }

	        if (item.EquipmentPrice != 0)
	        {
		        apu.ApuEquipments.Add(new ApuEquipment
		        {
			        Id = Guid.NewGuid(),
			        ApuId = apu.Id,
			        BaseEquipmentId = null,
			        Description = item.EquipmentDescription,
			        Unit = item.EquipmentUnit,
			        ItemTypeId = item.EquipmentItemType.Id,
			        Quantity = item.EquipmentQty,
			        Price = item.EquipmentPrice,
			        LastUpdatedAt = lastUpdatedAt,
			        LastUpdatedById = liu.Id
		        });
	        }

	        if (item.ContractPrice != 0)
	        {
		        apu.ApuContracts.Add(new ApuContract
		        {
			        Id = Guid.NewGuid(),
			        ApuId = apu.Id,
			        BaseContractId = null,
			        Description = item.ContractDescription,
			        Unit = item.ContractUnit,
			        ItemTypeId = item.ContractItemType.Id,
			        Quantity = item.ContractQty,
			        Price = item.ContractPrice,
			        LastUpdatedAt = lastUpdatedAt,
			        LastUpdatedById = liu.Id
		        });
	        }

            unitCostApus.Add(apu);
        }

        var assemblyCostApus = new List<Apu>();
        foreach (var item in assemblyCostLineItems)
        {
	        var apu = new Apu();
	        apu.Id = Guid.NewGuid();
	        apu.ProjectId = project.Id;
	        apu.StatusId = ApuStatusDefinitions.Progress.Id;

	        apu.GroupId = assemblyCostGroupId.Value;
	        apu.ItemId = assemblyCostApus.Count + 1;
	        apu.Description = item.Description;

	        apu.LaborGrossPercentage = project.GrossLabor;
	        apu.MaterialGrossPercentage = project.GrossMaterials;
	        apu.EquipmentGrossPercentage = project.GrossEquipment;
	        apu.SubcontractorGrossPercentage = project.GrossContracts;
	        apu.SuperPercentage = project.Supervision;

	        apu.Unit = item.Unit;
	        apu.Quantity = item.Qty;

	        apu.LastUpdatedAt = lastUpdatedAt;
	        apu.LastUpdatedById = liu.Id;

	        if (item.ContractPrice != 0)
	        {
		        apu.ApuContracts.Add(new ApuContract
		        {
			        Id = Guid.NewGuid(),
			        ApuId = apu.Id,
			        BaseContractId = null,
			        Description = item.ContractDescription,
			        Unit = item.ContractUnit,
			        ItemTypeId = item.ContractItemType.Id,
			        Quantity = item.ContractQty,
			        Price = item.ContractPrice,
			        LastUpdatedAt = lastUpdatedAt,
			        LastUpdatedById = liu.Id
		        });
	        }

            assemblyCostApus.Add(apu);
		}

		return Result<(List<Apu> unitCostApus, List<Apu> assemblyCostApus)>.OkData((unitCostApus, assemblyCostApus));
    }
}