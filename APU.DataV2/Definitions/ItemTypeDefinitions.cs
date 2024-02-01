using System.Collections.ObjectModel;
using APU.DataV2.Entities;

namespace APU.DataV2.Definitions;

public static class ItemTypeDefinitions
{
    static ItemTypeDefinitions()
    {
        Total = new ItemType
        {
            Id = 1,
            Name = "Total"
        };

        ByUnit = new ItemType
        {
            Id = 2,
            Name = "By Unit"
        };

        Collection = new ObservableCollection<ItemType>
        {
            Total,
            ByUnit
        };
    }

    public static ObservableCollection<ItemType> Collection { get; }

    public static ItemType Total { get; }
    public static ItemType ByUnit { get; }
}