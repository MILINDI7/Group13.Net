using System.ComponentModel.DataAnnotations;

namespace ThirteenthAvenue.ViewModels
{
    public class CreateBookingViewModel
    {
        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Venue { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string? BannerImageUrl { get; set; }

        public bool IsAuthenticatedUser { get; set; }

        [Display(Name = "Full Name")]
        public string? GuestName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string? GuestEmail { get; set; }

        [Display(Name = "Phone Number")]
        public string? GuestPhone { get; set; }

        public List<BookingTicketInputViewModel> Tickets { get; set; } = new();
    }

    public class BookingTicketInputViewModel
    {
        public int TicketTypeId { get; set; }

        public string TicketName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int AvailableQuantity { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}