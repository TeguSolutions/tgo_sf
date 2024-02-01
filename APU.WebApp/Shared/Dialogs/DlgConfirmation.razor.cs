namespace APU.WebApp.Shared.Dialogs;

public class DlgConfirmationVM<T> : ComponentBase
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

    internal string Description { get; set; }

    internal string Value { get; set; }

    private T t;

    public void Open(string description, string value, T model)
    {
        Description = description;
        Value = value;
        t = model;
        IsVisible = true;

        StateHasChanged();
    }

    // Verification Mode
    
    internal string VerificationText { get; set; }
    internal string TypedVerificationText { get; set; }

    public void Open(string description, string value, T model, string verificationText)
    {
        Description = description;
        Value = value;
        t = model;
        VerificationText = verificationText;

        IsVisible = true;

        StateHasChanged();
    }



    // Dialog functions

    public async void DialogCancel()
    {
        IsVisible = false;
        StateHasChanged();

        await Task.Delay(300);
        Description = "";
        Value = "";
        t = default;
        VerificationText = "";
        TypedVerificationText = "";
    }

    public async void DialogSubmit()
    {
        if (!string.IsNullOrWhiteSpace(VerificationText))
        {
            if (VerificationText != TypedVerificationText)
                return;
        }

        Submit?.Invoke(t);

        IsVisible = false;
        StateHasChanged();

        await Task.Delay(300);
        Description = "";
        Value = "";
        t = default;
        VerificationText = "";
        TypedVerificationText = "";
    }


    public Action<T> Submit { get; set; }
}