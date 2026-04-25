using ThirteenthAvenue.Models;

namespace ThirteenthAvenue.ViewModels
{
    public class LandingPageViewModel
    {
        public string? Search { get; set; }

        public List<Event> UpcomingEvents { get; set; } = new();

        public List<Event> LatestEvents { get; set; } = new();
    }
}