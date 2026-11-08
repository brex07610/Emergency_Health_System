using System.ComponentModel.DataAnnotations;

namespace Emergency_Health_System.Models;

public class UpdateBookingStatusViewModel
{
    public int BookingId { get; set; }

    [EnumDataType(typeof(BookingStatus))]
    [Display(Name = "Booking status")]
    public BookingStatus CurrentStatus { get; set; }

    [Display(Name = "Estimated arrival")]
    public DateTime? EstimatedArrival { get; set; }
}
