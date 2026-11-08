namespace Emergency_Health_System.Models;

public class TrackingSnapshotViewModel
{
    public int BookingId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? EstimatedArrivalUtc { get; set; }

    public string? VehicleNumber { get; set; }

    public string? VehicleLocation { get; set; }

    public DateTime? VehicleUpdatedUtc { get; set; }

    public DateTime CheckedAtUtc { get; set; }

    public bool IsTerminal { get; set; }
}
