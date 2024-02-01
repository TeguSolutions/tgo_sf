using System.Collections.ObjectModel;

namespace APU.WebApp.Services.Latency;

public class LatencyService
{
    private readonly Dictionary<Guid, LatencyClass> values;
    private readonly List<Guid> expiredValues;

    #region Lifecycle

    public LatencyService()
    {
        values = new Dictionary<Guid, LatencyClass>();
        expiredValues = new List<Guid>();
    }    

    #endregion

    public void AddValue(Guid instanceId, Guid userId, string userName, double avgLatency)
    {
        if (!values.ContainsKey(instanceId))
            values.Add(instanceId, new LatencyClass
            {
                LastUpdatedAt = DateTimeOffset.UtcNow,
                UserId = userId,
                UserName = userName,
                AvgLatency = avgLatency
            });
        else
        {
            values[instanceId].LastUpdatedAt = DateTimeOffset.UtcNow;
            values[instanceId].AvgLatency = avgLatency;
        }

        Cleanup();
    }

    private void Cleanup()
    {
        foreach (var kvp in values)
        {
            if ((DateTimeOffset.UtcNow - kvp.Value.LastUpdatedAt).TotalMinutes > 1)
                expiredValues.Add(kvp.Key);
        }

        foreach (var expiredValue in expiredValues)
        {
            values.Remove(expiredValue);
        }

        expiredValues.Clear();
    }

    public ObservableCollection<LatencyClass> GetUserLatencies()
    {
        if (values is null)
            return new ObservableCollection<LatencyClass>();

        return values.Values.OrderBy(q => q.UserName).ToObservableCollection();
    }


    public class LatencyClass
    {
        public DateTimeOffset LastUpdatedAt { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public double AvgLatency { get; set; }

        public string AvgLatencyText => AvgLatency.ToString("0") + " ms";
    }
}