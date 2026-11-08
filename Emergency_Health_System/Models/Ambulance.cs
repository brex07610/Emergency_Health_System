using System.ComponentModel.DataAnnotations;

namespace Emergency_Health_System.Models;

public class Ambulance
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string VehicleNumber { get; set; } = string.Empty;

    public TransportType TransportType { get; set; }

    public AmbulanceStatus Status { get; set; } = AmbulanceStatus.Available;

    [Required, StringLength(255)]
    public string CurrentLocation { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
