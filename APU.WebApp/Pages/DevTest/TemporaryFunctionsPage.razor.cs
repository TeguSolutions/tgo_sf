using APU.DataV2.Context;
using Microsoft.EntityFrameworkCore;

namespace APU.WebApp.Pages.DevTest;

[Authorize(Roles = RD.AdministratorText)]
public class TemporaryFunctionsVM : PageVMBase
{
    [Inject]
    public IDbContextFactory<ApuDbContext> DbContextFactory { get; set; }

    internal string Message { get; set; }

    internal async void AssignCreatedAt()
    {
        Message = "";

        ProgressStart();

        try
        {
            var dbContext = await DbContextFactory.CreateDbContextAsync();

            var apuLabors = await dbContext.ApuLabors.ToListAsync();
            var apuMaterials = await dbContext.ApuMaterials.ToListAsync();
            var apuEquipments = await dbContext.ApuEquipments.ToListAsync();
            var apuContracts = await dbContext.ApuContracts.ToListAsync();

            var updatedApuLabors = new List<ApuLabor>();
            var updatedApuMaterials = new List<ApuMaterial>();
            var updatedApuEquipments = new List<ApuEquipment>();
            var updatedApuContracts = new List<ApuContract>();

            foreach (var apuLabor in apuLabors)
            {
                if (apuLabor.CreatedAt == DateTime.MinValue)
                {
                    apuLabor.CreatedAt = apuLabor.LastUpdatedAt;
                    updatedApuLabors.Add(apuLabor);
                }
            }

            foreach (var apuMaterial in apuMaterials)
            {
                if (apuMaterial.CreatedAt == DateTime.MinValue)
                {
                    apuMaterial.CreatedAt = apuMaterial.LastUpdatedAt;
                    updatedApuMaterials.Add(apuMaterial);
                }
            }

            foreach (var apuEquipment in apuEquipments)
            {
                if (apuEquipment.CreatedAt == DateTime.MinValue)
                {
                    apuEquipment.CreatedAt = apuEquipment.LastUpdatedAt;
                    updatedApuEquipments.Add(apuEquipment);
                }
            }

            foreach (var apuContract in apuContracts)
            {
                if (apuContract.CreatedAt == DateTime.MinValue)
                {
                    apuContract.CreatedAt = apuContract.LastUpdatedAt;
                    updatedApuContracts.Add(apuContract);
                }
            }

            dbContext.ApuLabors.UpdateRange(updatedApuLabors);
            dbContext.ApuMaterials.UpdateRange(updatedApuMaterials);
            dbContext.ApuEquipments.UpdateRange(updatedApuEquipments);
            dbContext.ApuContracts.UpdateRange(updatedApuContracts);

            await dbContext.SaveChangesAsync();

            Message =
                $"ApuLabors: {updatedApuLabors.Count} | ApuMaterials: {updatedApuMaterials.Count} | ApuEquipments: {updatedApuEquipments.Count} | ApuContracts: {updatedApuContracts.Count}";

            ShowSuccess("Apu Items CreatedAt assigned successfully!");
        }
        catch (Exception e)
        {
            ShowError(e.Message);
        }
    }
}