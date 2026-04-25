namespace ThirteenthAvenue.ViewModels
{
    public class BookingSuccessViewModel
    {
        public string BookingReference { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}