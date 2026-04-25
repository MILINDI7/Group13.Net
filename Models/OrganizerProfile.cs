using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThirteenthAvenue.Models.Enums;

namespace ThirteenthAvenue.Models
{
    public class OrganizerProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string BusinessName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(500)]
        public string? Website { get; set; }

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

        public string? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [StringLength(500)]
        public string? DisclaimerText { get; set; }

        [Range(0, 100)]
        public decimal OrganizerSharePercentage { get; set; } = 90;

        [Range(0, 100)]
        public decimal AdminSharePercentage { get; set; } = 10;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(VerifiedByUserId))]
        public ApplicationUser? VerifiedByUser { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}