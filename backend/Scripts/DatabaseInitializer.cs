using backend.Data;
using backend.Options;
using backend.Services.Authentication;
using Microsoft.EntityFrameworkCore;

namespace backend.Scripts;

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
        var passwordHashService = scope.ServiceProvider.GetRequiredService<PasswordHashService>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = authOptions.DefaultUsername,
            Role = "RdfAdmin"
        };
        user.PasswordHash = passwordHashService.Hash(authOptions.DefaultPassword);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedAdminAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var authOptions = scope.ServiceProvider.GetRequiredService<AuthOptions>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<PasswordHashService>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        var user = await dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Username == authOptions.DefaultUsername, cancellationToken);

        if (user is not null)
        {
            if (user.Role != "RdfAdmin" || !user.IsActive)
            {
                user.Role = "RdfAdmin";
                user.IsActive = true;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        user = new User
        {
            Id = Guid.NewGuid(),
            Username = authOptions.DefaultUsername,
            Role = "RdfAdmin",
            IsActive = true
        };
        user.PasswordHash = passwordHashService.Hash(authOptions.DefaultPassword);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
