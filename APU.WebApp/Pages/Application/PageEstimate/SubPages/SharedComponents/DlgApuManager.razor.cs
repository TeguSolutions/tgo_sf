using APU.WebApp.Services.Components;
using APU.WebApp.Shared.FormClasses;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;

public class DlgApuManagerVM : ComponentBase, IPopup, IDisposable
{
	#region Lifecycle

	protected override void OnInitialized()
	{
		FormClass = new ApuFormClass();

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

    internal ApuFormClass FormClass { get; set; }

    public void Open(Apu apu)
    {
        PopupService.ClosePopups(this);

        FormClass = new ApuFormClass();
        if (apu is not null)
        {
            FormClass.Id = apu.Id;
            FormClass.GroupId = apu.GroupId;
            FormClass.ItemId = apu.ItemId;
            FormClass.Code = apu.Code;
            FormClass.Description = apu.Description;
            FormClass.Unit = apu.Unit;
            FormClass.Quantity = apu.Quantity;
        }

        IsVisible = true;
        StateHasChanged();
    }


    public void DialogSubmit()
    {
        Submit?.Invoke(FormClass);
        IsVisible = false;
        FormClass = new ApuFormClass();
    }

    public void DialogCancel()
	{
        IsVisible = false;
		FormClass = new ApuFormClass();
	}

	public Action<ApuFormClass> Submit { get; set; }


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