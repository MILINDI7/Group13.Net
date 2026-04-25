using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThirteenthAvenue.Models
{
    public class TicketType
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int QuantityAvailable { get; set; }

        public int QuantitySold { get; set; } = 0;

        public DateTime? SaleStartDate { get; set; }

        public DateTime? SaleEndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; } = null!;

        public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }
}