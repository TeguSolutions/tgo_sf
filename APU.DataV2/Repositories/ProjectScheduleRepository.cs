using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class ProjectScheduleRepository : IRepository
{
    private readonly ILogger<ProjectScheduleRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    #region Lifecycle

    public ProjectScheduleRepository(ILogger<ProjectScheduleRepository> logger, IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }    

    #endregion

    #region Create

    public async Task<Result> AddAsync(ProjectSchedule schedule)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.ProjectSchedules.AddAsync(schedule);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> AddRangeAsync(List<ProjectSchedule> schedules)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.ProjectSchedules.AddRangeAsync(schedules);
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

    public async Task<Result<List<ProjectSchedule>>> GetAllAsync(Guid projectId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var items = await dbContext.ProjectSchedules.Where(q => q.ProjectId == projectId).ToListAsync();

            return Result<List<ProjectSchedule>>.OkData(items);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<ProjectSchedule>>.FailMessage(e.Message);
        }
    }

    public async Task<int> GetCountAsync(Guid projectId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var count = await dbContext.ProjectSchedules.Where(q => q.ProjectId == projectId).CountAsync();
            return count;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return -1;
        }
    }

    #endregion

    #region Update

    public async Task<Result> UpdateAsync(Guid id, Guid? parentId, string description, DateTime startDate, DateTime endDate, string duration, string predecessor)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbSchedule = await dbContext.ProjectSchedules.FirstAsync(q => q.Id == id);
            dbSchedule.ParentId = parentId;
            dbSchedule.Description = description;
            dbSchedule.StartDate = startDate;
            dbSchedule.EndDate = endDate;
            dbSchedule.Duration = duration;
            dbSchedule.Predecessor = predecessor;

            dbContext.ProjectSchedules.Update(dbSchedule);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> UpdateIsHiddenAsync(Guid id, bool isHidden)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbSchedule = await dbContext.ProjectSchedules.FirstAsync(q => q.Id == id);
            dbSchedule.IsHidden = isHidden;

            dbContext.ProjectSchedules.Update(dbSchedule);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> UpdateOrderNosAndParentIdAsync(Guid projectId, List<Tuple<Guid, int, Guid?>> p)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbSchedules = await dbContext.ProjectSchedules.Where(q => q.ProjectId == projectId).ToListAsync();

            foreach (var dbSchedule in dbSchedules)
            {
                var schedule = p.FirstOrDefault(q => q.Item1 == dbSchedule.Id);
                if (schedule is not null)
                {
                    dbSchedule.OrderNo = schedule.Item2;
                    dbSchedule.ParentId = schedule.Item3;
                }
                else
                {
                    _logger.LogWarning("Missing db Schedule for OrderNo update: " + dbSchedule.Id);
                    dbSchedule.OrderNo = dbSchedules.Count + 1;
                }
            }

            dbContext.ProjectSchedules.UpdateRange(dbSchedules);

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

    #region Delete

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var schedule = await dbContext.ProjectSchedules.FirstAsync(q => q.Id == id);
            dbContext.ProjectSchedules.Remove(schedule);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> DeleteCompleteAsync(Guid projectId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var schedules = await dbContext.ProjectSchedules.Where(q => q.ProjectId == projectId).ToListAsync();
            dbContext.ProjectSchedules.RemoveRange(schedules);
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