using System.Collections.ObjectModel;

namespace APU.DataV2.Definitions;

public static class RoleDefinitions
{
    static RoleDefinitions()
    {
        Administrator = new Role
        {
            Id = 100,
            Name = "Administrator"
        };

        Supervisor = new Role
        {
            Id = 60,
            Name = "Supervisor"
        };

        Estimator = new Role
        {
            Id = 40,
            Name = "Estimator"
        };

        CostManager = new Role
        {
            Id = 20,
            Name = "Cost Manager"
        };

        Collection = new ObservableCollection<Role>
        {
            Administrator,
            Supervisor,
            Estimator,
            CostManager
        };
    }

    public static ObservableCollection<Role> Collection { get; }

    public static Role Administrator { get; }
    public static Role Supervisor { get; }
    public static Role Estimator { get; }
    public static Role CostManager { get; }

    public const string AdministratorText = "Administrator";
    public const string SupervisorText = "Supervisor";
    public const string EstimatorText = "Estimator";
    public const string CostManagerText = "Cost Manager";
}