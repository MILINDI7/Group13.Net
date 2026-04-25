using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThirteenthAvenue.Models.Enums;

namespace ThirteenthAvenue.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string BookingReference { get; set; } = string.Empty;

        [Required]
        public int EventId { get; set; }

        public string? UserId { get; set; }

        [StringLength(150)]
        public string? GuestName { get; set; }

        [StringLength(150)]
        public string? GuestEmail { get; set; }

        [StringLength(20)]
        public string? GuestPhone { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(100)]
        public string? PaymentReference { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public RevenueShare? RevenueShare { get; set; }
    }
}