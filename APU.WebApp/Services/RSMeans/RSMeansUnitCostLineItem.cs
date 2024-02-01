namespace APU.WebApp.Services.RSMeans;

public class RSMeansUnitCostLineItem
{
    public string Description { get; set; }
    public string Unit { get; set; }
    public decimal Qty { get; set; }

    public string PerformanceDescription { get; set; }
    public decimal PerformanceValue { get; set; }
    public decimal PerformanceHours { get; set; }

    public string MaterialDescription { get; set; }
    public ItemType MaterialItemType { get; set; }
    public string MaterialUnit { get; set; }
    public decimal MaterialQty { get; set; }
    public decimal MaterialPrice { get; set; }

    public string EquipmentDescription { get; set; }
    public ItemType EquipmentItemType { get; set; }
    public string EquipmentUnit { get; set; }
    public decimal EquipmentQty { get; set; }
    public decimal EquipmentPrice { get; set; }

    public string ContractDescription { get; set; }
    public ItemType ContractItemType { get; set; }
    public string ContractUnit { get; set; }
    public decimal ContractQty { get; set; }
    public decimal ContractPrice { get; set; }
}