using APU.DataV2.EntityModels;
using APU.DataV2.Utils.Helpers;
using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class ProjectRepository : IRepository
{
    private readonly ILogger<ProjectRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    #region Lifecycle

    public ProjectRepository(ILogger<ProjectRepository> logger, IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }    

    #endregion

    #region Add

    public async Task<Result<Project>> AddAsync(Project project, User user)
    {
        try
        {
            project.LastUpdatedAt = DateTime.Now;
            project.LastUpdatedById = user.Id;

            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.Projects.AddAsync(project);
            await dbContext.SaveChangesAsync();

            project.LastUpdatedBy = user;

            return Result<Project>.OkData(project);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<Project>.FailMessage(e.Message);
        }
    }    

    #endregion

    #region Get

    public async Task<Result<Project>> GetAsync(Guid projectId, bool includeCity = false, bool includeApu = false)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var project = await dbContext.Projects
                .If(includeCity, q => q.Include(p => p.City).ThenInclude(p => p.County))

                .If(includeApu, q => q.Include(p => p.Apus))
                .If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.Status))
                .If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.ApuPerformances))
                .If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.ApuLabors))
                .If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.ApuMaterials))
                .If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.ApuEquipments))
                .If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.ApuContracts))

                .Include(q => q.LastUpdatedBy)

                .FirstAsync(q => q.Id == projectId);

            return Result<Project>.OkData(project);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<Project>.FailMessage(e.Message);
        }
    }
    public async Task<Result<Project>> GetLineItemsAsync(Guid projectId)
    {
	    try
	    {
		    var dbContext = await _dbContextFactory.CreateDbContextAsync();

		    var project = await dbContext.Projects
			    .Include(p => p.City).ThenInclude(p => p.County)
			     //.Include(q => q.Apus.Where(p => p. GroupId < 999 && p.ItemId >= 1 && p.ItemId <= 999))
			    .Include(q => q.Apus.Where(p => p. GroupId <= 1000 && p.ItemId >= 1 && p.ItemId <= 999))
			    .Include(p => p.Apus).ThenInclude(o => o.Status)
				.Include(p => p.Apus).ThenInclude(o => o.ApuPerformances)
			    .Include(p => p.Apus).ThenInclude(o => o.ApuLabors)
			    .Include(p => p.Apus).ThenInclude(o => o.ApuMaterials)
			    .Include(p => p.Apus).ThenInclude(o => o.ApuEquipments)
			    .Include(p => p.Apus).ThenInclude(o => o.ApuContracts)

			    .Include(q => q.LastUpdatedBy)

			    .FirstAsync(q => q.Id == projectId);

		    return Result<Project>.OkData(project);
	    }
	    catch (Exception e)
	    {
            _logger.LogError(e.Message);
		    return Result<Project>.FailMessage(e.Message);
	    }
    }
 
    public async Task<Result<List<Project>>> GetAllAsync(bool includeCity = false/*, bool includeApu = false*/ )
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var allProjects = await dbContext.Projects
                .If(includeCity, q => q.Include(p => p.City).ThenInclude(p => p.County))

                //.If(includeApu, q => q.Include(p => p.Apus))
                //.If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.Status))
                //.If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.ApuPerformances))
                //.If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.ApuLabors))
                //.If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.ApuMaterials))
                //.If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.ApuEquipments))
                //.If(includeApu, q => q.Include(p => p.Apus).ThenInclude(o => o.ApuContracts))

                .Include(q => q.LastUpdatedBy)

                .ToListAsync();

            return Result<List<Project>>.OkData(allProjects);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<Project>>.FailMessage(e.Message);
        }
    }

    public async Task<Result<ProjectModel>> GetModelAsync(Guid projectId)
    {
	    try
	    {
		    var dbContext = await _dbContextFactory.CreateDbContextAsync();

		    var projectModel = await dbContext.Projects.Select(q => new ProjectModel
                {
                    Id = q.Id, 
                    IsBlocked = q.IsBlocked,
                    ProjectName = q.ProjectName,
                    SalesTax = q.SalesTax,
                    Bond = q.Bond,
                    Tools = q.Tools
                })
			    .FirstAsync(q => q.Id == projectId);

		    return Result<ProjectModel>.OkData(projectModel);
	    }
	    catch (Exception e)
	    {
            _logger.LogError(e.Message);
		    return Result<ProjectModel>.FailMessage(e.Message);
	    }
    }
    public async Task<Result<List<ProjectModel>>> GetAllModelAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var allProjectModels = await dbContext.Projects
                .Select(q => new ProjectModel
                {
                    Id = q.Id, 
                    IsBlocked = q.IsBlocked, 
                    ProjectName = q.ProjectName
                })
                .ToListAsync();

            return Result<List<ProjectModel>>.OkData(allProjectModels);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<ProjectModel>>.FailMessage(e.Message);
        }
    }
    public async Task<Result<List<ProjectModel>>> GetAllModelWithScheduleAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var allProjectModels = await dbContext.Projects
                .Select(q => new ProjectModel
                {
                    Id = q.Id, 
                    IsBlocked = q.IsBlocked, 
                    ProjectName = q.ProjectName,
                    HasSchedule = q.HasSchedule
                })
                .ToListAsync();

            return Result<List<ProjectModel>>.OkData(allProjectModels);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<ProjectModel>>.FailMessage(e.Message);
        }
    }

    #endregion

    #region Update

    public async Task<Result<Project>> UpdateAsync(Project project, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbProject = await dbContext.Projects.FirstAsync(q => q.Id == project.Id);

            dbProject.LastUpdatedAt = DateTime.Now;
            dbProject.LastUpdatedById = user.Id;

            dbProject.IsBlocked = project.IsBlocked;
            dbProject.ProjectName = project.ProjectName;
            dbProject.Owner = project.Owner;
            dbProject.Phone = project.Phone;
            dbProject.Email = project.Email;
            //dbProject.County = project.County;
            dbProject.Address = project.Address;
            dbProject.CityId = project.CityId;
            dbProject.State = project.State;
            dbProject.Zip = project.Zip;
            dbProject.Estimator = project.Estimator;
            dbProject.Link = project.Link;

            dbProject.Gross = project.Gross;
            dbProject.GrossContracts = project.GrossContracts;
            dbProject.GrossEquipment = project.GrossEquipment;
            dbProject.GrossLabor = project.GrossLabor;
            dbProject.GrossMaterials = project.GrossMaterials;

            dbProject.Supervision = project.Supervision;
            dbProject.Tools = project.Tools;
            dbProject.Bond = project.Bond;
            dbProject.SalesTax = project.SalesTax;

            dbContext.Projects.Update(dbProject);
            await dbContext.SaveChangesAsync();

            dbProject.LastUpdatedBy = user;

            return Result<Project>.OkData(dbProject);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<Project>.FailMessage(e.Message);
        }
    }

    public async Task<Result> UpdateHasScheduleAsync(Guid projectId, bool hasSchedule)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbProject = await dbContext.Projects.FirstAsync(q => q.Id == projectId);

            dbProject.HasSchedule = hasSchedule;

            dbContext.Projects.Update(dbProject);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> UpdateStartDateAsync(Guid projectId, DateTime? startDate)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbProject = await dbContext.Projects.FirstAsync(q => q.Id == projectId);

            dbProject.StartDate = startDate;

            dbContext.Projects.Update(dbProject);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }
    public async Task<Result> UpdateEndDateAsync(Guid projectId, DateTime? endDate)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbProject = await dbContext.Projects.FirstAsync(q => q.Id == projectId);

            dbProject.EndDate = endDate;

            dbContext.Projects.Update(dbProject);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail(e.Message);
        }
    }
    
    public async Task<Result<Project>> DuplicateAsync(Guid sProjectId, User liu)
    {
        try
        {
            var lastUpdatedAt = DateTime.Now;
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            #region Step 1: Get the whole Source Project

            var sProject = await dbContext.Projects
                .Include(q => q.Apus)
                .Include(q => q.Apus).ThenInclude(p => p.Status)
                .Include(q => q.Apus).ThenInclude(p => p.LastUpdatedBy)

                .Include(q => q.Apus).ThenInclude(p => p.ApuPerformances)
                .Include(q => q.Apus).ThenInclude(p => p.ApuLabors)
                .Include(q => q.Apus).ThenInclude(p => p.ApuMaterials)
                .Include(q => q.Apus).ThenInclude(p => p.ApuEquipments)
                .Include(q => q.Apus).ThenInclude(p => p.ApuContracts)

                .Include(q => q.City)

                .Include(q => q.LastUpdatedBy)

                .FirstAsync(q => q.Id == sProjectId);            

            #endregion

            #region Step 2: Project

            var project = DataHelper.GetProject(sProject, lastUpdatedAt, liu.Id);
            await dbContext.Projects.AddAsync(project);

            #endregion

            #region Step 3: Non Line Item Apus

            var layoutApus = new List<Apu>();

            var sApuSeparators = sProject.Apus.Where(q => q.GroupId < 1000).Where(q => q.ItemId is -2 or 0 or 1000 or 1002);
            foreach (var sApu in sApuSeparators)
            {
                var apu = DataHelper.GetBaseApu(sApu, project.Id, lastUpdatedAt, liu.Id);
                layoutApus.Add(apu);
            }

            var sApuOthers = sProject.Apus.Where(q => q.IsGroupSubTotalHeader || q.IsGroupSubTotalFooter ||
                                                                        q.IsGroup998Item1003 ||
                                                                        q.IsGroup999SubTotalHeader || q.IsGroup999SubTotalFooter || q.IsGroup999Item1003 ||
                                                                        q.IsGroup1000SubTotalHeader || q.IsGroup1000SubTotalFooter ||
                                                                        q.IsGroup1000Item1003 || q.IsGroup1000Item1004 || q.IsGroup1000Item1005 || q.IsGroup1000Item1006);
            foreach (var sApu in sApuOthers)
            {
                var apu = DataHelper.GetBaseApu(sApu, project.Id, lastUpdatedAt, liu.Id);
                layoutApus.Add(apu);
            }

            await dbContext.Apus.AddRangeAsync(layoutApus);

            #endregion

            #region Step 4: Line Item Apus

            var sLineItems = sProject.Apus.Where(q => q.IsLineItem || q.IsGroup999Item || q.IsGroup1000Item);
            foreach (var sApu in sLineItems)
            {
                var apu = DataHelper.GetLineItemApu(sApu, project.Id, lastUpdatedAt, liu.Id);
                await dbContext.Apus.AddAsync(apu);

                foreach (var sApuPerformance in sApu.ApuPerformances)
                {
                    var apuPerformance = DataHelper.GetNewApuPerformance(sApuPerformance, apu.Id, lastUpdatedAt, liu.Id);
                    await dbContext.ApuPerformances.AddAsync(apuPerformance);
                }

                foreach (var sApuLabor in sApu.ApuLabors)
                {
                    var apuLabor = DataHelper.GetNewApuLabor(sApuLabor, apu.Id, lastUpdatedAt, liu.Id);
                    await dbContext.ApuLabors.AddAsync(apuLabor);
                }

                foreach (var sApuMaterial in sApu.ApuMaterials)
                {
                    var apuMaterial = DataHelper.GetNewApuMaterial(sApuMaterial, apu.Id, lastUpdatedAt, liu.Id);
                    await dbContext.ApuMaterials.AddAsync(apuMaterial);
                }

                foreach (var sApuEquipment in sApu.ApuEquipments)
                {
                    var apuEquipment = DataHelper.GetNewApuEquipment(sApuEquipment, apu.Id, lastUpdatedAt, liu.Id);
                    await dbContext.ApuEquipments.AddAsync(apuEquipment);
                }

                foreach (var sApuContract in sApu.ApuContracts)
                {
                    var apuContract = DataHelper.GetNewApuContract(sApuContract, apu.Id, lastUpdatedAt, liu.Id);
                    await dbContext.ApuContracts.AddAsync(apuContract);
                }
            }

            #endregion

            await dbContext.SaveChangesAsync();

            project.City = sProject.City;
            //project.County = sProject.County;
            project.LastUpdatedBy = liu;   

            await dbContext.DisposeAsync();
            return Result<Project>.OkData(project);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<Project>.FailMessage(e.Message);
        }
    }

    #endregion

    #region Delete

    public async Task<Result> DeleteAsync(Guid projectId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var project = await dbContext.Projects
                .Include(q => q.Apus)
                .Include(q => q.Apus).ThenInclude(p => p.Status)
                .Include(q => q.Apus).ThenInclude(p => p.LastUpdatedBy)

                .Include(q => q.Apus).ThenInclude(p => p.ApuPerformances)
                .Include(q => q.Apus).ThenInclude(p => p.ApuLabors)
                .Include(q => q.Apus).ThenInclude(p => p.ApuMaterials)
                .Include(q => q.Apus).ThenInclude(p => p.ApuEquipments)
                .Include(q => q.Apus).ThenInclude(p => p.ApuContracts)

                .Include(q => q.ProjectSchedules)

                .FirstAsync(q => q.Id == projectId);

            // Remove the relations
            foreach (var apu in project.Apus)
            {
                foreach (var apuPerformance in apu.ApuPerformances)
                    dbContext.ApuPerformances.Remove(apuPerformance);
                
                foreach (var apuLabor in apu.ApuLabors)
                    dbContext.ApuLabors.Remove(apuLabor);
                
                foreach (var apuMaterial in apu.ApuMaterials)
                    dbContext.ApuMaterials.Remove(apuMaterial);
                
                foreach (var apuEquipment in apu.ApuEquipments)
                    dbContext.ApuEquipments.Remove(apuEquipment);
                
                foreach (var apuContract in apu.ApuContracts)
                    dbContext.ApuContracts.Remove(apuContract);

                dbContext.Apus.Remove(apu);
            }

            dbContext.ProjectSchedules.RemoveRange(project.ProjectSchedules);

            dbContext.Projects.Remove(project);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<Project>.FailMessage(e.Message);
        }
    }    

    #endregion
}