using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ThirteenthAvenue.ViewModels
{
    public class CreateEventViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(3000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Venue { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Event Banner")]
        public IFormFile? BannerImage { get; set; }

        public string? ExistingBannerImageUrl { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();

        public List<TicketInputViewModel> Tickets { get; set; } = new()
        {
            new TicketInputViewModel()
        };
    }

    public class TicketInputViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 999999999)]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity Available")]
        public int QuantityAvailable { get; set; }
    }
}