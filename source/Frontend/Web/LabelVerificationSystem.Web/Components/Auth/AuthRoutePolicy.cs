namespace LabelVerificationSystem.Web.Components.Auth;

public static class AuthRoutePolicy
{
    private static readonly HashSet<string> PublicRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/signin",
        "/signin-basic",
        "/signup",
        "/reset-password",
        "/error",
        "/error401"
    };

    public static bool IsProtected(string? relativePath)
    {
        var normalized = NormalizePath(relativePath);
        return !PublicRoutes.Contains(normalized);
    }

    public static bool IsPublic(string? relativePath) => !IsProtected(relativePath);

    public static bool IsSignInRoute(string? relativePath)
    {
        var normalized = NormalizePath(relativePath);
        return string.Equals(normalized, "/signin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "/signin-basic", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return "/";
        }

        var path = relativePath.Split('?', '#')[0].Trim();
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        if (path.Length > 1 && path.EndsWith('/'))
        {
            path = path.TrimEnd('/');
        }

        return path;
    }
}
