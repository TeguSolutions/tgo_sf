using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    public partial class Initv3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApuStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApuStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LaborWorkTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaborWorkTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPasswordRecoveryLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPasswordRecoveryLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Initials = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseContracts_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaseEquipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseEquipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseEquipments_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaseLabors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HrsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HrsStandardYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HrsOvertimeYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VacationsDays = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HolydaysYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SickDaysYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BonusYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HealthYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LifeInsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Percentage401 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MeetingsHrsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OfficeHrsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TrainingHrsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UniformsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SafetyYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseLabors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseLabors_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaseMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseMaterials_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BasePerformances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Hours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasePerformances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasePerformances_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Counties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Counties_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DefaultValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Gross = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Supervision = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tools = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WorkersComp = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fica = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TopFica = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FutaSuta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalesTax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Bond = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HrsDay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefaultValues_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    BlockProject = table.Column<bool>(type: "bit", nullable: true),
                    SelectedProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstimatePageGridColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Web = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Building = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Bids = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BidSync = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CountyId = table.Column<int>(type: "int", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Counties_CountyId",
                        column: x => x.CountyId,
                        principalTable: "Counties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cities_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Zip = table.Column<int>(type: "int", nullable: true),
                    Estimator = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Gross = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Supervision = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tools = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Bond = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalesTax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossLabor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossMaterials = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossEquipment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossContracts = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    CountyId = table.Column<int>(type: "int", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Projects_Counties_CountyId",
                        column: x => x.CountyId,
                        principalTable: "Counties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Projects_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Apus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsBlocked = table.Column<byte>(type: "tinyint", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LaborNotes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MaterialNotes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EquipmentNotes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ContractNotes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SuperPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LaborGrossPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaterialGrossPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EquipmentGrossPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubcontractorGrossPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Apus_ApuStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ApuStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Apus_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Apus_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApuContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    ItemTypeId = table.Column<int>(type: "int", nullable: false),
                    ApuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApuContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApuContracts_Apus_ApuId",
                        column: x => x.ApuId,
                        principalTable: "Apus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuContracts_BaseContracts_BaseContractId",
                        column: x => x.BaseContractId,
                        principalTable: "BaseContracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuContracts_ItemTypes_ItemTypeId",
                        column: x => x.ItemTypeId,
                        principalTable: "ItemTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuContracts_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApuEquipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    ItemTypeId = table.Column<int>(type: "int", nullable: false),
                    ApuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseEquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApuEquipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApuEquipments_Apus_ApuId",
                        column: x => x.ApuId,
                        principalTable: "Apus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuEquipments_BaseEquipments_BaseEquipmentId",
                        column: x => x.BaseEquipmentId,
                        principalTable: "BaseEquipments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuEquipments_ItemTypes_ItemTypeId",
                        column: x => x.ItemTypeId,
                        principalTable: "ItemTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuEquipments_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApuLabors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HrsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HrsStandardYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HrsOvertimeYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VacationsDays = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HolydaysYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SickDaysYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BonusYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HealthYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LifeInsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Percentage401 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MeetingsHrsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OfficeHrsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TrainingHrsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UniformsYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SafetyYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WorkTypeId = table.Column<int>(type: "int", nullable: false),
                    BaseLaborId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApuLabors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApuLabors_Apus_ApuId",
                        column: x => x.ApuId,
                        principalTable: "Apus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuLabors_BaseLabors_BaseLaborId",
                        column: x => x.BaseLaborId,
                        principalTable: "BaseLabors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuLabors_LaborWorkTypes_WorkTypeId",
                        column: x => x.WorkTypeId,
                        principalTable: "LaborWorkTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuLabors_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApuMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Waste = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    ItemTypeId = table.Column<int>(type: "int", nullable: false),
                    ApuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseMaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApuMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApuMaterials_Apus_ApuId",
                        column: x => x.ApuId,
                        principalTable: "Apus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuMaterials_BaseMaterials_BaseMaterialId",
                        column: x => x.BaseMaterialId,
                        principalTable: "BaseMaterials",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuMaterials_ItemTypes_ItemTypeId",
                        column: x => x.ItemTypeId,
                        principalTable: "ItemTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuMaterials_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApuPerformances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Hours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BasePerformanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApuPerformances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApuPerformances_Apus_ApuId",
                        column: x => x.ApuId,
                        principalTable: "Apus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuPerformances_BasePerformances_BasePerformanceId",
                        column: x => x.BasePerformanceId,
                        principalTable: "BasePerformances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApuPerformances_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "ApuStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Progress" },
                    { 2, "Review" },
                    { 3, "Ready" }
                });

            migrationBuilder.InsertData(
                table: "DefaultValues",
                columns: new[] { "Id", "Bond", "Fica", "FutaSuta", "Gross", "HrsDay", "LastUpdatedAt", "LastUpdatedById", "SalesTax", "Supervision", "Tools", "TopFica", "WorkersComp" },
                values: new object[] { 1, 2m, 7.65m, 231m, 35m, 7m, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 7m, 30m, 3m, 65000m, 12.5m });

            migrationBuilder.InsertData(
                table: "ItemTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Total" },
                    { 2, "By Unit" }
                });

            migrationBuilder.InsertData(
                table: "LaborWorkTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Hours" },
                    { 2, "Workers" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApuContracts_ApuId",
                table: "ApuContracts",
                column: "ApuId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuContracts_BaseContractId",
                table: "ApuContracts",
                column: "BaseContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuContracts_ItemTypeId",
                table: "ApuContracts",
                column: "ItemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuContracts_LastUpdatedById",
                table: "ApuContracts",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApuEquipments_ApuId",
                table: "ApuEquipments",
                column: "ApuId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuEquipments_BaseEquipmentId",
                table: "ApuEquipments",
                column: "BaseEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuEquipments_ItemTypeId",
                table: "ApuEquipments",
                column: "ItemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuEquipments_LastUpdatedById",
                table: "ApuEquipments",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApuLabors_ApuId",
                table: "ApuLabors",
                column: "ApuId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuLabors_BaseLaborId",
                table: "ApuLabors",
                column: "BaseLaborId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuLabors_LastUpdatedById",
                table: "ApuLabors",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApuLabors_WorkTypeId",
                table: "ApuLabors",
                column: "WorkTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuMaterials_ApuId",
                table: "ApuMaterials",
                column: "ApuId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuMaterials_BaseMaterialId",
                table: "ApuMaterials",
                column: "BaseMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuMaterials_ItemTypeId",
                table: "ApuMaterials",
                column: "ItemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuMaterials_LastUpdatedById",
                table: "ApuMaterials",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApuPerformances_ApuId",
                table: "ApuPerformances",
                column: "ApuId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuPerformances_BasePerformanceId",
                table: "ApuPerformances",
                column: "BasePerformanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApuPerformances_LastUpdatedById",
                table: "ApuPerformances",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Apus_LastUpdatedById",
                table: "Apus",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Apus_ProjectId",
                table: "Apus",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Apus_StatusId",
                table: "Apus",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseContracts_LastUpdatedById",
                table: "BaseContracts",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEquipments_LastUpdatedById",
                table: "BaseEquipments",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BaseLabors_LastUpdatedById",
                table: "BaseLabors",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BaseMaterials_LastUpdatedById",
                table: "BaseMaterials",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BasePerformances_LastUpdatedById",
                table: "BasePerformances",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountyId",
                table: "Cities",
                column: "CountyId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_LastUpdatedById",
                table: "Cities",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Counties_LastUpdatedById",
                table: "Counties",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultValues_LastUpdatedById",
                table: "DefaultValues",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CityId",
                table: "Projects",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CountyId",
                table: "Projects",
                column: "CountyId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_LastUpdatedById",
                table: "Projects",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_UserId",
                table: "UserRefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApuContracts");

            migrationBuilder.DropTable(
                name: "ApuEquipments");

            migrationBuilder.DropTable(
                name: "ApuLabors");

            migrationBuilder.DropTable(
                name: "ApuMaterials");

            migrationBuilder.DropTable(
                name: "ApuPerformances");

            migrationBuilder.DropTable(
                name: "DefaultValues");

            migrationBuilder.DropTable(
                name: "UserPasswordRecoveryLinks");

            migrationBuilder.DropTable(
                name: "UserRefreshTokens");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "BaseContracts");

            migrationBuilder.DropTable(
                name: "BaseEquipments");

            migrationBuilder.DropTable(
                name: "BaseLabors");

            migrationBuilder.DropTable(
                name: "LaborWorkTypes");

            migrationBuilder.DropTable(
                name: "BaseMaterials");

            migrationBuilder.DropTable(
                name: "ItemTypes");

            migrationBuilder.DropTable(
                name: "Apus");

            migrationBuilder.DropTable(
                name: "BasePerformances");

            migrationBuilder.DropTable(
                name: "ApuStatuses");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Counties");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
