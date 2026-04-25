using System.ComponentModel.DataAnnotations;
using ThirteenthAvenue.Models.Enums;

namespace ThirteenthAvenue.ViewModels
{
    public class AdminOrganizerManagementViewModel
    {
        public int OrganizerProfileId { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public string OrganizerName { get; set; } = string.Empty;

        public string OrganizerEmail { get; set; } = string.Empty;

        public VerificationStatus VerificationStatus { get; set; }

        public string? DisclaimerText { get; set; }

        [Range(0, 100)]
        public decimal OrganizerSharePercentage { get; set; }

        [Range(0, 100)]
        public decimal AdminSharePercentage { get; set; }
    }
}