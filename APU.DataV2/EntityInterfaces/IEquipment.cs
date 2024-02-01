// ReSharper disable once CheckNamespace
namespace APU.DataV2.Entities;

public interface IEquipment
{
    Guid Id { get; set; }

    string Description { get; set; }

    string Unit { get; set; }

    decimal Quantity { get; set; }

    decimal Price { get; set; }

    string Vendor { get; set; }

    string Phone { get; set; }

    string Link { get; set; }
}