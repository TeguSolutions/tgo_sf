// ReSharper disable once CheckNamespace
namespace Tegu.Blazor.Controls;

public class TeguJsService
{
    #region Lifecycle

    private readonly IJSRuntime js;

    public TeguJsService(IJSRuntime jsRuntime)
    {
        js = jsRuntime;
    }

    #endregion

    public async Task ResizeObserverStart(object dotNetObjectReference, string id, string callback)
    {
        try
        {
            await js.InvokeVoidAsync("resizeObserver.start", dotNetObjectReference, id, callback);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public async Task ResizeObserverStop(string id)
    {
        try
        {
            await js.InvokeVoidAsync("resizeObserver.stop", id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public async Task<double> GetClientHeight(string id)
    {
        try
        {
            var height = await js.InvokeAsync<double>("getClientHeight", id);
            return height;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return 0;
        }
    }

    public async Task<double> GetScrollHeight(string id)
    {
        try
        {
            var height = await js.InvokeAsync<double>("getScrollHeight", id);
            return height;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return 0;
        }
    }

    #region TextArea

    public async Task TextAreaAddAutoResizeListener(string id)
    {
        try
        {
            await js.InvokeVoidAsync("textAreaAddAutoResizeListener", id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public async Task TextAreaRemoveAutoResizeListener(string id)
    {
        try
        {
            await js.InvokeVoidAsync("textAreaRemoveAutoResizeListener", id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
    


    #endregion



}
