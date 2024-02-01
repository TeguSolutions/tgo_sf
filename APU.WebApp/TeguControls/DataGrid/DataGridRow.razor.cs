// ReSharper disable once CheckNamespace
namespace Tegu.Blazor.Controls;

public class DataGridRowVM<T> : ComponentBase
{
    #region Lifecycle

    private bool isFirstParametersSet;

    protected override void OnParametersSet()
    {
        if (isFirstParametersSet)
            return;

        Id = "ts-datagrid-row-" + Guid.NewGuid();

        isFirstParametersSet = true;

        base.OnParametersSet();
    }

    #endregion

    #region CascadingParameters

    [CascadingParameter]
    public DataGrid<T> DataGrid { get; set; }

    #endregion

    #region Parameters

    [Parameter]
    public string Id { get; set; }

    [Parameter]
    public T Item { get; set; }

    #endregion

    #region Events

    internal async void Click()
    {
        DataGrid.SelectedItem = Item;
        await DataGrid.SelectedItemChanged.InvokeAsync(Item);
    }

    #endregion

    #region Expand Template Control

    internal bool ShowExpandTemplate { get; set; }

    internal void ToggleExpandTemplate()
    {
        ShowExpandTemplate = !ShowExpandTemplate;
        StateHasChanged();
    }    

    #endregion
}