using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThirteenthAvenue.Models.Enums;

namespace ThirteenthAvenue.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        public int OrganizerProfileId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(3000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Venue { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        [StringLength(500)]
        public string? BannerImageUrl { get; set; }

        public EventStatus Status { get; set; } = EventStatus.Draft;

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(OrganizerProfileId))]
        public OrganizerProfile OrganizerProfile { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;

        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}