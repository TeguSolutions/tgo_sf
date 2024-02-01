using Blazored.LocalStorage;

namespace APU.WebApp.Services.Authentication;

public class LocalTokenService : ILocalTokenService
{
    private const string liu_jwttoken = "liu_jwttoken";
    private const string liu_refreshtoken = "liu_refreshtoken";

    private readonly ILogger<LocalTokenService> _logger;
    private readonly ILocalStorageService _lss;

    public LocalTokenService(ILogger<LocalTokenService> logger, ILocalStorageService lss)
    {
        _logger = logger;
        _lss = lss;
    }

    public async Task<string> GetJwtTokenAsync()
    {
        var jwt = await _lss.GetItemAsync<string>(liu_jwttoken);
        return jwt;
    }

    public async Task<string> GetRefreshTokenAsync()
    {
        var refreshToken = await _lss.GetItemAsync<string>(liu_refreshtoken);
        return refreshToken;
    }

    public async Task SetTokensAsync(string jwtToken, string refreshToken)
    {
        await _lss.SetItemAsync(liu_jwttoken, jwtToken);
        await _lss.SetItemAsync(liu_refreshtoken, refreshToken);
    }


    public async Task RemoveTokensAsync()
    {
        await _lss.RemoveItemAsync(liu_jwttoken);
        await _lss.RemoveItemAsync(liu_refreshtoken);
    }
}