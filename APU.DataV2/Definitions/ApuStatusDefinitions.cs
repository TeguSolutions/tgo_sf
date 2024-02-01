using System.Collections.ObjectModel;

namespace APU.DataV2.Definitions;

public static class ApuStatusDefinitions
{
    static ApuStatusDefinitions()
    {
        Progress = new ApuStatus
        {
            Id = 1,
            Name = "Progress"
        };

        Review = new ApuStatus
        {
            Id = 2,
            Name = "Review"
        };

        Ready = new ApuStatus
        {
            Id = 3,
            Name = "Ready"
        };

        Collection = new ObservableCollection<ApuStatus>
        {
            Progress,
            Review,
            Ready
        };
    }

    public static ObservableCollection<ApuStatus> Collection { get; }

    public static ApuStatus Progress { get; }
    public static ApuStatus Review { get; }
    public static ApuStatus Ready { get; }
}