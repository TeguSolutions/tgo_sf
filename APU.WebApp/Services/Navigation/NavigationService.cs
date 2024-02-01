namespace APU.WebApp.Services.Navigation;

public class NavS
{
    public static string Login => "/login";
    public static string ForgotPassword => "/forgotpassword";
    public static string PasswordRecovery => "/passwordrecovery";
    public static string Index => "/";


    // NavigationHelpers
    public static string LogoutAndNavigatetoLogin => "/logoutandnavigatetologin";


    public class Home
    {
        public static string UserManager => "/users";
        public static string Municipalities => "/municipalities";
        public static string Certificates => "/certificates";
    }

    public class Estimates
    {
        public static string Base => "/estimates";

        public static string APU => $"{Base}/apu";
        public static string APUView => $"{Base}/apuview";
        public static string Bids => $"{Base}/bids"; 
        public static string Contracts => $"{Base}/contracts";
        public static string Defaults => $"{Base}/defaults";
        public static string Equipments => $"{Base}/equipments";
        public static string Estimate => $"{Base}/estimate";
        public static string Items => $"{Base}/items";
        public static string Labor => $"{Base}/labor";
        public static string Materials => $"{Base}/materials";
        public static string Performance => $"{Base}/performance";
        public static string Vendors => $"{Base}/vendors";
        public static string Definitions => $"{Base}/definitions";

        public static string Schedule => $"{Base}/schedule";
    }
}