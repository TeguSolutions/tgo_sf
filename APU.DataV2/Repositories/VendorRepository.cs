using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class VendorRepository : IRepository
{
    private readonly ILogger<VendorRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    #region Lifecycle

    public VendorRepository(ILogger<VendorRepository> logger, IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }    

    #endregion

    public async Task<Result> AddAsync(Vendor contractor)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.Contractors.AddAsync(contractor);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<List<Vendor>>> GetAllAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var contractors = await dbContext.Contractors
                .Include(q => q.Type)
                .Include(q => q.Trade)
                .Include(q => q.County)
                .ToListAsync();
            return Result<List<Vendor>>.OkData(contractors);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<Vendor>>.FailMessage(e.Message);
        }
    }

    public async Task<Result> UpdateAsync(Vendor contractor)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbContractor = await dbContext.Contractors.FirstAsync(q => q.Id == contractor.Id);

            dbContractor.CompanyName = contractor.CompanyName;
            dbContractor.Address = contractor.Address;
            dbContractor.ContactPerson = contractor.ContactPerson;
            dbContractor.Phone = contractor.Phone;
            dbContractor.CEL = contractor.CEL;
            dbContractor.Email = contractor.Email;
            dbContractor.Email2 = contractor.Email2;
            dbContractor.Url = contractor.Url;
            dbContractor.Comments = contractor.Comments;

            dbContractor.TypeId = contractor.TypeId;
            dbContractor.TradeId = contractor.TradeId;
            dbContractor.CountyId = contractor.CountyId;

            dbContext.Contractors.Update(dbContractor);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> DeleteAsync(Guid contractorId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbContractor = await dbContext.Contractors.FirstOrDefaultAsync(q => q.Id == contractorId);
            if (dbContractor is not null)
            {
                dbContext.Contractors.Remove(dbContractor);
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
}