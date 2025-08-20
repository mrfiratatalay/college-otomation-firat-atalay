using ElasoftCommunityManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<UserModel> Users { get; set; }
    public DbSet<BudgetRequestModel> BudgetRequest { get; set; }
    public DbSet<ClubMembershipModel> ClubMembership { get; set; }
    public DbSet<ClubModel> Club { get; set; }
    public DbSet<DocumentModel> Documents { get; set; }
    public DbSet<EventModel> Event { get; set; }
    public DbSet<EventParticipantModel> EventParticipant { get; set; }
    public DbSet<ClubExpenceModel> ClubExpenses { get; set; }
    public DbSet<AnnouncementModel> Announcement { get; set; }
    public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
    public DbSet<CategoryModel> Categories { get; set; }
    public DbSet<DepartmentModel> Departments { get; set; }
    public DbSet<NotificationModel> Notifications { get; set; }
    public DbSet<SettingModel> Settings { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<TimeSlot> TimeSlots { get; set; }
    public DbSet<EventReservation> EventReservations { get; set; }
    public DbSet<DailySlot> DailySlots { get; set; }
    public DbSet<DisabledSlot> DisabledSlots { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClubModel>()
            .HasOne(c => c.Advisor)
            .WithMany()
            .HasForeignKey(c => c.AdvisorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<RefreshTokenModel>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<BudgetRequestModel>()
            .Property(b => b.RequestedAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<TimeSlot>()
             .HasOne(t => t.Location)
             .WithMany(l => l.TimeSlots)
             .HasForeignKey(t => t.LocationId)
              .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventReservation>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Reservations)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventReservation>()
            .HasOne(r => r.Location)
            .WithMany(l => l.Reservations)
            .HasForeignKey(r => r.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventReservation>()
        .HasOne(r => r.TimeSlot)
        .WithMany() // <-- Eksik olan buydu
        .HasForeignKey(r => r.TimeSlotId)
        .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<DisabledSlot>()
        .HasOne(d => d.Location)
        .WithMany()
        .HasForeignKey(d => d.LocationId)
        .OnDelete(DeleteBehavior.Restrict); // 🔒 cycle hatasını engelemek için 

        modelBuilder.Entity<DisabledSlot>()
            .HasOne(d => d.Slot)
            .WithMany()
            .HasForeignKey(d => d.SlotId)
            .OnDelete(DeleteBehavior.Restrict); // 🔒 cycle hatasını engellemek için 


        modelBuilder.Entity<Location>()
            .Ignore(l => l.ValidDays)
            .Ignore(l => l.DisabledDates);

        modelBuilder.Entity<Location>()
                .Property<string>("ValidDaysSerialized");

        modelBuilder.Entity<Location>()
                .Property<string>("DisabledDatesSerialized");

    }
}
