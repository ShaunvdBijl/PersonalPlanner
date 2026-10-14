using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using PersonalPlanner.Data;
using PersonalPlanner.Data.Models;

namespace PersonalPlanner.Services;

public record AuthCrossCheckResult(bool Succeeded, string? ErrorMessage, AppUser? User);

public class FirebaseAuthService(IDbContextFactory<AppDbContext> dbFactory, ILogger<FirebaseAuthService> logger)
{
    public async Task<AuthCrossCheckResult> VerifyAndCrossCheckUserAsync(
        string idToken,
        bool isSignUp,
        string? displayName = null)
    {
        try
        {
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            string uid = decodedToken.Uid;
            string email = decodedToken.Claims.TryGetValue("email", out var emailObj)
                ? emailObj?.ToString() ?? string.Empty
                : string.Empty;
            string resolvedName = !string.IsNullOrWhiteSpace(displayName)
                ? displayName
                : (decodedToken.Claims.TryGetValue("name", out var nameObj) ? nameObj?.ToString() ?? email : email);

            await using var db = await dbFactory.CreateDbContextAsync();

            var existingUser = await db.Users
                .Include(u => u.Categories)
                .FirstOrDefaultAsync(u => u.FirebaseUid == uid || u.Email == email);

            if (existingUser is null)
            {
                if (!isSignUp)
                {
                    return new AuthCrossCheckResult(
                        false,
                        "Account not found in the PersonalPlanner database. Please switch to Sign Up to register your profile.",
                        null);
                }

                var newUser = new AppUser
                {
                    FirebaseUid = uid,
                    Email = email,
                    DisplayName = resolvedName,
                    CreatedAtUtc = DateTime.UtcNow,
                    LastLoginUtc = DateTime.UtcNow,
                    Categories = Category.DefaultPresets.Select(p => new Category
                    {
                        Name = p.Name,
                        ColorHex = p.ColorHex,
                        IsPreset = true
                    }).ToList()
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();
                return new AuthCrossCheckResult(true, null, newUser);
            }

            // Cross-check succeeded: synchronize UID if matched by email and update LastLoginUtc
            if (existingUser.FirebaseUid != uid)
            {
                existingUser.FirebaseUid = uid;
            }
            existingUser.LastLoginUtc = DateTime.UtcNow;

            // Ensure preset categories exist for this user
            if (existingUser.Categories.Count == 0)
            {
                foreach (var preset in Category.DefaultPresets)
                {
                    db.Categories.Add(new Category
                    {
                        UserId = existingUser.Id,
                        Name = preset.Name,
                        ColorHex = preset.ColorHex,
                        IsPreset = true
                    });
                }
            }

            await db.SaveChangesAsync();
            return new AuthCrossCheckResult(true, null, existingUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Firebase authentication or PostgreSQL cross-check failed.");
            return new AuthCrossCheckResult(false, "Authentication verification failed. Please check your credentials.", null);
        }
    }
}