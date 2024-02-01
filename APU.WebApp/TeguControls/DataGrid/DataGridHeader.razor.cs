// ReSharper disable once CheckNamespace
namespace Tegu.Blazor.Controls;

public class DataGridHeaderVM<T> : ComponentBase
{
    #region Lifecycle

    private bool isFirstParametersSet;

    protected override void OnParametersSet()
    {
        if (isFirstParametersSet)
            return;

        Id = "ts-datagrid-header-" + Guid.NewGuid();

        isFirstParametersSet = true;

        DataGrid.GridSizeChangedAction = SetScrollPadding;

        base.OnParametersSet();
    }

    #endregion

    #region Cascading Parameters

    [CascadingParameter]
    public DataGrid<T> DataGrid { get; set; }

    #endregion

    #region Parameters

    [Parameter]
    public string Id { get; set; }

    #endregion

    #region ScrollPadding

    internal string ScrollPadding { get; set; }

    internal void SetScrollPadding(bool scrollbarVisible)
    {
        ScrollPadding = !scrollbarVisible ? "" : "padding-right: 16px;";

        StateHasChanged();
    }    

    #endregion
}