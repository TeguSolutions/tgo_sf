using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class MunicipalityRepository : IRepository
{
    private readonly ILogger<MunicipalityRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    #region Lifecycle

    public MunicipalityRepository(ILogger<MunicipalityRepository> logger,
        IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }    

    #endregion

    #region Add

    public async Task<Result> AddAsync(Municipality municipality, User user)
    {
        try
        {
            var county = municipality.County;
            municipality.County = null;

            municipality.SetLastUpdatedDb(user);

            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.Municipalities.AddAsync(municipality);
            await dbContext.SaveChangesAsync();

            municipality.LastUpdatedBy = user;
            municipality.County = county;

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    #endregion

    #region Get

    public async Task<Result<List<Municipality>>> GetAllAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var certificates = await dbContext.Municipalities
                .Include(q => q.County)
                .Include(q => q.LastUpdatedBy)
                .ToListAsync();
            return Result<List<Municipality>>.OkData(certificates);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<Municipality>>.FailMessage(e.Message);
        }
    }

    #endregion

    #region Update

    public async Task<Result> UpdateAsync(Municipality municipality, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbMunicipality = await dbContext.Municipalities.FirstAsync(q => q.Id == municipality.Id);

            dbMunicipality.Name = municipality.Name;
            dbMunicipality.Address = municipality.Address;
            dbMunicipality.Phone = municipality.Phone;
            dbMunicipality.Fax = municipality.Fax;
            dbMunicipality.WebLink = municipality.WebLink;
            dbMunicipality.Building = municipality.Building;
            dbMunicipality.Bid = municipality.Bid;
            dbMunicipality.BidSync = municipality.BidSync;
            dbMunicipality.CountyId = municipality.CountyId;

            dbMunicipality.SetLastUpdatedDb(user);

            dbContext.Municipalities.Update(dbMunicipality);
            await dbContext.SaveChangesAsync();

            municipality.SetLastUpdated(user);
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<Certificate>>.FailMessage(e.Message);
        }
    }

    #endregion

    #region Delete

    public async Task<Result> DeleteAsync(Guid municipalityId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var municipality = await dbContext.Municipalities.FirstOrDefaultAsync(q => q.Id == municipalityId);
            if (municipality is not null)
            {
                dbContext.Municipalities.Remove(municipality);
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