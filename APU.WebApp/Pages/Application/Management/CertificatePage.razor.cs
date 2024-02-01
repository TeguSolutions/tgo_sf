using System.Collections.ObjectModel;
using APU.WebApp.Pages.Application.Management.Dialogs;
using APU.WebApp.Shared.Dialogs;
using Syncfusion.Blazor.Inputs;

namespace APU.WebApp.Pages.Application.Management;

[Authorize(Roles = $"{RD.AdministratorText}, {RD.SupervisorText}")]
public class CertificateVM : PageVMBase
{
    #region ElementRefs - Dialogs

    internal DlgConfirmation<Certificate> ConfirmationDialog { get; set; }   

    #endregion

    #region ElementRef - Certificate Manager

    internal DlgCertificateManager CertificateManager { get; set; }
    internal void CertificateManagerOpen()
    {
        CertificateManager.Open();
    }

    #endregion

    #region ElementRefs - Grid

    internal SfGrid<Certificate> Grid { get; set; }

    internal async void DataGridOnActionBegin(ActionEventArgs<Certificate> args)
    {
        if (args.RequestType == SfGridAction.Save)
        {
            var result = await UpdateCertificateAsync(args.Data);
            if (!result.IsSuccess())
                args.Cancel = true;
        }
    }

    internal void DataGridCommandClick(CommandClickEventArgs<Certificate> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            ConfirmationDialog.Open("Are you sure to delete the following Certificate?", args.RowData.Name, args.RowData);
            args.Cancel = true;
        }
    }

    #endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderHome));
            await EventAggregator.PublishAsync(new HeaderLinkMessage());

            ConfirmationDialog.Submit = DeleteCertificate;
            CertificateManager.Submit = CreateCertificate;

            await GetLIU();

            await GetAllCertificates();
            GetFilteredItems();
            await Grid.Refresh();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region Filters

    private string filterText = "";

    internal void TbFilterInputChanged(InputEventArgs args)
    {
        filterText = args.Value;
    }
    internal void TbFilterKeyPressed(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            GetFilteredItems();
        }
    }

    #endregion

    #region Items / FilteredItems

    private List<Certificate> allCertificates;

    private async Task GetAllCertificates()
    {
        ProgressStart();

        var result = await CertificateRepo.GetAllAsync();
        if (!result.IsSuccess())
        {
            allCertificates = new List<Certificate>();
            ShowError("Failed to collect Certificates!");
            return;
        }

        allCertificates = result.Data.OrderBy(q => q.ExpiresAt).ToList();

        ProgressStop();
    }

    public ObservableCollection<Certificate> FilteredCertificates { get; set; }

    internal async void GetFilteredItems()
    {
        FilteredCertificates = allCertificates
            .If(!string.IsNullOrWhiteSpace(filterText), q => 
                q.Where(o =>
                    o.Name.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.Name, filterText) ||
                    o.Initials.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.Initials, filterText) ||
                    o.IssuedBy.ToLower().Contains(filterText.ToLower()) || TeguStringComparer.CompareToFilterBool(o.IssuedBy, filterText)
                )
            )
            .ToObservableCollection();

        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region CRUD

    private async void CreateCertificate(Certificate certificate)
    {
        ProgressStart();

        var result = await CertificateRepo.AddAsync(certificate, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add new Certificate!");
            return;
        }

        allCertificates.Add(certificate);
        GetFilteredItems();

        ProgressStop();
    }

    private async Task<Result> UpdateCertificateAsync(Certificate certificate)
    {
        ProgressStart();

        var result = await CertificateRepo.UpdateAsync(certificate, Liu);
        if (!result.IsSuccess())
        {
            ShowError("Failed to update Certificate!");
            return Result.Fail();
        }

        GetFilteredItems();
        ProgressStop();

        return Result.Ok();
    }

    private async void DeleteCertificate(Certificate certificate)
    {
        ProgressStart();

        var result = await CertificateRepo.DeleteAsync(certificate.Id);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove Certificate!");
            return;
        }

        allCertificates.Remove(certificate);
        GetFilteredItems();

        ProgressStop();
    }

    #endregion
}