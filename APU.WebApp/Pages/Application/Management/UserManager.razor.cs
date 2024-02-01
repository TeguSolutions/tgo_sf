using System.Collections.ObjectModel;
using APU.WebApp.Shared.Dialogs;
using APU.WebApp.Shared.FormClasses;
using APU.WebApp.Utils.Security;

namespace APU.WebApp.Pages.Application.Management;

[Authorize(Roles = RoleDefinitions.AdministratorText)]
public class UserManagerVM : PageVMBase
{
    #region Dialogs

    internal DlgConfirmation<User> DlgConfirmation { get; set; }
    internal DlgUserRegistration DlgUserRegistration { get; set; }

    #endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SendHeaderMessage(typeof(HeaderHome));
            await EventAggregator.PublishAsync(new HeaderLinkMessage());

            DlgConfirmation.Submit = DeleteUser;
            DlgUserRegistration.Submit = RegisterNewUser;

            await GetLIU();

            await GetUsersAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region Grid

    internal async void DataGridActionBegin(ActionEventArgs<User> args)
    {
        if (args.RequestType == SfGridAction.Save)
        {
            var result = await UpdateUserAsync(args.Data);
            if (!result.IsSuccess())
                args.Cancel = true;
        }
    }

    internal void DataGridCommandClick(CommandClickEventArgs<User> args)
    {
        var result = false;

        if (args.CommandColumn.Type == CommandButtonType.Delete)
            DlgConfirmation.Open("Delete User?", args.RowData.Name, args.RowData);

        else if (args.CommandColumn.Type == CommandButtonType.Save)
            result = true;

        else if (args.CommandColumn.Type == CommandButtonType.Edit)
            result = true;

        else if (args.CommandColumn.Type == CommandButtonType.Cancel)
            result = true;

        if (!result)
            args.Cancel = true;
    }

    #endregion

    #region DropDownButtons



    #endregion

    internal ObservableCollection<User> Users { get; set; } = new();

    private async Task GetUsersAsync()
    {
        ProgressStart();

        var userResult = await UserRepo.GetAllAsync();
        if (!userResult.IsSuccess())
        {
            ShowError("Failed to collect Users!");
            return;
        }

        Users = userResult.Data.OrderBy(u => u.Name).ToObservableCollection();
        foreach (var user in Users)
            user.SortRolesAscending();
        
        ProgressStop();
    }

    internal async void RegisterNewUser(FC_UserRegistration fcuser)
    {
        var now = DateTime.Now;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = fcuser.Name,
            Email = fcuser.Email,
            Initials = fcuser.Initials,
            PasswordHash = PasswordHash.ComputeSha256Hash(Guid.NewGuid().ToString()),
            LastUpdatedAt = now,
            LastUpdatedById = Liu.Id,
            UserRoles = new List<UserRole>()
        };

        var userSession = new UserSession
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = user.Id
        };

        ProgressStart();

        var registerResult = await UserRepo.CreateAsync(user, userSession);
        if (!registerResult.IsSuccess())
        {
            ShowError("Failed to register user!");
            return;
        }

        user.Sessions.Add(userSession);
        USS.AddSession(userSession);

        //user.LastUpdatedBy = Liu;

        Users.Add(user);
        Users = Users.OrderBy(u => u.Name).ToObservableCollection();

        ProgressStop();
    }


    private async Task<Result> UpdateUserAsync(User user)
    {
        ProgressStart();

        var result = await UserRepo.UpdateAsync(user, Liu);
        if (!result.IsSuccess())
        {
            ShowError("User update failed!");
            return Result.Fail();
        }

        ProgressStop();
        return Result.Ok();
    }

    internal async void UserAddRole(User user, Role role)
    {
        ProgressStart();

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
        };

        var result = await UserRepo.AddRoleAsync(userRole);
        if (!result.IsSuccess())
        {
            ShowError("Failed to add role!");
            ProgressStop();
            return;
        }

        userRole.User = user;
        userRole.Role = role;
        user.UserRoles.Add(userRole);
        user.SortRolesAscending();

        ProgressStop();
    }
    internal async void UserRemoveRole(User user, UserRole userRole)
    {
        ProgressStart();
        
        var result = await UserRepo.RemoveRoleAsync(userRole.UserId, userRole.RoleId);
        if (!result.IsSuccess())
        {
            ShowError("Failed to remove role!");
            ProgressStop();
            return;
        }

        user.UserRoles.Remove(userRole);

        ProgressStop();
    }


    private async void DeleteUser(User user)
    {
        ProgressStart();

        var deleteResult = await UserRepo.DeleteAsync(user.Id);
        if (!deleteResult.IsSuccess())
        {
            ShowError("Failed to delete User!");
            return;
        }

        Users.Remove(user);
        USS.RemoveSession(user.Id);

        ProgressStop();
    }

    internal async void CreateUserSession(User user)
    {
        ProgressStart();

        var sessionCheckResult = await UserRepo.GetUserSessionAsync(user.Id);
        if (!sessionCheckResult.IsSuccess())
        {
            ShowError("User Session Creation Fail!");
            return;
        }

        if (sessionCheckResult.Data is not null)
        {
            user.Sessions.Add(sessionCheckResult.Data);
            USS.AddSession(sessionCheckResult.Data);
            ProgressStop();
            return;
        }

        var userSession = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CreatedAt = DateTimeOffset.Now
        };

        var createResult = await UserRepo.CreateUserSessionAsync(userSession);
        if (!createResult.IsSuccess())
        {
            ShowError("User Session Creation Fail!");
            return;
        }

        user.Sessions.Add(userSession);
        USS.AddSession(userSession);

        ProgressStop();
    }


    internal List<Role> GetAvailableRoles(ICollection<UserRole> userRoles)
    {
        var availableRoles = new List<Role>();

        foreach (var role in RoleDefinitions.Collection)
        {
            if (userRoles.FirstOrDefault(q => q.RoleId == role.Id) == null)
                availableRoles.Add(role);
        }

        return availableRoles;
    }
}