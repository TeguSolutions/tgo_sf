using System.Diagnostics;
using System.Security.Claims;
using APU.DataV2.Context;
using APU.WebApp.Services.Navigation;
using APU.WebApp.Utils.Security;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace APU.WebApp.Services.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    #region Injected Services

    private const string auth_type = "auth_type";

    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private readonly NavigationManager _navm;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IDbContextFactory<ApuDbContext> _dbContextFactory;

    #endregion

    #region Constructor & Initialization

    public CustomAuthenticationStateProvider(ILogger<CustomAuthenticationStateProvider> logger, 
        NavigationManager navm,
        JwtTokenService jwtTokenService,
        IDbContextFactory<ApuDbContext> dbContextFactory)
    {
        _logger = logger;
        _navm = navm;
        _jwtTokenService = jwtTokenService;
        _dbContextFactory = dbContextFactory;
    }

    #endregion

    /// <summary>
    /// Optionally call it explicitly when Authentication needed
    /// </summary>
    public async Task<bool> CheckAuthenticationState()
    {
        var (jwtTokenCheckResult, _, failedRenewal) = await _jwtTokenService.CheckOrTryRefreshJwtToken();
        if (!jwtTokenCheckResult)
        {
            if (failedRenewal)
                _navm.NavigateTo(NavS.LogoutAndNavigatetoLogin);

            return false;
        }

        return true;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var (jwtTokenCheckResult, claims, failedRenewal) = await _jwtTokenService.CheckOrTryRefreshJwtToken();
            if (!jwtTokenCheckResult)
            {
                if (failedRenewal)
                    _navm.NavigateTo("logoutandnavigatetologin");

                return await ReturnEmptyAuthState();
            }

            var identity = new ClaimsIdentity(new Claim[]{}, auth_type);
            identity.AddClaims(claims);

            var principal = new ClaimsPrincipal(identity);
            var authTask = Task.FromResult(new AuthenticationState(principal));
            return await authTask;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            _logger.LogError(e.Message);
            return await ReturnEmptyAuthState();
        }
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _jwtTokenService.DeleteTokensAsync();

        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        var task = Task.FromResult(new AuthenticationState(user));

        NotifyAuthenticationStateChanged(task);
    }

    public async Task<(bool success, string message, Guid? userId, string email)> Authenticate(string email, string password)
    {
        try
        {
            #region User Authentication & Branch

            var passwordHash = PasswordHash.ComputeSha256Hash(password);

            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbUser = await dbContext.Users.Include(q =>q.UserRoles)
                                                .ThenInclude(p => p.Role)
                                                .SingleOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);
            if (dbUser is null)
	            return (false, "", null, null);
            
            if (dbUser.IsBlocked)
                return (false, "User account is blocked!", null, null);

            #endregion

            #region Token Management

            var roles = dbUser.UserRoles.Select(q => q.Role.Name).ToList();

            var jwtToken = _jwtTokenService.GenerateJwtToken(dbUser.Id, dbUser.Email, dbUser.Name, dbUser.Initials, roles);
            var refreshToken = await _jwtTokenService.GenerateRefreshToken(dbUser.Id, "");
            await _jwtTokenService.StoreTokensAsync(jwtToken, refreshToken.Token);

            #endregion

            #region Framework Auth

            var claims = _jwtTokenService.GetClaimsFromJwtToken(jwtToken);

            var identity = new ClaimsIdentity(new Claim[] { }, auth_type);
            identity.AddClaims(claims);

            var userclaim = new ClaimsPrincipal(identity);
            var task = Task.FromResult(new AuthenticationState(userclaim));

            NotifyAuthenticationStateChanged(task);

            #endregion

            return (true, "", dbUser.Id, dbUser.Email);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return (false, "",  null, null);
        }
    }

    #region Helper

    private async Task<AuthenticationState> ReturnEmptyAuthState()
    {
        var identityEmpty = new ClaimsIdentity();
        var principalEmpty = new ClaimsPrincipal(identityEmpty);
        var authTaskEmpty = Task.FromResult(new AuthenticationState(principalEmpty));
        return await authTaskEmpty;
    }

    #endregion
}