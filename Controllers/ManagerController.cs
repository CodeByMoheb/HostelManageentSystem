using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HostelManageentSystem.Data;
using HostelManageentSystem.Models;
using HostelManageentSystem.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HostelManageentSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ManagerController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            var managerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(managerId))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ManagerDashboardViewModel();
            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.ManagerId == managerId);

            model.HasHostel = hostel != null;
            model.CurrentHostel = hostel;

            if (hostel != null)
            {
                // Room Statistics
                model.TotalRooms = hostel.TotalRooms;
                model.AvailableRooms = hostel.AvailableRooms;
                model.AvailableRoomsList = await _context.Rooms
                    .Where(r => r.HostelId == hostel.Id && r.IsAvailable)
                    .Take(5)
                    .ToListAsync();

                // Student Statistics
                model.TotalStudents = await _context.Students
                    .CountAsync(s => s.HostelId == hostel.Id && s.IsActive);
                model.ActiveMealPlans = await _context.Students
                    .CountAsync(s => s.HostelId == hostel.Id && s.HasMealPlan && s.IsActive);

                // Booking Statistics
                model.TotalBookings = await _context.Bookings
                    .CountAsync(b => b.HostelId == hostel.Id);
                model.PendingBookings = await _context.Bookings
                    .CountAsync(b => b.HostelId == hostel.Id && b.Status == "Pending");

                // Financial Statistics
                model.TotalRevenue = await _context.Payments
                    .Where(p => p.HostelId == hostel.Id && p.Status == "Completed")
                    .SumAsync(p => p.Amount);
                model.PendingPayments = await _context.Payments
                    .Where(p => p.HostelId == hostel.Id && p.Status == "Pending")
                    .SumAsync(p => p.Amount);

                // Recent Activities
                model.RecentBookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                    .Where(b => b.HostelId == hostel.Id)
                    .OrderByDescending(b => b.CheckInDate)
                    .Take(5)
                    .ToListAsync();

                model.RecentStudents = await _context.Students
                    .Include(s => s.Room)
                    .Where(s => s.HostelId == hostel.Id && s.IsActive)
                    .OrderByDescending(s => s.CheckInDate)
                    .Take(5)
                    .ToListAsync();

                // Upcoming Meals
                model.UpcomingMeals = await _context.Meals
                    .Where(m => m.HostelId == hostel.Id && m.MealDate >= DateTime.Today)
                    .OrderBy(m => m.MealDate)
                    .Take(5)
                    .ToListAsync();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterHostel()
        {
            var managerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(managerId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if manager already has a hostel
            var hasHostel = _context.Hostels.Any(h => h.ManagerId == managerId);
            if (hasHostel)
            {
                TempData["Message"] = "You already have a registered hostel.";
                return RedirectToAction("Dashboard");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterHostel(Hostel hostel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var managerId = _userManager.GetUserId(User);
                    if (string.IsNullOrEmpty(managerId))
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    // Check if manager already has a hostel
                    var hasHostel = await _context.Hostels.AnyAsync(h => h.ManagerId == managerId);
                    if (hasHostel)
                    {
                        TempData["Message"] = "You already have a registered hostel.";
                        return RedirectToAction("Dashboard");
                    }

                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        hostel.ManagerId = user.Id;
                        hostel.ManagerName = user.UserName ?? string.Empty;
                        hostel.ManagerPhone = user.PhoneNumber ?? string.Empty;
                        hostel.CreatedAt = DateTime.Now;
                        hostel.IsActive = true;
                        hostel.AvailableRooms = hostel.TotalRooms; // Set available rooms equal to total rooms initially

                        _context.Add(hostel);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Hostel registered successfully!";
                        return RedirectToAction("Dashboard");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while registering the hostel. Please try again.");
                    _logger.LogError(ex, "Error registering hostel for user {UserId}", _userManager.GetUserId(User));
                }
            }

            return View(hostel);
        }

        // Hostel Management
        public async Task<IActionResult> Hostels()
        {
            var managerId = _userManager.GetUserId(User);
            var hostels = await _context.Hostels
                .Where(h => h.ManagerId == managerId)
                .ToListAsync();
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
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    hostel.ManagerId = user.Id;
                    hostel.ManagerName = user.UserName ?? string.Empty;
                    hostel.ManagerPhone = user.PhoneNumber ?? string.Empty;

                    _context.Add(hostel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Hostels));
                }
            }
            return View(hostel);
        }

        public async Task<IActionResult> EditHostel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managerId = _userManager.GetUserId(User);
            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.Id == id && h.ManagerId == managerId);

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

            var managerId = _userManager.GetUserId(User);
            if (!await _context.Hostels.AnyAsync(h => h.Id == id && h.ManagerId == managerId))
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

        // Room Management
        public async Task<IActionResult> Rooms(int hostelId)
        {
            var managerId = _userManager.GetUserId(User);
            var rooms = await _context.Rooms
                .Include(r => r.Hostel)
                .Where(r => r.HostelId == hostelId && r.Hostel.ManagerId == managerId)
                .ToListAsync();
            return View(rooms);
        }

        public IActionResult CreateRoom(int hostelId)
        {
            ViewBag.HostelId = hostelId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(Room room)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.ManagerId == user.Id);
            if (hostel == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                room.HostelId = hostel.Id;
                room.CreatedAt = DateTime.Now;
                room.IsAvailable = true;
                room.IsActive = true;

                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Rooms));
            }

            return View(room);
        }

        public async Task<IActionResult> EditRoom(int id, Room room)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.ManagerId == user.Id);
            if (hostel == null)
            {
                return NotFound();
            }

            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    room.HostelId = hostel.Id;
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Rooms));
            }

            return View(room);
        }

        // Meal Management
        public async Task<IActionResult> Meals(int hostelId)
        {
            var managerId = _userManager.GetUserId(User);
            var meals = await _context.Meals
                .Include(m => m.Hostel)
                .Where(m => m.HostelId == hostelId && m.Hostel.ManagerId == managerId)
                .OrderByDescending(m => m.MealDate)
                .ToListAsync();
            return View(meals);
        }

        public IActionResult CreateMeal(int hostelId)
        {
            ViewBag.HostelId = hostelId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMeal(Meal meal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(meal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Meals), new { hostelId = meal.HostelId });
            }
            ViewBag.HostelId = meal.HostelId;
            return View(meal);
        }

        public async Task<IActionResult> EditMeal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managerId = _userManager.GetUserId(User);
            var meal = await _context.Meals
                .Include(m => m.Hostel)
                .FirstOrDefaultAsync(m => m.Id == id && m.Hostel.ManagerId == managerId);

            if (meal == null)
            {
                return NotFound();
            }
            return View(meal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMeal(int id, Meal meal)
        {
            if (id != meal.Id)
            {
                return NotFound();
            }

            var managerId = _userManager.GetUserId(User);
            if (!await _context.Meals
                .Include(m => m.Hostel)
                .AnyAsync(m => m.Id == id && m.Hostel.ManagerId == managerId))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MealExists(meal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Meals), new { hostelId = meal.HostelId });
            }
            return View(meal);
        }

        // Student Management
        public async Task<IActionResult> Students()
        {
            var managerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(managerId))
            {
                return RedirectToAction("Login", "Account");
            }

            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.ManagerId == managerId);

            if (hostel == null)
            {
                return RedirectToAction("RegisterHostel");
            }

            var students = await _context.Students
                .Include(s => s.Room)
                .Where(s => s.HostelId == hostel.Id)
                .ToListAsync();

            return View(students);
        }

        public async Task<IActionResult> AddStudent()
        {
            var managerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(managerId))
            {
                return RedirectToAction("Login", "Account");
            }

            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.ManagerId == managerId);

            if (hostel == null)
            {
                return RedirectToAction("RegisterHostel");
            }

            ViewBag.AvailableRooms = await _context.Rooms
                .Where(r => r.HostelId == hostel.Id && r.IsAvailable)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(Student student)
        {
            if (ModelState.IsValid)
            {
                var managerId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(managerId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var hostel = await _context.Hostels
                    .FirstOrDefaultAsync(h => h.ManagerId == managerId);

                if (hostel == null)
                {
                    return RedirectToAction("RegisterHostel");
                }

                student.HostelId = hostel.Id;
                student.CheckInDate = DateTime.Now;
                student.IsActive = true;

                // Update room availability
                var room = await _context.Rooms.FindAsync(student.RoomId);
                if (room != null)
                {
                    room.IsAvailable = false;
                    _context.Update(room);
                }

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction("Students");
            }

            ViewBag.AvailableRooms = await _context.Rooms
                .Where(r => r.HostelId == student.HostelId && r.IsAvailable)
                .ToListAsync();

            return View(student);
        }

        private bool HostelExists(int id)
        {
            return _context.Hostels.Any(e => e.Id == id);
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        private bool MealExists(int id)
        {
            return _context.Meals.Any(e => e.Id == id);
        }
    }
} 