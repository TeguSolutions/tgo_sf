using APU.WebApp.Services.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;

public class DlgLaborManagerVM : ComponentBase, IPopup, IDisposable
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        Labor = new BaseLabor();

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

    internal bool IsVisible { get; set; }

    internal ManagerOpenMode Mode { get; set; }

    internal ILabor Labor { get; set; }

    public void Open(ManagerOpenMode mode, ILabor labor)
    {
        PopupService.ClosePopups(this);

        Mode = mode;

        if (Mode == ManagerOpenMode.Create)
            Labor = new BaseLabor();

        else if (Mode == ManagerOpenMode.Update || Mode == ManagerOpenMode.UpdateMix)
            Labor = labor;

        else if (Mode == ManagerOpenMode.Duplicate || Mode == ManagerOpenMode.Copy)
            Labor = new BaseLabor
            {
                Description = labor.Description,
                Salary = labor.Salary,
                HrsYear = labor.HrsYear,
                HrsStandardYear = labor.HrsStandardYear,
                HrsOvertimeYear = labor.HrsOvertimeYear,
                VacationsDays = labor.VacationsDays,
                HolydaysYear = labor.HolydaysYear,
                SickDaysYear = labor.SickDaysYear,
                BonusYear = labor.BonusYear,
                HealthYear = labor.HealthYear,
                LifeInsYear = labor.LifeInsYear,
                Percentage401 = labor.Percentage401,
                MeetingsHrsYear = labor.MeetingsHrsYear,
                OfficeHrsYear = labor.OfficeHrsYear,
                TrainingHrsYear = labor.TrainingHrsYear,
                UniformsYear = labor.UniformsYear,
                SafetyYear = labor.SafetyYear
            };

        IsVisible = true;
        StateHasChanged();
    }

    internal void DialogSubmit(ManagerSubmitMode mode)
    {
        if (Form.EditContext?.Validate() != true)
            return;

        Submit?.Invoke((mode, Labor));
        IsVisible = false;
        Labor = new BaseLabor();
    }

    internal void DialogCancel()
    {
        IsVisible = false;
        Labor = new BaseLabor();
    }


    public Action<(ManagerSubmitMode mode, ILabor labor)> Submit { get; set; }

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