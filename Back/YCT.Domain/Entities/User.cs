using YCT.Domain.Common;

namespace YCT.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = "Customer";
    public bool IsActive { get; set; } = true;
    public string? GoogleId { get; set; }
    public string? AvatarUrl { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
