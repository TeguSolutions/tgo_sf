using APU.WebApp.Services.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;

public class DlgPerformanceManagerVM : ComponentBase, IPopup, IDisposable
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        Performance = new BasePerformance();

        base.OnInitialized();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            PopupService.Add(this);
        }

        base.OnAfterRender(firstRender);
    }

    #endregion

    #region Services

    [Inject] 
    protected PopupService PopupService { get; set; }

    #endregion

    #region Parameters

    [Parameter]
    public string Target { get; set; }   

    #endregion

    internal EditForm Form { get; set; }

    public bool IsVisible { get; set; }

    internal ManagerOpenMode Mode { get; set; }

    internal IPerformance Performance { get; set; }


    public void Open(ManagerOpenMode mode, IPerformance performance)
    {
        PopupService.ClosePopups(this);

        Mode = mode;

        if (Mode == ManagerOpenMode.Create)
            Performance = new BasePerformance();

        else if (Mode == ManagerOpenMode.Update || Mode == ManagerOpenMode.UpdateMix)
            Performance = performance;

        else if (Mode == ManagerOpenMode.Duplicate || Mode == ManagerOpenMode.Copy)
            Performance = new BasePerformance
            {
                Description = performance.Description,
                Value = performance.Value,
                Hours = performance.Hours
            };

        IsVisible = true;
        StateHasChanged();
    }

    internal void DialogSubmit(ManagerSubmitMode mode)
    {
        if (Form.EditContext?.Validate() != true)
            return;

        Submit?.Invoke((mode, Performance));
        IsVisible = false;
        Performance = new BasePerformance();
    }

    internal void DialogCancel()
    {
        Performance = new BasePerformance();
        IsVisible = false;
    }


    internal Action<(ManagerSubmitMode mode, IPerformance performance)> Submit { get; set; }


    #region IPopup

    public void ClosePopup()
    {
        DialogCancel();
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        PopupService.Remove(this);
    }    

    #endregion
}