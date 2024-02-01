namespace APU.WebApp.Services.Settings;

public class AppSettings
{
    public string JwtSecret { get; set; }

    // JwtToken Time to Live (in minutes)
    public int JwtTokenTTLMins { get; set; }

    // RefreshToken Time to Live (in days)
    public int RefreshTokenTTLDays { get; set; }
}