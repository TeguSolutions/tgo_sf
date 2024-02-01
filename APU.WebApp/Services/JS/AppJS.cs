using System.Diagnostics;

namespace APU.WebApp.Services.JS;

public class AppJS
{
    public IJSRuntime JSRuntime { get; set; }

    public AppJS(IJSRuntime jsRuntime)
    {
        JSRuntime = jsRuntime;
    }

    public async void OpenUrlInNewTab(string url)
    {
        try
        {
            if (url.StartsWith("//") || url.StartsWith("http://") || url.StartsWith("https://"))
                await JSRuntime.InvokeVoidAsync("open", url, "_blank");
            else
            {
                url = "https://" + url;
                await JSRuntime.InvokeVoidAsync("open", url, "_blank");
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            throw;
        }
    }

    public async void OpenFileDialog(string id = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                id = "fileinput";
            await JSRuntime.InvokeVoidAsync("openFileDialog", id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    //public double GetClientHeight(string id)
    //{
    //    try
    //    {
    //        var height = JSRuntime.Invoke<double>("getClientHeight", id);
    //        return height;
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.WriteLine(e);
    //        return 0;
    //    }
    //}

    public async Task<bool> CopyToClipboardAsync(string text)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }
}