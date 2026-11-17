using Microsoft.EntityFrameworkCore;
using CEMS.Models;

namespace CEMS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Registration> Registrations { get; set; }
    public DbSet<RegistrationLog> RegistrationLogs { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Enquiry> Enquiries { get; set; }
    public DbSet<EventResource> EventResources { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // SQL Server rejects schemas with multiple cascade paths (e.g. Users -> Registrations and
        // Users -> Events -> Registrations). Records are therefore never deleted automatically:
        // events are cancelled instead of deleted, and users with data cannot be deleted.
        foreach (var fk in mb.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        // A user can hold only one ACTIVE registration per event (cancelled ones stay as history)
        mb.Entity<Registration>()
            .HasIndex(r => new { r.UserId, r.EventId })
            .IsUnique()
            .HasFilter("[Status] = 'Registered'");

        mb.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // One feedback per user per event, rating limited to 1-5
        mb.Entity<Feedback>()
            .HasIndex(f => new { f.EventId, f.UserId })
            .IsUnique();

        mb.Entity<Feedback>()
            .ToTable(t => t.HasCheckConstraint("CK_Feedbacks_Rating", "[Rating] BETWEEN 1 AND 5"));
    }
}
