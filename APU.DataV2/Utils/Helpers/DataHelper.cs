using APU.DataV2.Definitions;

namespace APU.DataV2.Utils.Helpers;

public static class DataHelper
{
    public static void CopyApu_BaseValues(Apu sourceApu, Apu targetApu)
    {
        targetApu.IsBlocked = sourceApu.IsBlocked;
        targetApu.Code = sourceApu.Code;
        //targetApu.Description = sourceApu.Description + "_Copy";
        //targetApu.Unit = sourceApu.Unit;
        //targetApu.Quantity = sourceApu.Quantity;
        //targetApu.GroupId = sApu.GroupId;
        //targetApu.ItemId = sApu.ItemId;

        targetApu.LaborNotes = sourceApu.LaborNotes;
        targetApu.MaterialNotes = sourceApu.MaterialNotes;
        targetApu.EquipmentNotes = sourceApu.EquipmentNotes;
        targetApu.ContractNotes = sourceApu.ContractNotes;

        targetApu.SuperPercentage = sourceApu.SuperPercentage;
        targetApu.LaborGrossPercentage = sourceApu.LaborGrossPercentage;
        targetApu.MaterialGrossPercentage = sourceApu.MaterialGrossPercentage;
        targetApu.EquipmentGrossPercentage = sourceApu.EquipmentGrossPercentage;
        targetApu.SubcontractorGrossPercentage = sourceApu.SubcontractorGrossPercentage;

        targetApu.StatusId = ApuStatusDefinitions.Progress.Id;
    }



    // Project Duplication

    public static Project GetProject(Project sProject, DateTime lastUpdatedAt, Guid liuId)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            IsBlocked = false,
            ProjectName = sProject.ProjectName + " Copy",
            Owner = sProject.Owner,
            Phone = sProject.Phone,
            Email = sProject.Email,

            Address = sProject.Address,
            State = sProject.State,
            Zip = sProject.Zip,
            Estimator = sProject.Estimator,
            Link = sProject.Link,

            Gross = sProject.Gross,
            Supervision = sProject.Supervision,
            Tools = sProject.Tools,
            Bond = sProject.Bond,
            SalesTax = sProject.SalesTax,
            GrossLabor = sProject.GrossLabor,
            GrossMaterials = sProject.GrossMaterials,
            GrossEquipment = sProject.GrossEquipment,
            GrossContracts = sProject.GrossContracts,

            CityId = sProject.CityId,
            //CountyId = sProject.CountyId,

            LastUpdatedAt = lastUpdatedAt,
            LastUpdatedById = liuId
        };

        return project;
    }

    public static Apu GetBaseApu(Apu sApu, Guid projectId, DateTime lastUpdatedAt, Guid liuId)
    {
        var apu = new Apu
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,

            GroupId = sApu.GroupId,
            ItemId = sApu.ItemId,
            Description = sApu.Description,

            StatusId = ApuStatusDefinitions.Ready.Id,

            LastUpdatedAt = lastUpdatedAt,
            LastUpdatedById = liuId
        };

        return apu;
    }

    public static Apu GetLineItemApu(Apu sApu, Guid projectId, DateTime lastUpdatedAt, Guid liuId)
    {
        var apu = new Apu
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,

            GroupId = sApu.GroupId,
            ItemId = sApu.ItemId,

            Description = sApu.Description,
            Unit = sApu.Unit,
            Quantity = sApu.Quantity,

            IsBlocked = false,
            Code = sApu.Code,

            LaborNotes = sApu.LaborNotes,
            MaterialNotes = sApu.MaterialNotes,
            EquipmentNotes = sApu.EquipmentNotes,
            ContractNotes = sApu.ContractNotes,

            SuperPercentage = sApu.SuperPercentage,
            LaborGrossPercentage = sApu.LaborGrossPercentage,
            MaterialGrossPercentage = sApu.MaterialGrossPercentage,
            EquipmentGrossPercentage = sApu.EquipmentGrossPercentage,
            SubcontractorGrossPercentage = sApu.SubcontractorGrossPercentage,

            StatusId = sApu.StatusId,

            LastUpdatedAt = lastUpdatedAt,
            LastUpdatedById = liuId
        };

        return apu;
    }


    // New Apu Items

    public static ApuPerformance GetNewApuPerformance(ApuPerformance sPerformance, Guid apuId, DateTime lastUpdatedAt, Guid liuId)
    {
        var performance = new ApuPerformance
        {
            Id = Guid.NewGuid(),
            ApuId = apuId,
            BasePerformanceId = sPerformance.BasePerformanceId,

            Description = sPerformance.Description,
            Value = sPerformance.Value,
            Hours = sPerformance.Hours,

            LastUpdatedAt = lastUpdatedAt,
            LastUpdatedById = liuId
        };

        return performance;
    }

    public static ApuLabor GetNewApuLabor(ApuLabor sApuLabor, Guid apuId, DateTime lastUpdatedAt, Guid liuId)
    {
        var labor = new ApuLabor
        {
            Id = Guid.NewGuid(),
            ApuId = apuId,
            BaseLaborId = sApuLabor.BaseLaborId,

            Description = sApuLabor.Description,
            Quantity = sApuLabor.Quantity,
            Salary = sApuLabor.Salary,
            HrsYear = sApuLabor.HrsYear,
            HrsStandardYear = sApuLabor.HrsStandardYear,
            HrsOvertimeYear = sApuLabor.HrsOvertimeYear,
            VacationsDays = sApuLabor.VacationsDays,
            HolydaysYear = sApuLabor.HolydaysYear,
            SickDaysYear = sApuLabor.SickDaysYear,
            BonusYear = sApuLabor.BonusYear,
            HealthYear = sApuLabor.HealthYear,
            LifeInsYear = sApuLabor.LifeInsYear,
            Percentage401 = sApuLabor.Percentage401,
            MeetingsHrsYear = sApuLabor.MeetingsHrsYear,
            OfficeHrsYear = sApuLabor.OfficeHrsYear,
            TrainingHrsYear = sApuLabor.TrainingHrsYear,
            UniformsYear = sApuLabor.UniformsYear,
            SafetyYear = sApuLabor.SafetyYear,

            WorkTypeId = sApuLabor.WorkTypeId,

            LastUpdatedAt = lastUpdatedAt,
            LastUpdatedById = liuId
        };

        return labor;
    }

    public static ApuMaterial GetNewApuMaterial(ApuMaterial sApuMaterial, Guid apuId, DateTime lastUpdatedAt, Guid liuId)
    {
        var material = new ApuMaterial
        {
            Id = Guid.NewGuid(),
            ApuId = apuId,
            BaseMaterialId = sApuMaterial.BaseMaterialId,

            Description = sApuMaterial.Description,
            Unit = sApuMaterial.Unit,
            Quantity = sApuMaterial.Quantity,
            Waste = sApuMaterial.Waste,
            Price = sApuMaterial.Price,
            Vendor = sApuMaterial.Vendor,
            Phone = sApuMaterial.Phone,
            Link = sApuMaterial.Link,

            ItemTypeId = sApuMaterial.ItemTypeId,

            LastUpdatedAt = lastUpdatedAt,
            LastUpdatedById = liuId
        };

        return material;
    }

    public static ApuEquipment GetNewApuEquipment(ApuEquipment sApuEquipment, Guid apuId, DateTime lastUpdatedAt, Guid liuId)
    {
        var equipment = new ApuEquipment
        {
            Id = Guid.NewGuid(),
            ApuId = apuId,
            BaseEquipmentId = sApuEquipment.BaseEquipmentId,

            Description = sApuEquipment.Description,
            Unit = sApuEquipment.Unit,
            Quantity = sApuEquipment.Quantity,
            Price = sApuEquipment.Price,
            Vendor = sApuEquipment.Vendor,
            Phone = sApuEquipment.Phone,
            Link = sApuEquipment.Link,

            ItemTypeId = sApuEquipment.ItemTypeId,

            LastUpdatedAt = lastUpdatedAt,
            LastUpdatedById = liuId
        };

        return equipment;
    }

    public static ApuContract GetNewApuContract(ApuContract sApuContract, Guid apuId, DateTime lastUpdatedAt, Guid liuId)
    {
        var contract = new ApuContract
        {
            Id = Guid.NewGuid(),
            ApuId = apuId,
            BaseContractId = sApuContract.BaseContractId,

            Description = sApuContract.Description,
            Unit = sApuContract.Unit,
            Quantity = sApuContract.Quantity,
            Price = sApuContract.Price,
            Vendor = sApuContract.Vendor,
            Phone = sApuContract.Phone,
            Link = sApuContract.Link,

            ItemTypeId = sApuContract.ItemTypeId,

            LastUpdatedAt = lastUpdatedAt,
            LastUpdatedById = liuId
        };

        return contract;
    }


    // Copy Apu Item Properties
}