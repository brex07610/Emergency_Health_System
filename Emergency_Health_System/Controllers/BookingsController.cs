using System.Security.Claims;
using Emergency_Health_System.Data;
using Emergency_Health_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emergency_Health_System.Controllers;

[Authorize]
public class BookingsController(ApplicationDbContext dbContext) : Controller
{
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateBookingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var booking = new Booking
        {
            UserId = userId,
            RequestedTransport = model.RequestedTransport,
            PickupLocation = model.PickupLocation.Trim(),
            Destination = model.Destination.Trim(),
            EmergencyType = model.EmergencyType.Trim(),
            MedicalProblem = model.MedicalProblem.Trim(),
            PatientHistory = string.IsNullOrWhiteSpace(model.PatientHistory)
                ? null
                : model.PatientHistory.Trim(),
            BookingTime = DateTime.UtcNow,
            CurrentStatus = BookingStatus.Pending
        };

        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = booking.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var bookings = await dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.UserId == userId)
            .OrderByDescending(booking => booking.BookingTime)
            .ToListAsync();

        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var booking = await dbContext.Bookings
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (booking is null)
        {
            return NotFound();
        }

        if (booking.CurrentStatus != BookingStatus.Pending || booking.AmbulanceId.HasValue)
        {
            TempData["ErrorMessage"] = "Only a pending request without an assigned ambulance can be cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        booking.CurrentStatus = BookingStatus.Cancelled;
        await dbContext.SaveChangesAsync();

        TempData["StatusMessage"] = "Your request was cancelled.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Track(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var booking = await dbContext.Bookings
            .AsNoTracking()
            .Include(item => item.Ambulance)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (booking is null || (booking.UserId != userId && !User.IsInRole("Admin")))
        {
            return NotFound();
        }

        return View("Track", booking);
    }

    [HttpGet("/api/bookings/{id:int}/tracking")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> TrackingStatus(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var booking = await dbContext.Bookings
            .AsNoTracking()
            .Include(item => item.Ambulance)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (booking is null || (booking.UserId != userId && !User.IsInRole("Admin")))
        {
            return NotFound();
        }

        return Ok(new TrackingSnapshotViewModel
        {
            BookingId = booking.Id,
            Status = booking.CurrentStatus.ToString(),
            EstimatedArrivalUtc = booking.EstimatedArrival,
            VehicleNumber = booking.Ambulance?.VehicleNumber,
            VehicleLocation = booking.Ambulance?.CurrentLocation,
            VehicleUpdatedUtc = booking.Ambulance?.LastUpdated,
            CheckedAtUtc = DateTime.UtcNow,
            IsTerminal = booking.CurrentStatus is BookingStatus.Complete or BookingStatus.Cancelled
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var booking = await dbContext.Bookings
            .AsNoTracking()
            .Include(item => item.Ambulance)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        return booking is null ? NotFound() : View(booking);
    }
}
