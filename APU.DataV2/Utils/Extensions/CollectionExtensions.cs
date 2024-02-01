using System.Collections.ObjectModel;

namespace APU.DataV2.Utils.Extensions;

public static class CollectionExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
            return null;

        var collection = new ObservableCollection<T>();
        foreach (var i in enumerable)
            collection.Add(i);
        return collection;
    }
}