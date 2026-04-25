using System.ComponentModel.DataAnnotations.Schema;

namespace ThirteenthAvenue.Models
{
    public class BookingItem
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        public int TicketTypeId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;

        [ForeignKey(nameof(TicketTypeId))]
        public TicketType TicketType { get; set; } = null!;
    }
}