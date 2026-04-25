using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThirteenthAvenue.Data;
using ThirteenthAvenue.Models;
using ThirteenthAvenue.Models.Enums;
using ThirteenthAvenue.Services.Pdf;
using ThirteenthAvenue.ViewModels;

namespace ThirteenthAvenue.Controllers
{
    [AllowAnonymous]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly TicketPdfService _ticketPdfService;

        public BookingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            TicketPdfService ticketPdfService)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _ticketPdfService = ticketPdfService;
        }

        public async Task<IActionResult> Create(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.Status == EventStatus.Published);

            if (ev == null)
            {
                return NotFound();
            }

            var vm = new CreateBookingViewModel
            {
                EventId = ev.Id,
                EventTitle = ev.Title,
                City = ev.City,
                Venue = ev.Venue,
                EventDate = ev.EventDate,
                StartTime = ev.StartTime,
                EndTime = ev.EndTime,
                BannerImageUrl = ev.BannerImageUrl,
                IsAuthenticatedUser = User.Identity?.IsAuthenticated ?? false,
                Tickets = ev.TicketTypes
                    .Where(t => t.IsActive)
                    .Select(t => new BookingTicketInputViewModel
                    {
                        TicketTypeId = t.Id,
                        TicketName = t.Name,
                        Price = t.Price,
                        AvailableQuantity = t.QuantityAvailable - t.QuantitySold,
                        Quantity = 0
                    })
                    .ToList()
            };

            if (vm.IsAuthenticatedUser)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    vm.GuestName = user.FullName;
                    vm.GuestEmail = user.Email;
                    vm.GuestPhone = user.PhoneNumber;
                }
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel vm)
        {
            var ev = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == vm.EventId && e.Status == EventStatus.Published);

            if (ev == null)
            {
                return NotFound();
            }

            vm.EventTitle = ev.Title;
            vm.City = ev.City;
            vm.Venue = ev.Venue;
            vm.EventDate = ev.EventDate;
            vm.StartTime = ev.StartTime;
            vm.EndTime = ev.EndTime;
            vm.BannerImageUrl = ev.BannerImageUrl;
            vm.IsAuthenticatedUser = User.Identity?.IsAuthenticated ?? false;

            var appUser = vm.IsAuthenticatedUser ? await _userManager.GetUserAsync(User) : null;

            if (!vm.IsAuthenticatedUser)
            {
                if (string.IsNullOrWhiteSpace(vm.GuestName))
                {
                    ModelState.AddModelError(nameof(vm.GuestName), "Full name is required.");
                }

                if (string.IsNullOrWhiteSpace(vm.GuestEmail))
                {
                    ModelState.AddModelError(nameof(vm.GuestEmail), "Email is required.");
                }

                if (string.IsNullOrWhiteSpace(vm.GuestPhone))
                {
                    ModelState.AddModelError(nameof(vm.GuestPhone), "Phone number is required.");
                }
            }
            else
            {
                vm.GuestName = appUser?.FullName;
                vm.GuestEmail = appUser?.Email;
                vm.GuestPhone = appUser?.PhoneNumber;
            }

            if (vm.Tickets == null || !vm.Tickets.Any())
            {
                ModelState.AddModelError(string.Empty, "No ticket types were submitted.");
            }

            var selectedTickets = new List<(TicketType TicketType, int Quantity)>();

            if (vm.Tickets != null)
            {
                foreach (var input in vm.Tickets)
                {
                    var ticketType = ev.TicketTypes.FirstOrDefault(t => t.Id == input.TicketTypeId && t.IsActive);

                    if (ticketType == null)
                    {
                        continue;
                    }

                    input.TicketName = ticketType.Name;
                    input.Price = ticketType.Price;
                    input.AvailableQuantity = ticketType.QuantityAvailable - ticketType.QuantitySold;

                    if (input.Quantity > 0)
                    {
                        if (input.Quantity > input.AvailableQuantity)
                        {
                            ModelState.AddModelError(
                                string.Empty,
                                $"Only {input.AvailableQuantity} '{ticketType.Name}' tickets are available.");
                        }
                        else
                        {
                            selectedTickets.Add((ticketType, input.Quantity));
                        }
                    }
                }
            }

            if (!selectedTickets.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one ticket.");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var totalAmount = selectedTickets.Sum(x => x.TicketType.Price * x.Quantity);
            var bookingReference = GenerateBookingReference();
            var paymentReference = GeneratePaymentReference();

            string recipientEmail = vm.IsAuthenticatedUser
                ? appUser?.Email ?? string.Empty
                : vm.GuestEmail ?? string.Empty;

            string customerName = vm.IsAuthenticatedUser
                ? appUser?.FullName ?? string.Empty
                : vm.GuestName ?? string.Empty;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var booking = new Booking
                {
                    BookingReference = bookingReference,
                    EventId = ev.Id,
                    UserId = appUser?.Id,
                    GuestName = vm.IsAuthenticatedUser ? null : vm.GuestName,
                    GuestEmail = vm.IsAuthenticatedUser ? null : vm.GuestEmail,
                    GuestPhone = vm.IsAuthenticatedUser ? null : vm.GuestPhone,
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.Paid,
                    PaymentStatus = PaymentStatus.Successful,
                    TotalAmount = totalAmount,
                    PaymentReference = paymentReference
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                foreach (var selected in selectedTickets)
                {
                    var bookingItem = new BookingItem
                    {
                        BookingId = booking.Id,
                        TicketTypeId = selected.TicketType.Id,
                        Quantity = selected.Quantity,
                        UnitPrice = selected.TicketType.Price,
                        LineTotal = selected.TicketType.Price * selected.Quantity
                    };

                    _context.BookingItems.Add(bookingItem);
                    selected.TicketType.QuantitySold += selected.Quantity;
                }

                var payment = new Payment
                {
                    BookingId = booking.Id,
                    Provider = "Manual",
                    ProviderTransactionId = paymentReference,
                    Amount = totalAmount,
                    Currency = "RWF",
                    Status = PaymentStatus.Successful,
                    PaidAt = DateTime.UtcNow,
                    RawResponseJson = "{\"mode\":\"manual\",\"message\":\"Temporary manual transaction before payment integration\"}"
                };

                _context.Payments.Add(payment);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Something went wrong while creating the booking.");
                return View(vm);
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(recipientEmail))
                {
                    await _emailSender.SendEmailAsync(
                        recipientEmail,
                        $"Your 13th Avenue Ticket - {ev.Title}",
                        BuildTicketEmailHtml(
                            bookingReference,
                            paymentReference,
                            ev,
                            customerName,
                            selectedTickets,
                            totalAmount));
                }
            }
            catch
            {
                TempData["SuccessMessage"] = "Booking was successful, but the ticket email could not be sent.";
            }

            return RedirectToAction(nameof(Success), new
            {
                bookingReference,
                paymentReference,
                eventTitle = ev.Title,
                recipientEmail,
                totalAmount
            });
        }

        public IActionResult Success(string bookingReference, string paymentReference, string eventTitle, string recipientEmail, decimal totalAmount)
        {
            var vm = new BookingSuccessViewModel
            {
                BookingReference = bookingReference,
                PaymentReference = paymentReference,
                EventTitle = eventTitle,
                RecipientEmail = recipientEmail,
                TotalAmount = totalAmount
            };

            return View(vm);
        }

        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.TicketType)
                .Include(b => b.Payments)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        public async Task<IActionResult> DownloadTicket(string bookingReference)
        {
            var booking = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.Category)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.TicketType)
                .FirstOrDefaultAsync(b => b.BookingReference == bookingReference);

            if (booking == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            var isOwner = booking.UserId != null && booking.UserId == currentUserId;
            var isGuestBooking = booking.UserId == null && !string.IsNullOrWhiteSpace(booking.GuestEmail);

            if (!isOwner && !isGuestBooking)
            {
                return Forbid();
            }

            string customerName;
            string customerEmail;

            if (booking.UserId != null)
            {
                var user = await _userManager.FindByIdAsync(booking.UserId);
                customerName = user?.FullName ?? "Customer";
                customerEmail = user?.Email ?? "";
            }
            else
            {
                customerName = booking.GuestName ?? "Guest";
                customerEmail = booking.GuestEmail ?? "";
            }

            var pdfBytes = _ticketPdfService.GenerateBookingTicketPdf(
                booking,
                booking.Event!,
                booking.BookingItems.ToList(),
                customerName,
                customerEmail);

            return File(pdfBytes, "application/pdf", $"{booking.BookingReference}-ticket.pdf");
        }

        private static string GenerateBookingReference()
        {
            return $"BK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        private static string GeneratePaymentReference()
        {
            return $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        private static string BuildTicketEmailHtml(
            string bookingReference,
            string paymentReference,
            Event ev,
            string customerName,
            List<(TicketType TicketType, int Quantity)> selectedTickets,
            decimal totalAmount)
        {
            var sb = new StringBuilder();

            sb.Append($@"
                <h2>Your Ticket - {ev.Title}</h2>
                <p>Hello {customerName},</p>
                <p>Your booking has been confirmed successfully.</p>
                <p><strong>Booking Reference:</strong> {bookingReference}</p>
                <p><strong>Transaction Reference:</strong> {paymentReference}</p>
                <p><strong>Event:</strong> {ev.Title}</p>
                <p><strong>Date:</strong> {ev.EventDate:dd MMM yyyy}</p>
                <p><strong>Time:</strong> {ev.StartTime:hh\:mm} - {ev.EndTime:hh\:mm}</p>
                <p><strong>Venue:</strong> {ev.Venue}, {ev.City}</p>
                <hr />
                <h4>Tickets</h4>
                <table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse;'>
                    <thead>
                        <tr>
                            <th>Ticket</th>
                            <th>Price</th>
                            <th>Quantity</th>
                            <th>Line Total</th>
                        </tr>
                    </thead>
                    <tbody>");

            foreach (var item in selectedTickets)
            {
                sb.Append($@"
                    <tr>
                        <td>{item.TicketType.Name}</td>
                        <td>{item.TicketType.Price:N2} RWF</td>
                        <td>{item.Quantity}</td>
                        <td>{(item.TicketType.Price * item.Quantity):N2} RWF</td>
                    </tr>");
            }

            sb.Append($@"
                    </tbody>
                </table>
                <p><strong>Total Amount:</strong> {totalAmount:N2} RWF</p>
                <p>This email serves as your temporary ticket confirmation.</p>
                <p>Thank you for booking with 13th Avenue.</p>");

            return sb.ToString();
        }
    }
}