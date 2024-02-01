namespace APU.WebApp.Pages.Application.Management.Dialogs;

public class DlgMunicipalityManagerVM : ComponentBase
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        Counties = new List<County>();
        NewItem = new Municipality();

        base.OnInitialized();
    }

    #endregion

    #region Parameters

    [Parameter]
    public string Target { get; set; }   

    #endregion

    public bool IsVisible { get; set; }

    internal List<County> Counties { get; set; }
    internal Municipality NewItem { get; set; }


    internal void CountyChanged(int countyId)
    {
        NewItem.CountyId = countyId;
        NewItem.County = Counties.FirstOrDefault(q => q.Id == countyId);
    }


    public void Open(List<County> counties)
    {
        Counties = counties ?? new List<County>();

        NewItem = new Municipality();
        NewItem.Id = Guid.NewGuid();
        NewItem.Id = Guid.NewGuid();

        IsVisible = true;
        StateHasChanged();
    }

    private async void Close()
    {
        IsVisible = false;
        StateHasChanged();

        await Task.Delay(200);
        NewItem = new Municipality();
    }

    public void DialogSubmit()
    {
        Submit?.Invoke(NewItem);
        Close();
    }

    public void DialogCancel()
    {
        Close();
    }

    public Action<Municipality> Submit { get; set; }
}