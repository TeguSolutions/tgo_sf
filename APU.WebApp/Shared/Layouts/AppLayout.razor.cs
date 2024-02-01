using APU.WebApp.Utils.Definitions;
using Syncfusion.Blazor.Notifications;

namespace APU.WebApp.Shared.Layouts;

 [Authorize]
public class AppLayoutVM : LayoutComponentBase, IHandle<ToastMessage>
{
    private bool isFirstParameterSet;

    [Inject]
    public IEventAggregator EventAggregator { get; set; }

    #region Lifecycle

    protected override void OnParametersSet()
    {
        if (!isFirstParameterSet)
        {            
            EventAggregator.Subscribe(this);
            isFirstParameterSet = true;
        }

        base.OnParametersSet();
    }    

    #endregion

    #region Toast Service

    internal SfToast ToastObj { get; set; }
    
    internal void ToastOnClickHandler(ToastClickEventArgs args)
    {
        args.ClickToClose = true;
    }

    public async Task HandleAsync(ToastMessage message)
    {
        await InvokeAsync(async () =>
        {
            await ToastObj.ShowAsync(GetToastModel(message));
        });
        //await ToastObj.ShowAsync(GetToastModel(message));
    }

    // Helper - Toast Builder
    private static ToastModel GetToastModel(ToastMessage message)
    {
        var toastModel = new ToastModel
        {
            Title = message.Title,
            Content = message.Content,
            Timeout = 10000,
            ExtendedTimeout = 10000,
            Target = "#app-layout-body",
            ShowProgressBar = true
        };

        //toastModel.ShowCloseButton = true;

        if (message.Type == NotificationType.Success)
        {
            toastModel.CssClass = "e-toast-success";
            toastModel.Icon = "e-success toast-icons";
        }
        else if (message.Type == NotificationType.Info)
        {
            toastModel.CssClass = "e-toast-info";
            toastModel.Icon = "e-info toast-icons";
        }
        else if (message.Type == NotificationType.Warning)
        {
            toastModel.CssClass = "e-toast-warning";
            toastModel.Icon = "e-warning toast-icons";
        }
        else if (message.Type == NotificationType.Error)
        {
            toastModel.CssClass = "e-toast-danger";
            toastModel.Icon = "e-error toast-icons";
        }

        return toastModel;
    }

    #endregion
}