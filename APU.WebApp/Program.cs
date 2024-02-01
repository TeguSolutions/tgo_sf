using APU.DataV2.Context;
using APU.DataV2.Repositories;
using APU.WebApp.Services.Authentication;
using APU.WebApp.Services.Components;
using APU.WebApp.Services.Email;
using APU.WebApp.Services.JS;
using APU.WebApp.Services.Latency;
using APU.WebApp.Services.SignalRHub;
using APU.WebApp.Services.SignalRHub.HubClients.ProjectScheduleHub;
using APU.WebApp.Services.UserSession;
using Blazored.LocalStorage;
using BlazorFileSaver;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

static void MapAuthenticationServices(IServiceCollection services)
{
    services.AddScoped<CustomAuthenticationStateProvider>();
    services.AddScoped<AuthenticationStateProvider>(provider =>
        provider.GetRequiredService<CustomAuthenticationStateProvider>());

    services.AddScoped<ILocalTokenService, LocalTokenService>();
    services.AddScoped<JwtTokenService>();
}

static void MapDatabaseServices(IServiceCollection services, ConfigurationManager configuration)
{
    // Set up the DB Context class and associate with the DB Connection string
    services.AddDbContextFactory<ApuDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("EstimtorApu"))
                // For detailed Migration infos / values (only debug?)
                .EnableSensitiveDataLogging().EnableDetailedErrors(),
        // Default is Scoped, but it doesn't match for the SPA framework, kept the same context for the whole lifetime of the app
        ServiceLifetime.Transient);

    // Automatic Repository Registration
    var dataAssembly = typeof(IRepository).Assembly;
    var repositoryTypes = dataAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IRepository))).ToList();

    foreach (var repository in repositoryTypes)
        services.AddTransient(repository);
}

static void MapSignalRHubs(IServiceCollection services)
{
    services.AddTransient<ApuHubClient>();
    services.AddTransient<ProjectHubClient>();
    services.AddTransient<ProjectScheduleHubClient>();
    services.AddTransient<BaseItemHubClient>();
}

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// configure strongly typed settings object
//builder.Services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

// Add services to the container.
services.AddRazorPages();
services.AddServerSideBlazor().AddHubOptions(options => options.MaximumReceiveMessageSize = 64 * 1024);

MapAuthenticationServices(services);
MapDatabaseServices(services, configuration);
MapSignalRHubs(services);

// App service registrations
//services.AddScoped(serviceProvider => (IJSInProcessRuntime) serviceProvider.GetRequiredService<IJSRuntime>());
services.AddScoped<AppJS>();
services.AddSingleton<UserSessionService>();
services.AddScoped<PopupService>();

services.AddTransient<PingService>();
services.AddSingleton<LatencyService>();

// Email service
services.AddTransient<IEmailSender, SendGridEmailSender>();
services.Configure<SendGridEmailSenderOptions>(options =>
{
    options.ApiKey = builder.Configuration["SendGridApiKey"];
    options.SenderEmail = builder.Configuration["SendGridSenderEmail"];
    options.SenderName = builder.Configuration["SendGridSenderName"];
});

// 3rd Party service registrations
services.AddBlazoredLocalStorage();
services.AddBlazorFileSaver();
services.AddScoped<IEventAggregator, global::EventAggregator.Blazor.EventAggregator>();

SyncfusionLicenseProvider.RegisterLicense(configuration["SyncfusionLicenceKey"]);
services.AddSyncfusionBlazor();
//services.AddSyncfusionBlazor(options => { options.IgnoreScriptIsolation = true; });

services.AddScoped<TeguJsService>();

// SignalR
services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumParallelInvocationsPerClient = 1;
});

//builder.Logging.AddApplicationInsights();
//builder.Services.AddApplicationInsightsTelemetry();

// -------- Web Application --------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
//app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapHub<MainHub>(MainHub.Url);

#region User Session Service - Init

var dbContextFactoryService = app.Services.GetRequiredService<IDbContextFactory<ApuDbContext>>();
var userSessionService = app.Services.GetRequiredService<UserSessionService>();
userSessionService.Initialize(dbContextFactoryService);

#endregion

#region Data Seed

void DataSeed()
{
    var dbContext = dbContextFactoryService.CreateDbContext();
    var baseUser = dbContext.Users.FirstOrDefault(q => q.Id == Guid.Parse("9b21ca58-ca8f-4be0-9ac8-d14abd1c4f53"));
    if (baseUser is not null)
        return;

    var dbUser = new User
    {
        Id = Guid.Parse("9b21ca58-ca8f-4be0-9ac8-d14abd1c4f53"),
        Email = "dev@login.com",
        Name = "Dev Test",
        Initials = "DT",
        // 2022
        PasswordHash = "b1ab1e892617f210425f658cf1d361b5489028c8771b56d845fe1c62c1fbc8b0"
    };

    var userRole = new UserRole
    {
        UserId = Guid.Parse("9b21ca58-ca8f-4be0-9ac8-d14abd1c4f53"),
        RoleId = RoleDefinitions.Administrator.Id
    };

    var userSession = new UserSession
    {
        Id = Guid.Parse("be6365ae-53e7-4459-ac18-3c6c7c49e354"),
        UserId = Guid.Parse("9b21ca58-ca8f-4be0-9ac8-d14abd1c4f53"),
        CreatedAt = new DateTime(2023, 08, 01, 0, 0, 0),
        BlockProject = null,
        EstimatePageGridColumns = "",
        SelectedProjectId = null
    };

    dbContext.Users.Add(dbUser);
    dbContext.UserRoles.Add(userRole);
    dbContext.UserSessions.Add(userSession);

    dbContext.SaveChanges();

}

// DEBUG | RELEASE
DataSeed();


#endregion

app.Run();