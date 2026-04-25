using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ThirteenthAvenue.Models;

namespace ThirteenthAvenue.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<OrganizerProfile> OrganizerProfiles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RevenueShare> RevenueShares { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<OrganizerVerificationLog> OrganizerVerificationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<OrganizerProfile>()
                .HasOne(op => op.User)
                .WithOne(u => u.OrganizerProfile)
                .HasForeignKey<OrganizerProfile>(op => op.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrganizerProfile>()
                .HasOne(op => op.VerifiedByUser)
                .WithMany()
                .HasForeignKey(op => op.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Event>()
                .HasOne(e => e.OrganizerProfile)
                .WithMany(op => op.Events)
                .HasForeignKey(e => e.OrganizerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Event>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TicketType>()
                .HasOne(tt => tt.Event)
                .WithMany(e => e.TicketTypes)
                .HasForeignKey(tt => tt.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BookingItem>()
                .HasOne(bi => bi.Booking)
                .WithMany(b => b.BookingItems)
                .HasForeignKey(bi => bi.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BookingItem>()
                .HasOne(bi => bi.TicketType)
                .WithMany(tt => tt.BookingItems)
                .HasForeignKey(bi => bi.TicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RevenueShare>()
                .HasOne(rs => rs.Booking)
                .WithOne(b => b.RevenueShare)
                .HasForeignKey<RevenueShare>(rs => rs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrganizerVerificationLog>()
                .HasOne(ovl => ovl.OrganizerProfile)
                .WithMany()
                .HasForeignKey(ovl => ovl.OrganizerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrganizerVerificationLog>()
                .HasOne(ovl => ovl.ReviewedByUser)
                .WithMany()
                .HasForeignKey(ovl => ovl.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasIndex(b => b.BookingReference)
                .IsUnique();
        }
    }
}