namespace MiniApi.Application.Shared;

public static class Permissions
{
    public static class Account
    {

        public const string AddRole = "Account.AddRole";

        public static List<string> All = new()
        {
          AddRole
        };
    }

    public static class Role
    {
        public const string GetAllPermissions = "Role.GetAllPermissions";
        public const string Create = "Role.Create";

        public static List<string> All = new()
        {
            GetAllPermissions,
            Create
        };
    }
}
