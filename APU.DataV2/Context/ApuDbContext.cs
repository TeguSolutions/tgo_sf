using APU.DataV2.Definitions;

namespace APU.DataV2.Context;

public partial class ApuDbContext : DbContext
{
    #region Lifecycle

    public ApuDbContext()
    {
    }

    public ApuDbContext(DbContextOptions<ApuDbContext> options) : base(options)
    {
    }    

    #endregion

    public DbSet<User> Users { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    public DbSet<UserPasswordRecoveryLink> UserPasswordRecoveryLinks { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Role> Roles { get; set; }

    // Base Definitions
    public DbSet<DefaultValue> DefaultValues { get; set; }

    public DbSet<City> Cities { get; set; }
    public DbSet<County> Counties { get; set; }
    public DbSet<Trade> Trades { get; set; }
    public DbSet<Holiday> Holidays { get; set; }

    // Project
    public DbSet<Project> Projects { get; set; }

    public DbSet<ProjectSchedule> ProjectSchedules { get; set; }

    // Apu
    public DbSet<Apu> Apus { get; set; }

    public DbSet<ApuPerformance> ApuPerformances { get; set; }
    public DbSet<ApuMaterial> ApuMaterials { get; set; }
    public DbSet<ApuEquipment> ApuEquipments { get; set; }
    public DbSet<ApuLabor> ApuLabors { get; set; }
    public DbSet<ApuContract> ApuContracts { get; set; }

    // Base Items
    public DbSet<BasePerformance> BasePerformances { get; set; }
    public DbSet<BaseLabor> BaseLabors { get; set; }
    public DbSet<BaseMaterial> BaseMaterials { get; set; }
    public DbSet<BaseEquipment> BaseEquipments { get; set; }
    public DbSet<BaseContract> BaseContracts { get; set; }

    // Item Definitions
    public DbSet<ApuStatus> ApuStatuses { get; set; }
    public DbSet<ItemType> ItemTypes { get; set; }
    public DbSet<LaborWorkType> LaborWorkTypes { get; set; }

    // Others
    public DbSet<Vendor> Contractors { get; set; }
    public DbSet<VendorType> ContractorTypes { get; set; }



    // Main
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<Municipality> Municipalities { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var relationship in builder.Model.GetEntityTypes().Where(e => !e.IsOwned())
                     .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
        }

        // Default Values
        builder.Entity<Apu>().Property(b => b.Code).HasDefaultValue("");
        builder.Entity<Apu>().Property(b => b.Unit).HasDefaultValue("");
        builder.Entity<Apu>().Property(b => b.LaborNotes).HasDefaultValue("");
        builder.Entity<Apu>().Property(b => b.MaterialNotes).HasDefaultValue("");
        builder.Entity<Apu>().Property(b => b.EquipmentNotes).HasDefaultValue("");
        builder.Entity<Apu>().Property(b => b.ContractNotes).HasDefaultValue("");

        // MtM Relations
        builder.Entity<UserRole>().HasKey(e => new { e.UserId, e.RoleId });
        
        // Data Seed
        foreach (var role in RoleDefinitions.Collection)
        {
            builder.Entity<Role>().HasData(role);
        }

        foreach (var apuStatus in ApuStatusDefinitions.Collection)
        {
            builder.Entity<ApuStatus>().HasData(apuStatus);
        }

        foreach (var itemType in ItemTypeDefinitions.Collection)
        {
            builder.Entity<ItemType>().HasData(itemType);
        }

        foreach (var laborWorkType in LaborWorkTypeDefinitions.Collection)
        {
            builder.Entity<LaborWorkType>().HasData(laborWorkType);
        }

        builder.Entity<DefaultValue>().HasData(
            new DefaultValue
            {
                Id = 1,
                Gross = 35,
                Supervision = 30,
                Tools = 3,
                WorkersComp = (decimal)12.5,
                Fica = (decimal)7.65,
                TopFica = 65000,
                FutaSuta = 231,
                SalesTax = 7,
                Bond = 2,
                HrsDay = 7
            }
        );

        //#region User seed

        //var userId = Guid.Parse("9b21ca58-ca8f-4be0-9ac8-d14abd1c4f53");
        //var userSessionId = Guid.Parse("be6365ae-53e7-4459-ac18-3c6c7c49e354");

        //builder.Entity<User>().HasData(
        //    new User
        //    {
        //        Id = userId,
        //        Email = "unused@aa.bb",
        //        Name = "Test User",
        //        Initials = "TU",
        //        // 2022
        //        PasswordHash = "b1ab1e892617f210425f658cf1d361b5489028c8771b56d845fe1c62c1fbc8b0",
        //        LastUpdatedAt = new DateTime(2023, 08, 01, 0, 0, 0),
        //        IsBlocked = false
        //    }
        //);

        //builder.Entity<UserRole>().HasData(
        //    new UserRole
        //    {
        //        UserId = userId,
        //        RoleId = RoleDefinitions.Administrator.Id
        //    });

        //builder.Entity<UserSession>().HasData(
        //    new UserSession
        //    {
        //        Id = userSessionId,
        //        UserId = userId,
        //        CreatedAt = new DateTime(2023, 08, 01, 0, 0, 0),
        //        BlockProject = null,
        //        EstimatePageGridColumns = "",
        //        SelectedProjectId = null
        //    });

        //#endregion

        OnModelCreatingPartial(builder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}