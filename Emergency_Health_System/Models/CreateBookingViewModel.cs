using System.ComponentModel.DataAnnotations;

namespace Emergency_Health_System.Models;

public class CreateBookingViewModel
{
    [Required]
    [EnumDataType(typeof(TransportType))]
    [Display(Name = "Transport type")]
    public TransportType RequestedTransport { get; set; }

    [Required, StringLength(255)]
    [Display(Name = "Pickup location")]
    public string PickupLocation { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string Destination { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [RegularExpression("^(Medical emergency|Injury|Pregnancy|Other)$", ErrorMessage = "Choose a valid emergency type.")]
    [Display(Name = "Emergency type")]
    public string EmergencyType { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    [Display(Name = "What medical help is needed?")]
    public string MedicalProblem { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Relevant medical history (optional)")]
    public string? PatientHistory { get; set; }
}
