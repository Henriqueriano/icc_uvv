using System.ComponentModel.DataAnnotations;

namespace backend.Data;

public class User
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public required string Username { get; set; }

    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public required string Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
