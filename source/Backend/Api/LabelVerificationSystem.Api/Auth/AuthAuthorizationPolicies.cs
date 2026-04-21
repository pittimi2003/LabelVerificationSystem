namespace LabelVerificationSystem.Api.Auth;

public static class AuthAuthorizationPolicies
{
    public const string UsersRead = "UsersRead";
    public const string UsersManage = "UsersManage";
}

public static class AuthModules
{
    public const string UsersAdministration = "UsersAdministration";
}

public static class AuthModuleActions
{
    public const string View = "View";
    public const string Edit = "Edit";
}

public static class AuthPermissionClaims
{
    public const string Type = "permission";
    public const string UsersRead = "users.read";
    public const string UsersManage = "users.manage";
}
