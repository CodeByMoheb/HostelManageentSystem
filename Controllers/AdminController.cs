using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HostelManageentSystem.Data;
using HostelManageentSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace HostelManageentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context, 
            ILogger<AdminController> logger, 
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Dashboard()
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
                    .ToListAsync(),
                PendingBookings = await _context.Bookings
                    .CountAsync(b => b.Status == "Pending"),
                ActiveHostels = await _context.Hostels
                    .CountAsync(h => h.IsActive)
            };

            return View(dashboardData);
        }

        // User Management
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var userRoles = new Dictionary<string, IList<string>>();
                
                foreach (var user in users)
                {
                    userRoles[user.Id] = await _userManager.GetRolesAsync(user);
                }
                
                ViewBag.UserRoles = userRoles;
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["ErrorMessage"] = "An error occurred while loading users. Please try again.";
                return View(new List<ApplicationUser>());
            }
        }

        public async Task<IActionResult> EditUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                ViewBag.UserRoles = userRoles;
                ViewBag.AllRoles = await _roleManager.Roles.ToListAsync();

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user for editing");
                TempData["ErrorMessage"] = "An error occurred while loading the user. Please try again.";
                return RedirectToAction(nameof(Users));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, ApplicationUser user, string[] selectedRoles)
        {
            try
            {
                if (id != user.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var existingUser = await _userManager.FindByIdAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.UpdatedAt = DateTime.Now;

                    var result = await _userManager.UpdateAsync(existingUser);
                    if (result.Succeeded)
                    {
                        // Update roles
                        var currentRoles = await _userManager.GetRolesAsync(existingUser);
                        var rolesToAdd = selectedRoles.Except(currentRoles);
                        var rolesToRemove = currentRoles.Except(selectedRoles);

                        await _userManager.AddToRolesAsync(existingUser, rolesToAdd);
                        await _userManager.RemoveFromRolesAsync(existingUser, rolesToRemove);

                        TempData["SuccessMessage"] = "User updated successfully!";
                        return RedirectToAction(nameof(Users));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                TempData["ErrorMessage"] = "An error occurred while updating the user. Please try again.";
                return View(user);
            }
        }

        // Hostel Management
        public async Task<IActionResult> Hostels()
        {
            try
            {
                var hostels = await _context.Hostels
                    .Include(h => h.Manager)
                    .Include(h => h.Rooms)
                    .ToListAsync();
                return View(hostels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading hostels");
                TempData["ErrorMessage"] = "An error occurred while loading hostels. Please try again.";
                return View(new List<Hostel>());
            }
        }

        public IActionResult CreateHostel()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHostel(Hostel hostel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    hostel.CreatedAt = DateTime.Now;
                    hostel.IsActive = true;
                    hostel.AvailableRooms = hostel.TotalRooms;

                    _context.Add(hostel);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Hostel created successfully!";
                    return RedirectToAction(nameof(Hostels));
                }
                return View(hostel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hostel");
                TempData["ErrorMessage"] = "An error occurred while creating the hostel. Please try again.";
                return View(hostel);
            }
        }

        public async Task<IActionResult> EditHostel(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var hostel = await _context.Hostels
                    .Include(h => h.Manager)
                    .Include(h => h.Rooms)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (hostel == null)
                {
                    return NotFound();
                }
                return View(hostel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading hostel for editing");
                TempData["ErrorMessage"] = "An error occurred while loading the hostel. Please try again.";
                return RedirectToAction(nameof(Hostels));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHostel(int id, Hostel hostel)
        {
            try
            {
                if (id != hostel.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var existingHostel = await _context.Hostels.FindAsync(id);
                    if (existingHostel == null)
                    {
                        return NotFound();
                    }

                    // Update available rooms if total rooms changed
                    if (existingHostel.TotalRooms != hostel.TotalRooms)
                    {
                        var difference = hostel.TotalRooms - existingHostel.TotalRooms;
                        hostel.AvailableRooms = existingHostel.AvailableRooms + difference;
                    }

                    _context.Entry(existingHostel).CurrentValues.SetValues(hostel);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Hostel updated successfully!";
                    return RedirectToAction(nameof(Hostels));
                }
                return View(hostel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hostel");
                TempData["ErrorMessage"] = "An error occurred while updating the hostel. Please try again.";
                return View(hostel);
            }
        }

        // Booking Management
        public async Task<IActionResult> Bookings()
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Hostel)
                    .Include(b => b.Room)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
                return View(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bookings");
                TempData["ErrorMessage"] = "An error occurred while loading bookings. Please try again.";
                return View(new List<Booking>());
            }
        }

        public async Task<IActionResult> EditBooking(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Hostel)
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                {
                    return NotFound();
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking for editing");
                TempData["ErrorMessage"] = "An error occurred while loading the booking. Please try again.";
                return RedirectToAction(nameof(Bookings));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBooking(int id, Booking booking)
        {
            try
            {
                if (id != booking.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var existingBooking = await _context.Bookings.FindAsync(id);
                    if (existingBooking == null)
                    {
                        return NotFound();
                    }

                    // Update booking status and dates
                    existingBooking.Status = booking.Status;
                    existingBooking.CheckInDate = booking.CheckInDate;
                    existingBooking.CheckOutDate = booking.CheckOutDate;
                    existingBooking.TotalAmount = booking.TotalAmount;
                    existingBooking.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Booking updated successfully!";
                    return RedirectToAction(nameof(Bookings));
                }
                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking");
                TempData["ErrorMessage"] = "An error occurred while updating the booking. Please try again.";
                return View(booking);
            }
        }

        private bool HostelExists(int id)
        {
            return _context.Hostels.Any(e => e.Id == id);
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
} 