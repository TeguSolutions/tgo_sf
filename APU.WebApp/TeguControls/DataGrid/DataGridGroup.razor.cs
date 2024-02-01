// ReSharper disable once CheckNamespace
namespace Tegu.Blazor.Controls;

public class DataGridGroupVM<T> : ComponentBase
{
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            DataGrid.GroupVMs.Add(this);
        }

        base.OnAfterRender(firstRender);
    }


    [Parameter]
    public RenderFragment<T> GroupTemplate { get; set; }

    [Parameter]
    public string Field { get; set; }

    [CascadingParameter]
    public DataGrid<T> DataGrid { get; set; }

    [Parameter]
    public Func<T, int, int> FuncInt { get; set; }

    [Parameter]
    public Func<T, decimal, decimal> FuncDecimal { get; set; }

    [Parameter] 
    public DataGridGroupType Type { get; set; } = DataGridGroupType.Field;
}

public enum DataGridGroupType
{
    Field = 0,
    Expression = 1,
    Separator = 2,
}