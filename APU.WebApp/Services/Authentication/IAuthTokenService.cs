namespace APU.WebApp.Services.Authentication;

public interface ILocalTokenService
{
    Task<string> GetJwtTokenAsync();
    Task<string> GetRefreshTokenAsync();

    Task SetTokensAsync(string jwtToken, string refreshToken);
    Task RemoveTokensAsync();
}