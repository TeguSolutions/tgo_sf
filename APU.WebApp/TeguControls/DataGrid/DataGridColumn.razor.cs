namespace Tegu.Blazor.Controls;

public class DataGridColumnVM<T> : ComponentBase
{
    #region Lifecycle

    private bool isFirstParametersSet;
    
    protected override void OnParametersSet()
    {
        if (isFirstParametersSet)
            return;

        Id = "ts-datagrid-column-" + Guid.NewGuid();

        isFirstParametersSet = true;

        base.OnParametersSet();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            DataGrid.DataColumns.Add(this);
        }

        base.OnAfterRender(firstRender);
    }

    #endregion

    #region Cascading Parameters

    [CascadingParameter]
    public DataGrid<T> DataGrid { get; set; }

    #endregion

    #region Parameters

    [Parameter]
    public string Id { get; set; }

    [Parameter]
    public string Field { get; set; }

    [Parameter]
    public string Header { get; set; }

    [Parameter]
    public string Width { get; set; }

    [Parameter] 
    public TextAlign HeaderTextAlign { get; set; } = TextAlign.Start;

    [Parameter] 
    public TextAlign TextAlign { get; set; } = TextAlign.Start;

    #endregion

    #region Templates

    [Parameter]
    public RenderFragment HeaderTemplate { get; set; }

    [Parameter]
    public RenderFragment<T> Template { get; set; }    

    #endregion
}