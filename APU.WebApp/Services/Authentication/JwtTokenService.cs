using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using APU.DataV2.Context;
using APU.WebApp.Services.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace APU.WebApp.Services.Authentication;

public class JwtTokenService
{
    private const string nameIdentifier = "nameid";
    private const string fullNameClaim = "fullname";
    private const string initialsClaim = "initials";
    private const string emailAddress = "email";

    #region Injected Services

    private readonly ILogger<JwtTokenService> _logger;
    private readonly AppSettings _appSettings;
    private readonly ILocalTokenService _localTokenService;
    private readonly ApuDbContext _dbContext;

    #endregion

    #region Lifecycle

    public JwtTokenService(ILogger<JwtTokenService> logger, IConfiguration config,
        ILocalTokenService localTokenService, ApuDbContext dbContext)
    {
        _logger = logger;
        _localTokenService = localTokenService;
        _dbContext = dbContext;

        _appSettings = new AppSettings();
        _appSettings.JwtSecret = config["JwtSecret"];

        var jwtTokenTTLText = config["JwtTokenTTLMins"];
        if (int.TryParse(jwtTokenTTLText, out var jwtTokenTTL))
            _appSettings.JwtTokenTTLMins = jwtTokenTTL;
        else
            _appSettings.JwtTokenTTLMins = 60;

        var refreshTokenTTLText = config["RefreshTokenTTLDays"];
        if (int.TryParse(refreshTokenTTLText, out var refreshTokenTTL))
            _appSettings.RefreshTokenTTLDays = refreshTokenTTL;
        else
            _appSettings.RefreshTokenTTLDays = 7;

    }

    #endregion

    /// <summary>
    /// Removes the Jwt/Refresh Tokens from the DB and local storage too
    /// </summary>
    /// <returns></returns>
    public async Task DeleteTokensAsync()
    {
        // Step 1 - Delete the RefreshToken from DB
        var refreshToken = await _localTokenService.GetRefreshTokenAsync();
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            try
            {
                var dbRefreshToken = await _dbContext.UserRefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
                if (dbRefreshToken is not null)
                {
                    _dbContext.UserRefreshTokens.Remove(dbRefreshToken);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // Step 2 - Remove the tokens locally
        await _localTokenService.RemoveTokensAsync();
    }

    public async Task StoreTokensAsync(string jwtToken, string refreshToken)
    {
        await DeleteTokensAsync();
        await _localTokenService.SetTokensAsync(jwtToken, refreshToken);
    }

    public string GenerateJwtToken(Guid userId, string email, string fullName, string initials, List<string> roles)
    {
        try
        {
            var claimsIdentity = GetClaimsIdentity(userId.ToString(), email, fullName, initials, roles);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.JwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,

                // No whitespaces!!
                Issuer = "APU",
                Expires = DateTime.UtcNow.AddMinutes(_appSettings.JwtTokenTTLMins),
                //Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw new Exception(e.Message);
        }
    }

    public async Task<UserRefreshToken> GenerateRefreshToken(Guid userId, string ipAddress)
    {
        try
        {
            var secureRandomNumberGenerator = RandomNumberGenerator.Create();
            var randomBytes = new byte[64];
            secureRandomNumberGenerator.GetBytes(randomBytes);
            var refreshToken = new UserRefreshToken
            {
                CreatedAt = DateTimeOffset.UtcNow,
                UserId = userId,
                Token = Convert.ToBase64String(randomBytes),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_appSettings.RefreshTokenTTLDays)
                //ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(2)
            };

            await _dbContext.UserRefreshTokens.AddAsync(new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.Now,
                UserId = userId,
                Token = refreshToken.Token,
                ExpiresAt = refreshToken.ExpiresAt
            });
            await _dbContext.SaveChangesAsync();

            return refreshToken;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }
    }

    #region JwtToken Check and Renewal

    private async Task<(bool hasToken, string jwtToken, string refreshToken)> CheckStep1_GetLocalTokens()
    {
        var jwtToken = await _localTokenService.GetJwtTokenAsync();
        var refreshToken = await _localTokenService.GetRefreshTokenAsync();

        if (string.IsNullOrWhiteSpace(jwtToken) || string.IsNullOrWhiteSpace(refreshToken))
            return (false, null, null);
        return (true, jwtToken, refreshToken);
    }
    private bool CheckStep2_ValidateJwtToken(string jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.JwtSecret);

            tokenHandler.ValidateToken(jwtToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // Skip the lifetime - check it separately!
                ValidateLifetime = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            // ReSharper disable once UnusedVariable
            var validatedJwtToken = (JwtSecurityToken)validatedToken;

            return true;
        }
        catch (Exception e)
        {
            // return null if validation fails
            _logger.LogError(e.Message);
            return false;
        }
    }

    public async Task<(bool jwtTokenCheckResult, List<Claim> claims, bool failedRenewal)> CheckOrTryRefreshJwtToken()
    {
        var (hasToken, jwtToken, refreshToken) = await CheckStep1_GetLocalTokens();
        if (!hasToken)
            return (false, null, false);

        var jwtTokenIsValid = CheckStep2_ValidateJwtToken(jwtToken);
        if (!jwtTokenIsValid)
	        return (false, null, true);

        var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);

        var userIdText = jwtSecurityToken.Claims.First(c => c.Type == nameIdentifier).Value;
        var userId = Guid.Parse(userIdText);

        var dbRefreshToken = await _dbContext.UserRefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (dbRefreshToken is null || dbRefreshToken.IsExpired)
        {
            await DeleteTokensAsync();
            return (false, null, true);
        }

        // JwtToken is expired -> try to refresh
        if (jwtSecurityToken.ValidTo < DateTimeOffset.UtcNow)
        {
            var dbUser = await _dbContext.Users.Include(q =>q.UserRoles)
                                                .ThenInclude(p => p.Role)
                                                .FirstOrDefaultAsync(u => u.Id == userId);
            if (dbUser is null)
                return (false, null, true);
            
            if (dbUser.IsBlocked)
	            return (false, null, true);

            var roles = dbUser.UserRoles.Select(q => q.Role.Name).ToList();

            var newJwtToken = GenerateJwtToken(dbUser.Id, dbUser.Email, dbUser.Name, dbUser.Initials, roles);
            await _localTokenService.SetTokensAsync(newJwtToken, refreshToken);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, dbUser.Id.ToString()),
                new(ClaimTypes.Email, dbUser.Email),
                new(fullNameClaim, dbUser.Name),
                new(initialsClaim, dbUser.Initials)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            return (true, claims, false);
        }

        // jwtToken is still active
        var email = jwtSecurityToken.Claims.First(c => c.Type == emailAddress).Value;
        var fullName = jwtSecurityToken.Claims.First(c => c.Type == fullNameClaim).Value;
        var initials = jwtSecurityToken.Claims.First(c => c.Type == initialsClaim).Value;

        var activeClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userIdText),
            new(ClaimTypes.Email, email),
            new(fullNameClaim, fullName),
            new(initialsClaim, initials)
        };

        var roleClaims = jwtSecurityToken.Claims.Where(q => q.Type == "role");
        foreach (var roleClaim in roleClaims)
            activeClaims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));

        return (true, activeClaims, false);
    }

    #endregion



    #region Helper functions

    private static ClaimsIdentity GetClaimsIdentity(string userId, string email, string fullName, string initials, List<string> roles)
    {
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(fullNameClaim, fullName),
            new Claim(initialsClaim, initials)
        });

        foreach (var role in roles)
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));

        return claimsIdentity;
    }

    public List<Claim> GetClaimsFromJwtToken(string jwtToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(jwtToken);

            var claims = new List<Claim>
            {
                jwtSecurityToken.Claims.First(c => c.Type == nameIdentifier),
                jwtSecurityToken.Claims.First(c => c.Type == emailAddress),
                jwtSecurityToken.Claims.First(c => c.Type == fullNameClaim),
                jwtSecurityToken.Claims.First(c => c.Type == initialsClaim)
            };

            claims.AddRange(jwtSecurityToken.Claims.Where(q => q.Type == "role"));

            return claims;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return new List<Claim>();
        }
    }

    public async Task<(Result result, Guid? userId, string email, string fullName, string initials, List<string> roles)> GetLoggedInUser()
    {
        var jwtToken = await _localTokenService.GetJwtTokenAsync();
        if (string.IsNullOrWhiteSpace(jwtToken))
            return (Result.Fail(), null, "", "", "", new List<string>());

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(jwtToken);

            var userIdString = jwtSecurityToken.Claims.First(c => c.Type == nameIdentifier).Value;
            var userId = Guid.Parse(userIdString);

            var email = jwtSecurityToken.Claims.First(c => c.Type == emailAddress).Value;
            var fullName = jwtSecurityToken.Claims.First(c => c.Type == fullNameClaim).Value;
            var initials = jwtSecurityToken.Claims.First(c => c.Type == initialsClaim).Value;

            var roles = jwtSecurityToken.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();

            return (Result.Ok(), userId, email, fullName, initials, roles);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return (Result.Fail(), null, "", "", "", new List<string>());
        }
    }

    #endregion
}