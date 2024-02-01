using APU.DataV2.Utils.HelperClasses;

namespace APU.WebApp.Utils.Extensions;

public static class ListExtensions
{
	public static List<GuidInt> ApuInsert(this List<Apu> list, int itemId, Apu item)
	{
		if (list is null)
			return null;

		// 0 1 2 - index
		// 1 2 3 - list.count
		list = list.OrderBy(q => q.ItemId).ToList();

		if (itemId < 1)
			list.Insert(0, item);
		else if (itemId > list.Count)
			list.Add(item);
		else
			list.Insert(itemId - 1, item);

		for (var o = 0; o < list.Count; o++)
			list[o].ItemId = o + 1;

		return list.Select(q => new GuidInt(q.Id, q.ItemId)).ToList();
	}
    public static List<GuidInt> ApuMove(this List<Apu> list, int itemId, Apu item)
    {
        if (list is null)
            return null;

        var oldIndex = list.IndexOf(list.First(q => q.Id == item.Id));
        if (oldIndex < 0)
            return null;

        var collection = list.ToObservableCollection();

        if (itemId < 1)
            collection.Move(oldIndex, 0);
        else if (itemId > list.Count)
            collection.Move(oldIndex, collection.Count - 1);
        else
            collection.Move(oldIndex, itemId - 1);

        for (var o = 0; o < collection.Count; o++)
            collection[o].ItemId = o + 1;

        return collection.Select(q => new GuidInt(q.Id, q.ItemId)).ToList();
    }

    public static List<GuidInt> ApuRemove(this List<Apu> list, Apu item)
    {
        if (list is null)
            return null;

        list.Remove(item);
        list = list.OrderBy(q => q.ItemId).ToList();

        for (var o = 0; o < list.Count; o++)
            list[o].ItemId = o + 1;

        return list.Select(q => new GuidInt(q.Id, q.ItemId)).ToList();
    }

	public static void ApuReorderGroup(this IList<Apu> items, List<GuidInt> order)
	{
        if (order is null)
            return;

        foreach (var p in order)
		{
			var item = items.FirstOrDefault(q => q.Id == p.ApuId);
			if (item is not null)
				item.ItemId = p.ItemId;
		}
    }

	public static List<T> ReplaceItem<T>(this List<T> items, T newItem, bool orderByLastUpdatedAt = false) where T : ICommon
	{
		var item = items.FirstOrDefault(q => q.Id == newItem.Id);
		if (item is null)
			return items;

		var index = items.IndexOf(item);
		if (index < 0)
			return items;

		items[index] = newItem;

		if (orderByLastUpdatedAt)
			items = items.OrderByDescending(q => q.LastUpdatedAt).ToList();


		return items;
	}

	public static IList<T> ReplaceItem<T>(this IList<T> items, T newItem, bool orderByLastUpdatedAt = false) where T : ICommon
	{
		var item = items.FirstOrDefault(q => q.Id == newItem.Id);
		if (item is null)
			return items;

		var index = items.IndexOf(item);
		if (index < 0)
			return items;

		items[index] = newItem;

		if (orderByLastUpdatedAt)
			items = items.OrderByDescending(q => q.LastUpdatedAt).ToList();


		return items;
	}
}