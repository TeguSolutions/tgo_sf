namespace APU.WebApp.Shared.Dialogs;

public class DlgTextVM<T> : ComponentBase
{
    #region Parameters

    [Parameter]
    public string Target { get; set; }

    [Parameter] 
    public string Width { get; set; } = "300px";

    [Parameter]
    public string HeaderText { get; set; }

    #endregion

    public bool IsVisible { get; set; }

    public string Text { get; set; }

    private T t;

    public void Open(T model)
    {
        t = model;
        Text = "";
        IsVisible = true;

        StateHasChanged();
    }

    public async void DialogCancel()
    {
        IsVisible = false;
        StateHasChanged();

        await Task.Delay(300);
        Text = "";
        t = default;
    }

    public async void DialogSubmit()
    {
        Submit?.Invoke((t, Text));

        IsVisible = false;
        StateHasChanged();

        await Task.Delay(300);
        Text = "";
        t = default;
    }


    public Action<(T model, string text)> Submit { get; set; }
}