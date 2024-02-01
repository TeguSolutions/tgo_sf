using Syncfusion.Blazor.Gantt;
using Syncfusion.Blazor.Navigations;
using Action = System.Action;

#pragma warning disable BL0005

namespace APU.WebApp.Utils.Modules;

public class ScheduleManager
{
    #region ElementRef - Gantt

    internal SfGantt<ProjectSchedule> Gantt { get; set; }    

    #endregion

    #region Lifecycle

    public ScheduleManager()
    {
        GanttHolidays = new List<GanttHoliday>();
    }

    internal void Initialize(List<Holiday> holidays)
    {
        _holidays = holidays;

        ResetGanttData();
    }

    #endregion

    #region Callbacks

    internal Action StateChanged { get; set; }

    internal Action<ProjectSchedule> ItemAdd { get; set; }
    internal Action<ProjectSchedule> ItemUpdate { get; set; }
    internal Action ItemUpdateOrderNos { get; set; }
    internal Action ItemDeletePre { get; set; }
    internal Action<ProjectSchedule> ItemDeletePost { get; set; }

    #endregion

    #region Gantt Data - Holidays

    private List<Holiday> _holidays;

    internal List<GanttHoliday> GanttHolidays { get; set; }

    internal void SetGanttHolidays(int? startYear, int? endYear)
    {
        foreach (var holiday in _holidays)
        {
            if (holiday.Year is not null)
            {
                GanttHolidays.Add(new GanttHoliday
                {
                    From = new DateTime(holiday.Year.Value, holiday.Month, holiday.Day),
                    //To = new DateTime(holiday.Year.Value, holiday.Month, holiday.Day),
                    Label = holiday.Name,
                });
            }
            else
            {
                startYear ??= DateTime.Today.Year;
                endYear ??= startYear + 1;

                for (var year = startYear; year < endYear + 1; year++)
                {
                    GanttHolidays.Add(new GanttHoliday
                    {
                        From = new DateTime(year.Value, holiday.Month, holiday.Day),
                        //To = new DateTime(year, holiday.Month, holiday.Day),
                        Label = holiday.Name,
                    });
                }
            }
        }
    }

    #endregion

    #region Gantt - Toolbar

    internal List<object> GanttToolbarItems { get; } = new()
    {
        "Add",
        new ToolbarItem
        {
            Id = "cst_delete",
            Text = "Delete",
            TooltipText = "Delete the selected item",
            PrefixIcon = "e-icons e-delete"
        },
        "Edit", "Update", "Cancel",
        "ExpandAll", "CollapseAll",
        "Indent", "Outdent",
        "ZoomIn", "ZoomOut", "ZoomToFit",
        "PrevTimeSpan", "NextTimeSpan",
        "ExcelExport",
        new ToolbarItem
        {
            Align = ItemAlign.Right
        },
        "Search",
    };

    internal async void GanttToolbarClick(SfNavigationClickEventArgs args)
    {
        if (args.Item.Id == "cst_delete")
        {
            ItemDeletePre.Invoke();
        }

        else if (args.Item.Text == "Excel export")
        {
            var exportProperties = new ExcelExportProperties
            {
                IncludeHiddenColumn = true
            };
            await Gantt.ExportToExcelAsync(exportProperties);
        }
        //else if (args.Item.Text == "CSV export")
        //{
        //    await Gantt.ExportToCsvAsync();
        //}
    }

    #endregion

    #region Gantt - Timeline

    internal GanttZoomTimelineSettings[] GanttZoomingLevels = 
    {
        new()
        {
            Level = 0,
            TopTier = new GanttTopTierSettings { Unit = TimelineViewMode.Year, Format = "yyyy", Count = 1 }, 
            BottomTier = new GanttBottomTierSettings { Unit = TimelineViewMode.Month, Format = "MMM", Count = 1 }, 
            TimelineUnitSize = 132, 
            TimelineViewMode = TimelineViewMode.None, 
            WeekStartDay = 0, 
            UpdateTimescaleView = true, 
            ShowTooltip = true, 
            PerDayWidth = 0
        },
        new()
        {
            Level = 1,
            TopTier = new GanttTopTierSettings { Unit = TimelineViewMode.Month, Format = "yyyy MMM", Count = 1 }, 
            BottomTier = new GanttBottomTierSettings { Unit = TimelineViewMode.Week, Format = "dd ddd", Count = 1 }, 
            TimelineUnitSize = 99, 
            TimelineViewMode = TimelineViewMode.None, 
            WeekStartDay = 0, 
            UpdateTimescaleView = true, 
            ShowTooltip = true, 
            PerDayWidth = 0
        },
        new()
        {
            Level = 2,
            TopTier = new GanttTopTierSettings { Unit = TimelineViewMode.Month, Format = "yyyy MMM", Count = 1 }, 
            BottomTier = new GanttBottomTierSettings { Unit = TimelineViewMode.Day, Format = "dd", Count = 1 }, 
            TimelineUnitSize = 33, 
            TimelineViewMode = TimelineViewMode.None, 
            WeekStartDay = 0, 
            UpdateTimescaleView = true, 
            ShowTooltip = true, 
            PerDayWidth = 0
        },
        new()
        {
            Level = 3,
            TopTier = new GanttTopTierSettings { Unit = TimelineViewMode.Week, Format = "yyyy MMM dd", Count = 1 }, 
            BottomTier = new GanttBottomTierSettings { Unit = TimelineViewMode.Day, Format = "dd ddd", Count = 1 }, 
            TimelineUnitSize = 66, 
            TimelineViewMode = TimelineViewMode.None, 
            WeekStartDay = 0, 
            UpdateTimescaleView = true, 
            ShowTooltip = true, 
            PerDayWidth = 0
        },
    };

    #endregion

    #region Filter - IsHidden

    internal bool? IsHidden { get; set; }

    internal void IsHiddenChanged()
    {
        if (IsHidden == null)
            IsHidden = true;
        else if (IsHidden == true)
            IsHidden = false;
        else if (IsHidden == false)
            IsHidden = null;

        GanttFilterAsync(IsHidden);
    }

    #endregion

    #region Items / FilteredItems

    internal List<ProjectSchedule> ScheduleItems { get; set; }

    internal void SortItems()
    {
        ScheduleItems = ScheduleItems
            .OrderBy(q => q.OrderNo)
            .ToList();
    }

    internal int GetAllCount()
    {
        return ScheduleItems.Count;
    }

    internal List<Tuple<Guid, int, Guid?>> GetOrderNosAndParentIds()
    {
        if (Gantt is null)
            return null;

        var itemOrders = new List<Tuple<Guid, int, Guid?>>();

        var i = 1;

        foreach (var item in ScheduleItems)
        {
            item.OrderNo = i;
            itemOrders.Add(new Tuple<Guid, int, Guid?>(item.Id, item.OrderNo, item.ParentId));
            i++;
        }

        return itemOrders;
    }

    internal bool HasChild(Guid id)
    {
        if (ScheduleItems is null)
            return false;

        foreach (var schedule in ScheduleItems)
        {
            if (schedule.ParentId == id)
                return true;
        }

        return false;
    }

    internal async Task<ProjectSchedule> GetSelectedItemAsync()
    {
        if (Gantt is null)
            return null;

        var selectedRecords = await Gantt.GetSelectedRecordsAsync();
        if (selectedRecords is null)
            return null;
        if (selectedRecords.Count == 0)
            return null;

        return selectedRecords[0];
    }

    #endregion

    #region Data Functions

    internal void ResetGanttData()
    {
        SetData(new List<Apu>(), new List<ProjectSchedule>());
    }

    internal Result SetData(List<Apu> lineItems, List<ProjectSchedule> schedules)
    {
        var errors = "";

        ScheduleItems = schedules;

        // Step 3: Populate the schedules
        foreach (var apu in lineItems)
        {
            var schedule = ScheduleItems.FirstOrDefault(q => q.ApuId == apu.Id);
            if (schedule is null)
            {
                if (string.IsNullOrWhiteSpace(errors))
                    errors += "Synchronization needed! \r\n";

                errors += "Missing Schedule Item for Apu: " + apu.Description + "\r\n";
            }
            else
            {
                schedule.Apu = apu;
                schedule.Description = apu.Description;
            }
        }

        // Assign the Description for the custom items
        foreach (var schedule in ScheduleItems)
        {
            if (schedule.ApuId is null)
                schedule.GanttCustomDescription = schedule.Description;
        }

        SortItems();

        return string.IsNullOrWhiteSpace(errors) ? Result.Ok() : Result.Fail(errors);
    }    

    #endregion

    #region Gantt - Events

    public void GanttOnActionComplete(GanttActionEventArgs<ProjectSchedule> args)
    {
        if (args.RequestType == SfGanttAction.Refresh)
            return;

        // Any update/save action
        if (args.RequestType == SfGanttAction.Save)
        {
            if (args.Action == "Add")
            {
                ItemAdd.Invoke(args.Data);
            }
            else if (args.Action is "DialogEditing" or "TaskbarEditing" or "DrawConnectorLine")
            {
                ItemUpdate.Invoke(args.Data);
            }
            else
            { }
        }
        else if (args.RequestType == SfGanttAction.Delete)
        {
            ItemDeletePost.Invoke(args.Data);
        }
        // Indent/Outdent
        else if (args.RequestType == SfGanttAction.RowDragAndDrop)
        {
            // Unused
        }
        else
        {
            
        }
    }

    // Including classic Drag&Drop and/or Indent/Outdent
    public void GanttRowDropped(RowDroppedEventArgs<ProjectSchedule> args)
    {
        ItemUpdateOrderNos.Invoke();
    }


    internal async void GanttRowCreating(GanttRowCreatingEventArgs<ProjectSchedule> args)
    {
        if (args.Data is null)
        {
            args.Cancel = true;
            return;
        }

        // Fill and pass to the ActionComplete

        var selectedSchedule = await GetSelectedItemAsync();
        Guid? parentId = null;
        var orderNo = GetAllCount() + 1;
        if (selectedSchedule is not null)
        {
            parentId = selectedSchedule.ParentId;
            orderNo = selectedSchedule.OrderNo + 1;
        }

        if (args.Data.Id == Guid.Empty)
            args.Data.Id = Guid.NewGuid();
        args.Data.ParentId = parentId;
        args.Data.OrderNo = orderNo;
        //args.Data.ProjectId = SelectedProjectModel.Id;
        args.Data.Description = args.Data.GanttCustomDescription;
    }
    public void GanttRowUpdating(RowUpdatingEventArgs<ProjectSchedule> args)
    {

    }
    public void GanttRowUpdated(RowUpdatedEventArgs<ProjectSchedule> args)
    {

    }

    public void GanttSearched(SearchedEventArgs args)
    {
        Gantt.RefreshAsync();
    }

    #endregion

    #region Gantt Control

    internal async void GanttFilterAsync(bool? isHidden)
    {
        if (Gantt is null)
            return;

        try
        {
            if (isHidden == true)
                await Gantt.FilterByColumnAsync(nameof(ProjectSchedule.IsHidden), "equal", "True");
            else if (isHidden == false)
                await Gantt.FilterByColumnAsync(nameof(ProjectSchedule.IsHidden), "equal", "False");
            else
                await Gantt.ClearFilteringAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    #endregion

    #region CRUD

    //internal void Update(ProjectSchedule ps)
    //{
    //    var lps = ScheduleItems.FirstOrDefault(q => q.Id == ps.Id);
    //    if (lps is null)
    //        return;

    //    var i = ScheduleItems.IndexOf(lps);
    //    if (i == -1)
    //        return;

    //    ScheduleItems[i] = ps;
    //}

    #endregion
}