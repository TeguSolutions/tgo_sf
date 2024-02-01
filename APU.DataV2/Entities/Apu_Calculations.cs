// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable ConstantNullCoalescingCondition

using System.Text.Json.Serialization;

namespace APU.DataV2.Entities;

public partial class Apu
{
    public Apu()
    {
        // Todo: temp, remove the init!
        ApuPerformances ??= new List<ApuPerformance>();
        ApuLabors ??= new List<ApuLabor>();
        ApuEquipments ??= new List<ApuEquipment>();
        ApuMaterials ??= new List<ApuMaterial>();
        ApuContracts ??= new List<ApuContract>();

        LaborSum = new LaborSummary(this);
        MaterialSum = new MaterialSummary(this);

        EquipmentSmallTools = new EquipmentSmallToolsItem(this);

        EquipmentSum = new EquipmentSummary(this);
        ContractSum = new ContractSummary(this);

        Sum = new Summary(this);
    }

    [JsonIgnore] [NotMapped] 
    public string GroupItemDescriptionText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Description))
                return GroupId + " - " + ItemId;
            return GroupId + " - " + ItemId + ": " + Description;
        }
    }

    #region Labor

    public class LaborSummary
    {
        #region Constructor

        private readonly Apu apu;

        public LaborSummary(Apu apu)
        {
            this.apu = apu;
        }        

        #endregion

        public decimal SubTotal { get; set; }
        public decimal SubTotalExtend { get; set; }

        public decimal SuperVision { get; set; }
        public decimal SuperVisionExtend { get; set; }

        public decimal SubTotalSuperVision { get; set; }
        public decimal SubTotalSuperVisionExtend { get; set; }

        public decimal GrossTotal { get; set; }
        public decimal GrossTotalExtend { get; set; }

        public decimal Total { get; set; }
        public decimal TotalExtend { get; set; }

        // ProRate For Estimate:
        public decimal TotalProRate { get; set; }
        public decimal TotalExtendProRate { get; set; }

        public void Calculate()
        {
            if (apu.Quantity == 0)
            {
                Reset();
                return;
            }

            SubTotalExtend = 0;
            foreach (var apuLabor in apu.ApuLabors)
            {
                if (apuLabor?.TotalExtend is not null)
                    SubTotalExtend += apuLabor.TotalExtend;
            }
            SubTotalExtend = SubTotalExtend.AsRound();

            SubTotal = SubTotalExtend / apu.Quantity;
            SubTotal = SubTotal.AsRound();


            SuperVisionExtend = SubTotalExtend * apu.SuperPercentage / 100;
            SuperVisionExtend = SuperVisionExtend.AsRound();

            SuperVision = SuperVisionExtend / apu.Quantity;
            SuperVision = SuperVision.AsRound();


            SubTotalSuperVisionExtend = SubTotalExtend + SuperVisionExtend;
            SubTotalSuperVisionExtend = SubTotalSuperVisionExtend.AsRound();

            SubTotalSuperVision = SubTotalSuperVisionExtend / apu.Quantity;
            SubTotalSuperVision = SubTotalSuperVision.AsRound();


            GrossTotalExtend = SubTotalSuperVisionExtend / (100 - apu.LaborGrossPercentage) * apu.LaborGrossPercentage;
            GrossTotalExtend = GrossTotalExtend.AsRound();

            GrossTotal = GrossTotalExtend / apu.Quantity;
            GrossTotal = GrossTotal.AsRound();


            TotalExtend = SubTotalSuperVisionExtend + GrossTotalExtend;
            TotalExtend = TotalExtend.AsRound();

            Total = TotalExtend / apu.Quantity;
            Total = Total.AsRound();
        }

        private void Reset()
        {
            SubTotal = 0;
            SubTotalExtend = 0;

            SuperVision = 0;
            SuperVisionExtend = 0;

            SubTotalSuperVision = 0;
            SubTotalSuperVisionExtend = 0;
            
            GrossTotal = 0;
            GrossTotalExtend = 0;
            
            Total = 0;
            TotalExtend = 0;
            
            TotalProRate = 0;
            TotalExtendProRate = 0;
        }
    }

    [JsonIgnore] [NotMapped] public LaborSummary LaborSum { get; }

    #endregion

    #region Material

    public class MaterialSummary
    {
        #region Constructor

        private readonly Apu apu;

        public MaterialSummary(Apu apu)
        {
            this.apu = apu;
        }        

        #endregion

        public decimal SubTotal { get; set; }
        public decimal SubTotalExtend { get; set; }

        public decimal SalesTotal { get; set; }
        public decimal SalesTotalExtend { get; set; }

        public decimal SubTotalSalesTotal { get; set; }
        public decimal SubTotalSalesTotalExtend { get; set; }

        public decimal GrossTotal { get; set; }
        public decimal GrossTotalExtend { get; set; }

        public decimal Total { get; set; }
        public decimal TotalExtend { get; set; }

        // Pro Rate For Estimate:
        public decimal TotalProRate { get; set; }
        public decimal TotalExtendProRate { get; set; }

        public void Calculate(decimal salesTax)
        {
            if (apu.Quantity == 0)
            {
                Reset();
                return;
            }

            SubTotalExtend = 0;
            foreach (var apuMaterial in apu.ApuMaterials)
            {
                if (apuMaterial?.TotalExtend is not null)
                {
                    SubTotalExtend += apuMaterial.TotalExtend;
                }
            }
            SubTotalExtend = SubTotalExtend.AsRound();

            SubTotal = SubTotalExtend / apu.Quantity;
            SubTotal = SubTotal.AsRound();


            SalesTotalExtend = SubTotalExtend * salesTax / 100;
            SalesTotalExtend = SalesTotalExtend.AsRound();

            SalesTotal = SalesTotalExtend / apu.Quantity;
            SalesTotal = SalesTotal.AsRound();


            SubTotalSalesTotalExtend = SubTotalExtend + SalesTotalExtend;
            SubTotalSalesTotalExtend = SubTotalSalesTotalExtend.AsRound();

            SubTotalSalesTotal = SubTotalSalesTotalExtend / apu.Quantity;
            SubTotalSalesTotal = SubTotalSalesTotal.AsRound();


            GrossTotalExtend = SubTotalSalesTotalExtend / (100 - apu.MaterialGrossPercentage) * apu.MaterialGrossPercentage;
            GrossTotalExtend = GrossTotalExtend.AsRound();

            GrossTotal = GrossTotalExtend / apu.Quantity;
            GrossTotal = GrossTotal.AsRound();


            TotalExtend = SubTotalSalesTotalExtend + GrossTotalExtend;
            TotalExtend = TotalExtend.AsRound();

            Total = TotalExtend / apu.Quantity;
            Total = Total.AsRound();
        }

        private void Reset()
        {
            SubTotal = 0;
            SubTotalExtend = 0;
            
            SalesTotal = 0;
            SalesTotalExtend = 0;
            
            SubTotalSalesTotal = 0;
            SubTotalSalesTotalExtend = 0;
            
            GrossTotal = 0;
            GrossTotalExtend = 0;
            
            Total = 0;
            TotalExtend = 0;
            
            TotalProRate = 0;
            TotalExtendProRate = 0;
        }
    }

    [JsonIgnore] [NotMapped] public MaterialSummary MaterialSum { get; }

    #endregion

    #region Equipment

    public class EquipmentSmallToolsItem
    {
        #region Constructor

        private readonly Apu apu;

        public EquipmentSmallToolsItem(Apu apu)
        {
            this.apu = apu;
        }        

        #endregion

        public decimal Total { get; set; }
        public decimal TotalExtend { get; set; }

        public void Calculate(decimal smallToolsPct)
        {
            if (apu.Quantity == 0)
            {
                Reset();
                return;
            }

            TotalExtend = apu.LaborSum.SubTotalSuperVisionExtend * smallToolsPct / 100;
            TotalExtend = TotalExtend.AsRound();

            Total = TotalExtend / apu.Quantity;
            Total = Total.AsRound();
        }

        private void Reset()
        {
            Total = 0;
            TotalExtend = 0;
        }
    }

    public class EquipmentSummary
    {
        #region Constructor

        private readonly Apu apu;

        public EquipmentSummary(Apu apu)
        {
            this.apu = apu;
        }        

        #endregion

        public decimal SubTotal { get; set; }
        public decimal SubTotalExtend { get; set; }

        public decimal GrossTotal { get; set; }
        public decimal GrossTotalExtend { get; set; }

        public decimal Total { get; set; }
        public decimal TotalExtend { get; set; }

        // Pro Rate For Estimate:
        public decimal TotalProRate { get; set; }
        public decimal TotalExtendProRate { get; set; }

        public void Calculate()
        {
            if (apu.Quantity == 0)
            {
                Reset();
                return;
            }

            SubTotalExtend = 0;
            foreach (var apuEquipment in apu.ApuEquipments)
            {
                if (apuEquipment?.TotalExtend is not null)
                    SubTotalExtend += apuEquipment.TotalExtend;
            }
            SubTotalExtend += apu.EquipmentSmallTools.TotalExtend;
            SubTotalExtend = SubTotalExtend.AsRound();

            // Todo: + tools_extend
            SubTotal = SubTotalExtend / apu.Quantity;
            SubTotal = SubTotal.AsRound();


            GrossTotalExtend = SubTotalExtend / (100 - apu.EquipmentGrossPercentage) * apu.EquipmentGrossPercentage;
            GrossTotalExtend = GrossTotalExtend.AsRound();

            GrossTotal = GrossTotalExtend / apu.Quantity;
            GrossTotal = GrossTotal.AsRound();

            TotalExtend = SubTotalExtend + GrossTotalExtend;
            TotalExtend = TotalExtend.AsRound();

            Total = TotalExtend / apu.Quantity;
            Total = Total.AsRound();
        }

        private void Reset()
        {
            SubTotal = 0;
            SubTotalExtend = 0;
            
            GrossTotal = 0;
            GrossTotalExtend = 0;
            
            Total = 0;
            TotalExtend = 0;
            
            TotalProRate = 0;
            TotalExtendProRate = 0;
        }
    }

    [JsonIgnore] [NotMapped] public EquipmentSmallToolsItem EquipmentSmallTools { get; }

    [JsonIgnore] [NotMapped] public EquipmentSummary EquipmentSum { get; }

    #endregion

    #region Contract

    public class ContractSummary
    {
        #region Constructor

        private readonly Apu apu;

        public ContractSummary(Apu apu)
        {
            this.apu = apu;
        }        

        #endregion

        public decimal SubTotal { get; set; }
        public decimal SubTotalExtend { get; set; }

        public decimal GrossTotal { get; set; }
        public decimal GrossTotalExtend { get; set; }

        public decimal Total { get; set; }
        public decimal TotalExtend { get; set; }

        // Pro Rate For Estimate:
        public decimal TotalProRate { get; set; }
        public decimal TotalExtendProRate { get; set; }

        public void Calculate()
        {
            if (apu.Quantity == 0)
            {
                Reset();
                return;
            }

            SubTotalExtend = 0;
            foreach (var apuContract in apu.ApuContracts)
            {
                if (apuContract?.TotalExtend is not null)
                    SubTotalExtend += apuContract.TotalExtend;
            }
            SubTotalExtend = SubTotalExtend.AsRound();

            SubTotal = SubTotalExtend / apu.Quantity;
            SubTotal = SubTotal.AsRound();


            GrossTotalExtend = SubTotalExtend / (100 - apu.SubcontractorGrossPercentage) * apu.SubcontractorGrossPercentage;
            GrossTotalExtend = GrossTotalExtend.AsRound();
            
            GrossTotal = GrossTotalExtend / apu.Quantity;
            GrossTotal = GrossTotal.AsRound();

            TotalExtend = SubTotalExtend + GrossTotalExtend;
            TotalExtend = TotalExtend.AsRound();

            Total = TotalExtend / apu.Quantity;
            Total = Total.AsRound();
        }

        private void Reset()
        {
            SubTotal = 0;
            SubTotalExtend = 0;
            
            GrossTotal = 0;
            GrossTotalExtend = 0;
            
            Total = 0;
            TotalExtend = 0;
            
            TotalProRate = 0;
            TotalExtendProRate = 0;
        }
    }

    [JsonIgnore] [NotMapped] public ContractSummary ContractSum { get; }

    #endregion

    #region Apu Summary

    public class Summary
    {
        #region Constructor

        private readonly Apu apu;

        public Summary(Apu apu)
        {
            this.apu = apu;
        }        

        #endregion

        public decimal SubTotal { get; set; }
        public decimal SubTotalExtend { get; set; }

        public decimal GrossTotalPct { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal GrossTotalExtend { get; set; }

        public decimal Total { get; set; }
        public decimal TotalExtend { get; set; }

        // Pro Rate For Estimate:
        public decimal TotalProRate { get; set; }
        public decimal TotalExtendProRate { get; set; }

        public void Calculate()
        {
            if (apu.Quantity == 0)
            {
                Reset();
                return;
            }

            SubTotalExtend = apu.LaborSum.SubTotalSuperVisionExtend + apu.MaterialSum.SubTotalSalesTotalExtend +
                             apu.EquipmentSum.SubTotalExtend + apu.ContractSum.SubTotalExtend;
            SubTotalExtend = SubTotalExtend.AsRound();

            SubTotal = SubTotalExtend / apu.Quantity;
            SubTotal = SubTotal.AsRound();


            GrossTotalExtend = apu.LaborSum.GrossTotalExtend + apu.MaterialSum.GrossTotalExtend +
                               apu.EquipmentSum.GrossTotalExtend + apu.ContractSum.GrossTotalExtend;
            GrossTotalExtend = GrossTotalExtend.AsRound();

            GrossTotal = GrossTotalExtend / apu.Quantity;
            GrossTotal = GrossTotal.AsRound();

            TotalExtend = apu.LaborSum.TotalExtend + apu.MaterialSum.TotalExtend + apu.EquipmentSum.TotalExtend +
                          apu.ContractSum.TotalExtend;
            TotalExtend = TotalExtend.AsRound();

            Total = TotalExtend / apu.Quantity;
            Total = Total.AsRound();

            if (Total == 0)
                GrossTotalPct = 0;
            else
                GrossTotalPct = GrossTotal / Total;
            GrossTotalPct *= 100;
            GrossTotalPct = GrossTotalPct.AsRound();
        }

        private void Reset()
        {
            SubTotal = 0;
            SubTotalExtend = 0;
            
            GrossTotalPct = 0;
            GrossTotal = 0;
            GrossTotalExtend = 0;
            
            Total = 0;
            TotalExtend = 0;
            
            TotalProRate = 0;
            TotalExtendProRate = 0;
        }
    }

    [JsonIgnore] [NotMapped] public Summary Sum { get; }

    #endregion

    public void CalculateAll(DefaultValue defaultValue, Project project)
    {
        if (defaultValue is null)
            return;
        if (project is null)
            return;

        var performance = ApuPerformances.FirstOrDefault();

        // Todo: review just in case...
        if (!IsLineItem && !IsGroup999Item && !IsGroup1000Item)
            return;

        performance?.Calculate(Quantity);

        if (ApuLabors is not null)
        {
            foreach (var apuLabor in ApuLabors)
            {
                apuLabor?.CalculateCost(defaultValue.TopFica, defaultValue.Fica, defaultValue.FutaSuta, defaultValue.WorkersComp);
                apuLabor?.Calculate(Quantity, performance);
            }
        }
        LaborSum.Calculate();

        if (ApuMaterials is not null)
        {
            foreach (var apuMaterial in ApuMaterials)
                apuMaterial?.Calculate(Quantity);
        }
        MaterialSum.Calculate(project.SalesTax);

        // Calculate before Equipments!
        EquipmentSmallTools.Calculate(project.Tools);

        if (ApuEquipments is not null)
        {
            foreach (var apuEquipment in ApuEquipments)
                apuEquipment?.Calculate(Quantity);
        }
        EquipmentSum.Calculate();

        if (ApuContracts is not null)
        {
            foreach (var apuContract in ApuContracts)
                apuContract?.Calculate(Quantity);
        }
        ContractSum.Calculate();

        Sum.Calculate();
    }

    public void OrderItems()
    {
        ApuLabors = ApuLabors.OrderBy(q => q.CreatedAt).ThenBy(q => q.Id).ToList();
        ApuMaterials = ApuMaterials.OrderBy(q => q.CreatedAt).ThenBy(q => q.Id).ToList();
        ApuEquipments = ApuEquipments.OrderBy(q => q.CreatedAt).ThenBy(q => q.Id).ToList();
        ApuContracts = ApuContracts.OrderBy(q => q.CreatedAt).ThenBy(q => q.Id).ToList();
    }

    #region Type Definitions

    // Separators (/Descriptions)

    /// <summary>
    /// XXX-(-2) Record will place a white lane before any top sub total in a group that could have a Description
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroupMainHeader => ItemId == -2;

    /// <summary>
    /// XXX-1002 Record will place a white lane after bottom sub-total that could have a Description
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroupMainFooter => ItemId == 1002;

    [JsonIgnore] [NotMapped] public bool IsGroupBaseSubtotal => GroupId < 999 && ItemId is -1 or 1001;

    /// <summary>
    /// XXX-(-1) Record will place a white lane before any group that could have a Description and will have a Group Sub-Totals
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroupSubTotalHeader => /*GroupId < 999 &&*/ ItemId == -1;

    /// <summary>
    /// XXX-1001 Record will place a white lane after a group that could have a Description and will have a Sub-Totals
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroupSubTotalFooter => /*GroupId < 999 &&*/ ItemId == 1001;


    /// <summary>
    /// XXX-00 Record will place a wWhite lane before any group that could have a Description only
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroupZeroHeader => ItemId == 0;

    /// <summary>
    /// XXX-1000 Record will place a white lane after a group that could have a Description only
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroupZeroFooter => ItemId == 1000;





    // Line Items

    [JsonIgnore] [NotMapped] public bool IsAnyLineItem => GroupId <= 1000 && ItemId is >= 1 and <= 999;

    /// <summary>
    /// XXX-001 to XXX-999 These Records will be items records with all their data
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsLineItem => GroupId < 999 && ItemId is >= 1 and <= 999;


    /// <summary>
    /// 998-1003 This Line will have total for the project before allowance and Pro_Rate
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroup998Item1003 => GroupId == 998 && ItemId == 1003;

    // 999
    /// <summary>
    /// 999-XXX All the records in this group will be allowances and wont have APU
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroup999Item => GroupId == 999 && ItemId is >= 1 and <= 999;

    [JsonIgnore] [NotMapped] public bool IsGroup999SubTotalHeader => GroupId == 999 && ItemId == -1;
    [JsonIgnore] [NotMapped] public bool IsGroup999SubTotalFooter => GroupId == 999 && ItemId == 1001;

    /// <summary>
    /// 999-1003 This Line will have total for the project, including allowance
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroup999Item1003 => GroupId == 999 && ItemId == 1003;

    // 1000
    /// <summary>
    /// GroupId = 1000 ||  1 &lt;= ItemId &lt;= 999 -##- 1000-001 to 999 These records on this group will be prorated between all recors for the groups 001 to 998
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroup1000Item => GroupId == 1000 && ItemId is >= 1 and <= 999;

    [JsonIgnore] [NotMapped] public bool IsGroup1000SubTotalHeader => GroupId == 1000 && ItemId == -1;
    [JsonIgnore] [NotMapped] public bool IsGroup1000SubTotalFooter => GroupId == 1000 && ItemId == 1001;

    // Totals..
    /// <summary>
    /// 1000-1003 This Line will have Grand Total for the project, including allowance and Pro_Rate but NOT Payment and performance.
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroup1000Item1003 => GroupId == 1000 && ItemId == 1003;

    /// <summary>
    /// 1000-1004 This Line will have Payment and Performance Bond for the project
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroup1000Item1004 => GroupId == 1000 && ItemId == 1004;

    /// <summary>
    /// 1000-1005 This Line will have Gross Profit for the project
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroup1000Item1005 => GroupId == 1000 && ItemId == 1005;

    /// <summary>
    /// 1000-1006 This Line will have Grand Total for the project, including Payment and Performance Bond
    /// </summary>
    [JsonIgnore] [NotMapped] public bool IsGroup1000Item1006 => GroupId == 1000 && ItemId == 1006;


    [JsonIgnore] [NotMapped] public bool DisplayItemText => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroupSubTotalHeader || IsGroupSubTotalFooter || IsGroup998Item1003 || IsGroup999Item1003;
    [JsonIgnore] [NotMapped] public bool DisplayCode => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayDescription => true;
    [JsonIgnore] [NotMapped] public bool DisplayUnit => IsLineItem || IsGroup999Item || IsGroup1000Item;

    [JsonIgnore] [NotMapped] public bool DisplayQuantity => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroupSubTotalHeader || IsGroupSubTotalFooter;

    // 01
    [JsonIgnore] [NotMapped] public bool DisplayLaborSumTotal => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayLaborSumTotalExtend => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplayMaterialSumTotal => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayMaterialSumTotalExtend => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplayEquipmentSumTotal => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayEquipmentSumTotalExtend => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplayContractSumTotal => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayContractSumTotalExtend => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplaySumTotal => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup1000Item1004;
    [JsonIgnore] [NotMapped] public bool DisplaySumTotalExtend => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter ||
                                                                  IsGroup1000Item1003 || IsGroup1000Item1004 || IsGroup1000Item1006;

    // 02
    [JsonIgnore] [NotMapped] public bool DisplayLaborSumTotalProRate => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayLaborSumTotalExtendProRate => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplayMaterialSumTotalProRate => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayMaterialSumTotalExtendProRate => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplayEquipmentSumTotalProRate => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayEquipmentSumTotalExtendProRate => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplayContractSumTotalProRate => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayContractSumTotalExtendProRate => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplaySumTotalProRate => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup1000Item1004;
    [JsonIgnore] [NotMapped] public bool DisplaySumTotalExtendProRate => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || 
                                                                         IsGroupSubTotalHeader || IsGroupSubTotalFooter ||
                                                                         IsGroup1000Item1003 || IsGroup1000Item1004 || IsGroup1000Item1006;

    // 03
    [JsonIgnore] [NotMapped] public bool DisplayLaborSumSubTotalSuperVision => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayLaborSumSubTotalSuperVisionExtend => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplayMaterialSumSubTotalSalesTotal => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayMaterialSumSubTotalSalesTotalExtend => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplayEquipmentSumSubTotal => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayEquipmentSumSubTotalExtend => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplayContractSumSubTotal => IsLineItem || IsGroup999Item || IsGroup1000Item;
    [JsonIgnore] [NotMapped] public bool DisplayContractSumSubTotalExtend => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || IsGroupSubTotalHeader || IsGroupSubTotalFooter;
    [JsonIgnore] [NotMapped] public bool DisplaySumSubTotal => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroupSubTotalHeader || IsGroupSubTotalFooter || IsGroup1000Item1004;
    [JsonIgnore] [NotMapped] public bool DisplaySumSubTotalExtend => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || 
                                                                     IsGroupSubTotalHeader || IsGroupSubTotalFooter || 
                                                                     IsGroup1000Item1003 || IsGroup1000Item1004 || IsGroup1000Item1005 || IsGroup1000Item1006;
    [JsonIgnore] [NotMapped] public bool DisplaySumGrossTotalPct => IsLineItem || IsGroup999Item || IsGroup1000Item || IsGroup998Item1003 || IsGroup999Item1003 || 
                                                                    IsGroupSubTotalHeader || IsGroupSubTotalFooter ||
                                                                    IsGroup1000Item1003 || IsGroup1000Item1005 || IsGroup1000Item1006;

    [JsonIgnore] [NotMapped] public bool DisplayAsBold => IsGroupMainHeader || IsGroupSubTotalHeader || IsGroupZeroHeader ||
                                                          IsGroupMainFooter || IsGroupSubTotalFooter || IsGroupZeroFooter ||
                                                          IsGroup998Item1003 || IsGroup999Item1003 ||
                                                          IsGroup1000Item1003 || IsGroup1000Item1004 || IsGroup1000Item1005 || IsGroup1000Item1006;

    //[NotMapped]
    //public bool IsLineItem => IsGroupItem || IsGroup999Item || IsGroup1000Item;


    //[NotMapped]
    //public decimal Quantity => Qty /*is null or < 1 ? 1 : Qty.Value*/;

    #endregion

    [JsonIgnore] [NotMapped] public ApuPerformance Performance => ApuPerformances.FirstOrDefault();
}