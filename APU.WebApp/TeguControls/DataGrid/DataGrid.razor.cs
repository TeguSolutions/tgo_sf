using System.Collections.ObjectModel;
using System.Globalization;

// ReSharper disable once CheckNamespace
namespace Tegu.Blazor.Controls;

public class DataGridVM<T> : TeguBaseComponent, IAsyncDisposable
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        Items = new ObservableCollection<T>();

        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        SetGroupItems();

        if (IsFirstParametersSet)
            return;

        Id = "ts-datagrid-" + Id;
        IdItems = "ts-datagrid-items-" + Id;

        IsFirstParametersSet = true;

        base.OnParametersSet();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var dotNetObjectReference = DotNetObjectReference.Create(this);
            await JS.ResizeObserverStart(dotNetObjectReference, IdItems, nameof(GridSizeChanged));
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    internal string IdItems { get; set; }

    internal List<DataGridColumnVM<T>> DataColumns { get; } = new();

    #region Parameters - Templates

    [Parameter]
    public RenderFragment Columns { get; set; }

    [Parameter]
    public RenderFragment<T> ExpandTemplate { get; set; }

    #endregion

    #region Paramaters - Settings - Grouping

    [Parameter]
    public RenderFragment Groups { get; set; }

    [Parameter]
    public bool AllowGrouping { get; set; }

    #endregion

    #region Parameters - Items

    [Parameter]
    public IEnumerable<T> Items { get; set; }

    [Parameter]
    public T SelectedItem { get; set; }

    [Parameter]
    public EventCallback<T> SelectedItemChanged { get; set; }

    internal Dictionary<T, DataGridRow<T>> Rows { get; set; } = new();

    #endregion

    #region Parameters - Styling

    [Parameter]
    public string Width { get; set; } = "auto";

    [Parameter]
    public string MaxHeight { get; set; } = "auto";


    [Parameter] 
    public string HeaderHeight { get; set; } = "36px";

    [Parameter] 
    public string RowHeight { get; set; } = "28px";

    internal string ItemContainerHeight => $"calc(100% - {HeaderHeight});";

    #endregion

    #region Groups

    internal class DataGridGroup
    {
        #region Lifecycle

        public DataGridGroup(string value, T item, RenderFragment<T> content, DataGridGroupVM<T> vm)
        {
            Value = value;
            Item = item;
            Content = content;
            VM = vm;
        }        

        #endregion

        public string Value { get; }
        public T Item { get; }
        public RenderFragment<T> Content { get; }

        public DataGridGroupVM<T> VM { get; }

        #region Equality

        protected bool Equals(DataGridGroup other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataGridGroup)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }        

        #endregion
    }

    internal class DataGridGroups
    {
        public DataGridGroups()
        {
            Groups = new List<DataGridGroup>();
        }

        public List<DataGridGroup> Groups { get; }

        #region Equality

        protected bool Equals(DataGridGroups other)
        {
            return Equals(Groups, other.Groups);
        }

        public override bool Equals(object obj)
        {
            var hashThis = GetHashCode();
            var hashObject = obj?.GetHashCode();

            return hashThis == hashObject;
        }

        public override int GetHashCode()
        {
            var hash = 0;

            foreach (var group in Groups)
                hash ^= group.GetHashCode();

            return hash;
        }        

        #endregion
    }

    private void SetGroupItems()
    {
        if (!AllowGrouping)
            return;

        if (Items is null)
            return;

        foreach (var item in Items)
        {
            var objects = new DataGridGroups();
            foreach (var gvm in GroupVMs)
            {
                // Func Group
                if (gvm.Type is DataGridGroupType.Expression)
                {
                    objects.Groups.Add(new DataGridGroup(
                        value: "",
                        item: item,
                        content: gvm.GroupTemplate,
                        vm: gvm));
                }
                // Field Group
                else if (gvm.Type is DataGridGroupType.Field)
                {
                    objects.Groups.Add(new DataGridGroup(
                        value:item.GetType().GetProperty(gvm.Field)?.GetValue(item)?.ToString(),
                        item: item,
                        content: gvm.GroupTemplate,
                        vm: gvm));
                }
                // Separator Group
                else if (gvm.Type is DataGridGroupType.Separator)
                {
                    objects.Groups.Add(new DataGridGroup(
                        value:"",
                        item: item,
                        content: gvm.GroupTemplate,
                        vm: gvm));
                }
            }

            if (!GroupedItems.ContainsKey(objects))
                GroupedItems.Add(objects, new List<T> { item });
            else
                GroupedItems[objects].Add(item);
        }

        // Func Execution
        foreach (var k in GroupedItems.Keys)
        {
            foreach (var dataGridGroup in k.Groups)
            {
                if (dataGridGroup.VM.FuncInt is not null)
                {
                    var intvalue = 0;

                    foreach (var kk in GroupedItems[k])
                        intvalue = dataGridGroup.VM.FuncInt.Invoke(kk, intvalue);

                    dataGridGroup.Item.GetType().GetProperty(dataGridGroup.VM.Field)?.SetValue(dataGridGroup.Item, intvalue.ToString());
                }

                if (dataGridGroup.VM.FuncDecimal is not null)
                {
                    decimal decimalValue = 0;

                    foreach (var kk in GroupedItems[k])
                        decimalValue = dataGridGroup.VM.FuncDecimal.Invoke(kk, decimalValue);

                    dataGridGroup.Item.GetType().GetProperty(dataGridGroup.VM.Field)?.SetValue(dataGridGroup.Item, decimalValue.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        StateHasChanged();
    }

    internal Dictionary<DataGridGroups, List<T>> GroupedItems { get; set; } = new();

    internal List<DataGridGroupVM<T>> GroupVMs { get; set; } = new();    

    #endregion

    public DataGridColumn<T> ColExpand { get; set; }

    #region Grid Size

    [JSInvokable]
    public async void GridSizeChanged()
    {
        var clientHeight = await JS.GetClientHeight(IdItems);
        var scrollHeight = await JS.GetScrollHeight(IdItems);

        GridSizeChangedAction?.Invoke(scrollHeight > clientHeight);
    }

    public Action<bool> GridSizeChangedAction { get; set; }

    #endregion

    #region Expand Template

    internal void ToggleExpandTemplate(T item)
    {
        if (ExpandTemplate is null)
            return;
        
        if (Rows.ContainsKey(item))
        {
            Rows[item].ToggleExpandTemplate();
            if (Rows[item].ShowExpandTemplate)
                RowExpanded?.Invoke(item);
            else
                RowCollapsed?.Invoke(item);
        }
    }

    [Parameter]
    public Action<T> RowExpanded { get; set; }

    [Parameter]
    public Action<T> RowCollapsed { get; set; }

    #endregion

    #region IDisposable

    public async ValueTask DisposeAsync()
    {
        await JS.ResizeObserverStop(IdItems);
    }

    #endregion
}