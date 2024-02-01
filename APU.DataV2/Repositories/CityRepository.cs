using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class CityRepository : IRepository
{
    private readonly ILogger<CityRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    public CityRepository(ILogger<CityRepository> logger, IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task<Result<List<City>>> GetAllAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var cities = await dbContext.Cities.ToListAsync();
            return Result<List<City>>.OkData(cities);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<City>>.FailMessage(e.Message);
        }
    }

}