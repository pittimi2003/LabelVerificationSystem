using System.Security.Cryptography;
using System.Text.Json;
using LabelVerificationSystem.Application.Contracts.Users;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.Users;
using LabelVerificationSystem.Domain.Entities.Auth;
using LabelVerificationSystem.Infrastructure.Authorization;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LabelVerificationSystem.Infrastructure.Users;

public sealed class UserAdministrationService : IUserAdministrationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _dbContext;
    private readonly AuthorizationRuntimeOptions _authorizationOptions;
    private static readonly string[] UsersManageActions = ["Create", "Edit", "ActivateDeactivate"];

    public UserAdministrationService(AppDbContext dbContext, IOptions<AuthorizationRuntimeOptions> authorizationOptions)
    {
        _dbContext = dbContext;
        _authorizationOptions = authorizationOptions.Value;
    }

    public async Task<IReadOnlyList<UserRoleCatalogItemDto>> ListRolesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.RoleCatalogs
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new UserRoleCatalogItemDto(x.Id, x.Code, x.Name, x.IsActive))
            .ToListAsync(cancellationToken);
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
        var cutoverEnabled = _authorizationOptions.RobustOnlyCutover.Enabled;
        var cutoverUserIds = _authorizationOptions.RobustOnlyCutover.UserIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

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
            usersQuery = usersQuery.Where(x =>
                _dbContext.SystemUserRoles.Any(r =>
                    r.SystemUserId == x.Id
                    && r.Role.IsActive
                    && r.Role.Code.ToLower().Contains(role))
                || (!_dbContext.SystemUserRoles.Any(r => r.SystemUserId == x.Id)
                    && (!cutoverEnabled || !cutoverUserIds.Contains(x.UserId))
                    && x.RolesJson.ToLower().Contains(role)));
        }

        var permission = NormalizeFilter(query.Permission);
        if (!string.IsNullOrWhiteSpace(permission))
        {
            var robustUserIds = await ResolveRobustPermissionUserIdsAsync(permission, cancellationToken);
            usersQuery = usersQuery.Where(x =>
                robustUserIds.Contains(x.Id)
                || ((!cutoverEnabled || !cutoverUserIds.Contains(x.UserId))
                    && x.PermissionsJson.ToLower().Contains(permission)));
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

        var roleMap = await ResolveEffectiveRolesAsync(users, cancellationToken);
        var permissionMap = await ResolveEffectivePermissionsAsync(users, roleMap, cancellationToken);
        var items = users.Select(user => MapToListItem(user, roleMap, permissionMap)).ToList();

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

        var roleMap = await ResolveEffectiveRolesAsync([user], cancellationToken);
        var permissionMap = await ResolveEffectivePermissionsAsync([user], roleMap, cancellationToken);
        return MapToDetail(user, roleMap, permissionMap);
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
            RolesJson = SerializeList([]),
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

        var normalizedRoles = NormalizeValues(request.Roles);
        var syncedRoles = await SyncUserRoleAssignmentsAsync(user, normalizedRoles, nowUtc, cancellationToken);
        UpdateLegacySnapshotsForTransition(user, syncedRoles, request.Permissions);

        await _dbContext.SaveChangesAsync(cancellationToken);
        var roleMap = await ResolveEffectiveRolesAsync([user], cancellationToken);
        var permissionMap = await ResolveEffectivePermissionsAsync([user], roleMap, cancellationToken);
        return MapToDetail(user, roleMap, permissionMap);
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
        var normalizedRoles = NormalizeValues(request.Roles);
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

        var syncedRoles = await SyncUserRoleAssignmentsAsync(user, normalizedRoles, nowUtc, cancellationToken);
        UpdateLegacySnapshotsForTransition(user, syncedRoles, request.Permissions);

        await _dbContext.SaveChangesAsync(cancellationToken);
        var roleMap = await ResolveEffectiveRolesAsync([user], cancellationToken);
        var permissionMap = await ResolveEffectivePermissionsAsync([user], roleMap, cancellationToken);
        return MapToDetail(user, roleMap, permissionMap);
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
        var roleMap = await ResolveEffectiveRolesAsync([user], cancellationToken);
        var permissionMap = await ResolveEffectivePermissionsAsync([user], roleMap, cancellationToken);
        return MapToDetail(user, roleMap, permissionMap);
    }

    private async Task<IReadOnlyList<string>> SyncUserRoleAssignmentsAsync(SystemUser user, IReadOnlyList<string> requestedRoleCodes, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var availableRoles = await _dbContext.RoleCatalogs
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        var availableByCode = availableRoles
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var targetRoles = requestedRoleCodes
            .Where(x => availableByCode.ContainsKey(x))
            .Select(x => availableByCode[x])
            .DistinctBy(x => x.Id)
            .ToList();

        if (targetRoles.Count == 0 && availableByCode.TryGetValue("Operators", out var fallbackRole))
        {
            targetRoles.Add(fallbackRole);
        }

        var targetRoleIds = targetRoles.Select(x => x.Id).ToHashSet();
        var currentMappings = await _dbContext.SystemUserRoles
            .Where(x => x.SystemUserId == user.Id)
            .ToListAsync(cancellationToken);

        foreach (var mapping in currentMappings.Where(x => !targetRoleIds.Contains(x.RoleId)))
        {
            _dbContext.SystemUserRoles.Remove(mapping);
        }

        foreach (var targetRole in targetRoles)
        {
            if (currentMappings.All(x => x.RoleId != targetRole.Id))
            {
                _dbContext.SystemUserRoles.Add(new SystemUserRole
                {
                    Id = Guid.NewGuid(),
                    SystemUserId = user.Id,
                    RoleId = targetRole.Id,
                    IsPrimary = false,
                    AssignedAtUtc = nowUtc
                });
            }
        }

        var primaryRoleId = targetRoles.FirstOrDefault()?.Id;

        foreach (var mapping in _dbContext.SystemUserRoles.Local.Where(x => x.SystemUserId == user.Id))
        {
            mapping.IsPrimary = primaryRoleId.HasValue && mapping.RoleId == primaryRoleId.Value;
        }

        foreach (var mapping in currentMappings)
        {
            if (_dbContext.Entry(mapping).State == EntityState.Deleted)
            {
                continue;
            }

            mapping.IsPrimary = primaryRoleId.HasValue && mapping.RoleId == primaryRoleId.Value;
        }

        return targetRoles.Select(x => x.Code).ToList();
    }

    private async Task<Dictionary<Guid, IReadOnlyList<string>>> ResolveEffectiveRolesAsync(IReadOnlyList<SystemUser> users, CancellationToken cancellationToken)
    {
        var result = users.ToDictionary(x => x.Id, _ => (IReadOnlyList<string>)[]);
        if (users.Count == 0)
        {
            return result;
        }

        var userIds = users.Select(x => x.Id).ToList();
        var assignedRoleRows = await _dbContext.SystemUserRoles
            .AsNoTracking()
            .Where(x => userIds.Contains(x.SystemUserId) && x.Role.IsActive)
            .Select(x => new { x.SystemUserId, x.Role.Code })
            .ToListAsync(cancellationToken);

        var grouped = assignedRoleRows
            .GroupBy(x => x.SystemUserId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<string>)x.Select(y => y.Code).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(y => y, StringComparer.OrdinalIgnoreCase).ToList());

        foreach (var user in users)
        {
            if (grouped.TryGetValue(user.Id, out var robustRoles) && robustRoles.Count > 0)
            {
                result[user.Id] = robustRoles;
                continue;
            }

            if (IsRobustOnlyCutoverUser(user.UserId))
            {
                result[user.Id] = [];
                continue;
            }

            result[user.Id] = DeserializeList(user.RolesJson);
        }

        return result;
    }

    private async Task<Dictionary<Guid, IReadOnlyList<string>>> ResolveEffectivePermissionsAsync(
        IReadOnlyList<SystemUser> users,
        IReadOnlyDictionary<Guid, IReadOnlyList<string>> roleMap,
        CancellationToken cancellationToken)
    {
        var result = users.ToDictionary(x => x.Id, _ => (IReadOnlyList<string>)[]);
        foreach (var user in users)
        {
            var isCutoverUser = IsRobustOnlyCutoverUser(user.UserId);
            if (roleMap.TryGetValue(user.Id, out var roleCodes) && roleCodes.Count > 0)
            {
                var robustPermissions = await ResolveRobustPermissionsFromRolesAsync(roleCodes, cancellationToken);
                if (isCutoverUser)
                {
                    result[user.Id] = robustPermissions;
                    continue;
                }

                var legacyPermissions = DeserializeList(user.PermissionsJson);
                result[user.Id] = robustPermissions
                    .Concat(legacyPermissions)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                continue;
            }

            if (isCutoverUser)
            {
                result[user.Id] = [];
                continue;
            }

            result[user.Id] = DeserializeList(user.PermissionsJson);
        }

        return result;
    }

    private static IReadOnlyList<string> BuildLegacyRolesSnapshot(IReadOnlyList<string> synchronizedRoles)
        => synchronizedRoles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private void UpdateLegacySnapshotsForTransition(
        SystemUser user,
        IReadOnlyList<string> synchronizedRoles,
        IReadOnlyList<string>? requestedPermissions)
    {
        if (IsRobustOnlyCutoverUser(user.UserId))
        {
            user.RolesJson = SerializeList([]);
            user.PermissionsJson = SerializeList([]);
            return;
        }

        user.RolesJson = SerializeList(BuildLegacyRolesSnapshot(synchronizedRoles));
        user.PermissionsJson = SerializeList(NormalizeValues(requestedPermissions));
    }

    private static UserListItemDto MapToListItem(
        SystemUser user,
        IReadOnlyDictionary<Guid, IReadOnlyList<string>> roleMap,
        IReadOnlyDictionary<Guid, IReadOnlyList<string>> permissionMap)
        => new(
            user.UserId,
            user.Username,
            user.DisplayName,
            user.Email,
            user.IsActive,
            roleMap.TryGetValue(user.Id, out var roles) ? roles : [],
            permissionMap.TryGetValue(user.Id, out var permissions) ? permissions : [],
            user.CreatedAtUtc,
            user.UpdatedAtUtc);

    private static UserDetailDto MapToDetail(
        SystemUser user,
        IReadOnlyDictionary<Guid, IReadOnlyList<string>> roleMap,
        IReadOnlyDictionary<Guid, IReadOnlyList<string>> permissionMap)
        => new(
            user.UserId,
            user.Username,
            user.DisplayName,
            user.Email,
            user.IsActive,
            roleMap.TryGetValue(user.Id, out var roles) ? roles : [],
            permissionMap.TryGetValue(user.Id, out var permissions) ? permissions : [],
            user.CreatedAtUtc,
            user.UpdatedAtUtc);

    private bool IsRobustOnlyCutoverUser(string userId)
    {
        var cutover = _authorizationOptions.RobustOnlyCutover;
        if (!cutover.Enabled)
        {
            return false;
        }

        return cutover.UserIds.Any(x => string.Equals(x?.Trim(), userId, StringComparison.OrdinalIgnoreCase));
    }

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

    private async Task<HashSet<Guid>> ResolveRobustPermissionUserIdsAsync(string normalizedPermission, CancellationToken cancellationToken)
    {
        var candidatePermissions = GetKnownPermissionClaims()
            .Where(x => x.Contains(normalizedPermission, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidatePermissions.Count == 0)
        {
            return [];
        }

        IQueryable<Guid> query = _dbContext.SystemUserRoles
            .AsNoTracking()
            .Where(x => x.Role.IsActive)
            .Select(x => x.SystemUserId)
            .Where(_ => false);

        if (candidatePermissions.Contains("users.read", StringComparer.OrdinalIgnoreCase))
        {
            query = query.Union(
                from userRole in _dbContext.SystemUserRoles.AsNoTracking()
                where userRole.Role.IsActive
                join moduleAuthorization in _dbContext.RoleModuleAuthorizations.AsNoTracking()
                    on userRole.RoleId equals moduleAuthorization.RoleId
                join actionAuthorization in _dbContext.RoleModuleActionAuthorizations.AsNoTracking()
                    on userRole.RoleId equals actionAuthorization.RoleId
                where moduleAuthorization.Module.IsActive
                      && moduleAuthorization.Module.Code == "UsersAdministration"
                      && moduleAuthorization.Authorized
                      && actionAuthorization.ModuleAction.IsActive
                      && actionAuthorization.ModuleAction.Module.IsActive
                      && actionAuthorization.ModuleAction.Module.Code == "UsersAdministration"
                      && actionAuthorization.ModuleAction.Code == "View"
                      && actionAuthorization.Authorized
                select userRole.SystemUserId);
        }

        if (candidatePermissions.Contains("users.manage", StringComparer.OrdinalIgnoreCase))
        {
            query = query.Union(
                from userRole in _dbContext.SystemUserRoles.AsNoTracking()
                where userRole.Role.IsActive
                join moduleAuthorization in _dbContext.RoleModuleAuthorizations.AsNoTracking()
                    on userRole.RoleId equals moduleAuthorization.RoleId
                join actionAuthorization in _dbContext.RoleModuleActionAuthorizations.AsNoTracking()
                    on userRole.RoleId equals actionAuthorization.RoleId
                where moduleAuthorization.Module.IsActive
                      && moduleAuthorization.Module.Code == "UsersAdministration"
                      && moduleAuthorization.Authorized
                      && actionAuthorization.ModuleAction.IsActive
                      && actionAuthorization.ModuleAction.Module.IsActive
                      && actionAuthorization.ModuleAction.Module.Code == "UsersAdministration"
                      && UsersManageActions.Contains(actionAuthorization.ModuleAction.Code)
                      && actionAuthorization.Authorized
                select userRole.SystemUserId);
        }

        if (candidatePermissions.Contains("authorization.matrix.manage", StringComparer.OrdinalIgnoreCase))
        {
            query = query.Union(
                from userRole in _dbContext.SystemUserRoles.AsNoTracking()
                where userRole.Role.IsActive
                join moduleAuthorization in _dbContext.RoleModuleAuthorizations.AsNoTracking()
                    on userRole.RoleId equals moduleAuthorization.RoleId
                join actionAuthorization in _dbContext.RoleModuleActionAuthorizations.AsNoTracking()
                    on userRole.RoleId equals actionAuthorization.RoleId
                where moduleAuthorization.Module.IsActive
                      && moduleAuthorization.Module.Code == "AuthorizationMatrixAdministration"
                      && moduleAuthorization.Authorized
                      && actionAuthorization.ModuleAction.IsActive
                      && actionAuthorization.ModuleAction.Module.IsActive
                      && actionAuthorization.ModuleAction.Module.Code == "AuthorizationMatrixAdministration"
                      && actionAuthorization.ModuleAction.Code == "Manage"
                      && actionAuthorization.Authorized
                select userRole.SystemUserId);
        }

        if (candidatePermissions.Contains("excel.uploads.read", StringComparer.OrdinalIgnoreCase))
        {
            query = query.Union(
                from userRole in _dbContext.SystemUserRoles.AsNoTracking()
                where userRole.Role.IsActive
                join moduleAuthorization in _dbContext.RoleModuleAuthorizations.AsNoTracking()
                    on userRole.RoleId equals moduleAuthorization.RoleId
                join actionAuthorization in _dbContext.RoleModuleActionAuthorizations.AsNoTracking()
                    on userRole.RoleId equals actionAuthorization.RoleId
                where moduleAuthorization.Module.IsActive
                      && moduleAuthorization.Module.Code == "ExcelUploads"
                      && moduleAuthorization.Authorized
                      && actionAuthorization.ModuleAction.IsActive
                      && actionAuthorization.ModuleAction.Module.IsActive
                      && actionAuthorization.ModuleAction.Module.Code == "ExcelUploads"
                      && actionAuthorization.ModuleAction.Code == "View"
                      && actionAuthorization.Authorized
                select userRole.SystemUserId);
        }

        if (candidatePermissions.Contains("excel.upload.create", StringComparer.OrdinalIgnoreCase))
        {
            query = query.Union(
                from userRole in _dbContext.SystemUserRoles.AsNoTracking()
                where userRole.Role.IsActive
                join moduleAuthorization in _dbContext.RoleModuleAuthorizations.AsNoTracking()
                    on userRole.RoleId equals moduleAuthorization.RoleId
                join actionAuthorization in _dbContext.RoleModuleActionAuthorizations.AsNoTracking()
                    on userRole.RoleId equals actionAuthorization.RoleId
                where moduleAuthorization.Module.IsActive
                      && moduleAuthorization.Module.Code == "ExcelUploads"
                      && moduleAuthorization.Authorized
                      && actionAuthorization.ModuleAction.IsActive
                      && actionAuthorization.ModuleAction.Module.IsActive
                      && actionAuthorization.ModuleAction.Module.Code == "ExcelUploads"
                      && actionAuthorization.ModuleAction.Code == "Upload"
                      && actionAuthorization.Authorized
                select userRole.SystemUserId);
        }

        return await query.Distinct().ToHashSetAsync(cancellationToken);
    }

    private static IReadOnlyList<string> GetKnownPermissionClaims()
        => ["users.read", "users.manage", "authorization.matrix.manage", "excel.uploads.read", "excel.upload.create"];

    private async Task<IReadOnlyList<string>> ResolveRobustPermissionsFromRolesAsync(IReadOnlyList<string> roleCodes, CancellationToken cancellationToken)
    {
        if (roleCodes.Count == 0)
        {
            return [];
        }

        var usersReadAllowed = await HasAuthorizedActionAsync(roleCodes, "UsersAdministration", "View", cancellationToken);
        var usersManageAllowed = await HasAuthorizedActionAsync(roleCodes, "UsersAdministration", UsersManageActions, cancellationToken);
        var authorizationMatrixManageAllowed = await HasAuthorizedActionAsync(roleCodes, "AuthorizationMatrixAdministration", "Manage", cancellationToken);
        var excelUploadsReadAllowed = await HasAuthorizedActionAsync(roleCodes, "ExcelUploads", "View", cancellationToken);
        var excelUploadsUploadAllowed = await HasAuthorizedActionAsync(roleCodes, "ExcelUploads", "Upload", cancellationToken);

        var permissions = new List<string>();
        if (usersReadAllowed)
        {
            permissions.Add("users.read");
        }

        if (usersManageAllowed)
        {
            permissions.Add("users.manage");
        }

        if (authorizationMatrixManageAllowed)
        {
            permissions.Add("authorization.matrix.manage");
        }

        if (excelUploadsReadAllowed)
        {
            permissions.Add("excel.uploads.read");
        }

        if (excelUploadsUploadAllowed)
        {
            permissions.Add("excel.upload.create");
        }

        return permissions;
    }

    private async Task<bool> HasAuthorizedActionAsync(
        IReadOnlyList<string> roleCodes,
        string moduleCode,
        string actionCode,
        CancellationToken cancellationToken)
        => await HasAuthorizedActionAsync(roleCodes, moduleCode, [actionCode], cancellationToken);

    private async Task<bool> HasAuthorizedActionAsync(
        IReadOnlyList<string> roleCodes,
        string moduleCode,
        IReadOnlyList<string> actionCodes,
        CancellationToken cancellationToken)
    {
        return await _dbContext.RoleModuleActionAuthorizations
            .AsNoTracking()
            .AnyAsync(x =>
                x.Role.IsActive
                && roleCodes.Contains(x.Role.Code)
                && x.Authorized
                && x.ModuleAction.IsActive
                && actionCodes.Contains(x.ModuleAction.Code)
                && x.ModuleAction.Module.IsActive
                && x.ModuleAction.Module.Code == moduleCode
                && _dbContext.RoleModuleAuthorizations.Any(moduleAuth =>
                    moduleAuth.RoleId == x.RoleId
                    && moduleAuth.ModuleId == x.ModuleAction.ModuleId
                    && moduleAuth.Module.IsActive
                    && moduleAuth.Authorized),
                cancellationToken);
    }
}
