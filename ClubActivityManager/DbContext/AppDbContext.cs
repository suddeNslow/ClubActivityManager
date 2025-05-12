using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClubActivityManager.Models;

namespace ArtClubApp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ResourceReservation> ResourceReservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Identity needs this

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany(u => u.CreatedEvents)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventRegistration>()
            .HasIndex(er => new { er.UserId, er.EventId }) 
            .IsUnique();

            modelBuilder.Entity<Resource>().HasData(
                new Resource { ResourceId = 1, Name = "Projector", Type = "Electronics", Availability = "available" },
                new Resource { ResourceId = 2, Name = "Meeting Room A", Type = "Room", Availability = "available" },
                new Resource { ResourceId = 3, Name = "Sound System", Type = "Audio", Availability = "available" }
            );
        }
    }
}
