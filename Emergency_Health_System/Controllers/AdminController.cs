using Emergency_Health_System.Data;
using Emergency_Health_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emergency_Health_System.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin")]
public class AdminController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var activeStatuses = new[]
        {
            BookingStatus.Confirmed,
            BookingStatus.Dispatched,
            BookingStatus.InTransit,
            BookingStatus.Arrived
        };

        var model = new AdminDashboardViewModel
        {
            PendingBookings = await dbContext.Bookings.CountAsync(booking => booking.CurrentStatus == BookingStatus.Pending),
            ActiveBookings = await dbContext.Bookings.CountAsync(booking => activeStatuses.Contains(booking.CurrentStatus)),
            AvailableAmbulances = await dbContext.Ambulances.CountAsync(ambulance => ambulance.Status == AmbulanceStatus.Available),
            TotalAmbulances = await dbContext.Ambulances.CountAsync(),
            RegisteredUsers = await userManager.Users.CountAsync(),
            RecentBookings = await dbContext.Bookings
                .AsNoTracking()
                .Include(booking => booking.User)
                .OrderByDescending(booking => booking.BookingTime)
                .Take(8)
                .ToListAsync()
        };

        return View(model);
    }

    [HttpGet("Users")]
    public async Task<IActionResult> Users([FromQuery] string? search)
    {
        if (search?.Length > 100)
        {
            return BadRequest("Search text must be 100 characters or fewer.");
        }

        var normalizedSearch = search?.Trim();
        var usersQuery = userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            usersQuery = usersQuery.Where(user =>
                (user.FullName != null && user.FullName.Contains(normalizedSearch)) ||
                (user.Email != null && user.Email.Contains(normalizedSearch)) ||
                (user.PhoneNumber != null && user.PhoneNumber.Contains(normalizedSearch)));
        }

        var users = await usersQuery
            .OrderBy(user => user.Email)
            .ToListAsync();
        var userIds = users.Select(user => user.Id).ToArray();
        var bookingCounts = await dbContext.Bookings
            .AsNoTracking()
            .Where(booking => userIds.Contains(booking.UserId))
            .GroupBy(booking => booking.UserId)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count);

        var model = new AdminUsersViewModel
        {
            SearchTerm = normalizedSearch,
            Users = users.Select(user => new AdminUserListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                BookingCount = bookingCounts.GetValueOrDefault(user.Id)
            }).ToList()
        };

        return View(model);
    }

    [HttpGet("Ambulances")]
    public async Task<IActionResult> Ambulances()
    {
        var ambulances = await dbContext.Ambulances
            .AsNoTracking()
            .OrderBy(ambulance => ambulance.VehicleNumber)
            .ToListAsync();

        return View(ambulances);
    }

    [HttpGet("Ambulances/Create")]
    public IActionResult CreateAmbulance()
    {
        return View("AmbulanceForm", new AmbulanceInputViewModel());
    }

    [HttpPost("Ambulances/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAmbulance(AmbulanceInputViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("AmbulanceForm", model);
        }

        var vehicleNumber = model.VehicleNumber.Trim();
        if (await dbContext.Ambulances.AnyAsync(ambulance => ambulance.VehicleNumber == vehicleNumber))
        {
            ModelState.AddModelError(nameof(model.VehicleNumber), "That vehicle number is already registered.");
            return View("AmbulanceForm", model);
        }

        dbContext.Ambulances.Add(new Ambulance
        {
            VehicleNumber = vehicleNumber,
            TransportType = model.TransportType,
            Status = model.Status,
            CurrentLocation = model.CurrentLocation.Trim(),
            LastUpdated = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        TempData["StatusMessage"] = "Ambulance added.";
        return RedirectToAction(nameof(Ambulances));
    }

    [HttpGet("Ambulances/{id:int}/Edit")]
    public async Task<IActionResult> EditAmbulance(int id)
    {
        var ambulance = await dbContext.Ambulances.FindAsync(id);
        if (ambulance is null)
        {
            return NotFound();
        }

        return View("AmbulanceForm", new AmbulanceInputViewModel
        {
            Id = ambulance.Id,
            VehicleNumber = ambulance.VehicleNumber,
            TransportType = ambulance.TransportType,
            Status = ambulance.Status,
            CurrentLocation = ambulance.CurrentLocation
        });
    }

    [HttpPost("Ambulances/{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAmbulance(int id, AmbulanceInputViewModel model)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("AmbulanceForm", model);
        }

        var ambulance = await dbContext.Ambulances.FindAsync(id);
        if (ambulance is null)
        {
            return NotFound();
        }

        var vehicleNumber = model.VehicleNumber.Trim();
        if (await dbContext.Ambulances.AnyAsync(item => item.Id != id && item.VehicleNumber == vehicleNumber))
        {
            ModelState.AddModelError(nameof(model.VehicleNumber), "That vehicle number is already registered.");
            return View("AmbulanceForm", model);
        }

        var hasActiveBooking = await dbContext.Bookings.AnyAsync(booking =>
            booking.AmbulanceId == id &&
            booking.CurrentStatus != BookingStatus.Complete &&
            booking.CurrentStatus != BookingStatus.Cancelled);
        if (hasActiveBooking && (model.TransportType != ambulance.TransportType || model.Status != ambulance.Status))
        {
            ModelState.AddModelError(string.Empty, "This vehicle is assigned to an active booking. Update the booking before changing its transport type or status.");
            return View("AmbulanceForm", model);
        }

        ambulance.VehicleNumber = vehicleNumber;
        ambulance.TransportType = model.TransportType;
        ambulance.Status = model.Status;
        ambulance.CurrentLocation = model.CurrentLocation.Trim();
        ambulance.LastUpdated = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["StatusMessage"] = "Ambulance updated.";
        return RedirectToAction(nameof(Ambulances));
    }

    [HttpGet("Bookings")]
    public async Task<IActionResult> Bookings()
    {
        var bookings = await dbContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.User)
            .Include(booking => booking.Ambulance)
            .OrderBy(booking => booking.CurrentStatus == BookingStatus.Pending ? 0 : 1)
            .ThenByDescending(booking => booking.BookingTime)
            .ToListAsync();

        return View(bookings);
    }

    [HttpGet("Bookings/{id:int}")]
    public async Task<IActionResult> Booking(int id)
    {
        var booking = await dbContext.Bookings
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.Ambulance)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (booking is null)
        {
            return NotFound();
        }

        ViewBag.AvailableAmbulances = await dbContext.Ambulances
            .AsNoTracking()
            .Where(ambulance => ambulance.Status == AmbulanceStatus.Available && ambulance.TransportType == booking.RequestedTransport)
            .OrderBy(ambulance => ambulance.VehicleNumber)
            .ToListAsync();
        ViewBag.StatusModel = new UpdateBookingStatusViewModel
        {
            BookingId = booking.Id,
            CurrentStatus = booking.CurrentStatus,
            EstimatedArrival = booking.EstimatedArrival
        };
        ViewBag.AllowedStatuses = GetAllowedStatuses(booking.CurrentStatus);

        return View("Booking", booking);
    }

    [HttpPost("Bookings/{id:int}/Assign")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignAmbulance(int id, int ambulanceId, DateTime? estimatedArrival)
    {
        var booking = await dbContext.Bookings.FirstOrDefaultAsync(item => item.Id == id);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.AmbulanceId.HasValue || booking.CurrentStatus is not (BookingStatus.Pending or BookingStatus.Confirmed))
        {
            return Conflict("This booking cannot be assigned a vehicle in its current state.");
        }

        if (!ModelState.IsValid)
        {
            return await Booking(id);
        }

        var ambulance = await dbContext.Ambulances.FirstOrDefaultAsync(item =>
            item.Id == ambulanceId &&
            item.Status == AmbulanceStatus.Available &&
            item.TransportType == booking.RequestedTransport);
        if (ambulance is null)
        {
            ModelState.AddModelError(string.Empty, "Choose an available vehicle with the requested transport type.");
            return await Booking(id);
        }

        booking.AmbulanceId = ambulance.Id;
        booking.CurrentStatus = BookingStatus.Confirmed;
        booking.EstimatedArrival = estimatedArrival?.ToUniversalTime();
        ambulance.Status = AmbulanceStatus.Dispatched;
        ambulance.LastUpdated = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["StatusMessage"] = "Ambulance assigned to booking.";
        return RedirectToAction(nameof(Booking), new { id });
    }

    [HttpPost("Bookings/{id:int}/Status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBookingStatus(int id, UpdateBookingStatusViewModel model)
    {
        if (model.BookingId != id || !ModelState.IsValid)
        {
            return BadRequest();
        }

        var booking = await dbContext.Bookings
            .Include(item => item.User)
            .Include(item => item.Ambulance)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (booking is null)
        {
            return NotFound();
        }

        if (!GetAllowedStatuses(booking.CurrentStatus).Contains(model.CurrentStatus))
        {
            ModelState.AddModelError(nameof(model.CurrentStatus), $"A booking cannot move from {booking.CurrentStatus} to {model.CurrentStatus}. Choose one of the available statuses.");
            ViewBag.AvailableAmbulances = await dbContext.Ambulances
                .AsNoTracking()
                .Where(ambulance => ambulance.Status == AmbulanceStatus.Available && ambulance.TransportType == booking.RequestedTransport)
                .OrderBy(ambulance => ambulance.VehicleNumber)
                .ToListAsync();
            ViewBag.StatusModel = model;
            ViewBag.AllowedStatuses = GetAllowedStatuses(booking.CurrentStatus);
            return View("Booking", booking);
        }

        booking.CurrentStatus = model.CurrentStatus;
        booking.EstimatedArrival = model.EstimatedArrival?.ToUniversalTime();

        if (booking.Ambulance is not null)
        {
            if (model.CurrentStatus is BookingStatus.Complete or BookingStatus.Cancelled)
            {
                booking.Ambulance.Status = AmbulanceStatus.Available;
            }
            else if (model.CurrentStatus == BookingStatus.InTransit)
            {
                booking.Ambulance.Status = AmbulanceStatus.InTransit;
            }
            else if (model.CurrentStatus is BookingStatus.Confirmed or BookingStatus.Dispatched or BookingStatus.Arrived)
            {
                booking.Ambulance.Status = AmbulanceStatus.Dispatched;
            }

            booking.Ambulance.LastUpdated = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
        TempData["StatusMessage"] = "Booking status updated.";
        return RedirectToAction(nameof(Booking), new { id });
    }

    private static IReadOnlyList<BookingStatus> GetAllowedStatuses(BookingStatus currentStatus)
    {
        return currentStatus switch
        {
            BookingStatus.Pending => [BookingStatus.Pending, BookingStatus.Cancelled],
            BookingStatus.Confirmed => [BookingStatus.Confirmed, BookingStatus.Dispatched, BookingStatus.Cancelled],
            BookingStatus.Dispatched => [BookingStatus.Dispatched, BookingStatus.InTransit, BookingStatus.Cancelled],
            BookingStatus.InTransit => [BookingStatus.InTransit, BookingStatus.Arrived, BookingStatus.Cancelled],
            BookingStatus.Arrived => [BookingStatus.Arrived, BookingStatus.Complete, BookingStatus.Cancelled],
            BookingStatus.Complete => [BookingStatus.Complete],
            BookingStatus.Cancelled => [BookingStatus.Cancelled],
            _ => throw new ArgumentOutOfRangeException(nameof(currentStatus), currentStatus, "Unknown booking status.")
        };
    }
}
