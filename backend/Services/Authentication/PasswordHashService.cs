using backend.Data;
using Microsoft.AspNetCore.Identity;

namespace backend.Services.Authentication;

public sealed class PasswordHashService
{
    public const int WorkFactor = 4;
    private readonly PasswordHasher<User> _legacyHasher = new();

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(User user, string password, out bool requiresUpgrade)
    {
        requiresUpgrade = false;

        if (user.PasswordHash.StartsWith("$2", StringComparison.Ordinal))
        {
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        var legacyResult = _legacyHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (legacyResult == PasswordVerificationResult.Success
            || legacyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            requiresUpgrade = true;
            return true;
        }

        return false;
    }
}
