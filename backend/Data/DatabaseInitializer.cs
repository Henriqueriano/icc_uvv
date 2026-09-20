using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using backend.Options;

namespace backend.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var authOptions = scope.ServiceProvider.GetRequiredService<AuthOptions>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = authOptions.DefaultUsername,
            Role = "RdfAdmin"
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, authOptions.DefaultPassword);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
