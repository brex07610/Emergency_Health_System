using System.ComponentModel.DataAnnotations;

namespace Emergency_Health_System.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public int? AmbulanceId { get; set; }

    public Ambulance? Ambulance { get; set; }

    public TransportType RequestedTransport { get; set; }

    [Required, StringLength(255)]
    public string PickupLocation { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public string MedicalProblem { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string EmergencyType { get; set; } = string.Empty;

    public string? PatientHistory { get; set; }

    public DateTime BookingTime { get; set; } = DateTime.UtcNow;

    public BookingStatus CurrentStatus { get; set; } = BookingStatus.Pending;

    public DateTime? EstimatedArrival { get; set; }
}
