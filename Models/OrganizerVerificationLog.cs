using System.ComponentModel.DataAnnotations.Schema;
using ThirteenthAvenue.Models.Enums;

namespace ThirteenthAvenue.Models
{
    public class OrganizerVerificationLog
    {
        public int Id { get; set; }

        public int OrganizerProfileId { get; set; }

        public string ReviewedByUserId { get; set; } = string.Empty;

        public VerificationStatus PreviousStatus { get; set; }

        public VerificationStatus NewStatus { get; set; }

        public string? Notes { get; set; }

        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(OrganizerProfileId))]
        public OrganizerProfile OrganizerProfile { get; set; } = null!;

        [ForeignKey(nameof(ReviewedByUserId))]
        public ApplicationUser ReviewedByUser { get; set; } = null!;
    }
}