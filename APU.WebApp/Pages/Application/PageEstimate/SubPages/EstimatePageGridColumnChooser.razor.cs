using APU.WebApp.Services.UserSession;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

public class EstimatePageGridColumnChooserVM : ComponentBase
{
    #region Private Properties

    private Guid _userId { get; set; }
    private UserSessionService _uss { get; set; }

    internal Dictionary<string, bool> GridColumns { get; private set; }

    internal SfGrid<Apu> EGrid { get; set; }

    #endregion

    #region Lifecycle



    /// <summary>
    /// Initialize after every dependencies and values are available!
    /// </summary>
    public async void Initialize(Guid userId, UserSessionService uss, SfGrid<Apu> grid)
    {
        _userId = userId;
        _uss = uss;
        EGrid = grid;

        // Step 1 - Get the state
        GridColumns = _uss.GetEstimateGridColumns(_userId);

        // Step 2 - Populate the state
        foreach (var (field, visible) in GridColumns)
        {
            var column = EGrid.Columns.FirstOrDefault(c => c.Field == field);
            column?.SetVisibility(visible);
        }

        //await Grid.RefreshColumnsAsync();

        // Step 3 - Syncronize the state
        //foreach (var gridColumn in Grid.Columns)
        //{
        //    if (!gridColumns.ContainsKey(gridColumn.Field))
        //        gridColumns[gridColumn.Field] = gridColumn.Visible;
        //}

    }

    #endregion

    private bool isChanged;

    public void TogglePopup()
    {
        if (string.IsNullOrWhiteSpace(PopupCss))
        {
            PopupCss = "estimate-grid-columnchooser-popup-show";
            isChanged = false;
        }
        else
        {
            PopupCss = "";

            if (isChanged)
                EGrid.RefreshColumnsAsync();
        }

        StateHasChanged();
    }

    internal string PopupCss { get; set; }

    internal async void CheckedChanged(GridColumn column)
    {
        column.SetVisibility(!column.Visible);
        //Grid.RefreshColumnsAsync();
        //Grid.Refresh();

        GridColumns[column.Field] = column.Visible;
        _uss.SetEstimateGridColumns(_userId, GridColumns);

        isChanged = true;
    }
}