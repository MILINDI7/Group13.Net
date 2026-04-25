using System.ComponentModel.DataAnnotations.Schema;

namespace ThirteenthAvenue.Models
{
    public class RevenueShare
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OrganizerAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdminAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal OrganizerPercentage { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal AdminPercentage { get; set; }

        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        public bool IsSettled { get; set; } = false;

        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;
    }
}