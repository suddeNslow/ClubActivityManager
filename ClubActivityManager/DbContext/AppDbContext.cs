using Microsoft.EntityFrameworkCore;
using ClubActivityManager.Models;

namespace ArtClubApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Member>? Members { get; set; }
        public DbSet<Event>? Events { get; set; }
        public DbSet<EventRegistration>? EventRegistrations { get; set; }
        public DbSet<Payment>? Payments { get; set; }
        public DbSet<Resource>? Resources { get; set; }
        public DbSet<ResourceReservation>? ResourceReservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany(m => m.CreatedEvents)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventRegistration>()
                .HasIndex(er => new { er.MemberId, er.EventId })
                .IsUnique(); // prevent duplicate registration

            // Optional: Enum conversion
            modelBuilder.Entity<Member>()
                .Property(m => m.Role)
                .HasConversion<string>();
        }
    }

}

