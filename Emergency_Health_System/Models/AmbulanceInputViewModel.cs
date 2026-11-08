using System.ComponentModel.DataAnnotations;

namespace Emergency_Health_System.Models;

public class AmbulanceInputViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Vehicle number")]
    public string VehicleNumber { get; set; } = string.Empty;

    [EnumDataType(typeof(TransportType))]
    [Display(Name = "Transport type")]
    public TransportType TransportType { get; set; }

    [EnumDataType(typeof(AmbulanceStatus))]
    public AmbulanceStatus Status { get; set; } = AmbulanceStatus.Available;

    [Required, StringLength(255)]
    [Display(Name = "Current location")]
    public string CurrentLocation { get; set; } = string.Empty;
}
