using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class UserRepository : IRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    public UserRepository(ILogger<UserRepository> logger, IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task<Result> CreateAsync(User user, UserSession session)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.Users.AddAsync(user);
            await dbContext.UserSessions.AddAsync(session);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    #region Get

    public async Task<Result<User>> GetAsync(Guid id)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var user = await dbContext.Users.FirstAsync(q => q.Id == id);

            return Result<User>.OkData(user);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<User>.FailMessage(e.Message);
        }
    }

    public async Task<Result<User>> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<User>.FailMessage("Invalid email");

        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var user = await dbContext.Users.FirstOrDefaultAsync(q => q.Email.ToLower() == email.ToLower());
            if (user is null)
                return Result<User>.OkData(null);

            return Result<User>.OkData(user);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<User>.FailMessage(e.Message);
        }
    }

    public async Task<Result<List<User>>> GetAllAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var users = await dbContext.Users
                .Include(u => u.Sessions)
                .Include(q => q.UserRoles)
                    .ThenInclude(p => p.Role)
                //.Include(q => q.LastUpdatedBy)
                .ToListAsync();

            return Result<List<User>>.OkData(users);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<User>>.FailMessage(e.Message);
        }
    }    

    #endregion

    #region Update

    public async Task<Result<User>> UpdateAsync(User user, User updatedBy)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbUser = await dbContext.Users.FirstAsync(q => q.Id == user.Id);

            dbUser.Name = user.Name;
            dbUser.Email = user.Email;
            dbUser.Initials = user.Initials;
            dbUser.IsBlocked = user.IsBlocked;
            
            dbUser.LastUpdatedAt = DateTime.Now;
            dbUser.LastUpdatedById = updatedBy.Id;

            dbContext.Users.Update(dbUser);
            await dbContext.SaveChangesAsync();

            return Result<User>.OkData(dbUser);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<User>.FailMessage(e.Message);
        }
    }

    public async Task<Result> UpdatePasswordHashAsync(Guid userId, string passwordHash)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var user = await dbContext.Users.FirstAsync(q => q.Id == userId);
            user.PasswordHash = passwordHash;

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> AddRoleAsync(UserRole userRole)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.UserRoles.AddAsync(userRole);

            await dbContext.SaveChangesAsync();
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }

    public async Task<Result> RemoveRoleAsync(Guid userId, int roleId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var userRole = await dbContext.UserRoles.FirstOrDefaultAsync(q => q.UserId == userId && q.RoleId == roleId);
            if (userRole is not null)
            {
                dbContext.UserRoles.Remove(userRole);
                await dbContext.SaveChangesAsync();
            }
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    
    #endregion

    #region Delete

    public async Task<Result> DeleteAsync(Guid userId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var userSessions = await dbContext.UserSessions.Where(q => q.UserId == userId).ToListAsync();

            var user = await dbContext.Users.FirstOrDefaultAsync(q => q.Id == userId);
            if (user is null)
                return Result.Ok();

            dbContext.UserSessions.RemoveRange(userSessions);
            dbContext.Users.Remove(user);
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


    #region UserSession

    public async Task<Result> CreateUserSessionAsync(UserSession session)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.UserSessions.AddAsync(session);
            await dbContext.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<UserSession>> GetUserSessionAsync(Guid userId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var userSession = await dbContext.UserSessions.FirstOrDefaultAsync(q => q.UserId == userId);

            return Result<UserSession>.OkData(userSession);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<UserSession>.FailMessage(e.Message);
        }
    }    

    #endregion
}