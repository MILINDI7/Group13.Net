using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThirteenthAvenue.Data;
using ThirteenthAvenue.Models.Enums;
using ThirteenthAvenue.ViewModels;

namespace ThirteenthAvenue.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var today = DateTime.Today;

            var query = _context.Events
                .Include(e => e.Category)
                .Include(e => e.OrganizerProfile)
                .Where(e => e.Status == EventStatus.Published);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.Title.Contains(search) ||
                    e.City.Contains(search) ||
                    e.Venue.Contains(search));
            }

            var model = new LandingPageViewModel
            {
                Search = search,

                UpcomingEvents = await query
                    .Where(e => e.EventDate >= today)
                    .OrderBy(e => e.EventDate)
                    .ThenBy(e => e.StartTime)
                    .Take(6)
                    .ToListAsync(),

                LatestEvents = await query
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(6)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}