using System.Collections.ObjectModel;
using APU.WebApp.Shared.Dialogs;
using Syncfusion.Blazor.Calendars;

namespace APU.WebApp.Pages.Application.PageEstimate.SubPages;

public class EstimateDefinitionsVM : PageVMBase
{
    #region ElementRefs - Dialogs

    internal DlgConfirmation<VendorType> DialogConfirmationVendorType { get; set; }  
    internal DlgConfirmation<Trade> DialogConfirmationTrade { get; set; }  
    internal DlgConfirmation<County> DialogConfirmationCounty { get; set; }  
    internal DlgConfirmation<(County, City)> DialogConfirmationCity { get; set; }
    internal DlgText<County> DialogCityAdd { get; set; }
    internal DlgConfirmation<Holiday> DialogConfirmationHoliday { get; set; }  

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        VendorTypes = new ObservableCollection<VendorType>();
        Trades = new ObservableCollection<Trade>();
        Counties = new ObservableCollection<County>();

        allHolidays = new List<Holiday>();
        FilteredHolidays = new ObservableCollection<Holiday>();
        NewHoliday = new Holiday()
        {
            Month = 1,
            Day = 1
        };

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderEstimate));

            DialogConfirmationVendorType.Submit = DeleteVendorType;
            DialogConfirmationTrade.Submit = DeleteTrade;
            DialogConfirmationCounty.Submit = DeleteCounty;
            DialogConfirmationCity.Submit = DeleteCity;
            DialogConfirmationHoliday.Submit = DeleteHoliday;

            DialogCityAdd.Submit = AddCity;

            ProgressStart();

            await GetVendorTypes();
            await GetTrades();
            await GetCounties();
            await GetHolidays();

            ProgressStop();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion



    #region Vendor Types

    internal ObservableCollection<VendorType> VendorTypes { get; set; }

    private async Task GetVendorTypes()
    {
        var result = await DefinitionsRepo.VendorTypesGetAllAsync();
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        VendorTypes = result.Data.OrderBy(q => q.Name).ToObservableCollection();
    }

    internal string NewVendorTypeName { get; set; }

    internal async void AddVendorType()
    {
        if (string.IsNullOrWhiteSpace(NewVendorTypeName))
            return;

        var name = NewVendorTypeName.Trim();

        var type = new VendorType();
        type.Id = Guid.NewGuid();
        type.Name = name;

        ProgressStart();

        var result = await DefinitionsRepo.VendorTypeAddAsync(type);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        NewVendorTypeName = "";

        VendorTypes.Add(type);
        VendorTypes = VendorTypes.OrderBy(q => q.Name).ToObservableCollection();

        ProgressStop();
    }

    internal async void UpdateVendorType(VendorType type)
    {
        ProgressStart();

        var result = await DefinitionsRepo.VendorTypeUpdateAsync(type.Id, type.Name);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        VendorTypes = VendorTypes.OrderBy(q => q.Name).ToObservableCollection();

        ProgressStop();
    }

    private async void DeleteVendorType(VendorType type)
    {
        ProgressStart();

        var result = await DefinitionsRepo.VendorTypeDeleteAsync(type.Id);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        VendorTypes.Remove(type);

        ProgressStop();
    }

    #endregion

    #region Trades

    internal ObservableCollection<Trade> Trades { get; set; }

    private async Task GetTrades()
    {
        var result = await DefinitionsRepo.TradeGetAllAsync();
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        Trades = result.Data.OrderBy(q => q.Name).ToObservableCollection();
    }

    internal string NewTradeName { get; set; }

    internal async void AddTrade()
    {
        if (string.IsNullOrWhiteSpace(NewTradeName))
            return;

        var name = NewTradeName.Trim();

        var trade = new Trade();
        trade.Id = Guid.NewGuid();
        trade.Name = name;

        ProgressStart();

        var result = await DefinitionsRepo.TradeAddAsync(trade);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        NewTradeName = "";

        Trades.Add(trade);
        Trades = Trades.OrderBy(q => q.Name).ToObservableCollection();

        ProgressStop();
    }

    internal async void UpdateTrade(Trade trade)
    {
        ProgressStart();

        var result = await DefinitionsRepo.TradeUpdateAsync(trade.Id, trade.Name);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        Trades = Trades.OrderBy(q => q.Name).ToObservableCollection();

        ProgressStop();
    }

    private async void DeleteTrade(Trade trade)
    {
        ProgressStart();

        var result = await DefinitionsRepo.TradeDeleteAsync(trade.Id);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        Trades.Remove(trade);

        ProgressStop();
    }

    #endregion

    #region Counties & Cities

    internal ObservableCollection<County> Counties { get; set; }

    private async Task GetCounties()
    {
        var result = await DefinitionsRepo.CountyGetAllAsync(includeCities: true);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        Counties = result.Data.OrderBy(q => q.Name).ToObservableCollection();
        foreach (var county in Counties)
            county.Cities = county.Cities.OrderBy(q => q.Name).ToList();
    }

    internal string NewCountyName { get; set; }

    internal async void AddCounty()
    {
        if (string.IsNullOrWhiteSpace(NewCountyName))
            return;

        var name = NewCountyName.Trim();

        ProgressStart();

        var result = await DefinitionsRepo.CountyAddAsync(name);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        NewCountyName = "";

        result.Data.Cities = new List<City>();
        Counties.Add(result.Data);
        Counties = Counties.OrderBy(q => q.Name).ToObservableCollection();

        ProgressStop();
    }

    internal async void UpdateCounty(County county)
    {
        ProgressStart();

        var result = await DefinitionsRepo.CountyUpdateAsync(county.Id, county.Name);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        Counties = Counties.OrderBy(q => q.Name).ToObservableCollection();

        ProgressStop();
    }

    private async void DeleteCounty(County county)
    {
        ProgressStart();

        var result = await DefinitionsRepo.CountyDeleteAsync(county.Id);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        Counties.Remove(county);

        ProgressStop();
    }


    // Cities
    internal async void AddCity((County county, string cityName) p)
    {
        var cityCheck = p.county.Cities.FirstOrDefault(q => q.Name.ToLower() == p.cityName.ToLower().Trim());
        if (cityCheck is not null)
        {
            ShowError($"City: {p.cityName} is already added!");
            return;
        }

        ProgressStart();

        var result = await DefinitionsRepo.CityAddAsync(p.county.Id, p.cityName);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        p.county.Cities.Add(result.Data);
        p.county.Cities = p.county.Cities.OrderBy(q => q.Name).ToList();

        ProgressStop();
    }

    internal async void UpdateCity((County county, City city) p)
    {
        ProgressStart();

        var result = await DefinitionsRepo.CityUpdateAsync(p.city.Id, p.city.Name);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        p.county.Cities = p.county.Cities.OrderBy(q => q.Name).ToList();

        ProgressStop();
    }

    private async void DeleteCity((County county, City city) p)
    {
        ProgressStart();

        var result = await DefinitionsRepo.CityDeleteAsync(p.city.Id);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            ProgressStop();
            return;
        }

        p.county.Cities.Remove(p.city);

        ProgressStop();
    }

    #endregion

    #region Holidays

    private List<Holiday> allHolidays;

    internal ObservableCollection<Holiday> FilteredHolidays { get; set; }

    private bool holidayFilterThisYear = true;
    internal bool HolidayFilterThisYear
    {
        get => holidayFilterThisYear;
        set
        {
            holidayFilterThisYear = value;
            GetFilteredHolidays();
        }
    }

    private bool holidayFilterUpcoming { get; set; }
    internal bool HolidayFilterUpcoming
    {
        get => holidayFilterUpcoming;
        set
        {
            holidayFilterUpcoming = value;
            GetFilteredHolidays();
        }
    }

    private bool holidayFilterAll;
    internal bool HolidayFilterAll
    {
        get => holidayFilterAll;
        set
        {
            holidayFilterAll = value;
            GetFilteredHolidays();
        }
    }

    private async Task GetHolidays()
    {
        var result = await DefinitionsRepo.HolidayGetAllAsync();
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        allHolidays = result.Data.OrderBy(q => q.Year).ThenBy(q => q.Month).ThenBy(q => q.Day).ToList();
        GetFilteredHolidays();
    }

    private void GetFilteredHolidays()
    {
        FilteredHolidays.Clear();

        foreach (var holiday in allHolidays)
        {
            if (holiday.Year is null)
            {
                FilteredHolidays.Add(holiday);
                continue;
            }

            if (holidayFilterUpcoming)
            {
                var holidayDate = new DateOnly(holiday.Year.Value, holiday.Month, holiday.Day);
                if (holidayDate >= DateOnly.FromDateTime(DateTime.Today))
                    FilteredHolidays.Add(holiday);
            }
            else if (holidayFilterThisYear)
            {
                if (holiday.Year == DateTime.Today.Year)
                    FilteredHolidays.Add(holiday);
            }
            else
            {
                FilteredHolidays.Add(holiday);
            }
        }
    }

    internal async void AddHoliday()
    {
        if (string.IsNullOrWhiteSpace(NewHoliday.Name))
        {
            ShowError("Holiday name is required!");
            return;
        }

        ProgressStart();

        var holiday = new Holiday();
        holiday.Id = Guid.NewGuid();
        holiday.Name = NewHoliday.Name;

        holiday.Year = NewHolidayYear ? NewHoliday.Year : null;
        holiday.Month = NewHoliday.Month;
        holiday.Day = NewHoliday.Day;

        var result = await DefinitionsRepo.HolidayAddAsync(holiday);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        allHolidays.Add(holiday);
        allHolidays = allHolidays.OrderBy(q => q.Year).ThenBy(q => q.Month).ThenBy(q => q.Day).ToList();

        // Reset the values
        NewHolidayDate = DateOnly.FromDateTime(DateTime.Today);
        NewHoliday = new Holiday();
        NewHoliday.Month = 1;
        NewHoliday.Day = 1;
        
        GetFilteredHolidays();
        ProgressStop();
    }    

    internal async void UpdateHoliday(Holiday holiday)
    {
        ProgressStart();

        var result = await DefinitionsRepo.HolidayUpdateAsync(holiday.Id, holiday.Name);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        GetFilteredHolidays();
        ProgressStop(false);
    }

    private async void DeleteHoliday(Holiday holiday)
    {
        ProgressStart();

        var result = await DefinitionsRepo.HolidayDeleteAsync(holiday.Id);
        if (!result.IsSuccess())
        {
            ShowError(result.Message);
            return;
        }

        allHolidays.Remove(holiday);
        GetFilteredHolidays();
        ProgressStop(false);
    }

    #endregion
    #region New Holiday

    internal DateOnly NewHolidayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    internal void NewHolidayDateChanged(ChangedEventArgs<DateOnly> args)
    {
        NewHolidayDate = args.Value;

        NewHoliday.Year = NewHolidayDate.Year;
        NewHoliday.Month = NewHolidayDate.Month;
        NewHoliday.Day = NewHolidayDate.Day;
    }

    internal bool NewHolidayYear { get; set; } = true;

    internal Holiday NewHoliday { get; set; } 

    #endregion
}