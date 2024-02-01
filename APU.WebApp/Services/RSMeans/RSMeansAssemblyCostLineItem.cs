namespace APU.WebApp.Services.RSMeans;

public class RSMeansAssemblyCostLineItem
{
    public string Description { get; set; }
    public string Unit { get; set; }
    public decimal Qty { get; set; }

    public string ContractDescription { get; set; }
    public ItemType ContractItemType { get; set; }
    public string ContractUnit { get; set; }
    public decimal ContractQty { get; set; }
    public decimal ContractPrice { get; set; }
}