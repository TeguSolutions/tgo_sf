using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using APU.DataV2.Utils.Helpers;

namespace APU.DataV2.Entities;

public partial class Project
{
    #region Calculation

    //public void CalculateBaseItems()
    //{
    //    // Todo: implament and use it on Apu page!!
    //}

    public Result Calculate(DefaultValue defaultValue)
    {
        if (defaultValue is null)
            return Result.Fail("Project calculation failed: Default Value is null!");

        try
        {
            #region Pre collected Items

            // Pre collect the items and groups
            var groupBaseItems = Apus.Where(q => q.IsLineItem).ToList();
            var groupBaseSubTotals = Apus.Where(q => q.IsGroupBaseSubtotal);

            var group998Item1003s = Apus.Where(q => q.IsGroup998Item1003);

            var group999AllowanceItems = Apus.Where(q => q.IsGroup999Item).ToList();
            var group999AllowanceSubTotals = Apus.Where(q => q.IsGroup999SubTotalHeader || q.IsGroup999SubTotalFooter);

            var group999Item1003s = Apus.Where(q => q.IsGroup999Item1003);

            // Group 1000 Items
            var group1000ProRateItems = Apus.Where(q => q.IsGroup1000Item).ToList();
            var group1000ProRateSubTotals = Apus.Where(q => q.IsGroup1000SubTotalHeader || q.IsGroup1000SubTotalFooter);

            // Project Sums (Group 1000)
            var group1000Item1003_projectgrandtotals = Apus.Where(q => q.IsGroup1000Item1003).ToList();
            var group1000Item1004_paymentandbonds = Apus.Where(q => q.IsGroup1000Item1004).ToList();
            var group1000Item1005_grossprofits = Apus.Where(q => q.IsGroup1000Item1005);
            var group1000Item1006_grandtotals = Apus.Where(q => q.IsGroup1000Item1006);

            // Item groups
            var groupBaseAnd999Items = Apus.Where(q => q.IsLineItem || q.IsGroup999Item).ToList();
            var allItems = Apus.Where(q => q.IsLineItem || q.IsGroup999Item || q.IsGroup1000Item).ToList();

            #endregion

            // Calculation ORDER is important!!

            // Step 1 - Group (Base Apus) Items
            foreach (var groupItem in groupBaseItems)
                groupItem.CalculateAll(defaultValue, this);

            // Step 2 - Group 999 (Allowance) Items
            foreach (var group999Item in group999AllowanceItems)
            {
                group999Item.CalculateAll(defaultValue, this);
                AssignAllowanceItemProRatePrices(group999Item);
            }

            // Step 3 - Group 999 (Allowance) SubTotals
            foreach (var group999AllowanceSubTotal in group999AllowanceSubTotals)
                CalculateGroupSubTotal(group999AllowanceSubTotal, group999AllowanceItems);

            // Step 4 - Group 1000 (Pro Rate) Items
            foreach (var group1000ProRateItem in group1000ProRateItems)
                group1000ProRateItem.CalculateAll(defaultValue, this);

            // Step 5 - Group 1000 (Pro Rate) SubTotals
            foreach (var group1000ProRateSubTotal in group1000ProRateSubTotals)
                CalculateGroupSubTotal(group1000ProRateSubTotal, group1000ProRateItems);
            
            // Step 6 - Group (Base Apu) Items - Pro Rate Prices [After Group 1000 Pro Rate]
            foreach (var groupBaseItem in groupBaseItems)
                CalculateGroupItemProratePrices(groupBaseItem, group1000ProRateItems, groupBaseItems);
            
            // Step 7 - Group (Base Items) SubTotals [After Group Base Pro Rate]
            foreach (var groupBaseSubTotal in groupBaseSubTotals)
                CalculateGroupSubTotal(groupBaseSubTotal, groupBaseItems.Where(q => q.GroupId == groupBaseSubTotal.GroupId).ToList());

            // Step 8 - Group 998 Item 1003 (Total of SubTotals)
            foreach (var group998Item1003 in group998Item1003s)
                CalculateGroupSubTotal(group998Item1003, groupBaseItems);

            // Step 9 - Group 999 Item 1003 (Total after Group 999)
            foreach (var group999Item1003 in group999Item1003s)
                CalculateGroup999Item1003_GroupTotal(group999Item1003, groupBaseAnd999Items, allItems);

            // Sums

            // Step 10 - Group 1000 Item 1003 (Project Grand Total)
            foreach (var projectgrandtotal in group1000Item1003_projectgrandtotals)
                CalculateGroup1000Item1003_ProjectGrandTotal(projectgrandtotal, allItems);
            
            // Step 11 - Group 1000 Item 1004 (Payment & Performance Bond)
            // Only use the bond value if there is 1000_1004 in the project!
            decimal bond = 0;
            foreach (var paymentandbond in group1000Item1004_paymentandbonds)
            {
                CalculateGroup1000Item1004_PaymentAndPerformanceBond(paymentandbond, Bond, allItems);
                bond = Bond;
            }

            // Step 12 - Group 1000 Item 1005 (Gross Profit)
            foreach (var grossprofit in group1000Item1005_grossprofits)
                CalculateGroup1000Item1005_GrossProfit(grossprofit, bond, allItems);

            // Step 13 - Group 1000 Item 1006 (Grand Total)
            foreach (var grandTotal in group1000Item1006_grandtotals)
                CalculateGroup1000Item1006_ProjectGrandTotal(grandTotal, bond, allItems, group1000Item1004_paymentandbonds);

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail("Project calculation failed: " + e.Message);
        }
    }

    private static void AssignAllowanceItemProRatePrices(Apu apu)
    {
        apu.LaborSum.TotalExtendProRate = apu.LaborSum.TotalExtend;
        apu.LaborSum.TotalProRate = apu.LaborSum.Total;

        apu.MaterialSum.TotalExtendProRate = apu.MaterialSum.TotalExtend;
        apu.MaterialSum.TotalProRate = apu.MaterialSum.Total;

        apu.EquipmentSum.TotalExtendProRate = apu.EquipmentSum.TotalExtend;
        apu.EquipmentSum.TotalProRate = apu.EquipmentSum.Total;

        apu.ContractSum.TotalExtendProRate = apu.ContractSum.TotalExtend;
        apu.ContractSum.TotalProRate = apu.ContractSum.Total;

        apu.Sum.TotalExtendProRate = apu.Sum.TotalExtend;
        apu.Sum.TotalProRate = apu.Sum.Total;
    }

    private static void CalculateGroupItemProratePrices(Apu apu, IReadOnlyCollection<Apu> prorateItems, IReadOnlyCollection<Apu> allGroupItems)
    {
        var allGroupSum = allGroupItems.Sum(q => q.Sum.TotalExtend);
        var prorateSum = prorateItems.Sum(q => q.Sum.TotalExtend);

        if (allGroupSum is not 0)
        {
            apu.LaborSum.TotalExtendProRate = (allGroupSum + prorateSum) * apu.LaborSum.TotalExtend / allGroupSum;
            apu.MaterialSum.TotalExtendProRate = (allGroupSum + prorateSum) * apu.MaterialSum.TotalExtend / allGroupSum;
            apu.EquipmentSum.TotalExtendProRate = (allGroupSum + prorateSum) * apu.EquipmentSum.TotalExtend / allGroupSum;
            apu.ContractSum.TotalExtendProRate = (allGroupSum + prorateSum) * apu.ContractSum.TotalExtend / allGroupSum;
 
            apu.Sum.TotalExtendProRate = (allGroupSum + prorateSum) * apu.Sum.TotalExtend / allGroupSum;
        }
        else
        {
            apu.LaborSum.TotalExtendProRate = 0;
            apu.MaterialSum.TotalExtendProRate = 0;
            apu.EquipmentSum.TotalExtendProRate = 0;
            apu.ContractSum.TotalExtendProRate = 0;
            
            apu.Sum.TotalExtendProRate = 0;
        }

        if (apu.Quantity is not 0)
        {
            apu.LaborSum.TotalExtendProRate = apu.LaborSum.TotalExtendProRate.AsRound();
            apu.LaborSum.TotalProRate = apu.LaborSum.TotalExtendProRate / apu.Quantity;
            apu.LaborSum.TotalProRate = apu.LaborSum.TotalProRate.AsRound();

            apu.MaterialSum.TotalExtendProRate = apu.MaterialSum.TotalExtendProRate.AsRound();
            apu.MaterialSum.TotalProRate = apu.MaterialSum.TotalExtendProRate / apu.Quantity;
            apu.MaterialSum.TotalProRate = apu.MaterialSum.TotalProRate.AsRound();

            apu.EquipmentSum.TotalExtendProRate = apu.EquipmentSum.TotalExtendProRate.AsRound();
            apu.EquipmentSum.TotalProRate = apu.EquipmentSum.TotalExtendProRate / apu.Quantity;
            apu.EquipmentSum.TotalProRate = apu.EquipmentSum.TotalProRate.AsRound();

            apu.ContractSum.TotalExtendProRate = apu.ContractSum.TotalExtendProRate.AsRound();
            apu.ContractSum.TotalProRate = apu.ContractSum.TotalExtendProRate / apu.Quantity;
            apu.ContractSum.TotalProRate = apu.ContractSum.TotalProRate.AsRound();

            apu.Sum.TotalExtendProRate = apu.Sum.TotalExtendProRate.AsRound();

            apu.Sum.TotalProRate = apu.Sum.TotalExtendProRate / apu.Quantity;
            apu.Sum.TotalProRate = apu.Sum.TotalProRate.AsRound();
        }
        else
        {
            apu.LaborSum.TotalExtendProRate = 0;
            apu.LaborSum.TotalProRate = 0;
            apu.LaborSum.TotalProRate = 0;

            apu.MaterialSum.TotalExtendProRate = 0;
            apu.MaterialSum.TotalProRate = 0;
            apu.MaterialSum.TotalProRate = 0;

            apu.EquipmentSum.TotalExtendProRate = 0;
            apu.EquipmentSum.TotalProRate = 0;
            apu.EquipmentSum.TotalProRate = 0;

            apu.ContractSum.TotalExtendProRate = 0;
            apu.ContractSum.TotalProRate = 0;
            apu.ContractSum.TotalProRate = 0;

            apu.Sum.TotalExtendProRate = 0;

            apu.Sum.TotalProRate = 0;
            apu.Sum.TotalProRate = 0;
        }
    }

    private static void CalculateGroupSubTotal(Apu apu, IReadOnlyCollection<Apu> items)
    {
        // 01
        apu.LaborSum.TotalExtend = items.Sum(item => item.LaborSum.TotalExtend).AsRound();
        apu.MaterialSum.TotalExtend = items.Sum(item => item.MaterialSum.TotalExtend).AsRound();
        apu.EquipmentSum.TotalExtend = items.Sum(item => item.EquipmentSum.TotalExtend).AsRound();
        apu.ContractSum.TotalExtend = items.Sum(item => item.ContractSum.TotalExtend).AsRound();
        apu.Sum.TotalExtend = items.Sum(item => item.Sum.TotalExtend).AsRound();

        // 02
        apu.LaborSum.TotalExtendProRate = items.Sum(item => item.LaborSum.TotalExtendProRate).AsRound();
        apu.MaterialSum.TotalExtendProRate = items.Sum(item => item.MaterialSum.TotalExtendProRate).AsRound();
        apu.EquipmentSum.TotalExtendProRate = items.Sum(item => item.EquipmentSum.TotalExtendProRate).AsRound();
        apu.ContractSum.TotalExtendProRate = items.Sum(item => item.ContractSum.TotalExtendProRate).AsRound();
        apu.Sum.TotalExtendProRate = items.Sum(item => item.Sum.TotalExtendProRate).AsRound();

        // 03
        apu.LaborSum.SubTotalSuperVisionExtend = items.Sum(item => item.LaborSum.SubTotalSuperVisionExtend).AsRound();
        apu.MaterialSum.SubTotalSalesTotalExtend = items.Sum(item => item.MaterialSum.SubTotalSalesTotalExtend).AsRound();
        apu.EquipmentSum.SubTotalExtend = items.Sum(item => item.EquipmentSum.SubTotalExtend).AsRound();
        apu.ContractSum.SubTotalExtend = items.Sum(item => item.ContractSum.SubTotalExtend).AsRound();
        apu.Sum.SubTotalExtend = items.Sum(item => item.Sum.SubTotalExtend).AsRound();

        // Gross Margin Pct
        apu.Sum.GrossTotalPct = Calculate_GrossMarginPct(items);
    }

    private static void CalculateGroup999Item1003_GroupTotal(Apu apu, IReadOnlyCollection<Apu> items, IReadOnlyCollection<Apu> allGroupItems)
    {
        // 01
        apu.LaborSum.TotalExtend = items.Sum(item => item.LaborSum.TotalExtend).AsRound();
        apu.MaterialSum.TotalExtend = items.Sum(item => item.MaterialSum.TotalExtend).AsRound();
        apu.EquipmentSum.TotalExtend = items.Sum(item => item.EquipmentSum.TotalExtend).AsRound();
        apu.ContractSum.TotalExtend = items.Sum(item => item.ContractSum.TotalExtend).AsRound();
        apu.Sum.TotalExtend = items.Sum(item => item.Sum.TotalExtend).AsRound();

        // 02
        apu.LaborSum.TotalExtendProRate = items.Sum(item => item.LaborSum.TotalExtendProRate).AsRound();
        apu.MaterialSum.TotalExtendProRate = items.Sum(item => item.MaterialSum.TotalExtendProRate).AsRound();
        apu.EquipmentSum.TotalExtendProRate = items.Sum(item => item.EquipmentSum.TotalExtendProRate).AsRound();
        apu.ContractSum.TotalExtendProRate = items.Sum(item => item.ContractSum.TotalExtendProRate).AsRound();
        apu.Sum.TotalExtendProRate = allGroupItems.Sum(item => item.Sum.TotalExtend).AsRound();

        // 03
        apu.LaborSum.SubTotalSuperVisionExtend = items.Sum(item => item.LaborSum.SubTotalSuperVisionExtend).AsRound();
        apu.MaterialSum.SubTotalSalesTotalExtend = items.Sum(item => item.MaterialSum.SubTotalSalesTotalExtend).AsRound();
        apu.EquipmentSum.SubTotalExtend = items.Sum(item => item.EquipmentSum.SubTotalExtend).AsRound();
        apu.ContractSum.SubTotalExtend = items.Sum(item => item.ContractSum.SubTotalExtend).AsRound();
        apu.Sum.SubTotalExtend = items.Sum(item => item.Sum.SubTotalExtend).AsRound();

        // Gross Margin Pct
        apu.Sum.GrossTotalPct = Calculate_GrossMarginPct(items);
    }

    private static void CalculateGroup1000Item1003_ProjectGrandTotal(Apu apu, IReadOnlyCollection<Apu> items)
    {
        // 01
        apu.LaborSum.TotalExtend = items.Sum(item => item.LaborSum.TotalExtend).AsRound();
        apu.MaterialSum.TotalExtend = items.Sum(item => item.MaterialSum.TotalExtend).AsRound();
        apu.EquipmentSum.TotalExtend = items.Sum(item => item.EquipmentSum.TotalExtend).AsRound();
        apu.ContractSum.TotalExtend = items.Sum(item => item.ContractSum.TotalExtend).AsRound();
        apu.Sum.TotalExtend = items.Sum(item => item.Sum.TotalExtend).AsRound();

        // 02
        apu.LaborSum.TotalExtendProRate = items.Sum(item => item.LaborSum.TotalExtendProRate).AsRound();
        apu.MaterialSum.TotalExtendProRate = items.Sum(item => item.MaterialSum.TotalExtendProRate).AsRound();
        apu.EquipmentSum.TotalExtendProRate = items.Sum(item => item.EquipmentSum.TotalExtendProRate).AsRound();
        apu.ContractSum.TotalExtendProRate = items.Sum(item => item.ContractSum.TotalExtendProRate).AsRound();
        apu.Sum.TotalExtendProRate = items.Sum(item => item.Sum.TotalExtend).AsRound();

        // 03
        apu.LaborSum.SubTotalSuperVisionExtend = items.Sum(item => item.LaborSum.SubTotalSuperVisionExtend).AsRound();
        apu.MaterialSum.SubTotalSalesTotalExtend = items.Sum(item => item.MaterialSum.SubTotalSalesTotalExtend).AsRound();
        apu.EquipmentSum.SubTotalExtend = items.Sum(item => item.EquipmentSum.SubTotalExtend).AsRound();
        apu.ContractSum.SubTotalExtend = items.Sum(item => item.ContractSum.SubTotalExtend).AsRound();
        apu.Sum.SubTotalExtend = items.Sum(item => item.Sum.SubTotalExtend).AsRound();

        var allItemsTotalPrice = items.Sum(q => q.Sum.TotalExtend);
        var allItemsSubTotalPrice = items.Sum(q => q.Sum.SubTotalExtend);
        apu.Sum.GrossTotalPct = ((allItemsTotalPrice - allItemsSubTotalPrice) / allItemsTotalPrice) * 100;
    }

    private static void CalculateGroup1000Item1004_PaymentAndPerformanceBond(Apu apu, decimal bondPct, IReadOnlyCollection<Apu> items)
    {
        // 01
        apu.Sum.TotalExtend = (items.Sum(item => item.Sum.TotalExtend).AsRound() / (100 - bondPct) * bondPct).AsRound();
        // 02
        apu.Sum.TotalExtendProRate = (items.Sum(item => item.Sum.TotalExtendProRate).AsRound() / (100 - bondPct) * bondPct).AsRound();
        // 03 
        // Todo: forced
        apu.Sum.SubTotalExtend = (items.Sum(item => item.Sum.TotalExtend).AsRound() / (100 - bondPct) * bondPct).AsRound();
    }

    private static void CalculateGroup1000Item1005_GrossProfit(Apu apu, decimal bondPct, IReadOnlyCollection<Apu> items)
    {
        var bondAmount = (items.Sum(item => item.Sum.TotalExtend).AsRound() / (100 - bondPct) * bondPct).AsRound();

        // Final - Total Price col
        var item1000_1006_TotalExtend = (items.Sum(item => item.Sum.TotalExtend).AsRound() / ((100 - bondPct) / 100)).AsRound();

        var itemSubTotalPrice =items.Sum(item => item.Sum.SubTotalExtend).AsRound();
        
        // Gross Total value
        // Formula Gross Profit = GrandTotal - Bond - CostGrandTotal
        // (Sum of all line items 1-999 Total Price) - (Bond amount) - (sum of of all line items 1-999 Cost Sub-Total)
        apu.Sum.SubTotalExtend = item1000_1006_TotalExtend - bondAmount - itemSubTotalPrice;

        // Gross Margin Pct
        // Percentage Gross profit = Formula Gross Profit / GrandTotal

        if (item1000_1006_TotalExtend == 0)
            apu.Sum.GrossTotalPct = 0;
        else
            apu.Sum.GrossTotalPct = apu.Sum.SubTotalExtend / item1000_1006_TotalExtend;

        apu.Sum.GrossTotalPct *= 100;
        //apu.Sum.GrossTotalPct = apu.Sum.GrossTotalPct.AsRound();
    }

    private static void CalculateGroup1000Item1006_ProjectGrandTotal(Apu apu, decimal bondPct, IReadOnlyCollection<Apu> items, IReadOnlyCollection<Apu> group1000Item1004_paymentandbonds)
    {
        var bondAmount = (items.Sum(item => item.Sum.TotalExtend).AsRound() / (100 - bondPct) * bondPct).AsRound();

        // Final - Total Price col
        var itemSubTotalPrice = items.Sum(item => item.Sum.SubTotalExtend).AsRound();

        // 01
        apu.Sum.TotalExtend = (items.Sum(item => item.Sum.TotalExtend).AsRound() / ((100 - bondPct) / 100)).AsRound();
        // 02
        apu.Sum.TotalExtendProRate = (items.Sum(item => item.Sum.TotalExtendProRate).AsRound() / ((100 - bondPct) / 100)).AsRound();
        // 03
        //apu.Sum.SubTotalExtend = (items.Sum(item => item.Sum.SubTotalExtend).AsRound() / ((100 - bondPct) / 100)).AsRound();
        //apu.Sum.SubTotalExtend = item1000_1006_TotalExtend - bondAmount - itemSubTotalPrice;
        // the sum of all line items (0 to 999) plus the Bond (1000-1005)
        apu.Sum.SubTotalExtend = itemSubTotalPrice + bondAmount;

        // Gross Margin Pct
        // Percentage Gross profit = Formula Gross Profit / GrandTotal
        var allItemsTotalPrice = items.Sum(q => q.Sum.TotalExtend);
        var bondItemsTotalPrice = group1000Item1004_paymentandbonds.Sum(q => q.Sum.TotalExtend);
        var allItemsSubTotalPrice = items.Sum(q => q.Sum.SubTotalExtend);

        if (allItemsTotalPrice + bondItemsTotalPrice == 0)
            apu.Sum.GrossTotalPct = 0;
        else
            apu.Sum.GrossTotalPct = (allItemsTotalPrice + bondItemsTotalPrice - (allItemsSubTotalPrice + bondItemsTotalPrice)) 
                                    / (allItemsTotalPrice + bondItemsTotalPrice);
        apu.Sum.GrossTotalPct *= 100;
    }

    #endregion
    #region Calculation - Helper

    private static decimal Calculate_GrossMarginPct(IReadOnlyCollection<Apu> items)
    {
        // Gross Margin
        var sumGrossTotal = items.Sum(item => item.Sum.GrossTotal).AsRound();
        var sumTotal = items.Sum(item => item.Sum.Total).AsRound();

        // Gross Margin Pct
        if (sumTotal == 0)
            return 0;

        var pct = sumGrossTotal / sumTotal;
        pct *= 100;
        //pct = pct.AsRound();

        return pct;
    }

    #endregion

    #region Filtered Apus

    [JsonIgnore]
    [NotMapped]
    public ObservableCollection<Apu> FilteredApus { get; set; } = new();

    public void GetFilteredApus(string filterText)
    {
        filterText ??= "";

        FilteredApus = Apus
            .If(!string.IsNullOrWhiteSpace(filterText), q =>
                q.Where(o => 
                    !string.IsNullOrWhiteSpace(o.Description) &&
                    (o.Description.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.Description, filterText)))
            )
            .OrderBy(p => p.GroupId)
            .ThenBy(q => q.ItemId)
            .ToObservableCollection();
    }

    #endregion

    #region Helpers

    public int? GetNextAvailableLineItemGroup(int i = 1)
    {
	    if (Apus is null)
		    return null;

	    for (; i < 1000; i++)
	    {
		    if (Apus.FirstOrDefault(q => q.GroupId == i) is null)
			    return i;
	    }

	    return null;
    }

    #endregion
}