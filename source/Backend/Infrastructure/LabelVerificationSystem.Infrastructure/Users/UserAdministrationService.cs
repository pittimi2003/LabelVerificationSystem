using System.Security.Cryptography;
using System.Text.Json;
using LabelVerificationSystem.Application.Contracts.Users;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.Users;
using LabelVerificationSystem.Domain.Entities.Auth;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabelVerificationSystem.Infrastructure.Users;

public sealed class UserAdministrationService : IUserAdministrationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _dbContext;

    public UserAdministrationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserListResponse> ListAsync(UserListQuery query, CancellationToken cancellationToken)
    {
        if (query.Page <= 0)
        {
            throw new AuthValidationException("page debe ser mayor o igual a 1.");
        }

        if (query.PageSize is < 1 or > 100)
        {
            throw new AuthValidationException("pageSize debe estar entre 1 y 100.");
        }

        var usersQuery = _dbContext.SystemUsers.AsNoTracking();

        var globalQuery = NormalizeFilter(query.Query);
        if (!string.IsNullOrWhiteSpace(globalQuery))
        {
            usersQuery = usersQuery.Where(x =>
                x.Username.ToLower().Contains(globalQuery)
                || x.DisplayName.ToLower().Contains(globalQuery)
                || (x.Email != null && x.Email.ToLower().Contains(globalQuery)));
        }

        var userId = NormalizeFilter(query.UserId);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            usersQuery = usersQuery.Where(x => x.UserId.ToLower().Contains(userId));
        }

        var username = NormalizeFilter(query.Username);
        if (!string.IsNullOrWhiteSpace(username))
        {
            usersQuery = usersQuery.Where(x => x.Username.ToLower().Contains(username));
        }

        var displayName = NormalizeFilter(query.DisplayName);
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            usersQuery = usersQuery.Where(x => x.DisplayName.ToLower().Contains(displayName));
        }

        var email = NormalizeFilter(query.Email);
        if (!string.IsNullOrWhiteSpace(email))
        {
            usersQuery = usersQuery.Where(x => x.Email != null && x.Email.ToLower().Contains(email));
        }

        var role = NormalizeFilter(query.Role);
        if (!string.IsNullOrWhiteSpace(role))
        {
            usersQuery = usersQuery.Where(x => x.RolesJson.ToLower().Contains(role));
        }

        var permission = NormalizeFilter(query.Permission);
        if (!string.IsNullOrWhiteSpace(permission))
        {
            usersQuery = usersQuery.Where(x => x.PermissionsJson.ToLower().Contains(permission));
        }

        if (query.IsActive.HasValue)
        {
            usersQuery = usersQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        var totalItems = await usersQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize);

        var users = await usersQuery
            .OrderBy(x => x.Username)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var items = users.Select(MapToListItem).ToList();

        return new UserListResponse(items, query.Page, query.PageSize, totalItems, totalPages);
    }

    public async Task<UserDetailDto> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        var normalizedUserId = ValidateRequiredTrimmed(userId, nameof(userId));

        var user = await _dbContext.SystemUsers.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == normalizedUserId, cancellationToken);
        if (user is null)
        {
            throw new AuthUnauthorizedException("Usuario no encontrado.");
        }

        return MapToDetail(user);
    }

    public async Task<UserDetailDto> CreateAsync(CreateUserRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        var username = ValidateRequiredTrimmed(request.Username, "username", minLength: 3, maxLength: 64);
        var displayName = ValidateRequiredTrimmed(request.DisplayName, "displayName", minLength: 3, maxLength: 128);
        var email = NormalizeOptionalEmail(request.Email);
        ValidatePassword(request.Password, "password");

        var usernameTaken = await _dbContext.SystemUsers.AnyAsync(x => x.Username == username, cancellationToken);
        if (usernameTaken)
        {
            throw new AuthConflictException("username ya está en uso.");
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailTaken = await _dbContext.SystemUsers.AnyAsync(x => x.Email == email, cancellationToken);
            if (emailTaken)
            {
                throw new AuthConflictException("email ya está en uso.");
            }
        }

        var nowUtc = DateTime.UtcNow;
        var userId = Guid.NewGuid().ToString("N");
        var user = new SystemUser
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Username = username,
            DisplayName = displayName,
            Email = email,
            IsActive = request.IsActive,
            RolesJson = SerializeList(NormalizeValues(request.Roles)),
            PermissionsJson = SerializeList(NormalizeValues(request.Permissions)),
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };

        _dbContext.SystemUsers.Add(user);
        _dbContext.UserPasswordCredentials.Add(new UserPasswordCredential
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PasswordHash = HashPassword(request.Password),
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
            UpdatedByIp = Truncate(ipAddress, 64),
            UpdatedByUserAgent = Truncate(userAgent, 512)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDetail(user);
    }

    public async Task<UserDetailDto> UpdateAsync(string userId, UpdateUserRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        var normalizedUserId = ValidateRequiredTrimmed(userId, nameof(userId));
        var displayName = ValidateRequiredTrimmed(request.DisplayName, "displayName", minLength: 3, maxLength: 128);
        var email = NormalizeOptionalEmail(request.Email);

        var user = await _dbContext.SystemUsers.FirstOrDefaultAsync(x => x.UserId == normalizedUserId, cancellationToken);
        if (user is null)
        {
            throw new AuthUnauthorizedException("Usuario no encontrado.");
        }

        if (!string.IsNullOrWhiteSpace(email) && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var emailTaken = await _dbContext.SystemUsers.AnyAsync(x => x.Email == email && x.UserId != normalizedUserId, cancellationToken);
            if (emailTaken)
            {
                throw new AuthConflictException("email ya está en uso.");
            }
        }

        var nowUtc = DateTime.UtcNow;
        user.DisplayName = displayName;
        user.Email = email;
        user.IsActive = request.IsActive;
        user.RolesJson = SerializeList(NormalizeValues(request.Roles));
        user.PermissionsJson = SerializeList(NormalizeValues(request.Permissions));
        user.UpdatedAtUtc = nowUtc;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            ValidatePassword(request.NewPassword, "newPassword");
            var credential = await _dbContext.UserPasswordCredentials.FirstOrDefaultAsync(x => x.UserId == normalizedUserId, cancellationToken);
            if (credential is null)
            {
                _dbContext.UserPasswordCredentials.Add(new UserPasswordCredential
                {
                    Id = Guid.NewGuid(),
                    UserId = normalizedUserId,
                    PasswordHash = HashPassword(request.NewPassword),
                    CreatedAtUtc = nowUtc,
                    UpdatedAtUtc = nowUtc,
                    UpdatedByIp = Truncate(ipAddress, 64),
                    UpdatedByUserAgent = Truncate(userAgent, 512)
                });
            }
            else
            {
                credential.PasswordHash = HashPassword(request.NewPassword);
                credential.UpdatedAtUtc = nowUtc;
                credential.UpdatedByIp = Truncate(ipAddress, 64);
                credential.UpdatedByUserAgent = Truncate(userAgent, 512);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDetail(user);
    }

    public async Task<UserDetailDto> SetActivationAsync(string userId, bool isActive, CancellationToken cancellationToken)
    {
        var normalizedUserId = ValidateRequiredTrimmed(userId, nameof(userId));
        var user = await _dbContext.SystemUsers.FirstOrDefaultAsync(x => x.UserId == normalizedUserId, cancellationToken);
        if (user is null)
        {
            throw new AuthUnauthorizedException("Usuario no encontrado.");
        }

        user.IsActive = isActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDetail(user);
    }

    private static UserListItemDto MapToListItem(SystemUser user)
        => new(
            user.UserId,
            user.Username,
            user.DisplayName,
            user.Email,
            user.IsActive,
            DeserializeList(user.RolesJson),
            DeserializeList(user.PermissionsJson),
            user.CreatedAtUtc,
            user.UpdatedAtUtc);

    private static UserDetailDto MapToDetail(SystemUser user)
        => new(
            user.UserId,
            user.Username,
            user.DisplayName,
            user.Email,
            user.IsActive,
            DeserializeList(user.RolesJson),
            DeserializeList(user.PermissionsJson),
            user.CreatedAtUtc,
            user.UpdatedAtUtc);

    private static string ValidateRequiredTrimmed(string value, string field, int minLength = 1, int maxLength = 256)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AuthValidationException($"{field} es obligatorio.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length < minLength || trimmed.Length > maxLength)
        {
            throw new AuthValidationException($"{field} debe tener entre {minLength} y {maxLength} caracteres.");
        }

        return trimmed;
    }

    private static string? NormalizeOptionalEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim().ToLowerInvariant();
        if (trimmed.Length > 256 || !trimmed.Contains('@'))
        {
            throw new AuthValidationException("email no es válido.");
        }

        return trimmed;
    }

    private static IReadOnlyList<string> NormalizeValues(IReadOnlyList<string>? values)
        => (values ?? [])
            .Select(x => x?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string SerializeList(IReadOnlyList<string> values) => JsonSerializer.Serialize(values, JsonOptions);

    private static IReadOnlyList<string> DeserializeList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static void ValidatePassword(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 8)
        {
            throw new AuthValidationException($"{field} debe tener mínimo 8 caracteres.");
        }

        var hasLetter = value.Any(char.IsLetter);
        var hasDigit = value.Any(char.IsDigit);
        if (!hasLetter || !hasDigit)
        {
            throw new AuthValidationException($"{field} debe contener al menos una letra y un número.");
        }
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string? NormalizeFilter(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
