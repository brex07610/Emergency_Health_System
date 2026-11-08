namespace Emergency_Health_System.Models;

public enum TransportType
{
    Road,
    Air
}

public enum AmbulanceStatus
{
    Available,
    Dispatched,
    Maintenance,
    InTransit
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Dispatched,
    InTransit,
    Arrived,
    Complete,
    Cancelled
}
