namespace Emergency_Health_System.Models;

public class AdminDashboardViewModel
{
    public int PendingBookings { get; set; }

    public int ActiveBookings { get; set; }

    public int AvailableAmbulances { get; set; }

    public int TotalAmbulances { get; set; }

    public int RegisteredUsers { get; set; }

    public IReadOnlyList<Booking> RecentBookings { get; set; } = [];
}
