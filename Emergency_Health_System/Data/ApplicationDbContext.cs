using Emergency_Health_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Emergency_Health_System.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Ambulance> Ambulances => Set<Ambulance>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Ambulance>()
            .HasIndex(ambulance => ambulance.VehicleNumber)
            .IsUnique();

        builder.Entity<Ambulance>()
            .Property(ambulance => ambulance.TransportType)
            .HasConversion<string>();

        builder.Entity<Ambulance>()
            .Property(ambulance => ambulance.Status)
            .HasConversion<string>();

        builder.Entity<Booking>()
            .Property(booking => booking.RequestedTransport)
            .HasConversion<string>();

        builder.Entity<Booking>()
            .Property(booking => booking.CurrentStatus)
            .HasConversion<string>();

        builder.Entity<Booking>()
            .HasOne(booking => booking.User)
            .WithMany()
            .HasForeignKey(booking => booking.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Booking>()
            .HasOne(booking => booking.Ambulance)
            .WithMany(ambulance => ambulance.Bookings)
            .HasForeignKey(booking => booking.AmbulanceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
