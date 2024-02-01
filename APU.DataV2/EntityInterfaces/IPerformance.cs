// ReSharper disable once CheckNamespace
namespace APU.DataV2.Entities;

public interface IPerformance
{
    Guid Id { get; set; }

    string Description { get; set; }

    decimal Value { get; set; }

    decimal Hours { get; set; }
}