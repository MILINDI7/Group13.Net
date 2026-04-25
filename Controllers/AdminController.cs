using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThirteenthAvenue.Data;
using ThirteenthAvenue.Models;
using ThirteenthAvenue.Models.Enums;
using ThirteenthAvenue.ViewModels;

namespace ThirteenthAvenue.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Organizers()
        {
            var organizers = await _context.OrganizerProfiles
                .Include(o => o.User)
                .OrderBy(o => o.BusinessName)
                .Select(o => new AdminOrganizerManagementViewModel
                {
                    OrganizerProfileId = o.Id,
                    BusinessName = o.BusinessName,
                    OrganizerName = o.User.FullName,
                    OrganizerEmail = o.User.Email ?? "",
                    VerificationStatus = o.VerificationStatus,
                    DisclaimerText = o.DisclaimerText,
                    OrganizerSharePercentage = o.OrganizerSharePercentage,
                    AdminSharePercentage = o.AdminSharePercentage
                })
                .ToListAsync();

            return View(organizers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOrganizer(int id)
        {
            var organizer = await _context.OrganizerProfiles.FindAsync(id);

            if (organizer == null)
            {
                return NotFound();
            }

            var adminUser = await _userManager.GetUserAsync(User);

            organizer.VerificationStatus = VerificationStatus.Verified;
            organizer.VerifiedAt = DateTime.UtcNow;
            organizer.VerifiedByUserId = adminUser?.Id;
            organizer.DisclaimerText = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Organizer verified successfully.";
            return RedirectToAction(nameof(Organizers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOrganizer(int id)
        {
            var organizer = await _context.OrganizerProfiles.FindAsync(id);

            if (organizer == null)
            {
                return NotFound();
            }

            var adminUser = await _userManager.GetUserAsync(User);

            organizer.VerificationStatus = VerificationStatus.Rejected;
            organizer.VerifiedAt = DateTime.UtcNow;
            organizer.VerifiedByUserId = adminUser?.Id;
            organizer.DisclaimerText = "This organizer has not been verified by 13th Avenue.";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Organizer rejected.";
            return RedirectToAction(nameof(Organizers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRevenueShare(int id, decimal organizerSharePercentage, decimal adminSharePercentage)
        {
            var organizer = await _context.OrganizerProfiles.FindAsync(id);

            if (organizer == null)
            {
                return NotFound();
            }

            if (organizerSharePercentage < 0 || organizerSharePercentage > 100 ||
                adminSharePercentage < 0 || adminSharePercentage > 100)
            {
                TempData["ErrorMessage"] = "Percentages must be between 0 and 100.";
                return RedirectToAction(nameof(Organizers));
            }

            if (organizerSharePercentage + adminSharePercentage != 100)
            {
                TempData["ErrorMessage"] = "Organizer share and admin share must add up to 100%.";
                return RedirectToAction(nameof(Organizers));
            }

            organizer.OrganizerSharePercentage = organizerSharePercentage;
            organizer.AdminSharePercentage = adminSharePercentage;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Revenue share updated successfully.";
            return RedirectToAction(nameof(Organizers));
        }
    }
}