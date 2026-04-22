namespace LabelVerificationSystem.Api.Auth;

public static class AuthAuthorizationPolicies
{
    public const string UsersRead = "UsersRead";
    public const string UsersCreate = "UsersCreate";
    public const string UsersEdit = "UsersEdit";
    public const string UsersActivateDeactivate = "UsersActivateDeactivate";
    public const string AuthorizationMatrixManage = "AuthorizationMatrixManage";
}

public static class AuthModules
{
    public const string UsersAdministration = "UsersAdministration";
    public const string AuthorizationMatrixAdministration = "AuthorizationMatrixAdministration";
}

public static class AuthModuleActions
{
    public const string View = "View";
    public const string Create = "Create";
    public const string Edit = "Edit";
    public const string ActivateDeactivate = "ActivateDeactivate";
    public const string Manage = "Manage";
}

public static class AuthPermissionClaims
{
    public const string Type = "permission";
    public const string UsersRead = "users.read";
    public const string UsersManage = "users.manage";
    public const string AuthorizationMatrixManage = "authorization.matrix.manage";
}
