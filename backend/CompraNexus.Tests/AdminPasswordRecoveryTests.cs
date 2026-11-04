using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using CompraNexus.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CompraNexus.Tests;

public class AdminPasswordRecoveryTests
{
    private const string Email = "admin@compranexus.com.do";
    private const string OldPassword = "Anterior-1234567!Original";
    private const string NewPassword = "NuevaClaveSegura#2026";

    private static async Task<(CompraNexusDbContext Db, AppUser User, IPasswordHasher<AppUser> Hasher)> CreateAsync(
        string role = ApplicationRoles.Administrator, bool active = true)
    {
        var db = new CompraNexusDbContext(
            new DbContextOptionsBuilder<CompraNexusDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options);
        IPasswordHasher<AppUser> hasher = new PasswordHasher<AppUser>();
        var user = new AppUser
        {
            Email = Email, NormalizedEmail = Email.ToUpperInvariant(),
            Role = role, IsActive = active, FailedAttempts = 0,
            LockoutEndUtc = DateTimeOffset.UtcNow.AddMinutes(15)
        };
        user.PasswordHash = hasher.HashPassword(user, OldPassword);
        db.AppUsers.Add(user);
        db.Departments.Add(new Department { Name = "Departamento conservado" });
        await db.SaveChangesAsync();
        return (db, user, hasher);
    }

    [Fact]
    public async Task Reset_RotatesHashAndSecurityStamp_ClearsLockout_WithoutAffectingOtherData()
    {
        var (db, user, hasher) = await CreateAsync();
        await using (db)
        {
            var previousHash = user.PasswordHash;
            var previousStamp = user.SecurityStamp;
            await AdminPasswordRecovery.ResetAsync(db, hasher, Email.ToUpperInvariant(), NewPassword);
            db.ChangeTracker.Clear();
            var updated = await db.AppUsers.SingleAsync();

            Assert.NotEqual(previousHash, updated.PasswordHash);
            Assert.NotEqual(previousStamp, updated.SecurityStamp);
            Assert.Equal(0, updated.FailedAttempts);
            Assert.Null(updated.LockoutEndUtc);
            Assert.True(updated.IsActive);
            Assert.Equal(ApplicationRoles.Administrator, updated.Role);
            Assert.Equal(PasswordVerificationResult.Failed,
                hasher.VerifyHashedPassword(updated, updated.PasswordHash, OldPassword));
            Assert.NotEqual(PasswordVerificationResult.Failed,
                hasher.VerifyHashedPassword(updated, updated.PasswordHash, NewPassword));
            Assert.Equal(1, await db.Departments.CountAsync());
        }
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("")]
    public async Task Reset_RejectsWeakPasswordsWithoutChanges(string password)
    {
        var (db, user, hasher) = await CreateAsync();
        await using (db)
        {
            var hash = user.PasswordHash;
            var stamp = user.SecurityStamp;
            await Assert.ThrowsAsync<ArgumentException>(
                () => AdminPasswordRecovery.ResetAsync(db, hasher, Email, password));

            Assert.Equal(hash, user.PasswordHash);
            Assert.Equal(stamp, user.SecurityStamp);
            Assert.NotNull(user.LockoutEndUtc);
        }
    }

    [Fact]
    public async Task Reset_RejectsPasswordsLongerThanMaximum()
    {
        var (db, user, hasher) = await CreateAsync();
        await using (db)
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => AdminPasswordRecovery.ResetAsync(db, hasher, Email, new string('a', 129)));
            Assert.NotNull(user.LockoutEndUtc);
        }
    }

    [Fact]
    public async Task Reset_DoesNotCreateMissingAdministrator()
    {
        var (db, user, hasher) = await CreateAsync();
        await using (db)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => AdminPasswordRecovery.ResetAsync(db, hasher, "desconocido@example.test", NewPassword));
            Assert.Single(await db.AppUsers.ToListAsync());
            Assert.NotNull(user.LockoutEndUtc);
        }
    }

    [Theory]
    [InlineData(ApplicationRoles.Purchases, true)]
    [InlineData(ApplicationRoles.Administrator, false)]
    public async Task Reset_RejectsNonAdministratorsAndInactiveUsers(string role, bool active)
    {
        var (db, user, hasher) = await CreateAsync(role, active);
        await using (db)
        {
            var originalHash = user.PasswordHash;
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => AdminPasswordRecovery.ResetAsync(db, hasher, Email, NewPassword));
            Assert.Equal(originalHash, user.PasswordHash);
            Assert.NotNull(user.LockoutEndUtc);
        }
    }
}
