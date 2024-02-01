using System.Collections.ObjectModel;
using APU.DataV2.Entities;

namespace APU.DataV2.Definitions;

public static class LaborWorkTypeDefinitions
{
    static LaborWorkTypeDefinitions()
    {
        Hours = new LaborWorkType
        {
            Id = 1,
            Name = "Hours"
        };

        Workers = new LaborWorkType
        {
            Id = 2,
            Name = "Workers"
        };

        Collection = new ObservableCollection<LaborWorkType>
        {
            Hours,
            Workers
        };
    }

    public static ObservableCollection<LaborWorkType> Collection { get; }

    public static LaborWorkType Hours { get; }
    public static LaborWorkType Workers { get; }
}