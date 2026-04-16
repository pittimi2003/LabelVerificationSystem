namespace LabelVerificationSystem.Application.Interfaces.Auth;

public sealed class AuthValidationException : Exception
{
    public AuthValidationException(string message) : base(message) { }
}

public sealed class AuthUnauthorizedException : Exception
{
    public AuthUnauthorizedException(string message) : base(message) { }
}

public sealed class AuthConflictException : Exception
{
    public AuthConflictException(string message) : base(message) { }
}
