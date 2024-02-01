using Microsoft.VisualBasic.FileIO;

namespace APU.WebApp.Services.RSMeans;

public static class RSMeansParser
{
    public static Result<(List<RSMeansUnitCostLineItem> UnitCostLineItems, List<RSMeansAssemblyCostLineItem> AssemblyCostLineItems)> Parse(List<string> lines)
    {
        if (lines is null || lines.Count == 0)
            return Result<(List<RSMeansUnitCostLineItem> UnitCostLineItems, List<RSMeansAssemblyCostLineItem> AssemblyCostLineItems)>.FailMessage("Invalid text data!");

        var parsedLines = new List<string[]>();
        try
        {
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
        }
        catch (Exception e)
        {
            return Result<(List<RSMeansUnitCostLineItem> UnitCostLineItems, List<RSMeansAssemblyCostLineItem> AssemblyCostLineItems)>.FailMessage("Csv parse failed - TextFieldParser error: " + e.Message);
        }

        try
        {
            var unitCostLineItems = new List<RSMeansUnitCostLineItem>();
            var assemblyCostLineItems = new List<RSMeansAssemblyCostLineItem>();

            var rsmeansFileType = GetRsMeansFileType(parsedLines);
            if (rsmeansFileType is RSMeansFileType.Unknow)
                return Result<(List<RSMeansUnitCostLineItem> UnitCostLineItems, List<RSMeansAssemblyCostLineItem> AssemblyCostLineItems)>.FailMessage("Couldn't determine the file type");

            if (rsmeansFileType is RSMeansFileType.Mix)
            {
                var unitCostLineItemsResult = Get_Mix_UnitCostLineItems(parsedLines);
                var assemblyCostLineItemsResult = Get_Mix_AssemblyCostLineItems(parsedLines);

                if (!unitCostLineItemsResult.IsSuccess() || !assemblyCostLineItemsResult.IsSuccess())
                    return Result<(List<RSMeansUnitCostLineItem> UnitCostLineItems, List<RSMeansAssemblyCostLineItem> AssemblyCostLineItems)>.Fail();

                unitCostLineItems = unitCostLineItemsResult.Data;
                assemblyCostLineItems = assemblyCostLineItemsResult.Data;
            }
            else if (rsmeansFileType is RSMeansFileType.OnlyUnitCost)
            {
                var unitCostLineItemsResult = Get_OnlyUnitCostLineItems(parsedLines);
                if (!unitCostLineItemsResult.IsSuccess())
                    return Result<(List<RSMeansUnitCostLineItem> UnitCostLineItems, List<RSMeansAssemblyCostLineItem> AssemblyCostLineItems)>.Fail();

                unitCostLineItems = unitCostLineItemsResult.Data;
            }
            else if (rsmeansFileType is RSMeansFileType.OnlyAssemblyCost)
            {
                var assemblyCostLineItemsResult = Get_OnlyAssemblyCostLineItems(parsedLines);
                if (!assemblyCostLineItemsResult.IsSuccess())
                    return Result<(List<RSMeansUnitCostLineItem> UnitCostLineItems, List<RSMeansAssemblyCostLineItem> AssemblyCostLineItems)>.Fail();

                assemblyCostLineItems = assemblyCostLineItemsResult.Data;
            }

            return Result<(List<RSMeansUnitCostLineItem> UnitCostLineItems, List<RSMeansAssemblyCostLineItem> AssemblyCostLineItems)>.OkData((unitCostLineItems, assemblyCostLineItems));
        }
        catch (Exception e)
        {
            return Result<(List<RSMeansUnitCostLineItem> UnitCostLineItems, List<RSMeansAssemblyCostLineItem> AssemblyCostLineItems)>.FailMessage("Csv parse failed " + e.Message);
        }
    }

    private static RSMeansFileType GetRsMeansFileType(List<string[]> lines)
    {
        // Only Assembly Cost Lines
        foreach (var line in lines)
        {
            if (line[Cell('F')] == "Assembly Cost Estimate")
                return RSMeansFileType.OnlyAssemblyCost;
        }

        // Only Unit Cost Lines
        var hasLegend = false;
        var hasEstimate = false;

        foreach (var line in lines)
        {
            if (line[Cell('A')].ToLower() == "legend")
                hasLegend = true;

            if (line[Cell('A')].ToLower() == "estimate")
                hasEstimate = true;
        }

        if (hasLegend && hasEstimate)
            return RSMeansFileType.OnlyUnitCost;

        // Mix
        var hasUnitCostTotal = false;
        var hasAssemblyCostTotal = false;
            
        foreach (var line in lines)
        {
            if (line[Cell('A')].ToLower() == "unit cost total")
                hasUnitCostTotal = true;

            if (line[Cell('A')].ToLower() == "assembly cost total")
                hasAssemblyCostTotal = true;
        }

        if (hasUnitCostTotal || hasAssemblyCostTotal)
            return RSMeansFileType.Mix;

        return RSMeansFileType.Unknow;
    }
    
    #region Mix

    private static Result<List<RSMeansUnitCostLineItem>> Get_Mix_UnitCostLineItems(List<string[]> lines)
    {
        var unitCostLineItems = new List<RSMeansUnitCostLineItem>();
        var startIndex = Get_Mix_UnitCostLine_StartIndex(lines);
        if (startIndex < 0)
            return Result<List<RSMeansUnitCostLineItem>>.Fail();

        for (var i = startIndex; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i][Cell('A')]) || lines[i][Cell('A')] == "Unit Cost Total")
                return Result<List<RSMeansUnitCostLineItem>>.OkData(unitCostLineItems);

            var ucl = new RSMeansUnitCostLineItem();
            ucl.Description = lines[i][Cell('E')] + " (RsMeans - Unit Cost)";
            ucl.Unit = lines[i][Cell('I')];
            ucl.Qty = decimal.Parse(lines[i][Cell('A')]);

            ucl.PerformanceDescription = lines[i][Cell('E')] + " (RsMeans - Unit Cost)";
            ucl.PerformanceValue = decimal.Parse(lines[i][Cell('G')]);
            ucl.PerformanceHours = 8;

            ucl.MaterialDescription = lines[i][Cell('E')] + " (RsMeans - Unit Cost - Only Material)";
            ucl.MaterialItemType = ItemTypeDefinitions.ByUnit;
            ucl.MaterialUnit = ucl.Unit;
            ucl.MaterialQty = 1;
            ucl.MaterialPrice = GetAmount(lines[i][Cell('J')]);

            ucl.EquipmentDescription = lines[i][Cell('E')] + " (RsMeans - Unit Cost - Only Equipment)";
            ucl.EquipmentItemType = ItemTypeDefinitions.ByUnit;
            ucl.EquipmentUnit = ucl.Unit;
            ucl.EquipmentQty = 1;
            ucl.EquipmentPrice = GetAmount(lines[i][Cell('L')]);

            ucl.ContractDescription = lines[i][Cell('E')] + " (RsMeans - Unit Cost - Only Labor)";
            ucl.ContractItemType = ItemTypeDefinitions.ByUnit;
            ucl.ContractUnit = ucl.Unit;
            ucl.ContractQty = 1;
            ucl.ContractPrice = GetAmount(lines[i][Cell('K')]);

            unitCostLineItems.Add(ucl);
        }

        return Result<List<RSMeansUnitCostLineItem>>.OkData(unitCostLineItems);
    }
    private static int Get_Mix_UnitCostLine_StartIndex(List<string[]> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Length < 4) 
                continue;

            if (lines[i][4] != "Unit Cost Lines") 
                continue;
                
            if (lines[i + 1].Contains("Quantity") || lines[i + 1].Contains("Description"))
                return i + 2;
            if (lines[i + 2].Contains("Quantity") || lines[i + 2].Contains("Description"))
                return i + 3;
            if (lines[i + 3].Contains("Quantity") || lines[i + 3].Contains("Description"))
                return i + 4;
        }
            
        return -1;
    }


    private static Result<List<RSMeansAssemblyCostLineItem>> Get_Mix_AssemblyCostLineItems(List<string[]> lines)
    {
        var assemblyCostLineItems = new List<RSMeansAssemblyCostLineItem>();
        var startIndex = Get_Mix_AssemblyCostLine_StartIndex(lines);
        if (startIndex < 0)
            return Result<List<RSMeansAssemblyCostLineItem>>.Fail();

        for (var i = startIndex; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i][Cell('A')]) || lines[i][Cell('A')] == "Assembly Cost Total")
                return Result<List<RSMeansAssemblyCostLineItem>>.OkData(assemblyCostLineItems);

            var acl = new RSMeansAssemblyCostLineItem();
            acl.Description = lines[i][Cell('E')] + " (RsMeans - Assembly Cost)";
            acl.Unit = lines[i][Cell('I')];
            acl.Qty = decimal.Parse(lines[i][Cell('A')]);

            acl.ContractDescription = lines[i][Cell('E')] + " (RsMeans - Assembly Cost)";
            acl.ContractItemType = ItemTypeDefinitions.ByUnit;
            acl.ContractUnit = acl.Unit;
            acl.ContractQty = 1;
            acl.ContractPrice = GetAmount(lines[i][Cell('M')]);

            assemblyCostLineItems.Add(acl);
        }

        return Result<List<RSMeansAssemblyCostLineItem>>.OkData(assemblyCostLineItems);
    }
    private static int Get_Mix_AssemblyCostLine_StartIndex(List<string[]> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Length < 4) 
                continue;

            if (lines[i][4] != "Assembly Cost Lines") 
                continue;
                
            if (lines[i + 1].Contains("Quantity") || lines[i + 1].Contains("Description"))
                return i + 2;
            if (lines[i + 2].Contains("Quantity") || lines[i + 2].Contains("Description"))
                return i + 3;
            if (lines[i + 3].Contains("Quantity") || lines[i + 3].Contains("Description"))
                return i + 4;
        }

        return -1;
    }    

    #endregion

    #region Only Unit Cost

    private static Result<List<RSMeansUnitCostLineItem>> Get_OnlyUnitCostLineItems(List<string[]> lines)
    {
        var unitCostLineItems = new List<RSMeansUnitCostLineItem>();
        var startIndex = Get_OnlyUnitCostLineItems_StartIndex(lines);
        if (startIndex < 0)
            return Result<List<RSMeansUnitCostLineItem>>.Fail();

        for (var i = startIndex; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i][Cell('A')]) || lines[i][Cell('A')].ToLower().Contains("grand total"))
                return Result<List<RSMeansUnitCostLineItem>>.OkData(unitCostLineItems);

            var ucl = new RSMeansUnitCostLineItem();
            ucl.Description = lines[i][Cell('E')] + " (RsMeans - Unit Cost)";
            ucl.Unit = lines[i][Cell('I')];
            ucl.Qty = decimal.Parse(lines[i][Cell('A')]);

            ucl.PerformanceDescription = lines[i][Cell('E')] + " (RsMeans - Unit Cost)";
            ucl.PerformanceValue = decimal.Parse(lines[i][Cell('G')]);
            ucl.PerformanceHours = 8;

            ucl.MaterialDescription = lines[i][Cell('E')] + " (RsMeans - Unit Cost - Only Material)";
            ucl.MaterialItemType = ItemTypeDefinitions.ByUnit;
            ucl.MaterialUnit = ucl.Unit;
            ucl.MaterialQty = 1;
            ucl.MaterialPrice = GetAmount(lines[i][Cell('J')]);

            ucl.EquipmentDescription = lines[i][Cell('E')] + " (RsMeans - Unit Cost - Only Equipment)";
            ucl.EquipmentItemType = ItemTypeDefinitions.ByUnit;
            ucl.EquipmentUnit = ucl.Unit;
            ucl.EquipmentQty = 1;
            ucl.EquipmentPrice = GetAmount(lines[i][Cell('L')]);

            ucl.ContractDescription = lines[i][Cell('E')] + " (RsMeans - Unit Cost - Only Labor)";
            ucl.ContractItemType = ItemTypeDefinitions.ByUnit;
            ucl.ContractUnit = ucl.Unit;
            ucl.ContractQty = 1;
            ucl.ContractPrice = GetAmount(lines[i][Cell('K')]);

            unitCostLineItems.Add(ucl);
        }

        return Result<List<RSMeansUnitCostLineItem>>.OkData(unitCostLineItems);
    }
    private static int Get_OnlyUnitCostLineItems_StartIndex(List<string[]> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Length < 3)
                continue;

            if (lines[i][Cell('A')].ToLower().Contains("quantity") &&
                lines[i][Cell('B')].ToLower().Contains("linenumber"))
                return i + 1;
        }

        return -1;
    }

    #endregion

    #region Only Assembly Cost

    private static Result<List<RSMeansAssemblyCostLineItem>> Get_OnlyAssemblyCostLineItems(List<string[]> lines)
    {
        var assemblyCostLineItems = new List<RSMeansAssemblyCostLineItem>();
        var startIndex = Get_OnlyAssemblyCostLineItems_StartIndex(lines);
        if (startIndex < 0)
            return Result<List<RSMeansAssemblyCostLineItem>>.Fail();

        for (var i = startIndex; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i][Cell('A')]))
                return Result<List<RSMeansAssemblyCostLineItem>>.OkData(assemblyCostLineItems);

            var acl = new RSMeansAssemblyCostLineItem();
            acl.Description = lines[i][Cell('E')] + " (RsMeans - Assembly Cost)";
            acl.Unit = lines[i][Cell('F')];
            acl.Qty = decimal.Parse(lines[i][Cell('A')]);

            acl.ContractDescription = lines[i][Cell('E')] + " (RsMeans - Assembly Cost)";
            acl.ContractItemType = ItemTypeDefinitions.ByUnit;
            acl.ContractUnit = acl.Unit;
            acl.ContractQty = 1;
            acl.ContractPrice = GetAmount(lines[i][Cell('I')]);

            assemblyCostLineItems.Add(acl);
        }

        return Result<List<RSMeansAssemblyCostLineItem>>.OkData(assemblyCostLineItems);
    }
    private static int Get_OnlyAssemblyCostLineItems_StartIndex(List<string[]> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Length < 3)
                continue;

            if (lines[i][Cell('B')].ToLower().Contains("assembly number"))
                return i + 1;
        }

        return -1;
    }

    #endregion

    #region Helpers

    private static int Cell(char col)
    {
        if (col == 'A')
            return 0;
        if (col == 'B')
            return 1;
        if (col == 'C')
            return 2;
        if (col == 'D')
            return 3;
        if (col == 'E')
            return 4;
        if (col == 'F')
            return 5;
        if (col == 'G')
            return 6;
        if (col == 'H')
            return 7;
        if (col == 'I')
            return 8;
        if (col == 'J')
            return 9;
        if (col == 'K')
            return 10;
        if (col == 'L')
            return 11;
        if (col == 'M')
            return 12;
        if (col == 'N')
            return 13;
        if (col == 'O')
            return 14;
        if (col == 'P')
            return 15;
        if (col == 'Q')
            return 16;
        if (col == 'R')
            return 17;

        return 18;
    }

    private static decimal GetAmount(string value)
    {
        value = value.Replace("$", "");
        value = value.Replace("£", "");
        value = value.Replace("*", "");
        value = value.Replace("Ł", "");
        value = value.Replace("€", "");
   
        value = value.TrimStart();
        value = value.TrimEnd();

        if (value.Contains('-'))
            return 0;

        return decimal.Parse(value);
    }    

    #endregion
}

public enum RSMeansFileType
{
    Unknow,
    Mix,
    OnlyAssemblyCost,
    OnlyUnitCost
}