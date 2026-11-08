namespace Emergency_Health_System.Models;

public class AdminUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public int BookingCount { get; set; }
}
