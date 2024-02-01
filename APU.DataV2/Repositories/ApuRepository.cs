using APU.DataV2.Definitions;
using APU.DataV2.EntityModels;
using APU.DataV2.Utils.HelperClasses;
using APU.DataV2.Utils.Helpers;
using Microsoft.Extensions.Logging;

namespace APU.DataV2.Repositories;

public class ApuRepository : IRepository
{
    #region Lifecycle

    private readonly ILogger<ApuRepository> _logger;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    public ApuRepository(ILogger<ApuRepository> logger, IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }    

    #endregion

    #region Add

    public async Task<Result<Apu>> AddLineItemAsync(Apu apu, List<GuidInt> reordedGroupItems, Project project, ApuStatus status, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            // Step 1: Add the new Apu
            apu.ProjectId = project.Id;
            apu.StatusId = status.Id;
            apu.LastUpdatedAt = DateTime.Now;
            apu.LastUpdatedById = user.Id;

            await dbContext.Apus.AddAsync(apu);

            // Step 2: Get and update the ItemIds
            await ReorderRemainingItems(dbContext, reordedGroupItems);

            // Step 3: Save
            await dbContext.SaveChangesAsync();

            apu.Project = project;
            apu.Status = status;
            apu.LastUpdatedBy = user;

            return Result<Apu>.OkData(apu);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<Apu>.FailMessage(e.Message);
        }
    }

    public async Task<Result<Apu>> AddNonLineItemAsync(Apu apu, Project project, ApuStatus status, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            apu.ProjectId = project.Id;
            apu.StatusId = status.Id;
            apu.LastUpdatedAt = DateTime.Now;
            apu.LastUpdatedById = user.Id;

            await dbContext.Apus.AddAsync(apu);
            await dbContext.SaveChangesAsync();

            apu.Project = project;
            apu.Status = status;
            apu.LastUpdatedBy = user;

            return Result<Apu>.OkData(apu);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<Apu>.FailMessage(e.Message);
        }
    }

    public async Task<Result> AddRangeAsync(List<Apu> apus, List<Apu> apus2)
    {
	    try
	    {
		    var dbContext = await _dbContextFactory.CreateDbContextAsync();

		    foreach (var apu in apus)
			    await dbContext.Apus.AddAsync(apu);

		    foreach (var apu in apus2)
			    await dbContext.Apus.AddAsync(apu);

		    await dbContext.SaveChangesAsync();

            return Result.Ok();
	    }
	    catch (Exception e)
	    {
		    _logger.LogError(e.Message);
            return Result.Fail();
	    }
    }

    #endregion

    #region Get

    public async Task<Result<Apu>> GetAsync(Guid apuId, bool includeApuItems = false, bool includeProject = false)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var apu = await dbContext.Apus
                .If(includeApuItems, q => q.Include(p => p.ApuPerformances))
                .If(includeApuItems, q => q.Include(p => p.ApuLabors))
                .If(includeApuItems, q => q.Include(p => p.ApuMaterials))
                .If(includeApuItems, q => q.Include(p => p.ApuEquipments))
                .If(includeApuItems, q => q.Include(p => p.ApuContracts))
                .If(includeProject, q => q.Include(p => p.Project)
                    .ThenInclude(o => o.City))
                .Include(q => q.Status)
                .FirstAsync(q => q.Id == apuId);

            return Result<Apu>.OkData(apu);
        }
        catch (Exception e)
        {
            return Result<Apu>.FailMessage(e.Message);
        }
    }  
    public async Task<Result<List<Apu>>> GetMultipleAsync(List<Guid> apuIds, bool includeApuItems = false)
    {
	    try
	    {
		    var dbContext = await _dbContextFactory.CreateDbContextAsync();

		    var apus = await dbContext.Apus
			    .Where(q => apuIds.Contains(q.Id))
			    .If(includeApuItems, q => q.Include(p => p.ApuPerformances))
			    .If(includeApuItems, q => q.Include(p => p.ApuLabors))
			    .If(includeApuItems, q => q.Include(p => p.ApuMaterials))
			    .If(includeApuItems, q => q.Include(p => p.ApuEquipments))
			    .If(includeApuItems, q => q.Include(p => p.ApuContracts))
			    .Include(q => q.Status)
			
			    .ToListAsync();

		    return Result<List<Apu>>.OkData(apus);
	    }
	    catch (Exception e)
	    {
		    return Result<List<Apu>>.FailMessage(e.Message);
	    }
    }  

    public async Task<Result<List<Apu>>> GetLineItemsAsync(bool includeApuItems = false, bool includeProject = false)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var apus = await dbContext.Apus
                .Where(p => p.ItemId >= 1 && p.ItemId <= 999)

                .If(includeApuItems, q => q.Include(p => p.ApuPerformances))
                .If(includeApuItems, q => q.Include(p => p.ApuLabors))
                .If(includeApuItems, q => q.Include(p => p.ApuMaterials))
                .If(includeApuItems, q => q.Include(p => p.ApuEquipments))
                .If(includeApuItems, q => q.Include(p => p.ApuContracts))
                .If(includeProject, q => q.Include(p => p.Project)
                    .ThenInclude(o => o.City))

                .Include(q => q.LastUpdatedBy)

                .ToListAsync();

            await dbContext.DisposeAsync();

            return Result<List<Apu>>.OkData(apus);
        }
        catch (Exception e)
        {
            return Result<List<Apu>>.FailMessage(e.Message);
        }
    }



    #endregion

    #region Get - SelectorModels

    public async Task<Result<ApuSelectorModel>> GetLineItemApuSelectorModelAsync(Guid apuId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var apu = await dbContext.Apus
                .Select(q => new ApuSelectorModel
                {
                    Id = q.Id,
                    Description = q.Description,
                    ProjectId = q.ProjectId,
                    LastUpdatedAt = q.LastUpdatedAt
                })
                .FirstAsync(q => q.Id == apuId);

            //await dbContext.DisposeAsync();

            return Result<ApuSelectorModel>.OkData(apu);
        }
        catch (Exception e)
        {
            return Result<ApuSelectorModel>.FailMessage(e.Message);
        }
    }
    public async Task<Result<List<ApuSelectorModel>>> GetLineItemApuSelectorModelsAsync(List<Guid> apuIds)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var apus = await dbContext.Apus
                .Where(p => apuIds.Contains(p.Id))
                .Select(q => new ApuSelectorModel
                {
                    Id = q.Id, 
                    Description = q.Description,
                    ProjectId = q.ProjectId,
                    LastUpdatedAt = q.LastUpdatedAt
                })
                .ToListAsync();

            //await dbContext.DisposeAsync();

            return Result<List<ApuSelectorModel>>.OkData(apus);
        }
        catch (Exception e)
        {
            return Result<List<ApuSelectorModel>>.FailMessage(e.Message);
        }
    }  
    public async Task<Result<List<ApuSelectorModel>>> GetLineItemApuSelectorModelsAsync()
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var apus = await dbContext.Apus
                .Where(p => p.ItemId >= 1 && p.ItemId <= 999)
                .Select(q => new ApuSelectorModel
                {
                    Id = q.Id, 
                    Description = q.Description,
                    ProjectId = q.ProjectId,
                    LastUpdatedAt = q.LastUpdatedAt
                })
                .ToListAsync();

            //await dbContext.DisposeAsync();

            return Result<List<ApuSelectorModel>>.OkData(apus);
        }
        catch (Exception e)
        {
            return Result<List<ApuSelectorModel>>.FailMessage(e.Message);
        }
    }    

    #endregion

    #region Update

    public async Task<Result> UpdateBaseAsync(Guid apuId, int groupId, int itemId, string code, string description,
        string unit, decimal quantity, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.GroupId = groupId;
            dbApu.ItemId = itemId;
            dbApu.Code = code;
            dbApu.Description = description;
            dbApu.Unit = unit;
            dbApu.Quantity = quantity;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> UpdateBaseAndReorderGroupAsync(Guid apuId, int groupId, int itemId, string code, string description,
        string unit, decimal quantity, User user, 
        List<GuidInt> reorderedGroupItems1 = null, List<GuidInt> reorderedGroupItems2 = null)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.GroupId = groupId;
            dbApu.ItemId = itemId;
            dbApu.Code = code;
            dbApu.Description = description;
            dbApu.Unit = unit;
            dbApu.Quantity = quantity;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);

            // Step 2: Get and update the ItemIds
            if (reorderedGroupItems1 is not null)
                await ReorderRemainingItems(dbContext, reorderedGroupItems1);
            if (reorderedGroupItems2 is not null)
                await ReorderRemainingItems(dbContext, reorderedGroupItems2);

            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }

    public async Task<Result> UpdateIsBlockedAsync(Guid apuId, bool isBlocked, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.IsBlocked = isBlocked;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> UpdateStatusAsync(Guid apuId, int statusId, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.StatusId = statusId;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }

    public async Task<Result> UpdateLaborSuperPercentageAsync(Guid apuId, decimal laborSuperPct, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.SuperPercentage = laborSuperPct;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> UpdateLaborGrossPercentageAsync(Guid apuId, decimal laborGrossPct, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.LaborGrossPercentage = laborGrossPct;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> UpdateMaterialGrossPercentageAsync(Guid apuId, decimal materialGrossPct, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.MaterialGrossPercentage = materialGrossPct;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> UpdateEquipmentGrossPercentageAsync(Guid apuId, decimal equipmentGrossPct, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.EquipmentGrossPercentage = equipmentGrossPct;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> UpdateContractGrossPercentageAsync(Guid apuId, decimal contractGrossPct, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.SubcontractorGrossPercentage = contractGrossPct;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }

    public async Task<Result> UpdateLaborNotesAsync(Guid apuId, string laborNotes, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.LaborNotes = laborNotes;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> UpdateMaterialNotesAsync(Guid apuId, string materialNotes, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.MaterialNotes = materialNotes;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> UpdateEquipmentNotesAsync(Guid apuId, string equipmentNotes, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.EquipmentNotes = equipmentNotes;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> UpdateContractNotesAsync(Guid apuId, string contractNotes, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus.FirstAsync(q => q.Id == apuId);
 
            dbApu.ContractNotes = contractNotes;

            dbApu.SetLastUpdatedDb(user);
            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }

    #endregion
    #region Update - Range

    public async Task<Result> UpdateRangeAsync(List<Apu> apus, User user)
    {
	    try
	    {
		    var ids = apus.Select(q => q.Id).ToList();

		    var dbContext = await _dbContextFactory.CreateDbContextAsync();

		    var dbApus = await dbContext.Apus.Where(q => ids.Contains(q.Id)).ToListAsync();
		    foreach (var dbApu in dbApus)
		    {
			    var apu = apus.First(q => q.Id == dbApu.Id);

			    dbApu.SuperPercentage = apu.SuperPercentage;
			    dbApu.LaborGrossPercentage = apu.LaborGrossPercentage;
			    dbApu.MaterialGrossPercentage = apu.MaterialGrossPercentage;
			    dbApu.EquipmentGrossPercentage = apu.EquipmentGrossPercentage;
			    dbApu.SubcontractorGrossPercentage = apu.SubcontractorGrossPercentage;

			    dbContext.Apus.Update(dbApu);
		    }

		    await dbContext.SaveChangesAsync();
		    return Result.Ok();
	    }
	    catch (Exception e)
	    {
		    _logger.LogError(e.Message);
		    return Result.Fail();
	    }
    }

    #endregion

    #region UpdateOrAdd

    // Existing LineItems:    updates the GroupId/ItemId
    // New LineItems:         adds a new APU
    // Existing NonLineItems: updates the Description
    // New NonLineItems:      adds a new APU
    public async Task<Result> UpdateOrAddRangeFromExcelImportAsync(List<Apu> apus, User user)
    {
        try
        {
            var lineItemApus = apus.Where(q => q.IsLineItem).ToList();
            var nonLineItemApus = apus.Where(q => !q.IsLineItem).ToList();

            var lineItemIds = lineItemApus.Select(q => q.Id).ToList();
            var nonLineItemIds = nonLineItemApus.Select(q => q.Id).ToList();

            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            //// LineItens
            // Step 1: Update only the GroupId/ItemId for the existing apus
            var dbLineItemApus = await dbContext.Apus.Where(q => lineItemIds.Contains(q.Id)).ToListAsync();
            foreach (var dbApu in dbLineItemApus)
            {
                var apu = lineItemApus.First(q => q.Id == dbApu.Id);

                dbApu.GroupId = apu.GroupId;
                dbApu.ItemId = apu.ItemId;

                dbApu.SetLastUpdatedDb(user);
                dbContext.Apus.Update(dbApu);

                lineItemApus.Remove(apu);
            }

            // Step 2: Add the new apus
            foreach (var apu in lineItemApus)
            {
                apu.SetLastUpdatedDb(user);
                await dbContext.Apus.AddAsync(apu);
            }

            //// NonLineItems
            // Step 1: Update only the Description for the existing apus
            var dbNonLineItemApus = await dbContext.Apus.Where(q => nonLineItemIds.Contains(q.Id)).ToListAsync();
            foreach (var dbApu in dbNonLineItemApus)
            {
                var apu = nonLineItemApus.First(q => q.Id == dbApu.Id);

                dbApu.Description = apu.Description;
                dbApu.SetLastUpdatedDb(user);
                dbContext.Apus.Update(dbApu);

                nonLineItemApus.Remove(apu);
            }

            // Step 2: Add the new apus
            foreach (var apu in nonLineItemApus)
            {
                apu.SetLastUpdatedDb(user);
                await dbContext.Apus.AddAsync(apu);
            }

            await dbContext.SaveChangesAsync();
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

    public async Task<Result> DeleteLineItemAsync(Guid apuId, List<GuidInt> reordedGroupItems)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus
                .Include(q => q.ApuPerformances)
                .Include(q => q.ApuLabors)
                .Include(q => q.ApuMaterials)
                .Include(q => q.ApuEquipments)
                .Include(q => q.ApuContracts)
                .Include(q => q.ProjectSchedules)
                .FirstAsync(q => q.Id == apuId);

            // Step 1: Remove the Apu Items (removes from the dbApu automatically)
            dbContext.ApuPerformances.RemoveRange(dbApu.ApuPerformances);
            dbContext.ApuLabors.RemoveRange(dbApu.ApuLabors);
            dbContext.ApuMaterials.RemoveRange(dbApu.ApuMaterials);
            dbContext.ApuEquipments.RemoveRange(dbApu.ApuEquipments);
            dbContext.ApuContracts.RemoveRange(dbApu.ApuContracts);
            dbContext.ProjectSchedules.RemoveRange(dbApu.ProjectSchedules);

            dbContext.Apus.Remove(dbApu);

            // Step 2: Get and update the ItemIds
            await ReorderRemainingItems(dbContext, reordedGroupItems);

            // Step 3: Save
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
    public async Task<Result> DeleteNonLineItemAsync(Guid apuId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus
                .Include(q => q.ApuPerformances)
                .Include(q => q.ApuLabors)
                .Include(q => q.ApuMaterials)
                .Include(q => q.ApuEquipments)
                .Include(q => q.ApuContracts)
                .FirstAsync(q => q.Id == apuId);

            // Step 1: Remove the Apu Items (removes from the dbApu automatically)
            dbContext.ApuPerformances.RemoveRange(dbApu.ApuPerformances);
            dbContext.ApuLabors.RemoveRange(dbApu.ApuLabors);
            dbContext.ApuMaterials.RemoveRange(dbApu.ApuMaterials);
            dbContext.ApuEquipments.RemoveRange(dbApu.ApuEquipments);
            dbContext.ApuContracts.RemoveRange(dbApu.ApuContracts);

            dbContext.Apus.Remove(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }    

    #endregion

    #region Apu Helper

    private async Task ReorderRemainingItems(ApuDbContext dbContext, List<GuidInt> reordedGroupItems)
    {
        var apuIds = reordedGroupItems.Select(q => q.ApuId).ToList();
        var apus = await dbContext.Apus.Where(q => apuIds.Contains(q.Id)).ToListAsync();

        foreach (var reordedGroupItem in reordedGroupItems)
        {
            var oapu = apus.FirstOrDefault(q => q.Id == reordedGroupItem.ApuId);
            if (oapu is not null)
                oapu.ItemId = reordedGroupItem.ItemId;
        }
    }

    #endregion

    #region Apu Items

    public async Task<Result> RemoveItemsAsync(Guid apuId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbApu = await dbContext.Apus
                .Include(q => q.ApuPerformances)
                .Include(q => q.ApuLabors)
                .Include(q => q.ApuMaterials)
                .Include(q => q.ApuEquipments)
                .Include(q => q.ApuContracts)
                .FirstAsync(q => q.Id == apuId);

            // Step 1: Remove the Apu Items (removes from the dbApu automatically)
            dbContext.ApuPerformances.RemoveRange(dbApu.ApuPerformances);
            dbContext.ApuLabors.RemoveRange(dbApu.ApuLabors);
            dbContext.ApuMaterials.RemoveRange(dbApu.ApuMaterials);
            dbContext.ApuEquipments.RemoveRange(dbApu.ApuEquipments);
            dbContext.ApuContracts.RemoveRange(dbApu.ApuContracts);

            dbContext.Apus.Update(dbApu);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<Apu>> AddItemsAsync(Apu sourceApu, Guid targetApuId)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var targetApu = await dbContext.Apus.FirstAsync(q => q.Id == targetApuId);

            // Step 1: Copy the base values
            DataHelper.CopyApu_BaseValues(sourceApu, targetApu);

            // Step 2: Add the Apu Performances
            foreach (var saperformance in sourceApu.ApuPerformances)
            {
                var apuPerformance = new ApuPerformance
                {
                    Id = Guid.NewGuid(),
                    ApuId = targetApu.Id,
                    BasePerformanceId = saperformance.BasePerformanceId,

                    Description = saperformance.Description,
                    Value = saperformance.Value,
                    Hours = saperformance.Hours
                };

                dbContext.ApuPerformances.Add(apuPerformance);
                //targetApu.ApuPerformances.Add(apuPerformance);
            }

            foreach (var salabor in sourceApu.ApuLabors)
            {
                var apuLabor = new ApuLabor
                {
                    Id = Guid.NewGuid(),
                    ApuId = targetApu.Id,
                    BaseLaborId = salabor.BaseLaborId,

                    Description = salabor.Description,
                    Quantity = salabor.Quantity,
                    Salary = salabor.Salary,
                    HrsYear = salabor.HrsYear,
                    HrsStandardYear = salabor.HrsStandardYear,
                    HrsOvertimeYear = salabor.HrsOvertimeYear,
                    VacationsDays = salabor.VacationsDays,
                    HolydaysYear = salabor.HolydaysYear,
                    SickDaysYear = salabor.SickDaysYear,
                    BonusYear = salabor.BonusYear,
                    HealthYear = salabor.HealthYear,
                    LifeInsYear = salabor.LifeInsYear,
                    Percentage401 = salabor.Percentage401,
                    MeetingsHrsYear = salabor.MeetingsHrsYear,
                    OfficeHrsYear = salabor.OfficeHrsYear,
                    TrainingHrsYear = salabor.TrainingHrsYear,
                    UniformsYear = salabor.UniformsYear,
                    SafetyYear = salabor.SafetyYear,

                    WorkTypeId = salabor.WorkTypeId,
                };

                dbContext.ApuLabors.Add(apuLabor);
                //targetApu.ApuLabors.Add(apuLabor);
            }

            foreach (var samaterial in sourceApu.ApuMaterials)
            {
                var apuMaterial = new ApuMaterial
                {
                    Id = Guid.NewGuid(),
                    ApuId = targetApu.Id,
                    BaseMaterialId = samaterial.BaseMaterialId,

                    Description = samaterial.Description,
                    Unit = samaterial.Unit,
                    Quantity = samaterial.Quantity,
                    Waste = samaterial.Waste,
                    Price = samaterial.Price,
                    Vendor = samaterial.Vendor,
                    Phone = samaterial.Phone,
                    Link = samaterial.Link,

                    ItemTypeId = samaterial.ItemTypeId,
                };

                dbContext.ApuMaterials.Add(apuMaterial);
                //targetApu.ApuMaterials.Add(apuMaterial);
            }

            foreach (var saequipment in sourceApu.ApuEquipments)
            {
                var apuEquipment = new ApuEquipment
                {
                    Id = Guid.NewGuid(),
                    ApuId = targetApu.Id,
                    BaseEquipmentId = saequipment.BaseEquipmentId,

                    Description = saequipment.Description,
                    Unit = saequipment.Unit,
                    Quantity = saequipment.Quantity,
                    Price = saequipment.Price,
                    Vendor = saequipment.Vendor,
                    Phone = saequipment.Phone,
                    Link = saequipment.Link,

                    ItemTypeId = saequipment.ItemTypeId,
                };

                dbContext.ApuEquipments.Add(apuEquipment);
                //targetApu.ApuEquipments.Add(apuEquipment);
            }

            foreach (var sacontract in sourceApu.ApuContracts)
            {
                var apuContract = new ApuContract
                {
                    Id = Guid.NewGuid(),
                    ApuId = targetApu.Id,
                    BaseContractId = sacontract.BaseContractId,

                    Description = sacontract.Description,
                    Unit = sacontract.Unit,
                    Quantity = sacontract.Quantity,
                    Price = sacontract.Price,
                    Vendor = sacontract.Vendor,
                    Phone = sacontract.Phone,
                    Link = sacontract.Link,

                    ItemTypeId = sacontract.ItemTypeId,
                };

                dbContext.ApuContracts.Add(apuContract);
                //targetApu.ApuContracts.Add(apuContract);
            }

            await dbContext.SaveChangesAsync();

            return Result<Apu>.OkData(targetApu);
        }
        catch (Exception e)
        {
            return Result<Apu>.FailMessage(e.Message);
        }
    }

    #endregion

    #region ApuPerformance

    public async Task<Result<ApuPerformance>> ApuPerformanceAddFromBasePerformanceAsync(BasePerformance basePerformance, Apu apu, User user)
    {
        try
        {
            var performance = new ApuPerformance
            {
                Id = Guid.NewGuid(),
                ApuId = apu.Id,
                BasePerformanceId = basePerformance.Id,

                Description = basePerformance.Description,
                Value = basePerformance.Value,
                Hours = basePerformance.Hours,

                LastUpdatedAt = DateTime.Now,
                LastUpdatedById = user.Id
            };

            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.ApuPerformances.AddAsync(performance);
            await dbContext.SaveChangesAsync();

            performance.Apu = apu;
            performance.BasePerformance = basePerformance;
            performance.LastUpdatedBy = user;

            return Result<ApuPerformance>.OkData(performance);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<ApuPerformance>.Fail();
        }
    }
    public async Task<Result<ApuPerformance>> ApuPerformanceUpdateAsync(ApuPerformance performance, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbPerformance = await dbContext.ApuPerformances.FirstAsync(q => q.Id == performance.Id);

            dbPerformance.Description = performance.Description;
            dbPerformance.Value = performance.Value;
            dbPerformance.Hours = performance.Hours;
            
            dbPerformance.LastUpdatedAt = DateTime.Now;
            dbPerformance.LastUpdatedById = user.Id;

            dbContext.ApuPerformances.Update(dbPerformance);
            await dbContext.SaveChangesAsync();

            dbPerformance.Apu = performance.Apu;
            dbPerformance.BasePerformance = performance.BasePerformance;
            dbPerformance.LastUpdatedBy = user;

            return Result<ApuPerformance>.OkData(dbPerformance);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<ApuPerformance>.Fail();
        }
    }
    public async Task<Result> ApuPerformanceRemoveAsync(ApuPerformance performance)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbPerformance = await dbContext.ApuPerformances.FirstOrDefaultAsync(q => q.Id == performance.Id);
            if (dbPerformance is null)
                return Result.Ok();

            dbContext.ApuPerformances.Remove(dbPerformance);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }    

    #endregion

    #region ApuLabors

    public async Task<Result<ApuLabor>> ApuLaborAddFromBaseLaborAsync(BaseLabor baseLabor, Apu apu, User user)
    {
        try
        {
            var apuLabor = new ApuLabor
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                ApuId = apu.Id,
                BaseLaborId = baseLabor.Id,

                Description = baseLabor.Description,
                Quantity = 1,
                Salary = baseLabor.Salary,
                HrsYear = baseLabor.HrsYear,
                HrsStandardYear = baseLabor.HrsStandardYear,
                HrsOvertimeYear = baseLabor.HrsOvertimeYear,
                VacationsDays = baseLabor.VacationsDays,
                HolydaysYear = baseLabor.HolydaysYear,
                SickDaysYear = baseLabor.SickDaysYear,
                BonusYear = baseLabor.BonusYear,
                HealthYear = baseLabor.HealthYear,
                LifeInsYear = baseLabor.LifeInsYear,
                Percentage401 = baseLabor.Percentage401,
                MeetingsHrsYear = baseLabor.MeetingsHrsYear,
                OfficeHrsYear = baseLabor.OfficeHrsYear,
                TrainingHrsYear = baseLabor.TrainingHrsYear,
                UniformsYear = baseLabor.UniformsYear,
                SafetyYear = baseLabor.SafetyYear,

                WorkTypeId = LaborWorkTypeDefinitions.Hours.Id,

                LastUpdatedAt = DateTime.Now,
                LastUpdatedById = user.Id
            };

            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.ApuLabors.AddAsync(apuLabor);
            await dbContext.SaveChangesAsync();

            apuLabor.Apu = apu;
            apuLabor.BaseLabor = baseLabor;
            apuLabor.WorkType = LaborWorkTypeDefinitions.Hours;
            apuLabor.LastUpdatedBy = user;

            return Result<ApuLabor>.OkData(apuLabor);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<ApuLabor>.Fail();
        }
    }
    public async Task<Result> ApuLaborRemoveAsync(ApuLabor labor)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbLabor = await dbContext.ApuLabors.FirstOrDefaultAsync(q => q.Id == labor.Id);
            if (dbLabor is null)
                return Result.Ok();

            dbContext.ApuLabors.Remove(dbLabor);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }    

    public async Task<Result<ApuLabor>> ApuLaborUpdateAsync(ApuLabor labor, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbLabor = await dbContext.ApuLabors.FirstAsync(q => q.Id == labor.Id);

            dbLabor.Quantity = labor.Quantity;
            dbLabor.WorkTypeId = labor.WorkTypeId;

            // Base values
            dbLabor.Description = labor.Description;
            dbLabor.Salary = labor.Salary;
            dbLabor.HrsYear = labor.HrsYear;
            dbLabor.HrsStandardYear = labor.HrsStandardYear;
            dbLabor.HrsOvertimeYear = labor.HrsOvertimeYear;
            dbLabor.VacationsDays = labor.VacationsDays;
            dbLabor.HolydaysYear = labor.HolydaysYear;
            dbLabor.SickDaysYear = labor.SickDaysYear;
            dbLabor.BonusYear = labor.BonusYear;
            dbLabor.HealthYear = labor.HealthYear;
            dbLabor.LifeInsYear = labor.LifeInsYear;
            dbLabor.Percentage401 = labor.Percentage401;
            dbLabor.MeetingsHrsYear = labor.MeetingsHrsYear;
            dbLabor.OfficeHrsYear = labor.OfficeHrsYear;
            dbLabor.TrainingHrsYear = labor.TrainingHrsYear;
            dbLabor.UniformsYear = labor.UniformsYear;
            dbLabor.SafetyYear = labor.SafetyYear;

            dbLabor.LastUpdatedAt = DateTime.Now;
            dbLabor.LastUpdatedById = user.Id;

            dbContext.ApuLabors.Update(dbLabor);
            await dbContext.SaveChangesAsync();

            dbLabor.Apu = labor.Apu;
            dbLabor.BaseLabor = labor.BaseLabor;
            dbLabor.WorkType = labor.WorkType;
            dbLabor.LastUpdatedBy = user;

            return Result<ApuLabor>.OkData(dbLabor);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<ApuLabor>.Fail();
        }
    }
    public async Task<Result> ApuLaborUpdateWorkTypeAsync(Guid laborId, int workTypeId, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbLabor = await dbContext.ApuLabors.FirstAsync(q => q.Id == laborId);

            dbLabor.WorkTypeId = workTypeId;

            dbLabor.LastUpdatedAt = DateTime.Now;
            dbLabor.LastUpdatedById = user.Id;

            dbContext.ApuLabors.Update(dbLabor);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> ApuLaborUpdateQuantityAsync(Guid laborId, decimal quantity, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbLabor = await dbContext.ApuLabors.FirstAsync(q => q.Id == laborId);

            dbLabor.Quantity = quantity;

            dbLabor.LastUpdatedAt = DateTime.Now;
            dbLabor.LastUpdatedById = user.Id;

            dbContext.ApuLabors.Update(dbLabor);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }

    #endregion

    #region ApuMaterials

    public async Task<Result<ApuMaterial>> ApuMaterialAddFromBaseMaterialAsync(BaseMaterial baseMaterial, Apu apu, User user)
    {
        try
        {
            var apuMaterial = new ApuMaterial
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                ApuId = apu.Id,
                BaseMaterialId = baseMaterial.Id,

                ItemTypeId = ItemTypeDefinitions.ByUnit.Id,

                Description = baseMaterial.Description,
                Unit = baseMaterial.Unit,
                Quantity = baseMaterial.Quantity,
                Price = baseMaterial.Price,
                Vendor = baseMaterial.Vendor,
                Phone = baseMaterial.Phone,
                Link = baseMaterial.Link,

                LastUpdatedAt = DateTime.Now,
                LastUpdatedById = user.Id
            };

            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.ApuMaterials.AddAsync(apuMaterial);
            await dbContext.SaveChangesAsync();

            apuMaterial.Apu = apu;
            apuMaterial.BaseMaterial = baseMaterial;
            apuMaterial.ItemType = ItemTypeDefinitions.ByUnit;
            apuMaterial.LastUpdatedBy = user;

            return Result<ApuMaterial>.OkData(apuMaterial);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<ApuMaterial>.Fail();
        }
    }
    public async Task<Result> ApuMaterialRemoveAsync(ApuMaterial material)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbMaterial = await dbContext.ApuMaterials.FirstOrDefaultAsync(q => q.Id == material.Id);
            if (dbMaterial is null)
                return Result.Ok();

            dbContext.ApuMaterials.Remove(dbMaterial);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }    

    public async Task<Result<ApuMaterial>> ApuMaterialUpdateAsync(ApuMaterial material, User user)
    {
        try
        {           
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbMaterial = await dbContext.ApuMaterials.FirstAsync(q => q.Id == material.Id);

            dbMaterial.Quantity = material.Quantity;
            dbMaterial.Waste = material.Waste;

            dbMaterial.ItemTypeId = material.ItemTypeId;

            // Base values
            dbMaterial.Description = material.Description;
            dbMaterial.Unit = material.Unit;
            dbMaterial.Price = material.Price;
            dbMaterial.Vendor = material.Vendor;
            dbMaterial.Phone = material.Phone;
            dbMaterial.Link = material.Link;

            dbMaterial.LastUpdatedAt = DateTime.Now;
            dbMaterial.LastUpdatedById = user.Id;

            dbContext.ApuMaterials.Update(dbMaterial);
            await dbContext.SaveChangesAsync();

            dbMaterial.Apu = material.Apu;
            dbMaterial.BaseMaterial = material.BaseMaterial;
            dbMaterial.ItemType = material.ItemType;
            dbMaterial.LastUpdatedBy = user;

            return Result<ApuMaterial>.OkData(dbMaterial);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<ApuMaterial>.Fail();
        }
    }
    public async Task<Result> ApuMaterialUpdateItemTypeAsync(Guid materialId, int itemTypeId, User user)
    {
        try
        {           
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbMaterial = await dbContext.ApuMaterials.FirstAsync(q => q.Id == materialId);

            dbMaterial.ItemTypeId = itemTypeId;

            dbMaterial.LastUpdatedAt = DateTime.Now;
            dbMaterial.LastUpdatedById = user.Id;

            dbContext.ApuMaterials.Update(dbMaterial);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> ApuMaterialUpdateQuantityAsync(Guid materialId, decimal quantity, User user)
    {
        try
        {           
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbMaterial = await dbContext.ApuMaterials.FirstAsync(q => q.Id == materialId);

            dbMaterial.Quantity = quantity;

            dbMaterial.LastUpdatedAt = DateTime.Now;
            dbMaterial.LastUpdatedById = user.Id;

            dbContext.ApuMaterials.Update(dbMaterial);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> ApuMaterialUpdateWasteAsync(Guid materialId, decimal waste, User user)
    {
        try
        {           
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbMaterial = await dbContext.ApuMaterials.FirstAsync(q => q.Id == materialId);

            dbMaterial.Waste = waste;

            dbMaterial.LastUpdatedAt = DateTime.Now;
            dbMaterial.LastUpdatedById = user.Id;

            dbContext.ApuMaterials.Update(dbMaterial);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }

    #endregion

    #region ApuEquipments

    public async Task<Result<ApuEquipment>> ApuEquipmentAddFromBaseEquipmentAsync(BaseEquipment baseEquipment, Apu apu, User user)
    {
        try
        {
            var apuEquipment = new ApuEquipment
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                ApuId = apu.Id,
                BaseEquipmentId = baseEquipment.Id,

                ItemTypeId = ItemTypeDefinitions.ByUnit.Id,

                Description = baseEquipment.Description,
                Unit = baseEquipment.Unit,
                Quantity = baseEquipment.Quantity,
                Price = baseEquipment.Price,
                Vendor = baseEquipment.Vendor,
                Phone = baseEquipment.Phone,
                Link = baseEquipment.Link,

                LastUpdatedAt = DateTime.Now,
                LastUpdatedById = user.Id
            };

            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.ApuEquipments.AddAsync(apuEquipment);
            await dbContext.SaveChangesAsync();

            apuEquipment.Apu = apu;
            apuEquipment.BaseEquipment = baseEquipment;
            apuEquipment.ItemType = ItemTypeDefinitions.ByUnit;
            apuEquipment.LastUpdatedBy = user;

            return Result<ApuEquipment>.OkData(apuEquipment);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<ApuEquipment>.Fail();
        }
    }
    public async Task<Result> ApuEquipmentRemoveAsync(ApuEquipment equipment)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbEquipment = await dbContext.ApuEquipments.FirstOrDefaultAsync(q => q.Id == equipment.Id);
            if (dbEquipment is null)
                return Result.Ok();

            dbContext.ApuEquipments.Remove(dbEquipment);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }    

    public async Task<Result<ApuEquipment>> ApuEquipmentUpdateAsync(ApuEquipment equipment, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbEquipment = await dbContext.ApuEquipments.FirstAsync(q => q.Id == equipment.Id);

            dbEquipment.Quantity = equipment.Quantity;

            dbEquipment.ItemTypeId = equipment.ItemTypeId;

            // Base values
            dbEquipment.Description = equipment.Description;
            dbEquipment.Unit = equipment.Unit;
            dbEquipment.Price = equipment.Price;
            dbEquipment.Vendor = equipment.Vendor;
            dbEquipment.Phone = equipment.Phone;
            dbEquipment.Link = equipment.Link;

            dbEquipment.LastUpdatedAt = DateTime.Now;
            dbEquipment.LastUpdatedById = user.Id;

            dbContext.ApuEquipments.Update(dbEquipment);
            await dbContext.SaveChangesAsync();

            dbEquipment.Apu = equipment.Apu;
            dbEquipment.BaseEquipment = equipment.BaseEquipment;
            dbEquipment.ItemType = equipment.ItemType;
            dbEquipment.LastUpdatedBy = user;

            return Result<ApuEquipment>.OkData(dbEquipment);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<ApuEquipment>.Fail();
        }
    }
    public async Task<Result> ApuEquipmentUpdateItemTypeAsync(Guid equipmentId, int itemTypeId, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbEquipment = await dbContext.ApuEquipments.FirstAsync(q => q.Id == equipmentId);

            dbEquipment.ItemTypeId = itemTypeId;

            dbEquipment.LastUpdatedAt = DateTime.Now;
            dbEquipment.LastUpdatedById = user.Id;

            dbContext.ApuEquipments.Update(dbEquipment);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> ApuEquipmentUpdateQuantityAsync(Guid equipmentId, decimal quantity, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbEquipment = await dbContext.ApuEquipments.FirstAsync(q => q.Id == equipmentId);

            dbEquipment.Quantity = quantity;

            dbEquipment.LastUpdatedAt = DateTime.Now;
            dbEquipment.LastUpdatedById = user.Id;

            dbContext.ApuEquipments.Update(dbEquipment);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }

    #endregion

    #region ApuContracts

    public async Task<Result<ApuContract>> ApuContractAddFromBaseContractAsync(BaseContract baseContract, Apu apu, User user)
    {
        try
        {
            var apuContract = new ApuContract
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                ApuId = apu.Id,
                BaseContractId = baseContract.Id,

                ItemTypeId = ItemTypeDefinitions.ByUnit.Id,

                Description = baseContract.Description,
                Unit = baseContract.Unit,
                Quantity = baseContract.Quantity,
                Price = baseContract.Price,
                Vendor = baseContract.Vendor,
                Phone = baseContract.Phone,
                Link = baseContract.Link,

                LastUpdatedAt = DateTime.Now,
                LastUpdatedById = user.Id
            };

            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            await dbContext.ApuContracts.AddAsync(apuContract);
            await dbContext.SaveChangesAsync();

            apuContract.Apu = apu;
            apuContract.BaseContract = baseContract;
            apuContract.ItemType = ItemTypeDefinitions.ByUnit;
            apuContract.LastUpdatedBy = user;

            return Result<ApuContract>.OkData(apuContract);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<ApuContract>.Fail();
        }
    }
    public async Task<Result> ApuContractRemoveAsync(ApuContract contract)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var dbContract = await dbContext.ApuContracts.FirstOrDefaultAsync(q => q.Id == contract.Id);
            if (dbContract is null)
                return Result.Ok();

            dbContext.ApuContracts.Remove(dbContract);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }    

    public async Task<Result<ApuContract>> ApuContractUpdateAsync(ApuContract contract, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbContract = await dbContext.ApuContracts.FirstAsync(q => q.Id == contract.Id);

            dbContract.Quantity = contract.Quantity;
            dbContract.ItemTypeId = contract.ItemTypeId;

            // Base values
            dbContract.Description = contract.Description;
            dbContract.Unit = contract.Unit;
            dbContract.Quantity = contract.Quantity;
            dbContract.Price = contract.Price;
            dbContract.Vendor = contract.Vendor;
            dbContract.Phone = contract.Phone;
            dbContract.Link = contract.Link;

            dbContract.LastUpdatedAt = DateTime.Now;
            dbContract.LastUpdatedById = user.Id;

            dbContext.ApuContracts.Update(dbContract);
            await dbContext.SaveChangesAsync();

            dbContract.Apu = contract.Apu;
            dbContract.BaseContract = contract.BaseContract;
            dbContract.ItemType = contract.ItemType;
            dbContract.LastUpdatedBy = user;

            return Result<ApuContract>.OkData(dbContract);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<ApuContract>.Fail();
        }
    }
    public async Task<Result> ApuContractUpdateItemTypeAsync(Guid contractId, int itemTypeId, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbContract = await dbContext.ApuContracts.FirstAsync(q => q.Id == contractId);

            dbContract.ItemTypeId = itemTypeId;

            dbContract.LastUpdatedAt = DateTime.Now;
            dbContract.LastUpdatedById = user.Id;

            dbContext.ApuContracts.Update(dbContract);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }
    public async Task<Result> ApuContractUpdateQuantityAsync(Guid contractId, decimal quantity, User user)
    {
        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbContract = await dbContext.ApuContracts.FirstAsync(q => q.Id == contractId);

            dbContract.Quantity = quantity;

            dbContract.LastUpdatedAt = DateTime.Now;
            dbContract.LastUpdatedById = user.Id;

            dbContext.ApuContracts.Update(dbContract);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Fail();
        }
    }

    #endregion
}