using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HostelManageentSystem.Data;
using HostelManageentSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HostelManageentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var dashboardData = new AdminDashboardViewModel
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalHostels = await _context.Hostels.CountAsync(),
                    TotalBookings = await _context.Bookings.CountAsync(),
                    RecentBookings = await _context.Bookings
                        .Include(b => b.User)
                        .Include(b => b.Hostel)
                        .Include(b => b.Room)
                        .OrderByDescending(b => b.CheckInDate)
                        .Take(5)
                        .ToListAsync(),
                    RecentUsers = await _context.Users
                        .OrderByDescending(u => u.CreatedAt)
                        .Take(5)
                        .ToListAsync()
                };

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error loading admin dashboard");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        // Hostel Management
        public async Task<IActionResult> Hostels()
        {
            var hostels = await _context.Hostels.ToListAsync();
            return View(hostels);
        }

        public IActionResult CreateHostel()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHostel(Hostel hostel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hostel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Hostels));
            }
            return View(hostel);
        }

        public async Task<IActionResult> EditHostel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hostel = await _context.Hostels.FindAsync(id);
            if (hostel == null)
            {
                return NotFound();
            }
            return View(hostel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHostel(int id, Hostel hostel)
        {
            if (id != hostel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hostel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HostelExists(hostel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Hostels));
            }
            return View(hostel);
        }

        // User Management
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // Booking Management
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Hostel)
                .ToListAsync();
            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = status;
            booking.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Bookings));
        }

        private bool HostelExists(int id)
        {
            return _context.Hostels.Any(e => e.Id == id);
        }
    }
} 