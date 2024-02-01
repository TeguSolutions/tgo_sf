namespace APU.WebApp.Pages.Application.Management.Dialogs;

public class DlgCertificateManagerVM : ComponentBase
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        NewItem = new Certificate();

        base.OnInitialized();
    }

    #endregion

    #region Parameters

    [Parameter]
    public string Target { get; set; }   

    #endregion

    public bool IsVisible { get; set; }

    internal Certificate NewItem { get; set; }


    private void Close()
    {
        IsVisible = false;
        StateHasChanged();

        NewItem = new Certificate();
    }

    public void Open()
    {
        NewItem = new Certificate();
        NewItem.Id = Guid.NewGuid();
        NewItem.IssuedAt = DateTime.Now;
        NewItem.ExpiresAt = DateTime.Now + TimeSpan.FromDays(365);

        IsVisible = true;
        StateHasChanged();
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

    public Action<Certificate> Submit { get; set; }
}