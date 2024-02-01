using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class DefinitionsRepository : IRepository
{
    private readonly ILogger<DefinitionsRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    #region Lifecycle

    public DefinitionsRepository(ILogger<DefinitionsRepository> logger, IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }    

    #endregion

    #region Vendor Types

    public async Task<Result> VendorTypeAddAsync(VendorType type)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.ContractorTypes.Add(type);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<List<VendorType>>> VendorTypesGetAllAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var contractorTypes = await dbContext.ContractorTypes.ToListAsync();

            return Result<List<VendorType>>.OkData(contractorTypes);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<VendorType>>.FailMessage(e.Message);
        }
    }

    public async Task<Result> VendorTypeUpdateAsync(Guid id, string name)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbType = await dbContext.ContractorTypes.FirstAsync(q => q.Id == id);
            dbType.Name = name;
            dbContext.ContractorTypes.Update(dbType);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> VendorTypeDeleteAsync(Guid id)
    {
        try
        {            
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbType = await dbContext.ContractorTypes.FirstOrDefaultAsync(q => q.Id == id);
            if (dbType is not null)
            {
                // Resolve the relations
                var vendors = await dbContext.Contractors.Where(q => q.TypeId == id).ToListAsync();
                foreach (var contractor in vendors)
                    contractor.TypeId = null;
                if (vendors.Count > 0)
                    dbContext.Contractors.UpdateRange(vendors);

                dbContext.ContractorTypes.Remove(dbType);
                await dbContext.SaveChangesAsync();
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    #endregion

    #region Trades

    public async Task<Result> TradeAddAsync(Trade trade)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.Trades.Add(trade);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<List<Trade>>> TradeGetAllAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var trades = await dbContext.Trades.ToListAsync();
            return Result<List<Trade>>.OkData(trades);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<Trade>>.FailMessage(e.Message);
        }
    }

    public async Task<Result> TradeUpdateAsync(Guid id, string name)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbTrade = await dbContext.Trades.FirstAsync(q => q.Id == id);
            dbTrade.Name = name;
            dbContext.Trades.Update(dbTrade);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> TradeDeleteAsync(Guid id)
    {
        try
        {            
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbTrade = await dbContext.Trades.FirstOrDefaultAsync(q => q.Id == id);
            if (dbTrade is not null)
            {
                // Resolve the relations
                var contractors = await dbContext.Contractors.Where(q => q.TradeId == id).ToListAsync();
                foreach (var contractor in contractors)
                    contractor.TradeId = null;
                if (contractors.Count > 0)
                    dbContext.Contractors.UpdateRange(contractors);

                dbContext.Trades.Remove(dbTrade);
                await dbContext.SaveChangesAsync();
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    #endregion

    #region Counties

    public async Task<Result<County>> CountyAddAsync(string name)
    {
        try
        {
            var county = new County();
            county.Name = name;

            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.Counties.AddAsync(county);
            await dbContext.SaveChangesAsync();

            return Result<County>.OkData(county);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<County>.FailMessage(e.Message);
        }
    }

    public async Task<Result<List<County>>> CountyGetAllAsync(bool includeCities = false)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var counties = await dbContext.Counties
                .If(includeCities, q => q.Include(p => p.Cities))
                .ToListAsync();

            return Result<List<County>>.OkData(counties);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<County>>.FailMessage(e.Message);
        }
    }

    public async Task<Result> CountyUpdateAsync(int id, string name)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbCounty = await dbContext.Counties.FirstAsync(q => q.Id == id);
            dbCounty.Name = name;
            dbContext.Counties.Update(dbCounty);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> CountyDeleteAsync(int id)
    {
        try
        {            
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbCounty = await dbContext.Counties.Include(q => q.Cities).FirstOrDefaultAsync(q => q.Id == id);
            if (dbCounty is not null)
            {
                dbContext.Cities.RemoveRange(dbCounty.Cities);
                dbContext.Counties.Remove(dbCounty);
                await dbContext.SaveChangesAsync();
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    #endregion
    #region Cities

    public async Task<Result<City>> CityAddAsync(int countyId, string name)
    {
        try
        {
            var city = new City();
            city.CountyId = countyId;
            city.Name = name;

            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.Cities.AddAsync(city);
            await dbContext.SaveChangesAsync();

            return Result<City>.OkData(city);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<City>.FailMessage(e.Message);
        }
    }

    public async Task<Result> CityUpdateAsync(int id, string name)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbCity = await dbContext.Cities.FirstAsync(q => q.Id == id);
            dbCity.Name = name;
            dbContext.Cities.Update(dbCity);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> CityDeleteAsync(int id)
    {
        try
        {            
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbCity = await dbContext.Cities.FirstOrDefaultAsync(q => q.Id == id);
            if (dbCity is not null)
            {
                dbContext.Cities.Remove(dbCity);
                await dbContext.SaveChangesAsync();
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    #endregion

    #region Holidays

    public async Task<Result> HolidayAddAsync(Holiday holiday)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.Holidays.Add(holiday);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<List<Holiday>>> HolidayGetAllAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var holidays = await dbContext.Holidays.ToListAsync();
            return Result<List<Holiday>>.OkData(holidays);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<Holiday>>.FailMessage(e.Message);
        }
    }

    public async Task<Result> HolidayUpdateAsync(Guid id, string name)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbHoliday = await dbContext.Holidays.FirstAsync(q => q.Id == id);
            dbHoliday.Name = name;
            dbContext.Holidays.Update(dbHoliday);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> HolidayDeleteAsync(Guid id)
    {
        try
        {            
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbHoliday = await dbContext.Holidays.FirstOrDefaultAsync(q => q.Id == id);
            if (dbHoliday is not null)
            {
                dbContext.Holidays.Remove(dbHoliday);
                await dbContext.SaveChangesAsync();
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    #endregion
}