namespace ThirteenthAvenue.ViewModels
{
    public class OrganizerDashboardViewModel
    {
        public string BusinessName { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;

        public int TotalEvents { get; set; }
        public int TotalTicketsSold { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal OrganizerRevenue { get; set; }

        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartData { get; set; } = new();

        public List<EventSummaryViewModel> Events { get; set; } = new();
    }

    public class EventSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string City { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}