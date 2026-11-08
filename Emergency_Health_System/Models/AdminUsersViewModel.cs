namespace Emergency_Health_System.Models;

public class AdminUsersViewModel
{
    public string? SearchTerm { get; set; }

    public IReadOnlyList<AdminUserListItemViewModel> Users { get; set; } = [];
}
