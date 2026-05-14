using SmartApiary.Domain.Enums;

namespace SmartApiary.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public string? ActivationToken { get; set; }
    public DateTime? ActivationTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public double WeightDropThreshold { get; set; } = 10.0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Apiary> Apiaries { get; set; } = new List<Apiary>();
    public ICollection<Parcel> Parcels { get; set; } = new List<Parcel>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
