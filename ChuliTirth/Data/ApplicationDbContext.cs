using ChuliTirth.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomImage> RoomImages => Set<RoomImage>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<RoomAmenity> RoomAmenities => Set<RoomAmenity>();

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingRoom> BookingRooms => Set<BookingRoom>();
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<BhojanshalaTiming> BhojanshalaTimings => Set<BhojanshalaTiming>();
    public DbSet<JainTithi> JainTithis => Set<JainTithi>();
    public DbSet<JainQuote> JainQuotes => Set<JainQuote>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<GalleryCategory> GalleryCategories => Set<GalleryCategory>();
    public DbSet<GalleryImage> GalleryImages => Set<GalleryImage>();
    public DbSet<BookingRule> BookingRules => Set<BookingRule>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.Mobile).HasMaxLength(20);
        });

        modelBuilder.Entity<RoomType>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Price).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Room>(e =>
        {
            e.HasIndex(x => new { x.RoomTypeId, x.RoomNumber }).IsUnique();
            e.Property(x => x.RoomNumber).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.RoomType).WithMany(x => x.Rooms).HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoomImage>(e =>
        {
            e.HasOne(x => x.RoomType).WithMany(x => x.Images).HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoomAmenity>(e =>
        {
            e.HasKey(x => new { x.RoomTypeId, x.AmenityId });
            e.HasOne(x => x.RoomType).WithMany(x => x.RoomAmenities).HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Amenity).WithMany(x => x.RoomAmenities).HasForeignKey(x => x.AmenityId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasIndex(x => x.BookingNumber).IsUnique();
            e.Property(x => x.BookingNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.RoomAmount).HasPrecision(10, 2);
            e.Property(x => x.AdditionalCharges).HasPrecision(10, 2);
            e.Property(x => x.TotalAmount).HasPrecision(10, 2);
            e.HasOne(x => x.User).WithMany(x => x.Bookings).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<BookingRoom>(e =>
        {
            e.Property(x => x.RatePerNight).HasPrecision(10, 2);
            e.HasOne(x => x.Booking).WithMany(x => x.BookingRooms).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Room).WithMany(x => x.BookingRooms).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RoomType).WithMany().HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
            // Speeds up the overlap check that guards against double-booking a room.
            e.HasIndex(x => new { x.RoomId, x.CheckIn, x.CheckOut });
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(10, 2);
            e.Property(x => x.RefundedAmount).HasPrecision(10, 2);
            e.HasOne(x => x.Booking).WithMany(x => x.Payments).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            // Looked up by the Razorpay checkout callback and webhook to find the matching Payment.
            e.HasIndex(x => x.GatewayOrderId);
        });

        modelBuilder.Entity<GalleryImage>(e =>
        {
            e.HasOne(x => x.GalleryCategory).WithMany(x => x.Images).HasForeignKey(x => x.GalleryCategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SiteSetting>(e =>
        {
            e.HasIndex(x => x.Key).IsUnique();
            e.Property(x => x.Key).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<JainTithi>(e =>
        {
            e.HasIndex(x => x.GregorianDate).IsUnique();
        });
    }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
    }
}
