using System.Text.Json.Serialization;
using APU.DataV2.Definitions;

namespace APU.DataV2.Entities;

public partial class User
{
    [NotMapped]
    public bool HasSession => Sessions?.Count > 0;

    public void SortRolesAscending()
    {
        if (UserRoles is null)
            return;

        UserRoles = UserRoles.OrderBy(q => q.Role.Id).ToList();
    }

    #region Local Role Helpers from JWT Claims

    [NotMapped]
    [JsonIgnore]
    public List<string> Roles { get; set; }

    [JsonIgnore]
    public bool IsAdministrator => Roles?.Contains(RoleDefinitions.AdministratorText) ?? false;
    [JsonIgnore]
    public bool IsCostManager => Roles?.Contains(RoleDefinitions.CostManagerText) ?? false;
    [JsonIgnore]
    public bool IsEstimator => Roles?.Contains(RoleDefinitions.EstimatorText) ?? false;
    [JsonIgnore]
    public bool IsSupervisor => Roles?.Contains(RoleDefinitions.SupervisorText) ?? false;

    #endregion
}