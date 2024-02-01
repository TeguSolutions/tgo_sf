// ReSharper disable once CheckNamespace
namespace Tegu.Blazor.Controls;

public class DataGridHeaderCellVM<T> : ComponentBase
{
    #region Lifecycle

    private bool isFirstParametersSet;

    protected override void OnParametersSet()
    {
        if (isFirstParametersSet)
            return;

        Id = "ts-datagrid-headercell-" + Guid.NewGuid();

        isFirstParametersSet = true;

        base.OnParametersSet();
    }

    #endregion

    #region Cascading Parameters

    [CascadingParameter]
    public DataGrid<T> DataGrid { get; set; }

    [CascadingParameter]
    public DataGridHeader<T> Header { get; set; }

    #endregion

    #region Parameters

    [Parameter]
    public string Id { get; set; }

    #endregion

    [Parameter]
    public DataGridColumnVM<T> Column { get; set; }

    internal string WidthStyle
    {
        get
        {
            // It makes the column to fill up the max available space
            if (string.IsNullOrWhiteSpace(Column.Width) || Column.Width == "auto")
                return "width: 100%; ";

            return $"min-width: {Column.Width}; width: {Column.Width}; max-width: {Column.Width}; ";
        }
    } 
}