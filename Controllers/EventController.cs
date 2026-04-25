using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThirteenthAvenue.Data;
using ThirteenthAvenue.Models;
using ThirteenthAvenue.Models.Enums;
using ThirteenthAvenue.ViewModels;

namespace ThirteenthAvenue.Controllers
{
    [Authorize(Roles = "Organizer")]
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public EventController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Create()
        {
            var organizerProfile = await GetCurrentOrganizerProfileAsync();
            if (organizerProfile == null)
            {
                return NotFound("Organizer profile not found.");
            }

            var vm = new CreateEventViewModel
            {
                EventDate = DateTime.Today,
                Categories = await GetCategoriesAsync(),
                Tickets = new List<TicketInputViewModel>
                {
                    new TicketInputViewModel()
                }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel vm)
        {
            var organizerProfile = await GetCurrentOrganizerProfileAsync();
            if (organizerProfile == null)
            {
                return NotFound("Organizer profile not found.");
            }

            if (vm.StartTime >= vm.EndTime)
            {
                ModelState.AddModelError(nameof(vm.EndTime), "End time must be after start time.");
            }

            if (vm.Tickets == null || !vm.Tickets.Any())
            {
                ModelState.AddModelError(string.Empty, "At least one ticket type is required.");
            }

            if (vm.Tickets != null)
            {
                for (int i = 0; i < vm.Tickets.Count; i++)
                {
                    var ticket = vm.Tickets[i];

                    if (string.IsNullOrWhiteSpace(ticket.Name))
                    {
                        ModelState.AddModelError($"Tickets[{i}].Name", "Ticket name is required.");
                    }

                    if (ticket.Price <= 0)
                    {
                        ModelState.AddModelError($"Tickets[{i}].Price", "Ticket price must be greater than zero.");
                    }

                    if (ticket.QuantityAvailable <= 0)
                    {
                        ModelState.AddModelError($"Tickets[{i}].QuantityAvailable", "Quantity must be greater than zero.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Categories = await GetCategoriesAsync();
                if (vm.Tickets == null || !vm.Tickets.Any())
                {
                    vm.Tickets = new List<TicketInputViewModel> { new TicketInputViewModel() };
                }
                return View(vm);
            }

            string? bannerPath = null;

            if (vm.BannerImage != null && vm.BannerImage.Length > 0)
            {
                bannerPath = await SaveBannerAsync(vm.BannerImage);
            }

            var ev = new Event
            {
                Title = vm.Title,
                Description = vm.Description,
                CategoryId = vm.CategoryId,
                Venue = vm.Venue,
                City = vm.City,
                EventDate = vm.EventDate,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                BannerImageUrl = bannerPath,
                OrganizerProfileId = organizerProfile.Id,
                Status = EventStatus.Draft,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            var ticketTypes = (vm.Tickets ?? new List<TicketInputViewModel>())
                .Select(t => new TicketType
                {
                    EventId = ev.Id,
                    Name = t.Name,
                    Price = t.Price,
                    QuantityAvailable = t.QuantityAvailable,
                    QuantitySold = 0,
                    IsActive = true
                })
                .ToList();

            _context.TicketTypes.AddRange(ticketTypes);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event created successfully as draft.";
            return RedirectToAction(nameof(MyEvents));
        }

        public async Task<IActionResult> MyEvents()
        {
            var organizerProfile = await GetCurrentOrganizerProfileAsync();
            if (organizerProfile == null)
            {
                return NotFound("Organizer profile not found.");
            }

            var events = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.TicketTypes)
                .Where(e => e.OrganizerProfileId == organizerProfile.Id)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(events);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.OrganizerProfile)
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
            {
                return NotFound();
            }

            return View(ev);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var organizerProfile = await GetCurrentOrganizerProfileAsync();
            if (organizerProfile == null)
            {
                return NotFound("Organizer profile not found.");
            }

            var ev = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerProfileId == organizerProfile.Id);

            if (ev == null)
            {
                return NotFound();
            }

            var vm = new CreateEventViewModel
            {
                Title = ev.Title,
                Description = ev.Description,
                CategoryId = ev.CategoryId,
                Venue = ev.Venue,
                City = ev.City,
                EventDate = ev.EventDate,
                StartTime = ev.StartTime,
                EndTime = ev.EndTime,
                ExistingBannerImageUrl = ev.BannerImageUrl,
                Categories = await GetCategoriesAsync(),
                Tickets = ev.TicketTypes.Select(t => new TicketInputViewModel
                {
                    Name = t.Name,
                    Price = t.Price,
                    QuantityAvailable = t.QuantityAvailable
                }).ToList()
            };

            if (!vm.Tickets.Any())
            {
                vm.Tickets.Add(new TicketInputViewModel());
            }

            ViewBag.EventId = ev.Id;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateEventViewModel vm)
        {
            var organizerProfile = await GetCurrentOrganizerProfileAsync();
            if (organizerProfile == null)
            {
                return NotFound("Organizer profile not found.");
            }

            var ev = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerProfileId == organizerProfile.Id);

            if (ev == null)
            {
                return NotFound();
            }

            if (vm.StartTime >= vm.EndTime)
            {
                ModelState.AddModelError(nameof(vm.EndTime), "End time must be after start time.");
            }

            if (vm.Tickets == null || !vm.Tickets.Any())
            {
                ModelState.AddModelError(string.Empty, "At least one ticket type is required.");
            }

            if (vm.Tickets != null)
            {
                for (int i = 0; i < vm.Tickets.Count; i++)
                {
                    var ticket = vm.Tickets[i];

                    if (string.IsNullOrWhiteSpace(ticket.Name))
                    {
                        ModelState.AddModelError($"Tickets[{i}].Name", "Ticket name is required.");
                    }

                    if (ticket.Price <= 0)
                    {
                        ModelState.AddModelError($"Tickets[{i}].Price", "Ticket price must be greater than zero.");
                    }

                    if (ticket.QuantityAvailable <= 0)
                    {
                        ModelState.AddModelError($"Tickets[{i}].QuantityAvailable", "Quantity must be greater than zero.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Categories = await GetCategoriesAsync();
                vm.ExistingBannerImageUrl = ev.BannerImageUrl;

                if (vm.Tickets == null || !vm.Tickets.Any())
                {
                    vm.Tickets = new List<TicketInputViewModel> { new TicketInputViewModel() };
                }

                ViewBag.EventId = id;
                return View(vm);
            }

            if (vm.BannerImage != null && vm.BannerImage.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(ev.BannerImageUrl))
                {
                    DeleteBannerFile(ev.BannerImageUrl);
                }

                ev.BannerImageUrl = await SaveBannerAsync(vm.BannerImage);
            }

            ev.Title = vm.Title;
            ev.Description = vm.Description;
            ev.CategoryId = vm.CategoryId;
            ev.Venue = vm.Venue;
            ev.City = vm.City;
            ev.EventDate = vm.EventDate;
            ev.StartTime = vm.StartTime;
            ev.EndTime = vm.EndTime;
            ev.UpdatedAt = DateTime.UtcNow;

            _context.TicketTypes.RemoveRange(ev.TicketTypes);

            var newTickets = (vm.Tickets ?? new List<TicketInputViewModel>())
                .Select(t => new TicketType
                {
                    EventId = ev.Id,
                    Name = t.Name,
                    Price = t.Price,
                    QuantityAvailable = t.QuantityAvailable,
                    QuantitySold = 0,
                    IsActive = true
                })
                .ToList();

            _context.TicketTypes.AddRange(newTickets);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event updated successfully.";
            return RedirectToAction(nameof(MyEvents));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var organizerProfile = await GetCurrentOrganizerProfileAsync();
            if (organizerProfile == null)
            {
                return NotFound("Organizer profile not found.");
            }

            var ev = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerProfileId == organizerProfile.Id);

            if (ev == null)
            {
                return NotFound();
            }

            return View(ev);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organizerProfile = await GetCurrentOrganizerProfileAsync();
            if (organizerProfile == null)
            {
                return NotFound("Organizer profile not found.");
            }

            var ev = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerProfileId == organizerProfile.Id);

            if (ev == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(ev.BannerImageUrl))
            {
                DeleteBannerFile(ev.BannerImageUrl);
            }

            _context.TicketTypes.RemoveRange(ev.TicketTypes);
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event deleted successfully.";
            return RedirectToAction(nameof(MyEvents));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var organizerProfile = await GetCurrentOrganizerProfileAsync();
            if (organizerProfile == null)
            {
                return NotFound("Organizer profile not found.");
            }

            var ev = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerProfileId == organizerProfile.Id);

            if (ev == null)
            {
                return NotFound();
            }

            ev.Status = EventStatus.Published;
            ev.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event published successfully.";
            return RedirectToAction(nameof(MyEvents));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(int id)
        {
            var organizerProfile = await GetCurrentOrganizerProfileAsync();
            if (organizerProfile == null)
            {
                return NotFound("Organizer profile not found.");
            }

            var ev = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerProfileId == organizerProfile.Id);

            if (ev == null)
            {
                return NotFound();
            }

            ev.Status = EventStatus.Draft;
            ev.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event moved back to draft.";
            return RedirectToAction(nameof(MyEvents));
        }

        private async Task<OrganizerProfile?> GetCurrentOrganizerProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            return await _context.OrganizerProfiles
                .FirstOrDefaultAsync(o => o.UserId == user.Id);
        }

        private async Task<string> SaveBannerAsync(IFormFile bannerFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "events");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(bannerFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await bannerFile.CopyToAsync(fileStream);

            return $"/uploads/events/{uniqueFileName}";
        }

        private void DeleteBannerFile(string bannerPath)
        {
            var fullPath = Path.Combine(
                _environment.WebRootPath,
                bannerPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        private async Task<List<SelectListItem>> GetCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }
    }
}