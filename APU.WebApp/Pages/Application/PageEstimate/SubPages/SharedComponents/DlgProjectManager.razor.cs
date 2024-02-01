using System.Collections.ObjectModel;
using APU.WebApp.Services.Components;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;

public class DlgProjectManagerVM : ComponentBase, IPopup, IDisposable
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        Project = new Project();

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

    public bool IsVisible { get; set; }

    internal ManagerMode Mode { get; set; }

    internal ObservableCollection<City> Cities { get; set; }
    internal Project Project { get; set; }

    public void Open(ManagerMode mode, Project project, DefaultValue defaultValues, ObservableCollection<City> cities)
    {
        PopupService.ClosePopups(this);

        Mode = mode;
        Cities = cities;

        if (Mode is ManagerMode.Add)
        {
            Project = new Project
            {
                IsBlocked = false,

                Gross = defaultValues.Gross,
                GrossContracts = defaultValues.Gross,
                GrossEquipment = defaultValues.Gross,
                GrossLabor = defaultValues.Gross,
                GrossMaterials = defaultValues.Gross,

                Supervision = defaultValues.Supervision,
                Tools = defaultValues.Tools,
                Bond = defaultValues.Bond,
                SalesTax = defaultValues.SalesTax,

                //LastUpdatedAt = DateTime.Now,
                //LastUpdatedById = liu.Id
            };
        }

        else if (Mode is ManagerMode.Update)
            Project = project;

        else if (Mode is ManagerMode.Duplicate)
        {
            Project = new Project
            {
                ProjectName = project.ProjectName,
                Owner = project.Owner,
                Phone = project.Phone,
                Email = project.Email,
                Address = project.Address,
                CityId = project.CityId,
                State = project.State,
                Zip = project.Zip,
                Estimator = project.Estimator,
                Link = project.Link,

                Gross = project.Gross,
                GrossContracts = project.GrossContracts,
                GrossEquipment = project.GrossEquipment,
                GrossLabor = project.GrossLabor,
                GrossMaterials = project.GrossMaterials,
                Supervision = project.Supervision,

                Tools = project.Tools,
                Bond = project.Bond,
                SalesTax = project.SalesTax
            };
        }

        IsVisible = true;
        StateHasChanged();
    }

    public void DialogSubmit()
    {
        Submit?.Invoke((Mode, Project));

        IsVisible = false;
        Project = new Project();
    }

    public void DialogCancel()
    {
        Project = new Project();
        IsVisible = false;
    }

    public Action<(ManagerMode mode, Project project)> Submit { get; set; }

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