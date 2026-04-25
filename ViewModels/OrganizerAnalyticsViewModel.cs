namespace ThirteenthAvenue.ViewModels
{
    public class OrganizerAnalyticsViewModel
    {
        public string BusinessName { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public string? DisclaimerText { get; set; }

        public int TotalEvents { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalAmountCollected { get; set; }

        public List<EventPerformanceViewModel> EventPerformances { get; set; } = new();

        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartAmounts { get; set; } = new();
    }

    public class EventPerformanceViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TicketsSold { get; set; }
        public decimal AmountCollected { get; set; }
        public DateTime EventDate { get; set; }
    }
}