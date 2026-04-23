using LabelVerificationSystem.Application.Contracts.Authorization;
using LabelVerificationSystem.Infrastructure.Authorization;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LabelVerificationSystem.IntegrationTests;

public sealed class AuthorizationAdministrationServiceTests
{
    [Fact]
    public async Task UpdateRoleMatrixAsync_ReplacesMalformedAuthorizationRows_WithoutConcurrencyException()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();

            var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var moduleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var actionId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            await setupContext.Database.ExecuteSqlRawAsync(
                "INSERT INTO RoleCatalog (Id, Code, Name, IsActive, CreatedAtUtc, UpdatedAtUtc) VALUES ({0}, {1}, {2}, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                roleId,
                "Operators",
                "Operators");

            await setupContext.Database.ExecuteSqlRawAsync(
                "INSERT INTO ModuleCatalog (Id, Code, Name, Description, DisplayOrder, IsActive, CreatedAtUtc, UpdatedAtUtc) VALUES ({0}, {1}, {2}, NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                moduleId,
                "UsersAdministration",
                "Users Administration");

            await setupContext.Database.ExecuteSqlRawAsync(
                "INSERT INTO ModuleActionCatalog (Id, ModuleId, Code, Name, Description, DisplayOrder, IsActive, CreatedAtUtc, UpdatedAtUtc) VALUES ({0}, {1}, {2}, {3}, NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                actionId,
                moduleId,
                "View",
                "View");

            // Simula filas históricas con Id en formato 32 hex sin guiones (origen de la excepción de concurrencia).
            await setupContext.Database.ExecuteSqlRawAsync(
                "INSERT INTO RoleModuleAuthorization (Id, RoleId, ModuleId, Authorized, CreatedAtUtc, UpdatedAtUtc) VALUES ('aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa', {0}, {1}, 0, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                roleId,
                moduleId);

            await setupContext.Database.ExecuteSqlRawAsync(
                "INSERT INTO RoleModuleActionAuthorization (Id, RoleId, ModuleActionId, Authorized, CreatedAtUtc, UpdatedAtUtc) VALUES ('bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb', {0}, {1}, 0, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                roleId,
                actionId);
        }

        await using (var context = new AppDbContext(options))
        {
            var sut = new AuthorizationAdministrationService(context);

            var request = new UpdateRoleAuthorizationMatrixRequest(
                new List<UpdateRoleModuleAuthorizationDto>
                {
                    new(
                        Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        true,
                        new List<UpdateRoleModuleActionAuthorizationDto>
                        {
                            new(Guid.Parse("33333333-3333-3333-3333-333333333333"), true)
                        })
                });

            var result = await sut.UpdateRoleMatrixAsync("Operators", request, CancellationToken.None);

            Assert.Single(result.Modules);
            Assert.True(result.Modules[0].ModuleAuthorized);
            Assert.Single(result.Modules[0].Actions);
            Assert.True(result.Modules[0].Actions[0].Authorized);
        }

        await using (var assertContext = new AppDbContext(options))
        {
            var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var moduleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var actionId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            var moduleRow = await assertContext.RoleModuleAuthorizations
                .SingleAsync(x => x.RoleId == roleId && x.ModuleId == moduleId);
            var actionRow = await assertContext.RoleModuleActionAuthorizations
                .SingleAsync(x => x.RoleId == roleId && x.ModuleActionId == actionId);

            Assert.True(moduleRow.Authorized);
            Assert.True(actionRow.Authorized);

            await using var cmd = assertContext.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"
SELECT
    (SELECT COUNT(*) FROM RoleModuleAuthorization WHERE Id = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa') AS ModuleMalformedCount,
    (SELECT COUNT(*) FROM RoleModuleActionAuthorization WHERE Id = 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb') AS ActionMalformedCount;";

            await assertContext.Database.OpenConnectionAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            Assert.Equal(0, reader.GetInt32(0));
            Assert.Equal(0, reader.GetInt32(1));
        }
    }
}
