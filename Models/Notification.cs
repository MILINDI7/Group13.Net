using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThirteenthAvenue.Models.Enums;

namespace ThirteenthAvenue.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [StringLength(150)]
        public string? EmailTo { get; set; }

        public NotificationType Type { get; set; } = NotificationType.Email;

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        public DateTime? SentAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}