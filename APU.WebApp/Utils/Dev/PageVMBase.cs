using APU.DataV2.Context;
using APU.DataV2.Repositories;
using APU.WebApp.Services.Authentication;
using APU.WebApp.Services.JS;
using APU.WebApp.Services.UserSession;
using APU.WebApp.Utils.Definitions;
using Blazored.LocalStorage;
using BlazorFileSaver;
using Microsoft.EntityFrameworkCore;

namespace APU.WebApp.Utils.Dev;

public class PageVMBase : ComponentBase
{
    public bool IsFirstParameterSet { get; set; }

    #region Services

    [Inject]
    protected IServiceProvider ServiceProvider { get; set; }

    [Inject]
    protected NavigationManager NavM { get; set; }

    [Inject]
    protected CustomAuthenticationStateProvider CASP { get; set; }

    [Inject]
    protected JwtTokenService JWT { get; set; }

    [Inject]
    protected AppJS JS { get; set; }

    /// <summary>
    /// UserSessionService
    /// </summary>
    [Inject]
    protected UserSessionService USS { get; set; }

    //[Inject]
    //protected IJSRuntime JS { get; set; }    

    #endregion

    #region Data Layer

    [Inject]
    protected IDbContextFactory<ApuDbContext> ApuContextFactory { get; set; }

    [Inject]
    protected AuthRepository AuthRepo { get; set; }

    [Inject]
    protected DefaultRepository DefaultRepo { get; set; }

    [Inject] 
    protected ApuRepository ApuRepo { get; set; }    

    [Inject] 
    protected BaseItemRepository BaseItemRepo { get; set; }    

    [Inject]
    protected ProjectRepository ProjectRepo { get; set; }

    [Inject]
    protected ProjectScheduleRepository ProjectScheduleRepo { get; set; }

    [Inject]
    protected UserRepository UserRepo { get; set; }

    [Inject]
    protected CityRepository CityRepo { get; set; }

    [Inject]
    protected VendorRepository VendorRepo { get; set; }

    [Inject]
    protected DefinitionsRepository DefinitionsRepo { get; set; }

    [Inject]
    protected CertificateRepository CertificateRepo { get; set; }

    [Inject]
    protected MunicipalityRepository MunicipalityRepo { get; set; }

    #endregion


    [Inject]
    protected ILocalStorageService LocalStorage { get; set; }

    [Inject]
    protected IBlazorFileSaver FileSaver { get; set; }

    #region Liu

    public async Task<bool> GetLIU()
    {
        var (liuResult, userId, email, name, initials, roles) = await JWT.GetLoggedInUser();
        if (!liuResult.IsSuccess())
        {
            Liu = new User();
            ShowError("Failed to load the logged in user!");
            return false;
        }

        Liu = new User
        {
            Id = userId.Value,
            Email = email,
            Name = name,
            Initials = initials,
            Roles = roles
        };

        return true;
    }

    internal User Liu { get; private set; }

    #endregion

    #region Messaging

    [Inject]
    protected IEventAggregator EventAggregator { get; set; }

    public void SendHeaderMessage(Type headerType)
    {
        EventAggregator.PublishAsync(new HeaderMessage(headerType));
    }

    #endregion

    #region Progress

    internal bool IsLoading { get; set; }

    public void ProgressStart()
    {
        IsLoading = true;

        EventAggregator.PublishAsync(new ProgressMessage(true));
    }

    public async void ProgressStop(bool stateHasChanged = true)
    {
        IsLoading = false;

        await EventAggregator.PublishAsync(new ProgressMessage(false));

        if (stateHasChanged)
            await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Notification

    public async void ShowError(string message, bool progressStop = true, bool stateHasChanged = true)
    {
        await EventAggregator.PublishAsync(new ToastMessage(NotificationType.Error, "Error", message));

        if (progressStop)
            ProgressStop(stateHasChanged);
    }
    public async void ShowError(Result result, bool progressStop = true, bool stateHasChanged = true)
    {        
        await EventAggregator.PublishAsync(new ToastMessage(NotificationType.Error, "Error", result.Message));

        if (progressStop)
            ProgressStop(stateHasChanged);
    }
    public async void ShowError<T>(Result<T> result, bool progressStop = true, bool stateHasChanged = true)
    {        
        await EventAggregator.PublishAsync(new ToastMessage(NotificationType.Error, "Error", result.Message));

        if (progressStop)
            ProgressStop(stateHasChanged);
    }



    public async void ShowInfo(string message, bool progressStop = true, bool stateHasChanged = true)
    {
        await EventAggregator.PublishAsync(new ToastMessage(NotificationType.Info, "Info", message));

        if (progressStop)
            ProgressStop(stateHasChanged);
    }

    public async void ShowSuccess(string message, bool progressStop = true, bool stateHasChanged = true)
    {
        await EventAggregator.PublishAsync(new ToastMessage(NotificationType.Success, "Success", message));

        if (progressStop)
            ProgressStop(stateHasChanged);
    }

    public async void ShowWarning(string message, bool progressStop = true, bool stateHasChanged = true)
    {
        await EventAggregator.PublishAsync(new ToastMessage(NotificationType.Warning, "Warning", message));

        if (progressStop)
            ProgressStop(stateHasChanged);
    }

    #endregion
}