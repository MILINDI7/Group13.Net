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
    [Authorize(Roles = "Organizer")]
    public class OrganizerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrganizerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var organizerProfile = await _context.OrganizerProfiles
                .FirstOrDefaultAsync(o => o.UserId == user.Id);

            if (organizerProfile == null)
            {
                return NotFound("Organizer profile not found.");
            }

            var events = await _context.Events
                .Include(e => e.Bookings)
                    .ThenInclude(b => b.BookingItems)
                .Where(e => e.OrganizerProfileId == organizerProfile.Id)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var eventSummaries = events.Select(e => new EventSummaryViewModel
            {
                Id = e.Id,
                Title = e.Title,
                EventDate = e.EventDate,
                City = e.City,
                IsApproved = e.Status == EventStatus.Published,
                TotalRevenue = e.Bookings
                    .Where(b => b.PaymentStatus == PaymentStatus.Successful)
                    .Sum(b => b.TotalAmount)
            }).ToList();

            var totalTicketsSold = events
                .SelectMany(e => e.Bookings)
                .Where(b => b.PaymentStatus == PaymentStatus.Successful)
                .SelectMany(b => b.BookingItems)
                .Sum(bi => bi.Quantity);

            var totalRevenue = events
                .SelectMany(e => e.Bookings)
                .Where(b => b.PaymentStatus == PaymentStatus.Successful)
                .Sum(b => b.TotalAmount);

            var organizerRevenue = totalRevenue * (organizerProfile.OrganizerSharePercentage / 100m);

            var successfulBookings = events
                .SelectMany(e => e.Bookings)
                .Where(b => b.PaymentStatus == PaymentStatus.Successful)
                .OrderBy(b => b.BookingDate)
                .ToList();

            var groupedByDate = successfulBookings
                .GroupBy(b => b.BookingDate.Date)
                .OrderBy(g => g.Key)
                .ToList();

            var vm = new OrganizerDashboardViewModel
            {
                BusinessName = organizerProfile.BusinessName,
                VerificationStatus = organizerProfile.VerificationStatus.ToString(),
                TotalEvents = events.Count,
                TotalTicketsSold = totalTicketsSold,
                TotalRevenue = totalRevenue,
                OrganizerRevenue = organizerRevenue,
                ChartLabels = groupedByDate
                    .Select(g => g.Key.ToString("dd MMM yyyy"))
                    .ToList(),
                ChartData = groupedByDate
                    .Select(g => g.Sum(x => x.TotalAmount))
                    .ToList(),
                Events = eventSummaries
            };

            return View(vm);
        }
    }
}