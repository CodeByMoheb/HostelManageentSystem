using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HostelManageentSystem.Models;

namespace HostelManageentSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Hostel> Hostels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure relationships
            builder.Entity<Hostel>()
                .HasOne(h => h.Manager)
                .WithMany()
                .HasForeignKey(h => h.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Room>()
                .HasOne(r => r.Hostel)
                .WithMany()
                .HasForeignKey(r => r.HostelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Meal>()
                .HasOne(m => m.Hostel)
                .WithMany()
                .HasForeignKey(m => m.HostelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Hostel)
                .WithMany()
                .HasForeignKey(b => b.HostelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Room)
                .WithMany()
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Hostel>()
                .HasMany(h => h.Rooms)
                .WithOne(r => r.Hostel)
                .HasForeignKey(r => r.HostelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Hostel>()
                .HasMany(h => h.Meals)
                .WithOne(m => m.Hostel)
                .HasForeignKey(m => m.HostelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Hostel>()
                .HasMany(h => h.Students)
                .WithOne(s => s.Hostel)
                .HasForeignKey(s => s.HostelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Room>()
                .HasMany(r => r.Students)
                .WithOne(s => s.Room)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 