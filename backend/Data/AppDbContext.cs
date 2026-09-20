using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.Username).HasColumnName("username").IsRequired();
            entity.HasIndex(user => user.Username).IsUnique();
            entity.Property(user => user.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(user => user.Role).HasColumnName("role").IsRequired();
            entity.Property(user => user.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(user => user.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        });
    }
}
