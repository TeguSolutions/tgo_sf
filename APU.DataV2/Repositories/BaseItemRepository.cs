using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class BaseItemRepository : IRepository
{
    private readonly ILogger<BaseItemRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    #region Lifecycle

    public BaseItemRepository(ILogger<BaseItemRepository> logger, IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    #endregion

    #region Common

    public async Task<Result<(List<BasePerformance> basePerformances, List<BaseLabor> baseLabors, List<BaseMaterial> baseMaterials, List<BaseEquipment> baseEquipments, List<BaseContract> baseContracts)>> GetItemsAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var basePerformances = await dbContext.BasePerformances.ToListAsync();
            var baseLabors = await dbContext.BaseLabors.ToListAsync();
            var baseMaterials = await dbContext.BaseMaterials.ToListAsync();
            var baseEquipments = await dbContext.BaseEquipments.ToListAsync();
            var baseContracts = await dbContext.BaseContracts.ToListAsync();

            return Result<(List<BasePerformance> basePerformances, List<BaseLabor> baseLabors, List<BaseMaterial> baseMaterials, List<BaseEquipment> baseEquipments, List<BaseContract> baseContracts)>.OkData((basePerformances, baseLabors, baseMaterials, baseEquipments, baseContracts));
        }
        catch (Exception e)
        {
            return Result<(List<BasePerformance> basePerformances, List<BaseLabor> baseLabors, List<BaseMaterial> baseMaterials, List<BaseEquipment> baseEquipments, List<BaseContract> baseContracts)>.FailMessage(e.Message);
        }
    }    

    #endregion

    #region Base Performances

    public async Task<Result<BasePerformance>> PerformanceAddAsync(BasePerformance performance, User user)
    {
        try
        {
            performance.SetLastUpdatedDb(user);

            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.BasePerformances.AddAsync(performance);
            await dbContext.SaveChangesAsync();

            performance.LastUpdatedBy = user;

            return Result<BasePerformance>.OkData(performance);
        }
        catch (Exception e)
        {
            return Result<BasePerformance>.FailMessage(e.Message);
        }
    }

    public async Task<Result<BasePerformance>> PerformanceGetAsync(Guid performanceId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var basePerformance = await dbContext.BasePerformances.Include(q => q.LastUpdatedBy).FirstAsync(q => q.Id == performanceId);
            return Result<BasePerformance>.OkData(basePerformance);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<BasePerformance>.Fail();
        }
    }

    public async Task<Result<List<BasePerformance>>> PerformanceGetAllAsync(bool includeLastUpdated = false)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var basePerformances = await dbContext.BasePerformances
                .If(includeLastUpdated, q => q.Include(p => p.LastUpdatedBy))
                .ToListAsync();

            return Result<List<BasePerformance>>.OkData(basePerformances);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<BasePerformance>>.FailMessage(e.Message);
        }
    }

    public async Task<Result<BasePerformance>> PerformanceUpdateFromIPerformanceAsync(Guid performanceId, IPerformance apuPerformance, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbPerformance = await dbContext.BasePerformances.FirstAsync(q => q.Id == performanceId);

            dbPerformance.Description = apuPerformance.Description;
            dbPerformance.Value = apuPerformance.Value;
            dbPerformance.Hours = apuPerformance.Hours;
            
            dbPerformance.LastUpdatedAt = DateTime.Now;
            dbPerformance.LastUpdatedById = user.Id;

            dbContext.BasePerformances.Update(dbPerformance);
            await dbContext.SaveChangesAsync();

            dbPerformance.LastUpdatedBy = user;

            return Result<BasePerformance>.OkData(dbPerformance);
        }
        catch (Exception e)
        {
            return Result<BasePerformance>.FailMessage(e.Message);
        }
    }

    public async Task<Result> PerformanceDeleteAsync(Guid performanceId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbPerformance = await dbContext.BasePerformances.FirstAsync(q => q.Id == performanceId);

            dbContext.BasePerformances.Remove(dbPerformance);

            // Reset the BasePerformanceIds!
            var apuPerformances = await dbContext.ApuPerformances.Where(q => q.BasePerformanceId == performanceId).ToListAsync();
            foreach (var apuPerformance in apuPerformances)
                apuPerformance.BasePerformanceId = null;
            dbContext.ApuPerformances.UpdateRange(apuPerformances);

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

    #region Base Labors

    public async Task<Result<BaseLabor>> LaborAddAsync(BaseLabor labor, User user)
    {
        try
        {            
            labor.LastUpdatedAt = DateTime.Now;
            labor.LastUpdatedById = user.Id;
            
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.BaseLabors.AddAsync(labor);
            await dbContext.SaveChangesAsync();

            labor.LastUpdatedBy = user;

            return Result<BaseLabor>.OkData(labor);
        }
        catch (Exception e)
        {
            return Result<BaseLabor>.FailMessage(e.Message);
        }
    }

    public async Task<Result<BaseLabor>> LaborGetAsync(Guid laborId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var baseLabor = await dbContext.BaseLabors.Include(q => q.LastUpdatedBy).FirstAsync(q => q.Id == laborId);
            return Result<BaseLabor>.OkData(baseLabor);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<BaseLabor>.Fail();
        }
    }

    public async Task<Result<List<BaseLabor>>> LaborGetAllAsync(bool includeLastUpdated = false)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var baseLabors = await dbContext.BaseLabors
	            .If(includeLastUpdated, q => q.Include(p => p.LastUpdatedBy))
                .ToListAsync();

            return Result<List<BaseLabor>>.OkData(baseLabors);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<BaseLabor>>.FailMessage(e.Message);
        }
    }

    public async Task<Result<BaseLabor>> LaborUpdateFromILaborAsync(Guid laborId, ILabor apuLabor, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbLabor = await dbContext.BaseLabors.FirstAsync(q => q.Id == laborId);

            dbLabor.Description = apuLabor.Description;
            dbLabor.Salary = apuLabor.Salary;
            dbLabor.HrsYear = apuLabor.HrsYear;
            dbLabor.HrsStandardYear = apuLabor.HrsStandardYear;
            dbLabor.HrsOvertimeYear = apuLabor.HrsOvertimeYear;
            dbLabor.VacationsDays = apuLabor.VacationsDays;
            dbLabor.HolydaysYear = apuLabor.HolydaysYear;
            dbLabor.SickDaysYear = apuLabor.SickDaysYear;
            dbLabor.BonusYear = apuLabor.BonusYear;
            dbLabor.HealthYear = apuLabor.HealthYear;
            dbLabor.LifeInsYear = apuLabor.LifeInsYear;
            dbLabor.Percentage401 = apuLabor.Percentage401;
            dbLabor.MeetingsHrsYear = apuLabor.MeetingsHrsYear;
            dbLabor.OfficeHrsYear = apuLabor.OfficeHrsYear;
            dbLabor.TrainingHrsYear = apuLabor.TrainingHrsYear;
            dbLabor.UniformsYear = apuLabor.UniformsYear;
            dbLabor.SafetyYear = apuLabor.SafetyYear;

            dbLabor.LastUpdatedAt = DateTime.Now;
            dbLabor.LastUpdatedById = user.Id;

            dbContext.BaseLabors.Update(dbLabor);
            await dbContext.SaveChangesAsync();

            dbLabor.LastUpdatedBy = user;

            return Result<BaseLabor>.OkData(dbLabor);
        }
        catch (Exception e)
        {
            return Result<BaseLabor>.FailMessage(e.Message);
        }
    }

    public async Task<Result> LaborDeleteAsync(Guid laborId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbLabor = await dbContext.BaseLabors.FirstAsync(q => q.Id == laborId);

            dbContext.BaseLabors.Remove(dbLabor);

            // Reset the BaseLaborIds!
            var apuLabors = await dbContext.ApuLabors.Where(q => q.BaseLaborId == laborId).ToListAsync();
            foreach (var apuLabor in apuLabors)
                apuLabor.BaseLaborId = null;
            dbContext.ApuLabors.UpdateRange(apuLabors);

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

    #region Base Materials

    public async Task<Result<BaseMaterial>> MaterialAddAsync(BaseMaterial material, User user)
    {
        try
        {
            material.LastUpdatedAt = DateTime.Now;
            material.LastUpdatedById = user.Id;

            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.BaseMaterials.AddAsync(material);
            await dbContext.SaveChangesAsync();

            material.LastUpdatedBy = user;

            return Result<BaseMaterial>.OkData(material);
        }
        catch (Exception e)
        {
            return Result<BaseMaterial>.FailMessage(e.Message);
        }
    }

    public async Task<Result<BaseMaterial>> MaterialGetAsync(Guid materialId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var baseMaterial = await dbContext.BaseMaterials.Include(q => q.LastUpdatedBy).FirstAsync(q => q.Id == materialId);
            return Result<BaseMaterial>.OkData(baseMaterial);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<BaseMaterial>.Fail();
        }
    }

    public async Task<Result<List<BaseMaterial>>> MaterialGetAllAsync(bool includeLastUpdated = false)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var baseMaterials = await dbContext.BaseMaterials
	            .If(includeLastUpdated, q => q.Include(p => p.LastUpdatedBy))
                .ToListAsync();

            return Result<List<BaseMaterial>>.OkData(baseMaterials);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<BaseMaterial>>.FailMessage(e.Message);
        }
    }

    public async Task<Result<BaseMaterial>> MaterialUpdateFromIMaterialAsync(Guid materialId, IMaterial apuMaterial, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbMaterial = await dbContext.BaseMaterials.FirstAsync(q => q.Id == materialId);

            dbMaterial.Description = apuMaterial.Description;
            dbMaterial.Unit = apuMaterial.Unit;
            dbMaterial.Quantity = apuMaterial.Quantity;
            dbMaterial.Price = apuMaterial.Price;
            dbMaterial.Vendor = apuMaterial.Vendor;
            dbMaterial.Phone = apuMaterial.Phone;
            dbMaterial.Link = apuMaterial.Link;

            dbMaterial.LastUpdatedAt = DateTime.Now;
            dbMaterial.LastUpdatedById = user.Id;

            dbContext.BaseMaterials.Update(dbMaterial);
            await dbContext.SaveChangesAsync();

            dbMaterial.LastUpdatedBy = user;

            return Result<BaseMaterial>.OkData(dbMaterial);
        }
        catch (Exception e)
        {
            return Result<BaseMaterial>.FailMessage(e.Message);
        }
    }

    public async Task<Result> MaterialDeleteAsync(Guid materialId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbMaterial = await dbContext.BaseMaterials.FirstAsync(q => q.Id == materialId);

            dbContext.BaseMaterials.Remove(dbMaterial);
            
            // Reset the BaseMaterialIds!
            var apuMaterials = await dbContext.ApuMaterials.Where(q => q.BaseMaterialId == materialId).ToListAsync();
            foreach (var apuMaterial in apuMaterials)
                apuMaterial.BaseMaterialId = null;
            dbContext.ApuMaterials.UpdateRange(apuMaterials);
            
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

    #region Base Equipments

    public async Task<Result<BaseEquipment>> EquipmentAddAsync(BaseEquipment equipment, User user)
    {
        try
        {
            equipment.LastUpdatedAt = DateTime.Now;
            equipment.LastUpdatedById = user.Id;

            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.BaseEquipments.AddAsync(equipment);
            await dbContext.SaveChangesAsync();

            equipment.LastUpdatedBy = user;

            return Result<BaseEquipment>.OkData(equipment);
        }
        catch (Exception e)
        {
            return Result<BaseEquipment>.FailMessage(e.Message);
        }
    }

    public async Task<Result<BaseEquipment>> EquipmentGetAsync(Guid equipmentId)
    {
	    try
	    {
		    var dbContext = await _dbContextFactory.CreateDbContextAsync();
		    var baseEquipment = await dbContext.BaseEquipments.Include(q => q.LastUpdatedBy).FirstAsync(q => q.Id == equipmentId);
		    return Result<BaseEquipment>.OkData(baseEquipment);
	    }
	    catch (Exception e)
	    {
		    _logger.LogError(e.Message);
		    return Result<BaseEquipment>.Fail();
	    }
    }

    public async Task<Result<List<BaseEquipment>>> EquipmentGetAllAsync(bool includeLastUpdated = false)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var baseEquipments = await dbContext.BaseEquipments
	            .If(includeLastUpdated, q => q.Include(p => p.LastUpdatedBy))
                .ToListAsync();

            return Result<List<BaseEquipment>>.OkData(baseEquipments);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<BaseEquipment>>.FailMessage(e.Message);
        }
    }

    public async Task<Result<BaseEquipment>> EquipmentUpdateFromIEquipmentAsync(Guid equipmentId, IEquipment apuEquipment, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbEquipment = await dbContext.BaseEquipments.FirstAsync(q => q.Id == equipmentId);

            dbEquipment.Description = apuEquipment.Description;
            dbEquipment.Unit = apuEquipment.Unit;
            dbEquipment.Quantity = apuEquipment.Quantity;
            dbEquipment.Price = apuEquipment.Price;
            dbEquipment.Vendor = apuEquipment.Vendor;
            dbEquipment.Phone = apuEquipment.Phone;
            dbEquipment.Link = apuEquipment.Link;

            dbEquipment.LastUpdatedAt = DateTime.Now;
            dbEquipment.LastUpdatedById = user.Id;

            dbContext.BaseEquipments.Update(dbEquipment);
            await dbContext.SaveChangesAsync();

            dbEquipment.LastUpdatedBy = user;

            return Result<BaseEquipment>.OkData(dbEquipment);
        }
        catch (Exception e)
        {
            return Result<BaseEquipment>.FailMessage(e.Message);
        }
    }

    public async Task<Result> EquipmentDeleteAsync(Guid equipmentId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbEquipment = await dbContext.BaseEquipments.FirstAsync(q => q.Id == equipmentId);

            dbContext.BaseEquipments.Remove(dbEquipment);

            // Reset the BaseEquipmentIds!
            var apuEquipments = await dbContext.ApuEquipments.Where(q => q.BaseEquipmentId == equipmentId).ToListAsync();
            foreach (var apuEquipment in apuEquipments)
                apuEquipment.BaseEquipmentId = null;
            dbContext.ApuEquipments.UpdateRange(apuEquipments);

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

    #region Base Contracts

    public async Task<Result<BaseContract>> ContractAddAsync(BaseContract contract, User user)
    {
        try
        {
            contract.LastUpdatedAt = DateTime.Now;
            contract.LastUpdatedById = user.Id;

            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.BaseContracts.AddAsync(contract);
            await dbContext.SaveChangesAsync();

            contract.LastUpdatedBy = user;
            
            return Result<BaseContract>.OkData(contract);
        }
        catch (Exception e)
        {
            return Result<BaseContract>.FailMessage(e.Message);
        }
    }

    public async Task<Result<BaseContract>> ContractGetAsync(Guid contractId)
    {
	    try
	    {
		    var dbContext = await _dbContextFactory.CreateDbContextAsync();
		    var contract = await dbContext.BaseContracts.Include(q => q.LastUpdatedBy).FirstAsync(q => q.Id == contractId);
            return Result<BaseContract>.OkData(contract);
	    }
	    catch (Exception e)
	    {
		    _logger.LogError(e.Message);
            return Result<BaseContract>.Fail();
	    }
    }

    public async Task<Result<List<BaseContract>>> ContractGetAllAsync(bool includeLastUpdated = false)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var baseContracts = await dbContext.BaseContracts
	            .If(includeLastUpdated, q => q.Include(p => p.LastUpdatedBy))
                .ToListAsync();

            return Result<List<BaseContract>>.OkData(baseContracts);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<List<BaseContract>>.FailMessage(e.Message);
        }
    }

    public async Task<Result<BaseContract>> ContractUpdateFromIContractAsync(Guid contractId, IContract iContract, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbContract = await dbContext.BaseContracts.FirstAsync(q => q.Id == contractId);

            dbContract.Description = iContract.Description;
            dbContract.Unit = iContract.Unit;
            dbContract.Quantity = iContract.Quantity;
            dbContract.Price = iContract.Price;
            dbContract.Vendor = iContract.Vendor;
            dbContract.Phone = iContract.Phone;
            dbContract.Link = iContract.Link;

            dbContract.LastUpdatedAt = DateTime.Now;
            dbContract.LastUpdatedById = user.Id;

            dbContext.BaseContracts.Update(dbContract);
            await dbContext.SaveChangesAsync();

            dbContract.LastUpdatedBy = user;

            return Result<BaseContract>.OkData(dbContract);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<BaseContract>.FailMessage(e.Message);
        }
    }

    public async Task<Result> ContractDeleteAsync(Guid contractId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbContract = await dbContext.BaseContracts.FirstAsync(q => q.Id == contractId);

            dbContext.BaseContracts.Remove(dbContract);

            // Reset the ContractIds!
            var apuContracts = await dbContext.ApuContracts.Where(q => q.BaseContractId == contractId).ToListAsync();
            foreach (var apuContract in apuContracts)
                apuContract.BaseContractId = null;
            dbContext.ApuContracts.UpdateRange(apuContracts);

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