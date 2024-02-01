using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class AuthRepository : IRepository
{
    private readonly ILogger<AuthRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    public AuthRepository(ILogger<AuthRepository> logger, IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    #region Add

    public async Task<Result> PasswordRecoveryLinkAddAsync(UserPasswordRecoveryLink link)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.UserPasswordRecoveryLinks.AddRangeAsync(link);
            await dbContext.SaveChangesAsync();

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

    public async Task<Result<UserPasswordRecoveryLink>> PasswordRecoveryLinkGetAsync(Guid linkId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var link = await dbContext.UserPasswordRecoveryLinks.FirstAsync(q => q.Id == linkId);
            return Result<UserPasswordRecoveryLink>.OkData(link);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<UserPasswordRecoveryLink>.FailMessage(e.Message);
        }
    }

    #endregion

    #region Update

    public async Task<Result> PasswordRecoveryLinkUpdateIsUsedAsync(Guid linkId, bool isUsed)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var link = await dbContext.UserPasswordRecoveryLinks.FirstOrDefaultAsync(q => q.Id == linkId);
            if (link is null) 
                return Result.Ok();
            
            link.IsUsed = isUsed;
            dbContext.UserPasswordRecoveryLinks.Update(link);
            await dbContext.SaveChangesAsync();

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