using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ThirteenthAvenue.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public OrganizerProfile? OrganizerProfile { get; set; }
    }
}