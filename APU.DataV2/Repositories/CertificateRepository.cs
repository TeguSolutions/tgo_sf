using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class CertificateRepository : IRepository
{
    private readonly ILogger<CertificateRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    #region Lifecycle

    public CertificateRepository(ILogger<CertificateRepository> logger,
        IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }    

    #endregion

    #region Add

    public async Task<Result> AddAsync(Certificate certificate, User user)
    {
        try
        {
            certificate.SetLastUpdatedDb(user);

            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.Certificates.AddAsync(certificate);
            await dbContext.SaveChangesAsync();

            certificate.LastUpdatedBy = user;
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

    public async Task<Result<List<Certificate>>> GetAllAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var certificates = await dbContext.Certificates
                .Include(q => q.LastUpdatedBy)
                .ToListAsync();
            return Result<List<Certificate>>.OkData(certificates);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<Certificate>>.FailMessage(e.Message);
        }
    }

    #endregion

    #region Update

    public async Task<Result> UpdateAsync(Certificate certificate, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbCertificate = await dbContext.Certificates.FirstAsync(q => q.Id == certificate.Id);

            dbCertificate.Name = certificate.Name;
            dbCertificate.Initials = certificate.Initials;
            dbCertificate.IssuedBy = certificate.IssuedBy;
            dbCertificate.IssuedAt = certificate.IssuedAt;
            dbCertificate.ExpiresAt = certificate.ExpiresAt;
            dbCertificate.Link = certificate.Link;

            dbCertificate.SetLastUpdatedDb(user);

            dbContext.Certificates.Update(dbCertificate);
            await dbContext.SaveChangesAsync();

            certificate.SetLastUpdated(user);
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

    public async Task<Result> DeleteAsync(Guid certificateId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var certificate = await dbContext.Certificates.FirstOrDefaultAsync(q => q.Id == certificateId);
            if (certificate is not null)
            {
                dbContext.Certificates.Remove(certificate);
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