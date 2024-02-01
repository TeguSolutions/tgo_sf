namespace APU.WebApp.Pages;

[Authorize]
public class IndexVM : PageVMBase
{
    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderHome));
        }

        await base.OnAfterRenderAsync(firstRender);
    }    

    #endregion
}