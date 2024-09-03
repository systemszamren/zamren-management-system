namespace zamren_management_system.Areas.Security.Interfaces;

public interface IRoleService
{
    /// <summary>
    ///     This method is used to check if a role has a privilege
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="privilegeId"></param>
    /// <returns></returns>
    Task<bool> HasPrivilegeAsync(string roleId, string privilegeId);
}