using Microsoft.AspNetCore.WebUtilities;

namespace APU.WebApp.Shared.NavigationHelpers;

public class LogoutAndNavigateToLoginVM : PageVMBase
{
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {        
            await CASP.MarkUserAsLoggedOut();
            var query = new Dictionary<string, string> { { "reasonmessage", "User session ended, please login again!" } };
            NavM.NavigateTo(QueryHelpers.AddQueryString("/", query));
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}
