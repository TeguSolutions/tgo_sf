// ReSharper disable once CheckNamespace
namespace Tegu.Blazor.Controls;

public class TeguBaseComponent : ComponentBase
{
    #region Lifecycle

    protected override void OnInitialized()
    {
        Id = Guid.NewGuid().ToString();

        base.OnInitialized();
    }

    #endregion

    internal bool IsFirstParametersSet { get; set; }

    //[Inject]
    //public TeguBlazorService Service { get; set; }

    [Inject]
    protected TeguJsService JS { get; set; }

    [Parameter]
    public string Id { get; set; }

    [Parameter]
    public string Css { get; set; }

    [Parameter]
    public string Style { get; set; }

    [Parameter]
    public bool Disabled { get; set; }
}