using SmartApiary.Domain.Enums;

namespace SmartApiary.Domain.Entities;

public class Alert
{
    public Guid Id { get; set; }
    public AlertType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid? ApiaryId { get; set; }
    public Guid? HiveId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
