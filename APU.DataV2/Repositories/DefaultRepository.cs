namespace APU.DataV2.Repositories;

public class DefaultRepository : IRepository
{
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    public DefaultRepository(IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task<Result<DefaultValue>> GetAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var defaultValue = await dbContext.DefaultValues
                .Include(q => q.LastUpdatedBy)
                .FirstAsync();

            return Result<DefaultValue>.OkData(defaultValue);
        }
        catch (Exception e)
        {
            return Result<DefaultValue>.FailMessage(e.Message);
        }
    }

    public async Task<Result<DefaultValue>> UpdateAsync(DefaultValue df, User liu)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbDefault = await dbContext.DefaultValues
                .Include(q => q.LastUpdatedBy)
                .FirstAsync(q => q.Id == df.Id);

            dbDefault.Gross = df.Gross;
            dbDefault.Supervision = df.Supervision;
            dbDefault.Tools = df.Tools;
            dbDefault.WorkersComp = df.WorkersComp;
            dbDefault.Fica = df.Fica;
            dbDefault.TopFica = df.TopFica;
            dbDefault.FutaSuta = df.FutaSuta;
            dbDefault.SalesTax = df.SalesTax;
            dbDefault.Bond = df.Bond;
            dbDefault.HrsDay = df.HrsDay;

            dbDefault.LastUpdatedAt = DateTime.Now;
            dbDefault.LastUpdatedById = liu.Id;

            dbContext.DefaultValues.Update(dbDefault);
            await dbContext.SaveChangesAsync();

            dbDefault.LastUpdatedBy = liu;

            return Result<DefaultValue>.OkData(dbDefault);
        }
        catch (Exception e)
        {
            return Result<DefaultValue>.FailMessage(e.Message);
        }
    }
}